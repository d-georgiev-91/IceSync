using Microsoft.IdentityModel.Tokens;

namespace IceSync.ApiClient
{
    public interface IJwtSecurityTokenHandler
    {
        SecurityToken ReadToken(string token);
    }
}
