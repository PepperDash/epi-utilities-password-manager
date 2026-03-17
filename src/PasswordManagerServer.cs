using System;
using System.Collections.Generic;
using System.Linq;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronIO;
using Newtonsoft.Json;
using PepperDash.Core;
using PepperDash.Core.Logging;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;

namespace PepperDash.Essentials.Plugin.PasswordManager
{
    /// <summary>
    /// Password Manager Server Device
    /// </summary>
    /// <remarks>
    /// Manages user credentials stored in a JSON file. Multiple clients can connect to this server.
    /// </remarks>
    public class PasswordManagerServer : EssentialsDevice
    {
        #region Fields

        private readonly PasswordManagerServerConfig _config;
        private readonly CCriticalSection _fileLock = new CCriticalSection();
        private readonly CCriticalSection _usersLock = new CCriticalSection();
        private readonly CTimer _saveTimer;
        private List<UserCredential> _users;

        #endregion

        #region Events

        /// <summary>
        /// Fired when the user list changes (add, delete, update)
        /// </summary>
        public event EventHandler<EventArgs> UsersChanged;

        #endregion

        #region Properties

        /// <summary>
        /// Gets a read-only copy of the current users list
        /// </summary>
        public IReadOnlyList<UserCredential> Users
        {
            get
            {
                _usersLock.Enter();
                try
                {
                    return _users.ToList().AsReadOnly();
                }
                finally
                {
                    _usersLock.Leave();
                }
            }
        }

