using System.Collections.Generic;
using PepperDash.Core;
using PepperDash.Core.Logging;
using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.Plugin.PasswordManager
{
  /// <summary>
  /// Factory for creating Password Manager logic devices
  /// </summary>
  public class PasswordManagerLogicDeviceFactory : EssentialsPluginDeviceFactory<PasswordManagerLogicDevice>
  {
    /// <summary>
    /// Password Manager Device Factory constructor
    /// </summary>
    public PasswordManagerLogicDeviceFactory()
    {
      // Set the minimum Essentials Framework Version
      MinimumEssentialsFrameworkVersion = "2.12.1";

      // TypeNames that will build an instance of this device
      TypeNames = new List<string>() { "passwordManager", "passwordmanager", "PasswordManager" };
    }

    /// <summary>
    /// Builds and returns an instance of PasswordManagerLogicDevice
    /// </summary>
    /// <param name="dc">Device configuration</param>
    /// <returns>Password Manager device or null</returns>
    public override EssentialsDevice BuildDevice(PepperDash.Essentials.Core.Config.DeviceConfig dc)
    {
      Debug.LogDebug("[{key}] Factory attempting to create Password Manager from type: {type}", dc.Key, dc.Type);

      // Get the plugin device properties configuration object
      var propertiesConfig = dc.Properties.ToObject<PasswordManagerConfig>();
      if (propertiesConfig == null)
      {
        Debug.LogError("[{key}] Factory: Failed to read properties config for {name}", dc.Key, dc.Name);
        return null;
      }

      return new PasswordManagerLogicDevice(dc.Key, dc.Name, propertiesConfig, dc);
    }
  }
}

