namespace AgendaSalud.AuthService.Application.Settings
{
    public class GoogleOAuthSettings
    {
        public string ClientId { get; set; } = string.Empty;
        public string ClientSecret { get; set; } = string.Empty;
        public string CallbackPath { get; set; } = "/auth/google/callback";
    }
}
