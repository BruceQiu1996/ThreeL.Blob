using ThreeL.Blob.Application.Contract.Dtos;
using ThreeL.Blob.Application.Contract.Dtos.Management;
using ThreeL.Blob.Shared.Application.Contract.Services;

namespace ThreeL.Blob.Application.Contract.Services
{
    public interface IAdminService
    {
        Task<ServiceResult<MUserLoginResponseDto>> LoginAsync(UserLoginDto userLoginDto);
        Task<ServiceResult<MQueryUsersResponseDto>> QueryUsersAsync(int page);
    }
}
