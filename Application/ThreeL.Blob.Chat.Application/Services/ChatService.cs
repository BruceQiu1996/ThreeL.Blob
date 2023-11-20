using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using ThreeL.Blob.Chat.Application.Contract.Dtos;
using ThreeL.Blob.Chat.Application.Contract.Services;
using ThreeL.Blob.Chat.Domain.Entities;
using ThreeL.Blob.Infra.Repository.IRepositories;
using ThreeL.Blob.Shared.Application.Contract.Services;

namespace ThreeL.Blob.Chat.Application.Services
{
    public class ChatService : IChatService, IAppService
    {
        private readonly IMongoRepository<ChatRecord> _chatRecordRepository;
        private readonly IMapper _mapper;
        public ChatService(IMongoRepository<ChatRecord> chatRecordRepository, IMapper mapper)
        {
            _mapper = mapper;
            _chatRecordRepository = chatRecordRepository;
        }

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

            textMessageDto.RemoteSendTime = DateTime.Now;
            //TODO判断是否是好友以及消息存储
            await _chatRecordRepository.AddAsync(_mapper.Map<ChatRecord>(textMessageDto));

            //发送给两个人
            await clients.User(textMessageDto.From.ToString()).SendAsync("ReceiveTextMessage", new UserSendTextMessageToUserResultDto()
            {
                Message = textMessageDto,
                Success = true
            });

            await clients.User(textMessageDto.To.ToString()).SendAsync("ReceiveTextMessage", new UserSendTextMessageToUserResultDto()
            {
                Message = textMessageDto,
                Success = true
            });
        }
    }
}
