using FluentModbus;
using Microsoft.Extensions.Caching.Memory;
using Roses.SolarAPI.Configuration;
using Roses.SolarAPI.Extensions;
using Roses.SolarAPI.Models;
using Roses.SolarAPI.Models.Local;
using System.Net;

namespace Roses.SolarAPI.Services
{
    public partial class FoxESSService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<FoxESSService> _logger;
        private readonly FoxESSConfiguration? _config;
        private const string BatteryConfigurationCacheKeyPrefix = "BatteryConfiguration";
        private bool _initalised = false;

        public FoxESSService(ILogger<FoxESSService> logger, IMemoryCache memoryCache, IConfiguration configuration)
        {
            _logger = logger;
            _memoryCache = memoryCache;
            _config = configuration.GetSection(nameof(FoxESSConfiguration)).Get<FoxESSConfiguration>();

            if (_config == null)
            {
                _logger.LogWarning($"{nameof(FoxESSConfiguration)} is not specified. FoxESS integration not enabled.");
                return;
            }

            if (string.IsNullOrWhiteSpace(_config.IPAddress))
            {
                _logger.LogWarning("No IP address is specified for modbus TCP connection to FoxESS inverter.");
                return;
            }

            _initalised = true;
            _logger.LogInformation("Using modbus TCP to IP address {ipAddress} on port {port}. Device ID is set to {deviceId}", _config.IPAddress, _config.Port, _config.DeviceId);
        }

        public BatteryConfiguration GetBatteryConfiguration(bool includeSoc = true)
        {
            AssertConfigured();

            if (!_memoryCache.TryGetValue($"{BatteryConfigurationCacheKeyPrefix}:{_config!.IPAddress}:{_config.Port}:{_config.DeviceId}", out BatteryConfiguration response))
            {
                _logger.LogInformation("No cached battery configuration object found. Reading modbus registers.");

                ModbusTcpClient? client = null;
                try
                {
                    client = new ModbusTcpClient();

                    client.Connect(new IPEndPoint(IPAddress.Parse(_config.IPAddress!), _config.Port), ModbusEndianness.BigEndian);

                    // Read both time periods in one call
                    short[] values = client.ReadHoldingRegisters<short>(_config.DeviceId, 
                        (int)FoxESSRegisters.BATTERY_TIMEPERIOD1_CHARGE_FROM_GRID, 6).ToArray();

                    // Read min SOC
                    short minSocValue = includeSoc ? client.ReadHoldingRegisters<short>(_config.DeviceId,
                        (int)FoxESSRegisters.BATTERY_MIN_SOC_RW, 1).ToArray().FirstOrDefault() : (short)0;

                    // Read min SOC (on grid)
                    short minSocOnGridValue = includeSoc ? client.ReadHoldingRegisters<short>(_config.DeviceId,
                        (int)FoxESSRegisters.BATTERY_MIN_SOC_ON_GRID_RW, 1).ToArray().FirstOrDefault() : (short)0;

                    client.Disconnect();

                    response = new BatteryConfiguration()
                    {
                        ForceCharge1 = values[1] != 0 || values[2] != 0,
                        ChargeFromGrid1 = values[0] == 1,
                        StartDate1 = values[1].ToTimeOnlyFoxESS(),
                        EndDate1 = values[2].ToTimeOnlyFoxESS(),
                        ForceCharge2 = values[4] != 0 || values[5] != 0,
                        ChargeFromGrid2 = values[3] == 1,
                        StartDate2 = values[4].ToTimeOnlyFoxESS(),
                        EndDate2 = values[5].ToTimeOnlyFoxESS(),
                        MinSoC = (ushort)minSocValue,
                        MinSoCOnGrid = (ushort)minSocOnGridValue
                    };

                    _logger.LogInformation("Retrieved battery configuration from FoxESS inverter. Adding to memory cache.");

                    MemoryCacheEntryOptions cacheEntryOptions = new MemoryCacheEntryOptions()
                        .SetAbsoluteExpiration(TimeSpan.FromSeconds(_config.CacheSeconds));

                    _memoryCache.Set($"{BatteryConfigurationCacheKeyPrefix}:{_config.IPAddress}:{_config.Port}:{_config.DeviceId}", response, cacheEntryOptions);
                }
                finally
                {
                    client?.Disconnect();
                }
            }
            else
            {
                _logger.LogInformation("Cached battery configuration object found.");
            }

            return response;
        }

