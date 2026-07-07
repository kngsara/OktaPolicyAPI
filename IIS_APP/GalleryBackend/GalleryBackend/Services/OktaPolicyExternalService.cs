using OktaBackend.Models;
using OktaBackend.Settings;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text.Json;

namespace OktaBackend.Services
{
    public interface IOktaPolicyExternalService
    {
        Task<List<OktaPolicy>> GetPoliciesAsync();
    }

    public class OktaPolicyExternalService : IOktaPolicyExternalService
    {
        private readonly HttpClient _httpClient;
        private readonly ApiSettings _settings;

        public OktaPolicyExternalService(HttpClient httpClient, IOptions<ApiSettings> options)
        {
            _httpClient = httpClient;
            _settings = options.Value;
        }

        public async Task<List<OktaPolicy>> GetPoliciesAsync()
        {
            if (string.IsNullOrWhiteSpace(_settings.OktaDomain))
            {
                throw new InvalidOperationException("OktaDomain nije postavljen u appsettings.json.");
            }

            if (string.IsNullOrWhiteSpace(_settings.OktaApiToken))
            {
                throw new InvalidOperationException("OktaApiToken nije postavljen u appsettings.json.");
            }

            var requestUrl =
                $"{_settings.OktaDomain.TrimEnd('/')}/{_settings.OktaPoliciesUrl.TrimStart('/')}";

            var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);

            request.Headers.Authorization =
                new AuthenticationHeaderValue("SSWS", _settings.OktaApiToken);

            request.Headers.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            var response = await _httpClient.SendAsync(request);

            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException(
                    $"Okta API error {(int)response.StatusCode} {response.ReasonPhrase}: {json}");
            }

            var policies = JsonSerializer.Deserialize<List<OktaPolicyDto>>(
                json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            if (policies == null)
            {
                return new List<OktaPolicy>();
            }

            return policies.Select(policy => new OktaPolicy
            {
                Id = 0,
                OktaId = policy.Id ?? string.Empty,
                Name = policy.Name ?? "No policy name",
                Type = policy.Type ?? "Unknown",
                Status = policy.Status ?? "Unknown",
                Priority = policy.Priority,
                Link = $"{_settings.OktaDomain.TrimEnd('/')}/api/v1/policies/{policy.Id}",
                CreatedAt = policy.Created ?? DateTime.UtcNow,
                LastUpdatedAt = policy.LastUpdated
            }).ToList();
        }
    }
}