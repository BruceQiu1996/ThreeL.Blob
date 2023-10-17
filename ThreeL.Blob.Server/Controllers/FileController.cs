using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ThreeL.Blob.Application.Contract.Dtos;
using ThreeL.Blob.Application.Contract.Services;
using ThreeL.Blob.Shared.Application.Contract.Extensions;

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

        [HttpPost("folder")]
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
    }
}
