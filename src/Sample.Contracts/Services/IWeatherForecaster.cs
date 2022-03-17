using System.Collections.Generic;
using System.Threading.Tasks;
using Sample.Models;

namespace Sample.Services
{
    public interface IWeatherForecaster
    {
        Task<IEnumerable<WeatherForecast>> GetForecastAsync();
    }
}
