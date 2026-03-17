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

namespace PepperDash.Essentials.Plugin.PasswordManager
{
    /// <summary>
    /// Password Manager Logic Device
    /// </summary>
    /// <remarks>
    /// Manages user credentials stored in a JSON file with bridge access for CRUD operations
    /// </remarks>
    public class PasswordManagerLogicDevice : EssentialsBridgeableDevice
    {
        #region Fields

        private readonly PasswordManagerConfig _config;
        private readonly DeviceConfig _deviceConfig;
        private readonly CCriticalSection _fileLock = new CCriticalSection();
        private readonly CTimer _saveTimer;
        private List<UserCredential> _users;
        private int _selectedUserIndex;

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

        private string _statusMessage = string.Empty;

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor for Password Manager Logic Device
        /// </summary>
        /// <param name="key">Device key</param>
        /// <param name="name">Device name</param>
        /// <param name="config">Password Manager configuration</param>
        /// <param name="deviceConfig">Full device configuration</param>
        public PasswordManagerLogicDevice(string key, string name, PasswordManagerConfig config, DeviceConfig deviceConfig)
            : base(key, name)
        {
            this.LogInformation("Constructing Password Manager: {name}", name);

            _config = config;
            _deviceConfig = deviceConfig;
            _users = new List<UserCredential>();
            _selectedUserIndex = -1;

            // Initialize save timer
            var saveDelay = _config.SaveDelayMs > 0 ? _config.SaveDelayMs : 1000;
            _saveTimer = new CTimer(_ => SaveUsersToFile(), Timeout.Infinite);

            // Initialize feedbacks
            InitializeFeedbacks();

            // Register console commands
            CrestronConsole.AddNewConsoleCommand(ConsoleCommand, "passwordmanager",
                "list | add [user pass access] | delete [user] | validate [user pass]",
                ConsoleAccessLevelEnum.AccessOperator);
        }

        private void InitializeFeedbacks()
        {
            UserCountFeedback = new IntFeedback("UserCount", () => _users?.Count ?? 0);
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
        /// <returns>True if initialization was successful</returns>
        public override bool CustomActivate()
        {
            this.LogDebug("Activating Password Manager");

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
                _users = _config.DefaultUsers != null ? new List<UserCredential>(_config.DefaultUsers) : new List<UserCredential>();
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
                    _users = JsonConvert.DeserializeObject<List<UserCredential>>(json) ?? new List<UserCredential>();
                    SetStatusMessage(string.Format("Loaded {0} users from file", _users.Count));
                }
                else
                {
                    this.LogInformation("File does not exist, creating with default users: {path}", fullPath);
                    _users = _config.DefaultUsers != null ? new List<UserCredential>(_config.DefaultUsers) : new List<UserCredential>();

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
                _users = new List<UserCredential>();
                SetStatusMessage(string.Format("Error loading users: {0}", ex.Message));
            }
            finally
            {
                _fileLock.Leave();
            }

            // Reset selection
            _selectedUserIndex = _users.Count > 0 ? 0 : -1;
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
                var json = JsonConvert.SerializeObject(_users, Formatting.Indented);

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
        /// <returns>True if user was created successfully</returns>
        public bool CreateUser(string username, string password, int access)
        {
            if (string.IsNullOrEmpty(username))
            {
                SetStatusMessage("Error: Username cannot be empty");
                SetCreateSuccess(false);
                return false;
            }

            // Check if user already exists
            if (_users.Any(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase)))
            {
                SetStatusMessage(string.Format("Error: User '{0}' already exists", username));
                SetCreateSuccess(false);
                return false;
            }

            var newUser = new UserCredential(username, password, access);
            _users.Add(newUser);

            ScheduleSave();
            SetStatusMessage(string.Format("User '{0}' created successfully", username));
            SetCreateSuccess(true);

            // Select the new user
            _selectedUserIndex = _users.Count - 1;
            UpdateAllFeedbacks();

            return true;
        }

