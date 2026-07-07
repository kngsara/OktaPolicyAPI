using OktaBackend.Protos;
using Grpc.Core;
using System.Xml.Linq;

namespace OktaBackend.Grpc
{
    public class WeatherGrpcService : WeatherService.WeatherServiceBase
    {
        private readonly HttpClient _httpClient;

        public WeatherGrpcService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public override async Task<WeatherResponse> GetTemperaturesByCity(
            WeatherRequest request,
            ServerCallContext context)
        {
            var response = new WeatherResponse();

            if (string.IsNullOrWhiteSpace(request.City))
            {
                return response;
            }

            var searchTerm = request.City.Trim().ToLower();

            var xmlContent = await _httpClient.GetStringAsync("https://vrijeme.hr/hrvatska_n.xml");

            var document = XDocument.Parse(xmlContent);

            var gradovi = document.Descendants("Grad");

            foreach (var grad in gradovi)
            {
                var naziv = grad.Element("GradIme")?.Value?.Trim();
                var temperatura = grad.Element("Podatci")?.Element("Temp")?.Value?.Trim();

                if (string.IsNullOrWhiteSpace(naziv))
                {
                    continue;
                }

                if (naziv.ToLower().Contains(searchTerm))
                {
                    response.Results.Add(new WeatherInfo
                    {
                        City = naziv,
                        Temperature = string.IsNullOrWhiteSpace(temperatura)
                            ? "N/A"
                            : temperatura
                    });
                }
            }

            return response;
        }
    }
}