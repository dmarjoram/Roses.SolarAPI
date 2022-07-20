using Roses.SolarAPI.Extensions;
using System.Text.Json.Serialization;

namespace Roses.SolarAPI.Models.Local
{
    public class SetBothBatteryMinSoCRequest
    {
        [JsonPropertyName("minGridSoc")]
        public ushort? MinGridSoc { get; set; } = 10;

        [JsonPropertyName("minSoc")]
        public ushort? MinSoc { get; set; } = 10;

        /// <summary>
        /// Validate the request parameters
        /// </summary>
        public void Validate()
        {
            if (MinGridSoc < 10 || MinGridSoc > 100)
            {
                throw new ArgumentOutOfRangeException(nameof(SetBothBatteryMinSoCRequest), "MinGridSoc must between 10-100.");
            }

            if (MinSoc < 10 || MinSoc > 100)
            {
                throw new ArgumentOutOfRangeException(nameof(SetBothBatteryMinSoCRequest), "MinSoc must between 10-100.");
            }
        }
    }
}
