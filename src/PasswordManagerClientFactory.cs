using System.Collections.Generic;
using Newtonsoft.Json;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;

namespace PepperDash.Essentials.Plugin.PasswordManager
{
    /// <summary>
    /// Password Manager Client Factory
    /// </summary>
    /// <remarks>
    /// Creates Password Manager Client devices.
    /// Type key: "passwordManagerClient"
    /// </remarks>
    public class PasswordManagerClientFactory : EssentialsPluginDeviceFactory<PasswordManagerClient>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public PasswordManagerClientFactory()
        {
            TypeNames = new List<string> { "passwordManagerClient" };
            MinimumEssentialsFrameworkVersion = "2.12.1";
        }

        /// <summary>
        /// Build device from config
        /// </summary>
        public override EssentialsDevice BuildDevice(DeviceConfig dc)
        {
            Debug.Console(1, "[{0}] Building Password Manager Client...", dc.Key);

            var config = dc.Properties.ToObject<PasswordManagerClientConfig>();
            if (config == null)
            {
                Debug.Console(0, "[{0}] Failed to parse config", dc.Key);
                return null;
            }

            if (string.IsNullOrEmpty(config.ServerKey))
            {
                Debug.Console(0, "[{0}] ServerKey is required", dc.Key);
                return null;
            }

            return new PasswordManagerClient(dc.Key, dc.Name, config);
        }
    }
}
