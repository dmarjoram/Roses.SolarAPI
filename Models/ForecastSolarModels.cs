namespace Roses.SolarAPI.Models
{
    using System.Collections.Generic;
    using System.Text.Json.Serialization;

    public partial class Forecast
    {
        [JsonPropertyName("result")]
        public Result? Result { get; set; }

        [JsonPropertyName("message")]
        public Message? Message { get; set; }
    }

    public partial class Message
    {
        [JsonPropertyName("code")]
        public long Code { get; set; }

        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("text")]
        public string? Text { get; set; }

        [JsonPropertyName("info")]
        public Info? Info { get; set; }

        [JsonPropertyName("ratelimit")]
        public Ratelimit? Ratelimit { get; set; }
    }

    public partial class Info
    {
        [JsonPropertyName("latitude")]
        public double Latitude { get; set; }

        [JsonPropertyName("longitude")]
        public double Longitude { get; set; }

        [JsonPropertyName("place")]
        public string? Place { get; set; }

        [JsonPropertyName("timezone")]
        public string? Timezone { get; set; }
    }

    public partial class Ratelimit
    {
        [JsonPropertyName("period")]
        public long Period { get; set; }

        [JsonPropertyName("limit")]
        public long Limit { get; set; }

        [JsonPropertyName("remaining")]
        public long Remaining { get; set; }
    }

    public partial class Result
    {
        [JsonPropertyName("watts")]
        public Dictionary<string, long>? Watts { get; set; }

        [JsonPropertyName("watt_hours")]
        public Dictionary<string, long>? WattHours { get; set; }

        [JsonPropertyName("watt_hours_day")]
        public Dictionary<string, long>? WattHoursDay { get; set; }
    }
}
