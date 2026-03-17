using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.Plugin.PasswordManager
{
	/// <summary>
	/// Password Manager Bridge Join Map
	/// </summary>
	/// <remarks>
	/// Defines the EISC bridge joins for user/password management operations
	/// </remarks>
	public class PasswordManagerBridgeJoinMap : JoinMapBaseAdvanced
	{
		#region Digital

		/// <summary>
		/// Trigger to create a new user with the provided username, password, and access level
		/// </summary>
		[JoinName("CreateUser")]
		public JoinDataComplete CreateUser = new JoinDataComplete(
			new JoinData { JoinNumber = 1, JoinSpan = 1 },
			new JoinMetadata
			{
				Description = "Create User (Pulse to execute)",
				JoinCapabilities = eJoinCapabilities.FromSIMPL,
				JoinType = eJoinType.Digital
			});

		/// <summary>
		/// Trigger to delete a user by the provided username
		/// </summary>
		[JoinName("DeleteUser")]
		public JoinDataComplete DeleteUser = new JoinDataComplete(
			new JoinData { JoinNumber = 2, JoinSpan = 1 },
			new JoinMetadata
			{
				Description = "Delete User (Pulse to execute)",
				JoinCapabilities = eJoinCapabilities.FromSIMPL,
				JoinType = eJoinType.Digital
			});

		/// <summary>
		/// Trigger to validate user credentials
		/// </summary>
		[JoinName("ValidateUser")]
		public JoinDataComplete ValidateUser = new JoinDataComplete(
			new JoinData { JoinNumber = 3, JoinSpan = 1 },
			new JoinMetadata
			{
				Description = "Validate User Credentials (Pulse to execute)",
				JoinCapabilities = eJoinCapabilities.FromSIMPL,
				JoinType = eJoinType.Digital
			});

		/// <summary>
		/// Feedback indicating the last create operation was successful
		/// </summary>
		[JoinName("CreateUserSuccessFb")]
		public JoinDataComplete CreateUserSuccessFb = new JoinDataComplete(
			new JoinData { JoinNumber = 4, JoinSpan = 1 },
			new JoinMetadata
			{
				Description = "Create User Success Feedback",
				JoinCapabilities = eJoinCapabilities.ToSIMPL,
				JoinType = eJoinType.Digital
			});

		/// <summary>
		/// Feedback indicating the last delete operation was successful
		/// </summary>
		[JoinName("DeleteUserSuccessFb")]
		public JoinDataComplete DeleteUserSuccessFb = new JoinDataComplete(
			new JoinData { JoinNumber = 5, JoinSpan = 1 },
			new JoinMetadata
			{
				Description = "Delete User Success Feedback",
				JoinCapabilities = eJoinCapabilities.ToSIMPL,
				JoinType = eJoinType.Digital
			});

		/// <summary>
		/// Feedback indicating the last validation was successful
		/// </summary>
		[JoinName("ValidateUserSuccessFb")]
		public JoinDataComplete ValidateUserSuccessFb = new JoinDataComplete(
			new JoinData { JoinNumber = 6, JoinSpan = 1 },
			new JoinMetadata
			{
				Description = "Validate User Success Feedback",
				JoinCapabilities = eJoinCapabilities.ToSIMPL,
				JoinType = eJoinType.Digital
			});

		/// <summary>
		/// Trigger to refresh/reload the user list from file
		/// </summary>
		[JoinName("RefreshUsers")]
		public JoinDataComplete RefreshUsers = new JoinDataComplete(
			new JoinData { JoinNumber = 7, JoinSpan = 1 },
			new JoinMetadata
			{
				Description = "Refresh Users List (Pulse to execute)",
				JoinCapabilities = eJoinCapabilities.FromSIMPL,
				JoinType = eJoinType.Digital
			});

		/// <summary>
		/// Trigger to select the next user in the list
		/// </summary>
		[JoinName("SelectNextUser")]
		public JoinDataComplete SelectNextUser = new JoinDataComplete(
			new JoinData { JoinNumber = 8, JoinSpan = 1 },
			new JoinMetadata
			{
				Description = "Select Next User",
				JoinCapabilities = eJoinCapabilities.FromSIMPL,
				JoinType = eJoinType.Digital
			});

		/// <summary>
		/// Trigger to select the previous user in the list
		/// </summary>
		[JoinName("SelectPreviousUser")]
		public JoinDataComplete SelectPreviousUser = new JoinDataComplete(
			new JoinData { JoinNumber = 9, JoinSpan = 1 },
			new JoinMetadata
			{
				Description = "Select Previous User",
				JoinCapabilities = eJoinCapabilities.FromSIMPL,
				JoinType = eJoinType.Digital
			});

		/// <summary>
		/// Trigger to update the selected user's password
		/// </summary>
		[JoinName("UpdatePassword")]
		public JoinDataComplete UpdatePassword = new JoinDataComplete(
			new JoinData { JoinNumber = 10, JoinSpan = 1 },
			new JoinMetadata
			{
				Description = "Update Selected User's Password (Pulse to execute)",
				JoinCapabilities = eJoinCapabilities.FromSIMPL,
				JoinType = eJoinType.Digital
			});

		/// <summary>
		/// Trigger to update the selected user's access level
		/// </summary>
		[JoinName("UpdateAccess")]
		public JoinDataComplete UpdateAccess = new JoinDataComplete(
			new JoinData { JoinNumber = 11, JoinSpan = 1 },
			new JoinMetadata
			{
				Description = "Update Selected User's Access Level (Pulse to execute)",
				JoinCapabilities = eJoinCapabilities.FromSIMPL,
				JoinType = eJoinType.Digital
			});

		#endregion

		#region Analog

		/// <summary>
		/// Total number of users stored
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
				Description = "Selected User Access Level Feedback",
				JoinCapabilities = eJoinCapabilities.ToSIMPL,
				JoinType = eJoinType.Analog
			});

		/// <summary>
		/// Validated user's access level (after successful validation)
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
		/// Username input for create/delete/validate operations
		/// </summary>
		[JoinName("UsernameInput")]
		public JoinDataComplete UsernameInput = new JoinDataComplete(
			new JoinData { JoinNumber = 2, JoinSpan = 1 },
			new JoinMetadata
			{
				Description = "Username Input",
				JoinCapabilities = eJoinCapabilities.FromSIMPL,
				JoinType = eJoinType.Serial
			});

		/// <summary>
		/// Password input for create/validate/update operations
		/// </summary>
		[JoinName("PasswordInput")]
		public JoinDataComplete PasswordInput = new JoinDataComplete(
			new JoinData { JoinNumber = 3, JoinSpan = 1 },
			new JoinMetadata
			{
				Description = "Password Input",
				JoinCapabilities = eJoinCapabilities.FromSIMPL,
				JoinType = eJoinType.Serial
			});

		/// <summary>
		/// Access level input for create/update operations
		/// </summary>
		[JoinName("AccessInput")]
		public JoinDataComplete AccessInput = new JoinDataComplete(
			new JoinData { JoinNumber = 4, JoinSpan = 1 },
			new JoinMetadata
			{
				Description = "Access Level Input",
				JoinCapabilities = eJoinCapabilities.FromSIMPL,
				JoinType = eJoinType.Serial
			});

		/// <summary>
		/// JSON array of all users (serialized)
		/// </summary>
		[JoinName("UserListFb")]
		public JoinDataComplete UserListFb = new JoinDataComplete(
			new JoinData { JoinNumber = 5, JoinSpan = 1 },
			new JoinMetadata
			{
				Description = "User List JSON Feedback",
				JoinCapabilities = eJoinCapabilities.ToSIMPL,
				JoinType = eJoinType.Serial
			});

		/// <summary>
		/// Last operation status message
		/// </summary>
		[JoinName("StatusMessageFb")]
		public JoinDataComplete StatusMessageFb = new JoinDataComplete(
			new JoinData { JoinNumber = 6, JoinSpan = 1 },
			new JoinMetadata
			{
				Description = "Status Message Feedback",
				JoinCapabilities = eJoinCapabilities.ToSIMPL,
				JoinType = eJoinType.Serial
			});

		/// <summary>
		/// Selected user's username
		/// </summary>
		[JoinName("SelectedUsernameFb")]
		public JoinDataComplete SelectedUsernameFb = new JoinDataComplete(
			new JoinData { JoinNumber = 7, JoinSpan = 1 },
			new JoinMetadata
			{
				Description = "Selected Username Feedback",
				JoinCapabilities = eJoinCapabilities.ToSIMPL,
				JoinType = eJoinType.Serial
			});

		/// <summary>
		/// Selected user's password (masked or actual based on config)
		/// </summary>
		[JoinName("SelectedPasswordFb")]
		public JoinDataComplete SelectedPasswordFb = new JoinDataComplete(
			new JoinData { JoinNumber = 8, JoinSpan = 1 },
			new JoinMetadata
			{
				Description = "Selected Password Feedback",
				JoinCapabilities = eJoinCapabilities.ToSIMPL,
				JoinType = eJoinType.Serial
			});

		/// <summary>
		/// Selected user's access level as string
		/// </summary>
		[JoinName("SelectedAccessFb")]
		public JoinDataComplete SelectedAccessFb = new JoinDataComplete(
			new JoinData { JoinNumber = 9, JoinSpan = 1 },
			new JoinMetadata
			{
				Description = "Selected Access Level Feedback",
				JoinCapabilities = eJoinCapabilities.ToSIMPL,
				JoinType = eJoinType.Serial
			});

		/// <summary>
		/// Validated user's username (after successful validation)
		/// </summary>
		[JoinName("ValidatedUsernameFb")]
		public JoinDataComplete ValidatedUsernameFb = new JoinDataComplete(
			new JoinData { JoinNumber = 10, JoinSpan = 1 },
			new JoinMetadata
			{
				Description = "Validated Username Feedback",
				JoinCapabilities = eJoinCapabilities.ToSIMPL,
				JoinType = eJoinType.Serial
			});

		#endregion

		/// <summary>
		/// Password Manager BridgeJoinMap constructor
		/// </summary>
		/// <param name="joinStart">This will be the join it starts on the EISC bridge</param>
		public PasswordManagerBridgeJoinMap(uint joinStart)
			: base(joinStart, typeof(PasswordManagerBridgeJoinMap))
		{
		}
	}
}