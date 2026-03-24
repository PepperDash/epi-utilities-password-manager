using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.Plugin.Password.Client
{
    /// <summary>
    /// Password Client Bridge Join Map
    /// </summary>
    /// <remarks>
    /// Defines the EISC bridge joins for password client operations.
    /// Multiple clients can be bridged to panels to interact with the server as the authentication authority.
    /// 
    /// Join Layout:
    /// 
    /// DIGITAL:
    ///   Feedbacks (1-7)    - Server status and success feedbacks
    ///   Actions (11-20)    - User actions (clear, unmask, validate, create, delete, update)
    ///   Navigation (25-27) - Refresh and list navigation
    ///   User List (31-80)  - List select (31-50) and visibility (61-80)
    /// 
    /// ANALOG:
    ///   Core (1-5)         - Counts, indexes, access levels
    /// 
    /// SERIAL:
    ///   Device/Status (1-6)  - Device name, status, validated user, input feedbacks
    ///   Inputs (7-9)         - User input fields
    ///   List/Edit (11-15)    - User list JSON, editing username, selected user info
    ///   User List (31-50)    - List item usernames
    /// </remarks>
    public class PasswordClientBridgeJoinMap : JoinMapBaseAdvanced
    {
        #region Digital - Feedbacks (1-7)

        /// <summary>
        /// Feedback indicating server is connected
        /// </summary>
        [JoinName("ServerConnectedFb")]
        public JoinDataComplete ServerConnectedFb = new JoinDataComplete(
            new JoinData { JoinNumber = 1, JoinSpan = 1 },
            new JoinMetadata
            {
                Description = "Server Connected Feedback",
                JoinCapabilities = eJoinCapabilities.ToSIMPL,
                JoinType = eJoinType.Digital
            });

        /// <summary>
        /// Feedback indicating validation/login was successful
        /// </summary>
        [JoinName("ValidateUserSuccessFb")]
        public JoinDataComplete ValidateUserSuccessFb = new JoinDataComplete(
            new JoinData { JoinNumber = 2, JoinSpan = 1 },
            new JoinMetadata
            {
                Description = "Validate/Login Success Feedback",
                JoinCapabilities = eJoinCapabilities.ToSIMPL,
                JoinType = eJoinType.Digital
            });

        /// <summary>
        /// Feedback indicating create user was successful
        /// </summary>
        [JoinName("CreateUserSuccessFb")]
        public JoinDataComplete CreateUserSuccessFb = new JoinDataComplete(
            new JoinData { JoinNumber = 3, JoinSpan = 1 },
            new JoinMetadata
            {
                Description = "Create User Success Feedback",
                JoinCapabilities = eJoinCapabilities.ToSIMPL,
                JoinType = eJoinType.Digital
            });

        /// <summary>
        /// Feedback indicating delete user was successful
        /// </summary>
        [JoinName("DeleteUserSuccessFb")]
        public JoinDataComplete DeleteUserSuccessFb = new JoinDataComplete(
            new JoinData { JoinNumber = 4, JoinSpan = 1 },
            new JoinMetadata
            {
                Description = "Delete User Success Feedback",
                JoinCapabilities = eJoinCapabilities.ToSIMPL,
                JoinType = eJoinType.Digital
            });

        /// <summary>
        /// Feedback indicating update user was successful
        /// </summary>
        [JoinName("UpdateUserSuccessFb")]
        public JoinDataComplete UpdateUserSuccessFb = new JoinDataComplete(
            new JoinData { JoinNumber = 5, JoinSpan = 1 },
            new JoinMetadata
            {
                Description = "Update User Success Feedback",
                JoinCapabilities = eJoinCapabilities.ToSIMPL,
                JoinType = eJoinType.Digital
            });

        /// <summary>
        /// Feedback indicating save button should be enabled
        /// </summary>
        [JoinName("SaveEnabledFb")]
        public JoinDataComplete SaveEnabledFb = new JoinDataComplete(
            new JoinData { JoinNumber = 6, JoinSpan = 1 },
            new JoinMetadata
            {
                Description = "Save Button Enabled Feedback",
                JoinCapabilities = eJoinCapabilities.ToSIMPL,
                JoinType = eJoinType.Digital
            });

        /// <summary>
        /// Feedback indicating changes have been made to inputs
        /// </summary>
        [JoinName("HasChangesFb")]
        public JoinDataComplete HasChangesFb = new JoinDataComplete(
            new JoinData { JoinNumber = 7, JoinSpan = 1 },
            new JoinMetadata
            {
                Description = "Has Unsaved Changes Feedback",
                JoinCapabilities = eJoinCapabilities.ToSIMPL,
                JoinType = eJoinType.Digital
            });

        #endregion

        #region Digital - Actions (11-20)

        /// <summary>
        /// Trigger to clear all input fields
        /// </summary>
        [JoinName("ClearInputs")]
        public JoinDataComplete ClearInputs = new JoinDataComplete(
            new JoinData { JoinNumber = 11, JoinSpan = 1 },
            new JoinMetadata
            {
                Description = "Clear All Inputs (Pulse)",
                JoinCapabilities = eJoinCapabilities.FromSIMPL,
                JoinType = eJoinType.Digital
            });

        /// <summary>
        /// When high, shows unmasked password in PasswordInputFb
        /// </summary>
        [JoinName("UnmaskPasswordInput")]
        public JoinDataComplete UnmaskPasswordInput = new JoinDataComplete(
            new JoinData { JoinNumber = 12, JoinSpan = 1 },
            new JoinMetadata
            {
                Description = "Unmask Password (Hold high)",
                JoinCapabilities = eJoinCapabilities.FromSIMPL,
                JoinType = eJoinType.Digital
            });

        /// <summary>
        /// Trigger to validate user credentials (login)
        /// </summary>
        [JoinName("ValidateUser")]
        public JoinDataComplete ValidateUser = new JoinDataComplete(
            new JoinData { JoinNumber = 13, JoinSpan = 1 },
            new JoinMetadata
            {
                Description = "Validate/Login (Pulse)",
                JoinCapabilities = eJoinCapabilities.FromSIMPL,
                JoinType = eJoinType.Digital
            });

        /// <summary>
        /// Trigger to create a new user
        /// </summary>
        [JoinName("CreateUser")]
        public JoinDataComplete CreateUser = new JoinDataComplete(
            new JoinData { JoinNumber = 14, JoinSpan = 1 },
            new JoinMetadata
            {
                Description = "Create User (Pulse)",
                JoinCapabilities = eJoinCapabilities.FromSIMPL,
                JoinType = eJoinType.Digital
            });

        /// <summary>
        /// Trigger to delete a user by username input
        /// </summary>
        [JoinName("DeleteUser")]
        public JoinDataComplete DeleteUser = new JoinDataComplete(
            new JoinData { JoinNumber = 15, JoinSpan = 1 },
            new JoinMetadata
            {
                Description = "Delete User By Input (Pulse)",
                JoinCapabilities = eJoinCapabilities.FromSIMPL,
                JoinType = eJoinType.Digital
            });

        /// <summary>
        /// Delete the currently selected user
        /// </summary>
        [JoinName("DeleteSelectedUser")]
        public JoinDataComplete DeleteSelectedUser = new JoinDataComplete(
            new JoinData { JoinNumber = 16, JoinSpan = 1 },
            new JoinMetadata
            {
                Description = "Delete Selected User (Pulse)",
                JoinCapabilities = eJoinCapabilities.FromSIMPL,
                JoinType = eJoinType.Digital
            });

        /// <summary>
        /// Update selected user's password only
        /// </summary>
        [JoinName("UpdatePassword")]
        public JoinDataComplete UpdatePassword = new JoinDataComplete(
            new JoinData { JoinNumber = 17, JoinSpan = 1 },
            new JoinMetadata
            {
                Description = "Update Selected Password (Pulse)",
                JoinCapabilities = eJoinCapabilities.FromSIMPL,
                JoinType = eJoinType.Digital
            });

        /// <summary>
        /// Update selected user's access level only
        /// </summary>
        [JoinName("UpdateAccess")]
        public JoinDataComplete UpdateAccess = new JoinDataComplete(
            new JoinData { JoinNumber = 18, JoinSpan = 1 },
            new JoinMetadata
            {
                Description = "Update Selected Access (Pulse)",
                JoinCapabilities = eJoinCapabilities.FromSIMPL,
                JoinType = eJoinType.Digital
            });

        /// <summary>
        /// Load selected user's data into input fields for editing
        /// </summary>
        [JoinName("LoadSelectedUserToInputs")]
        public JoinDataComplete LoadSelectedUserToInputs = new JoinDataComplete(
            new JoinData { JoinNumber = 19, JoinSpan = 1 },
            new JoinMetadata
            {
                Description = "Load Selected User To Inputs (Pulse)",
                JoinCapabilities = eJoinCapabilities.FromSIMPL,
                JoinType = eJoinType.Digital
            });

        /// <summary>
        /// Save current inputs to selected user (username + password)
        /// </summary>
        [JoinName("UpdateSelectedUser")]
        public JoinDataComplete UpdateSelectedUser = new JoinDataComplete(
            new JoinData { JoinNumber = 20, JoinSpan = 1 },
            new JoinMetadata
            {
                Description = "Save Changes To Selected User (Pulse)",
                JoinCapabilities = eJoinCapabilities.FromSIMPL,
                JoinType = eJoinType.Digital
            });

        #endregion

        #region Digital - Navigation (25-27)

        /// <summary>
        /// Trigger to refresh users from server
        /// </summary>
        [JoinName("RefreshUsers")]
        public JoinDataComplete RefreshUsers = new JoinDataComplete(
            new JoinData { JoinNumber = 25, JoinSpan = 1 },
            new JoinMetadata
            {
                Description = "Refresh Users (Pulse)",
                JoinCapabilities = eJoinCapabilities.FromSIMPL,
                JoinType = eJoinType.Digital
            });

        /// <summary>
        /// Select next user in list
        /// </summary>
        [JoinName("SelectNextUser")]
        public JoinDataComplete SelectNextUser = new JoinDataComplete(
            new JoinData { JoinNumber = 26, JoinSpan = 1 },
            new JoinMetadata
            {
                Description = "Select Next User",
                JoinCapabilities = eJoinCapabilities.FromSIMPL,
                JoinType = eJoinType.Digital
            });

        /// <summary>
        /// Select previous user in list
        /// </summary>
        [JoinName("SelectPreviousUser")]
        public JoinDataComplete SelectPreviousUser = new JoinDataComplete(
            new JoinData { JoinNumber = 27, JoinSpan = 1 },
            new JoinMetadata
            {
                Description = "Select Previous User",
                JoinCapabilities = eJoinCapabilities.FromSIMPL,
                JoinType = eJoinType.Digital
            });

        #endregion

        #region Digital - User List (31-80)

        /// <summary>
        /// User list item select buttons (1-20)
        /// </summary>
        [JoinName("UserListSelect")]
        public JoinDataComplete UserListSelect = new JoinDataComplete(
            new JoinData { JoinNumber = 31, JoinSpan = 20 },
            new JoinMetadata
            {
                Description = "User List Select [1-20] (Pulse)",
                JoinCapabilities = eJoinCapabilities.FromSIMPL,
                JoinType = eJoinType.Digital
            });

        /// <summary>
        /// User list item visible feedback (1-20)
        /// </summary>
        [JoinName("UserListVisibleFb")]
        public JoinDataComplete UserListVisibleFb = new JoinDataComplete(
            new JoinData { JoinNumber = 61, JoinSpan = 20 },
            new JoinMetadata
            {
                Description = "User List Item Visible [1-20]",
                JoinCapabilities = eJoinCapabilities.ToSIMPL,
                JoinType = eJoinType.Digital
            });

        #endregion

        #region Analog (1-5)

        /// <summary>
        /// Total number of users
        /// </summary>
        [JoinName("UserCountFb")]
        public JoinDataComplete UserCountFb = new JoinDataComplete(
            new JoinData { JoinNumber = 1, JoinSpan = 1 },
            new JoinMetadata
            {
                Description = "User Count Feedback",
                JoinCapabilities = eJoinCapabilities.ToSIMPL,
                JoinType = eJoinType.Analog
            });

        /// <summary>
        /// Currently selected user index (0-based)
        /// </summary>
        [JoinName("SelectedUserIndex")]
        public JoinDataComplete SelectedUserIndex = new JoinDataComplete(
            new JoinData { JoinNumber = 2, JoinSpan = 1 },
            new JoinMetadata
            {
                Description = "Selected User Index (set/feedback)",
                JoinCapabilities = eJoinCapabilities.ToFromSIMPL,
                JoinType = eJoinType.Analog
            });

        /// <summary>
        /// Selected user's access level
        /// </summary>
        [JoinName("SelectedUserAccessFb")]
        public JoinDataComplete SelectedUserAccessFb = new JoinDataComplete(
            new JoinData { JoinNumber = 3, JoinSpan = 1 },
            new JoinMetadata
            {
                Description = "Selected User Access Feedback",
                JoinCapabilities = eJoinCapabilities.ToSIMPL,
                JoinType = eJoinType.Analog
            });

        /// <summary>
        /// Validated user's access level
        /// </summary>
        [JoinName("ValidatedUserAccessFb")]
        public JoinDataComplete ValidatedUserAccessFb = new JoinDataComplete(
            new JoinData { JoinNumber = 4, JoinSpan = 1 },
            new JoinMetadata
            {
                Description = "Validated User Access Feedback",
                JoinCapabilities = eJoinCapabilities.ToSIMPL,
                JoinType = eJoinType.Analog
            });

        /// <summary>
        /// Access level input (numeric)
        /// </summary>
        [JoinName("AccessLevelInput")]
        public JoinDataComplete AccessLevelInput = new JoinDataComplete(
            new JoinData { JoinNumber = 5, JoinSpan = 1 },
            new JoinMetadata
            {
                Description = "Access Level Input (analog)",
                JoinCapabilities = eJoinCapabilities.FromSIMPL,
                JoinType = eJoinType.Analog
            });

        #endregion

        #region Serial - Device/Status (1-6)

        /// <summary>
        /// Device name
        /// </summary>
        [JoinName("DeviceName")]
        public JoinDataComplete DeviceName = new JoinDataComplete(
            new JoinData { JoinNumber = 1, JoinSpan = 1 },
            new JoinMetadata
            {
                Description = "Device Name",
                JoinCapabilities = eJoinCapabilities.ToSIMPL,
                JoinType = eJoinType.Serial
            });

        /// <summary>
        /// Status message
        /// </summary>
        [JoinName("StatusMessageFb")]
        public JoinDataComplete StatusMessageFb = new JoinDataComplete(
            new JoinData { JoinNumber = 2, JoinSpan = 1 },
            new JoinMetadata
            {
                Description = "Status Message Feedback",
                JoinCapabilities = eJoinCapabilities.ToSIMPL,
                JoinType = eJoinType.Serial
            });

        /// <summary>
        /// Validated username feedback
        /// </summary>
        [JoinName("ValidatedUsernameFb")]
        public JoinDataComplete ValidatedUsernameFb = new JoinDataComplete(
            new JoinData { JoinNumber = 3, JoinSpan = 1 },
            new JoinMetadata
            {
                Description = "Validated Username Feedback",
                JoinCapabilities = eJoinCapabilities.ToSIMPL,
                JoinType = eJoinType.Serial
            });

        /// <summary>
        /// Username input feedback - shows current username input value
        /// </summary>
        [JoinName("UsernameInputFb")]
        public JoinDataComplete UsernameInputFb = new JoinDataComplete(
            new JoinData { JoinNumber = 4, JoinSpan = 1 },
            new JoinMetadata
            {
                Description = "Username Input Feedback",
                JoinCapabilities = eJoinCapabilities.ToSIMPL,
                JoinType = eJoinType.Serial
            });

        /// <summary>
        /// Password input feedback - shows masked/unmasked password input
        /// </summary>
        [JoinName("PasswordInputFb")]
        public JoinDataComplete PasswordInputFb = new JoinDataComplete(
            new JoinData { JoinNumber = 5, JoinSpan = 1 },
            new JoinMetadata
            {
                Description = "Password Input Feedback (masked/unmasked)",
                JoinCapabilities = eJoinCapabilities.ToSIMPL,
                JoinType = eJoinType.Serial
            });

        /// <summary>
        /// Access level input feedback - shows current access input value
        /// </summary>
        [JoinName("AccessInputFb")]
        public JoinDataComplete AccessInputFb = new JoinDataComplete(
            new JoinData { JoinNumber = 6, JoinSpan = 1 },
            new JoinMetadata
            {
                Description = "Access Level Input Feedback",
                JoinCapabilities = eJoinCapabilities.ToSIMPL,
                JoinType = eJoinType.Serial
            });

        #endregion

        #region Serial - Inputs (7-9)

        /// <summary>
        /// Username input
        /// </summary>
        [JoinName("UsernameInput")]
        public JoinDataComplete UsernameInput = new JoinDataComplete(
            new JoinData { JoinNumber = 7, JoinSpan = 1 },
            new JoinMetadata
            {
                Description = "Username Input",
                JoinCapabilities = eJoinCapabilities.FromSIMPL,
                JoinType = eJoinType.Serial
            });

        /// <summary>
        /// Password input
        /// </summary>
        [JoinName("PasswordInput")]
        public JoinDataComplete PasswordInput = new JoinDataComplete(
            new JoinData { JoinNumber = 8, JoinSpan = 1 },
            new JoinMetadata
            {
                Description = "Password Input",
                JoinCapabilities = eJoinCapabilities.FromSIMPL,
                JoinType = eJoinType.Serial
            });

        /// <summary>
        /// Access level input (string)
        /// </summary>
        [JoinName("AccessInput")]
        public JoinDataComplete AccessInput = new JoinDataComplete(
            new JoinData { JoinNumber = 9, JoinSpan = 1 },
            new JoinMetadata
            {
                Description = "Access Level Input (string)",
                JoinCapabilities = eJoinCapabilities.FromSIMPL,
                JoinType = eJoinType.Serial
            });

        #endregion

        #region Serial - List/Edit (11-15)

        /// <summary>
        /// User list JSON
        /// </summary>
        [JoinName("UserListFb")]
        public JoinDataComplete UserListFb = new JoinDataComplete(
            new JoinData { JoinNumber = 11, JoinSpan = 1 },
            new JoinMetadata
            {
                Description = "User List JSON Feedback",
                JoinCapabilities = eJoinCapabilities.ToSIMPL,
                JoinType = eJoinType.Serial
            });

        /// <summary>
        /// Original username being edited (for tracking changes)
        /// </summary>
        [JoinName("EditingUsernameFb")]
        public JoinDataComplete EditingUsernameFb = new JoinDataComplete(
            new JoinData { JoinNumber = 12, JoinSpan = 1 },
            new JoinMetadata
            {
                Description = "Original Username Being Edited",
                JoinCapabilities = eJoinCapabilities.ToSIMPL,
                JoinType = eJoinType.Serial
            });

        /// <summary>
        /// Selected user's username
        /// </summary>
        [JoinName("SelectedUsernameFb")]
        public JoinDataComplete SelectedUsernameFb = new JoinDataComplete(
            new JoinData { JoinNumber = 13, JoinSpan = 1 },
            new JoinMetadata
            {
                Description = "Selected Username Feedback",
                JoinCapabilities = eJoinCapabilities.ToSIMPL,
                JoinType = eJoinType.Serial
            });

        /// <summary>
        /// Selected user's password (masked)
        /// </summary>
        [JoinName("SelectedPasswordFb")]
        public JoinDataComplete SelectedPasswordFb = new JoinDataComplete(
            new JoinData { JoinNumber = 14, JoinSpan = 1 },
            new JoinMetadata
            {
                Description = "Selected Password Feedback",
                JoinCapabilities = eJoinCapabilities.ToSIMPL,
                JoinType = eJoinType.Serial
            });

        /// <summary>
        /// Selected user's access level
        /// </summary>
        [JoinName("SelectedAccessFb")]
        public JoinDataComplete SelectedAccessFb = new JoinDataComplete(
            new JoinData { JoinNumber = 15, JoinSpan = 1 },
            new JoinMetadata
            {
                Description = "Selected Access Feedback",
                JoinCapabilities = eJoinCapabilities.ToSIMPL,
                JoinType = eJoinType.Serial
            });

        #endregion

        #region Serial - User List (31-50)

        /// <summary>
        /// User list item usernames (1-20)
        /// </summary>
        [JoinName("UserListItemFb")]
        public JoinDataComplete UserListItemFb = new JoinDataComplete(
            new JoinData { JoinNumber = 31, JoinSpan = 20 },
            new JoinMetadata
            {
                Description = "User List Item Username [1-20]",
                JoinCapabilities = eJoinCapabilities.ToSIMPL,
                JoinType = eJoinType.Serial
            });

        #endregion

        /// <summary>
        /// Password Client BridgeJoinMap constructor
        /// </summary>
        /// <param name="joinStart">This will be the join it starts on the EISC bridge</param>
        public PasswordClientBridgeJoinMap(uint joinStart)
            : base(joinStart, typeof(PasswordClientBridgeJoinMap))
        {
        }
    }
}
