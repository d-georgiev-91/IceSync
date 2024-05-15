using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;

namespace IceSync.ApiClient;

public class JwtSecurityTokenHandlerWrapper : IJwtSecurityTokenHandler
{
    private readonly JwtSecurityTokenHandler _handler = new();

    public SecurityToken ReadToken(string token) => _handler.ReadToken(token);
}