using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.ServiceStackRedis;

namespace AspNetCore.WebSamples.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;
        private readonly IServiceStackRedisCache _redisCache;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, IServiceStackRedisCache redisCache)
        {
            _logger = logger;
            this._redisCache = redisCache;
        }

        [HttpGet]
        public IEnumerable<WeatherForecast> Get()
        {
#if DEBUG
            CheckRedisFreeLicenseRequestLimit();
#endif
            var array = _redisCache.Get<WeatherForecast[]>("WeatherForecast");
            if (array == null)
            {
                var rng = new Random();
                array = Enumerable.Range(1, 5).Select(index => new WeatherForecast
                {
                    Date = DateTime.Now.AddDays(index),
                    TemperatureC = rng.Next(-20, 55),
                    Summary = Summaries[rng.Next(Summaries.Length)]
                }).ToArray();

                // Cache for 30 minutes
                _redisCache.Set("WeatherForecast", array, 60 * 1 * 30);
            }

            return array;
        }

        private void CheckRedisFreeLicenseRequestLimit()
        {
            _redisCache.SetString("word", "ok");
            for (int i = 0; i < 9999; i++)
            {
                _logger.LogInformation($"The {i}st request: {_redisCache.GetString("word")}");
            }
        }
    }
}
