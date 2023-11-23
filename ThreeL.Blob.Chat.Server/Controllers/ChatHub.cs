﻿using Microsoft.AspNetCore.Authorization;
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
    }
}
