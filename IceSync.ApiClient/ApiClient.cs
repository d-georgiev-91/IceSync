using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using IceSync.ApiClient.Configs;
using IceSync.ApiClient.ResponseModels;
using Microsoft.Extensions.Options;

namespace IceSync.ApiClient
{
    public class ApiClient : IApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly IJwtTokenManager _jwtTokenManager;
        private readonly IOptions<ApiClientConfig> _config;

        public ApiClient(HttpClient httpClient, IJwtTokenManager jwtTokenManager, IOptions<ApiClientConfig> config)
        {
            _httpClient = httpClient;
            _jwtTokenManager = jwtTokenManager;
            _config = config;
            _httpClient.BaseAddress = new Uri(_config.Value.ApiBaseUrl);
        }

        public async Task<IEnumerable<Workflow>?> GetWorkflowsAsync()
        {
            await EnsureAuthenticatedAsync();

            var response = await SendWithRetryAsync(() => _httpClient.GetAsync("/workflows"));

            var workflows = await response.Content.ReadFromJsonAsync<IEnumerable<Workflow>>();

            return workflows;
        }

        public async Task<bool> RunWorkflowAsync(int id)
        {
            await EnsureAuthenticatedAsync();

            var response = await SendWithRetryAsync(() => _httpClient.PostAsync($"/workflows/{id}/run", null), false);
            return response.IsSuccessStatusCode;
        }

        private async Task EnsureAuthenticatedAsync()
        {
            var token = await _jwtTokenManager.LoadTokenAsync();

            if (token != null && !_jwtTokenManager.IsTokenExpired(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            else
            {
                await AuthenticateAsync();
            }
        }

        private async Task AuthenticateAsync()
        {
            var requestBody = new
            {
                apiCompanyId = _config.Value.ApiCompanyId,
                apiUserId = _config.Value.ApiUserId,
                apiUserSecret = _config.Value.ApiUserSecret
            };

            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("/authenticate", content);
            response.EnsureSuccessStatusCode();

            var token = await response.Content.ReadAsStringAsync();
            await _jwtTokenManager.SaveTokenAsync(token);
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        private async Task<HttpResponseMessage> SendWithRetryAsync(Func<Task<HttpResponseMessage>> sendRequest, bool ensureSuccessStatusCode = true)
        {
            var response = await sendRequest();

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                await AuthenticateAsync();
                response = await sendRequest();
            }

            if (ensureSuccessStatusCode)
            {
                response.EnsureSuccessStatusCode();
            }

            return response;
        }
    }
}
