![PepperDash Essentials Plugin Logo](/images/essentials-plugin-blue.png)

# Password Manager EPI (c) 2025

## License

Provided under MIT license

## Overview

This Essentials Plugin Interface (EPI) provides user and password management functionality. Users are stored in a JSON file with the following properties:
- **username**: The user's unique identifier
- **password**: The user's password
- **access**: The user's access level (integer)

The plugin uses a **Server/Client architecture** to support multiple touch panels accessing the same user database simultaneously.

## Architecture

### Server/Client Model

The plugin consists of two device types:

- **Password Manager Server** (`passwordManagerServer`): Central device that manages user data and file persistence. Only one server is needed per system.
- **Password Manager Client** (`passwordManagerClient`): Bridge-enabled device for each touch panel. Multiple clients can connect to one server.

```
┌─────────────────┐     ┌─────────────────┐     ┌─────────────────┐
│   Touch Panel 1 │     │   Touch Panel 2 │     │   Touch Panel 3 │
│   (EISC Bridge) │     │   (EISC Bridge) │     │   (EISC Bridge) │
└────────┬────────┘     └────────┬────────┘     └────────┬────────┘
         │                       │                       │
         ▼                       ▼                       ▼
┌─────────────────┐     ┌─────────────────┐     ┌─────────────────┐
│  pwClient-tp1   │     │  pwClient-tp2   │     │  pwClient-tp3   │
│    (Client)     │     │    (Client)     │     │    (Client)     │
└────────┬────────┘     └────────┬────────┘     └────────┬────────┘
         │                       │                       │
         └───────────────────────┼───────────────────────┘
                                 │
                                 ▼
                    ┌────────────────────────┐
                    │      pwServer          │
                    │      (Server)          │
                    │                        │
                    │  ┌──────────────────┐  │
                    │  │  users.json      │  │
                    │  └──────────────────┘  │
                    └────────────────────────┘
```

## Features

- **Server/Client architecture** for multi-panel support
- Store user credentials in JSON file with automatic persistence
- Create and delete users via bridge joins
- Validate user credentials (login)
- **Input feedback signals** (UsernameInputFb, PasswordInputFb, AccessInputFb)
- **Unmask password signal** - hold digital join high to reveal password
- **Clear inputs on login** - automatically clear input fields after successful login
- Navigate through user list with next/previous selection
- Update user passwords and access levels
- Masked password display option for security
- Console commands for administration
- Default users seeding on first run

## Dependencies

