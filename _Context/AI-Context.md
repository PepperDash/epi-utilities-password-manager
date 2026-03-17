# AI Agent Context for PepperDash Essentials EPI Development

This document provides essential context for AI agents working on PepperDash Essentials Plugin Interfaces (EPIs). Copy this file to other EPI projects to maintain consistent AI assistance.

---

## Framework Overview

### What is Essentials?
PepperDash Essentials is a C# framework for Crestron control systems. EPIs (Essentials Plugin Interfaces) extend this framework to add device support or utility functionality.

### Technology Stack
| Component | Version/Technology |
|-----------|-------------------|
| Target Framework | .NET Framework 4.7.2 |
| Minimum Essentials | 2.12.1+ |
| Language | C# |
| Serialization | Newtonsoft.Json |
| Runtime | Crestron 4-Series processors |

---

## Project Structure

```
epi-[category]-[name]/
├── src/
│   ├── [Name]BridgeJoinMap.cs      # EISC bridge join definitions
│   ├── [Name]ConfigObject.cs       # Configuration properties  
│   ├── [Name]Device.cs             # Main device logic
│   ├── [Name]DeviceFactory.cs      # Device instantiation
│   ├── Directory.Build.props       # Build properties
│   └── [Name].4Series.csproj       # Project file
├── _Context/                        # AI context (this folder)
├── _Documentation/                  # Developer docs
├── README.md
└── LICENSE.md
```

---

## Naming Conventions

| Item | Convention | Example |
|------|------------|---------|
| Repository | `epi-[category]-[name]` | `epi-utilities-password-manager` |
| Namespace | `PepperDash.Essentials.Plugin.[Name]` | `PepperDash.Essentials.Plugin.PasswordManager` |
| Device class | `[Name]Device` or `[Name]LogicDevice` | `PasswordManagerLogicDevice` |
| Factory class | `[Name]Factory` or `[Name]DeviceFactory` | `PasswordManagerLogicDeviceFactory` |
| Config class | `[Name]Config` or `[Name]ConfigObject` | `PasswordManagerConfigObject` |
| JoinMap class | `[Name]BridgeJoinMap` | `PasswordManagerBridgeJoinMap` |

---

## Key Base Classes

### EssentialsDevice
Base class for devices that don't need bridge communication.
```csharp
public class MyDevice : EssentialsDevice
{
    public override bool CustomActivate() { ... }
}
```

### EssentialsBridgeableDevice
Base class for devices that communicate via EISC bridge to SIMPL.
```csharp
public class MyDevice : EssentialsBridgeableDevice
{
    public override void LinkToApi(BasicTriList trilist, uint joinStart, 
        string joinMapKey, EiscApiAdvanced bridge) { ... }
}
```

### EssentialsPluginDeviceFactory<T>
Factory base class for creating device instances.
```csharp
public class MyFactory : EssentialsPluginDeviceFactory<MyDevice>
{
    public MyFactory()
    {
        MinimumEssentialsFrameworkVersion = "2.12.1";
        TypeNames = new List<string> { "myDevice" };
    }
    
    public override EssentialsDevice BuildDevice(DeviceConfig dc) { ... }
}
```

---

## Bridge Join Types

### Join Capabilities
| Capability | Direction | Use Case |
|------------|-----------|----------|
| `FromSIMPL` | SIMPL → EPI | Button press, text input |
| `ToSIMPL` | EPI → SIMPL | Feedback, status display |
| `ToFromSIMPL` | Bidirectional | Value with feedback (e.g., volume) |

### Join Data Types
| Type | C# Signal Type | Range |
|------|---------------|-------|
| Digital | `BooleanInput`/`BooleanOutput` | true/false |
| Analog | `UShortInput`/`UShortOutput` | 0-65535 |
| Serial | `StringInput`/`StringOutput` | string |

### Wiring Patterns in LinkToApi

```csharp
// Digital input (SIMPL → EPI) - trigger on rising edge
trilist.SetSigTrueAction(joinMap.MyJoin.JoinNumber, () => MyMethod());

// Digital input (SIMPL → EPI) - track state
trilist.SetBoolSigAction(joinMap.MyJoin.JoinNumber, value => MyBoolMethod(value));

// Digital output (EPI → SIMPL)
MyBoolFeedback.LinkInputSig(trilist.BooleanInput[joinMap.MyFb.JoinNumber]);

// Analog input (SIMPL → EPI)
trilist.SetUShortSigAction(joinMap.MyJoin.JoinNumber, value => MyIntMethod(value));

// Analog output (EPI → SIMPL)
MyIntFeedback.LinkInputSig(trilist.UShortInput[joinMap.MyFb.JoinNumber]);

// Serial input (SIMPL → EPI)
trilist.SetStringSigAction(joinMap.MyJoin.JoinNumber, value => MyStringMethod(value));

// Serial output (EPI → SIMPL) - static
trilist.SetString(joinMap.MyJoin.JoinNumber, "value");

// Serial output (EPI → SIMPL) - feedback
MyStringFeedback.LinkInputSig(trilist.StringInput[joinMap.MyFb.JoinNumber]);
```

---

## Feedback Classes

