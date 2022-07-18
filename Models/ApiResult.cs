using System.Text.Json.Serialization;

namespace Roses.SolarAPI.Models
{
    public class ApiResult
    {
        [JsonPropertyName("resultCode")]
        public string? ResultCode { get; set; }
    }
}
