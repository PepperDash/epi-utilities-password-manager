using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.Plugin.Password.Server
{
	/// <summary>
	/// Password Server Bridge Join Map
	/// </summary>
	/// <remarks>
	/// Defines the EISC bridge joins for the Password Server.
	/// Server can only be bridged once and provides status feedback and user management operations.
	/// Input/output pairs share the same join for EISC alignment.
	/// 
	/// Join Layout:
	/// 
	/// DIGITAL (1-13):
	///   1     : CreateUser/CreateUserSuccessFb (bidirectional)
	///   2     : DeleteUser/DeleteUserSuccessFb (bidirectional)
	///   3     : ValidateUser/ValidateUserSuccessFb (bidirectional)
	///   4     : UpdatePassword (FromSIMPL)
	///   5     : UpdateAccess (FromSIMPL)
	///   11    : RefreshUsers (FromSIMPL)
	///   12    : SelectNextUser (FromSIMPL)
	///   13    : SelectPreviousUser (FromSIMPL)
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
	///   3     : FormUsernameInput/ValidatedUsernameFb (bidirectional)
	///   4     : Form password input (FromSIMPL)
	///   5     : Form access input (FromSIMPL)
	///   11    : User list JSON (ToSIMPL)
	///   12    : Selected username (ToSIMPL)
	///   13    : Selected password (ToSIMPL)
	///   14    : Selected access (ToSIMPL)
	/// </remarks>
	public class PasswordServerBridgeJoinMap : JoinMapBaseAdvanced
	{
		#region Digital

		/// <summary>
		/// Create user trigger
		/// </summary>
		[JoinName("CreateUser")]
		public JoinDataComplete CreateUser = new JoinDataComplete(
			new JoinData { JoinNumber = 1, JoinSpan = 1 },
			new JoinMetadata
			{
				Description = "Create User (Pulse)",
				JoinCapabilities = eJoinCapabilities.FromSIMPL,
				JoinType = eJoinType.Digital
			});

		/// <summary>
		/// Create user success feedback
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
		/// Delete user trigger
		/// </summary>
		[JoinName("DeleteUser")]
		public JoinDataComplete DeleteUser = new JoinDataComplete(
			new JoinData { JoinNumber = 2, JoinSpan = 1 },
			new JoinMetadata
			{
				Description = "Delete User (Pulse)",
				JoinCapabilities = eJoinCapabilities.FromSIMPL,
				JoinType = eJoinType.Digital
			});


		/// <summary>
		/// Delete user success feedback
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
		/// Validate user credentials trigger
		/// </summary>
		[JoinName("ValidateUser")]
		public JoinDataComplete ValidateUser = new JoinDataComplete(
			new JoinData { JoinNumber = 3, JoinSpan = 1 },
			new JoinMetadata
			{
				Description = "Validate User Credentials (Pulse)",
				JoinCapabilities = eJoinCapabilities.FromSIMPL,
				JoinType = eJoinType.Digital
			});

		/// <summary>
		/// Validate user success feedback
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

		/// <summary>
		/// Update selected user's password trigger
		/// </summary>
		[JoinName("UpdatePassword")]
		public JoinDataComplete UpdatePassword = new JoinDataComplete(
			new JoinData { JoinNumber = 4, JoinSpan = 1 },
			new JoinMetadata
			{
				Description = "Update Selected User's Password (Pulse)",
				JoinCapabilities = eJoinCapabilities.FromSIMPL,
				JoinType = eJoinType.Digital
			});

		/// <summary>
		/// Update selected user's access level trigger
		/// </summary>
		[JoinName("UpdateAccess")]
		public JoinDataComplete UpdateAccess = new JoinDataComplete(
			new JoinData { JoinNumber = 5, JoinSpan = 1 },
			new JoinMetadata
			{
				Description = "Update Selected User's Access Level (Pulse)",
				JoinCapabilities = eJoinCapabilities.FromSIMPL,
				JoinType = eJoinType.Digital
			});

		/// <summary>
		/// Refresh users from file trigger
		/// </summary>
		[JoinName("RefreshUsers")]
		public JoinDataComplete RefreshUsers = new JoinDataComplete(
			new JoinData { JoinNumber = 11, JoinSpan = 1 },
			new JoinMetadata
			{
				Description = "Refresh Users List (Pulse)",
				JoinCapabilities = eJoinCapabilities.FromSIMPL,
				JoinType = eJoinType.Digital
			});

		/// <summary>
		/// Select next user trigger
		/// </summary>
		[JoinName("SelectNextUser")]
		public JoinDataComplete SelectNextUser = new JoinDataComplete(
			new JoinData { JoinNumber = 12, JoinSpan = 1 },
			new JoinMetadata
			{
				Description = "Select Next User",
				JoinCapabilities = eJoinCapabilities.FromSIMPL,
				JoinType = eJoinType.Digital
			});

		/// <summary>
		/// Select previous user trigger
		/// </summary>
		[JoinName("SelectPreviousUser")]
		public JoinDataComplete SelectPreviousUser = new JoinDataComplete(
			new JoinData { JoinNumber = 13, JoinSpan = 1 },
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

		/// <summary>
		/// Form username input for create/delete/validate operations
		/// </summary>
		[JoinName("FormUsernameInput")]
		public JoinDataComplete FormUsernameInput = new JoinDataComplete(
			new JoinData { JoinNumber = 3, JoinSpan = 1 },
			new JoinMetadata
			{
				Description = "Form Username Input",
				JoinCapabilities = eJoinCapabilities.FromSIMPL,
				JoinType = eJoinType.Serial
			});

		/// <summary>
		/// Form password input for create/validate/update operations
		/// </summary>
		[JoinName("FormPasswordInput")]
		public JoinDataComplete FormPasswordInput = new JoinDataComplete(
			new JoinData { JoinNumber = 4, JoinSpan = 1 },
			new JoinMetadata
			{
				Description = "Form Password Input",
				JoinCapabilities = eJoinCapabilities.FromSIMPL,
				JoinType = eJoinType.Serial
			});

		/// <summary>
		/// Form access level input for create/update operations
		/// </summary>
		[JoinName("FormAccessInput")]
		public JoinDataComplete FormAccessInput = new JoinDataComplete(
			new JoinData { JoinNumber = 5, JoinSpan = 1 },
			new JoinMetadata
			{
				Description = "Form Access Level Input",
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
		/// Selected user's username feedback
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
		/// Selected user's password feedback (masked based on config)
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
		/// Selected user's access level feedback (string)
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