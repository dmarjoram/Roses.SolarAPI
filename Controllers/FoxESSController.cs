using Microsoft.AspNetCore.Mvc;
using Roses.SolarAPI.Models;
using Roses.SolarAPI.Services;
using Roses.SolarAPI.Extensions;
using Roses.SolarAPI.Configuration;
using FluentModbus;
using System.Net;

namespace Roses.SolarAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FoxESSController : ControllerBase
    {
        private readonly ILogger<FoxESSController> _logger;
        private readonly ForecastService _forecastService;
        private readonly SolarConfiguration _config;

        public FoxESSController(ILogger<FoxESSController> logger, ForecastService forecastService, IConfiguration configuration)
        {
            _logger = logger;
            _forecastService = forecastService;
            _config = configuration.GetSection(nameof(SolarConfiguration)).Get<SolarConfiguration>();
        }

        [HttpGet]
        [Route("BatteryConfiguration")]
        public BatteryConfiguration GetBatteryConfiguration(CancellationToken ct)
        {
            ModbusTcpClient client = new ModbusTcpClient();

            client.Connect(new IPEndPoint(IPAddress.Parse("192.168.0.153"), 502), ModbusEndianness.BigEndian);

            ushort[] values = client.ReadHoldingRegisters<ushort>(247, 41001, 6).ToArray();
            ushort minSocValue = client.ReadHoldingRegisters<ushort>(247, 41009, 1).ToArray().FirstOrDefault();
            ushort minSocOnGridValue = client.ReadHoldingRegisters<ushort>(247, 41011, 1).ToArray().FirstOrDefault();

            client.Disconnect();

            return new BatteryConfiguration()
            {
                ForceCharge1 = values[1] != 0 || values[2] != 0,
                ChargeFromGrid1 = values[0] == 1,
                StartDate1 = values[1].ToTimeOnlyFoxESS(),
                EndDate1 = values[2].ToTimeOnlyFoxESS(),
                ForceCharge2 = values[4] != 0 || values[5] != 0,
                ChargeFromGrid2 = values[3] == 1,
                StartDate2 = values[4].ToTimeOnlyFoxESS(),
                EndDate2 = values[5].ToTimeOnlyFoxESS(),
                MinSoC = minSocValue,
                MinSoCOnGrid = minSocOnGridValue
            };
        }

        [HttpGet]
        [Route("EstimateProduction")]
        public async Task<SolarProduction> EstimateProduction(int startHour, int startMin, int durationHours, CancellationToken ct)
        {
            if (durationHours < _config.IntervalHours)
            {
                throw new ArgumentOutOfRangeException(nameof(durationHours), $"You need to provide {nameof(durationHours)} greater or equal to {Math.Round(_config.IntervalHours, MidpointRounding.AwayFromZero)}.");
            }

            Forecast forecast = await _forecastService.Estimate(ct);

            var watts = forecast.Result?.Watts?.ToDateTimeKey();

            DateTime startDate = new DateTime(DateTime.Now.Year, 
                DateTime.Now.Month, 
                DateTime.Now.Day, startHour, startMin, 0);

            DateTime endDate = startDate.AddHours(durationHours);

            double totalWattsHours = watts?
                .Where(kvp => kvp.Key >= startDate && kvp.Key <= endDate)
                .Select(kvp => kvp.Value * _config.IntervalHours) // Watts * NumberOfHours
                .Sum() ?? 0;

            return new SolarProduction()
            {
                StartDate = startDate,
                EndDate = endDate,
                Hours = durationHours,
                Kwh = Math.Round(totalWattsHours / 1000d, 2)
            };
        }
    }
}