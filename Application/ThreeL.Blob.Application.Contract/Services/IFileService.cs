using ThreeL.Blob.Application.Contract.Dtos;
using ThreeL.Blob.Shared.Application.Contract.Interceptors.Attributes;
using ThreeL.Blob.Shared.Application.Contract.Services;

namespace ThreeL.Blob.Application.Contract.Services
{
    public interface IFileService
    {
        [Uow]
        Task<ServiceResult<UploadFileResponseDto>> UploadAsync(UploadFileDto uploadFileDto, long userId);
        Task<ServiceResult<IEnumerable<FileObjDto>>> GetItemsAsync(long parentId, long userId);
        Task<ServiceResult<IEnumerable<FolderSimpleDto>>> GetAllFoldersAsync(long userId);
        [Uow]
        Task<ServiceResult> DeleteItemsAsync(long[] fileIds, long userId);
        Task<ServiceResult> UpdateFileObjectNameAsync(UpdateFileObjectNameDto updateFileObjectDto, long userId);
        Task<ServiceResult> UpdateFileObjectsLocationAsync(UpdateFileObjectLocationDto updateFileObjectLocationDto, long userId);
        Task<ServiceResult<FileObjDto>> CreateFolderAsync(FolderCreationDto folderCreationDto, long userId);
        Task<ServiceResult<IEnumerable<FolderTreeCreationResponseDto>>> CreateFoldersAsync(FolderTreeCreationDto folderTreeCreationDto,long userId);
        Task<ServiceResult<FileUploadingStatusDto>> GetUploadingStatusAsync(long fileId, long userId);
        Task<ServiceResult<FileUploadingStatusDto>> CancelUploadingAsync(long fileId, long userId);
        Task<ServiceResult> CancelDownloadingAsync(string taskId, long userId);
        [Uow]
        Task<ServiceResult<DownloadFileResponseDto>> DownloadAsync(long fileId, long userId);
        [Uow]
        Task<ServiceResult<DownloadFileResponseDto>> DownloadSharedAsync(string token, long userId);
        Task<ServiceResult<PreDownloadFolderResponseDto>> PreDownloadFolderAsync(long folderId);
    }
}
