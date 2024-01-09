using FluentModbus;
using Microsoft.AspNetCore.Hosting;
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
        private readonly IWebHostEnvironment _webHostEnvironment;
        private const string BatteryConfigurationCacheKeyPrefix = "BatteryConfiguration";
		private const string WorkModeCacheKeyPrefix = "WorkMode";
		private const string AddressValueCacheKeyPrefix = "AddressValue";
		private bool _initalised = false;

        public FoxESSService(ILogger<FoxESSService> logger, IMemoryCache memoryCache, IConfiguration configuration, IWebHostEnvironment webHostEnvironment)
        {
            _logger = logger;
            _memoryCache = memoryCache;
            _config = configuration.GetSection(nameof(FoxESSConfiguration)).Get<FoxESSConfiguration>();
            _webHostEnvironment = webHostEnvironment;

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

                    // Read work mode
                    short workMode = client.ReadInputRegisters<short>(_config.DeviceId,
                        (int)FoxESSRegisters.INVERTER_WORKMODE, 1).ToArray().FirstOrDefault();

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
                        MinSoCOnGrid = (ushort)minSocOnGridValue,
                        WorkMode = ((WorkMode)workMode).ToString()
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


		public short GetAddressValue(int address = 0)
		{
			AssertConfigured();

            if (!_memoryCache.TryGetValue($"{AddressValueCacheKeyPrefix}:{_config!.IPAddress}:{_config.Port}:{_config.DeviceId}", out short response))
            {
                _logger.LogInformation("No cached address value found. Reading modbus registers.");

                ModbusTcpClient? client = null;
                try
                {
                    client = new ModbusTcpClient();

                    client.Connect(new IPEndPoint(IPAddress.Parse(_config!.IPAddress!), _config.Port), ModbusEndianness.BigEndian);

                    response = client.ReadInputRegisters<short>(_config.DeviceId, address, 1).ToArray().FirstOrDefault();

                    _logger.LogInformation("Retrieved address value from FoxESS inverter. Adding to memory cache.");

                    MemoryCacheEntryOptions cacheEntryOptions = new MemoryCacheEntryOptions()
                        .SetAbsoluteExpiration(TimeSpan.FromSeconds(_config.CacheSeconds));

                    _memoryCache.Set($"{AddressValueCacheKeyPrefix}:{_config.IPAddress}:{_config.Port}:{_config.DeviceId}", response, cacheEntryOptions);
                }
                finally
                {
                    client?.Disconnect();
                }
            }

            return response;
		}

		public async Task<string> SetBatteryMinGridSoCToCurrentSoc(CancellationToken ct = default)
        {
            bool success = await CopySingleRegister(
                FoxESSRegisters.BATTERY_SOC, 
                FoxESSRegisters.BATTERY_MIN_SOC_ON_GRID_RW, 10, 100, ct);

            return success ? FoxErrorNumber.OK.ToString() : FoxErrorNumber.OutOfBounds.ToString();
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

        public async Task<string> SetWorkMode(WorkMode workMode, CancellationToken ct = default)
        {
            await WriteSingleRegister((FoxESSRegisters.INVERTER_WORKMODE, (short)workMode), ct);

            // Invalidate cache
            _memoryCache.Remove($"{BatteryConfigurationCacheKeyPrefix}:{_config!.IPAddress}:{_config.Port}:{_config.DeviceId}");

			return FoxErrorNumber.OK.ToString();
        }

		public async Task<string> ForceChargeForToday(CancellationToken ct = default)
        {
            TimeOnly now = TimeOnly.FromDateTime(DateTime.Now);
            TimeOnly end = new TimeOnly(23, 59);

			await WriteMultipleRegisters(ct,
				(FoxESSRegisters.BATTERY_TIMEPERIOD1_CHARGE_FROM_GRID, 1),
				(FoxESSRegisters.BATTERY_TIMEPERIOD1_START_TIME, (ushort)now.ToFoxESSRegister()),
				(FoxESSRegisters.BATTERY_TIMEPERIOD1_END_TIME, (ushort)end.ToFoxESSRegister()),
				(FoxESSRegisters.BATTERY_TIMEPERIOD2_CHARGE_FROM_GRID, 0),
				(FoxESSRegisters.BATTERY_TIMEPERIOD2_START_TIME, 0),
				(FoxESSRegisters.BATTERY_TIMEPERIOD2_END_TIME, 0));

			return FoxErrorNumber.OK.ToString();
        }

		public async Task<string> ForceChargeAllToday(CancellationToken ct = default)
		{
			TimeOnly now = new TimeOnly(0, 1);
			TimeOnly end = new TimeOnly(23, 59);

            await WriteMultipleRegisters(ct,
                (FoxESSRegisters.BATTERY_TIMEPERIOD1_CHARGE_FROM_GRID, 1),
				(FoxESSRegisters.BATTERY_TIMEPERIOD1_START_TIME, (ushort)now.ToFoxESSRegister()),
				(FoxESSRegisters.BATTERY_TIMEPERIOD1_END_TIME, (ushort)end.ToFoxESSRegister()),
				(FoxESSRegisters.BATTERY_TIMEPERIOD2_CHARGE_FROM_GRID, 0),
				(FoxESSRegisters.BATTERY_TIMEPERIOD2_START_TIME, 0),
				(FoxESSRegisters.BATTERY_TIMEPERIOD2_END_TIME, 0));

			return FoxErrorNumber.OK.ToString();
		}

        public async Task<string> DisableForceCharge(CancellationToken ct = default)
        {
			await WriteMultipleRegisters(ct,
				(FoxESSRegisters.BATTERY_TIMEPERIOD1_CHARGE_FROM_GRID, 0),
				(FoxESSRegisters.BATTERY_TIMEPERIOD1_START_TIME, 0),
				(FoxESSRegisters.BATTERY_TIMEPERIOD1_END_TIME, 0),
			    (FoxESSRegisters.BATTERY_TIMEPERIOD2_CHARGE_FROM_GRID, 0),
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

		/// <summary>
		/// Write a set of registers to the inverter
		/// </summary>
		/// <param name="ct">cancellation token</param>
		/// <param name="updates">register updates</param>
		private async Task WriteMultipleRegisters(CancellationToken ct = default, params (FoxESSRegisters Register, ushort Value)[] updates)
		{
			AssertConfigured();

			ModbusTcpClient? client = null;
            try
            {
                client = new ModbusTcpClient();

                client.Connect(new IPEndPoint(IPAddress.Parse(_config!.IPAddress!), _config.Port), ModbusEndianness.BigEndian);

                ushort[] updateValues = updates.Select(u => u.Value).ToArray();

                int startAddress = updates.Min(u => (int)u.Register);

                await client.WriteMultipleRegistersAsync(_config.DeviceId, startAddress, updateValues, ct);

                _logger.LogInformation("Successfully written {value} to register(s) {register}.", $"[{(string.Join(',', updates.Select(u => u.Value)))}]", startAddress);
            }
            finally
            {
                client?.Disconnect();
            }
		}

		/// <summary>
		/// Read a register value and write it to a different register
		/// </summary>
		/// <param name="ct">cancellation token</param>
		private async Task<bool> CopySingleRegister(FoxESSRegisters originRegister, FoxESSRegisters destinationRegister, short minValue = 10, short maxValue = 100, CancellationToken ct = default)
        {
            AssertConfigured();

            ModbusTcpClient? client = null;
            try
            {
                client = new ModbusTcpClient();

                client.Connect(new IPEndPoint(IPAddress.Parse(_config!.IPAddress!), _config.Port), ModbusEndianness.BigEndian);

                // Read min SOC (on grid)
                short originalValue = client.ReadInputRegisters<short>(_config.DeviceId, (int)originRegister, 1).ToArray().FirstOrDefault();

                if (originalValue < minValue || originalValue > maxValue)
                {
                    _logger.LogWarning("Read value was outside bounds. No write will be performed.");

                    return false;
                }

                // Write new value
                await client.WriteSingleRegisterAsync(_config.DeviceId, (int)destinationRegister, originalValue, ct);

                _logger.LogInformation("Successfully copied value {originalValue} from {originRegister} to register {destinationRegister}.", originalValue, originRegister, destinationRegister);

                return true;
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
