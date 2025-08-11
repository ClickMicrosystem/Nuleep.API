using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Nuleep.Models;
using Nuleep.Models.Response;
using Google.Apis.Auth;
using System.Net.Http;

namespace Nuleep.Business.Services
{
    public class GoogleOAuthService
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;

        public GoogleOAuthService(IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<GoogleProfile> GetProfileInfo(string code)
        {
            var clientId = _configuration["Google:ClientId"];
            var clientSecret = _configuration["Google:ClientSecret"];

            var httpClient = _httpClientFactory.CreateClient();

            var requestBody = new Dictionary<string, string>
            {
                { "code", code },
                { "client_id", clientId },
                { "client_secret", clientSecret },
                { "redirect_uri", "postmessage" },
                { "grant_type", "authorization_code" }
            };

            var requestContent = new FormUrlEncodedContent(requestBody);
            var response = await httpClient.PostAsync("https://oauth2.googleapis.com/token", requestContent);

            if (!response.IsSuccessStatusCode)
            {
                throw new UnauthorizedAccessException("Failed to exchange code for token.");
            }

            var responseString = await response.Content.ReadAsStringAsync();
            var tokenResponse = JsonSerializer.Deserialize<GoogleTokenResponse>(responseString);

            var payload = await GoogleJsonWebSignature.ValidateAsync(tokenResponse.IdToken, new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new[] { clientId }
            });

            return new GoogleProfile
            {
                Email = payload.Email,
                Sub = payload.Subject,
                Name = payload.Name,
                Picture = payload.Picture
            };
        }
    }
}
