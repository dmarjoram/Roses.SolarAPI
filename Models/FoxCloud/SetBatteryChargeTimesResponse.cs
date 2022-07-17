using System.Text.Json.Serialization;

namespace Roses.SolarAPI.Models.FoxCloud
{
    public class SetBatteryChargeTimesResponse
    {
        /// <summary>
        /// 41808 means token is invalid
        /// </summary>
        [JsonPropertyName("errno")]
        public int Errno { get; set; }

        [JsonPropertyName("result")]
        public object? Result { get; set; }
    }
}
