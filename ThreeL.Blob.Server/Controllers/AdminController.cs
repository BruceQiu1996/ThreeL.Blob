using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ThreeL.Blob.Application.Contract.Dtos;
using ThreeL.Blob.Application.Contract.Services;
using ThreeL.Blob.Application.Contract.Validators.User;
using ThreeL.Blob.Domain.Aggregate.User;
using ThreeL.Blob.Shared.Application.Contract.Extensions;
using ThreeL.Blob.Shared.Application.Contract.Interceptors.Attributes;
using ThreeL.Blob.Shared.Domain.Metadata.User;

namespace ThreeL.Blob.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;
        private readonly ILogger<AdminController> _logger;
        public AdminController(IAdminService adminService, ILogger<AdminController> logger)
        {
            _logger = logger;
            _adminService = adminService;
        }

        [ParamValidate(typeof(UserLoginDtoValidator))]
        [HttpPost("login")]
        public async Task<ActionResult> Login(UserLoginDto userLoginDto)
        {
            try
            {
                var result = await _adminService.LoginAsync(userLoginDto);

                return result.ToActionResult();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return Problem();
            }
        }


        [Authorize(Roles = $"{nameof(Role.Admin)},{nameof(Role.SuperAdmin)}")]
        [HttpGet("users")]
        public async Task<ActionResult> QueryUsers([FromQuery] int page)
        {
            try
            {
                var result = await _adminService.QueryUsersAsync(page);

                return result.ToActionResult();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return Problem();
            }
        }

        [Authorize(Roles = $"{nameof(Role.Admin)},{nameof(Role.SuperAdmin)}")]
        [ParamValidate(typeof(UserCreationDtoValidator))]
        [HttpPost("users")]
        public async Task<ActionResult> Create(UserCreationDto creationDto)
        {
            try
            {
                long.TryParse(HttpContext.User.Identity?.Name, out var userId);
                var sresult = await _adminService.CreateUserAsync(creationDto, userId);

                return sresult.ToActionResult();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return Problem();
            }
        }
    }
}
