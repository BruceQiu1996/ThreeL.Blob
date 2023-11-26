using Microsoft.AspNetCore.Http;
using ThreeL.Blob.Application.Contract.Dtos;
using ThreeL.Blob.Shared.Application.Contract.Interceptors.Attributes;
using ThreeL.Blob.Shared.Application.Contract.Services;

namespace ThreeL.Blob.Application.Contract.Services
{
    public interface IUserService
    {
        Task<ServiceResult> ModifyUserPasswordAsync(UserModifyPasswordDto modifyPasswordDto, long creator);
        Task<UserRefreshTokenDto> RefreshAuthTokenAsync(UserRefreshTokenDto token);
        Task<ServiceResult<UserLoginResponseDto>> AccountLoginAsync(UserLoginDto userLoginDto);
        [Uow]
        Task<ServiceResult<FileInfo>> UploadUserAvatarAsync(long userId, IFormFile file);
    }
}
