using Microsoft.IdentityModel.Tokens;

namespace ThreeL.Blob.Application.Contract.Services
{
    public interface IJwtService
    {
        IEnumerable<SecurityKey> ValidateIssuerSigningKey(string token, SecurityToken securityToken, string kid, TokenValidationParameters validationParameters);
    }
}
