using System.Net;
using System.Text.Json;
using IceSync.ApiClient.Configs;
using IceSync.ApiClient.ResponseModels;
using Microsoft.Extensions.Options;
using NSubstitute;
using NUnit.Framework;
using RichardSzalay.MockHttp;

namespace IceSync.ApiClient.Tests
{
    [TestFixture]
    public class ApiClientTests
    {
        private IJwtTokenManager _jwtTokenManager;
        private IOptions<ApiClientConfig> _config;
        private ApiClient _apiClient;
        private HttpClient _httpClient;
        private MockHttpMessageHandler _messageHandler;

        private const string ApiBaseUrl = "https://api.example.com/";
        private const string? Token = "mock-jwt-token";
        private readonly ApiClientConfig _apiClientConfig = new()
        {
            ApiBaseUrl = ApiBaseUrl,
            ApiCompanyId = "company-id",
            ApiUserId = "user-id",
            ApiUserSecret = "user-secret"
        };

        [SetUp]
        public void SetUp()
        {
            _jwtTokenManager = Substitute.For<IJwtTokenManager>();
            _config = Options.Create(_apiClientConfig);

            _messageHandler = new MockHttpMessageHandler();
            _httpClient = _messageHandler.ToHttpClient();

            _apiClient = new ApiClient(_httpClient, _jwtTokenManager, _config);
        }

        [Test]
        public async Task GetWorkflowsAsync_ReturnsWorkflows()
        {
            var workflows = new List<Workflow>
            {
                new()
                {
                    Id = 1,
                    Name = "Test Workflow",
                    MultiExecBehavior = "MultiExecBehavior"
                }
            };
            var responseJson = JsonSerializer.Serialize(workflows);
            _messageHandler.When("/workflows")
                .Respond("application/json", responseJson);

            _jwtTokenManager.LoadTokenAsync().Returns(Task.FromResult(Token));
            _jwtTokenManager.IsTokenExpired(Token!).Returns(false);

            var result = await _apiClient.GetWorkflowsAsync();

            Assert.That(result, Is.Not.Null);
            Assert.That(JsonSerializer.Serialize(result), Is.EqualTo(responseJson));
        }

        [Test]
        public async Task RunWorkflowAsync_ValidId_ReturnsTrue()
        {
            _messageHandler.When("/workflows/*/run")
                .Respond(_ => new HttpResponseMessage(HttpStatusCode.OK));

            _jwtTokenManager.LoadTokenAsync().Returns(Task.FromResult(Token));
            _jwtTokenManager.IsTokenExpired(Token!).Returns(false);

            var result = await _apiClient.RunWorkflowAsync(1);

            Assert.That(result, Is.True);
        }

        [Test]
        public async Task RunWorkflowAsync_InvalidId_ReturnsFalse()
        {
            _messageHandler.When("/workflows/*/run")
                .Respond(_ => new HttpResponseMessage(HttpStatusCode.NotFound));

            _jwtTokenManager.LoadTokenAsync().Returns(Task.FromResult(Token));
            _jwtTokenManager.IsTokenExpired(Token!).Returns(false);

            var result = await _apiClient.RunWorkflowAsync(999);

            Assert.That(result, Is.False);
        }

        [Test]
        public async Task EnsureAuthenticatedAsync_TokenIsValid_SetsAuthorizationHeader()
        {
            _jwtTokenManager.LoadTokenAsync().Returns(Task.FromResult(Token));
            _jwtTokenManager.IsTokenExpired(Token!).Returns(false);
            var workflows = new List<Workflow>
            {
                new()
                {
                    Id = 1,
                    Name = "Test Workflow",
                    MultiExecBehavior = "MultiExecBehavior"
                }
            };
            var responseJson = JsonSerializer.Serialize(workflows);
            _messageHandler.When("/workflows")
                .Respond("application/json", responseJson);

            await _apiClient.GetWorkflowsAsync();

            Assert.That(_httpClient.DefaultRequestHeaders.Authorization, Is.Not.Null);
            Assert.That(_httpClient.DefaultRequestHeaders.Authorization.Scheme, Is.EqualTo("Bearer"));
            Assert.That(_httpClient.DefaultRequestHeaders.Authorization.Parameter, Is.EqualTo(Token));
        }

        [Test]
        public async Task EnsureAuthenticatedAsync_TokenIsExpired_AuthenticatesAndSetsNewToken()
        {
            _jwtTokenManager.LoadTokenAsync().Returns(Task.FromResult<string?>(null));
            _jwtTokenManager.IsTokenExpired(Arg.Any<string>()).Returns(true);

            var newToken = "new-jwt-token";
            var workflows = new List<Workflow>
            {
                new()
                {
                    Id = 1,
                    Name = "Test Workflow",
                    MultiExecBehavior = "MultiExecBehavior"
                }
            };
            var responseJson = JsonSerializer.Serialize(workflows);
            _messageHandler.When("/workflows")
                .Respond("application/json", responseJson);
            _messageHandler.When(HttpMethod.Post, "/authenticate")
                .Respond("application/json", newToken);

            await _apiClient.GetWorkflowsAsync();

            await _jwtTokenManager.Received().SaveTokenAsync(newToken);
            Assert.That(_httpClient.DefaultRequestHeaders.Authorization, Is.Not.Null);
            Assert.That(_httpClient.DefaultRequestHeaders.Authorization.Scheme, Is.EqualTo("Bearer"));
            Assert.That(_httpClient.DefaultRequestHeaders.Authorization.Parameter, Is.EqualTo(newToken));
        }

        [Test]
        public async Task SendWithRetryAsync_WhenUnauthorized_ReauthenticatesAndRetries()
        {
            _jwtTokenManager.LoadTokenAsync().Returns(Task.FromResult(Token));
            _jwtTokenManager.IsTokenExpired(Token!).Returns(false);
            var newToken = "new-jwt-token";
            var workflows = new List<Workflow>
            {
                new()
                {
                    Id = 1,
                    Name = "Test Workflow",
                    MultiExecBehavior = "MultiExecBehavior"
                }
            };
            var responseJson = JsonSerializer.Serialize(workflows);

            _messageHandler.When("/workflows")
                .Respond(req => req.Headers.Authorization.Parameter == Token
                    ? new HttpResponseMessage(HttpStatusCode.Unauthorized)
                    : new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(responseJson) });

            _messageHandler.When(HttpMethod.Post, "/authenticate")
                .Respond("application/json", newToken);

            var result = await _apiClient.GetWorkflowsAsync();

            await _jwtTokenManager.Received().SaveTokenAsync(newToken);
            Assert.That(_httpClient.DefaultRequestHeaders.Authorization, Is.Not.Null);
            Assert.That(_httpClient.DefaultRequestHeaders.Authorization.Scheme, Is.EqualTo("Bearer"));
            Assert.That(_httpClient.DefaultRequestHeaders.Authorization.Parameter, Is.EqualTo(newToken));
            Assert.That(JsonSerializer.Serialize(result), Is.EqualTo(responseJson));
        }

        [TearDown]
        public void TearDown()
        {
            _messageHandler.Clear();
            _messageHandler.Flush();
        }
    }
}
