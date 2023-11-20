using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
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

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            long.TryParse(HttpContext.User.Identity?.Name, out var userId);
            var result = await _relationService.GetRelationsAsync(userId);

            return result.ToActionResult();
        }

        [HttpPost("addFriend/{target}")]
        public async Task<IActionResult> AddFriend(long target)
        {
            long.TryParse(HttpContext.User.Identity?.Name, out var userId);
            var result = await _relationService.AddFriendApplyAsync(userId, target);

            return result.ToActionResult();
        }

        [HttpGet("query/{key}")]
        public async Task<IActionResult> QueryRelations(string key)
        {
            long.TryParse(HttpContext.User.Identity?.Name, out var userId);
            var result = await _relationService.QueryRelationsByKeywordAsync(userId, key);

            return result.ToActionResult();
        }
    }
}
