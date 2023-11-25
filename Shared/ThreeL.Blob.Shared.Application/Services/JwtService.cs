using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using ThreeL.Blob.Domain.Aggregate.User;
using ThreeL.Blob.Infra.Redis;
using ThreeL.Blob.Shared.Application.Contract.Configurations;
using ThreeL.Blob.Shared.Application.Contract.Services;

namespace ThreeL.Blob.Shared.Application.Services
{
    public class JwtService : IJwtService, IAppService
    {
        private readonly IOptions<JwtOptions> _jwtOptions;
        private readonly IOptions<SystemOptions> _systemOptions;
        private readonly IRedisProvider _redisProvider;
        public JwtService(IOptions<JwtOptions> jwtOptions,
                          IOptions<SystemOptions> systemOptions,
                          IRedisProvider redisProvider,
                          bool generateKey = false)
        {
            _jwtOptions = jwtOptions;
            _systemOptions = systemOptions;
            _redisProvider = redisProvider;
            if (generateKey)
            {
                GenerateSecretKeys();
            }
        }

        public void GenerateSecretKeys()
        {
            //随机创建token
            var _ = Task.Run(async () =>
            {
                do
                {
                    var jwtSetting = _jwtOptions.Value.ToEntity();
                    jwtSetting.Issuer = _systemOptions.Value.Name;
                    jwtSetting.SecretExpireAt = DateTime.Now.AddSeconds(_jwtOptions.Value.SecretExpireSeconds);
                    //默认设置jwt secret key每三天过期
                    await _redisProvider.HSetAsync(Const.REDIS_JWT_SECRET_KEY, $"{_systemOptions.Value.Name}-{DateTime.Now}",
                        jwtSetting, TimeSpan.FromSeconds(_jwtOptions.Value.SecretExpireSeconds), When.Always);

                    await Task.Delay(_jwtOptions.Value.SecretExpireSeconds * 1000 - 60 * 60 * 1000 * 3);
                } while (true);
            });
        }

        public IEnumerable<SecurityKey> ValidateIssuerSigningKey(string token, SecurityToken securityToken, string kid, TokenValidationParameters validationParameters)
        {
            var settings = _redisProvider.HGetAllAsync<JwtSetting>(Const.REDIS_JWT_SECRET_KEY).Result;
            foreach (var item in settings.Where(x => x.Value.Issuer == securityToken.Issuer))
            {
                yield return new SymmetricSecurityKey(Encoding.UTF8.GetBytes(item.Value.SecretKey));
            }
        }

        public async Task<(string accessToken, string refreshToken)> CreateTokenAsync(User user, string origin)
        {
            var settings = await _redisProvider.HGetAllAsync<JwtSetting>(Const.REDIS_JWT_SECRET_KEY);
            //拿到一个最晚过期的token
            var setting = settings.OrderByDescending(x => x.Value.SecretExpireAt).FirstOrDefault(x => x.Key.StartsWith(_systemOptions.Value.Name)
                && x.Value.Issuer == _systemOptions.Value.Name).Value;

            if (setting == null)
                throw new Exception("token获取异常");

            var refreshToken = await CreateRefreshTokenAsync(user.Id);
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, user.Id.ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Sid,user.UserName),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
                new Claim(Const.CLAIM_REFRESHTOKEN,refreshToken.refreshTokenId)
            };

            var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(setting.SecretKey));
            var algorithm = SecurityAlgorithms.HmacSha256;
            var signingCredentials = new SigningCredentials(secretKey, algorithm);
            var jwtSecurityToken = new JwtSecurityToken(
                    _systemOptions.Value.Name,             //Issuer
                    origin,                         //Audience TODO 客户端携带客户端类型头
                    claims,
                    null,
                    DateTime.Now.AddSeconds(_jwtOptions.Value.TokenExpireSeconds),    //expires
                    signingCredentials               //Credentials
            );

            return (new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken), refreshToken.refreshToken);
        }

        private async Task<(string refreshTokenId, string refreshToken)> CreateRefreshTokenAsync(long userId)
        {
            var tokenId = Guid.NewGuid().ToString("N");
            var rnBytes = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(rnBytes);
            var token = Convert.ToBase64String(rnBytes);
            await _redisProvider.StringSetAsync($"refresh-token:{userId}-{tokenId}", token, TimeSpan.FromSeconds(_jwtOptions.Value.RefreshTokenExpireSeconds));

            return (tokenId, token);
        }
    }
}
