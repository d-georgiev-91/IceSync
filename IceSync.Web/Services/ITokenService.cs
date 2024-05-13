namespace IceSync.Web.Services;

public interface ITokenService
{
    Task<string> GetTokenAsync();
}