using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using ThreeL.Blob.Application.Contract.Dtos;
using ThreeL.Blob.Application.Contract.Services;
using ThreeL.Blob.Domain.Aggregate.User;
using ThreeL.Blob.Infra.Redis;
using ThreeL.Blob.Infra.Repository.IRepositories;
using ThreeL.Blob.Shared.Application.Contract.Configurations;
using ThreeL.Blob.Shared.Application.Contract.Helpers;
using ThreeL.Blob.Shared.Application.Contract.Services;

namespace ThreeL.Blob.Application.Services
{
    public class UserService : IUserService, IAppService
    {
        private const string RefreshTokenIdClaimType = "refresh_token_id";
        private readonly IConfiguration _configuration;
        private readonly JwtOptions _jwtOptions;
        private readonly JwtBearerOptions _jwtBearerOptions;
        private readonly SystemOptions _systemOptions;
        private readonly IEfBasicRepository<User, long> _userBasicRepository;
        private readonly PasswordHelper _passwordHelper;
        private readonly IRedisProvider _redisProvider;
        private readonly IMapper _mapper;
        public UserService(IEfBasicRepository<User, long> userBasicRepository,
                           PasswordHelper passwordHelper,
                           IOptions<JwtOptions> jwtOptions,
                           IOptionsSnapshot<JwtBearerOptions> jwtBearerOptions,
                           IRedisProvider redisProvider, IOptions<SystemOptions> systemOptions,
                           IMapper mapper)
        {
            _mapper = mapper;
            _redisProvider = redisProvider;
            _passwordHelper = passwordHelper;
            _userBasicRepository = userBasicRepository;
            _jwtOptions = jwtOptions.Value;
            _systemOptions = systemOptions.Value;
            _jwtBearerOptions = jwtBearerOptions.Get(JwtBearerDefaults.AuthenticationScheme);
        }

        public async Task<ServiceResult<UserLoginResponseDto>> AccountLoginAsync(UserLoginDto userLoginDto)
        {
            var user = await _userBasicRepository.FirstOrDefaultAsync(x => x.UserName == userLoginDto.UserName);
            if (user == null)
                return new ServiceResult<UserLoginResponseDto>(HttpStatusCode.BadRequest, "用户名或密码错误");

            if (!_passwordHelper.VerifyHashedPassword(userLoginDto.Password, user.Password))
            {
                return new ServiceResult<UserLoginResponseDto>(HttpStatusCode.BadRequest, "用户名或密码错误");
            }
            else
            {
                user.LastLoginTime = DateTime.UtcNow;
                await _userBasicRepository.UpdateAsync(user);
                var token = await CreateTokenAsync(user, userLoginDto.Origin);
                var respDto = _mapper.Map<UserLoginResponseDto>(user);
                respDto.RefreshToken = token.refreshToken;
                respDto.AccessToken = token.accessToken;

                return new ServiceResult<UserLoginResponseDto>(respDto);
            }
        }

        private async Task<(string accessToken, string refreshToken)> CreateTokenAsync(User user, string origin)
        {
            var settings = await _redisProvider.HGetAllAsync<JwtSetting>(Const.REDIS_JWT_SECRET_KEY);
            //拿到一个最晚过期的token
            var setting = settings.OrderByDescending(x => x.Value.SecretExpireAt).FirstOrDefault(x => x.Key.StartsWith(_systemOptions.Name)
                && x.Value.Issuer == _systemOptions.Name).Value;

            if (setting == null)
                throw new Exception("token获取异常");

            var refreshToken = await CreateRefreshTokenAsync(user.Id);
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, user.Id.ToString()),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
                new Claim(RefreshTokenIdClaimType,refreshToken.refreshTokenId)
            };

            var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(setting.SecretKey));
            var algorithm = SecurityAlgorithms.HmacSha256;
            var signingCredentials = new SigningCredentials(secretKey, algorithm);
            var jwtSecurityToken = new JwtSecurityToken(
                    _systemOptions.Name,             //Issuer
                    origin,                         //Audience TODO 客户端携带客户端类型头
                    claims,
                    null,
                    DateTime.Now.AddSeconds(_jwtOptions.TokenExpireSeconds),    //expires
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
            await _redisProvider.StringSetAsync($"refresh-token:{userId}-{tokenId}", token, TimeSpan.FromSeconds(_jwtOptions.RefreshTokenExpireSeconds));

            return (tokenId, token);
        }

        public async Task<UserRefreshTokenDto> RefreshAuthTokenAsync(UserRefreshTokenDto token)
        {
            var validationParameters = _jwtBearerOptions.TokenValidationParameters.Clone();
            validationParameters.ValidateLifetime = false; // 此时对于jwt的过期与否已经不关心
            var handler = _jwtBearerOptions.SecurityTokenValidators.OfType<JwtSecurityTokenHandler>().FirstOrDefault()
                ?? new JwtSecurityTokenHandler();
            var principal = handler.ValidateToken(token.AccessToken, validationParameters, out _);

            var identity = principal.Identities.First();
            var userId = identity.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
            var refreshTokenId = identity.Claims.FirstOrDefault(c => c.Type == RefreshTokenIdClaimType)?.Value;
            var refreshToken = await _redisProvider.StringGetAsync($"refresh-token:{userId}-{refreshTokenId}");
            if (refreshToken != token.RefreshToken)
            {
                return null;
            }

            await _redisProvider.KeyDelAsync($"refresh-token:{userId}-{refreshTokenId}");
            var user = await _userBasicRepository.FirstOrDefaultAsync(x => x.Id == long.Parse(userId));
            if (user == null)
            {
                return null;
            }

            var result = await CreateTokenAsync(user, token.Origin);

            return new UserRefreshTokenDto()
            {
                RefreshToken = result.refreshToken,
                AccessToken = result.accessToken
            };
        }

        public async Task<ServiceResult> CreateUserAsync(UserCreationDto creationDto, long creator)
        {
            var user = creationDto.ToUser(creator);
            user.Password = _passwordHelper.HashPassword(creationDto.Password);
            var userLocation = Path.Combine(_configuration.GetSection("FileStorage:RootLocation").Value,user.UserName);
            if (!Directory.Exists(userLocation)) 
            {
                Directory.CreateDirectory(userLocation);
            }
            user.Location = userLocation;
            await _userBasicRepository.InsertAsync(user);

            return new ServiceResult();
        }

        public async Task<ServiceResult> ModifyUserPasswordAsync(UserModifyPasswordDto modifyPasswordDto, long creator)
        {
            var user = await _userBasicRepository.GetAsync(creator);
            if (user == null)
                return new ServiceResult(HttpStatusCode.BadRequest, "用户数据异常");

           var flag  = _passwordHelper.VerifyHashedPassword(modifyPasswordDto.OldPassword, user.Password);
            if (!flag) 
            {
                return new ServiceResult(HttpStatusCode.BadRequest, "密码错误");
            }

            user.Password = _passwordHelper.HashPassword(modifyPasswordDto.NewPassword);
            await _userBasicRepository.UpdateAsync(user);

            return new ServiceResult();
        }
    }
}
