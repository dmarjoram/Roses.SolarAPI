using System.Text.Json.Serialization;

namespace Roses.SolarAPI.Models.FoxCloud
{
    /// <summary>
    /// POST body request for https://www.foxesscloud.com/c/v0/device/battery/time/set
    /// </summary>
    public class SetBatteryChargeTimesRequest
    {
        [JsonPropertyName("sn")]
        public string? Sn { get; set; }

        [JsonPropertyName("times")]
        public Time[]? Times { get; set; }
    }

    public class EndTime
    {
        /// <summary>
        // String must be two digits with 0 prefix for 0-9
        /// </summary>
        [JsonPropertyName("hour")]
        public string? Hour { get; set; }

        /// <summary>
        // String must be two digits with 0 prefix for 0-9
        /// </summary>
        [JsonPropertyName("minute")]
        public string? Minute { get; set; }
    }

    public class StartTime
    {
        /// <summary>
        // String must be two digits with 0 prefix for 0-9
        /// </summary>
        [JsonPropertyName("hour")]
        public string? Hour { get; set; }

        /// <summary>
        // String must be two digits with 0 prefix for 0-9
        /// </summary>
        [JsonPropertyName("minute")]
        public string? Minute { get; set; }
    }

    public class Time
    {
        [JsonPropertyName("tip")]
        public string? Tip { get; set; }

        [JsonPropertyName("enableCharge")]
        public bool EnableCharge { get; set; }

        [JsonPropertyName("enableGrid")]
        public bool EnableGrid { get; set; }

        [JsonPropertyName("startTime")]
        public StartTime? StartTime { get; set; }

        [JsonPropertyName("endTime")]
        public EndTime? EndTime { get; set; }
    }

}
