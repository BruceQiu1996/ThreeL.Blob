using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using System.Text;
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
    }
}
