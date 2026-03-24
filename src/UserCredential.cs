using Newtonsoft.Json;

namespace PepperDash.Essentials.Plugin.PasswordManager
{
    /// <summary>
    /// Represents a user credential with username, password, and access level
    /// </summary>
    public class UserCredential
    {
        /// <summary>
        /// The username for this credential
        /// </summary>
        [JsonProperty("username")]
        public string Username { get; set; }

        /// <summary>
        /// The password for this credential
        /// </summary>
        [JsonProperty("password")]
        public string Password { get; set; }

        /// <summary>
        /// The access level for this user (e.g., 0=None, 1=User, 2=Tech, 3=Admin)
        /// </summary>
        [JsonProperty("access")]
        public int Access { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public UserCredential()
        {
            Username = string.Empty;
            Password = string.Empty;
            Access = 0;
        }

        /// <summary>
        /// Constructor with parameters
        /// </summary>
        /// <param name="username">Username</param>
        /// <param name="password">Password</param>
        /// <param name="access">Access level</param>
        public UserCredential(string username, string password, int access)
        {
            Username = username;
            Password = password;
            Access = access;
        }
    }
}
