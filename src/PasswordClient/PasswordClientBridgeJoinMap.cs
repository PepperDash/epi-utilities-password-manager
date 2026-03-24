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
    /// Join Layout (input/output pairs share same join for EISC alignment):
    /// 
    /// DIGITAL (1-50):
    ///   1-9   : Status feedbacks (ToSIMPL)
    ///   11-24 : Actions (FromSIMPL)
    ///   31-50 : User list select/visible (FromSIMPL input, ToSIMPL feedback on same joins)
    /// 
    /// ANALOG (1-5):
    ///   1     : User count (ToSIMPL)
    ///   2     : Selected user index (bidirectional)
    ///   3     : Selected user access (ToSIMPL)
    ///   4     : Validated user access (ToSIMPL)
    ///   5     : Access level input (FromSIMPL)
    /// 
    /// SERIAL (1-50):
    ///   1     : Device name (ToSIMPL)
    ///   2     : Status message (ToSIMPL)
    ///   3     : Validated username (ToSIMPL)
    ///   4     : Username input/feedback (bidirectional)
    ///   5     : Password input/feedback (bidirectional)
    ///   6     : Access input/feedback (bidirectional)
    ///   11    : User list JSON (ToSIMPL)
    ///   12    : Editing username (ToSIMPL)
    ///   13    : Selected username (ToSIMPL)
    ///   14    : Selected password (ToSIMPL)
    ///   15    : Selected access (ToSIMPL)
    ///   31-50 : User list item usernames (ToSIMPL)
    /// </remarks>
    public class PasswordClientBridgeJoinMap : JoinMapBaseAdvanced
    {
        #region Digital

        // ===== Status Feedbacks (1-9) =====

        /// <summary>
        /// D1: Server connected feedback
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
        /// D2: User is logged in feedback
        /// </summary>
        [JoinName("IsLoggedInFb")]
        public JoinDataComplete IsLoggedInFb = new JoinDataComplete(
            new JoinData { JoinNumber = 2, JoinSpan = 1 },
            new JoinMetadata
            {
                Description = "User Is Logged In Feedback",
                JoinCapabilities = eJoinCapabilities.ToSIMPL,
                JoinType = eJoinType.Digital
            });

        /// <summary>
        /// D3: User can manage users (has admin access) feedback
        /// </summary>
        [JoinName("CanManageUsersFb")]
        public JoinDataComplete CanManageUsersFb = new JoinDataComplete(
            new JoinData { JoinNumber = 3, JoinSpan = 1 },
            new JoinMetadata
            {
                Description = "Can Manage Users (Admin Access) Feedback",
                JoinCapabilities = eJoinCapabilities.ToSIMPL,
                JoinType = eJoinType.Digital
            });

        /// <summary>
        /// D4: Validate/login success feedback
        /// </summary>
        [JoinName("ValidateUserSuccessFb")]
        public JoinDataComplete ValidateUserSuccessFb = new JoinDataComplete(
            new JoinData { JoinNumber = 4, JoinSpan = 1 },
            new JoinMetadata
            {
                Description = "Validate/Login Success Feedback",
                JoinCapabilities = eJoinCapabilities.ToSIMPL,
                JoinType = eJoinType.Digital
            });

        /// <summary>
        /// D5: Create user success feedback
        /// </summary>
        [JoinName("CreateUserSuccessFb")]
        public JoinDataComplete CreateUserSuccessFb = new JoinDataComplete(
            new JoinData { JoinNumber = 5, JoinSpan = 1 },
            new JoinMetadata
            {
                Description = "Create User Success Feedback",
                JoinCapabilities = eJoinCapabilities.ToSIMPL,
                JoinType = eJoinType.Digital
            });

        /// <summary>
        /// D6: Delete user success feedback
        /// </summary>
        [JoinName("DeleteUserSuccessFb")]
        public JoinDataComplete DeleteUserSuccessFb = new JoinDataComplete(
            new JoinData { JoinNumber = 6, JoinSpan = 1 },
            new JoinMetadata
            {
                Description = "Delete User Success Feedback",
                JoinCapabilities = eJoinCapabilities.ToSIMPL,
                JoinType = eJoinType.Digital
            });

        /// <summary>
        /// D7: Update user success feedback
        /// </summary>
        [JoinName("UpdateUserSuccessFb")]
        public JoinDataComplete UpdateUserSuccessFb = new JoinDataComplete(
            new JoinData { JoinNumber = 7, JoinSpan = 1 },
            new JoinMetadata
            {
                Description = "Update User Success Feedback",
                JoinCapabilities = eJoinCapabilities.ToSIMPL,
                JoinType = eJoinType.Digital
            });

        /// <summary>
        /// D8: Save button enabled feedback
        /// </summary>
        [JoinName("SaveEnabledFb")]
        public JoinDataComplete SaveEnabledFb = new JoinDataComplete(
            new JoinData { JoinNumber = 8, JoinSpan = 1 },
            new JoinMetadata
            {
                Description = "Save Button Enabled Feedback",
                JoinCapabilities = eJoinCapabilities.ToSIMPL,
                JoinType = eJoinType.Digital
            });

        /// <summary>
        /// D9: Has unsaved changes feedback
        /// </summary>
        [JoinName("HasChangesFb")]
        public JoinDataComplete HasChangesFb = new JoinDataComplete(
            new JoinData { JoinNumber = 9, JoinSpan = 1 },
            new JoinMetadata
            {
                Description = "Has Unsaved Changes Feedback",
                JoinCapabilities = eJoinCapabilities.ToSIMPL,
                JoinType = eJoinType.Digital
            });

        // ===== Actions (11-24) =====

        /// <summary>
        /// D11: Validate/Login trigger
        /// </summary>
        [JoinName("ValidateUser")]
        public JoinDataComplete ValidateUser = new JoinDataComplete(
            new JoinData { JoinNumber = 11, JoinSpan = 1 },
            new JoinMetadata
            {
                Description = "Validate/Login (Pulse)",
                JoinCapabilities = eJoinCapabilities.FromSIMPL,
                JoinType = eJoinType.Digital
            });

        /// <summary>
        /// D12: Logout trigger
        /// </summary>
        [JoinName("Logout")]
        public JoinDataComplete Logout = new JoinDataComplete(
            new JoinData { JoinNumber = 12, JoinSpan = 1 },
            new JoinMetadata
            {
                Description = "Logout (Pulse)",
                JoinCapabilities = eJoinCapabilities.FromSIMPL,
                JoinType = eJoinType.Digital
            });

        /// <summary>
        /// D13: Create user trigger
        /// </summary>
        [JoinName("CreateUser")]
        public JoinDataComplete CreateUser = new JoinDataComplete(
            new JoinData { JoinNumber = 13, JoinSpan = 1 },
            new JoinMetadata
            {
                Description = "Create User (Pulse)",
                JoinCapabilities = eJoinCapabilities.FromSIMPL,
                JoinType = eJoinType.Digital
            });

        /// <summary>
        /// D14: Delete user by input trigger
        /// </summary>
        [JoinName("DeleteUser")]
        public JoinDataComplete DeleteUser = new JoinDataComplete(
            new JoinData { JoinNumber = 14, JoinSpan = 1 },
            new JoinMetadata
            {
                Description = "Delete User By Input (Pulse)",
                JoinCapabilities = eJoinCapabilities.FromSIMPL,
                JoinType = eJoinType.Digital
            });

        /// <summary>
        /// D15: Delete selected user trigger
        /// </summary>
        [JoinName("DeleteSelectedUser")]
        public JoinDataComplete DeleteSelectedUser = new JoinDataComplete(
            new JoinData { JoinNumber = 15, JoinSpan = 1 },
            new JoinMetadata
            {
                Description = "Delete Selected User (Pulse)",
                JoinCapabilities = eJoinCapabilities.FromSIMPL,
                JoinType = eJoinType.Digital
            });

        /// <summary>
        /// D16: Update selected user's password trigger
        /// </summary>
        [JoinName("UpdatePassword")]
        public JoinDataComplete UpdatePassword = new JoinDataComplete(
            new JoinData { JoinNumber = 16, JoinSpan = 1 },
            new JoinMetadata
            {
                Description = "Update Selected Password (Pulse)",
                JoinCapabilities = eJoinCapabilities.FromSIMPL,
                JoinType = eJoinType.Digital
            });

        /// <summary>
        /// D17: Update selected user's access trigger
        /// </summary>
        [JoinName("UpdateAccess")]
        public JoinDataComplete UpdateAccess = new JoinDataComplete(
            new JoinData { JoinNumber = 17, JoinSpan = 1 },
            new JoinMetadata
            {
                Description = "Update Selected Access (Pulse)",
                JoinCapabilities = eJoinCapabilities.FromSIMPL,
                JoinType = eJoinType.Digital
            });

        /// <summary>
        /// D18: Save changes to selected user trigger
        /// </summary>
        [JoinName("UpdateSelectedUser")]
        public JoinDataComplete UpdateSelectedUser = new JoinDataComplete(
            new JoinData { JoinNumber = 18, JoinSpan = 1 },
            new JoinMetadata
            {
                Description = "Save Changes To Selected User (Pulse)",
                JoinCapabilities = eJoinCapabilities.FromSIMPL,
                JoinType = eJoinType.Digital
            });

        /// <summary>
        /// D19: Load selected user to inputs trigger
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
        /// D20: Clear all inputs trigger
        /// </summary>
        [JoinName("ClearInputs")]
        public JoinDataComplete ClearInputs = new JoinDataComplete(
            new JoinData { JoinNumber = 20, JoinSpan = 1 },
            new JoinMetadata
            {
                Description = "Clear All Inputs (Pulse)",
                JoinCapabilities = eJoinCapabilities.FromSIMPL,
                JoinType = eJoinType.Digital
            });

        /// <summary>
        /// D21: Unmask password (hold high)
        /// </summary>
        [JoinName("UnmaskPasswordInput")]
        public JoinDataComplete UnmaskPasswordInput = new JoinDataComplete(
            new JoinData { JoinNumber = 21, JoinSpan = 1 },
            new JoinMetadata
            {
                Description = "Unmask Password (Hold high)",
                JoinCapabilities = eJoinCapabilities.FromSIMPL,
                JoinType = eJoinType.Digital
            });

        /// <summary>
        /// D22: Refresh users from server trigger
        /// </summary>
        [JoinName("RefreshUsers")]
        public JoinDataComplete RefreshUsers = new JoinDataComplete(
            new JoinData { JoinNumber = 22, JoinSpan = 1 },
            new JoinMetadata
            {
                Description = "Refresh Users (Pulse)",
                JoinCapabilities = eJoinCapabilities.FromSIMPL,
                JoinType = eJoinType.Digital
            });

        /// <summary>
        /// D23: Select next user trigger
        /// </summary>
        [JoinName("SelectNextUser")]
        public JoinDataComplete SelectNextUser = new JoinDataComplete(
            new JoinData { JoinNumber = 23, JoinSpan = 1 },
            new JoinMetadata
            {
                Description = "Select Next User",
                JoinCapabilities = eJoinCapabilities.FromSIMPL,
                JoinType = eJoinType.Digital
            });

        /// <summary>
        /// D24: Select previous user trigger
        /// </summary>
        [JoinName("SelectPreviousUser")]
        public JoinDataComplete SelectPreviousUser = new JoinDataComplete(
            new JoinData { JoinNumber = 24, JoinSpan = 1 },
            new JoinMetadata
            {
                Description = "Select Previous User",
                JoinCapabilities = eJoinCapabilities.FromSIMPL,
                JoinType = eJoinType.Digital
            });

        // ===== User List (31-50) =====

        /// <summary>
        /// D31-50: User list item select (input) and visible feedback (output) - same joins for EISC alignment
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
        /// D31-50: User list item visible feedback (same joins as select for EISC alignment)
        /// </summary>
        [JoinName("UserListVisibleFb")]
        public JoinDataComplete UserListVisibleFb = new JoinDataComplete(
            new JoinData { JoinNumber = 31, JoinSpan = 20 },
            new JoinMetadata
            {
                Description = "User List Item Visible [1-20]",
                JoinCapabilities = eJoinCapabilities.ToSIMPL,
                JoinType = eJoinType.Digital
            });

        #endregion

        #region Analog

        /// <summary>
        /// A1: User count feedback
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
        /// A2: Selected user index (bidirectional - set and feedback)
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
        /// Access level input (numeric)
        /// </summary>
        [JoinName("AccessLevelInput")]
        public JoinDataComplete AccessLevelInput = new JoinDataComplete(
            new JoinData { JoinNumber = 3, JoinSpan = 1 },
            new JoinMetadata
            {
                Description = "Access Level Input (analog)",
                JoinCapabilities = eJoinCapabilities.FromSIMPL,
                JoinType = eJoinType.Analog
            });


        /// <summary>
        /// A3: Selected user's access level feedback
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
        /// A4: Validated user's access level feedback
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


        #endregion

        #region Serial

        /// <summary>
        /// Device name feedback
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
        /// Status message feedback
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

        // <summary>
        /// Username input (bidirectional - input and feedback on same join for EISC alignment)
        /// </summary>
        [JoinName("UsernameInput")]
        public JoinDataComplete UsernameInput = new JoinDataComplete(
            new JoinData { JoinNumber = 4, JoinSpan = 1 },
            new JoinMetadata
            {
                Description = "Username Input",
                JoinCapabilities = eJoinCapabilities.FromSIMPL,
                JoinType = eJoinType.Serial
            });

        /// <summary>
        /// Username input feedback (same join as input for EISC alignment)
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
        /// S5: Password input (bidirectional - input and feedback on same join for EISC alignment)
        /// </summary>
        [JoinName("PasswordInput")]
        public JoinDataComplete PasswordInput = new JoinDataComplete(
            new JoinData { JoinNumber = 5, JoinSpan = 1 },
            new JoinMetadata
            {
                Description = "Password Input",
                JoinCapabilities = eJoinCapabilities.FromSIMPL,
                JoinType = eJoinType.Serial
            });

        /// <summary>
        /// S5: Password input feedback (same join as input for EISC alignment)
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
        /// S6: Access input (bidirectional - input and feedback on same join for EISC alignment)
        /// </summary>
        [JoinName("AccessInput")]
        public JoinDataComplete AccessInput = new JoinDataComplete(
            new JoinData { JoinNumber = 6, JoinSpan = 1 },
            new JoinMetadata
            {
                Description = "Access Level Input (string)",
                JoinCapabilities = eJoinCapabilities.FromSIMPL,
                JoinType = eJoinType.Serial
            });

        /// <summary>
        /// S6: Access input feedback (same join as input for EISC alignment)
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

        /// <summary>
        /// S11: User list JSON feedback
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
        /// S12: Original username being edited feedback
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
        /// S13: Selected user's username feedback
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
        /// S14: Selected user's password feedback (masked)
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
        /// S15: Selected user's access level feedback
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

        /// <summary>
        /// S31-50: User list item usernames feedback
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
