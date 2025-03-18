using Newtonsoft.Json;
using System;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net.Mime;
using System.Text;
using TakeHome.Models;

namespace TakeHome.Client
{
    public class CMSClient
    {
        private string CurrentToken { get; set; } = string.Empty;
        private ulong ExpiryDate { get; set; } = default;

        private readonly HttpClient _client;

        public CMSClient(string baseAdress)
        {
            _client = new HttpClient
            {
                BaseAddress = new Uri(baseAdress)
            };
        }

        public async Task<List<Document>> GetDocumentsMetadataAsync(List<string> documentIds)
        {
            var allDocuments = await GetDocumentsMetadataAsync();
            return allDocuments.Where(doc => documentIds.Contains(doc.Id)).ToList();
        }

        public async Task<List<Document>> GetDocumentsMetadataAsync()
        {
            var request = new HttpRequestMessage
            {
                RequestUri = new Uri("documents", UriKind.Relative),
                Method = HttpMethod.Get
            };

            var response = await SendAuthenticatedRequestAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Failed to retrieve documents. Status code: {response.StatusCode}");
            }

            return await response.Content.ReadFromJsonAsync<List<Document>>();
        }

        public async Task<Document> GetDocumentAsync(string documentId)
        {
            var request = new HttpRequestMessage
            {
                RequestUri = new Uri($"document/{documentId}", UriKind.Relative),
                Method = HttpMethod.Get
            };

            var response = await SendAuthenticatedRequestAsync(request);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                throw new Exception("Document not found.");
            }

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Failed to retrieve document. Status code: {response.StatusCode}");
            }

            return await response.Content.ReadFromJsonAsync<Document>();
        }

        public async Task CreateDocumentAsync(Document document)
        {
            var request = new HttpRequestMessage
            {
                RequestUri = new Uri("document", UriKind.Relative),
                Method = HttpMethod.Post,
                Content = new StringContent(JsonConvert.SerializeObject(document), Encoding.UTF8, MediaTypeNames.Application.Json)
            };

            var response = await SendAuthenticatedRequestAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Failed to create document. Status code: {response.StatusCode}");
            }
        }


        private async Task<HttpResponseMessage> SendAuthenticatedRequestAsync(HttpRequestMessage request)
        {
            // Sending request with the current token
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", CurrentToken);
            var response = await _client.SendAsync(request);

            // Refreshing token and retry
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                try
                {
                    await RefreshAsync();
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", CurrentToken);
                    response = await _client.SendAsync(request);
                }
                catch
                {
                    throw;
                }
            }

            return response;
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
                RequestUri = new Uri("auth", UriKind.Relative),
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
            ExpiryDate = authData.ExpiryDate;
        }

        public async Task RefreshAsync()
        {
            var request = new HttpRequestMessage
            {
                RequestUri = new Uri("auth/refresh", UriKind.Relative),
                Method = HttpMethod.Get
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Authorization", CurrentToken);

            var response = await _client.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Token refresh failed.");
            }

            var authData = await response.Content.ReadFromJsonAsync<AuthResponse>();
            CurrentToken = authData.BearerToken;
            ExpiryDate = authData.ExpiryDate;
        }
    }
}