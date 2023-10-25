using Google.Protobuf;
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
        /// <param name="pauseToken">取消令牌</param>
        /// <param name="cancelToken">取消令牌</param>
        /// <param name="file">上传文件</param>
        /// <param name="fileId">上传文件唯一标识</param>
        /// <param name="statIndex">文件起点，包括</param>
        /// <param name="singlePacket">分片大小</param>
        /// <param name="sendedBytesCallBack">数据上传回调</param>
        /// <returns></returns>
        public async Task<UploadFileResponse> UploadFileAsync(CancellationToken pauseToken, CancellationToken cancelToken, string file, long fileId, long statIndex = 0, int singlePacket = 1024 * 1024,
                                                              Action<string> exceptionCallback = null, Action<long> sendedBytesCallBack = null)
        {
            var call = _fileGrpcServiceClient.UploadFile(new Metadata()
            {
                 { "Authorization", $"Bearer {_token}" }
            });

            using (var fileStream = File.OpenRead(file))
            {
                try
                {
                    var sended = statIndex;
                    fileStream.Seek(statIndex, SeekOrigin.Begin);
                    var totalLength = fileStream.Length;
                    Memory<byte> buffer = new byte[1024 * 100];
                    if(sended == totalLength) //如果已发送等于文件总长度，则直接发送完成
                    {
                        await call.RequestStream.WriteAsync(new UploadFileRequest()
                        {
                            Type = UploadFileRequestType.NoDataAndComplete,
                            FileId = fileId
                        });
                    }
                    while (sended < totalLength)
                    {
                        if (pauseToken.IsCancellationRequested)
                        {
                            await call.RequestStream.WriteAsync(new UploadFileRequest()
                            {
                                Type = UploadFileRequestType.Pause
                            });

                            break;
                        }

                        if (cancelToken.IsCancellationRequested)
                        {
                            await call.RequestStream.WriteAsync(new UploadFileRequest()
                            {
                                Type = UploadFileRequestType.Cancel
                            });

                            break;
                        }

                        var length = await fileStream.ReadAsync(buffer);
                        await Task.Delay(200);
                        sended += length;

                        var request = new UploadFileRequest()
                        {
                            Content = ByteString.CopyFrom(buffer.Slice(0, length).ToArray()),
                            FileId = fileId,
                            Type = UploadFileRequestType.Data,
                        };

                        await call.RequestStream.WriteAsync(request);
                        sendedBytesCallBack(sended);
                    }

                    await call.RequestStream.CompleteAsync();
                }
                catch (Exception ex)
                {
                    exceptionCallback?.Invoke(ex.Message);
                }
            }

            return await call.ResponseAsync;
        }

        public void SetToken(string token)
        {
            _token = token;
        }
    }
}
