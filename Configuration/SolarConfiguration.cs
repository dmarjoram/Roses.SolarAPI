namespace Roses.SolarAPI.Configuration
{
    public class SolarConfiguration
    {
        public string? ApiKey { get; set; }

        public double Lat { get; set; }

        public double Lon { get; set; }

        public double Az { get; set; }

        public double Dec { get; set; }

        public double Kwp { get; set; }

        public double IntervalHours { get; set; } = 0.5d;

        public string GenerateRequestUri(string apiUri)
        {
            return apiUri
                .Replace(":apikey", ApiKey)
                .Replace(":lat", Lat.ToString())
                .Replace(":lon", Lon.ToString())
                .Replace(":dec1", Dec.ToString())
                .Replace(":az1", Az.ToString())
                .Replace(":kwp1", Kwp.ToString());
        }
    }
}