        /// <summary>
        /// Deletes a user by username
        /// </summary>
        /// <param name="username">Username to delete</param>
        /// <returns>True if user was deleted successfully</returns>
        public bool DeleteUser(string username)
        {
            if (string.IsNullOrEmpty(username))
            {
                SetStatusMessage("Error: Username cannot be empty");
                SetDeleteSuccess(false);
                return false;
            }

            var user = _users.FirstOrDefault(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
            if (user == null)
            {
                SetStatusMessage(string.Format("Error: User '{0}' not found", username));
                SetDeleteSuccess(false);
                return false;
            }

            _users.Remove(user);

            ScheduleSave();
            SetStatusMessage(string.Format("User '{0}' deleted successfully", username));
            SetDeleteSuccess(true);

            // Adjust selected index
            if (_selectedUserIndex >= _users.Count)
            {
                _selectedUserIndex = _users.Count - 1;
            }

            UpdateAllFeedbacks();

            return true;
        }

        /// <summary>
        /// Validates user credentials
        /// </summary>
        /// <param name="username">Username</param>
        /// <param name="password">Password</param>
        /// <returns>True if credentials are valid</returns>
        public bool ValidateUser(string username, string password)
        {
            if (string.IsNullOrEmpty(username))
            {
                SetStatusMessage("Error: Username cannot be empty");
                SetValidateSuccess(false, string.Empty, 0);
                return false;
            }

            var user = _users.FirstOrDefault(u =>
                u.Username.Equals(username, StringComparison.OrdinalIgnoreCase) &&
                u.Password == password);

            if (user != null)
            {
                SetStatusMessage(string.Format("User '{0}' validated successfully (Access: {1})", username, user.Access));
                SetValidateSuccess(true, user.Username, user.Access);
                return true;
            }
            else
            {
                SetStatusMessage(string.Format("Invalid credentials for user '{0}'", username));
                SetValidateSuccess(false, string.Empty, 0);
                return false;
            }
        }

        /// <summary>
        /// Updates the selected user's password
        /// </summary>
        /// <param name="newPassword">New password</param>
        /// <returns>True if password was updated</returns>
        public bool UpdateSelectedUserPassword(string newPassword)
        {
            var user = GetSelectedUser();
            if (user == null)
            {
                SetStatusMessage("Error: No user selected");
                return false;
            }

            user.Password = newPassword;
            ScheduleSave();
            SetStatusMessage(string.Format("Password updated for user '{0}'", user.Username));
            UpdateAllFeedbacks();
            return true;
        }

        /// <summary>
        /// Updates the selected user's access level
        /// </summary>
        /// <param name="newAccess">New access level</param>
        /// <returns>True if access was updated</returns>
        public bool UpdateSelectedUserAccess(int newAccess)
        {
            var user = GetSelectedUser();
            if (user == null)
            {
                SetStatusMessage("Error: No user selected");
                return false;
            }

            user.Access = newAccess;
            ScheduleSave();
            SetStatusMessage(string.Format("Access updated for user '{0}' to {1}", user.Username, newAccess));
            UpdateAllFeedbacks();
            return true;
        }

        /// <summary>
        /// Selects the next user in the list
        /// </summary>
        public void SelectNextUser()
        {
            if (_users.Count == 0)
            {
                return;
            }

            _selectedUserIndex++;
            if (_selectedUserIndex >= _users.Count)
            {
                _selectedUserIndex = 0;
            }

            UpdateSelectionFeedbacks();
        }

        /// <summary>
        /// Selects the previous user in the list
        /// </summary>
        public void SelectPreviousUser()
        {
            if (_users.Count == 0)
            {
                return;
            }

            _selectedUserIndex--;
            if (_selectedUserIndex < 0)
            {
                _selectedUserIndex = _users.Count - 1;
            }

            UpdateSelectionFeedbacks();
        }

        /// <summary>
        /// Selects a user by index
        /// </summary>
        /// <param name="index">User index (0-based)</param>
        public void SelectUserByIndex(int index)
        {
            if (index < 0 || index >= _users.Count)
            {
                return;
            }

            _selectedUserIndex = index;
            UpdateSelectionFeedbacks();
        }

        #endregion

        #region Helper Methods

        private UserCredential GetSelectedUser()
        {
            if (_selectedUserIndex < 0 || _selectedUserIndex >= _users.Count)
            {
                return null;
            }

            return _users[_selectedUserIndex];
        }

        private string GetSelectedPasswordDisplay()
        {
            var user = GetSelectedUser();
            if (user == null)
            {
                return string.Empty;
            }

            if (_config.MaskPasswords)
            {
                return new string('*', user.Password.Length);
            }

            return user.Password;
        }

        private string GetUserListJson()
        {
            try
            {
                // Return a sanitized list (optionally with masked passwords)
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

        #region Console Command

        private void ConsoleCommand(string command)
        {
            if (string.IsNullOrEmpty(command))
            {
                this.LogInformation("Password Manager Commands:");
                this.LogInformation("  list - List all users");
                this.LogInformation("  add [username] [password] [access] - Add a new user");
                this.LogInformation("  delete [username] - Delete a user");
                this.LogInformation("  validate [username] [password] - Validate credentials");
                return;
            }

            var args = command.Split(' ');
            var cmd = args[0].ToLower();

            switch (cmd)
            {
                case "list":
                    this.LogInformation("Users ({count}):", _users.Count);
                    foreach (var user in _users)
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
                    CreateUser(args[1], args[2], accessLevel);
                    break;

                case "delete":
                    if (args.Length < 2)
                    {
                        this.LogWarning("Usage: delete [username]");
                        return;
                    }
                    DeleteUser(args[1]);
                    break;

                case "validate":
                    if (args.Length < 3)
                    {
                        this.LogWarning("Usage: validate [username] [password]");
                        return;
                    }
                    ValidateUser(args[1], args[2]);
                    break;

                default:
                    this.LogWarning("Unknown command: {cmd}", cmd);
                    break;
            }
        }

        #endregion

        #region Bridge Linking

        /// <summary>
        /// Links the plugin device to the EISC bridge
        /// </summary>
        /// <param name="trilist">Bridge tri-list</param>
        /// <param name="joinStart">Starting join number</param>
        /// <param name="joinMapKey">Join map key</param>
        /// <param name="bridge">Bridge instance</param>
        public override void LinkToApi(BasicTriList trilist, uint joinStart, string joinMapKey, EiscApiAdvanced bridge)
        {
            var joinMap = new PasswordManagerBridgeJoinMap(joinStart);

            // Add join map to bridge
            if (bridge != null)
            {
                bridge.AddJoinMap(Key, joinMap);
            }

            var customJoins = JoinMapHelper.TryGetJoinMapAdvancedForDevice(joinMapKey);
            if (customJoins != null)
            {
                joinMap.SetCustomJoinData(customJoins);
            }

            this.LogDebug("Linking to Trilist {id}", trilist.ID.ToString("X"));

            // Serial inputs
            trilist.SetStringSigAction(joinMap.UsernameInput.JoinNumber, value => _usernameInput = value);
            trilist.SetStringSigAction(joinMap.PasswordInput.JoinNumber, value => _passwordInput = value);
            trilist.SetStringSigAction(joinMap.AccessInput.JoinNumber, value => _accessInput = value);

            // Digital triggers
            trilist.SetSigTrueAction(joinMap.CreateUser.JoinNumber, () =>
            {
                int access;
                int.TryParse(_accessInput, out access);
                CreateUser(_usernameInput, _passwordInput, access);
            });

            trilist.SetSigTrueAction(joinMap.DeleteUser.JoinNumber, () => DeleteUser(_usernameInput));

            trilist.SetSigTrueAction(joinMap.ValidateUser.JoinNumber, () => ValidateUser(_usernameInput, _passwordInput));

            trilist.SetSigTrueAction(joinMap.RefreshUsers.JoinNumber, () => LoadUsersFromFile());

            trilist.SetSigTrueAction(joinMap.SelectNextUser.JoinNumber, () => SelectNextUser());

            trilist.SetSigTrueAction(joinMap.SelectPreviousUser.JoinNumber, () => SelectPreviousUser());

            trilist.SetSigTrueAction(joinMap.UpdatePassword.JoinNumber, () => UpdateSelectedUserPassword(_passwordInput));

            trilist.SetSigTrueAction(joinMap.UpdateAccess.JoinNumber, () =>
            {
                int access;
                if (int.TryParse(_accessInput, out access))
                {
                    UpdateSelectedUserAccess(access);
                }
            });

            // Analog input for user selection
            trilist.SetUShortSigAction(joinMap.SelectedUserIndex.JoinNumber, value => SelectUserByIndex(value));

            // Link feedbacks
            trilist.SetString(joinMap.DeviceName.JoinNumber, Name);

            UserCountFeedback.LinkInputSig(trilist.UShortInput[joinMap.UserCountFb.JoinNumber]);
            SelectedUserIndexFeedback.LinkInputSig(trilist.UShortInput[joinMap.SelectedUserIndex.JoinNumber]);
            SelectedUserAccessFeedback.LinkInputSig(trilist.UShortInput[joinMap.SelectedUserAccessFb.JoinNumber]);
            ValidatedUserAccessFeedback.LinkInputSig(trilist.UShortInput[joinMap.ValidatedUserAccessFb.JoinNumber]);

            CreateUserSuccessFeedback.LinkInputSig(trilist.BooleanInput[joinMap.CreateUserSuccessFb.JoinNumber]);
            DeleteUserSuccessFeedback.LinkInputSig(trilist.BooleanInput[joinMap.DeleteUserSuccessFb.JoinNumber]);
            ValidateUserSuccessFeedback.LinkInputSig(trilist.BooleanInput[joinMap.ValidateUserSuccessFb.JoinNumber]);

            UserListFeedback.LinkInputSig(trilist.StringInput[joinMap.UserListFb.JoinNumber]);
            StatusMessageFeedback.LinkInputSig(trilist.StringInput[joinMap.StatusMessageFb.JoinNumber]);
            SelectedUsernameFeedback.LinkInputSig(trilist.StringInput[joinMap.SelectedUsernameFb.JoinNumber]);
            SelectedPasswordFeedback.LinkInputSig(trilist.StringInput[joinMap.SelectedPasswordFb.JoinNumber]);
            SelectedAccessFeedback.LinkInputSig(trilist.StringInput[joinMap.SelectedAccessFb.JoinNumber]);
            ValidatedUsernameFeedback.LinkInputSig(trilist.StringInput[joinMap.ValidatedUsernameFb.JoinNumber]);

            // Update feedbacks on trilist online
            trilist.OnlineStatusChange += (o, a) =>
            {
                if (!a.DeviceOnLine)
                {
                    return;
                }

                trilist.SetString(joinMap.DeviceName.JoinNumber, Name);
                UpdateAllFeedbacks();
            };

            // Initial update
            UpdateAllFeedbacks();
        }

        #endregion
    }
}

