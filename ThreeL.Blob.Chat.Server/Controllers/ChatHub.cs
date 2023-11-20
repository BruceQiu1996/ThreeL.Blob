using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using ThreeL.Blob.Chat.Application.Channels;
using ThreeL.Blob.Chat.Application.Contract.Dtos;
using ThreeL.Blob.Chat.Application.Contract.Services;

namespace ThreeL.Blob.Chat.Server.Controllers
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly IChatService _chatService;
        private readonly PushMessageToClientChannel _pushMessageToClientChannel;
        public ChatHub(IChatService chatService,PushMessageToClientChannel pushMessageToClientChannel)
        {
            _chatService = chatService;
            _pushMessageToClientChannel = pushMessageToClientChannel;
            _pushMessageToClientChannel.MessageHandler = async x =>
            {
                await Clients.User(x.id.ToString()).SendAsync(x.topic, x.body);
            };
        }

        public async override Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
            //登录成功
            await Clients.Client(Context.ConnectionId).SendAsync("LoginSuccess");
        }

        public async override Task OnDisconnectedAsync(Exception? exception)
        {
            var id = long.Parse(Context.User.Identity.Name);
        }

        [HubMethodName("SendTextMessage")]
        [Authorize]
        public async Task SendTextMessage(TextMessageDto messageDto)
        {
            var id = long.Parse(Context.User.Identity.Name);
            await _chatService.SendTextMessageAsync(id, messageDto, Clients);
        }
    }
}
