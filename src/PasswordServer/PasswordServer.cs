using System;
using System.Collections.Generic;
using System.Linq;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronIO;
using Crestron.SimplSharpPro.DeviceSupport;
using Newtonsoft.Json;
using PepperDash.Core;
using PepperDash.Core.Logging;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Bridges;
using PepperDash.Essentials.Core.Config;
using PepperDash.Essentials.Plugin.PasswordManager;

namespace PepperDash.Essentials.Plugin.Password.Server
{
    /// <summary>
    /// Password Server Device
    /// </summary>
    /// <remarks>
    /// Manages user credentials stored in a JSON file with bridge access for CRUD operations.
    /// Server can only be bridged once to a single EISC.
    /// </remarks>
    public class PasswordServer : EssentialsBridgeableDevice
    {
        #region Fields

        private readonly PasswordServerConfig _config;
        private bool _isBridged;
        private readonly CCriticalSection _fileLock = new CCriticalSection();
        private readonly CCriticalSection _usersLock = new CCriticalSection();
        private readonly CTimer _saveTimer;
        private List<UserCredential> _users;

        // Selected user tracking
        private int _selectedUserIndex = -1;

        // Input buffers from SIMPL
        private string _usernameInput = string.Empty;
        private string _passwordInput = string.Empty;
        private string _accessInput = string.Empty;

        // Last validated user
        private string _lastValidatedUsername = string.Empty;
        private int _lastValidatedAccess;

        // Operation result flags
        private bool _createUserSuccess;
        private bool _deleteUserSuccess;
        private bool _validateUserSuccess;

        // Status message
        private string _statusMessage = string.Empty;

        #endregion

        #region Events

        /// <summary>
        /// Fired when the user list changes (add, delete, update)
        /// </summary>
        public event EventHandler<EventArgs> UsersChanged;

        #endregion

        #region Feedbacks

        /// <summary>
        /// User count feedback
        /// </summary>
        public IntFeedback UserCountFeedback { get; private set; }

        /// <summary>
        /// Selected user index feedback
        /// </summary>
        public IntFeedback SelectedUserIndexFeedback { get; private set; }

        /// <summary>
        /// Selected user access level feedback
        /// </summary>
        public IntFeedback SelectedUserAccessFeedback { get; private set; }

        /// <summary>
        /// Validated user access level feedback
        /// </summary>
        public IntFeedback ValidatedUserAccessFeedback { get; private set; }

        /// <summary>
        /// Create user success feedback
        /// </summary>
        public BoolFeedback CreateUserSuccessFeedback { get; private set; }

        /// <summary>
        /// Delete user success feedback
        /// </summary>
        public BoolFeedback DeleteUserSuccessFeedback { get; private set; }

        /// <summary>
        /// Validate user success feedback
        /// </summary>
        public BoolFeedback ValidateUserSuccessFeedback { get; private set; }

        /// <summary>
        /// User list JSON feedback
        /// </summary>
        public StringFeedback UserListFeedback { get; private set; }

        /// <summary>
        /// Status message feedback
        /// </summary>
        public StringFeedback StatusMessageFeedback { get; private set; }

        /// <summary>
        /// Selected username feedback
        /// </summary>
        public StringFeedback SelectedUsernameFeedback { get; private set; }

        /// <summary>
        /// Selected password feedback (masked or actual based on config)
        /// </summary>
        public StringFeedback SelectedPasswordFeedback { get; private set; }

        /// <summary>
        /// Selected access level feedback (as string)
        /// </summary>
        public StringFeedback SelectedAccessFeedback { get; private set; }

        /// <summary>
        /// Validated username feedback
        /// </summary>
        public StringFeedback ValidatedUsernameFeedback { get; private set; }

        #endregion

        #region Properties

        /// <summary>
        /// Gets a read-only copy of the current users list
        /// </summary>
        public IReadOnlyList<UserCredential> Users
        {
            get
            {
                _usersLock.Enter();
                try
                {
                    return _users.ToList().AsReadOnly();
                }
                finally
                {
                    _usersLock.Leave();
                }
            }
        }

