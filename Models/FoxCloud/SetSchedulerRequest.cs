using Roses.SolarAPI.Extensions;
using System.Text.Json.Serialization;

namespace Roses.SolarAPI.Models.FoxCloud
{
    /// <summary>
    /// POST body request for https://www.foxesscloud.com/generic/v0/device/scheduler/enable
    /// </summary>
    public class SetSchedulerRequest : IFoxRequest
    {
        [JsonPropertyName("pollcy")]
        public Policy[]? Policies { get; set; }

		[JsonPropertyName("deviceSN")]
		public string? DeviceSerialNumber { get; set; }

		[JsonIgnore]
        public string RequestUri => SetSchedulerUri;

		[JsonIgnore]
		public bool GetRequest => false;

		/// <summary>
		/// Validate the request parameters
		/// </summary>
		public void Validate()
        {
            if (string.IsNullOrWhiteSpace(DeviceSerialNumber))
            {
                throw new ArgumentNullException(nameof(SetSchedulerRequest), "No serial number is provided.");
            }

            if (Policies!.Any(time => time!.ToStartTimeOnly() > time!.ToEndTimeOnly()))
            {
                throw new ArgumentOutOfRangeException(nameof(SetSchedulerRequest), "Start time is after end time.");
            }

            TimeOnly defaultTime = new TimeOnly(0, 0);

            if (Policies!.Any(time => time.ToStartTimeOnly() == defaultTime || time.ToEndTimeOnly() == defaultTime))
            {
                throw new ArgumentOutOfRangeException(nameof(SetSchedulerRequest), "Schedule can't be enabled with start time or end time of 00:00.");
            }
        }

        private const string SetSchedulerUri = "https://www.foxesscloud.com/generic/v0/device/scheduler/enable";
    }

    public class Policy
    {
		/// <summary>
		// Start hour for schedule policy
		/// </summary>
		[JsonPropertyName("startH")]
        public int? StartHour { get; set; }

        /// <summary>
        // Start minute for schedule policy
        /// </summary>
        [JsonPropertyName("startM")]
        public int? StartMinute { get; set; }

		/// <summary>
		// Start hour for schedule policy
		/// </summary>
		[JsonPropertyName("endH")]
		public int? EndHour { get; set; }

		/// <summary>
		// Start minute for schedule policy
		/// </summary>
		[JsonPropertyName("endM")]
		public int? EndMinute { get; set; }

        /// <summary>
        /// Work mode for schedule policy
        /// </summary>
		[JsonPropertyName("workMode")]
		public string? WorkMode { get; set; }

		/// <summary>
		// MinSOC for on-grid
		/// </summary>
		[JsonPropertyName("minsocongrid")]
		public int? MinSOCOnGrid { get; set; }

		/// <summary>
		// MinSOC for force discharge to be active
		/// </summary>
		[JsonPropertyName("fdsoc")]
		public int? ForceDischargeSOC { get; set; }

		/// <summary>
		// Force discharge power in W
		/// </summary>
		[JsonPropertyName("fdpwr")]
		public int? ForceDischargePower { get; set; }
	}

}
