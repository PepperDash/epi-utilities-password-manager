# AI Agent Prompts for Creating Essentials EPIs

This document contains reusable prompts for creating PepperDash Essentials Plugin Interfaces (EPIs) using AI agents in VS Code.

---

## Agent Information

### Recommended Agent
- **Agent**: GitHub Copilot (Agent Mode in VS Code)
- **Model**: Claude (Sonnet or Opus recommended)
- **Extension**: GitHub Copilot Chat

### How to Use Agent Mode
1. Open VS Code with the GitHub Copilot extension installed
2. Open the Copilot Chat panel (Ctrl+Shift+I or click the Copilot icon)
3. Click the "Agent" button or use `@workspace` to give the agent context
4. Paste your prompt and let the agent work

---

## Base Prompts

### 1. Create a New Logic-Based EPI (No External Communication)

Use this prompt when creating an EPI that performs internal logic without communicating with external devices.

```
I want to build an Essentials EPI that will be used as a [DESCRIPTION OF FUNCTIONALITY]. 
The [DATA TYPE] will be stored in a JSON file. Each [ITEM] has the following properties: 
[LIST PROPERTIES]. 

The EPI should allow me to [LIST OPERATIONS] through the EssentialsPluginTemplateBridgeJoinMap.

You can review this repo as reference: https://github.com/PepperDash/epi-utilities-customvalues

Also review the files in the current project - they might help you understand the EPI structure.
```

**Example (Password Manager):**
```
I want to build an Essentials EPI that will be used as a user & password manager. 
The users will be stored in a JSON file. Each user has the following properties: 
username, password, access. 

The EPI should allow me to create and delete users through the EssentialsPluginTemplateBridgeJoinMap.

You can review this repo as reference: https://github.com/PepperDash/epi-utilities-customvalues

Also review the files in the current project - they might help you understand the EPI structure.
```

---

### 2. Create a Device Control EPI (With External Communication)

Use this prompt when creating an EPI that communicates with external hardware via TCP/IP, RS-232, etc.

```
I want to build an Essentials EPI to control a [DEVICE TYPE] from [MANUFACTURER].

The device communicates via [TCP/IP | RS-232 | SSH | etc] and uses [ASCII | HEX | JSON] 
commands.

The API documentation is: [LINK OR ATTACH FILE]

Required functionality:
- [LIST FEATURES: e.g., Power On/Off, Volume Control, Input Selection]

I need bridge joins for:
- Digital: [LIST DIGITAL CONTROLS]
- Analog: [LIST ANALOG CONTROLS]  
- Serial: [LIST SERIAL CONTROLS]

You can review this repo as reference: https://github.com/PepperDash/EssentialsPluginTemplate

Also review the files in the current project for the EPI structure.
```

---

### 3. Add Features to an Existing EPI

```
I need to add the following features to this EPI:

1. [FEATURE 1 DESCRIPTION]
2. [FEATURE 2 DESCRIPTION]
3. [FEATURE 3 DESCRIPTION]

For each feature, please:
- Add the necessary bridge joins to the JoinMap
- Implement the logic in the device class
- Update any feedbacks as needed
- Add console commands if appropriate

Please review the existing code structure before making changes.
```

---

### 4. Implement Server/Client Architecture

Use this when multiple touch panels need to access the same data simultaneously.

```
I need to split this EPI into a server/client paradigm to support multiple touch panels.

Requirements:
- Server device: Manages data storage, file persistence, and business logic
- Client devices: Handle UI bridge communication, reference the server via config key
- Multiple clients should connect to one server

For the client, I also need:
- Input feedback signals (send current input values back to SIMPL)
- [Any additional client-specific features]

Please create:
1. Server config and device classes
2. Client config, device, and bridge join map classes  
3. Separate factory classes for each type
4. Update the README with the new architecture
```

---

### 5. Fix Common EPI Issues

**Console Command Help Too Long:**
```
I'm getting this exception: "Help is too long for command [COMMAND]. 
Please restrict to 79 bytes in length"

Please shorten the console command help text while keeping it informative.
```

**Missing Bridge Joins:**
```
I need to add the following bridge joins to this EPI:

Digital Joins:
- Join [NUMBER]: [DESCRIPTION] ([FromSIMPL | ToSIMPL | ToFromSIMPL])

Analog Joins:
- Join [NUMBER]: [DESCRIPTION] ([FromSIMPL | ToSIMPL | ToFromSIMPL])

Serial Joins:
- Join [NUMBER]: [DESCRIPTION] ([FromSIMPL | ToSIMPL | ToFromSIMPL])

Update the JoinMap and wire them in LinkToApi.
```

---

### 6. Create Configuration Documentation

```
Please create or update the README.md for this EPI with:

1. Overview of what the EPI does
2. Configuration example (JSON format)
3. Complete bridge join map table with all joins documented
4. Console commands if any
5. Any special notes or requirements

Use the existing EPI code to extract accurate information.
```

---

## Tips for Better Results

### Do's
- ✅ Attach the `src` folder to give the agent context
- ✅ Reference existing PepperDash repos as examples
- ✅ Be specific about property names and data types
- ✅ List all required operations upfront
- ✅ Mention if you need file persistence or in-memory only
- ✅ Point the agent to the `_Context` folder if one exists
- ✅ Specify if multi-panel support is needed (server/client architecture)

### Don'ts
- ❌ Don't ask for features without specifying join behavior
- ❌ Don't assume the agent knows Crestron-specific limitations (like 79-byte help strings)
- ❌ Don't skip testing - always build and test on a processor
- ❌ Don't forget about touch panel behaviors (e.g., password fields do their own masking)

---

## Touch Panel Considerations

### Password Field Behavior

Touch panel password text fields have built-in masking behavior:
- They show the last character briefly while typing, then mask it
- This happens regardless of what string you send from the EPI

**Solution**: Use two separate fields in VTPro:
1. **Hidden input field**: Captures keystrokes, sends to EPI
2. **Display field**: Regular text (not password), receives feedback from EPI

This gives the EPI full control over what characters are displayed.

---

## Reference Repositories

| Repository | Use Case |
|------------|----------|
| [EssentialsPluginTemplate](https://github.com/PepperDash/EssentialsPluginTemplate) | Base template for all EPIs |
| [epi-utilities-customvalues](https://github.com/PepperDash/epi-utilities-customvalues) | JSON data storage patterns |
| [Essentials](https://github.com/PepperDash/Essentials) | Core framework reference |

---

## Post-Generation Checklist

After the agent creates your EPI, verify:

- [ ] Project builds without errors
- [ ] Namespace is correct in all files (`PepperDash.Essentials.Plugin.[YourPlugin]`)
- [ ] TypeNames in factory match your desired config type
- [ ] JoinMap class name is updated (not template name)
- [ ] Console command help is under 79 characters
- [ ] README is updated with accurate documentation
- [ ] Configuration example is valid JSON
- [ ] All unused template files are removed

---

## Using Context Files

For complex or ongoing projects, maintain a `_Context` folder with:

1. **AI-Context.md**: Technical context for AI agents including:
   - Framework details (Essentials version, .NET version)
   - Common patterns and conventions
   - Known limitations and workarounds
   - Architecture decisions

2. Point the AI agent to this file at the start of conversations:
   ```
   Please review the _Context/AI-Context.md file for project-specific context.
   ```

This ensures consistent behavior across sessions and when context is copied to other projects.
