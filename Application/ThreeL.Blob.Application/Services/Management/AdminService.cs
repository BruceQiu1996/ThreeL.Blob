using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Net;
using ThreeL.Blob.Application.Contract.Dtos;
using ThreeL.Blob.Application.Contract.Dtos.Management;
using ThreeL.Blob.Application.Contract.Services;
using ThreeL.Blob.Domain.Aggregate.User;
using ThreeL.Blob.Infra.Redis;
using ThreeL.Blob.Infra.Repository.IRepositories;
using ThreeL.Blob.Shared.Application.Contract.Helpers;
using ThreeL.Blob.Shared.Application.Contract.Services;

namespace ThreeL.Blob.Application.Services.Management
{
    public class AdminService : IAdminService, IAppService
    {
        private readonly IEfBasicRepository<User, long> _userBasicRepository;
        private readonly PasswordHelper _passwordHelper;
        private readonly IRedisProvider _redisProvider;
        private readonly IMapper _mapper;
        private readonly IJwtService _jwtService;

        public AdminService(IEfBasicRepository<User, long> userBasicRepository,
                            PasswordHelper passwordHelper,
                            IRedisProvider redisProvider,
                            IMapper mapper,
                            IJwtService jwtService)
        {
            _mapper = mapper;
            _jwtService = jwtService;
            _redisProvider = redisProvider;
            _userBasicRepository = userBasicRepository;
            _passwordHelper = passwordHelper;
        }

        public async Task<ServiceResult<MUserLoginResponseDto>> LoginAsync(UserLoginDto userLoginDto)
        {
            var user = await _userBasicRepository.FirstOrDefaultAsync(x => x.UserName == userLoginDto.UserName);
            if (user == null)
                return new ServiceResult<MUserLoginResponseDto>(HttpStatusCode.BadRequest, "用户名或密码错误");

            if (!_passwordHelper.VerifyHashedPassword(userLoginDto.Password, user.Password))
            {
                return new ServiceResult<MUserLoginResponseDto>(HttpStatusCode.BadRequest, "用户名或密码错误");
            }
            else
            {
                user.LastLoginTime = DateTime.Now;
                await _userBasicRepository.UpdateAsync(user);
                var token = await _jwtService.CreateTokenAsync(user, userLoginDto.Origin);
                var respDto = _mapper.Map<MUserLoginResponseDto>(user);
                respDto.AccessToken = token.accessToken;

                return new ServiceResult<MUserLoginResponseDto>(respDto);
            }
        }

        public async Task<ServiceResult<MQueryUsersResponseDto>> QueryUsersAsync(int page)
        {
            var usersCount = await _userBasicRepository.CountAsync(ignoreFilters: true);
            var users = await _userBasicRepository.All(ignoreFilters: true).OrderByDescending(x => x.CreateTime).Skip(page * 10).Take(10).ToListAsync();

            return new ServiceResult<MQueryUsersResponseDto>
            {
                Value = new MQueryUsersResponseDto()
                {
                    Count = usersCount,
                    Users = users.Select(x => _mapper.Map<MUserBriefResponseDto>(x))
                }
            };
        }
    }
}
