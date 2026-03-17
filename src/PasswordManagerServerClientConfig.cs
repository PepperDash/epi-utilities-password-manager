using System.Collections.Generic;
using Newtonsoft.Json;
using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.Plugin.PasswordManager
{
    /// <summary>
    /// Password Manager Server configuration object
    /// </summary>
    /// <remarks>
    /// Configuration for the Password Manager Server device that stores users
    /// </remarks>
    [ConfigSnippet("\"properties\":{\"filePath\":\"PasswordManager/users.json\"}")]
    public class PasswordManagerServerConfig
    {
        /// <summary>
        /// File path for storing user credentials JSON file (relative to program directory)
        /// </summary>
        [JsonProperty("filePath")]
        public string FilePath { get; set; }

        /// <summary>
        /// Default users to seed the file with if it doesn't exist
        /// </summary>
        [JsonProperty("defaultUsers")]
        public List<UserCredential> DefaultUsers { get; set; }

        /// <summary>
        /// Time in milliseconds to wait before saving changes to file
        /// </summary>
        [JsonProperty("saveDelayMs")]
        public long SaveDelayMs { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public PasswordManagerServerConfig()
        {
            DefaultUsers = new List<UserCredential>();
            SaveDelayMs = 1000;
        }
    }

    /// <summary>
    /// Password Manager Client configuration object
    /// </summary>
    /// <remarks>
    /// Configuration for the Password Manager Client device that references a server
    /// </remarks>
    [ConfigSnippet("\"properties\":{\"serverKey\":\"passwordManagerServer-1\"}")]
    public class PasswordManagerClientConfig
    {
        /// <summary>
        /// The device key of the Password Manager Server to use
        /// </summary>
        [JsonProperty("serverKey")]
        public string ServerKey { get; set; }

        /// <summary>
        /// Whether to mask passwords in feedback (shows asterisks instead of actual password)
        /// </summary>
        [JsonProperty("maskPasswords")]
        public bool MaskPasswords { get; set; }

        /// <summary>
        /// Whether to clear input fields on successful login
        /// </summary>
        [JsonProperty("clearInputsOnLogin")]
        public bool ClearInputsOnLogin { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public PasswordManagerClientConfig()
        {
            MaskPasswords = true;
            ClearInputsOnLogin = true;
        }
    }
}
