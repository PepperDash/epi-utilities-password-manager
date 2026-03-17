# Essentials Plugin Interface (EPI) Developer Guide

A comprehensive guide for new developers to create PepperDash Essentials plugins.

---

## Table of Contents

1. [Introduction](#introduction)
2. [Prerequisites](#prerequisites)
3. [Understanding the EPI Architecture](#understanding-the-epi-architecture)
4. [Getting Started](#getting-started)
5. [Creating Your First EPI](#creating-your-first-epi)
6. [The Bridge Join Map](#the-bridge-join-map)
7. [Configuration Objects](#configuration-objects)
8. [The Device Class](#the-device-class)
9. [The Factory Class](#the-factory-class)
10. [Testing Your EPI](#testing-your-epi)
11. [Common Patterns](#common-patterns)
12. [Troubleshooting](#troubleshooting)
13. [Best Practices](#best-practices)

---

## Introduction

### What is an EPI?

An **Essentials Plugin Interface (EPI)** is a modular component that extends the PepperDash Essentials framework. EPIs allow you to:

- Add support for new device types
- Create custom utility functions
- Store and manage data
- Bridge between Essentials and SIMPL Windows programs

### When Do You Need an EPI?

- Controlling a device not already supported by Essentials
- Creating reusable utility functions (timers, schedulers, data managers)
- Storing configuration data that persists across reboots
- Custom integrations with third-party systems

---

## Prerequisites

### Required Software
- Visual Studio 2022 (or VS Code with C# extension)
- .NET Framework 4.7.2 SDK
- Git
- NuGet CLI (optional but recommended)

### Required Knowledge
- Basic C# programming
- Understanding of JSON configuration files
- Familiarity with Crestron processors (helpful but not required)

### Required Access
- GitHub account
- Access to PepperDash repositories

---

## Understanding the EPI Architecture

### Key Components

Every EPI consists of these core files:

```
src/
├── [Name]BridgeJoinMap.cs      # Defines EISC bridge joins
├── [Name]ConfigObject.cs        # Configuration properties
├── [Name]Device.cs              # Main device logic
├── [Name]DeviceFactory.cs       # Creates device instances
└── [Name].csproj                # Project file
```

### How It Works

```
┌─────────────────┐     ┌─────────────────┐     ┌─────────────────┐
│  Essentials     │────▶│    Your EPI     │────▶│  SIMPL Bridge   │
│  Config JSON    │     │    Device       │     │  (EISC)         │
└─────────────────┘     └─────────────────┘     └─────────────────┘
        │                       │                       │
        ▼                       ▼                       ▼
   Device Config          Logic & State           Join Numbers
   (type, properties)     (your code)             (D1, A1, S1...)
```

1. **Essentials** reads the config JSON and finds your device type
2. **Factory** creates an instance of your device class
3. **Device** initializes and connects to the EISC bridge
4. **Bridge** provides joins that SIMPL Windows can use

---

## Getting Started

### Step 1: Fork the Template

1. Go to [EssentialsPluginTemplate](https://github.com/PepperDash/EssentialsPluginTemplate)
2. Click "Use this template" → "Create a new repository"
3. Name your repository: `epi-[category]-[name]`
   - Example: `epi-display-samsung`, `epi-utilities-scheduler`

### Step 2: Clone Your Repository

```bash
git clone https://github.com/YourOrg/epi-your-plugin.git
cd epi-your-plugin
```

### Step 3: Install Dependencies

```bash
cd src
nuget restore
# Or use the GetPackages.BAT file
```

### Step 4: Open in Visual Studio

Open the `.sln` file in Visual Studio.

---

## Creating Your First EPI

Let's create a simple "Room Counter" EPI that tracks occupancy.

### Step 1: Plan Your Features

Before coding, define:
- **What data will it store?** → Current count, max capacity
- **What operations are needed?** → Increment, decrement, reset
- **What feedback goes to SIMPL?** → Current count, is-at-capacity flag

### Step 2: Update the Project

1. Rename files to match your device:
   - `PasswordManagerBridgeJoinMap.cs` → `RoomCounterBridgeJoinMap.cs`
   - `PasswordManagerConfigObject.cs` → `RoomCounterConfig.cs`
   - etc.

2. Update the `.csproj` file:
```xml
<RootNamespace>PepperDash.Essentials.Plugin.RoomCounter</RootNamespace>
<AssemblyTitle>PepperDash.Essentials.Plugin.RoomCounter</AssemblyTitle>
```

3. Update namespaces in all files:
```csharp
namespace PepperDash.Essentials.Plugin.RoomCounter
```

---

## The Bridge Join Map

The Bridge Join Map defines how your EPI communicates with SIMPL Windows.

### File: `RoomCounterBridgeJoinMap.cs`

```csharp
using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.Plugin.RoomCounter
{
    public class RoomCounterBridgeJoinMap : JoinMapBaseAdvanced
    {
        #region Digital Joins

        // Digital joins are for on/off, trigger, or boolean values
        
        [JoinName("Increment")]
        public JoinDataComplete Increment = new JoinDataComplete(
            new JoinData { JoinNumber = 1, JoinSpan = 1 },
            new JoinMetadata
            {
                Description = "Increment counter (pulse)",
                JoinCapabilities = eJoinCapabilities.FromSIMPL,  // SIMPL sends to EPI
                JoinType = eJoinType.Digital
            });

        [JoinName("Decrement")]
        public JoinDataComplete Decrement = new JoinDataComplete(
            new JoinData { JoinNumber = 2, JoinSpan = 1 },
            new JoinMetadata
            {
                Description = "Decrement counter (pulse)",
                JoinCapabilities = eJoinCapabilities.FromSIMPL,
                JoinType = eJoinType.Digital
            });

        [JoinName("Reset")]
        public JoinDataComplete Reset = new JoinDataComplete(
            new JoinData { JoinNumber = 3, JoinSpan = 1 },
            new JoinMetadata
            {
                Description = "Reset to zero (pulse)",
                JoinCapabilities = eJoinCapabilities.FromSIMPL,
                JoinType = eJoinType.Digital
            });

        [JoinName("AtCapacityFb")]
        public JoinDataComplete AtCapacityFb = new JoinDataComplete(
            new JoinData { JoinNumber = 4, JoinSpan = 1 },
            new JoinMetadata
            {
                Description = "At capacity feedback",
                JoinCapabilities = eJoinCapabilities.ToSIMPL,  // EPI sends to SIMPL
                JoinType = eJoinType.Digital
            });

        #endregion

        #region Analog Joins

        // Analog joins are for numeric values (0-65535)

        [JoinName("CurrentCountFb")]
        public JoinDataComplete CurrentCountFb = new JoinDataComplete(
            new JoinData { JoinNumber = 1, JoinSpan = 1 },
            new JoinMetadata
            {
                Description = "Current count feedback",
                JoinCapabilities = eJoinCapabilities.ToSIMPL,
                JoinType = eJoinType.Analog
            });

        [JoinName("MaxCapacityFb")]
        public JoinDataComplete MaxCapacityFb = new JoinDataComplete(
            new JoinData { JoinNumber = 2, JoinSpan = 1 },
            new JoinMetadata
            {
                Description = "Max capacity feedback",
                JoinCapabilities = eJoinCapabilities.ToSIMPL,
                JoinType = eJoinType.Analog
            });

        #endregion

        #region Serial Joins

        // Serial joins are for text strings

        [JoinName("DeviceName")]
        public JoinDataComplete DeviceName = new JoinDataComplete(
            new JoinData { JoinNumber = 1, JoinSpan = 1 },
            new JoinMetadata
            {
                Description = "Device name",
                JoinCapabilities = eJoinCapabilities.ToSIMPL,
                JoinType = eJoinType.Serial
            });

        #endregion

        // Constructor - MUST call base with joinStart and the type
        public RoomCounterBridgeJoinMap(uint joinStart)
            : base(joinStart, typeof(RoomCounterBridgeJoinMap))
        {
        }
    }
}
```

### Join Capabilities Explained

| Capability | Direction | Example Use |
|------------|-----------|-------------|
| `FromSIMPL` | SIMPL → EPI | Button press, text input |
| `ToSIMPL` | EPI → SIMPL | Status feedback, display text |
| `ToFromSIMPL` | Both ways | Volume level with feedback |

---

## Configuration Objects

The configuration object defines what properties can be set in the Essentials config JSON.

### File: `RoomCounterConfig.cs`

```csharp
using Newtonsoft.Json;
using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.Plugin.RoomCounter
{
    [ConfigSnippet("\"properties\":{\"maxCapacity\":100}")]
    public class RoomCounterConfig
    {
        /// <summary>
        /// Maximum room capacity
        /// </summary>
        [JsonProperty("maxCapacity")]
        public int MaxCapacity { get; set; }

        /// <summary>
        /// Starting count value
        /// </summary>
        [JsonProperty("initialCount")]
        public int InitialCount { get; set; }

        /// <summary>
        /// Constructor with defaults
        /// </summary>
        public RoomCounterConfig()
        {
            MaxCapacity = 100;
            InitialCount = 0;
        }
    }
}
```

### Corresponding Config JSON

```json
{
    "key": "roomCounter-1",
    "name": "Main Room Counter",
    "type": "roomCounter",
    "group": "plugin",
    "properties": {
        "maxCapacity": 50,
        "initialCount": 0
    }
}
```

---

## The Device Class

The device class contains your main logic.

### File: `RoomCounterDevice.cs`

```csharp
using Crestron.SimplSharpPro.DeviceSupport;
using PepperDash.Core;
using PepperDash.Core.Logging;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Bridges;

namespace PepperDash.Essentials.Plugin.RoomCounter
{
    public class RoomCounterDevice : EssentialsBridgeableDevice
    {
        // Store the config
        private readonly RoomCounterConfig _config;
        
        // Current state
        private int _currentCount;

        // Feedbacks - these send values to SIMPL
        public IntFeedback CurrentCountFeedback { get; private set; }
        public IntFeedback MaxCapacityFeedback { get; private set; }
        public BoolFeedback AtCapacityFeedback { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public RoomCounterDevice(string key, string name, RoomCounterConfig config)
            : base(key, name)
        {
            _config = config;
            _currentCount = config.InitialCount;

            // Initialize feedbacks
            CurrentCountFeedback = new IntFeedback("CurrentCount", () => _currentCount);
            MaxCapacityFeedback = new IntFeedback("MaxCapacity", () => _config.MaxCapacity);
            AtCapacityFeedback = new BoolFeedback("AtCapacity", () => _currentCount >= _config.MaxCapacity);

            this.LogInformation("Room Counter constructed: {name}", name);
        }

        /// <summary>
        /// Called when device activates
        /// </summary>
        public override bool CustomActivate()
        {
            this.LogDebug("Room Counter activating");
            return base.CustomActivate();
        }

        /// <summary>
        /// Increment the counter
        /// </summary>
        public void Increment()
        {
            if (_currentCount < _config.MaxCapacity)
            {
                _currentCount++;
                UpdateFeedbacks();
                this.LogDebug("Count incremented to {count}", _currentCount);
            }
        }

        /// <summary>
        /// Decrement the counter
        /// </summary>
        public void Decrement()
        {
            if (_currentCount > 0)
            {
                _currentCount--;
                UpdateFeedbacks();
                this.LogDebug("Count decremented to {count}", _currentCount);
            }
        }

        /// <summary>
        /// Reset to zero
        /// </summary>
        public void Reset()
        {
            _currentCount = 0;
            UpdateFeedbacks();
            this.LogDebug("Count reset to zero");
        }

        private void UpdateFeedbacks()
        {
            CurrentCountFeedback.FireUpdate();
            AtCapacityFeedback.FireUpdate();
        }

        /// <summary>
        /// Links this device to the EISC bridge
        /// </summary>
        public override void LinkToApi(BasicTriList trilist, uint joinStart, 
            string joinMapKey, EiscApiAdvanced bridge)
        {
            // Create join map instance
            var joinMap = new RoomCounterBridgeJoinMap(joinStart);

            // Add to bridge collection
            if (bridge != null)
            {
                bridge.AddJoinMap(Key, joinMap);
            }

            // Check for custom join overrides
            var customJoins = JoinMapHelper.TryGetJoinMapAdvancedForDevice(joinMapKey);
            if (customJoins != null)
            {
                joinMap.SetCustomJoinData(customJoins);
            }

            this.LogDebug("Linking to Trilist {id}", trilist.ID.ToString("X"));

            // Wire up DIGITAL inputs (SIMPL → EPI)
            trilist.SetSigTrueAction(joinMap.Increment.JoinNumber, () => Increment());
            trilist.SetSigTrueAction(joinMap.Decrement.JoinNumber, () => Decrement());
            trilist.SetSigTrueAction(joinMap.Reset.JoinNumber, () => Reset());

            // Wire up DIGITAL outputs (EPI → SIMPL)
            AtCapacityFeedback.LinkInputSig(trilist.BooleanInput[joinMap.AtCapacityFb.JoinNumber]);

            // Wire up ANALOG outputs (EPI → SIMPL)
            CurrentCountFeedback.LinkInputSig(trilist.UShortInput[joinMap.CurrentCountFb.JoinNumber]);
            MaxCapacityFeedback.LinkInputSig(trilist.UShortInput[joinMap.MaxCapacityFb.JoinNumber]);

            // Wire up SERIAL outputs (EPI → SIMPL)
            trilist.SetString(joinMap.DeviceName.JoinNumber, Name);

            // Handle EISC coming online
            trilist.OnlineStatusChange += (o, a) =>
            {
                if (!a.DeviceOnLine) return;

                trilist.SetString(joinMap.DeviceName.JoinNumber, Name);
                UpdateFeedbacks();
                MaxCapacityFeedback.FireUpdate();
            };

            // Initial update
            UpdateFeedbacks();
            MaxCapacityFeedback.FireUpdate();
        }
    }
}
```

---

## The Factory Class

The factory tells Essentials how to create your device.

### File: `RoomCounterFactory.cs`

```csharp
using System.Collections.Generic;
using PepperDash.Core;
using PepperDash.Essentials.Core;

namespace PepperDash.Essentials.Plugin.RoomCounter
{
    public class RoomCounterFactory : EssentialsPluginDeviceFactory<RoomCounterDevice>
    {
        public RoomCounterFactory()
        {
            // Minimum Essentials version required
            MinimumEssentialsFrameworkVersion = "2.12.1";

            // Type names that trigger this factory
            // These match the "type" field in config JSON
            TypeNames = new List<string>() 
            { 
                "roomCounter", 
                "roomcounter", 
                "RoomCounter" 
            };
        }

        public override EssentialsDevice BuildDevice(
            PepperDash.Essentials.Core.Config.DeviceConfig dc)
        {
            Debug.LogDebug("[{key}] Factory building RoomCounter", dc.Key);

            // Parse the properties from config
            var config = dc.Properties.ToObject<RoomCounterConfig>();
            if (config == null)
            {
                Debug.LogError("[{key}] Failed to parse config", dc.Key);
                return null;
            }

            // Create and return the device
            return new RoomCounterDevice(dc.Key, dc.Name, config);
        }
    }
}
```

---

## Testing Your EPI

### 1. Build the Project

```bash
cd src
dotnet build [ProjectName].4Series.csproj
```

### 2. Deploy to Processor

Copy the `.dll` file from `bin/Debug/net472/` to the processor's plugins folder:
- 4-Series: `/opt/crestron/virtualcontrol/RunningPrograms/[AppNumber]/plugins/`

### 3. Create Config Entry

Add your device to the Essentials config:

```json
{
    "devices": [
        {
            "key": "roomCounter-1",
            "uid": 100,
            "name": "Lobby Counter",
            "type": "roomCounter",
            "group": "plugin",
            "properties": {
                "maxCapacity": 50
            }
        }
    ]
}
```

### 4. Create Bridge Entry

```json
{
    "key": "roomCounterBridge",
    "uid": 101,
    "name": "Room Counter Bridge",
    "group": "api",
    "type": "eiscApiAdvanced",
    "properties": {
        "control": {
            "ipid": "B1",
            "method": "ipidTcp",
            "tcpSshProperties": {
                "address": "127.0.0.2",
                "port": 0
            }
        },
        "devices": [
            {
                "deviceKey": "roomCounter-1",
                "joinStart": 1
            }
        ]
    }
}
```

### 5. Test with Console Commands

```
devjson roomCounter-1
devprops roomCounter-1
```

---

## Common Patterns

### File Persistence

Store data to a JSON file:

```csharp
using Crestron.SimplSharp.CrestronIO;
using Newtonsoft.Json;

// Save
var json = JsonConvert.SerializeObject(data, Formatting.Indented);
File.WriteAllText(Path.Combine(Global.FilePathPrefix, "mydata.json"), json);

// Load
if (File.Exists(fullPath))
{
    var json = File.ReadToEnd(fullPath, System.Text.Encoding.UTF8);
    data = JsonConvert.DeserializeObject<MyDataType>(json);
}
```

### Console Commands

Add debug commands (help text must be under 79 characters!):

```csharp
CrestronConsole.AddNewConsoleCommand(
    HandleConsole,           // Method to call
    "mycmd",                 // Command name
    "mycmd help | status",   // Help text (< 79 chars!)
    ConsoleAccessLevelEnum.AccessOperator
);

private void HandleConsole(string args)
{
    var parts = args.Split(' ');
    switch (parts[0].ToLower())
    {
        case "help":
            this.LogInformation("Available: help, status");
            break;
        case "status":
            this.LogInformation("Count: {0}", _count);
            break;
    }
}
```

### Debounced Saves

Prevent excessive file writes:

```csharp
private CTimer _saveTimer;

public MyDevice()
{
    _saveTimer = new CTimer(_ => SaveToFile(), Timeout.Infinite);
}

private void ScheduleSave()
{
    _saveTimer.Reset(1000); // Wait 1 second before saving
}

private void OnDataChanged()
{
    UpdateFeedbacks();
    ScheduleSave(); // Multiple rapid changes = one save
}
```

### Server/Client Architecture

When multiple touch panels need to access the same data:

```csharp
// Server - manages data and persistence
public class MyServer : EssentialsDevice
{
    private readonly CCriticalSection _lock = new CCriticalSection();
    private List<MyData> _data;
    
    public event EventHandler<EventArgs> DataChanged;
    
    // Thread-safe data access
    public IReadOnlyList<MyData> Data
    {
        get
        {
            _lock.Enter();
            try { return _data.ToList().AsReadOnly(); }
            finally { _lock.Leave(); }
        }
    }
    
    public bool AddItem(MyData item, out string error)
    {
        _lock.Enter();
        try
        {
            _data.Add(item);
            ScheduleSave();
            OnDataChanged();
            error = null;
            return true;
        }
        finally { _lock.Leave(); }
    }
    
    private void OnDataChanged()
    {
        DataChanged?.Invoke(this, EventArgs.Empty);
    }
}

// Client - handles UI and references server
public class MyClient : EssentialsBridgeableDevice
{
    private MyServer _server;
    private string _inputBuffer;  // Per-client input state
    
    public override bool CustomActivate()
    {
        // Find server by key from config
        _server = DeviceManager.GetDeviceForKey(_config.ServerKey) as MyServer;
        _server.DataChanged += (s, e) => RefreshFromServer();
        return base.CustomActivate();
    }
}
```

**Config structure:**
```json
{
    "key": "myServer",
    "type": "myPluginServer",
    "properties": { "filePath": "data.json" }
},
{
    "key": "myClient-tp1",
    "type": "myPluginClient", 
    "properties": { "serverKey": "myServer" }
}
```

### Input Feedback Pattern

For UI responsiveness, echo input values back to SIMPL:

```csharp
// Input buffer
private string _usernameInput = string.Empty;

// Feedback
public StringFeedback UsernameInputFeedback { get; private set; }

public MyDevice()
{
    UsernameInputFeedback = new StringFeedback("UsernameInput", () => _usernameInput);
}

public void SetUsernameInput(string value)
{
    _usernameInput = value ?? string.Empty;
    UsernameInputFeedback.FireUpdate();  // Echo back immediately
}

// In LinkToApi
trilist.SetStringSigAction(joinMap.UsernameInput.JoinNumber, SetUsernameInput);
UsernameInputFeedback.LinkInputSig(trilist.StringInput[joinMap.UsernameInputFb.JoinNumber]);
```

---

## Troubleshooting

### Build Errors

| Error | Solution |
|-------|----------|
| "Type not found" | Check `using` statements, verify NuGet packages |
| "Namespace mismatch" | Ensure namespace matches in all files |
| "Assembly reference" | Run `nuget restore` |

### Runtime Errors

| Error | Solution |
|-------|----------|
| "Help too long" | Console help must be < 79 characters |
| "Factory not found" | Check `TypeNames` matches config `type` |
| "Config parse failed" | Verify JSON property names match `[JsonProperty]` attributes |

### Common Console Commands

```bash
# List all devices
devlist

# Show device JSON
devjson [deviceKey]

# Show device properties
devprops [deviceKey]

# Check loaded plugins
getplugins
```

---

## Best Practices

### Naming Conventions
- Repository: `epi-[category]-[name]` (e.g., `epi-display-lg`)
- Namespace: `PepperDash.Essentials.Plugin.[Name]`
- Class names: PascalCase, descriptive

### Code Organization
- Keep one class per file
- Group related joins in the JoinMap
- Use regions to organize large classes
- Comment public methods with XML docs

### Error Handling
- Always validate config properties
- Use try-catch around file operations
- Log errors with meaningful messages
- Provide feedback to SIMPL when operations fail

### Performance
- Debounce file saves
- Don't fire feedbacks unnecessarily
- Use critical sections for thread safety
- Dispose of timers properly

### Success Feedback Pulse Pattern

When using digital feedback signals to indicate operation success (e.g., "ValidateUserSuccessFb"), ensure SIMPL sees repeated successes by creating proper pulses:

**Problem**: If `_success` stays `true`, a second successful operation won't cause a signal transition—SIMPL won't detect it.

**Solution**: Auto-reset success flags after a brief delay:

```csharp
private CTimer _successResetTimer;
private const long SuccessPulseDurationMs = 500;

private void SetOperationResult(bool success, string message)
{
    // Stop any pending reset
    if (_successResetTimer != null)
    {
        _successResetTimer.Stop();
        _successResetTimer.Dispose();
        _successResetTimer = null;
    }

    _success = success;
    _statusMessage = message;

    SuccessFeedback.FireUpdate();
    StatusMessageFeedback.FireUpdate();

    // Auto-reset after delay to create proper pulse
    if (success)
    {
        _successResetTimer = new CTimer(_ =>
        {
            _success = false;
            SuccessFeedback.FireUpdate();
        }, SuccessPulseDurationMs);
    }
}
```

This ensures:
1. Success signal goes HIGH immediately
2. After 500ms, signal goes LOW automatically
3. Next success can trigger another HIGH transition

---

## Next Steps

1. Clone the template repository
2. Follow the steps above to create your device
3. Test thoroughly on a processor
4. Document your join map in the README
5. Create a sample config file

For additional help, review existing EPIs in the PepperDash GitHub organization.
