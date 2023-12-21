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

        [JsonIgnore]
        public string RequestUri => FoxCloudLoginUri;

		[JsonIgnore]
		public bool GetRequest => false;

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

        private const string FoxCloudLoginUri = "https://www.foxesscloud.com/c/v0/user/login";
        //private const string FoxCloudLoginUri = "https://www.foxesscloud.com/c/v0/errors/message";
        
    }
}
