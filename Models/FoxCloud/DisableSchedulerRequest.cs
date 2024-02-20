using System.Text.Json.Serialization;

namespace Roses.SolarAPI.Models.FoxCloud
{
    /// <summary>
    /// Request for https://www.foxesscloud.com/generic/v0/device/scheduler/disable?deviceSN={deviceSN}
    /// </summary>
    public class DisableSchedulerRequest : IFoxRequest
    {
		[JsonPropertyName("deviceSN")]
		public string? DeviceSerialNumber { get; set; }

		[JsonIgnore]
        public string RequestUri => DisableSchedulerUri.Replace(":deviceSN:", DeviceSerialNumber);

		[JsonIgnore]
		public bool GetRequest => true;

		/// <summary>
		/// Validate the request parameters
		/// </summary>
		public void Validate()
        {
            if (string.IsNullOrWhiteSpace(DeviceSerialNumber))
            {
                throw new ArgumentNullException(nameof(SetSchedulerRequest), "No serial number is provided.");
            }
        }

        private const string DisableSchedulerUri = "https://www.foxesscloud.com/generic/v0/device/scheduler/disable?deviceSN=:deviceSN:";
    }

    /// <summary>
    /// Request for https://www.foxesscloud.com/generic/v0/device/setting/set
    /// </summary>
    public class DisableSchedulerRequest2 : IFoxRequest
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("key")]
        public string? Key { get; set; }

        [JsonPropertyName("values")]
        public object? Values { get; set; }

        [JsonIgnore]
        public string RequestUri => DisableSchedulerUri2;

        [JsonIgnore]
        public bool GetRequest => false;

        public DisableSchedulerRequest2(string spaKey)
        {

            if (string.IsNullOrWhiteSpace(spaKey))
            {
                throw new ArgumentOutOfRangeException(nameof(spaKey), "Please provide a spaKey.");
            }

            if (!DisableScheduleSpaKeys.ALL.Any(key => key == spaKey))
            {
                throw new ArgumentOutOfRangeException(nameof(spaKey), "A valid SPA key has not been provided.");
            }

            Key = $"{spaKey.Trim()}__segmented_time_enable";

            switch (spaKey)
            {
                case DisableScheduleSpaKeys.H106:
                    Values = new Values106() { Mode = "disable" };
                    break;
                case DisableScheduleSpaKeys.H108:
                    Values = new Values108() { Mode = "disable" };
                    break;
                case DisableScheduleSpaKeys.H111:
                    Values = new Values111() { Mode = "disable" };
                    break;
                case DisableScheduleSpaKeys.H112:
                    Values = new Values112() { Mode = "disable" };
                    break;
                case DisableScheduleSpaKeys.H115:
                    Values = new Values115() { Mode = "disable" };
                    break;
            }
        }

        /// <summary>
        /// Validate the request parameters
        /// </summary>
        public void Validate()
        {
            if (Id == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(SetWorkModeRequest), "No cloud device ID is provided.");
            }

            if (string.IsNullOrWhiteSpace(Key))
            {
                throw new ArgumentOutOfRangeException(nameof(SetWorkModeRequest), "Key has been changed.");
            }
        }

        private const string DisableSchedulerUri2 = "https://www.foxesscloud.com/generic/v0/device/setting/set";

        private interface IValues
        {
            string? Mode { get; set; }
        }

        private class Values106 : IValues
        {
            [JsonPropertyName("h106__segmented_time_enable__time_mode_flag")]
            public string? Mode { get; set; }
        }

        private class Values108 : IValues
        {
            [JsonPropertyName("h108__segmented_time_enable__time_mode_flag")]
            public string? Mode { get; set; }
        }

        private class Values111 : IValues
        {
            [JsonPropertyName("h111__segmented_time_enable__time_mode_flag")]
            public string? Mode { get; set; }
        }

        private class Values112 : IValues
        {
            [JsonPropertyName("h112__segmented_time_enable__time_mode_flag")]
            public string? Mode { get; set; }
        }

        private class Values115 : IValues
        {
            [JsonPropertyName("h115__segmented_time_enable__time_mode_flag")]
            public string? Mode { get; set; }
        }

        public class DisableScheduleSpaKeys
        {
            public const string H106 = "h106";
            public const string H108 = "h108";
            public const string H111 = "h111";
            public const string H112 = "h112";
            public const string H115 = "h115";

            public const string DEFAULT = H115;
            public readonly static string[] ALL = new[] { H115, H106, H108, H111, H112 };
        }

    }


}