        /// <summary>
        /// Gets the count of users
        /// </summary>
        public int UserCount
        {
            get
            {
                _usersLock.Enter();
                try
                {
                    return _users?.Count ?? 0;
                }
                finally
                {
                    _usersLock.Leave();
                }
            }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor for Password Server
        /// </summary>
        public PasswordServer(string key, string name, PasswordServerConfig config)
            : base(key, name)
        {
            this.LogInformation("Constructing Password Server: {name}", name);

            _config = config;
            _users = new List<UserCredential>();

            // Initialize save timer
            var saveDelay = _config.SaveDelayMs > 0 ? _config.SaveDelayMs : 1000;
            _saveTimer = new CTimer(_ => SaveUsersToFile(), Timeout.Infinite);

            // Initialize feedbacks
            InitializeFeedbacks();

            // Register console commands
            CrestronConsole.AddNewConsoleCommand(ConsoleCommand,
                string.Format("pwmgr-{0}", key),
                "list | add [u p a] | del [u] | val [u p]",
                ConsoleAccessLevelEnum.AccessOperator);
        }

        private void InitializeFeedbacks()
        {
            UserCountFeedback = new IntFeedback("UserCount", () => UserCount);
            SelectedUserIndexFeedback = new IntFeedback("SelectedUserIndex", () => _selectedUserIndex);
            SelectedUserAccessFeedback = new IntFeedback("SelectedUserAccess", () => GetSelectedUser()?.Access ?? 0);
            ValidatedUserAccessFeedback = new IntFeedback("ValidatedUserAccess", () => _lastValidatedAccess);

            CreateUserSuccessFeedback = new BoolFeedback("CreateUserSuccess", () => _createUserSuccess);
            DeleteUserSuccessFeedback = new BoolFeedback("DeleteUserSuccess", () => _deleteUserSuccess);
            ValidateUserSuccessFeedback = new BoolFeedback("ValidateUserSuccess", () => _validateUserSuccess);

            UserListFeedback = new StringFeedback("UserList", () => GetUserListJson());
            StatusMessageFeedback = new StringFeedback("StatusMessage", () => _statusMessage);
            SelectedUsernameFeedback = new StringFeedback("SelectedUsername", () => GetSelectedUser()?.Username ?? string.Empty);
            SelectedPasswordFeedback = new StringFeedback("SelectedPassword", () => GetSelectedPasswordDisplay());
            SelectedAccessFeedback = new StringFeedback("SelectedAccess", () => GetSelectedUser()?.Access.ToString() ?? string.Empty);
            ValidatedUsernameFeedback = new StringFeedback("ValidatedUsername", () => _lastValidatedUsername);
        }

        #endregion

        #region EssentialsDevice Overrides

        /// <summary>
        /// Initializes the device
        /// </summary>
        public override bool CustomActivate()
        {
            this.LogDebug("Activating Password Server");

            LoadUsersFromFile();

            return base.CustomActivate();
        }

        #endregion

        #region File Operations

        private void LoadUsersFromFile()
        {
            if (string.IsNullOrEmpty(_config.FilePath))
            {
                this.LogInformation("No file path specified, using in-memory storage only");
                _usersLock.Enter();
                try
                {
                    _users = _config.DefaultUsers != null
                        ? new List<UserCredential>(_config.DefaultUsers)
                        : new List<UserCredential>();
                }
                finally
                {
                    _usersLock.Leave();
                }

                _selectedUserIndex = UserCount > 0 ? 0 : -1;
                SetStatusMessage("Using in-memory storage");
                OnUsersChanged();
                UpdateAllFeedbacks();
                return;
            }

            var fullPath = Path.Combine(Global.FilePathPrefix, _config.FilePath);

            _fileLock.Enter();
            try
            {
                if (File.Exists(fullPath))
                {
                    this.LogDebug("Loading users from file: {path}", fullPath);
                    var json = File.ReadToEnd(fullPath, System.Text.Encoding.UTF8);

                    _usersLock.Enter();
                    try
                    {
                        _users = JsonConvert.DeserializeObject<List<UserCredential>>(json)
                            ?? new List<UserCredential>();
                    }
                    finally
                    {
                        _usersLock.Leave();
                    }

                    this.LogInformation("Loaded {count} users from file", UserCount);
                    SetStatusMessage(string.Format("Loaded {0} users from file", UserCount));
                }
                else
                {
                    this.LogInformation("File does not exist, creating with default users: {path}", fullPath);

                    _usersLock.Enter();
                    try
                    {
                        _users = _config.DefaultUsers != null
                            ? new List<UserCredential>(_config.DefaultUsers)
                            : new List<UserCredential>();
                    }
                    finally
                    {
                        _usersLock.Leave();
                    }

                    // Ensure directory exists
                    var directory = Path.GetDirectoryName(fullPath);
                    if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                    {
                        Directory.Create(directory);
                    }

                    SaveUsersToFileImmediate();
                    SetStatusMessage("Created new user file with default users");
                }
            }
            catch (Exception ex)
            {
                this.LogError("Error loading users from file: {error}", ex.Message);
                _usersLock.Enter();
                try
                {
                    _users = new List<UserCredential>();
                }
                finally
                {
                    _usersLock.Leave();
                }
                SetStatusMessage(string.Format("Error loading users: {0}", ex.Message));
            }
            finally
            {
                _fileLock.Leave();
            }

            _selectedUserIndex = UserCount > 0 ? 0 : -1;
            OnUsersChanged();
            UpdateAllFeedbacks();
        }

        private void ScheduleSave()
        {
            if (string.IsNullOrEmpty(_config.FilePath))
            {
                return;
            }

            var saveDelay = _config.SaveDelayMs > 0 ? _config.SaveDelayMs : 1000;
            _saveTimer.Reset(saveDelay);
        }

        private void SaveUsersToFile()
        {
            if (string.IsNullOrEmpty(_config.FilePath))
            {
                return;
            }

            SaveUsersToFileImmediate();
        }

        private void SaveUsersToFileImmediate()
        {
            if (string.IsNullOrEmpty(_config.FilePath))
            {
                return;
            }

            var fullPath = Path.Combine(Global.FilePathPrefix, _config.FilePath);
            var tempPath = fullPath + ".tmp";

            _fileLock.Enter();
            try
            {
                // Ensure directory exists
                var directory = Path.GetDirectoryName(fullPath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.Create(directory);
                }

                string json;
                _usersLock.Enter();
                try
                {
                    json = JsonConvert.SerializeObject(_users, Formatting.Indented);
                }
                finally
                {
                    _usersLock.Leave();
                }

                // Write to temp file first
                using (var stream = new FileStream(tempPath, FileMode.Create, FileAccess.Write))
                using (var writer = new StreamWriter(stream))
                {
                    writer.Write(json);
                    writer.Flush();
                    stream.Flush();
                }

                // Replace original file
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                }
                File.Move(tempPath, fullPath);

                this.LogDebug("Users saved to file: {path}", fullPath);
            }
            catch (Exception ex)
            {
                this.LogError("Error saving users to file: {error}", ex.Message);
                SetStatusMessage(string.Format("Error saving: {0}", ex.Message));
            }
            finally
            {
                // Clean up temp file if it still exists
                try
                {
                    if (File.Exists(tempPath))
                    {
                        File.Delete(tempPath);
                    }
                }
                catch { }

                _fileLock.Leave();
            }
        }

