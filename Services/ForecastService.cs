using Microsoft.Extensions.Caching.Memory;
using Roses.SolarAPI.Configuration;
using Roses.SolarAPI.Models;

namespace Roses.SolarAPI.Services
{
    public class ForecastService
    {
        private const string ESTIMATE_API_URI = "https://api.forecast.solar/:apikey/estimate/:lat/:lon/:dec1/:az1/:kwp1";

        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<ForecastService> _logger;
        private readonly SolarConfiguration? _config;
        private const string ForecastCacheKeyPrefix = "Forecast";
        private bool _initalised = false;

        public ForecastService(ILogger<ForecastService> logger, IMemoryCache memoryCache, IConfiguration configuration)
        {
            _logger = logger;
            _memoryCache = memoryCache;
            _config = configuration.GetSection(nameof(SolarConfiguration)).Get<SolarConfiguration>();

            if (_config == null)
            {
                _logger.LogWarning($"{nameof(SolarConfiguration)} is not specified. Forecast.Solar integration not enabled.");
                return;
            }

            _initalised = true;
    }

        public async Task<Forecast> Estimate(CancellationToken ct)
        {
            AssertConfigured();

            HttpClient client = new HttpClient();

            string requestUri = _config!.GenerateRequestUri(ESTIMATE_API_URI);

            if (!_memoryCache.TryGetValue($"{ForecastCacheKeyPrefix}:{requestUri}", out Forecast response))
            {
                _logger.LogInformation("No cached forecast object found. Calling API.");

                response = await client.GetFromJsonAsync<Forecast>(requestUri, ct) ?? new();

                MemoryCacheEntryOptions cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromHours(_config.IntervalHours / 2d));

                _logger.LogInformation("Got response. Adding to memory cache.");

                _memoryCache.Set($"{ForecastCacheKeyPrefix}:{requestUri}", response, cacheEntryOptions);
            }
            else
            {
                _logger.LogInformation("Cached forecast object found.");
            }

            return response;
        }

        private void AssertConfigured()
        {
            if (!_initalised)
            {
                throw new InvalidOperationException("The solar configuration for the Forecast.Solar intergration is not correctly configured.");
            }
        }
    }
}
