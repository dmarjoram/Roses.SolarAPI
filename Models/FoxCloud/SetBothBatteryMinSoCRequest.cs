using Roses.SolarAPI.Extensions;
using System.Text.Json.Serialization;

namespace Roses.SolarAPI.Models.FoxCloud
{
    /// <summary>
    /// POST body request for https://www.foxesscloud.com/c/v0/device/battery/soc/set
    /// </summary>
    public class SetBothBatteryMinSoCRequest : IFoxRequest
    {
        [JsonPropertyName("sn")]
        public string? Sn { get; set; }

        [JsonPropertyName("minGridSoc")]
        public ushort? MinGridSoc { get; set; }

        [JsonPropertyName("minSoc")]
        public ushort? MinSoc { get; set; }

        [JsonIgnore]
        public string RequestUri => SetMinSocUri;

        /// <summary>
        /// Validate the request parameters
        /// </summary>
        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(Sn))
            {
                throw new ArgumentNullException(nameof(SetBothBatteryMinSoCRequest), "No serial number is provided.");
            }

            if (MinGridSoc < 10 || MinGridSoc > 100)
            {
                throw new ArgumentOutOfRangeException(nameof(SetBothBatteryMinSoCRequest), "MinGridSoc must between 10-100.");
            }

            if (MinSoc < 10 || MinSoc > 100)
            {
                throw new ArgumentOutOfRangeException(nameof(SetBothBatteryMinSoCRequest), "MinSoc must between 10-100.");
            }
        }

        private const string SetMinSocUri = "https://www.foxesscloud.com/c/v0/device/battery/soc/set";
    }
}
