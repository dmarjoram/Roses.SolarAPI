using System.Text.Json.Serialization;

namespace Roses.SolarAPI.Models.FoxCloud
{
    public partial class DeviceListRequest : IFoxRequest    
    {
        [JsonPropertyName("pageSize")]
        public int PageSize { get; set; } = 10;

        [JsonPropertyName("currentPage")]
        public int CurrentPage { get; set; } = 1;

        [JsonPropertyName("total")]
        public int Total { get; set; } = 0;

        [JsonPropertyName("condition")]
        public Condition Condition { get; set; } = new Condition();

        [JsonIgnore]
        public string RequestUri => DeviceListUri;

        [JsonIgnore]
        public bool GetRequest => false;

        public void Validate()
        {
            // Nothing to do here
        }

        private const string DeviceListUri = "https://www.foxesscloud.com/c/v0/device/list";
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
