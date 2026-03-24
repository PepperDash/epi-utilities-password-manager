using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.Plugin.Password.Server
{
	/// <summary>
	/// Password Server Bridge Join Map
	/// </summary>
	/// <remarks>
	/// Defines the EISC bridge joins for the Password Server.
	/// Server can only be bridged once and provides status feedback and user management operations.
	/// 
	/// Join Layout:
	/// 
	/// DIGITAL (1-20):
	///   1-3   : Success feedback pulses (ToSIMPL)
	///   11-18 : Action triggers (FromSIMPL)
	/// 
	/// ANALOG (1-4):
	///   1     : User count (ToSIMPL)
	///   2     : Selected user index (bidirectional)
	///   3     : Selected user access (ToSIMPL)
	///   4     : Validated user access (ToSIMPL)
	/// 
	/// SERIAL (1-14):
	///   1     : Device name (ToSIMPL)
	///   2     : Status message (ToSIMPL)
	///   3     : Validated username (ToSIMPL)
	///   4     : Username input (FromSIMPL)
	///   5     : Password input (FromSIMPL)
	///   6     : Access input (FromSIMPL)
	///   11    : User list JSON (ToSIMPL)
	///   12    : Selected username (ToSIMPL)
	///   13    : Selected password (ToSIMPL)
	///   14    : Selected access (ToSIMPL)
	/// </remarks>
	public class PasswordServerBridgeJoinMap : JoinMapBaseAdvanced
	{
		#region Digital

		// ===== Success Feedbacks (1-3) =====

		/// <summary>
		/// D1: Create user success feedback
		/// </summary>
		[JoinName("CreateUserSuccessFb")]
		public JoinDataComplete CreateUserSuccessFb = new JoinDataComplete(
			new JoinData { JoinNumber = 1, JoinSpan = 1 },
			new JoinMetadata
			{
				Description = "Create User Success Feedback",
				JoinCapabilities = eJoinCapabilities.ToSIMPL,
				JoinType = eJoinType.Digital
			});

		/// <summary>
		/// D2: Delete user success feedback
		/// </summary>
		[JoinName("DeleteUserSuccessFb")]
		public JoinDataComplete DeleteUserSuccessFb = new JoinDataComplete(
			new JoinData { JoinNumber = 2, JoinSpan = 1 },
			new JoinMetadata
			{
				Description = "Delete User Success Feedback",
				JoinCapabilities = eJoinCapabilities.ToSIMPL,
				JoinType = eJoinType.Digital
			});

		/// <summary>
		/// D3: Validate user success feedback
		/// </summary>
		[JoinName("ValidateUserSuccessFb")]
		public JoinDataComplete ValidateUserSuccessFb = new JoinDataComplete(
			new JoinData { JoinNumber = 3, JoinSpan = 1 },
			new JoinMetadata
			{
				Description = "Validate User Success Feedback",
				JoinCapabilities = eJoinCapabilities.ToSIMPL,
				JoinType = eJoinType.Digital
			});

		// ===== Actions (11-18) =====

		/// <summary>
		/// D11: Validate user credentials trigger
		/// </summary>
		[JoinName("ValidateUser")]
		public JoinDataComplete ValidateUser = new JoinDataComplete(
			new JoinData { JoinNumber = 11, JoinSpan = 1 },
			new JoinMetadata
			{
				Description = "Validate User Credentials (Pulse)",
				JoinCapabilities = eJoinCapabilities.FromSIMPL,
				JoinType = eJoinType.Digital
			});

		/// <summary>
		/// D12: Create user trigger
		/// </summary>
		[JoinName("CreateUser")]
		public JoinDataComplete CreateUser = new JoinDataComplete(
			new JoinData { JoinNumber = 12, JoinSpan = 1 },
			new JoinMetadata
			{
				Description = "Create User (Pulse)",
				JoinCapabilities = eJoinCapabilities.FromSIMPL,
				JoinType = eJoinType.Digital
			});

		/// <summary>
		/// D13: Delete user trigger
		/// </summary>
		[JoinName("DeleteUser")]
		public JoinDataComplete DeleteUser = new JoinDataComplete(
			new JoinData { JoinNumber = 13, JoinSpan = 1 },
			new JoinMetadata
			{
				Description = "Delete User (Pulse)",
				JoinCapabilities = eJoinCapabilities.FromSIMPL,
				JoinType = eJoinType.Digital
			});

		/// <summary>
		/// D14: Update selected user's password trigger
		/// </summary>
		[JoinName("UpdatePassword")]
		public JoinDataComplete UpdatePassword = new JoinDataComplete(
			new JoinData { JoinNumber = 14, JoinSpan = 1 },
			new JoinMetadata
			{
				Description = "Update Selected User's Password (Pulse)",
				JoinCapabilities = eJoinCapabilities.FromSIMPL,
				JoinType = eJoinType.Digital
			});

		/// <summary>
		/// D15: Update selected user's access level trigger
		/// </summary>
		[JoinName("UpdateAccess")]
		public JoinDataComplete UpdateAccess = new JoinDataComplete(
			new JoinData { JoinNumber = 15, JoinSpan = 1 },
			new JoinMetadata
			{
				Description = "Update Selected User's Access Level (Pulse)",
				JoinCapabilities = eJoinCapabilities.FromSIMPL,
				JoinType = eJoinType.Digital
			});

		/// <summary>
		/// D16: Refresh users from file trigger
		/// </summary>
		[JoinName("RefreshUsers")]
		public JoinDataComplete RefreshUsers = new JoinDataComplete(
			new JoinData { JoinNumber = 16, JoinSpan = 1 },
			new JoinMetadata
			{
				Description = "Refresh Users List (Pulse)",
				JoinCapabilities = eJoinCapabilities.FromSIMPL,
				JoinType = eJoinType.Digital
			});

		/// <summary>
		/// D17: Select next user trigger
		/// </summary>
		[JoinName("SelectNextUser")]
		public JoinDataComplete SelectNextUser = new JoinDataComplete(
			new JoinData { JoinNumber = 17, JoinSpan = 1 },
			new JoinMetadata
			{
				Description = "Select Next User",
				JoinCapabilities = eJoinCapabilities.FromSIMPL,
				JoinType = eJoinType.Digital
			});

		/// <summary>
		/// D18: Select previous user trigger
		/// </summary>
		[JoinName("SelectPreviousUser")]
		public JoinDataComplete SelectPreviousUser = new JoinDataComplete(
			new JoinData { JoinNumber = 18, JoinSpan = 1 },
			new JoinMetadata
			{
				Description = "Select Previous User",
				JoinCapabilities = eJoinCapabilities.FromSIMPL,
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
		/// A3: Selected user's access level feedback
		/// </summary>
		[JoinName("SelectedUserAccessFb")]
		public JoinDataComplete SelectedUserAccessFb = new JoinDataComplete(
			new JoinData { JoinNumber = 3, JoinSpan = 1 },
			new JoinMetadata
			{
				Description = "Selected User Access Level Feedback",
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
				Description = "Validated User Access Level Feedback",
				JoinCapabilities = eJoinCapabilities.ToSIMPL,
				JoinType = eJoinType.Analog
			});

		#endregion

		#region Serial

		/// <summary>
		/// S1: Device name feedback
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
		/// S2: Status message feedback
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
		/// S3: Validated username feedback
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
		/// S4: Username input for create/delete/validate operations
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
		/// S5: Password input for create/validate/update operations
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
		/// S6: Access level input for create/update operations
		/// </summary>
		[JoinName("AccessInput")]
		public JoinDataComplete AccessInput = new JoinDataComplete(
			new JoinData { JoinNumber = 6, JoinSpan = 1 },
			new JoinMetadata
			{
				Description = "Access Level Input",
				JoinCapabilities = eJoinCapabilities.FromSIMPL,
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
		/// S12: Selected user's username feedback
		/// </summary>
		[JoinName("SelectedUsernameFb")]
		public JoinDataComplete SelectedUsernameFb = new JoinDataComplete(
			new JoinData { JoinNumber = 12, JoinSpan = 1 },
			new JoinMetadata
			{
				Description = "Selected Username Feedback",
				JoinCapabilities = eJoinCapabilities.ToSIMPL,
				JoinType = eJoinType.Serial
			});

		/// <summary>
		/// S13: Selected user's password feedback (masked based on config)
		/// </summary>
		[JoinName("SelectedPasswordFb")]
		public JoinDataComplete SelectedPasswordFb = new JoinDataComplete(
			new JoinData { JoinNumber = 13, JoinSpan = 1 },
			new JoinMetadata
			{
				Description = "Selected Password Feedback",
				JoinCapabilities = eJoinCapabilities.ToSIMPL,
				JoinType = eJoinType.Serial
			});

		/// <summary>
		/// S14: Selected user's access level feedback (string)
		/// </summary>
		[JoinName("SelectedAccessFb")]
		public JoinDataComplete SelectedAccessFb = new JoinDataComplete(
			new JoinData { JoinNumber = 14, JoinSpan = 1 },
			new JoinMetadata
			{
				Description = "Selected Access Level Feedback",
				JoinCapabilities = eJoinCapabilities.ToSIMPL,
				JoinType = eJoinType.Serial
			});

		#endregion

		/// <summary>
		/// Password Server BridgeJoinMap constructor
		/// </summary>
		/// <param name="joinStart">This will be the join it starts on the EISC bridge</param>
		public PasswordServerBridgeJoinMap(uint joinStart)
			: base(joinStart, typeof(PasswordServerBridgeJoinMap))
		{
		}
	}
}