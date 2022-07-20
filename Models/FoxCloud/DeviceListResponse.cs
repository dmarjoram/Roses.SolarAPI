using System.Text.Json.Serialization;

namespace Roses.SolarAPI.Models.FoxCloud
{
    public partial class DeviceListResponse : IFoxResponse
    {
        [JsonPropertyName("errno")]
        public int Errno { get; set; }

        [JsonPropertyName("result")]
        public Result? Result { get; set; }
    }

    public partial class Result
    {
        [JsonPropertyName("currentPage")]
        public int CurrentPage { get; set; }

        [JsonPropertyName("pageSize")]
        public int PageSize { get; set; }

        [JsonPropertyName("total")]
        public int Total { get; set; }

        [JsonPropertyName("devices")]
        public Device[]? Devices { get; set; }
    }

    public partial class Device
    {
        [JsonPropertyName("deviceID")]
        public Guid DeviceId { get; set; }

        [JsonPropertyName("deviceSN")]
        public string? DeviceSn { get; set; }

        [JsonPropertyName("moduleSN")]
        public string? ModuleSn { get; set; }

        [JsonPropertyName("plantName")]
        public string? PlantName { get; set; }

        [JsonPropertyName("deviceType")]
        public string? DeviceType { get; set; }

        [JsonPropertyName("country")]
        public string? Country { get; set; }

        [JsonPropertyName("countryCode")]
        public string? CountryCode { get; set; }

        [JsonPropertyName("feedinDate")]
        public string? FeedinDate { get; set; }

        [JsonPropertyName("status")]
        public int Status { get; set; }

        [JsonPropertyName("power")]
        public double Power { get; set; }

        [JsonPropertyName("generationToday")]
        public double GenerationToday { get; set; }

        [JsonPropertyName("generationTotal")]
        public double GenerationTotal { get; set; }

        [JsonPropertyName("productType")]
        public string? ProductType { get; set; }

        [JsonPropertyName("flowType")]
        public int FlowType { get; set; }

        [JsonPropertyName("hasBattery")]
        public bool HasBattery { get; set; }

        [JsonPropertyName("hasPV")]
        public bool HasPv { get; set; }

        [JsonPropertyName("dataLatestUploadDate")]
        public string? DataLatestUploadDate { get; set; }
    }
}
