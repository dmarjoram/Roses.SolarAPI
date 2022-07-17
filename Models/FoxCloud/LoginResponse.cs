using System.Text.Json.Serialization;

namespace Roses.SolarAPI.Models.FoxCloud
{
    public class Result
    {
        [JsonPropertyName("token")]
        public string? Token { get; set; }

        [JsonPropertyName("access")]
        public int Access { get; set; }

        [JsonPropertyName("user")]
        public string? User { get; set; }
    }

    public class LoginResponse
    {
        [JsonPropertyName("errno")]
        public int Errno { get; set; }

        [JsonPropertyName("result")]
        public Result? Result { get; set; }
    }
}
