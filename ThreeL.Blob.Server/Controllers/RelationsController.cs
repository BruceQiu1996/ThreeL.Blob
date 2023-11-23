using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using ThreeL.Blob.Application.Contract.Services;
using ThreeL.Blob.Shared.Application.Contract.Extensions;

namespace ThreeL.Blob.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RelationsController : ControllerBase
    {
        private readonly IRelationService _relationService;
        private readonly ILogger<RelationsController> _logger;

        public RelationsController(IRelationService relationService, ILogger<RelationsController> logger)
        {
            _relationService = relationService;
            _logger = logger;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            long.TryParse(HttpContext.User.Identity?.Name, out var userId);
            var result = await _relationService.GetRelationsAsync(userId);

            return result.ToActionResult();
        }

        [Authorize]
        [HttpGet("{Id}")]
        public async Task<IActionResult> GetSomeOne(long id)
        {
            long.TryParse(HttpContext.User.Identity?.Name, out var userId);
            var result = await _relationService.GetRelationAsync(userId,id);

            return result.ToActionResult();
        }

        [Authorize]
        [HttpGet("query/{key}")]
        public async Task<IActionResult> QueryRelations(string key)
        {
            long.TryParse(HttpContext.User.Identity?.Name, out var userId);
            var result = await _relationService.QueryRelationsByKeywordAsync(userId, key);

            return result.ToActionResult();
        }

        [Authorize]
        [HttpGet("applys")]
        public async Task<IActionResult> QueryApplys()
        {
            long.TryParse(HttpContext.User.Identity?.Name, out var userId);
            var result = await _relationService.QueryApplysAsync(userId);

            return result.ToActionResult();
        }
    }
}
