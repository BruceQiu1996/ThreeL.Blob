using AutoMapper;
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using ThreeL.Blob.Chat.Application.Contract.Configurations;
using ThreeL.Blob.Chat.Application.Contract.Dtos;
using ThreeL.Blob.Chat.Application.Contract.Protos;
using ThreeL.Blob.Chat.Application.Contract.Services;
using ThreeL.Blob.Chat.Domain.Entities;
using ThreeL.Blob.Infra.Redis;
using ThreeL.Blob.Infra.Repository.IRepositories;
using ThreeL.Blob.Shared.Application.Contract.Configurations;
using ThreeL.Blob.Shared.Application.Contract.Services;
using ThreeL.Blob.Shared.Domain;

namespace ThreeL.Blob.Chat.Application.Services
{
    public class ChatService : IChatService, IAppService
    {
        private readonly IMongoRepository<ChatRecord> _chatRecordRepository;
        private readonly IMapper _mapper;
        private readonly IRedisProvider _redisProvider;
        private readonly ContextApiGrpcOptions _contextApiGrpcOptions;
        private RpcContextAPIService.RpcContextAPIServiceClient _rpcContextAPIServiceClient;

        public ChatService(IMongoRepository<ChatRecord> chatRecordRepository,
                           IMapper mapper,
                           IOptions<ContextApiGrpcOptions> options,
                           IRedisProvider redisProvider)
        {
            _contextApiGrpcOptions = options.Value;
            _mapper = mapper;
            _redisProvider = redisProvider;
            _chatRecordRepository = chatRecordRepository;

            var channel = GrpcChannel.ForAddress($"http://{_contextApiGrpcOptions.Host}:{_contextApiGrpcOptions.Port}", new GrpcChannelOptions()
            {
                HttpHandler = new HttpClientHandler()
                {
                    AutomaticDecompression = System.Net.DecompressionMethods.GZip,
                    AllowAutoRedirect = true,
                    ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
                },
                MaxRetryAttempts = _contextApiGrpcOptions.RetryTimes,
                Credentials = ChannelCredentials.Insecure, //不使用安全连接 //TODO grpc ssl
            });

            _rpcContextAPIServiceClient = new RpcContextAPIService.RpcContextAPIServiceClient(channel);
        }

        public async Task AddFriendApplyAsync(long target, IHubCallerClients clients, HubCallerContext hubCallerContext)
        {
            var id = long.Parse(hubCallerContext.User.Identity.Name);
            var token = hubCallerContext.GetHttpContext().Request.Headers["Authorization"];
            var resp = await _rpcContextAPIServiceClient.AddFriendApplyAsync(new AddFriendApplyRequest()
            {
                Target = target
            }, new Metadata() { { "Authorization", $"{token}" } });

            if (resp.Success)
            {
                await clients.User(id.ToString()).SendAsync(HubConst.NewAddFriendApply,
                    HubMessageResponseDto<object>.GetNewDefaultSuccessInstance());
                await clients.User(target.ToString()).SendAsync(HubConst.NewAddFriendApply,
                    HubMessageResponseDto<object>.GetNewDefaultSuccessInstance());
            }
            else
            {
                await clients.User(id.ToString()).SendAsync(HubConst.NewAddFriendApply,
                    HubMessageResponseDto<object>.GetNewDefaultFailInstance(resp.Message));
            }
        }

        public async Task HandleAddFriendApplyAsync(HandleAddFriendApplyDto handleAddFriendApplyDto, IHubCallerClients clients, HubCallerContext hubCallerContext)
        {
            var id = long.Parse(hubCallerContext.User.Identity.Name);
            var token = hubCallerContext.GetHttpContext().Request.Headers["Authorization"];
            var resp = await _rpcContextAPIServiceClient.HandleAddFriendApplyAsync(new HandleAddFriendApplyRequest()
            {
                ApplyId = handleAddFriendApplyDto.ApplyId,
                IsAgree = handleAddFriendApplyDto.Access
            }, new Metadata() { { "Authorization", $"{token}" } });

            if (resp.Success)
            {
                await clients.User(resp.Passiver.ToString()).SendAsync(HubConst.AddFriendApplyResult,
                    HubMessageResponseDto<HandleAddFriendApplyResponseDto>.GetNewDefaultSuccessInstance(data: new HandleAddFriendApplyResponseDto()
                    {
                        IsAgree = handleAddFriendApplyDto.Access,
                        FriendId = resp.Activer
                    }));
                await clients.User(resp.Activer.ToString()).SendAsync(HubConst.AddFriendApplyResult,
                    HubMessageResponseDto<HandleAddFriendApplyResponseDto>.GetNewDefaultSuccessInstance(data: new HandleAddFriendApplyResponseDto()
                    {
                        IsAgree = handleAddFriendApplyDto.Access,
                        FriendId = resp.Passiver
                    }));
            }
            else
            {
                await clients.User(id.ToString()).SendAsync(HubConst.AddFriendApplyResult,
                        HubMessageResponseDto<HandleAddFriendApplyResponseDto>.GetNewDefaultFailInstance(resp.Message));
            }
        }

