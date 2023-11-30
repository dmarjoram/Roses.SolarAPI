using Roses.SolarAPI.Extensions;
using System.Text.Json.Serialization;

namespace Roses.SolarAPI.Models.FoxCloud
{
    /// <summary>
    /// POST body request for https://www.foxesscloud.com/c/v0/device/battery/time/set
    /// </summary>
    public class SetBatteryChargeTimesRequest : IFoxRequest
    {
        [JsonPropertyName("sn")]
        public string? Sn { get; set; }

        [JsonPropertyName("times")]
        public Time[]? Times { get; set; }

        [JsonIgnore]
        public string RequestUri => SetBatteryChargeTimesUri;

		[JsonIgnore]
		public bool GetRequest => false;

		/// <summary>
		/// Validate the request parameters
		/// </summary>
		public void Validate()
        {
            if (string.IsNullOrWhiteSpace(Sn))
            {
                throw new ArgumentNullException(nameof(SetBatteryChargeTimesRequest), "No serial number is provided.");
            }

            if (Times!.Any(time => time!.StartTime!.ToTimeOnly() > time!.EndTime!.ToTimeOnly()))
            {
                throw new ArgumentOutOfRangeException(nameof(SetBatteryChargeTimesRequest), "Start time is after end time.");
            }

            TimeOnly defaultTime = new TimeOnly(0, 0);

            if (Times!.Any(time => time.EnableCharge && (time.StartTime!.ToTimeOnly() == defaultTime || time.EndTime!.ToTimeOnly() == defaultTime)))
            {
                throw new ArgumentOutOfRangeException(nameof(SetBatteryChargeTimesRequest), "Charge can't be enabled with start time or end time of 00:00.");
            }
        }

        private const string SetBatteryChargeTimesUri = "https://www.foxesscloud.com/c/v0/device/battery/time/set";
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