        #endregion

        #region User Management Operations

        /// <summary>
        /// Creates a new user with the given credentials
        /// </summary>
        /// <param name="username">Username</param>
        /// <param name="password">Password</param>
        /// <param name="access">Access level</param>
        /// <param name="errorMessage">Error message if creation fails</param>
        /// <returns>True if user was created successfully</returns>
        public bool CreateUser(string username, string password, int access, out string errorMessage)
        {
            errorMessage = string.Empty;

            if (string.IsNullOrEmpty(username))
            {
                errorMessage = "Username cannot be empty";
                SetStatusMessage("Error: " + errorMessage);
                SetCreateSuccess(false);
                return false;
            }

            _usersLock.Enter();
            try
            {
                // Check if user already exists
                if (_users.Any(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase)))
                {
                    errorMessage = string.Format("User '{0}' already exists", username);
                    SetStatusMessage("Error: " + errorMessage);
                    SetCreateSuccess(false);
                    return false;
                }

                var newUser = new UserCredential(username, password, access);
                _users.Add(newUser);

                // Select the new user
                _selectedUserIndex = _users.Count - 1;
            }
            finally
            {
                _usersLock.Leave();
            }

            ScheduleSave();
            OnUsersChanged();
            UpdateAllFeedbacks();

            this.LogInformation("User '{username}' created with access level {access}", username, access);
            SetStatusMessage(string.Format("User '{0}' created successfully", username));
            SetCreateSuccess(true);
            return true;
        }

