﻿using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using ThreeL.Blob.Application.Contract.Dtos;
using ThreeL.Blob.Application.Contract.Services;
using ThreeL.Blob.Domain.Aggregate.User;
using ThreeL.Blob.Infra.Core.Extensions.System;
using ThreeL.Blob.Infra.Redis;
using ThreeL.Blob.Infra.Repository.IRepositories;
using ThreeL.Blob.Shared.Application.Contract.Configurations;
using ThreeL.Blob.Shared.Application.Contract.Helpers;
using ThreeL.Blob.Shared.Application.Contract.Services;

namespace ThreeL.Blob.Application.Services
{
    public class UserService : IUserService, IAppService
    {
        private readonly IJwtService _jwtService;
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
                           IConfiguration configuration,
                           IRedisProvider redisProvider, IOptions<SystemOptions> systemOptions,
                           IMapper mapper,
                           IJwtService jwtService)
        {
            _jwtService = jwtService;
            _mapper = mapper;
            _redisProvider = redisProvider;
            _passwordHelper = passwordHelper;
            _userBasicRepository = userBasicRepository;
            _jwtOptions = jwtOptions.Value;
            _systemOptions = systemOptions.Value;
            _jwtBearerOptions = jwtBearerOptions.Get(JwtBearerDefaults.AuthenticationScheme);
            _configuration = configuration;
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
                user.LastLoginTime = DateTime.Now;
                await _userBasicRepository.UpdateAsync(user);
                var token = await _jwtService.CreateTokenAsync(user, userLoginDto.Origin);
                var respDto = _mapper.Map<UserLoginResponseDto>(user);
                respDto.RefreshToken = token.refreshToken;
                respDto.AccessToken = token.accessToken;
                respDto.Avatar = respDto.Avatar == null ? respDto.Avatar : respDto.Avatar.Replace(_configuration.GetSection("FileStorage:AvatarImagesLocation").Value!, null);

                //获取占用大小
                var len = user.Location.GetDirectoryLength();
                respDto.UsedSpaceSize = len;
                return new ServiceResult<UserLoginResponseDto>(respDto);
            }
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
            var refreshTokenId = identity.Claims.FirstOrDefault(c => c.Type == Const.CLAIM_REFRESHTOKEN)?.Value;
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

            var result = await _jwtService.CreateTokenAsync(user, token.Origin);

            return new UserRefreshTokenDto()
            {
                RefreshToken = result.refreshToken,
                AccessToken = result.accessToken
            };
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

        public async Task<ServiceResult<FileInfo>> UploadUserAvatarAsync(long userId, IFormFile file)
        {
            var user = await _userBasicRepository.GetAsync(userId);
            if (user == null) 
            {
                return new ServiceResult<FileInfo>(HttpStatusCode.BadRequest, "用户异常");
            }

            if (file.Length > 1024 * 1024 * 2)
            {
                return new ServiceResult<FileInfo>(HttpStatusCode.BadRequest, "图片大小不符要求");
            }

            var fileExtension = Path.GetExtension(file.FileName);
            var saveFolder = Path.Combine(_configuration.GetSection("FileStorage:AvatarImagesLocation").Value!, $"user-{userId}");
            var fileName = $"{Path.GetRandomFileName()}{fileExtension}";
            var fullName = Path.Combine(saveFolder, fileName);

            if (!Directory.Exists(saveFolder))
            {
                Directory.CreateDirectory(saveFolder);
            }

            using (var fs = File.Create(fullName))
            {
                await file.CopyToAsync(fs);
                await fs.FlushAsync();
            }

            user.Avatar = fullName;
            await _userBasicRepository.UpdateAsync(user);

            return new ServiceResult<FileInfo>(new FileInfo(fullName));
        }
    }
}
