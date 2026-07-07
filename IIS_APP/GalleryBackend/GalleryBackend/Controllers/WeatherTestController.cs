using Microsoft.AspNetCore.Mvc;
using System.Xml.Linq;

namespace OktaBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WeatherTestController : ControllerBase
    {
        private readonly HttpClient _httpClient;

        public WeatherTestController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        [HttpGet("{city}")]
        public async Task<IActionResult> GetTemperaturesByCity(string city)
        {
            if (string.IsNullOrWhiteSpace(city))
            {
                return BadRequest("Naziv grada je obavezan.");
            }

            var searchTerm = city.Trim().ToLower();

            var xmlContent = await _httpClient.GetStringAsync("https://vrijeme.hr/hrvatska_n.xml");

            var document = XDocument.Parse(xmlContent);

            var results = document
                .Descendants("Grad")
                .Select(grad => new
                {
                    City = grad.Element("GradIme")?.Value?.Trim(),
                    Temperature = grad.Element("Podatci")?.Element("Temp")?.Value?.Trim()
                })
                .Where(x =>
                    !string.IsNullOrWhiteSpace(x.City) &&
                    x.City.ToLower().Contains(searchTerm))
                .ToList();

            return Ok(results);
        }
    }
}