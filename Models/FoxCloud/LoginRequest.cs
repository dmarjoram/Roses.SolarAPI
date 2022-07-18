using System.Text.Json.Serialization;

namespace Roses.SolarAPI.Models.FoxCloud
{
    /// <summary>
    /// POST request body for https://www.foxesscloud.com/c/v0/user/login
    /// </summary>
    public class LoginRequest : IFoxRequest
    {
        [JsonPropertyName("user")]
        public string? User { get; set; }

        [JsonPropertyName("password")]
        public string? Password { get; set; }

        /// <summary>
        /// Validate the request parameters
        /// </summary>
        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(User))
            {
                throw new ArgumentNullException(nameof(LoginRequest), "No user name is provided.");
            }

            if (string.IsNullOrWhiteSpace(Password))
            {
                throw new ArgumentNullException(nameof(LoginRequest), "No MD5 password hash is provided.");
            }
        }
    }
}
