using Roses.SolarAPI.Services;
using System.Text.Json.Serialization;

namespace Roses.SolarAPI.Models.FoxCloud
{
    public class LoginResult
    {
        [JsonPropertyName("token")]
        public string? Token { get; set; }

        [JsonPropertyName("access")]
        public int Access { get; set; }

        [JsonPropertyName("user")]
        public string? User { get; set; }
    }

    public class LoginResponse : IFoxResponse
    {
        [JsonPropertyName("errno")]
        public int Errno { get; set; } = (int)FoxErrorNumber.NoResponse;

        [JsonPropertyName("result")]
        public LoginResult? Result { get; set; }
    }
}
