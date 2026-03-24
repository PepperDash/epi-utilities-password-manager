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

- **Password Server** (`passwordServer`): Central device that manages user data and file persistence. Only one server is needed per system. Server can only be bridged once.
- **Password Client** (`passwordClient`): Bridge-enabled device for each touch panel. Multiple clients can connect to one server. Clients enforce access control for user management operations.

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
- **Access control** - users must be logged in with sufficient access level to manage users
- Store user credentials in JSON file with automatic persistence
- Create and delete users via bridge joins
- Validate user credentials (login/logout)
- **Input feedback signals** with EISC alignment (input/output on same join)
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
    "name": "Password Server",
    "type": "passwordServer",
    "group": "plugin",
    "properties": {
        "filePath": "PasswordManager/users.json",
        "saveDelayMs": 1000,
        "maskPasswords": true,
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
    "name": "Password Client TP1",
    "type": "passwordClient",
    "group": "plugin",
    "properties": {
        "serverKey": "pwServer",
        "maskPasswords": true,
        "clearInputsOnLogin": true,
        "requiredAccessLevelForAdmin": 2
    }
}
```

### Bridge Configuration

```json
{
    "key": "passwordClientBridge",
    "uid": 20,
    "name": "Password Client Bridge",
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

| Property        | Type   | Description                                                                                                         | Default |
| --------------- | ------ | ------------------------------------------------------------------------------------------------------------------- | ------- |
| `filePath`      | string | Path to JSON file for storing users (relative to `/USER/` on processor). If empty, users are stored in memory only. | (none)  |
| `saveDelayMs`   | long   | Debounce delay in milliseconds before saving changes to file                                                        | `1000`  |
| `maskPasswords` | bool   | When true, password feedback shows asterisks instead of actual passwords                                            | `true`  |
| `defaultUsers`  | array  | Array of default users to create when the file doesn't exist                                                        | `[]`    |

### Client Configuration Properties

| Property                      | Type   | Description                                                                                             | Default    |
| ----------------------------- | ------ | ------------------------------------------------------------------------------------------------------- | ---------- |
| `serverKey`                   | string | Key of the Password Server device to connect to                                                         | (required) |
| `maskPasswords`               | bool   | When true, password feedback shows asterisks instead of actual passwords                                | `true`     |
| `clearInputsOnLogin`          | bool   | When true, clears username/password/access inputs after successful login                                | `true`     |
| `requiredAccessLevelForAdmin` | int    | Minimum access level required to create, delete, or modify users. Set to 0 to allow any logged-in user. | `1`        |

## Bridge Join Map (Client)

Join numbers are organized by type with input/output pairs sharing the same join for EISC alignment.

### Digital Joins

| Join  | Name                     | Direction  | Description                               |
| ----- | ------------------------ | ---------- | ----------------------------------------- |
| 1     | ServerConnectedFb        | To SIMPL   | High when connected to server             |
| 2     | IsLoggedInFb             | To SIMPL   | High when a user is logged in             |
| 3     | CanManageUsersFb         | To SIMPL   | High when logged-in user can manage users |
| 4     | ValidateUserSuccessFb    | To SIMPL   | Pulse high when login succeeds            |
| 5     | CreateUserSuccessFb      | To SIMPL   | Pulse high when user creation succeeds    |
| 6     | DeleteUserSuccessFb      | To SIMPL   | Pulse high when user deletion succeeds    |
| 7     | UpdateUserSuccessFb      | To SIMPL   | Pulse high when user update succeeds      |
| 8     | SaveEnabledFb            | To SIMPL   | High when save button should be enabled   |
| 9     | HasChangesFb             | To SIMPL   | High when inputs differ from original     |
| 11    | ValidateUser             | From SIMPL | Pulse to validate credentials (login)     |
| 12    | Logout                   | From SIMPL | Pulse to logout current user              |
| 13    | CreateUser               | From SIMPL | Pulse to create a new user                |
| 14    | DeleteUser               | From SIMPL | Pulse to delete user by username input    |
| 15    | DeleteSelectedUser       | From SIMPL | Pulse to delete currently selected user   |
| 16    | UpdatePassword           | From SIMPL | Pulse to update password only             |
| 17    | UpdateAccess             | From SIMPL | Pulse to update access level only         |
| 18    | UpdateSelectedUser       | From SIMPL | Pulse to save changes to selected user    |
| 19    | LoadSelectedUserToInputs | From SIMPL | Pulse to load selected user into inputs   |
| 20    | ClearInputs              | From SIMPL | Pulse to clear all input fields           |
| 21    | UnmaskPasswordInput      | From SIMPL | **Hold high** to show unmasked password   |
| 22    | RefreshUsers             | From SIMPL | Pulse to reload users from server         |
| 23    | SelectNextUser           | From SIMPL | Pulse to select next user in list         |
| 24    | SelectPreviousUser       | From SIMPL | Pulse to select previous user in list     |
| 31-50 | UserListSelect[1-20]     | From SIMPL | Pulse to select user by list position     |
| 31-50 | UserListVisibleFb[1-20]  | To SIMPL   | High when user exists at list position    |

### Analog Joins

| Join | Name                  | Direction     | Description                             |
| ---- | --------------------- | ------------- | --------------------------------------- |
| 1    | UserCountFb           | To SIMPL      | Total number of users stored            |
| 2    | SelectedUserIndex     | To/From SIMPL | Currently selected user index (0-based) |
| 3    | SelectedUserAccessFb  | To SIMPL      | Selected user's access level            |
| 4    | ValidatedUserAccessFb | To SIMPL      | Access level of logged-in user          |
| 5    | AccessLevelInput      | From SIMPL    | Access level input (analog)             |

### Serial Joins

| Join  | Name                 | Direction  | Description                           |
| ----- | -------------------- | ---------- | ------------------------------------- |
| 1     | DeviceName           | To SIMPL   | Device name                           |
| 2     | StatusMessageFb      | To SIMPL   | Last operation status message         |
| 3     | ValidatedUsernameFb  | To SIMPL   | Username of logged-in user            |
| 4     | UsernameInput        | From SIMPL | Username input                        |
| 4     | UsernameInputFb      | To SIMPL   | Username input feedback               |
| 5     | PasswordInput        | From SIMPL | Password input                        |
| 5     | PasswordInputFb      | To SIMPL   | Password feedback (masked/unmasked)   |
| 6     | AccessInput          | From SIMPL | Access level input (string)           |
| 6     | AccessInputFb        | To SIMPL   | Access input feedback                 |
| 11    | UserListFb           | To SIMPL   | JSON array of all users               |
| 12    | EditingUsernameFb    | To SIMPL   | Original username being edited        |
| 13    | SelectedUsernameFb   | To SIMPL   | Selected user's username              |
| 14    | SelectedPasswordFb   | To SIMPL   | Selected user's password (masked)     |
| 15    | SelectedAccessFb     | To SIMPL   | Selected user's access level (string) |
| 31-50 | UserListItemFb[1-20] | To SIMPL   | Username at each list position        |

## Bridge Join Map (Server)

The server can also be bridged directly (once only) for direct access without a client.

### Digital Joins

| Join | Name                  | Direction  | Description                            |
| ---- | --------------------- | ---------- | -------------------------------------- |
| 1    | CreateUserSuccessFb   | To SIMPL   | Pulse high when user creation succeeds |
| 2    | DeleteUserSuccessFb   | To SIMPL   | Pulse high when user deletion succeeds |
| 3    | ValidateUserSuccessFb | To SIMPL   | Pulse high when validation succeeds    |
| 11   | ValidateUser          | From SIMPL | Pulse to validate credentials          |
| 12   | CreateUser            | From SIMPL | Pulse to create a new user             |
| 13   | DeleteUser            | From SIMPL | Pulse to delete user                   |
| 14   | UpdatePassword        | From SIMPL | Pulse to update password               |
| 15   | UpdateAccess          | From SIMPL | Pulse to update access level           |
| 16   | RefreshUsers          | From SIMPL | Pulse to reload users from file        |
| 17   | SelectNextUser        | From SIMPL | Pulse to select next user              |
| 18   | SelectPreviousUser    | From SIMPL | Pulse to select previous user          |

### Analog Joins

| Join | Name                  | Direction     | Description                             |
| ---- | --------------------- | ------------- | --------------------------------------- |
| 1    | UserCountFb           | To SIMPL      | Total number of users stored            |
| 2    | SelectedUserIndex     | To/From SIMPL | Currently selected user index (0-based) |
| 3    | SelectedUserAccessFb  | To SIMPL      | Selected user's access level            |
| 4    | ValidatedUserAccessFb | To SIMPL      | Access level of last validated user     |

### Serial Joins

| Join | Name                | Direction  | Description                     |
| ---- | ------------------- | ---------- | ------------------------------- |
| 1    | DeviceName          | To SIMPL   | Device name                     |
| 2    | StatusMessageFb     | To SIMPL   | Last operation status message   |
| 3    | ValidatedUsernameFb | To SIMPL   | Username of last validated user |
| 4    | UsernameInput       | From SIMPL | Username input                  |
| 5    | PasswordInput       | From SIMPL | Password input                  |
| 6    | AccessInput         | From SIMPL | Access level input              |
| 11   | UserListFb          | To SIMPL   | JSON array of all users         |
| 12   | SelectedUsernameFb  | To SIMPL   | Selected user's username        |
| 13   | SelectedPasswordFb  | To SIMPL   | Selected user's password        |
| 14   | SelectedAccessFb    | To SIMPL   | Selected user's access level    |

## UI Workflow Example

### Login Flow
1. User enters username via `UsernameInput` (S4)
2. User enters password via `PasswordInput` (S5)
3. On Login press, pulse `ValidateUser` (D11)
4. Check `ValidateUserSuccessFb` (D4) for success pulse
5. When logged in, `IsLoggedInFb` (D2) goes high
6. `ValidatedUsernameFb` (S3) shows logged-in username
7. `ValidatedUserAccessFb` (A4) shows access level
8. If access level >= `requiredAccessLevelForAdmin`, `CanManageUsersFb` (D3) goes high

### Logout Flow
1. Pulse `Logout` (D12)
2. `IsLoggedInFb` (D2) goes low
3. `CanManageUsersFb` (D3) goes low
4. `ValidatedUsernameFb` (S3) clears

### Manage Users List
1. Display users using `UserListItemFb[1-20]` (S31-50)
2. Show/hide list items using `UserListVisibleFb[1-20]` (D31-50)
3. When user taps a list item, pulse corresponding `UserListSelect[n]` (D31-50)
4. Navigate to edit page
5. **Note:** User must be logged in with sufficient access (`CanManageUsersFb` high) to modify users

### Add User
1. Check `CanManageUsersFb` (D3) - must be high to create users
2. User enters username via `UsernameInput` (S4)
3. User enters password via `PasswordInput` (S5)
4. Monitor `SaveEnabledFb` (D8) to enable/disable Save button
5. On Save press, pulse `CreateUser` (D13)
6. Check `CreateUserSuccessFb` (D5) and `StatusMessageFb` (S2) for result

### Edit User
1. Check `CanManageUsersFb` (D3) - must be high to edit users
2. Pulse `LoadSelectedUserToInputs` (D19) to populate input fields
3. `EditingUsernameFb` (S12) shows original username
4. User modifies `UsernameInput` and/or `PasswordInput`
5. Monitor `HasChangesFb` (D9) to detect unsaved changes
6. Monitor `SaveEnabledFb` (D8) to enable/disable Save button
7. On Save press, pulse `UpdateSelectedUser` (D18)
8. Check `UpdateUserSuccessFb` (D7) for result

### Delete User
1. Check `CanManageUsersFb` (D3) - must be high to delete users
2. From edit page, pulse `DeleteSelectedUser` (D15)
3. Check `DeleteUserSuccessFb` (D6) for result

### Password Visibility Toggle
- Hold `UnmaskPasswordInput` (D21) high to show actual password in `PasswordInputFb`
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

## Legacy Type Names

For backward compatibility, the server accepts the following type names:
- `passwordServer` (preferred)
- `passwordManager`
- `passwordmanager`
- `PasswordManager`

All resolve to the Password Server device.

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

| Level | Name  | Description                |
| ----- | ----- | -------------------------- |
| 0     | None  | No access                  |
| 1     | User  | Basic user access          |
| 2     | Tech  | Technician/advanced access |
| 3     | Admin | Administrator/full access  |

## Build Instructions

### NuGet Package

A NuGet package is automatically generated when the plugin is built. The package properties are defined in the .csproj file.