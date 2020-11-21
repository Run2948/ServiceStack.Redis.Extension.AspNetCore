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

        public WeatherForecastController(ILogger<WeatherForecastController> logger,IServiceStackRedisCache _redisCache)
        {
            _logger = logger;
            this._redisCache = _redisCache;
        }

        [HttpGet]
        public IEnumerable<WeatherForecast> Get()
        {
            CheckRedisFreeLicenseRequestLimit();

            var rng = new Random();
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            })
            .ToArray();
        }

        private void CheckRedisFreeLicenseRequestLimit()
        {
            _redisCache.SetString("word","ok");
            for (int i = 0; i < 9999; i++)
            {
                _logger.LogInformation($"The {i}st request: {_redisCache.GetString("word")}");
            }
        }
    }
}
