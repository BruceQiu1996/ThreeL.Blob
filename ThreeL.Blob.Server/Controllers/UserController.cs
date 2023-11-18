using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ThreeL.Blob.Application.Contract.Dtos;
using ThreeL.Blob.Application.Contract.Services;
using ThreeL.Blob.Shared.Application.Contract.Extensions;
using ThreeL.Blob.Shared.Domain.Metadata.User;

namespace ThreeL.ContextAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<UserController> _logger;

        public UserController(IUserService userService, ILogger<UserController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        [Authorize(Roles = $"{nameof(Role.Admin)},{nameof(Role.SuperAdmin)}")]
        [HttpPost]
        public async Task<ActionResult> Create(UserCreationDto creationDto)
        {
            try
            {
                long.TryParse(HttpContext.User.Identity?.Name, out var userId);
                var sresult = await _userService.CreateUserAsync(creationDto, userId);

                return sresult.ToActionResult();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return Problem();
            }
        }

        [AllowAnonymous]
        [HttpPost("refresh/token")]
        public async Task<ActionResult> RefreshToken(UserRefreshTokenDto tokenDto)
        {
            try
            {
                var resp = await _userService.RefreshAuthTokenAsync(tokenDto);
                if (resp == null)
                    return BadRequest();

                return Ok(resp);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return Problem();
            }
        }

        [HttpPost("login")]
        public async Task<ActionResult> Login(UserLoginDto userLoginDto)
        {
            try
            {
                var result = await _userService.AccountLoginAsync(userLoginDto);

                return result.ToActionResult();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return Problem();
            }
        }

        [Authorize]
        [HttpPut("password")]
        public async Task<ActionResult> ModifyPassword(UserModifyPasswordDto userModifyPasswordDto)
        {
            try
            {
                long.TryParse(HttpContext.User.Identity?.Name, out var userId);
                var sresult = await _userService.ModifyUserPasswordAsync(userModifyPasswordDto, userId);

                return sresult.ToActionResult();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return Problem();
            }
        }

        [Authorize]
        [HttpPost("upload-avatar")]
        public async Task<ActionResult> UploadAvatar([FromForm] IFormFile file)
        {
            try
            {
                long.TryParse(HttpContext.User.Identity?.Name, out var userId);
                var result = await _userService.UploadUserAvatarAsync(userId,file);

                if (result.Value == null)
                {
                    return result.ToActionResult();
                }

                return new FileStreamResult(new FileStream(result.Value.FullName, FileMode.Open), "application/octet-stream") { FileDownloadName = result.Value.Name };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return Problem();
            }
        }
    }
}
