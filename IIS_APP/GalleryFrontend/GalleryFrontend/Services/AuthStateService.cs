namespace GalleryFrontend.Services
{
    public class AuthStateService
    {
        public string AccessToken { get; private set; } = string.Empty;

        public string RefreshToken { get; private set; } = string.Empty;

        public string Role { get; private set; } = string.Empty;

        public bool IsLoggedIn => !string.IsNullOrWhiteSpace(AccessToken);

        public bool IsFullAccess => Role == "FullAccess";

        public bool IsReadOnly => Role == "ReadOnly";

        public void SetLogin(string accessToken, string refreshToken, string role)
        {
            AccessToken = accessToken;
            RefreshToken = refreshToken;
            Role = role;
        }

        public void Logout()
        {
            AccessToken = string.Empty;
            RefreshToken = string.Empty;
            Role = string.Empty;
        }
    }
}