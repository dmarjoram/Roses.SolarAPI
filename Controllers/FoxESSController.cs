using Microsoft.AspNetCore.Mvc;
using Roses.SolarAPI.Configuration;
using Roses.SolarAPI.Models;
using Roses.SolarAPI.Models.FoxCloud;
using Roses.SolarAPI.Models.Local;
using Roses.SolarAPI.Services;
using System.ComponentModel.DataAnnotations;

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
        [Route("Local/SetBatteryMinGridSoCToCurrentSoc")]
        public async Task<ApiResult> SetBatteryMinGridSoCToCurrentSoc(CancellationToken ct = default)
        {
            return new ApiResult() { ResultCode = await _foxESSService.SetBatteryMinGridSoCToCurrentSoc(ct) };
        }

        [HttpPost]
        [Route("Local/SetBatteryMinSoC")]
        public async Task<ApiResult> SetBatteryMinSoC(CancellationToken ct = default, [FromQuery][Required] ushort percentage = 10)
        {
            return new ApiResult() { ResultCode = await _foxESSService.SetBatteryMinSoC(percentage, ct) };
        }

        [HttpPost]
        [Route("Local/SetBatteryMinGridSoC")]
        public async Task<ApiResult> SetBatteryMinGridSoC(CancellationToken ct = default, [FromQuery][Required] ushort percentage = 10)
        {
            return new ApiResult() { ResultCode = await _foxESSService.SetBatteryMinGridSoC(percentage, ct) };
        }

        [HttpPost]
        [Route("Local/SetBothBatteryMinSoC")]
        public async Task<ApiResult> SetBothBatteryMinSoC(CancellationToken ct, [FromQuery][Required] ushort minSoc = 10, [FromQuery][Required] ushort minSocGrid = 10)
        {
            return new ApiResult()
            {
                ResultCode = await _foxESSService.SetBothBatteryMinSoC(new Models.Local.SetBothBatteryMinSoCRequest()
                {
                    MinSoc = minSoc,
                    MinGridSoc = minSocGrid
                }
                , ct)
            };
        }

		[HttpGet]
		[Route("Local/GetAddressValue")]
		public short GetAddressValue(CancellationToken ct, [FromQuery][Required] int address = 0)
		{
            return _foxESSService.GetAddressValue(address);
		}

		[HttpPost]
		[Route("Local/WorkMode/SelfUse")]
		public async Task<ApiResult> SetWorkModeSelfUse(CancellationToken ct = default)
		{
			return new ApiResult() { ResultCode = await _foxESSService.SetWorkMode(WorkMode.SELF_USE, ct) };
		}

		[HttpPost]
		[Route("Local/WorkMode/FeedIn")]
		public async Task<ApiResult> SetWorkModeFeedin(CancellationToken ct = default)
		{
			return new ApiResult() { ResultCode = await _foxESSService.SetWorkMode(WorkMode.FEED_IN, ct) };
		}

		[HttpPost]
		[Route("Local/WorkMode/Backup")]
		public async Task<ApiResult> SetWorkModeBackUp(CancellationToken ct = default)
		{
			return new ApiResult() { ResultCode = await _foxESSService.SetWorkMode(WorkMode.BACKUP, ct) };
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
		[Route("Local/ForceChargeAllTodayTimePeriod1")]
		public async Task<ApiResult> ForceChargeAllTodayTimePeriod1(CancellationToken ct)
		{
			return new ApiResult()
			{
				ResultCode = await _foxESSService.ForceChargeAllTodayTimePeriod1(ct)
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

		//[HttpPost]
		//[Route("Local/DisableForceChargeTimePeriod1")]
		//public async Task<ApiResult> DisableForceChargeTimePeriod1(CancellationToken ct)
		//{
		//    return new ApiResult()
		//    {
		//        ResultCode = await _foxESSService.DisableForceChargeTimePeriod1(ct)
		//    };
		//}

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
		[Route("Cloud/ForceChargeAllTodayTimePeriod1")]
		public async Task<ApiResult> FoxCloudForceChargeAllTodayTimePeriod1(CancellationToken ct, [FromQuery] bool enableGridCharging = false)
		{
			return new ApiResult()
			{
				ResultCode = await _foxESSService.FoxCloudForceChargeAllTodayTimePeriod1(enableGridCharging, ct)
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

        [HttpPost]
        [Route("Cloud/SetBothBatteryMinSoC")]
        public async Task<ApiResult> FoxCloudSetBothBatteryMinSoC(CancellationToken ct, [FromQuery][Required] ushort minSoc = 10, [FromQuery][Required] ushort minSocGrid = 10)
        {
            return new ApiResult()
            {
                ResultCode = await _foxESSService.FoxCloudSetBothBatteryMinSoC(minSoc, minSocGrid, ct)
            };
        }

        [HttpGet]
        [Route("Cloud/DeviceList")]
        public Task<Device[]> GetDeviceList(CancellationToken ct)
        {
            return _foxESSService.FoxCloudGetDeviceList(ct);
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