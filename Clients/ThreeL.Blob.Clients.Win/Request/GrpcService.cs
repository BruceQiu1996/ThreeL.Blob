﻿using Google.Protobuf;
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using ThreeL.Blob.Clients.Grpc.Protos;
using ThreeL.Blob.Clients.Win.Configurations;

namespace ThreeL.Blob.Clients.Win.Request
{
    public class GrpcService
    {
        private readonly RemoteOptions _remoteOptions;
        private string _token;
        private FileGrpcService.FileGrpcServiceClient _fileGrpcServiceClient;
        public GrpcService(IOptions<RemoteOptions> options)
        {
            _remoteOptions = options.Value;
            Initialize();
        }

        public Task Initialize()
        {
            var channel = GrpcChannel.ForAddress($"http://{_remoteOptions.Host}:{_remoteOptions.GrpcPort}", new GrpcChannelOptions()
            {
                HttpHandler = new HttpClientHandler()
                {
                    AutomaticDecompression = System.Net.DecompressionMethods.GZip,
                    AllowAutoRedirect = true,
                    ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
                },
                MaxRetryAttempts = _remoteOptions.MaxRetryAttempts,
                Credentials = ChannelCredentials.Insecure, //不使用安全连接 //TODO grpc ssl
            });

            _fileGrpcServiceClient = new FileGrpcService.FileGrpcServiceClient(channel);

            return Task.CompletedTask;
        }

        /// <summary>
        /// 上传文件
        /// </summary>
        /// <param name="cancellationToken">取消令牌</param>
        /// <param name="manualReset">本地暂停令牌</param>
        /// <param name="file">上传文件</param>
        /// <param name="statIndex">文件起点，包括</param>
        /// <param name="singlePacket">分片大小</param>
        /// <param name="sendedBytesCallBack">数据上传回调</param>
        /// <returns></returns>
        public async Task<UploadFileResponse> UploadFileAsync(CancellationToken cancellationToken, ManualResetEvent manualReset,
                                                              string file, long statIndex = 0, int singlePacket = 1024 * 1024,
                                                              Action<long> sendedBytesCallBack = null)
        {
            var call = _fileGrpcServiceClient.UploadFile(new Metadata()
            {
                 { "Authorization", $"Bearer {_token}" }
            });

            using (var fileStream = File.OpenRead(file))
            {
                var sended = 0L;
                var totalLength = fileStream.Length;
                var buffer = new byte[1024 * 1024];
                while (sended < totalLength)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        return new UploadFileResponse()
                        {
                            Result = true,
                        };
                    }
                    manualReset.WaitOne();
                    var length = await fileStream.ReadAsync(buffer);
                    sended += length;

                    var request = new UploadFileRequest()
                    {
                        Content = ByteString.CopyFrom(buffer),
                        FileId = 1
                    };

                    await call.RequestStream.WriteAsync(request);
                    sendedBytesCallBack(sended);
                }
            }

            await call.RequestStream.CompleteAsync();

            return await call.ResponseAsync;
        }

        public void SetToken(string token)
        {
            _token = token;
        }
    }
}