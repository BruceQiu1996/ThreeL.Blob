using Microsoft.AspNetCore.SignalR;
using ThreeL.Blob.Chat.Application.Contract.Dtos;
using ThreeL.Blob.Chat.Application.Contract.Services;
using ThreeL.Blob.Shared.Application.Contract.Services;

namespace ThreeL.Blob.Chat.Application.Services
{
    public class ChatService : IChatService, IAppService
    {
        public async Task SendTextMessageAsync(long sender, TextMessageDto textMessageDto, IHubCallerClients clients)
        {
            if (sender != textMessageDto.From)
            {
                await clients.User(textMessageDto.From.ToString()).SendAsync("ReceiveTextMessage", new UserSendTextMessageToUserResultDto()
                {
                    Success = false,
                    Description = "用户数据异常"
                });

                return;
            }

            //TODO判断是否是好友以及消息存储

            //发送给两个人
            await clients.User(textMessageDto.From.ToString()).SendAsync("ReceiveTextMessage", new UserSendTextMessageToUserResultDto()
            {
                Message = textMessageDto,
            });

            await clients.User(textMessageDto.To.ToString()).SendAsync("ReceiveTextMessage", new UserSendTextMessageToUserResultDto()
            {
                Message = textMessageDto
            });
        }
    }
}
