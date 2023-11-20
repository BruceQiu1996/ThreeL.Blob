using Microsoft.AspNetCore.SignalR;
using ThreeL.Blob.Chat.Application.Contract.Dtos;

namespace ThreeL.Blob.Chat.Application.Contract.Services
{
    public interface IChatService
    {
        Task SendTextMessageAsync(long sender, TextMessageDto textMessageDto, IHubCallerClients clients);
}
}
