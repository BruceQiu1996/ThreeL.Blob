using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using ThreeL.Blob.Chat.Application.Contract.Dtos;
using ThreeL.Blob.Chat.Application.Contract.Services;
using ThreeL.Blob.Shared.Domain;

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
            await Clients.Client(Context.ConnectionId).SendAsync(HubConst.LoginSuccess);
        }

        [HubMethodName(HubConst.SendTextMessage)]
        [Authorize]
        public async Task SendTextMessage(TextMessageDto messageDto)
        {
            var id = long.Parse(Context.User.Identity.Name);
            await _chatService.SendTextMessageAsync(id, messageDto, Clients);
        }

        [HubMethodName(HubConst.SendFileMessage)]
        [Authorize]
        public async Task SendFileMessage(FileMessageDto messageDto)
        {
            var id = long.Parse(Context.User.Identity.Name);
            await _chatService.SendFileMessageAsync(id, messageDto, Clients, Context);
        }

        //TODO 文件和文件夹撤回，需要删除分享记录
        //TODO 服务器报错，统一回复客户端某条消息错误
        [HubMethodName(HubConst.SendWithdrawMessage)]
        [Authorize]
        public async Task SendWithdrawMessage(WithdrawMessageDto messageDto)
        {
            var id = long.Parse(Context.User.Identity.Name);
            await _chatService.SendWithdrawMessageAsync(id, messageDto, Clients, Context);
        }

        /// <summary>
        /// 发送好友申请
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        [HubMethodName(HubConst.SendAddFriendApply)]
        [Authorize]
        public async Task SendAddFriendApply(long target)
        {
            await _chatService.AddFriendApplyAsync(target, Clients, Context);
        }

        /// <summary>
        /// 处理好友请求
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        [HubMethodName(HubConst.HandleAddFriendApply)]
        [Authorize]
        public async Task HandleAddFriendApply(HandleAddFriendApplyDto handleAddFriendApplyDto)
        {
            await _chatService.HandleAddFriendApplyAsync(handleAddFriendApplyDto, Clients, Context);
        }

        /// <summary>
        /// 拉取某人的聊天记录
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        [HubMethodName(HubConst.FetchChatRecords)]
        [Authorize]
        public async Task QueryChatRecords(QueryChatRecordsDto queryChatRecordsDto)
        {
            await _chatService.QueryChatRecordsAsync(queryChatRecordsDto, Clients, Context);
        }
    }
}