        public async Task<string> SetBatteryMinSoC(ushort percentage, CancellationToken ct = default)
        {
            new SetBothBatteryMinSoCRequest() { MinSoc = percentage }.Validate();

            await WriteSingleRegister((FoxESSRegisters.BATTERY_MIN_SOC_RW, (short)percentage), ct);

            return FoxErrorNumber.OK.ToString();
        }

        public async Task<string> SetBatteryMinGridSoC(ushort percentage, CancellationToken ct = default)
        {
            new SetBothBatteryMinSoCRequest() { MinGridSoc = percentage }.Validate();

            await WriteSingleRegister((FoxESSRegisters.BATTERY_MIN_SOC_ON_GRID_RW, (short)percentage), ct);

            return FoxErrorNumber.OK.ToString();
        }

        public async Task<string> SetBothBatteryMinSoC(SetBothBatteryMinSoCRequest request, CancellationToken ct = default)
        {
            request.Validate();

            await WriteSingleRegisters(ct,
                (FoxESSRegisters.BATTERY_MIN_SOC_RW, (short)request?.MinSoc!),
                (FoxESSRegisters.BATTERY_MIN_SOC_ON_GRID_RW, (short)request?.MinGridSoc!));

            return FoxErrorNumber.OK.ToString();
        }

        public async Task<string> ForceChargeForTodayTimePeriod1(CancellationToken ct = default)
        {
            TimeOnly now = TimeOnly.FromDateTime(DateTime.Now);
            TimeOnly end = new TimeOnly(23, 59);

            await WriteSingleRegisters(ct,
                (FoxESSRegisters.BATTERY_TIMEPERIOD1_START_TIME, now.ToFoxESSRegister()),
                (FoxESSRegisters.BATTERY_TIMEPERIOD1_END_TIME, end.ToFoxESSRegister()));

            return FoxErrorNumber.OK.ToString();
        }

        public async Task<string> ForceChargeForTodayTimePeriod2(CancellationToken ct = default)
        {
            TimeOnly now = TimeOnly.FromDateTime(DateTime.Now);
            TimeOnly end = new TimeOnly(23, 59);

            await WriteSingleRegisters(ct,
                (FoxESSRegisters.BATTERY_TIMEPERIOD2_START_TIME, now.ToFoxESSRegister()),
                (FoxESSRegisters.BATTERY_TIMEPERIOD2_END_TIME, end.ToFoxESSRegister()));

            return FoxErrorNumber.OK.ToString();
        }

        public async Task<string> DisableForceChargeTimePeriod1(CancellationToken ct = default)
        {
            await WriteSingleRegisters(ct,
                (FoxESSRegisters.BATTERY_TIMEPERIOD1_START_TIME, 0),
                (FoxESSRegisters.BATTERY_TIMEPERIOD1_END_TIME, 0));

            return FoxErrorNumber.OK.ToString();
        }

        public async Task<string> DisableForceChargeTimePeriod2(CancellationToken ct = default)
        {
            await WriteSingleRegisters(ct,
                (FoxESSRegisters.BATTERY_TIMEPERIOD2_START_TIME, 0),
                (FoxESSRegisters.BATTERY_TIMEPERIOD2_END_TIME, 0));

            return FoxErrorNumber.OK.ToString();
        }

        /// <summary>
        /// Write a single register to the inverter
        /// </summary>
        /// <param name="update">the update</param>
        /// <param name="ct">cancellation token</param>
        private Task WriteSingleRegister((FoxESSRegisters Register, short Value) update, CancellationToken ct = default)
        {
            return WriteSingleRegisters(ct, update);
        }

        /// <summary>
        /// Write a set of single registers to the inverter
        /// </summary>
        /// <param name="ct">cancellation token</param>
        /// <param name="updates">register updates</param>
        private async Task WriteSingleRegisters(CancellationToken ct = default, params (FoxESSRegisters Register, short Value)[] updates)
        {
            AssertConfigured();

            ModbusTcpClient? client = null;
            try
            {
                client = new ModbusTcpClient();

                client.Connect(new IPEndPoint(IPAddress.Parse(_config!.IPAddress!), _config.Port), ModbusEndianness.BigEndian);

                foreach (var update in updates)
                {
                    await client.WriteSingleRegisterAsync(_config.DeviceId, (int)update.Register, update.Value, ct);

                    _logger.LogInformation("Successfully written {value} to register {register}.", update.Value, update.Register);
                }
            }
            finally
            {
                client?.Disconnect();
            }
        }

        private void AssertConfigured()
        {
            if (!_initalised)
            {
                throw new InvalidOperationException("The modbus configuration for the FoxESS inverter connection is not correctly configured.");
            }
        }
    }
}