The [Essentials](https://github.com/PepperDash/Essentials) libraries are required (v2.12.1+). They are referenced via NuGet.

## Configuration

### Server Configuration

```json
{
    "key": "pwServer",
    "uid": 10,
    "name": "Password Manager Server",
    "type": "passwordManagerServer",
    "group": "plugin",
    "properties": {
        "filePath": "PasswordManager/users.json",
        "saveDelayMs": 1000,
        "defaultUsers": [
            {
                "username": "admin",
                "password": "admin123",
                "access": 3
            },
            {
                "username": "tech",
                "password": "tech123",
                "access": 2
            },
            {
                "username": "user",
                "password": "user123",
                "access": 1
            }
        ]
    }
}
```

### Client Configuration

```json
{
    "key": "pwClient-tp1",
    "uid": 11,
    "name": "Password Manager Client TP1",
    "type": "passwordManagerClient",
    "group": "plugin",
    "properties": {
        "serverKey": "pwServer",
        "maskPasswords": true,
        "clearInputsOnLogin": true
    }
}
```

### Bridge Configuration

```json
{
    "key": "passwordManagerBridge",
    "uid": 20,
    "name": "Password Manager Bridge",
    "group": "api",
    "type": "eiscApiAdvanced",
    "properties": {
        "control": {
            "tcpSshProperties": {
                "address": "127.0.0.2",
                "port": 0
            },
            "ipid": "B1",
            "method": "ipidTcp"
        },
        "devices": [
            {
                "deviceKey": "pwClient-tp1",
                "joinStart": 1
            }
        ]
    }
}
```

### Server Configuration Properties

| Property | Type | Description | Default |
|----------|------|-------------|---------|
| `filePath` | string | Path to JSON file for storing users (relative to program directory). If empty, users are stored in memory only. | (none) |
| `saveDelayMs` | long | Debounce delay in milliseconds before saving changes to file | `1000` |
| `defaultUsers` | array | Array of default users to create when the file doesn't exist | `[]` |

### Client Configuration Properties

| Property | Type | Description | Default |
|----------|------|-------------|---------|
| `serverKey` | string | Key of the Password Manager Server device to connect to | (required) |
| `maskPasswords` | bool | When true, password feedback shows asterisks instead of actual passwords | `true` |
| `clearInputsOnLogin` | bool | When true, clears username/password/access inputs after successful login | `false` |

## Bridge Join Map (Client)

Join numbers are organized into logical groups with room for expansion:

### Digital Joins - Feedbacks (1-7)

| Join | Name | Direction | Description |
|------|------|-----------|-------------|
| 1 | ServerConnectedFb | To SIMPL | High when connected to server |
| 2 | ValidateUserSuccessFb | To SIMPL | High when validation/login succeeds |
| 3 | CreateUserSuccessFb | To SIMPL | High when user creation succeeds |
| 4 | DeleteUserSuccessFb | To SIMPL | High when user deletion succeeds |
| 5 | UpdateUserSuccessFb | To SIMPL | High when user update succeeds |
| 6 | SaveEnabledFb | To SIMPL | High when save button should be enabled |
| 7 | HasChangesFb | To SIMPL | High when inputs differ from original |

### Digital Joins - Actions (11-20)

| Join | Name | Direction | Description |
|------|------|-----------|-------------|
| 11 | ClearInputs | From SIMPL | Pulse to clear all input fields |
| 12 | UnmaskPasswordInput | From SIMPL | **Hold high** to show unmasked password |
| 13 | ValidateUser | From SIMPL | Pulse to validate credentials (login) |
| 14 | CreateUser | From SIMPL | Pulse to create a new user |
| 15 | DeleteUser | From SIMPL | Pulse to delete user by username input |
| 16 | DeleteSelectedUser | From SIMPL | Pulse to delete currently selected user |
| 17 | UpdatePassword | From SIMPL | Pulse to update password only |
| 18 | UpdateAccess | From SIMPL | Pulse to update access level only |
| 19 | LoadSelectedUserToInputs | From SIMPL | Pulse to load selected user into inputs |
| 20 | UpdateSelectedUser | From SIMPL | Pulse to save changes to selected user |

### Digital Joins - Navigation (25-27)

| Join | Name | Direction | Description |
|------|------|-----------|-------------|
| 25 | RefreshUsers | From SIMPL | Pulse to reload users from server |
| 26 | SelectNextUser | From SIMPL | Pulse to select next user in list |
| 27 | SelectPreviousUser | From SIMPL | Pulse to select previous user in list |

### Digital Joins - User List (31-80)

| Join | Name | Direction | Description |
|------|------|-----------|-------------|
| 31-50 | UserListSelect[1-20] | From SIMPL | Pulse to select user by list position |
| 61-80 | UserListVisibleFb[1-20] | To SIMPL | High when user exists at list position |

### Analog Joins (1-5)

| Join | Name | Direction | Description |
|------|------|-----------|-------------|
| 1 | UserCountFb | To SIMPL | Total number of users stored |
| 2 | SelectedUserIndex | To/From SIMPL | Currently selected user index (0-based) |
| 3 | SelectedUserAccessFb | To SIMPL | Selected user's access level |
| 4 | ValidatedUserAccessFb | To SIMPL | Access level of last validated user |
| 5 | AccessLevelInput | From SIMPL | Access level input (analog) |

### Serial Joins - Device/Status (1-6)

| Join | Name | Direction | Description |
|------|------|-----------|-------------|
| 1 | DeviceName | To SIMPL | Device name |
| 2 | StatusMessageFb | To SIMPL | Last operation status message |
| 3 | ValidatedUsernameFb | To SIMPL | Username of last validated user |
| 4 | UsernameInputFb | To SIMPL | Username input feedback |
| 5 | PasswordInputFb | To SIMPL | Password feedback (masked/unmasked) |
| 6 | AccessInputFb | To SIMPL | Access input feedback |

### Serial Joins - Inputs (7-9)

| Join | Name | Direction | Description |
|------|------|-----------|-------------|
| 7 | UsernameInput | From SIMPL | Username input |
| 8 | PasswordInput | From SIMPL | Password input |
| 9 | AccessInput | From SIMPL | Access level input (string) |

### Serial Joins - List/Edit (11-15)

| Join | Name | Direction | Description |
|------|------|-----------|-------------|
| 11 | UserListFb | To SIMPL | JSON array of all users |
| 12 | EditingUsernameFb | To SIMPL | Original username being edited |
| 13 | SelectedUsernameFb | To SIMPL | Selected user's username |
| 14 | SelectedPasswordFb | To SIMPL | Selected user's password (masked) |
| 15 | SelectedAccessFb | To SIMPL | Selected user's access level (string) |

### Serial Joins - User List (31-50)

| Join | Name | Direction | Description |
|------|------|-----------|-------------|
| 31-50 | UserListItemFb[1-20] | To SIMPL | Username at each list position |

## UI Workflow Example

### Manage Users List
1. Display users using `UserListItemFb[1-20]` (S31-50)
2. Show/hide list items using `UserListVisibleFb[1-20]` (D61-80)
3. When user taps a list item, pulse corresponding `UserListSelect[n]` (D31-50)
4. Navigate to edit page

### Add User
1. User enters username via `UsernameInput` (S7)
2. User enters password via `PasswordInput` (S8)
3. Monitor `SaveEnabledFb` (D6) to enable/disable Save button
4. On Save press, pulse `CreateUser` (D14)
5. Check `CreateUserSuccessFb` (D3) and `StatusMessageFb` (S2) for result

### Edit User
1. Pulse `LoadSelectedUserToInputs` (D19) to populate input fields
2. `EditingUsernameFb` (S12) shows original username
3. User modifies `UsernameInput` and/or `PasswordInput`
4. Monitor `HasChangesFb` (D7) to detect unsaved changes
5. Monitor `SaveEnabledFb` (D6) to enable/disable Save button
6. On Save press, pulse `UpdateSelectedUser` (D20)
7. Check `UpdateUserSuccessFb` (D5) for result

### Delete User
1. From edit page, pulse `DeleteSelectedUser` (D16)
2. Check `DeleteUserSuccessFb` (D4) for result

### Password Visibility Toggle
- Hold `UnmaskPasswordInput` (D12) high to show actual password in `PasswordInputFb`
- Release to show masked password

## Console Commands

### Server Console Command

The server registers a console command `pwmgr-{key}` with the following subcommands:

```
pwmgr-pwServer list                              - List all users
pwmgr-pwServer add [username] [password] [access] - Add a new user
pwmgr-pwServer del [username]                    - Delete a user
pwmgr-pwServer val [username] [password]         - Validate credentials
```

## Legacy Single-Device Mode

The original single-device mode (`passwordManager` type) is still available for backward compatibility:

```json
{
    "key": "passwordManager-1",
    "type": "passwordManager",
    "properties": {
        "filePath": "PasswordManager/users.json",
        "maskPasswords": true,
        "saveDelayMs": 1000,
        "defaultUsers": [...]
    }
}
```

## User File Format

The users are stored in a JSON file with the following format:

```json
[
    {
        "username": "admin",
        "password": "admin123",
        "access": 3
    },
    {
        "username": "tech",
        "password": "tech123",
        "access": 2
    }
]
```

## Access Levels

Access levels are integers that can be used to define different permission tiers. Suggested values:

| Level | Name | Description |
|-------|------|-------------|
| 0 | None | No access |
| 1 | User | Basic user access |
| 2 | Tech | Technician/advanced access |
| 3 | Admin | Administrator/full access |

## Build Instructions

### NuGet Package

A NuGet package is automatically generated when the plugin is built. The package properties are defined in the .csproj file.