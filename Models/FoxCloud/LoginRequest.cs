using System.Text.Json.Serialization;

namespace Roses.SolarAPI.Models.FoxCloud
{
    /// <summary>
    /// POST request body for https://www.foxesscloud.com/c/v0/user/login
    /// </summary>
    public class LoginRequest
    {
        [JsonPropertyName("user")]
        public string? User { get; set; }

        [JsonPropertyName("password")]
        public string? Password { get; set; }
    }
}
