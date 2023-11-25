using Microsoft.IdentityModel.Tokens;
using ThreeL.Blob.Domain.Aggregate.User;

namespace ThreeL.Blob.Shared.Application.Contract.Services
{
    public interface IJwtService
    {
        IEnumerable<SecurityKey> ValidateIssuerSigningKey(string token, SecurityToken securityToken, string kid, TokenValidationParameters validationParameters);
        Task<(string accessToken, string refreshToken)> CreateTokenAsync(User user, string origin);
    }
}
