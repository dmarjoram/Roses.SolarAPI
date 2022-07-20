using Microsoft.AspNetCore.Mvc;
using Roses.SolarAPI.Configuration;
using Roses.SolarAPI.Models;
using Roses.SolarAPI.Models.FoxCloud;
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
        public async Task<ApiResult> ForceChargeForTodayTimePeriod1(CancellationToken ct)
        {
            return new ApiResult()
            {
                ResultCode = await _foxESSService.ForceChargeForTodayTimePeriod1(ct)
            };
        }

        [HttpPost]
        [Route("Local/DisableForceChargeTimePeriod1")]
        public async Task<ApiResult> DisableForceChargeTimePeriod1(CancellationToken ct)
        {
            return new ApiResult()
            {
                ResultCode = await _foxESSService.DisableForceChargeTimePeriod1(ct)
            };
        }

        [HttpPost]
        [Route("Cloud/ForceChargeForTodayTimePeriod1")]
        public async Task<ApiResult> FoxCloudForceChargeForTodayTimePeriod1(CancellationToken ct, [FromQuery] bool enableGridCharging = false)
        {
            return new ApiResult()
            {
                ResultCode = await _foxESSService.FoxCloudForceChargeForTodayTimePeriod1(enableGridCharging, ct)
            };
        }

        [HttpPost]
        [Route("Cloud/DisableForceChargeTimePeriod1")]
        public async Task<ApiResult> FoxCloudDisableForceChargeTimePeriod1(CancellationToken ct)
        {
            return new ApiResult()
            {
                ResultCode = await _foxESSService.FoxCloudDisableForceChargeTimePeriod1(ct)
            };
        }

        [HttpGet]
        [Route("Cloud/DeviceList")]
        public Task<Device[]> GetDeviceList(CancellationToken ct)
        {
            return _foxESSService.GetDeviceList(ct);
        }

        /// <summary>
        /// Set inverter for feed-in
        /// </summary>
        [HttpPost]
        [Route("Cloud/WorkMode/FeedIn")]
        public async Task<ApiResult> FoxCloudSetWorkModeFeedIn(CancellationToken ct = default)
        {
            return new ApiResult()
            {
                ResultCode = await _foxESSService.FoxCloudSetWorkModeFeedIn(ct)
            };
        }

        /// <summary>
        /// Set inverter for self-use
        /// </summary>
        [HttpPost]
        [Route("Cloud/WorkMode/SelfUse")]
        public async Task<ApiResult> FoxCloudSetWorkModeSelfUse(CancellationToken ct = default)
        {
            return new ApiResult()
            {
                ResultCode = await _foxESSService.FoxCloudSetWorkModeSelfUse(ct)
            };
        }

        /// <summary>
        /// Set inverter for backup
        /// </summary>
        [HttpPost]
        [Route("Cloud/WorkMode/Backup")]
        public async Task<ApiResult> FoxCloudSetWorkModeBackup(CancellationToken ct = default)
        {
            return new ApiResult()
            {
                ResultCode = await _foxESSService.FoxCloudSetWorkModeBackup(ct)
            };
        }
    }
}