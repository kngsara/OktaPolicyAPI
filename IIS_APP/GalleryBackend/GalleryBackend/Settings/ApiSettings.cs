namespace OktaBackend.Settings
{
    public class ApiSettings
    {
        public bool UseCustomApi { get; set; }

        public string OktaDomain { get; set; } = string.Empty;

        public string OktaPoliciesUrl { get; set; } = string.Empty;

        public string OktaApiToken { get; set; } = string.Empty;
    }
}