        /// <summary>
        /// Creates a new user (bridge-friendly overload)
        /// </summary>
        public bool CreateUser(string username, string password, int access)
        {
            string errorMessage;
            return CreateUser(username, password, access, out errorMessage);
        }

        /// <summary>
        /// Deletes a user by username
        /// </summary>
        /// <param name="username">Username to delete</param>
        /// <param name="errorMessage">Error message if deletion fails</param>
        /// <returns>True if user was deleted successfully</returns>
        public bool DeleteUser(string username, out string errorMessage)
        {
            errorMessage = string.Empty;

            if (string.IsNullOrEmpty(username))
            {
                errorMessage = "Username cannot be empty";
                SetStatusMessage("Error: " + errorMessage);
                SetDeleteSuccess(false);
                return false;
            }

            _usersLock.Enter();
            try
            {
                var user = _users.FirstOrDefault(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
                if (user == null)
                {
                    errorMessage = string.Format("User '{0}' not found", username);
                    SetStatusMessage("Error: " + errorMessage);
                    SetDeleteSuccess(false);
                    return false;
                }

                _users.Remove(user);

                // Adjust selected index
                if (_selectedUserIndex >= _users.Count)
                {
                    _selectedUserIndex = _users.Count - 1;
                }
            }
            finally
            {
                _usersLock.Leave();
            }

            ScheduleSave();
            OnUsersChanged();
            UpdateAllFeedbacks();

            this.LogInformation("User '{username}' deleted", username);
            SetStatusMessage(string.Format("User '{0}' deleted successfully", username));
            SetDeleteSuccess(true);
            return true;
        }

        /// <summary>
        /// Deletes a user (bridge-friendly overload)
        /// </summary>
        public bool DeleteUser(string username)
        {
            string errorMessage;
            return DeleteUser(username, out errorMessage);
        }

        /// <summary>
        /// Validates user credentials
        /// </summary>
        /// <param name="username">Username</param>
        /// <param name="password">Password</param>
        /// <param name="accessLevel">Access level of the validated user (output)</param>
        /// <param name="errorMessage">Error message if validation fails</param>
        /// <returns>True if credentials are valid</returns>
        public bool ValidateUser(string username, string password, out int accessLevel, out string errorMessage)
        {
            accessLevel = 0;
            errorMessage = string.Empty;

            if (string.IsNullOrEmpty(username))
            {
                errorMessage = "Username cannot be empty";
                SetStatusMessage("Error: " + errorMessage);
                SetValidateSuccess(false, string.Empty, 0);
                return false;
            }

            _usersLock.Enter();
            try
            {
                var user = _users.FirstOrDefault(u =>
                    u.Username.Equals(username, StringComparison.OrdinalIgnoreCase) &&
                    u.Password == password);

                if (user != null)
                {
                    accessLevel = user.Access;
                    this.LogDebug("User '{username}' validated successfully (Access: {access})", username, accessLevel);
                    SetStatusMessage(string.Format("User '{0}' validated (Access: {1})", username, accessLevel));
                    SetValidateSuccess(true, user.Username, user.Access);
                    return true;
                }
                else
                {
                    errorMessage = string.Format("Invalid credentials for user '{0}'", username);
                    this.LogDebug("Validation failed for user '{username}'", username);
                    SetStatusMessage(errorMessage);
                    SetValidateSuccess(false, string.Empty, 0);
                    return false;
                }
            }
            finally
            {
                _usersLock.Leave();
            }
        }

        /// <summary>
        /// Validates user credentials (bridge-friendly overload)
        /// </summary>
        public bool ValidateUser(string username, string password)
        {
            int accessLevel;
            string errorMessage;
            return ValidateUser(username, password, out accessLevel, out errorMessage);
        }

        /// <summary>
        /// Updates a user's password
        /// </summary>
        /// <param name="username">Username</param>
        /// <param name="newPassword">New password</param>
        /// <param name="errorMessage">Error message if update fails</param>
        /// <returns>True if password was updated</returns>
        public bool UpdateUserPassword(string username, string newPassword, out string errorMessage)
        {
            errorMessage = string.Empty;

            _usersLock.Enter();
            try
            {
                var user = _users.FirstOrDefault(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
                if (user == null)
                {
                    errorMessage = string.Format("User '{0}' not found", username);
                    SetStatusMessage("Error: " + errorMessage);
                    return false;
                }

                user.Password = newPassword;
            }
            finally
            {
                _usersLock.Leave();
            }

            ScheduleSave();
            OnUsersChanged();
            UpdateAllFeedbacks();

            this.LogInformation("Password updated for user '{username}'", username);
            SetStatusMessage(string.Format("Password updated for user '{0}'", username));
            return true;
        }

        /// <summary>
        /// Updates the selected user's password
        /// </summary>
        public bool UpdateSelectedUserPassword(string newPassword)
        {
            var user = GetSelectedUser();
            if (user == null)
            {
                SetStatusMessage("Error: No user selected");
                return false;
            }

            string errorMessage;
            return UpdateUserPassword(user.Username, newPassword, out errorMessage);
        }

        /// <summary>
        /// Updates a user's access level
        /// </summary>
        /// <param name="username">Username</param>
        /// <param name="newAccess">New access level</param>
        /// <param name="errorMessage">Error message if update fails</param>
        /// <returns>True if access was updated</returns>
        public bool UpdateUserAccess(string username, int newAccess, out string errorMessage)
        {
            errorMessage = string.Empty;

            _usersLock.Enter();
            try
            {
                var user = _users.FirstOrDefault(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
                if (user == null)
                {
                    errorMessage = string.Format("User '{0}' not found", username);
                    SetStatusMessage("Error: " + errorMessage);
                    return false;
                }

                user.Access = newAccess;
            }
            finally
            {
                _usersLock.Leave();
            }

            ScheduleSave();
            OnUsersChanged();
            UpdateAllFeedbacks();

            this.LogInformation("Access updated for user '{username}' to {access}", username, newAccess);
            SetStatusMessage(string.Format("Access updated for user '{0}' to {1}", username, newAccess));
            return true;
        }

        /// <summary>
        /// Updates the selected user's access level
        /// </summary>
        public bool UpdateSelectedUserAccess(int newAccess)
        {
            var user = GetSelectedUser();
            if (user == null)
            {
                SetStatusMessage("Error: No user selected");
                return false;
            }

            string errorMessage;
            return UpdateUserAccess(user.Username, newAccess, out errorMessage);
        }

        /// <summary>
        /// Updates a user's username
        /// </summary>
        /// <param name="oldUsername">Current username</param>
        /// <param name="newUsername">New username</param>
        /// <param name="errorMessage">Error message if update fails</param>
        /// <returns>True if username was updated</returns>
        public bool UpdateUsername(string oldUsername, string newUsername, out string errorMessage)
        {
            errorMessage = string.Empty;

            if (string.IsNullOrEmpty(newUsername))
            {
                errorMessage = "New username cannot be empty";
                SetStatusMessage("Error: " + errorMessage);
                return false;
            }

            _usersLock.Enter();
            try
            {
                var user = _users.FirstOrDefault(u => u.Username.Equals(oldUsername, StringComparison.OrdinalIgnoreCase));
                if (user == null)
                {
                    errorMessage = string.Format("User '{0}' not found", oldUsername);
                    SetStatusMessage("Error: " + errorMessage);
                    return false;
                }

                // Check if new username already exists (unless it's the same user)
                if (!oldUsername.Equals(newUsername, StringComparison.OrdinalIgnoreCase) &&
                    _users.Any(u => u.Username.Equals(newUsername, StringComparison.OrdinalIgnoreCase)))
                {
                    errorMessage = string.Format("Username '{0}' already exists", newUsername);
                    SetStatusMessage("Error: " + errorMessage);
                    return false;
                }

                user.Username = newUsername;
            }
            finally
            {
                _usersLock.Leave();
            }

            ScheduleSave();
            OnUsersChanged();
            UpdateAllFeedbacks();

            this.LogInformation("Username updated from '{oldUsername}' to '{newUsername}'", oldUsername, newUsername);
            SetStatusMessage(string.Format("Username updated to '{0}'", newUsername));
            return true;
        }

        /// <summary>
        /// Selects the next user in the list
        /// </summary>
        public void SelectNextUser()
        {
            _usersLock.Enter();
            try
            {
                if (_users.Count == 0) return;

                _selectedUserIndex++;
                if (_selectedUserIndex >= _users.Count)
                {
                    _selectedUserIndex = 0;
                }
            }
            finally
            {
                _usersLock.Leave();
            }

            UpdateSelectionFeedbacks();
        }

        /// <summary>
        /// Selects the previous user in the list
        /// </summary>
        public void SelectPreviousUser()
        {
            _usersLock.Enter();
            try
            {
                if (_users.Count == 0) return;

                _selectedUserIndex--;
                if (_selectedUserIndex < 0)
                {
                    _selectedUserIndex = _users.Count - 1;
                }
            }
            finally
            {
                _usersLock.Leave();
            }

            UpdateSelectionFeedbacks();
        }

        /// <summary>
        /// Selects a user by index
        /// </summary>
        public void SelectUserByIndex(int index)
        {
            _usersLock.Enter();
            try
            {
                if (index < 0 || index >= _users.Count) return;
                _selectedUserIndex = index;
            }
            finally
            {
                _usersLock.Leave();
            }

            UpdateSelectionFeedbacks();
        }

        /// <summary>
        /// Gets a user by index
        /// </summary>
        /// <param name="index">Index (0-based)</param>
        /// <returns>User credential or null if not found</returns>
        public UserCredential GetUserByIndex(int index)
        {
            _usersLock.Enter();
            try
            {
                if (index < 0 || index >= _users.Count)
                {
                    return null;
                }
                return _users[index];
            }
            finally
            {
                _usersLock.Leave();
            }
        }

        /// <summary>
        /// Reloads users from the file
        /// </summary>
        public void RefreshUsers()
        {
            LoadUsersFromFile();
        }

        #endregion

        #region Helper Methods

        private UserCredential GetSelectedUser()
        {
            _usersLock.Enter();
            try
            {
                if (_selectedUserIndex < 0 || _selectedUserIndex >= _users.Count)
                {
                    return null;
                }
                return _users[_selectedUserIndex];
            }
            finally
            {
                _usersLock.Leave();
            }
        }

        private string GetSelectedPasswordDisplay()
        {
            var user = GetSelectedUser();
            if (user == null) return string.Empty;

            if (_config.MaskPasswords)
            {
                return new string('*', user.Password.Length);
            }

            return user.Password;
        }

        private string GetUserListJson()
        {
            _usersLock.Enter();
            try
            {
                if (_config.MaskPasswords)
                {
                    var maskedUsers = _users.Select(u => new
                    {
                        username = u.Username,
                        password = new string('*', u.Password.Length),
                        access = u.Access
                    });
                    return JsonConvert.SerializeObject(maskedUsers);
                }

                return JsonConvert.SerializeObject(_users);
            }
            catch
            {
                return "[]";
            }
            finally
            {
                _usersLock.Leave();
            }
        }

        private void SetStatusMessage(string message)
        {
            _statusMessage = message;
            this.LogDebug("Status: {message}", message);
            StatusMessageFeedback?.FireUpdate();
        }

        private void SetCreateSuccess(bool success)
        {
            _createUserSuccess = success;
            CreateUserSuccessFeedback?.FireUpdate();

            // Reset after a short delay
            new CTimer(_ =>
            {
                _createUserSuccess = false;
                CreateUserSuccessFeedback?.FireUpdate();
            }, 500);
        }

        private void SetDeleteSuccess(bool success)
        {
            _deleteUserSuccess = success;
            DeleteUserSuccessFeedback?.FireUpdate();

            // Reset after a short delay
            new CTimer(_ =>
            {
                _deleteUserSuccess = false;
                DeleteUserSuccessFeedback?.FireUpdate();
            }, 500);
        }

        private void SetValidateSuccess(bool success, string username, int access)
        {
            _validateUserSuccess = success;
            _lastValidatedUsername = username;
            _lastValidatedAccess = access;

            ValidateUserSuccessFeedback?.FireUpdate();
            ValidatedUsernameFeedback?.FireUpdate();
            ValidatedUserAccessFeedback?.FireUpdate();

            // Reset success flag after a short delay (but keep username/access for reference)
            new CTimer(_ =>
            {
                _validateUserSuccess = false;
                ValidateUserSuccessFeedback?.FireUpdate();
            }, 500);
        }

        private void OnUsersChanged()
        {
            var handler = UsersChanged;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        private void UpdateAllFeedbacks()
        {
            UserCountFeedback?.FireUpdate();
            UserListFeedback?.FireUpdate();
            UpdateSelectionFeedbacks();
        }

        private void UpdateSelectionFeedbacks()
        {
            SelectedUserIndexFeedback?.FireUpdate();
            SelectedUsernameFeedback?.FireUpdate();
            SelectedPasswordFeedback?.FireUpdate();
            SelectedAccessFeedback?.FireUpdate();
            SelectedUserAccessFeedback?.FireUpdate();
        }

        #endregion

        #region Bridge Linking

        /// <summary>
        /// Link to API - Server can only be bridged once
        /// </summary>
        public override void LinkToApi(BasicTriList triList, uint joinStart, string joinMapKey, EiscApiAdvanced bridge)
        {
            if (_isBridged)
            {
                this.LogWarning("Password Server '{0}' is already bridged. Only one bridge connection is allowed.", Key);
                return;
            }

            _isBridged = true;

            var joinMap = new PasswordServerBridgeJoinMap(joinStart);

            if (bridge != null)
            {
                bridge.AddJoinMap(Key, joinMap);
            }

            var joinMapSerialized = JoinMapHelper.TryGetJoinMapAdvancedForDevice(joinMapKey);
            if (joinMapSerialized != null)
            {
                joinMap.SetCustomJoinData(joinMapSerialized);
            }

            this.LogInformation("Linking Password Server to EISC bridge at join {0}", joinStart);

            // Serial inputs
            triList.SetStringSigAction(joinMap.UsernameInput.JoinNumber, value => _usernameInput = value);
            triList.SetStringSigAction(joinMap.PasswordInput.JoinNumber, value => _passwordInput = value);
            triList.SetStringSigAction(joinMap.AccessInput.JoinNumber, value => _accessInput = value);

            // Digital triggers
            triList.SetSigTrueAction(joinMap.CreateUser.JoinNumber, () =>
            {
                int access;
                int.TryParse(_accessInput, out access);
                CreateUser(_usernameInput, _passwordInput, access);
            });

            triList.SetSigTrueAction(joinMap.DeleteUser.JoinNumber, () => DeleteUser(_usernameInput));

            triList.SetSigTrueAction(joinMap.ValidateUser.JoinNumber, () => ValidateUser(_usernameInput, _passwordInput));

            triList.SetSigTrueAction(joinMap.RefreshUsers.JoinNumber, () => RefreshUsers());

            triList.SetSigTrueAction(joinMap.SelectNextUser.JoinNumber, () => SelectNextUser());

            triList.SetSigTrueAction(joinMap.SelectPreviousUser.JoinNumber, () => SelectPreviousUser());

            triList.SetSigTrueAction(joinMap.UpdatePassword.JoinNumber, () => UpdateSelectedUserPassword(_passwordInput));

            triList.SetSigTrueAction(joinMap.UpdateAccess.JoinNumber, () =>
            {
                int access;
                if (int.TryParse(_accessInput, out access))
                {
                    UpdateSelectedUserAccess(access);
                }
            });

            // Analog input for user selection
            triList.SetUShortSigAction(joinMap.SelectedUserIndex.JoinNumber, value => SelectUserByIndex(value));

            // Link feedbacks
            triList.SetString(joinMap.DeviceName.JoinNumber, Name);

            UserCountFeedback.LinkInputSig(triList.UShortInput[joinMap.UserCountFb.JoinNumber]);
            SelectedUserIndexFeedback.LinkInputSig(triList.UShortInput[joinMap.SelectedUserIndex.JoinNumber]);
            SelectedUserAccessFeedback.LinkInputSig(triList.UShortInput[joinMap.SelectedUserAccessFb.JoinNumber]);
            ValidatedUserAccessFeedback.LinkInputSig(triList.UShortInput[joinMap.ValidatedUserAccessFb.JoinNumber]);

            CreateUserSuccessFeedback.LinkInputSig(triList.BooleanInput[joinMap.CreateUserSuccessFb.JoinNumber]);
            DeleteUserSuccessFeedback.LinkInputSig(triList.BooleanInput[joinMap.DeleteUserSuccessFb.JoinNumber]);
            ValidateUserSuccessFeedback.LinkInputSig(triList.BooleanInput[joinMap.ValidateUserSuccessFb.JoinNumber]);

            UserListFeedback.LinkInputSig(triList.StringInput[joinMap.UserListFb.JoinNumber]);
            StatusMessageFeedback.LinkInputSig(triList.StringInput[joinMap.StatusMessageFb.JoinNumber]);
            SelectedUsernameFeedback.LinkInputSig(triList.StringInput[joinMap.SelectedUsernameFb.JoinNumber]);
            SelectedPasswordFeedback.LinkInputSig(triList.StringInput[joinMap.SelectedPasswordFb.JoinNumber]);
            SelectedAccessFeedback.LinkInputSig(triList.StringInput[joinMap.SelectedAccessFb.JoinNumber]);
            ValidatedUsernameFeedback.LinkInputSig(triList.StringInput[joinMap.ValidatedUsernameFb.JoinNumber]);

            // Online status
            triList.OnlineStatusChange += (sender, args) =>
            {
                if (!args.DeviceOnLine) return;

                triList.SetString(joinMap.DeviceName.JoinNumber, Name);
                UpdateAllFeedbacks();
            };

            // Initial update
            UpdateAllFeedbacks();
        }

        #endregion

        #region Console Command

        private void ConsoleCommand(string command)
        {
            if (string.IsNullOrEmpty(command))
            {
                this.LogInformation("Server Commands: list | add [u p a] | del [u] | val [u p]");
                return;
            }

            var args = command.Split(' ');
            var cmd = args[0].ToLower();

            switch (cmd)
            {
                case "list":
                    this.LogInformation("Users ({count}):", UserCount);
                    foreach (var user in Users)
                    {
                        this.LogInformation("  {username} (Access: {access})", user.Username, user.Access);
                    }
                    break;

                case "add":
                    if (args.Length < 4)
                    {
                        this.LogWarning("Usage: add [username] [password] [access]");
                        return;
                    }
                    int accessLevel;
                    if (!int.TryParse(args[3], out accessLevel))
                    {
                        this.LogWarning("Invalid access level: {level}", args[3]);
                        return;
                    }
                    string createError;
                    if (CreateUser(args[1], args[2], accessLevel, out createError))
                    {
                        this.LogInformation("User created successfully");
                    }
                    else
                    {
                        this.LogWarning("Failed: {error}", createError);
                    }
                    break;

                case "del":
                    if (args.Length < 2)
                    {
                        this.LogWarning("Usage: del [username]");
                        return;
                    }
                    string deleteError;
                    if (DeleteUser(args[1], out deleteError))
                    {
                        this.LogInformation("User deleted successfully");
                    }
                    else
                    {
                        this.LogWarning("Failed: {error}", deleteError);
                    }
                    break;

                case "val":
                    if (args.Length < 3)
                    {
                        this.LogWarning("Usage: val [username] [password]");
                        return;
                    }
                    int access;
                    string valError;
                    if (ValidateUser(args[1], args[2], out access, out valError))
                    {
                        this.LogInformation("Valid! Access level: {access}", access);
                    }
                    else
                    {
                        this.LogWarning("Invalid: {error}", valError);
                    }
                    break;

                default:
                    this.LogWarning("Unknown command: {cmd}", cmd);
                    break;
            }
        }

        #endregion
    }
}