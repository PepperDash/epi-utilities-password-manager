using System;
using System.Collections.Generic;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro.DeviceSupport;
using Newtonsoft.Json;
using PepperDash.Core;
using PepperDash.Core.Logging;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Bridges;
using PepperDash.Essentials.Plugin.Password.Server;
using PepperDash.Essentials.Plugin.PasswordManager;

namespace PepperDash.Essentials.Plugin.Password.Client
{
    /// <summary>
    /// Password Client Device
    /// </summary>
    /// <remarks>
    /// Client device that connects to a Password Server.
    /// Provides input buffering with feedback signals and password unmask control.
    /// Multiple clients can bridge to panels and interact with the server as the authentication authority.
    /// </remarks>
    public class PasswordClient : EssentialsBridgeableDevice
    {
        private readonly PasswordClientConfig _config;
        private PasswordServer _server;
        private bool _serverConnected;

        // Input buffers
        private string _usernameInput = string.Empty;
        private string _passwordInput = string.Empty;
        private string _accessInput = string.Empty;
        private int _accessLevelInput;
        private bool _unmaskPassword;

        // Selected user tracking
        private int _selectedUserIndex;
        private List<UserCredential> _cachedUsers = new List<UserCredential>();

        // Validated user info
        private string _validatedUsername = string.Empty;
        private int _validatedAccess;

        // Edit mode tracking
        private string _editingUsername = string.Empty;  // Original username being edited
        private string _editingOriginalPassword = string.Empty;  // Original password for change detection

        // User list display (max 20 items)
        private const int MaxUserListItems = 20;

        // Bridge references
        private BasicTriList _triList;
        private PasswordClientBridgeJoinMap _joinMap;

        #region Feedbacks

        /// <summary>
        /// Device online feedback
        /// </summary>
        public BoolFeedback IsOnlineFeedback { get; private set; }

        /// <summary>
        /// Server connected feedback
        /// </summary>
        public BoolFeedback ServerConnectedFeedback { get; private set; }

        /// <summary>
        /// Validate user success feedback
        /// </summary>
        public BoolFeedback ValidateUserSuccessFeedback { get; private set; }

        /// <summary>
        /// Create user success feedback
        /// </summary>
        public BoolFeedback CreateUserSuccessFeedback { get; private set; }

        /// <summary>
        /// Delete user success feedback
        /// </summary>
        public BoolFeedback DeleteUserSuccessFeedback { get; private set; }

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
        /// Username input feedback
        /// </summary>
        public StringFeedback UsernameInputFeedback { get; private set; }

        /// <summary>
        /// Password input feedback (masked/unmasked based on signal)
        /// </summary>
        public StringFeedback PasswordInputFeedback { get; private set; }

        /// <summary>
        /// Access input feedback
        /// </summary>
        public StringFeedback AccessInputFeedback { get; private set; }

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
        /// Selected password feedback
        /// </summary>
        public StringFeedback SelectedPasswordFeedback { get; private set; }

        /// <summary>
        /// Selected access feedback
        /// </summary>
        public StringFeedback SelectedAccessFeedback { get; private set; }

        /// <summary>
        /// Validated username feedback
        /// </summary>
        public StringFeedback ValidatedUsernameFeedback { get; private set; }

        /// <summary>
        /// Original username being edited feedback
        /// </summary>
        public StringFeedback EditingUsernameFeedback { get; private set; }

        /// <summary>
        /// Save button enabled feedback
        /// </summary>
        public BoolFeedback SaveEnabledFeedback { get; private set; }

        /// <summary>
        /// Has unsaved changes feedback
        /// </summary>
        public BoolFeedback HasChangesFeedback { get; private set; }

        /// <summary>
        /// Update user success feedback
        /// </summary>
        public BoolFeedback UpdateUserSuccessFeedback { get; private set; }

        #endregion

        // Feedback backing values
        private bool _validateSuccess;
        private bool _createSuccess;
        private bool _deleteSuccess;
        private bool _updateSuccess;
        private string _statusMessage = string.Empty;

