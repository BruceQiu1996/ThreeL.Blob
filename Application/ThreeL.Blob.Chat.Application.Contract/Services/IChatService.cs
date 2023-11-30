using Microsoft.AspNetCore.SignalR;
using ThreeL.Blob.Chat.Application.Contract.Dtos;
using ThreeL.Blob.Shared.Application.Contract.Services;

namespace ThreeL.Blob.Chat.Application.Contract.Services
{
    public interface IChatService
    {
        Task SendTextMessageAsync(long sender, TextMessageDto textMessageDto, IHubCallerClients clients);
        Task SendFileMessageAsync(long sender, FileMessageDto fileMessageDto, IHubCallerClients clients, HubCallerContext hubCallerContext);
        Task SendWithdrawMessageAsync(long sender, WithdrawMessageDto withdrawMessageDto, IHubCallerClients clients, HubCallerContext hubCallerContext);
        Task AddFriendApplyAsync(long target, IHubCallerClients clients, HubCallerContext hubCallerContext);
        Task HandleAddFriendApplyAsync(HandleAddFriendApplyDto handleAddFriendApplyDto, IHubCallerClients clients, HubCallerContext hubCallerContext);
        Task<ServiceResult<QueryChatRecordResponseDto>> QueryChatRecordsAsync(long sender, long target, DateTime dateTime);
    }
}
