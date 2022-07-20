using System.Text.Json.Serialization;

namespace Roses.SolarAPI.Models.FoxCloud
{
    public partial class DeviceListRequest
    {
        [JsonPropertyName("pageSize")]
        public int PageSize { get; set; } = 10;

        [JsonPropertyName("currentPage")]
        public int CurrentPage { get; set; } = 1;

        [JsonPropertyName("total")]
        public int Total { get; set; } = 0;

        [JsonPropertyName("condition")]
        public Condition Condition { get; set; } = new Condition();
    }

    public partial class Condition
    {
        [JsonPropertyName("queryDate")]
        public QueryDate QueryDate { get; set; } = new QueryDate();
    }

    public partial class QueryDate
    {
        [JsonPropertyName("begin")]
        public int Begin { get; set; } = 0;

        [JsonPropertyName("end")]
        public int End { get; set; } = 0;
    }

}