        // Timer to auto-reset success flags (creates proper pulse)
        private CTimer _successResetTimer;
        private const long SuccessPulseDurationMs = 500;

        /// <summary>
        /// Constructor
        /// </summary>
        public PasswordClient(string key, string name, PasswordClientConfig config)
            : base(key, name)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));

            InitializeFeedbacks();
        }

        private void InitializeFeedbacks()
        {
            IsOnlineFeedback = new BoolFeedback("isOnline", () => _serverConnected);
            ServerConnectedFeedback = new BoolFeedback("serverConnected", () => _serverConnected);
            ValidateUserSuccessFeedback = new BoolFeedback("validateUserSuccess", () => _validateSuccess);
            CreateUserSuccessFeedback = new BoolFeedback("createUserSuccess", () => _createSuccess);
            DeleteUserSuccessFeedback = new BoolFeedback("deleteUserSuccess", () => _deleteSuccess);

            UserCountFeedback = new IntFeedback("userCount", () => _cachedUsers.Count);
            SelectedUserIndexFeedback = new IntFeedback("selectedUserIndex", () => _selectedUserIndex);
            SelectedUserAccessFeedback = new IntFeedback("selectedUserAccess", () => GetSelectedUserAccess());
            ValidatedUserAccessFeedback = new IntFeedback("validatedUserAccess", () => _validatedAccess);

            UsernameInputFeedback = new StringFeedback("usernameInput", () => _usernameInput);
            PasswordInputFeedback = new StringFeedback("passwordInput", () => GetPasswordInputDisplay());
            AccessInputFeedback = new StringFeedback("accessInput", () => _accessInput);
            UserListFeedback = new StringFeedback("userList", () => GetUserListJson());
            StatusMessageFeedback = new StringFeedback("statusMessage", () => _statusMessage);
            SelectedUsernameFeedback = new StringFeedback("selectedUsername", () => GetSelectedUsername());
            SelectedPasswordFeedback = new StringFeedback("selectedPassword", () => GetSelectedPassword());
            SelectedAccessFeedback = new StringFeedback("selectedAccess", () => GetSelectedAccess());
            ValidatedUsernameFeedback = new StringFeedback("validatedUsername", () => _validatedUsername);
            EditingUsernameFeedback = new StringFeedback("editingUsername", () => _editingUsername);

            // Edit mode feedbacks
            SaveEnabledFeedback = new BoolFeedback("saveEnabled", () => GetSaveEnabled());
            HasChangesFeedback = new BoolFeedback("hasChanges", () => GetHasChanges());
            UpdateUserSuccessFeedback = new BoolFeedback("updateUserSuccess", () => _updateSuccess);
        }

        /// <summary>
        /// Initialize and connect to server
        /// </summary>
        public override bool CustomActivate()
        {
            ConnectToServer();
            return base.CustomActivate();
        }

        private void ConnectToServer()
        {
            if (string.IsNullOrEmpty(_config.ServerKey))
            {
                this.LogInformation("ServerKey is not configured");
                SetStatusMessage("Error: Server not configured");
                return;
            }

            var device = DeviceManager.GetDeviceForKey(_config.ServerKey);
            if (device == null)
            {
                this.LogInformation("Server '{0}' not found", _config.ServerKey);
                SetStatusMessage($"Error: Server '{_config.ServerKey}' not found");
                return;
            }

            _server = device as PasswordServer;
            if (_server == null)
            {
                this.LogInformation("Device '{0}' is not a PasswordServer", _config.ServerKey);
                SetStatusMessage("Error: Invalid server type");
                return;
            }

            // Subscribe to server events
            _server.UsersChanged += OnServerUsersChanged;
            _serverConnected = true;

            // Initial load
            RefreshUsersFromServer();

            this.LogError("Connected to server: {0}", _config.ServerKey);
            SetStatusMessage("Connected to server");

            ServerConnectedFeedback.FireUpdate();
            IsOnlineFeedback.FireUpdate();
        }

        private void OnServerUsersChanged(object sender, EventArgs e)
        {
            RefreshUsersFromServer();
        }

        private void RefreshUsersFromServer()
        {
            if (_server == null) return;

            _cachedUsers = new List<UserCredential>(_server.Users);

            // Validate selected index
            if (_selectedUserIndex >= _cachedUsers.Count)
            {
                _selectedUserIndex = Math.Max(0, _cachedUsers.Count - 1);
            }

            UpdateAllFeedbacks();
        }

        #region Input Handling

        /// <summary>
        /// Set username input
        /// </summary>
        public void SetUsernameInput(string value)
        {
            _usernameInput = value ?? string.Empty;
            UsernameInputFeedback.FireUpdate();
            UpdateEditModeFeedbacks();
        }

        /// <summary>
        /// Set password input
        /// </summary>
        public void SetPasswordInput(string value)
        {
            _passwordInput = value ?? string.Empty;
            PasswordInputFeedback.FireUpdate();
            UpdateEditModeFeedbacks();
        }

        /// <summary>
        /// Set access input (string)
        /// </summary>
        public void SetAccessInput(string value)
        {
            _accessInput = value ?? string.Empty;
            AccessInputFeedback.FireUpdate();
        }

        /// <summary>
        /// Set access level input (analog)
        /// </summary>
        public void SetAccessLevelInput(int value)
        {
            _accessLevelInput = value;
        }

        /// <summary>
        /// Set unmask password state
        /// </summary>
        public void SetUnmaskPassword(bool unmask)
        {
            _unmaskPassword = unmask;
            PasswordInputFeedback.FireUpdate();
        }

        /// <summary>
        /// Clear all input fields
        /// </summary>
        public void ClearInputs()
        {
            _usernameInput = string.Empty;
            _passwordInput = string.Empty;
            _accessInput = string.Empty;
            _accessLevelInput = 0;
            _editingUsername = string.Empty;
            _editingOriginalPassword = string.Empty;

            UsernameInputFeedback.FireUpdate();
            PasswordInputFeedback.FireUpdate();
            AccessInputFeedback.FireUpdate();
            EditingUsernameFeedback.FireUpdate();
            UpdateEditModeFeedbacks();

            this.LogVerbose("Inputs cleared");
        }

        /// <summary>
        /// Load selected user's data into input fields for editing
        /// </summary>
        public void LoadSelectedUserToInputs()
        {
            if (_selectedUserIndex < 0 || _selectedUserIndex >= _cachedUsers.Count)
            {
                SetStatusMessage("No user selected");
                return;
            }

            var user = _cachedUsers[_selectedUserIndex];
            _editingUsername = user.Username;
            _editingOriginalPassword = user.Password;
            _usernameInput = user.Username;
            _passwordInput = user.Password;

            UsernameInputFeedback.FireUpdate();
            PasswordInputFeedback.FireUpdate();
            EditingUsernameFeedback.FireUpdate();
            UpdateEditModeFeedbacks();

            this.LogVerbose("Loaded user '{0}' to inputs for editing", user.Username);
        }

        private bool GetSaveEnabled()
        {
            // For create mode (no editing username): both username and password must have content
            if (string.IsNullOrEmpty(_editingUsername))
            {
                return !string.IsNullOrEmpty(_usernameInput) && !string.IsNullOrEmpty(_passwordInput);
            }

            // For edit mode: at least one field must have changed
            return GetHasChanges();
        }

        private bool GetHasChanges()
        {
            if (string.IsNullOrEmpty(_editingUsername))
            {
                return false;  // Not in edit mode
            }

            // Check if username or password has changed from original
            bool usernameChanged = !_usernameInput.Equals(_editingUsername, StringComparison.Ordinal);
            bool passwordChanged = !_passwordInput.Equals(_editingOriginalPassword, StringComparison.Ordinal);

            return usernameChanged || passwordChanged;
        }

        private void UpdateEditModeFeedbacks()
        {
            SaveEnabledFeedback.FireUpdate();
            HasChangesFeedback.FireUpdate();
        }

        private string GetPasswordInputDisplay()
        {
            if (string.IsNullOrEmpty(_passwordInput))
                return string.Empty;

            if (_unmaskPassword)
                return _passwordInput;

            if (_config.MaskPasswords)
                return new string('*', _passwordInput.Length);

            return _passwordInput;
        }

        #endregion

        #region User Operations

        /// <summary>
        /// Validate user credentials (login)
        /// </summary>
        public void ValidateUser()
        {
            if (_server == null)
            {
                SetOperationResult(false, false, false, false, "Server not connected");
                return;
            }

            if (string.IsNullOrEmpty(_usernameInput))
            {
                SetOperationResult(false, false, false, false, "Username required");
                return;
            }

            int accessLevel;
            string errorMessage;
            var success = _server.ValidateUser(_usernameInput, _passwordInput, out accessLevel, out errorMessage);

            if (success)
            {
                _validatedUsername = _usernameInput;
                _validatedAccess = accessLevel;
                ValidatedUsernameFeedback.FireUpdate();
                ValidatedUserAccessFeedback.FireUpdate();

                SetOperationResult(true, false, false, false, string.Format("Login successful: {0}", _usernameInput));

                if (_config.ClearInputsOnLogin)
                {
                    ClearInputs();
                }
            }
            else
            {
                SetOperationResult(false, false, false, false, errorMessage);
            }
        }

        /// <summary>
        /// Create a new user
        /// </summary>
        public void CreateUser()
        {
            if (_server == null)
            {
                SetOperationResult(false, false, false, false, "Server not connected");
                return;
            }

            if (string.IsNullOrEmpty(_usernameInput))
            {
                SetOperationResult(false, false, false, false, "Username required");
                return;
            }

            // Get access level from either string or analog input
            int accessLevel = _accessLevelInput;
            if (!string.IsNullOrEmpty(_accessInput) && int.TryParse(_accessInput, out int parsed))
            {
                accessLevel = parsed;
            }

            string errorMessage;
            bool success = _server.CreateUser(_usernameInput, _passwordInput, accessLevel, out errorMessage);

            SetOperationResult(false, success, false, false, success ? $"User '{_usernameInput}' created" : errorMessage);
        }

        /// <summary>
        /// Delete a user
        /// </summary>
        public void DeleteUser()
        {
            if (_server == null)
            {
                SetOperationResult(false, false, false, false, "Server not connected");
                return;
            }

            if (string.IsNullOrEmpty(_usernameInput))
            {
                SetOperationResult(false, false, false, false, "Username required");
                return;
            }

            string errorMessage;
            bool success = _server.DeleteUser(_usernameInput, out errorMessage);

            SetOperationResult(false, false, success, false, success ? $"User '{_usernameInput}' deleted" : errorMessage);
        }

        /// <summary>
        /// Update selected user's password
        /// </summary>
        public void UpdateSelectedUserPassword()
        {
            if (_server == null)
            {
                SetStatusMessage("Server not connected");
                return;
            }

            var username = GetSelectedUsername();
            if (string.IsNullOrEmpty(username))
            {
                SetStatusMessage("No user selected");
                return;
            }

            string errorMessage;
            bool success = _server.UpdateUserPassword(username, _passwordInput, out errorMessage);
            SetStatusMessage(success ? $"Password updated for '{username}'" : errorMessage);
        }

        /// <summary>
        /// Update selected user's access level
        /// </summary>
        public void UpdateSelectedUserAccess()
        {
            if (_server == null)
            {
                SetStatusMessage("Server not connected");
                return;
            }

            var username = GetSelectedUsername();
            if (string.IsNullOrEmpty(username))
            {
                SetStatusMessage("No user selected");
                return;
            }

            // Get access level from either string or analog input
            int accessLevel = _accessLevelInput;
            if (!string.IsNullOrEmpty(_accessInput) && int.TryParse(_accessInput, out int parsed))
            {
                accessLevel = parsed;
            }

            string errorMessage;
            bool success = _server.UpdateUserAccess(username, accessLevel, out errorMessage);
            SetStatusMessage(success ? $"Access updated for '{username}'" : errorMessage);
        }

        /// <summary>
        /// Save changes to selected user (username and/or password)
        /// </summary>
        public void UpdateSelectedUser()
        {
            if (_server == null)
            {
                SetOperationResult(false, false, false, false, "Server not connected");
                return;
            }

            if (string.IsNullOrEmpty(_editingUsername))
            {
                SetOperationResult(false, false, false, false, "No user loaded for editing");
                return;
            }

            if (string.IsNullOrEmpty(_usernameInput))
            {
                SetOperationResult(false, false, false, false, "Username cannot be empty");
                return;
            }

            string errorMessage;
            bool success = true;

            // Update username if changed
            if (!_usernameInput.Equals(_editingUsername, StringComparison.Ordinal))
            {
                success = _server.UpdateUsername(_editingUsername, _usernameInput, out errorMessage);
                if (!success)
                {
                    SetOperationResult(false, false, false, false, errorMessage);
                    return;
                }
            }

            // Update password if changed
            if (!_passwordInput.Equals(_editingOriginalPassword, StringComparison.Ordinal))
            {
                // Use the new username if it was changed
                var usernameToUpdate = _usernameInput;
                success = _server.UpdateUserPassword(usernameToUpdate, _passwordInput, out errorMessage);
                if (!success)
                {
                    SetOperationResult(false, false, false, false, errorMessage);
                    return;
                }
            }

            // Update tracking values to reflect saved state
            _editingUsername = _usernameInput;
            _editingOriginalPassword = _passwordInput;
            EditingUsernameFeedback.FireUpdate();
            UpdateEditModeFeedbacks();

            SetOperationResult(false, false, false, true, $"User '{_usernameInput}' updated");
        }

        /// <summary>
        /// Delete the currently selected user
        /// </summary>
        public void DeleteSelectedUser()
        {
            if (_server == null)
            {
                SetOperationResult(false, false, false, false, "Server not connected");
                return;
            }

            var username = GetSelectedUsername();
            if (string.IsNullOrEmpty(username))
            {
                SetOperationResult(false, false, false, false, "No user selected");
                return;
            }

            string errorMessage;
            bool success = _server.DeleteUser(username, out errorMessage);

            if (success)
            {
                ClearInputs();
            }

            SetOperationResult(false, false, success, false, success ? $"User '{username}' deleted" : errorMessage);
        }

        /// <summary>
        /// Select a user by list position (1-based index for UI)
        /// </summary>
        public void SelectUserByListPosition(int position)
        {
            int index = position - 1;  // Convert 1-based to 0-based
            if (index >= 0 && index < _cachedUsers.Count)
            {
                SetSelectedUserIndex(index);
            }
        }

        #endregion

        #region User Selection

        /// <summary>
        /// Set selected user index
        /// </summary>
        public void SetSelectedUserIndex(int index)
        {
            if (index < 0 || index >= _cachedUsers.Count)
            {
                this.LogVerbose("Invalid user index: {0}", index);
                return;
            }

            _selectedUserIndex = index;
            UpdateSelectedUserFeedbacks();
        }

        /// <summary>
        /// Select next user in list
        /// </summary>
        public void SelectNextUser()
        {
            if (_cachedUsers.Count == 0) return;

            _selectedUserIndex = (_selectedUserIndex + 1) % _cachedUsers.Count;
            UpdateSelectedUserFeedbacks();
        }

        /// <summary>
        /// Select previous user in list
        /// </summary>
        public void SelectPreviousUser()
        {
            if (_cachedUsers.Count == 0) return;

            _selectedUserIndex = _selectedUserIndex > 0 ? _selectedUserIndex - 1 : _cachedUsers.Count - 1;
            UpdateSelectedUserFeedbacks();
        }

        private void UpdateSelectedUserFeedbacks()
        {
            SelectedUserIndexFeedback.FireUpdate();
            SelectedUsernameFeedback.FireUpdate();
            SelectedPasswordFeedback.FireUpdate();
            SelectedAccessFeedback.FireUpdate();
            SelectedUserAccessFeedback.FireUpdate();
        }

        private string GetSelectedUsername()
        {
            if (_selectedUserIndex < 0 || _selectedUserIndex >= _cachedUsers.Count)
                return string.Empty;
            return _cachedUsers[_selectedUserIndex].Username;
        }

        private string GetSelectedPassword()
        {
            if (_selectedUserIndex < 0 || _selectedUserIndex >= _cachedUsers.Count)
                return string.Empty;

            var password = _cachedUsers[_selectedUserIndex].Password;
            return _config.MaskPasswords ? new string('*', password.Length) : password;
        }

        private string GetSelectedAccess()
        {
            if (_selectedUserIndex < 0 || _selectedUserIndex >= _cachedUsers.Count)
                return string.Empty;
            return _cachedUsers[_selectedUserIndex].Access.ToString();
        }

        private int GetSelectedUserAccess()
        {
            if (_selectedUserIndex < 0 || _selectedUserIndex >= _cachedUsers.Count)
                return 0;
            return _cachedUsers[_selectedUserIndex].Access;
        }

        #endregion

        #region Helpers

        private void SetOperationResult(bool validate, bool create, bool delete, bool update, string message)
        {
            // Stop any pending reset timer
            if (_successResetTimer != null)
            {
                _successResetTimer.Stop();
                _successResetTimer.Dispose();
                _successResetTimer = null;
            }

            _validateSuccess = validate;
            _createSuccess = create;
            _deleteSuccess = delete;
            _updateSuccess = update;
            _statusMessage = message;

            ValidateUserSuccessFeedback.FireUpdate();
            CreateUserSuccessFeedback.FireUpdate();
            DeleteUserSuccessFeedback.FireUpdate();
            UpdateUserSuccessFeedback.FireUpdate();
            StatusMessageFeedback.FireUpdate();

            this.LogError(message);

            // If any success flag is true, start timer to reset (creates pulse)
            if (validate || create || delete || update)
            {
                _successResetTimer = new CTimer(_ =>
                {
                    _validateSuccess = false;
                    _createSuccess = false;
                    _deleteSuccess = false;
                    _updateSuccess = false;

                    ValidateUserSuccessFeedback.FireUpdate();
                    CreateUserSuccessFeedback.FireUpdate();
                    DeleteUserSuccessFeedback.FireUpdate();
                    UpdateUserSuccessFeedback.FireUpdate();
                }, SuccessPulseDurationMs);
            }
        }

        private void SetStatusMessage(string message)
        {
            _statusMessage = message;
            StatusMessageFeedback.FireUpdate();
            this.LogError(message);
        }

        private string GetUserListJson()
        {
            try
            {
                var userList = new List<object>();
                foreach (var user in _cachedUsers)
                {
                    userList.Add(new
                    {
                        username = user.Username,
                        password = _config.MaskPasswords ? new string('*', user.Password.Length) : user.Password,
                        access = user.Access
                    });
                }
                return JsonConvert.SerializeObject(userList);
            }
            catch (Exception ex)
            {
                this.LogInformation("serializing user list: {0}", ex.Message);
                return "[]";
            }
        }

        private void UpdateAllFeedbacks()
        {
            UserCountFeedback.FireUpdate();
            UserListFeedback.FireUpdate();
            UpdateSelectedUserFeedbacks();
            UpdateUserListFeedbacks();
        }

        #endregion

        #region Bridge Linking

        /// <summary>
        /// Link to API
        /// </summary>
        public override void LinkToApi(BasicTriList triList, uint joinStart, string joinMapKey, EiscApiAdvanced bridge)
        {
            _triList = triList;
            _joinMap = new PasswordClientBridgeJoinMap(joinStart);

            if (bridge != null)
            {
                bridge.AddJoinMap(Key, _joinMap);
            }

            var joinMapSerialized = JoinMapHelper.TryGetJoinMapAdvancedForDevice(joinMapKey);
            if (joinMapSerialized != null)
            {
                _joinMap.SetCustomJoinData(joinMapSerialized);
            }

            this.LogError("Linking to EISC bridge at join {0}", joinStart);

            // Digital inputs from SIMPL
            triList.SetSigTrueAction(_joinMap.ValidateUser.JoinNumber, ValidateUser);
            triList.SetSigTrueAction(_joinMap.CreateUser.JoinNumber, CreateUser);
            triList.SetSigTrueAction(_joinMap.DeleteUser.JoinNumber, DeleteUser);
            triList.SetSigTrueAction(_joinMap.ClearInputs.JoinNumber, ClearInputs);
            triList.SetSigTrueAction(_joinMap.RefreshUsers.JoinNumber, RefreshUsersFromServer);
            triList.SetSigTrueAction(_joinMap.SelectNextUser.JoinNumber, SelectNextUser);
            triList.SetSigTrueAction(_joinMap.SelectPreviousUser.JoinNumber, SelectPreviousUser);
            triList.SetSigTrueAction(_joinMap.UpdatePassword.JoinNumber, UpdateSelectedUserPassword);
            triList.SetSigTrueAction(_joinMap.UpdateAccess.JoinNumber, UpdateSelectedUserAccess);
            triList.SetSigTrueAction(_joinMap.LoadSelectedUserToInputs.JoinNumber, LoadSelectedUserToInputs);
            triList.SetSigTrueAction(_joinMap.UpdateSelectedUser.JoinNumber, UpdateSelectedUser);
            triList.SetSigTrueAction(_joinMap.DeleteSelectedUser.JoinNumber, DeleteSelectedUser);

            // User list select buttons (1-20)
            for (uint i = 0; i < MaxUserListItems; i++)
            {
                var position = (int)(i + 1);  // 1-based position
                triList.SetSigTrueAction(_joinMap.UserListSelect.JoinNumber + i, () => SelectUserByListPosition(position));
            }

            // Unmask password (hold high)
            triList.SetBoolSigAction(_joinMap.UnmaskPasswordInput.JoinNumber, SetUnmaskPassword);

            // Analog inputs from SIMPL
            triList.SetUShortSigAction(_joinMap.SelectedUserIndex.JoinNumber, value => SetSelectedUserIndex(value));
            triList.SetUShortSigAction(_joinMap.AccessLevelInput.JoinNumber, value => SetAccessLevelInput(value));

            // Serial inputs from SIMPL
            triList.SetStringSigAction(_joinMap.UsernameInput.JoinNumber, SetUsernameInput);
            triList.SetStringSigAction(_joinMap.PasswordInput.JoinNumber, SetPasswordInput);
            triList.SetStringSigAction(_joinMap.AccessInput.JoinNumber, SetAccessInput);

            // Digital outputs to SIMPL
            ValidateUserSuccessFeedback.LinkInputSig(triList.BooleanInput[_joinMap.ValidateUserSuccessFb.JoinNumber]);
            CreateUserSuccessFeedback.LinkInputSig(triList.BooleanInput[_joinMap.CreateUserSuccessFb.JoinNumber]);
            DeleteUserSuccessFeedback.LinkInputSig(triList.BooleanInput[_joinMap.DeleteUserSuccessFb.JoinNumber]);
            ServerConnectedFeedback.LinkInputSig(triList.BooleanInput[_joinMap.ServerConnectedFb.JoinNumber]);
            SaveEnabledFeedback.LinkInputSig(triList.BooleanInput[_joinMap.SaveEnabledFb.JoinNumber]);
            HasChangesFeedback.LinkInputSig(triList.BooleanInput[_joinMap.HasChangesFb.JoinNumber]);
            UpdateUserSuccessFeedback.LinkInputSig(triList.BooleanInput[_joinMap.UpdateUserSuccessFb.JoinNumber]);

            // Analog outputs to SIMPL
            UserCountFeedback.LinkInputSig(triList.UShortInput[_joinMap.UserCountFb.JoinNumber]);
            SelectedUserIndexFeedback.LinkInputSig(triList.UShortInput[_joinMap.SelectedUserIndex.JoinNumber]);
            SelectedUserAccessFeedback.LinkInputSig(triList.UShortInput[_joinMap.SelectedUserAccessFb.JoinNumber]);
            ValidatedUserAccessFeedback.LinkInputSig(triList.UShortInput[_joinMap.ValidatedUserAccessFb.JoinNumber]);

            // Serial outputs to SIMPL
            triList.SetString(_joinMap.DeviceName.JoinNumber, Name);
            UsernameInputFeedback.LinkInputSig(triList.StringInput[_joinMap.UsernameInputFb.JoinNumber]);
            PasswordInputFeedback.LinkInputSig(triList.StringInput[_joinMap.PasswordInputFb.JoinNumber]);
            AccessInputFeedback.LinkInputSig(triList.StringInput[_joinMap.AccessInputFb.JoinNumber]);
            UserListFeedback.LinkInputSig(triList.StringInput[_joinMap.UserListFb.JoinNumber]);
            StatusMessageFeedback.LinkInputSig(triList.StringInput[_joinMap.StatusMessageFb.JoinNumber]);
            SelectedUsernameFeedback.LinkInputSig(triList.StringInput[_joinMap.SelectedUsernameFb.JoinNumber]);
            SelectedPasswordFeedback.LinkInputSig(triList.StringInput[_joinMap.SelectedPasswordFb.JoinNumber]);
            SelectedAccessFeedback.LinkInputSig(triList.StringInput[_joinMap.SelectedAccessFb.JoinNumber]);
            ValidatedUsernameFeedback.LinkInputSig(triList.StringInput[_joinMap.ValidatedUsernameFb.JoinNumber]);
            EditingUsernameFeedback.LinkInputSig(triList.StringInput[_joinMap.EditingUsernameFb.JoinNumber]);

            // Online status
            triList.OnlineStatusChange += (sender, args) =>
            {
                if (!args.DeviceOnLine) return;

                triList.SetString(_joinMap.DeviceName.JoinNumber, Name);
                UpdateAllFeedbacks();
                UpdateUserListFeedbacks();
                UsernameInputFeedback.FireUpdate();
                PasswordInputFeedback.FireUpdate();
                AccessInputFeedback.FireUpdate();
                StatusMessageFeedback.FireUpdate();
                ServerConnectedFeedback.FireUpdate();
                ValidatedUsernameFeedback.FireUpdate();
                ValidatedUserAccessFeedback.FireUpdate();
                EditingUsernameFeedback.FireUpdate();
                SaveEnabledFeedback.FireUpdate();
                HasChangesFeedback.FireUpdate();
            };
        }

        /// <summary>
        /// Update user list item feedbacks
        /// </summary>
        private void UpdateUserListFeedbacks()
        {
            if (_triList == null || _joinMap == null) return;

            for (int i = 0; i < MaxUserListItems; i++)
            {
                uint joinOffset = (uint)i;
                bool hasUser = i < _cachedUsers.Count;

                // Set username string
                var username = hasUser ? _cachedUsers[i].Username : string.Empty;
                _triList.SetString(_joinMap.UserListItemFb.JoinNumber + joinOffset, username);

                // Set visibility
                _triList.BooleanInput[_joinMap.UserListVisibleFb.JoinNumber + joinOffset].BoolValue = hasUser;
            }
        }

        #endregion
    }
}
