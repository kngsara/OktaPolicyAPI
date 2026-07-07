using GalleryFrontend.Models;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Xml.Linq;

namespace GalleryFrontend.Services
{
    public class OktaPolicyApiService
    {
        private readonly HttpClient _httpClient;
        private readonly AuthStateService _authState;

        public OktaPolicyApiService(HttpClient httpClient, AuthStateService authState)
        {
            _httpClient = httpClient;
            _authState = authState;
        }

        private void AddAuthorizationHeader()
        {
            _httpClient.DefaultRequestHeaders.Authorization = null;

            if (!string.IsNullOrWhiteSpace(_authState.AccessToken))
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", _authState.AccessToken);
            }
        }

        public async Task<LoginResponse?> LoginAsync(LoginRequest request)
        {
            var response = await _httpClient.PostAsJsonAsync("api/Auth/login", request);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content.ReadFromJsonAsync<LoginResponse>();
        }

        public async Task<List<OktaPolicy>> GetOktaPoliciesAsync()
        {
            AddAuthorizationHeader();

            var policies = await _httpClient.GetFromJsonAsync<List<OktaPolicy>>("api/OktaPolicies");

            return policies ?? new List<OktaPolicy>();
        }

        public async Task<bool> CreateOktaPolicyAsync(OktaPolicy policy)
        {
            AddAuthorizationHeader();

            var response = await _httpClient.PostAsJsonAsync("api/OktaPolicies", policy);

            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteOktaPolicyAsync(int id)
        {
            AddAuthorizationHeader();

            var response = await _httpClient.DeleteAsync($"api/OktaPolicies/{id}");

            return response.IsSuccessStatusCode;
        }

        public async Task<List<WeatherInfo>> GetWeatherByCityAsync(string city)
        {
            if (string.IsNullOrWhiteSpace(city))
            {
                return new List<WeatherInfo>();
            }

            var results = await _httpClient.GetFromJsonAsync<List<WeatherInfo>>(
                $"api/WeatherTest/{Uri.EscapeDataString(city)}");

            return results ?? new List<WeatherInfo>();
        }

        public async Task<List<SoapOktaPolicy>> SearchOktaPoliciesSoapAsync(string term)
        {
            if (string.IsNullOrWhiteSpace(term))
            {
                return new List<SoapOktaPolicy>();
            }

            var soapEnvelope = $"""
            <soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/"
                              xmlns:tem="http://tempuri.org/">
               <soapenv:Header/>
               <soapenv:Body>
                  <tem:SearchOktaPolicies>
                     <tem:term>{System.Security.SecurityElement.Escape(term)}</tem:term>
                  </tem:SearchOktaPolicies>
               </soapenv:Body>
            </soapenv:Envelope>
            """;

            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                "OktaPolicySoapService.asmx");

            request.Content = new StringContent(
                soapEnvelope,
                Encoding.UTF8,
                "text/xml");

            request.Headers.Add(
                "SOAPAction",
                "http://tempuri.org/IOktaPolicySoapService/SearchOktaPolicies");

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                return new List<SoapOktaPolicy>();
            }

            var responseXml = await response.Content.ReadAsStringAsync();

            var soapDocument = XDocument.Parse(responseXml);

            var resultElement = soapDocument
                .Descendants()
                .FirstOrDefault(x => x.Name.LocalName == "SearchOktaPoliciesResult");

            if (resultElement == null || string.IsNullOrWhiteSpace(resultElement.Value))
            {
                return new List<SoapOktaPolicy>();
            }

            var resultXml = resultElement.Value;

            var resultDocument = XDocument.Parse(resultXml);

            return resultDocument
                .Descendants("oktaPolicy")
                .Select(policy => new SoapOktaPolicy
                {
                    OktaId = policy.Element("oktaId")?.Value ?? string.Empty,
                    Name = policy.Element("name")?.Value ?? string.Empty,
                    Type = policy.Element("type")?.Value ?? string.Empty,
                    Status = policy.Element("status")?.Value ?? string.Empty,
                    Priority = int.TryParse(policy.Element("priority")?.Value, out var priority) ? priority : 0,
                    Link = policy.Element("link")?.Value ?? string.Empty,
                    CreatedAt = DateTime.TryParse(policy.Element("createdAt")?.Value, out var createdAt)
                        ? createdAt
                        : DateTime.MinValue,
                    LastUpdatedAt = DateTime.TryParse(policy.Element("lastUpdatedAt")?.Value, out var lastUpdatedAt)
                        ? lastUpdatedAt
                        : null
                })
                .ToList();
        }

        public async Task<string> ExportOktaPoliciesXmlAsync()
        {
            AddAuthorizationHeader();

            var response = await _httpClient.GetAsync("api/OktaPolicies/export-xml");

            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return $"XML export nije uspio: {content}";
            }

            return content;
        }

        public async Task<List<OktaPolicy>> GetOktaPoliciesGraphQlAsync()
        {
            AddAuthorizationHeader();

            var requestBody = new
            {
                query = """
                    query {
                      oktaPolicies {
                        id
                        oktaId
                        name
                        type
                        status
                        priority
                        link
                        createdAt
                        lastUpdatedAt
                      }
                    }
                    """
            };

            var response = await _httpClient.PostAsJsonAsync("graphql", requestBody);

            if (!response.IsSuccessStatusCode)
            {
                return new List<OktaPolicy>();
            }

            var graphQlResponse = await response.Content.ReadFromJsonAsync<GraphQlOktaPoliciesResponse>();

            return graphQlResponse?.Data?.OktaPolicies ?? new List<OktaPolicy>();
        }

        public async Task<bool> UpdateOktaPolicyAsync(int id, OktaPolicy policy)
        {
            AddAuthorizationHeader();

            var response = await _httpClient.PutAsJsonAsync($"api/OktaPolicies/{id}", policy);

            return response.IsSuccessStatusCode;
        }
    }
}