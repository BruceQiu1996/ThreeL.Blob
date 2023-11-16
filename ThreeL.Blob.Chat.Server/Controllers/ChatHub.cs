using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace ThreeL.Blob.Chat.Server.Controllers
{
    [Authorize]
    public class ChatHub : Hub
    {
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
    }
}
