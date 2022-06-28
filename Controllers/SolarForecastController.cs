using Microsoft.AspNetCore.Mvc;
using Roses.SolarAPI.Models;
using Roses.SolarAPI.Services;
using Roses.SolarAPI.Extensions;
using Roses.SolarAPI.Configuration;

namespace Roses.SolarAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SolarForecastController : ControllerBase
    {
        private readonly ILogger<SolarForecastController> _logger;
        private readonly ForecastService _forecastService;
        private readonly SolarConfiguration _config;

        public SolarForecastController(ILogger<SolarForecastController> logger, ForecastService forecastService, IConfiguration configuration)
        {
            _logger = logger;
            _forecastService = forecastService;
            _config = configuration.GetSection(nameof(SolarConfiguration)).Get<SolarConfiguration>();
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