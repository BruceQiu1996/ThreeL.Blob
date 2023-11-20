﻿using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.Options;
using ThreeL.Blob.Chat.Application.Contract.Configurations;
using ThreeL.Blob.Chat.Application.Contract.Services;
using ThreeL.Blob.Shared.Application.Contract.Services;

namespace ThreeL.Blob.Chat.Application.Services
{
    public class ContextGrpcService : IContextGrpcService, IAppService, IPreheatService
    {
        private readonly ContextApiOptions _contextApiOptions;
        public ContextGrpcService(IOptions<ContextApiOptions> contextApiOptions)
        {
            _contextApiOptions = contextApiOptions.Value;
        }

        public Task PreheatAsync()
        {
            var channel = GrpcChannel.ForAddress($"http://{_contextApiOptions.Host}:{_contextApiOptions.Port}", new GrpcChannelOptions()
            {
                HttpHandler = new HttpClientHandler()
                {
                    AutomaticDecompression = System.Net.DecompressionMethods.GZip,
                    AllowAutoRedirect = true,
                    ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
                },
                MaxRetryAttempts = _contextApiOptions.RetryTimes,
                Credentials = ChannelCredentials.Insecure, //不使用安全连接 //TODO grpc ssl
            });

            return Task.CompletedTask;
        }
    }
}
