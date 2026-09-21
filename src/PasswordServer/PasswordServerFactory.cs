using System.Collections.Generic;
using PepperDash.Core;
using PepperDash.Core.Logging;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;

namespace PepperDash.Essentials.Plugin.Password.Server
{
	/// <summary>
	/// Password Server Factory
	/// </summary>
	/// <remarks>
	/// Creates Password Server devices.
	/// Type keys: "passwordServer", "passwordManager"
	/// </remarks>
	public class PasswordServerFactory : EssentialsPluginDeviceFactory<PasswordServer>
	{
		/// <summary>
		/// Constructor
		/// </summary>
		public PasswordServerFactory()
		{
			// Accept both new and legacy type names
			TypeNames = new List<string> { "passwordServer", "passwordManager" };
			MinimumEssentialsFrameworkVersion = "2.12.1";
		}

		/// <summary>
		/// Build device from config
		/// </summary>
		public override EssentialsDevice BuildDevice(DeviceConfig dc)
		{
			Debug.LogMessage(Serilog.Events.LogEventLevel.Information, "[{key}] Building Password Server...", dc.Key);

			var config = dc.Properties.ToObject<PasswordServerConfig>();
			if (config == null)
			{
				Debug.LogMessage(Serilog.Events.LogEventLevel.Error, "[{key}] Failed to parse config", dc.Key);
				return null;
			}

			return new PasswordServer(dc.Key, dc.Name, config);
		}
	}
}
