using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Sample.Models;
using Sample.Services;

namespace Sample.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private readonly IWeatherForecaster weatherForecaster;

        public WeatherForecastController(IWeatherForecaster weatherForecaster)
        {
            this.weatherForecaster = weatherForecaster;
        }

        [HttpGet]
        public Task<IEnumerable<WeatherForecast>> GetAsync()
        {
            return this.weatherForecaster.GetForecastAsync();
        }
    }
}
