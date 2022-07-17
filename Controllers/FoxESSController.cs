using Microsoft.AspNetCore.Mvc;
using Roses.SolarAPI.Configuration;
using Roses.SolarAPI.Models;
using Roses.SolarAPI.Services;

namespace Roses.SolarAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FoxESSController : ControllerBase
    {
        private readonly ILogger<FoxESSController> _logger;
        private readonly FoxESSService _foxESSService;
        private readonly FoxESSConfiguration _config;

        public FoxESSController(ILogger<FoxESSController> logger, FoxESSService foxESSService, IConfiguration configuration)
        {
            _logger = logger;
            _foxESSService = foxESSService;
            _config = configuration.GetSection(nameof(FoxESSConfiguration)).Get<FoxESSConfiguration>();
        }

        [HttpGet]
        [Route("Local/BatteryConfiguration")]
        public BatteryConfiguration GetBatteryConfiguration(CancellationToken ct)
        {
            return _foxESSService.GetBatteryConfiguration();
        }

        [HttpPost]
        [Route("Local/ForceChargeForTodayTimePeriod1")]
        public Task ForceChargeForTodayTimePeriod1(CancellationToken ct)
        {
            return _foxESSService.ForceChargeForTodayTimePeriod1(ct);
        }

        [HttpPost]
        [Route("Local/DisableForceChargeTimePeriod1")]
        public Task DisableForceChargeTimePeriod1(CancellationToken ct)
        {
            return _foxESSService.DisableForceChargeTimePeriod1(ct);
        }
    }
}