using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ThreeL.Blob.Application.Contract.Dtos;
using ThreeL.Blob.Application.Contract.Services;
using ThreeL.Blob.Application.Contract.Validators.FileObject;
using ThreeL.Blob.Shared.Application.Contract.Extensions;
using ThreeL.Blob.Shared.Application.Contract.Interceptors.Attributes;

namespace ThreeL.Blob.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileController : ControllerBase
    {
        private readonly IFileService _fileService;
        private readonly ILogger<FileController> _logger;
        public FileController(IFileService fileService, ILogger<FileController> logger)
        {
            _fileService = fileService;
            _logger = logger;
        }

        [HttpPost]
        [ParamValidate(typeof(UploadFileDtoValidator))]
        [Authorize]
        public async Task<ActionResult> UploadFileAsync(UploadFileDto uploadFileDto)
        {
            try
            {
                long.TryParse(HttpContext.User.Identity?.Name, out var userId);
                var result = await _fileService.UploadAsync(uploadFileDto, userId);

                return result.ToActionResult();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return Problem();
            }
        }

        [HttpGet("preDownloadFolder/{folderId}")]
        [Authorize]
        public async Task<ActionResult> PreDownloadFolderAsync(long folderId)
        {
            try
            {
                var result = await _fileService.PreDownloadFolderAsync(folderId);

                return result.ToActionResult();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return Problem();
            }
        }

        [HttpPost("download/{fileId}")]
        [Authorize]
        public async Task<ActionResult> DownloadFileAsync(long fileId)
        {
            try
            {
                long.TryParse(HttpContext.User.Identity?.Name, out var userId);
                var result = await _fileService.DownloadAsync(fileId, userId);

                return result.ToActionResult();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return Problem();
            }
        }

        [HttpPost("download-shared/{token}")]
        [Authorize]
        public async Task<ActionResult> DownloadFileAsync(string token)
        {
            try
            {
                long.TryParse(HttpContext.User.Identity?.Name, out var userId);
                var result = await _fileService.DownloadSharedAsync(token, userId);

                return result.ToActionResult();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return Problem();
            }
        }

        [HttpGet("upload-status/{fileId}")]
        [Authorize]
        public async Task<ActionResult> UploadingStatusAsync(long fileId)
        {
            try
            {
                long.TryParse(HttpContext.User.Identity?.Name, out var userId);
                var result = await _fileService.GetUploadingStatusAsync(fileId, userId);

                return result.ToActionResult();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return Problem();
            }
        }

        [HttpPost("folder")]
        [ParamValidate(typeof(FolderCreationDtoValidator))]
        [Authorize]
        public async Task<ActionResult> CreateFolderAsync(FolderCreationDto folderCreationDto)
        {
            try
            {
                long.TryParse(HttpContext.User.Identity?.Name, out var userId);
                var result = await _fileService.CreateFolderAsync(folderCreationDto, userId);

                return result.ToActionResult();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return Problem();
            }
        }

        [HttpPost("folders")]
        [Authorize]
        public async Task<ActionResult> CreateFoldersAsync(FolderTreeCreationDto folderTreeCreationDto)
        {
            try
            {
                long.TryParse(HttpContext.User.Identity?.Name, out var userId);
                var result = await _fileService.CreateFoldersAsync(folderTreeCreationDto, userId);

                return result.ToActionResult();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return Problem();
            }
        }

        [HttpPost("update-name")]
        [Authorize]
        public async Task<ActionResult> UpdateItemNameAsync(UpdateFileObjectNameDto updateFileObjectNameDto)
        {
            try
            {
                long.TryParse(HttpContext.User.Identity?.Name, out var userId);
                var result = await _fileService.UpdateFileObjectNameAsync(updateFileObjectNameDto, userId);

                return result.ToActionResult();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return Problem();
            }
        }

        [HttpPost("update-location")]
        [Authorize]
        public async Task<ActionResult> UpdateItemsLocationAsync(UpdateFileObjectLocationDto updateFileObjectLocationDto)
        {
            try
            {
                long.TryParse(HttpContext.User.Identity?.Name, out var userId);
                var result = await _fileService.UpdateFileObjectsLocationAsync(updateFileObjectLocationDto, userId);

                return result.ToActionResult();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return Problem();
            }
        }

        [HttpPut("cancel/{fileId}")]
        [Authorize]
        public async Task<ActionResult> CancelUploadAsync(long fileId)
        {
            try
            {
                long.TryParse(HttpContext.User.Identity?.Name, out var userId);
                var result = await _fileService.CancelUploadingAsync(fileId, userId);

                return result.ToActionResult();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return Problem();
            }
        }

        [HttpPut("cancel-download/{taskId}")]
        [Authorize]
        public async Task<ActionResult> CancelDownloadAsync(string taskId)
        {
            try
            {
                long.TryParse(HttpContext.User.Identity?.Name, out var userId);
                var result = await _fileService.CancelDownloadingAsync(taskId, userId);

                return result.ToActionResult();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return Problem();
            }
        }

        [HttpGet("{parent}")]
        [Authorize]
        public async Task<ActionResult> GetItemsAsync(long parent)
        {
            try
            {
                long.TryParse(HttpContext.User.Identity?.Name, out var userId);
                var result = await _fileService.GetItemsAsync(parent, userId);

                return result.ToActionResult();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return Problem();
            }
        }

        [HttpGet("folders")]
        [Authorize]
        public async Task<ActionResult> GetFoldersAsync()
        {
            try
            {
                long.TryParse(HttpContext.User.Identity?.Name, out var userId);
                var result = await _fileService.GetAllFoldersAsync(userId);

                return result.ToActionResult();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return Problem();
            }
        }

        [HttpPost("delete")]
        [Authorize]
        public async Task<ActionResult> DeleteItemsAsync(DeleteFileObjectsDto deleteFileObjects)
        {
            try
            {
                long.TryParse(HttpContext.User.Identity?.Name, out var userId);
                var result = await _fileService.DeleteItemsAsync(deleteFileObjects.FileIds, userId);

                return result.ToActionResult();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return Problem();
            }
        }

        [HttpPost("zip")]
        [ParamValidate(typeof(CompressFileObjectsDtoValidator))]
        [Authorize]
        public async Task<ActionResult> CompressFileObjectsAsync(CompressFileObjectsDto compressFileObjectsDto)
        {
            try
            {
                long.TryParse(HttpContext.User.Identity?.Name, out var userId);
                var result = await _fileService.CompressFileObjectsAsync(userId, compressFileObjectsDto);

                return result.ToActionResult();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return Problem();
            }
        }
    }
}
