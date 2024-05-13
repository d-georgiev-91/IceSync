using System.Text.Json;
using System.Text;
using System.Text.Json.Serialization;

namespace IceSync.Web.Services;

public class TokenService : ITokenService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private string _token;
    private DateTime _tokenExpiration;

    public TokenService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<string> GetTokenAsync()
    {
        if (!string.IsNullOrEmpty(_token) && DateTime.UtcNow < _tokenExpiration)
        {
            return _token;
        }
        
        var client = _httpClientFactory.CreateClient();

        var requestBody = new
        {
            apiCompanyId = "ice-cream-ood",
            apiUserId = "ice-api-user",
            apiUserSecret = "n3yR7Bsk7El4"
        };

        var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json-patch+json");
        var request = new HttpRequestMessage(HttpMethod.Post, "https://api-test.universal-loader.com/authenticate")
        {
            Content = content
        };

        var response = await client.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var responseContent = await response.Content.ReadAsStringAsync();
        //var authResponse = JsonSerializer.Deserialize<AuthResponse>(responseContent);

        _token = responseContent;
        _tokenExpiration = DateTime.UtcNow.AddMinutes(59);

        return _token;
    }

    private class AuthResponse
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; }

        [JsonPropertyName("token_type")]
        public string TokenType { get; set; }

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }
    }
}