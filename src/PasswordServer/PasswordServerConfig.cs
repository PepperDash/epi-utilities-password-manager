using System.Collections.Generic;
using Newtonsoft.Json;
using PepperDash.Core;
using PepperDash.Core.Logging;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Plugin.PasswordManager;

namespace PepperDash.Essentials.Plugin.Password.Server
{
    /// <summary>
    /// Password Server configuration object
    /// </summary>
    /// <remarks>
    /// Configuration for the Password Server device that stores users.
    /// Server can only be bridged once.
    /// </remarks>
    [ConfigSnippet("\"properties\":{\"filePath\":\"PasswordManager/users.json\"}")]
    public class PasswordServerConfig
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
        /// Whether to mask passwords in feedback (shows asterisks instead of actual password)
        /// </summary>
        [JsonProperty("maskPasswords")]
        public bool MaskPasswords { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public PasswordServerConfig()
        {
            DefaultUsers = new List<UserCredential>();
            SaveDelayMs = 1000;
            MaskPasswords = true;
        }
    }
}
