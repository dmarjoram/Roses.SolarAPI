using Roses.SolarAPI.Services;
using System.Text.Json.Serialization;

namespace Roses.SolarAPI.Models.FoxCloud
{
    public class SetSchedulerResponse : IFoxResponse
    {
        /// <summary>
        /// 41808 means token is invalid
        /// 40257 means invalid request param
        /// 41203 means timeout
        /// 0 means OK
        /// </summary>
        [JsonPropertyName("errno")]
        public int Errno { get; set; } = (int)FoxErrorNumber.NoResponse;

		[JsonPropertyName("msg")]
		public string? Message { get; set; }

		[JsonPropertyName("result")]
        public object? Result { get; set; }
    }
}
