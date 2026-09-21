using System.Collections.Generic;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;

namespace PepperDash.Essentials.Plugin.Password.Client
{
    /// <summary>
    /// Password Client Factory
    /// </summary>
    /// <remarks>
    /// Creates Password Client devices.
    /// Type key: "passwordClient"
    /// </remarks>
    public class PasswordClientFactory : EssentialsPluginDeviceFactory<PasswordClient>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public PasswordClientFactory()
        {
            TypeNames = new List<string> { "passwordClient" };
            MinimumEssentialsFrameworkVersion = "2.12.1";
        }

        /// <summary>
        /// Build device from config
        /// </summary>
        public override EssentialsDevice BuildDevice(DeviceConfig dc)
        {
            Debug.LogMessage(Serilog.Events.LogEventLevel.Information, "[{key}] Building Password Client...", dc.Key);

            var config = dc.Properties.ToObject<PasswordClientConfig>();
            if (config == null)
            {
                Debug.LogMessage(Serilog.Events.LogEventLevel.Error, "[{key}] Failed to parse config", dc.Key);
                return null;
            }

            if (string.IsNullOrEmpty(config.ServerKey))
            {
                Debug.LogMessage(Serilog.Events.LogEventLevel.Error, "[{key}] ServerKey is required", dc.Key);
                return null;
            }

            return new PasswordClient(dc.Key, dc.Name, config);
        }
    }
}
