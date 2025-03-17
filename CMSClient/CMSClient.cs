using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net.Mime;
using System.Text;
using System.Text.Json.Serialization;

namespace TakeHomeLibrary
{
    public class CMSClient
    {
        private string TenantId { get; set; }
        private string Username { get; set; }
        private string Password { get; set; }

        private string CurrentToken { get; set; } = string.Empty;
        private ulong CurrentTokenExpiryDate { get; set; } = default;

        private readonly HttpClient _client;

        public CMSClient(string baseAdress)
        {
            _client = new HttpClient
            {
                BaseAddress = new Uri(baseAdress)
            };
        }

        public async Task AuthenticateAsync(string tenantId, string username, string password)
        {
            var authCredentials = new
            {
                tenantId,
                username,
                password,
            };
            var request = new HttpRequestMessage
            {
                RequestUri = new Uri("/auth"),
                Method = HttpMethod.Post,
                Content = new StringContent(JsonConvert.SerializeObject(authCredentials), Encoding.UTF8, MediaTypeNames.Application.Json)
            };

            var response = await _client.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Authentication failed.");
            }

            var authData = await response.Content.ReadFromJsonAsync<AuthResponse>();
            CurrentToken = authData.BearerToken;
            CurrentTokenExpiryDate = authData.ExpiryDate;            
        }

        public async Task RefreshAsync()
        {
            var request = new HttpRequestMessage
            {
                RequestUri = new Uri("/auth/refresh"),
                Method = HttpMethod.Get
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", CurrentToken);

            var response = await _client.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Token refresh failed.");
            }

            var authData = await response.Content.ReadFromJsonAsync<AuthResponse>();
            CurrentToken = authData.BearerToken;
            CurrentTokenExpiryDate = authData.ExpiryDate;
        }
    }
}
