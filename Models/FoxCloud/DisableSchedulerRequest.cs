using Roses.SolarAPI.Extensions;
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
}