```csharp
// Bool feedback
public BoolFeedback MyFeedback { get; private set; }
MyFeedback = new BoolFeedback("Key", () => _myBoolValue);

// Int feedback  
public IntFeedback MyFeedback { get; private set; }
MyFeedback = new IntFeedback("Key", () => _myIntValue);

// String feedback
public StringFeedback MyFeedback { get; private set; }
MyFeedback = new StringFeedback("Key", () => _myStringValue);

// Fire update when value changes
MyFeedback.FireUpdate();
```

---

## Common Patterns

### Thread-Safe Data Access
```csharp
private readonly CCriticalSection _lock = new CCriticalSection();

public void SafeOperation()
{
    _lock.Enter();
    try
    {
        // Thread-safe code here
    }
    finally
    {
        _lock.Leave();
    }
}
```

### Debounced File Saves
```csharp
private CTimer _saveTimer;

public MyDevice()
{
    _saveTimer = new CTimer(_ => SaveToFile(), Timeout.Infinite);
}

private void ScheduleSave()
{
    _saveTimer.Reset(1000); // 1 second debounce
}
```

### File Operations
```csharp
using Crestron.SimplSharp.CrestronIO;

var fullPath = Path.Combine(Global.FilePathPrefix, "relative/path.json");

// Read
var json = File.ReadToEnd(fullPath, System.Text.Encoding.UTF8);

// Write (use temp file for safety)
var tempPath = fullPath + ".tmp";
using (var stream = new FileStream(tempPath, FileMode.Create))
using (var writer = new StreamWriter(stream))
{
    writer.Write(json);
}
File.Delete(fullPath);
File.Move(tempPath, fullPath);
```

### Logging (Modern)
```csharp
using PepperDash.Core.Logging;

this.LogInformation("Message with {param}", value);
this.LogDebug("Debug message");
this.LogError("Error: {error}", ex.Message);
```

---

## Known Limitations & Gotchas

### Console Command Help Length
Console command help text **must be under 79 characters** or it throws an exception.

```csharp
// BAD - too long
"mycommand list | add [name] [value] | delete [name] | update [name] [value]"

// GOOD - shortened  
"list | add [n v] | del [n] | upd [n v]"
```

### Touch Panel Password Fields
Touch panel password-type text fields have built-in "show last char then mask" behavior. The EPI cannot fully control masking on these fields.

**Solution**: Use a regular text field for display, separate hidden field for input.

### Feedback Constructor Deprecation
The parameterless feedback constructors are deprecated. Use the constructor with a key:

```csharp
// Deprecated
new BoolFeedback(() => _value);

// Preferred
new BoolFeedback("UniqueKey", () => _value);
```

### Success Feedback Pulse Pattern
When success feedback signals are used for operations that can succeed multiple times in a row (e.g., login validation), SIMPL won't see repeated successes if the signal stays HIGH.

**Problem**: If `_validateSuccess` stays `true`, a second successful login won't cause a signal transition.

**Solution**: Use a CTimer to auto-reset success flags after a brief delay:

```csharp
private CTimer _successResetTimer;
private const long SuccessPulseDurationMs = 500;

private void SetOperationResult(bool success, string message)
{
    // Stop pending reset
    _successResetTimer?.Stop();
    _successResetTimer?.Dispose();

    _success = success;
    SuccessFeedback.FireUpdate();

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

### Debug.Console Deprecation
`Debug.Console()` is deprecated. Use extension methods instead:

```csharp
// Deprecated
Debug.Console(0, this, "Message");

// Preferred
this.LogInformation("Message");
```

---

## Server/Client Architecture

For multi-panel support where multiple UIs access the same data:

### Server Device
- Manages data storage and file persistence
- Provides thread-safe data access
- Fires events when data changes
- Type: `[name]Server`

### Client Device  
- References server via `serverKey` in config
- Maintains per-client input buffers
- Handles EISC bridge communication
- Type: `[name]Client`

```json
{
    "key": "pwServer",
    "type": "passwordManagerServer",
    "properties": { "filePath": "users.json" }
},
{
    "key": "pwClient-tp1", 
    "type": "passwordManagerClient",
    "properties": { "serverKey": "pwServer" }
}
```

---

## Configuration JSON Structure

### Device Entry
```json
{
    "key": "uniqueKey",
    "uid": 100,
    "name": "Display Name",
    "type": "deviceType",
    "group": "plugin",
    "properties": {
        "propertyName": "value"
    }
}
```

### Bridge Entry
```json
{
    "key": "bridgeKey",
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
            { "deviceKey": "myDevice", "joinStart": 1 }
        ]
    }
}
```

---

## Useful Console Commands

```bash
devlist              # List all devices
devjson [key]        # Show device config JSON
devprops [key]       # Show device properties
getplugins           # List loaded plugins
```

---

## Reference Repositories

| Repository | Purpose |
|------------|---------|
| [EssentialsPluginTemplate](https://github.com/PepperDash/EssentialsPluginTemplate) | Starting template |
| [epi-utilities-customvalues](https://github.com/PepperDash/epi-utilities-customvalues) | Data storage pattern |
| [Essentials](https://github.com/PepperDash/Essentials) | Core framework |

---

## Quick Reference: JoinMap Template

```csharp
[JoinName("MyJoin")]
public JoinDataComplete MyJoin = new JoinDataComplete(
    new JoinData { JoinNumber = 1, JoinSpan = 1 },
    new JoinMetadata
    {
        Description = "Description here",
        JoinCapabilities = eJoinCapabilities.FromSIMPL,
        JoinType = eJoinType.Digital
    });
```

---

*Last updated: March 2026*