        /// <summary>
        /// Gets the count of users
        /// </summary>
        public int UserCount
        {
            get
            {
                _usersLock.Enter();
                try
                {
                    return _users?.Count ?? 0;
                }
                finally
                {
                    _usersLock.Leave();
                }
            }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor for Password Manager Server
        /// </summary>
        public PasswordManagerServer(string key, string name, PasswordManagerServerConfig config)
            : base(key, name)
        {
            this.LogInformation("Constructing Password Manager Server: {name}", name);

            _config = config;
            _users = new List<UserCredential>();

            // Initialize save timer
            var saveDelay = _config.SaveDelayMs > 0 ? _config.SaveDelayMs : 1000;
            _saveTimer = new CTimer(_ => SaveUsersToFile(), Timeout.Infinite);

            // Register console commands
            CrestronConsole.AddNewConsoleCommand(ConsoleCommand,
                string.Format("pwmgr-{0}", key),
                "list | add [u p a] | del [u] | val [u p]",
                ConsoleAccessLevelEnum.AccessOperator);
        }

        #endregion

        #region EssentialsDevice Overrides

        /// <summary>
        /// Initializes the device
        /// </summary>
        public override bool CustomActivate()
        {
            this.LogDebug("Activating Password Manager Server");

            LoadUsersFromFile();

            return base.CustomActivate();
        }

        #endregion

        #region File Operations

        private void LoadUsersFromFile()
        {
            if (string.IsNullOrEmpty(_config.FilePath))
            {
                this.LogInformation("No file path specified, using in-memory storage only");
                _usersLock.Enter();
                try
                {
                    _users = _config.DefaultUsers != null
                        ? new List<UserCredential>(_config.DefaultUsers)
                        : new List<UserCredential>();
                }
                finally
                {
                    _usersLock.Leave();
                }
                OnUsersChanged();
                return;
            }

            var fullPath = Path.Combine(Global.FilePathPrefix, _config.FilePath);

            _fileLock.Enter();
            try
            {
                if (File.Exists(fullPath))
                {
                    this.LogDebug("Loading users from file: {path}", fullPath);
                    var json = File.ReadToEnd(fullPath, System.Text.Encoding.UTF8);

                    _usersLock.Enter();
                    try
                    {
                        _users = JsonConvert.DeserializeObject<List<UserCredential>>(json)
                            ?? new List<UserCredential>();
                    }
                    finally
                    {
                        _usersLock.Leave();
                    }

                    this.LogInformation("Loaded {count} users from file", UserCount);
                }
                else
                {
                    this.LogInformation("File does not exist, creating with default users: {path}", fullPath);

                    _usersLock.Enter();
                    try
                    {
                        _users = _config.DefaultUsers != null
                            ? new List<UserCredential>(_config.DefaultUsers)
                            : new List<UserCredential>();
                    }
                    finally
                    {
                        _usersLock.Leave();
                    }

                    // Ensure directory exists
                    var directory = Path.GetDirectoryName(fullPath);
                    if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                    {
                        Directory.Create(directory);
                    }

                    SaveUsersToFileImmediate();
                }
            }
            catch (Exception ex)
            {
                this.LogError("Error loading users from file: {error}", ex.Message);
                _usersLock.Enter();
                try
                {
                    _users = new List<UserCredential>();
                }
                finally
                {
                    _usersLock.Leave();
                }
            }
            finally
            {
                _fileLock.Leave();
            }

            OnUsersChanged();
        }

        private void ScheduleSave()
        {
            if (string.IsNullOrEmpty(_config.FilePath))
            {
                return;
            }

            var saveDelay = _config.SaveDelayMs > 0 ? _config.SaveDelayMs : 1000;
            _saveTimer.Reset(saveDelay);
        }

        private void SaveUsersToFile()
        {
            if (string.IsNullOrEmpty(_config.FilePath))
            {
                return;
            }

            SaveUsersToFileImmediate();
        }

        private void SaveUsersToFileImmediate()
        {
            if (string.IsNullOrEmpty(_config.FilePath))
            {
                return;
            }

            var fullPath = Path.Combine(Global.FilePathPrefix, _config.FilePath);
            var tempPath = fullPath + ".tmp";

            _fileLock.Enter();
            try
            {
                string json;
                _usersLock.Enter();
                try
                {
                    json = JsonConvert.SerializeObject(_users, Formatting.Indented);
                }
                finally
                {
                    _usersLock.Leave();
                }

                // Write to temp file first
                using (var stream = new FileStream(tempPath, FileMode.Create, FileAccess.Write))
                using (var writer = new StreamWriter(stream))
                {
                    writer.Write(json);
                    writer.Flush();
                    stream.Flush();
                }

                // Replace original file
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                }
                File.Move(tempPath, fullPath);

                this.LogDebug("Users saved to file: {path}", fullPath);
            }
            catch (Exception ex)
            {
                this.LogError("Error saving users to file: {error}", ex.Message);
            }
            finally
            {
                // Clean up temp file if it still exists
                try
                {
                    if (File.Exists(tempPath))
                    {
                        File.Delete(tempPath);
                    }
                }
                catch { }

                _fileLock.Leave();
            }
        }

        #endregion

        #region User Management Operations

        /// <summary>
        /// Creates a new user with the given credentials
        /// </summary>
        /// <param name="username">Username</param>
        /// <param name="password">Password</param>
        /// <param name="access">Access level</param>
        /// <param name="errorMessage">Error message if creation fails</param>
        /// <returns>True if user was created successfully</returns>
        public bool CreateUser(string username, string password, int access, out string errorMessage)
        {
            errorMessage = string.Empty;

            if (string.IsNullOrEmpty(username))
            {
                errorMessage = "Username cannot be empty";
                return false;
            }

            _usersLock.Enter();
            try
            {
                // Check if user already exists
                if (_users.Any(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase)))
                {
                    errorMessage = string.Format("User '{0}' already exists", username);
                    return false;
                }

                var newUser = new UserCredential(username, password, access);
                _users.Add(newUser);
            }
            finally
            {
                _usersLock.Leave();
            }

            ScheduleSave();
            OnUsersChanged();

            this.LogInformation("User '{username}' created with access level {access}", username, access);
            return true;
        }

