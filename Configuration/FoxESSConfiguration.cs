namespace Roses.SolarAPI.Configuration
{
    public class FoxESSConfiguration
    {
        public string? IPAddress { get; set; }

        public int Port { get; set; } = 502;

        public int DeviceId { get; set; } = 247;

        public int CacheSeconds { get; set; } = 5;

        public int TokenCacheSeconds { get; set; } = 60;

        public string? SerialNumber { get; set; }

        public string? Username { get; set; }

        public string? PasswordMD5Hash { get; set; }
    }
}
