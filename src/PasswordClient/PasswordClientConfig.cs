using System.Collections.Generic;
using Newtonsoft.Json;
using PepperDash.Core;
using PepperDash.Core.Logging;
using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.Plugin.Password.Client
{
    /// <summary>
    /// Password Client configuration object
    /// </summary>
    /// <remarks>
    /// Configuration for the Password Client device that references a server.
    /// Multiple clients can be bridged and interact with the server as the authentication authority.
    /// </remarks>
    [ConfigSnippet("\"properties\":{\"serverKey\":\"passwordServer-1\"}")]
    public class PasswordClientConfig
    {
        /// <summary>
        /// The device key of the Password Server to use
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
        public PasswordClientConfig()
        {
            MaskPasswords = true;
            ClearInputsOnLogin = true;
        }
    }
}