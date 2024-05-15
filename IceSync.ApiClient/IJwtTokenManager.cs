namespace IceSync.ApiClient;

public interface IJwtTokenManager
{
    bool IsTokenExpired(string token);
    
    Task<string?> LoadTokenAsync();

    Task SaveTokenAsync(string token);
}