        /// <summary>
        /// Deletes a user by username
        /// </summary>
        /// <param name="username">Username to delete</param>
        /// <param name="errorMessage">Error message if deletion fails</param>
        /// <returns>True if user was deleted successfully</returns>
        public bool DeleteUser(string username, out string errorMessage)
        {
            errorMessage = string.Empty;

            if (string.IsNullOrEmpty(username))
            {
                errorMessage = "Username cannot be empty";
                return false;
            }

            _usersLock.Enter();
            try
            {
                var user = _users.FirstOrDefault(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
                if (user == null)
                {
                    errorMessage = string.Format("User '{0}' not found", username);
                    return false;
                }

                _users.Remove(user);
            }
            finally
            {
                _usersLock.Leave();
            }

            ScheduleSave();
            OnUsersChanged();

            this.LogInformation("User '{username}' deleted", username);
            return true;
        }

        /// <summary>
        /// Validates user credentials
        /// </summary>
        /// <param name="username">Username</param>
        /// <param name="password">Password</param>
        /// <param name="accessLevel">Access level of the validated user (output)</param>
        /// <param name="errorMessage">Error message if validation fails</param>
        /// <returns>True if credentials are valid</returns>
        public bool ValidateUser(string username, string password, out int accessLevel, out string errorMessage)
        {
            accessLevel = 0;
            errorMessage = string.Empty;

            if (string.IsNullOrEmpty(username))
            {
                errorMessage = "Username cannot be empty";
                return false;
            }

            _usersLock.Enter();
            try
            {
                var user = _users.FirstOrDefault(u =>
                    u.Username.Equals(username, StringComparison.OrdinalIgnoreCase) &&
                    u.Password == password);

                if (user != null)
                {
                    accessLevel = user.Access;
                    this.LogDebug("User '{username}' validated successfully (Access: {access})", username, accessLevel);
                    return true;
                }
                else
                {
                    errorMessage = string.Format("Invalid credentials for user '{0}'", username);
                    this.LogDebug("Validation failed for user '{username}'", username);
                    return false;
                }
            }
            finally
            {
                _usersLock.Leave();
            }
        }

        /// <summary>
        /// Updates a user's password
        /// </summary>
        /// <param name="username">Username</param>
        /// <param name="newPassword">New password</param>
        /// <param name="errorMessage">Error message if update fails</param>
        /// <returns>True if password was updated</returns>
        public bool UpdateUserPassword(string username, string newPassword, out string errorMessage)
        {
            errorMessage = string.Empty;

            _usersLock.Enter();
            try
            {
                var user = _users.FirstOrDefault(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
                if (user == null)
                {
                    errorMessage = string.Format("User '{0}' not found", username);
                    return false;
                }

                user.Password = newPassword;
            }
            finally
            {
                _usersLock.Leave();
            }

            ScheduleSave();
            OnUsersChanged();

            this.LogInformation("Password updated for user '{username}'", username);
            return true;
        }

        /// <summary>
        /// Updates a user's access level
        /// </summary>
        /// <param name="username">Username</param>
        /// <param name="newAccess">New access level</param>
        /// <param name="errorMessage">Error message if update fails</param>
        /// <returns>True if access was updated</returns>
        public bool UpdateUserAccess(string username, int newAccess, out string errorMessage)
        {
            errorMessage = string.Empty;

            _usersLock.Enter();
            try
            {
                var user = _users.FirstOrDefault(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
                if (user == null)
                {
                    errorMessage = string.Format("User '{0}' not found", username);
                    return false;
                }

                user.Access = newAccess;
            }
            finally
            {
                _usersLock.Leave();
            }

            ScheduleSave();
            OnUsersChanged();

            this.LogInformation("Access updated for user '{username}' to {access}", username, newAccess);
            return true;
        }

        /// <summary>
        /// Updates a user's username
        /// </summary>
        /// <param name="oldUsername">Current username</param>
        /// <param name="newUsername">New username</param>
        /// <param name="errorMessage">Error message if update fails</param>
        /// <returns>True if username was updated</returns>
        public bool UpdateUsername(string oldUsername, string newUsername, out string errorMessage)
        {
            errorMessage = string.Empty;

            if (string.IsNullOrEmpty(newUsername))
            {
                errorMessage = "New username cannot be empty";
                return false;
            }

            _usersLock.Enter();
            try
            {
                var user = _users.FirstOrDefault(u => u.Username.Equals(oldUsername, StringComparison.OrdinalIgnoreCase));
                if (user == null)
                {
                    errorMessage = string.Format("User '{0}' not found", oldUsername);
                    return false;
                }

                // Check if new username already exists (unless it's the same user)
                if (!oldUsername.Equals(newUsername, StringComparison.OrdinalIgnoreCase) &&
                    _users.Any(u => u.Username.Equals(newUsername, StringComparison.OrdinalIgnoreCase)))
                {
                    errorMessage = string.Format("Username '{0}' already exists", newUsername);
                    return false;
                }

                user.Username = newUsername;
            }
            finally
            {
                _usersLock.Leave();
            }

            ScheduleSave();
            OnUsersChanged();

            this.LogInformation("Username updated from '{oldUsername}' to '{newUsername}'", oldUsername, newUsername);
            return true;
        }

        /// <summary>
        /// Gets a user by index
        /// </summary>
        /// <param name="index">Index (0-based)</param>
        /// <returns>User credential or null if not found</returns>
        public UserCredential GetUserByIndex(int index)
        {
            _usersLock.Enter();
            try
            {
                if (index < 0 || index >= _users.Count)
                {
                    return null;
                }
                return _users[index];
            }
            finally
            {
                _usersLock.Leave();
            }
        }

        /// <summary>
        /// Reloads users from the file
        /// </summary>
        public void RefreshUsers()
        {
            LoadUsersFromFile();
        }

        #endregion

        #region Helper Methods

        private void OnUsersChanged()
        {
            var handler = UsersChanged;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        #endregion

        #region Console Command

        private void ConsoleCommand(string command)
        {
            if (string.IsNullOrEmpty(command))
            {
                this.LogInformation("Server Commands: list | add [u p a] | del [u] | val [u p]");
                return;
            }

            var args = command.Split(' ');
            var cmd = args[0].ToLower();

            switch (cmd)
            {
                case "list":
                    this.LogInformation("Users ({count}):", UserCount);
                    foreach (var user in Users)
                    {
                        this.LogInformation("  {username} (Access: {access})", user.Username, user.Access);
                    }
                    break;

                case "add":
                    if (args.Length < 4)
                    {
                        this.LogWarning("Usage: add [username] [password] [access]");
                        return;
                    }
                    int accessLevel;
                    if (!int.TryParse(args[3], out accessLevel))
                    {
                        this.LogWarning("Invalid access level: {level}", args[3]);
                        return;
                    }
                    string createError;
                    if (CreateUser(args[1], args[2], accessLevel, out createError))
                    {
                        this.LogInformation("User created successfully");
                    }
                    else
                    {
                        this.LogWarning("Failed: {error}", createError);
                    }
                    break;

                case "del":
                    if (args.Length < 2)
                    {
                        this.LogWarning("Usage: del [username]");
                        return;
                    }
                    string deleteError;
                    if (DeleteUser(args[1], out deleteError))
                    {
                        this.LogInformation("User deleted successfully");
                    }
                    else
                    {
                        this.LogWarning("Failed: {error}", deleteError);
                    }
                    break;

                case "val":
                    if (args.Length < 3)
                    {
                        this.LogWarning("Usage: val [username] [password]");
                        return;
                    }
                    int access;
                    string valError;
                    if (ValidateUser(args[1], args[2], out access, out valError))
                    {
                        this.LogInformation("Valid! Access level: {access}", access);
                    }
                    else
                    {
                        this.LogWarning("Invalid: {error}", valError);
                    }
                    break;

                default:
                    this.LogWarning("Unknown command: {cmd}", cmd);
                    break;
            }
        }

        #endregion
    }
}
