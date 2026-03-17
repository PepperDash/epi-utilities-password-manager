using System.Collections.Generic;
using Newtonsoft.Json;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;

namespace PepperDash.Essentials.Plugin.PasswordManager
{
    /// <summary>
    /// Password Manager Server Factory
    /// </summary>
    /// <remarks>
    /// Creates Password Manager Server devices.
    /// Type key: "passwordManagerServer"
    /// </remarks>
    public class PasswordManagerServerFactory : EssentialsPluginDeviceFactory<PasswordManagerServer>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public PasswordManagerServerFactory()
        {
            TypeNames = new List<string> { "passwordManagerServer" };
            MinimumEssentialsFrameworkVersion = "2.12.1";
        }

        /// <summary>
        /// Build device from config
        /// </summary>
        public override EssentialsDevice BuildDevice(DeviceConfig dc)
        {
            Debug.Console(1, "[{0}] Building Password Manager Server...", dc.Key);

            var config = dc.Properties.ToObject<PasswordManagerServerConfig>();
            if (config == null)
            {
                Debug.Console(0, "[{0}] Failed to parse config", dc.Key);
                return null;
            }

            return new PasswordManagerServer(dc.Key, dc.Name, config);
        }
    }
}
