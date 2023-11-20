using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ThreeL.Blob.Application.Contract.Configurations;
using ThreeL.Blob.Application.Contract.Protos;
using ThreeL.Blob.Application.Contract.Services;
using ThreeL.Blob.Shared.Application.Contract.Services;

namespace ThreeL.Blob.Application.Services
{
    //与聊天服务器之间的通信
    public class ChatGrpcService : ChatService.ChatServiceClient, IChatGrpcService, IPreheatService, IAppService
    {
        private readonly ILogger<ChatGrpcService> _logger;
        private readonly ChatServerGrpcOptions _chatServerGrpcOptions;
        private ChatService.ChatServiceClient _chatServiceClient;
        public ChatGrpcService(ILogger<ChatGrpcService> logger, IOptions<ChatServerGrpcOptions> chatServerGrpcOptions)
        {
            _logger = logger;
            _chatServerGrpcOptions = chatServerGrpcOptions.Value;
        }

        public async Task<AddFriendApplyResponse> AddFriendApplyAsync(AddFriendApplyRequest addFriendApplyRequest)
        {
            return await _chatServiceClient.AddFriendApplyAsync(addFriendApplyRequest);
        }

        public Task PreheatAsync()
        {
            var channel = GrpcChannel.ForAddress($"http://{_chatServerGrpcOptions.Host}:{_chatServerGrpcOptions.Port}", new GrpcChannelOptions()
            {
                HttpHandler = new HttpClientHandler()
                {
                    AutomaticDecompression = System.Net.DecompressionMethods.GZip,
                    AllowAutoRedirect = true,
                    ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
                },
                MaxRetryAttempts = _chatServerGrpcOptions.RetryTimes,
                Credentials = ChannelCredentials.Insecure, //不使用安全连接 //TODO grpc ssl
            });

            _chatServiceClient = new ChatService.ChatServiceClient(channel);
            return Task.CompletedTask;
        }
    }
}
