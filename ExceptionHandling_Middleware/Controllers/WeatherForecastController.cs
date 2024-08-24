using ExceptionHandling_Middleware.Models;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Text.Json;

namespace ExceptionHandling_Middleware.Controllers
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
        private readonly IHttpContextAccessor _httpContextAccessor;

        public WeatherForecastController(
            ILogger<WeatherForecastController> logger,
            IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Gets the weather forecast.
        /// </summary>
        /// <returns>A list of weather forecasts.</returns>
        /// 
        [HttpGet(Name = "GetWeatherForecast")]
        [SwaggerOperation(
            OperationId = "GetWeatherForecast",
            Summary = "This method will return 5 weather and 4 middlewares",
            Description = "Middlewares will make a chain call to next and will come back just like stack structure")]
        public IActionResult Get()
        //public IEnumerable<WeatherForecast> Get()
        {

            var firstMiddlewareResult = _httpContextAccessor.HttpContext?.Items["FirstMiddlewareResult"] as string;
            var secondMiddlewareResult = _httpContextAccessor.HttpContext?.Items["SecondMiddlewareResult"] as string;
            var culturalMiddleware = _httpContextAccessor.HttpContext?.Items["CulturalMiddleware"] as string;
            var loggingMiddleware = _httpContextAccessor.HttpContext?.Items["LoggingMiddleware"] as string;


            var Results = Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToList();

            var combinedResult = new CombinedResult();
            combinedResult.WeatherData = Results;
            combinedResult.FirstMiddlewareResult = firstMiddlewareResult;
            combinedResult.SecondMiddlewareResult = secondMiddlewareResult;
            combinedResult.CulturalMiddlewareResult = culturalMiddleware;
            combinedResult.LoggingMiddlewareResult = loggingMiddleware;

            return new ContentResult
            {
                Content = JsonSerializer.Serialize(combinedResult),
                ContentType = "application/json",
                StatusCode = 200
            };

            //works: only return weather data
            string weather = JsonSerializer.Serialize(Results);
            return new ContentResult
            {
                Content = weather,
                ContentType = "application/json",
                StatusCode = 200
            };

        }

        public class CombinedResult
        {
            public string FirstMiddlewareResult { get; set; }
            public string SecondMiddlewareResult { get; set; }
            public string CulturalMiddlewareResult { get; set; }
            public string LoggingMiddlewareResult { get; set; }
            public List<WeatherForecast> WeatherData { get; set; }
        }

    }
}