        public async Task SendFileMessageAsync(long sender, FileMessageDto fileMessageDto, IHubCallerClients clients, HubCallerContext hubCallerContext)
        {
            fileMessageDto.From = sender;
            var token = hubCallerContext.GetHttpContext().Request.Headers["Authorization"];
            if (!await IsFriendAsync(fileMessageDto.From, fileMessageDto.To))
            {
                await clients.User(fileMessageDto.From.ToString()).SendAsync(HubConst.ReceiveTextMessage, new HubMessageResponseDto<FileMessageDto>()
                {
                    Success = false,
                    Message = "好友关系异常",
                    Data = fileMessageDto
                });

                return;
            };

            var resp = await _rpcContextAPIServiceClient.SendFileAsync(new SendFileRequest() 
            {
                FileId = fileMessageDto.FileObjectId,
                Target = fileMessageDto.To,
            }, new Metadata() { { "Authorization", $"{token}" } });

            var fileMessageResponseDto = _mapper.Map<FileMessageResponseDto>(fileMessageDto);
            fileMessageResponseDto.Token = resp.Token;
            fileMessageResponseDto.FileName = resp.FileName;
            fileMessageResponseDto.Size = resp.Size;
            if (resp.Success)
            {
                //记录到数据库
                await _chatRecordRepository.AddAsync(_mapper.Map<ChatRecord>(fileMessageResponseDto));
                fileMessageResponseDto.RemoteSendTime = DateTime.Now;
                await clients.User(fileMessageDto.From.ToString()).SendAsync(HubConst.ReceiveFileMessage, new HubMessageResponseDto<FileMessageResponseDto>()
                {
                    Success = true,
                    Data = fileMessageResponseDto
                });

                await clients.User(fileMessageDto.To.ToString()).SendAsync(HubConst.ReceiveFileMessage, new HubMessageResponseDto<FileMessageResponseDto>()
                {
                    Success = true,
                    Data = fileMessageResponseDto
                });
            }
            else 
            {
                await clients.User(fileMessageDto.From.ToString()).SendAsync(HubConst.ReceiveFileMessage, new HubMessageResponseDto<FileMessageResponseDto>()
                {
                    Success = false,
                    Message = resp.Message,
                    Data = fileMessageResponseDto
                });
            }
        }

        public async Task SendTextMessageAsync(long sender, TextMessageDto textMessageDto, IHubCallerClients clients)
        {
            textMessageDto.From = sender;

            if (!await IsFriendAsync(textMessageDto.From, textMessageDto.To))
            {
                await clients.User(textMessageDto.From.ToString()).SendAsync(HubConst.ReceiveTextMessage, new HubMessageResponseDto<TextMessageDto>()
                {
                    Success = false,
                    Message = "好友关系异常",
                    Data = textMessageDto
                });

                return;
            }
            textMessageDto.RemoteSendTime = DateTime.Now;
            await _chatRecordRepository.AddAsync(_mapper.Map<ChatRecord>(textMessageDto));

            //发送给两个人
            await clients.User(textMessageDto.From.ToString()).SendAsync(HubConst.ReceiveTextMessage, new HubMessageResponseDto<TextMessageDto>()
            {
                Success = true,
                Data = textMessageDto
            });

            await clients.User(textMessageDto.To.ToString()).SendAsync(HubConst.ReceiveTextMessage, new HubMessageResponseDto<TextMessageDto>()
            {
                Success = true,
                Data = textMessageDto
            });
        }

        public async Task SendWithdrawMessageAsync(long sender, WithdrawMessageDto withdrawMessageDto, IHubCallerClients clients, HubCallerContext hubCallerContext)
        {
            var message = await _chatRecordRepository
                .GetAsync(Builders<ChatRecord>.Filter.And(Builders<ChatRecord>.Filter.Eq(x => x.MessageId, withdrawMessageDto.MessageId),
                Builders<ChatRecord>.Filter.Eq(x => x.From, sender)));

            var resp = new WithdrawMessageResponseDto()
            {
                MessageId = message.MessageId,
            };
            if (message == null)
            {
                await clients.User(sender.ToString()).SendAsync(HubConst.ReceiveWithdrawMessage, new HubMessageResponseDto<WithdrawMessageResponseDto>()
                {
                    Success = false,
                    Message = "撤回消息失败",
                    Data = resp
                });

                return;
            }

            resp.From = message.From;
            resp.To = message.To;
            await _chatRecordRepository.UpdateAsync(message.Id, Builders<ChatRecord>.Update.Set(x => x.Withdraw, true)
                .Set(x => x.WithdrawTime, DateTime.Now));

            {
                //发送给两个人
                await clients.User(resp.From.ToString()).SendAsync(HubConst.ReceiveWithdrawMessage, new HubMessageResponseDto<WithdrawMessageResponseDto>()
                {
                    Success = true,
                    Data = resp
                });

                await clients.User(resp.From.ToString()).SendAsync(HubConst.ReceiveWithdrawMessage, new HubMessageResponseDto<WithdrawMessageResponseDto>()
                {
                    Success = true,
                    Data = resp
                });
            }
        }

        private async Task<bool> IsFriendAsync(long user,long friend) 
        {
            var min = Math.Min(user, friend);
            var max = Math.Max(user, friend);

            return await _redisProvider.SetIsMemberAsync(Const.REDIS_FRIEND_RELATIONS, $"{min}-{max}");
        }
    }
}
