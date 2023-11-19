using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using ThreeL.Blob.Chat.Application.Contract.Dtos;
using ThreeL.Blob.Chat.Application.Contract.Services;

namespace ThreeL.Blob.Chat.Server.Controllers
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly IChatService _chatService;
        public ChatHub(IChatService chatService)
        {
            _chatService = chatService;
        }

        public async override Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
            //登录成功
            await Clients.Client(Context.ConnectionId).SendAsync("" + "LoginSuccess");
        }

        public async override Task OnDisconnectedAsync(Exception? exception)
        {
            var id = long.Parse(Context.User.Identity.Name);
        }

        [Authorize]
        public async Task SendMessage(TextMessageDto messageDto)
        {
            var id = long.Parse(Context.User.Identity.Name);
            await _chatService.SendTextMessageAsync(id, messageDto, Clients);
        }
    }
}
