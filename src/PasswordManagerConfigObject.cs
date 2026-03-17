using System.Collections.Generic;
using Newtonsoft.Json;
using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.Plugin.PasswordManager
{
	/// <summary>
	/// Password Manager configuration object
	/// </summary>
	/// <remarks>
	/// Configuration for the Password Manager plugin device
	/// </remarks>
	/// <example>
	/// <code>
	/// "properties": {
	///     "filePath": "PasswordManager/users.json",
	///     "maskPasswords": true,
	///     "defaultUsers": [
	///         {
	///             "username": "admin",
	///             "password": "admin123",
	///             "access": 3
	///         }
	///     ]
	/// }
	/// </code>
	/// </example>
	[ConfigSnippet("\"properties\":{\"filePath\":\"PasswordManager/users.json\"}")]
	public class PasswordManagerConfig
	{
		/// <summary>
		/// File path for storing user credentials JSON file (relative to program directory)
		/// </summary>
		/// <remarks>
		/// If not provided, users will only be stored in memory and lost on restart
		/// </remarks>
		/// <example>
		/// <code>
		/// "properties": {
		///     "filePath": "PasswordManager/users.json"
		/// }
		/// </code>
		/// </example>
		[JsonProperty("filePath")]
		public string FilePath { get; set; }

		/// <summary>
		/// Whether to mask passwords in feedback (shows asterisks instead of actual password)
		/// </summary>
		/// <remarks>
		/// Default is true for security. Set to false to show actual passwords in feedback.
		/// </remarks>
		[JsonProperty("maskPasswords")]
		public bool MaskPasswords { get; set; }

		/// <summary>
		/// Default users to seed the file with if it doesn't exist
		/// </summary>
		/// <remarks>
		/// These users will be created when the file is first created.
		/// If the file already exists, this property is ignored.
		/// </remarks>
		/// <example>
		/// <code>
		/// "properties": {
		///     "defaultUsers": [
		///         {
		///             "username": "admin",
		///             "password": "admin123",
		///             "access": 3
		///         },
		///         {
		///             "username": "tech",
		///             "password": "tech123",
		///             "access": 2
		///         }
		///     ]
		/// }
		/// </code>
		/// </example>
		[JsonProperty("defaultUsers")]
		public List<UserCredential> DefaultUsers { get; set; }

		/// <summary>
		/// Time in milliseconds to wait before saving changes to file
		/// </summary>
		/// <remarks>
		/// Helps debounce rapid changes. Default is 1000ms (1 second).
		/// </remarks>
		[JsonProperty("saveDelayMs")]
		public long SaveDelayMs { get; set; }

		/// <summary>
		/// Constructor
		/// </summary>
		public PasswordManagerConfig()
		{
			DefaultUsers = new List<UserCredential>();
			MaskPasswords = true;
			SaveDelayMs = 1000;
		}
	}
}