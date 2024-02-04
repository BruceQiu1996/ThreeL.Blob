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

        public async Task<ServiceResult<QueryChatRecordResponseDto>> QueryChatRecordsAsync(long userId, long target, DateTime time)
        {
            if (!await IsFriendAsync(userId, target))
            {
                return new ServiceResult<QueryChatRecordResponseDto>(System.Net.HttpStatusCode.BadRequest, "好友关系异常");
            };

            var chatRecords = await _chatRecordRepository.PagedAsync(0, 30,
                   Builders<ChatRecord>.Filter.And(Builders<ChatRecord>.Filter.Where(x => x.RemoteSendTime < time),
                   Builders<ChatRecord>.Filter.Where(x => (x.From == userId && x.To == target) || (x.To == userId && x.From == target))),
                   x => x.RemoteSendTime);

            var respData = new QueryChatRecordResponseDto()
            {
                Count = chatRecords.Count,
                Records = chatRecords.Data.Select(x => _mapper.Map<ChatRecordResponseDto>(x))
            };

            return new ServiceResult<QueryChatRecordResponseDto>() { Value = respData };
        }

        public async Task SendFileMessageAsync(long sender, FileMessageDto fileMessageDto, IHubCallerClients clients, HubCallerContext hubCallerContext)
        {
            fileMessageDto.From = sender;
            var fileMessageResponseDto = _mapper.Map<FileMessageResponseDto>(fileMessageDto);
            var token = hubCallerContext.GetHttpContext().Request.Headers["Authorization"];
            if (!await IsFriendAsync(fileMessageDto.From, fileMessageDto.To))
            {
                await clients.User(fileMessageDto.From.ToString()).SendAsync(HubConst.ReceiveFileMessage, new HubMessageResponseDto<FileMessageResponseDto>()
                {
                    Success = false,
                    Message = "好友关系异常",
                    Data = fileMessageResponseDto
                });

                return;
            };

            var resp = await _rpcContextAPIServiceClient.SendFileAsync(new SendFileRequest()
            {
                FileId = fileMessageDto.FileObjectId,
                Target = fileMessageDto.To,
            }, new Metadata() { { "Authorization", $"{token}" } });

            fileMessageResponseDto.Token = resp.Token;
            fileMessageResponseDto.FileName = resp.FileName;
            fileMessageResponseDto.Size = resp.Size;
            if (resp.Success)
            {
                fileMessageResponseDto.RemoteSendTime = DateTime.Now;
                //记录到数据库
                await _chatRecordRepository.AddAsync(_mapper.Map<ChatRecord>(fileMessageResponseDto));
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

        public async Task SendFolderMessageAsync(long sender, FolderMessageDto folderMessageDto, IHubCallerClients clients, HubCallerContext hubCallerContext)
        {
            folderMessageDto.From = sender;
            var folderMessageResponseDto = _mapper.Map<FolderMessageResponseDto>(folderMessageDto);
            var token = hubCallerContext.GetHttpContext().Request.Headers["Authorization"];
            if (!await IsFriendAsync(folderMessageDto.From, folderMessageDto.To))
            {
                await clients.User(folderMessageDto.From.ToString()).SendAsync(HubConst.ReceiveFolderMessage, new HubMessageResponseDto<FolderMessageResponseDto>()
                {
                    Success = false,
                    Message = "好友关系异常",
                    Data = folderMessageResponseDto
                });

                return;
            };

            var resp = await _rpcContextAPIServiceClient.SendFolderAsync(new SendFolderRequest()
            {
                FileId = folderMessageDto.FileObjectId,
                Target = folderMessageDto.To,
            }, new Metadata() { { "Authorization", $"{token}" } });

            folderMessageResponseDto.Token = resp.Token;
            folderMessageResponseDto.FileName = resp.FileName;
            if (resp.Success)
            {
                folderMessageResponseDto.RemoteSendTime = DateTime.Now;
                //记录到数据库
                await _chatRecordRepository.AddAsync(_mapper.Map<ChatRecord>(folderMessageResponseDto));
                await clients.User(folderMessageDto.From.ToString()).SendAsync(HubConst.ReceiveFolderMessage, new HubMessageResponseDto<FolderMessageResponseDto>()
                {
                    Success = true,
                    Data = folderMessageResponseDto
                });

                await clients.User(folderMessageDto.To.ToString()).SendAsync(HubConst.ReceiveFolderMessage, new HubMessageResponseDto<FolderMessageResponseDto>()
                {
                    Success = true,
                    Data = folderMessageResponseDto
                });
            }
            else
            {
                await clients.User(folderMessageDto.From.ToString()).SendAsync(HubConst.ReceiveFolderMessage, new HubMessageResponseDto<FolderMessageResponseDto>()
                {
                    Success = false,
                    Message = resp.Message,
                    Data = folderMessageResponseDto
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

            if (message.RemoteSendTime.AddMinutes(2) < DateTime.Now)
            {
                await clients.User(sender.ToString()).SendAsync(HubConst.ReceiveWithdrawMessage, new HubMessageResponseDto<WithdrawMessageResponseDto>()
                {
                    Success = false,
                    Message = "消息超过两分钟，无法撤回",
                    Data = resp
                });

                return;
            }

            resp.From = message.From;
            resp.To = message.To;
            await _chatRecordRepository.UpdateAsync(message.Id, Builders<ChatRecord>.Update.Set(x => x.Withdraw, true)
                .Set(x => x.WithdrawTime, DateTime.Now));

            if (message.MessageType == Shared.Domain.Metadata.Message.MessageType.File) //撤回文件分享记录
            {
                await _rpcContextAPIServiceClient.CancelSendFileAsync(new CancelSendFileRequest() { Token = message.FileToken});
            }

            {
                //发送给两个人
                await clients.User(resp.From.ToString()).SendAsync(HubConst.ReceiveWithdrawMessage, new HubMessageResponseDto<WithdrawMessageResponseDto>()
                {
                    Success = true,
                    Data = resp
                });

                await clients.User(resp.To.ToString()).SendAsync(HubConst.ReceiveWithdrawMessage, new HubMessageResponseDto<WithdrawMessageResponseDto>()
                {
                    Success = true,
                    Data = resp
                });
            }
        }

        private async Task<bool> IsFriendAsync(long user, long friend)
        {
            var min = Math.Min(user, friend);
            var max = Math.Max(user, friend);

            return await _redisProvider.SetIsMemberAsync(Const.REDIS_FRIEND_RELATIONS, $"{min}-{max}");
        }
    }
}
