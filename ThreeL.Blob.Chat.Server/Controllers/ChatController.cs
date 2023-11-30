using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ThreeL.Blob.Chat.Application.Contract.Services;
using ThreeL.Blob.Infra.Core.Extensions.System;
using ThreeL.Blob.Shared.Application.Contract.Extensions;

namespace ThreeL.Blob.Chat.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;
        private readonly ILogger<ChatController> _logger;
        public ChatController(IChatService chatService, ILogger<ChatController> logger)
        {
            _logger = logger;
            _chatService = chatService;
        }

        /// <summary>
        /// 拉取某人的聊天记录
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        [HttpGet("{target}/{time}")]
        [Authorize]
        public async Task<ActionResult> QueryChatRecords(long target, long time)
        {
            try
            {
                long.TryParse(HttpContext.User.Identity?.Name, out var userId);
                return (await _chatService.QueryChatRecordsAsync(userId, target, time.ToDateTime())).ToActionResult();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());

                return Problem();
            }
        }
    }
}
