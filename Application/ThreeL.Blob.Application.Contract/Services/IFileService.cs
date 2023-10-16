using ThreeL.Blob.Application.Contract.Dtos;
using ThreeL.Blob.Shared.Application.Contract.Services;

namespace ThreeL.Blob.Application.Contract.Services
{
    public interface IFileService
    {
        Task<ServiceResult<UploadFileResponseDto>> UploadAsync(UploadFileDto uploadFileDto, long userId);
        Task<ServiceResult<IEnumerable<FileObjDto>>> GetItemsAsync(long parentId, long userId);
    }
}
