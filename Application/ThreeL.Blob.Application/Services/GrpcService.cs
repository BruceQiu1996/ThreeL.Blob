using Grpc.Core;
using Microsoft.Extensions.Logging;
using ThreeL.Blob.Application.Contract.Configurations;
using ThreeL.Blob.Application.Contract.Protos;
using ThreeL.Blob.Application.Contract.Services;
using ThreeL.Blob.Domain.Aggregate.FileObject;
using ThreeL.Blob.Infra.Redis;
using ThreeL.Blob.Infra.Repository.IRepositories;
using ThreeL.Blob.Shared.Application.Contract.Services;
using ThreeL.Blob.Shared.Domain.Metadata.FileObject;

namespace ThreeL.Blob.Application.Services
{
    public class GrpcService : IGrpcService, IAppService
    {
        private readonly IRedisProvider _redisProvider;
        private readonly IEfBasicRepository<FileObject, long> _efBasicRepository;
        private readonly ILogger<GrpcService> _logger;
        public GrpcService(IRedisProvider redisProvider, IEfBasicRepository<FileObject, long> efBasicRepository, ILogger<GrpcService> logger)
        {
            _logger = logger;
            _redisProvider = redisProvider;
            _efBasicRepository = efBasicRepository;
        }

        public async Task DownloadFileAsync(DownloadFileRequest request, IServerStreamWriter<DownloadFileResponse> responseStream, ServerCallContext context)
        {
            
        }

        public async Task<UploadFileResponse> UploadFileAsync(IAsyncStreamReader<UploadFileRequest> uploadFileRequest, ServerCallContext context)
        {
            try
            {
                var userIdentity = context.GetHttpContext().User.Identity?.Name;
                var userid = long.Parse(userIdentity!);
                await uploadFileRequest.MoveNext();
                //寻找临时文件位置
                var fileObject = await _efBasicRepository.GetAsync(uploadFileRequest.Current.FileId);
                var file = await _redisProvider
                    .HGetAsync($"{Const.REDIS_UPLOADFILE_CACHE_KEY}{userid}", uploadFileRequest.Current.FileId.ToString());

                if (file == null || fileObject == null || !File.Exists(fileObject.TempFileLocation) || file != new FileInfo(fileObject.TempFileLocation).Length)
                {
                    _logger.LogError($"文件数据异常{uploadFileRequest.Current.FileId}");
                    return new UploadFileResponse() { Result = false, Message = "文件数据异常或已过期", Status = UploadFileResponseStatus.ErrorStatus };
                }

                if (fileObject.Status != FileStatus.Wait && fileObject.Status != FileStatus.UploadingSuspend)
                {
                    _logger.LogError($"文件状态异常{uploadFileRequest.Current.FileId}");
                    return new UploadFileResponse() { Result = false, Message = "文件状态异常", Status = UploadFileResponseStatus.ErrorStatus };
                }

                fileObject.Status = FileStatus.Uploading;
                await _efBasicRepository.UpdateAsync(fileObject);
                using (var fileStream = File.Open(fileObject.TempFileLocation, FileMode.Append, FileAccess.Write))
                {
                    var received = file!.Value;
                    do
                    {
                        var request = uploadFileRequest.Current;
                        if (request.Type == UploadFileRequestType.Pause) //暂停
                        {
                            //更新数据库文件状态
                            fileObject.Status = FileStatus.UploadingSuspend;
                            await _efBasicRepository.UpdateAsync(fileObject);

                            return new UploadFileResponse() { Result = false, Message = "暂停文件上传", Status = UploadFileResponseStatus.PauseStatus };
                        }

                        if (request.Type == UploadFileRequestType.Cancel) //取消
                        {
                            //更新数据库文件状态
                            fileObject.Status = FileStatus.Cancel;
                            await _efBasicRepository.UpdateAsync(fileObject);

                            return new UploadFileResponse() { Result = false, Message = "取消文件上传", Status = UploadFileResponseStatus.CancelStatus };
                        }

                        else if (request.Type == UploadFileRequestType.NoDataAndComplete) //没有数据了直接完成
                        {
                            break;
                        }
                        var buffer = request.Content.ToByteArray();
                        fileStream.Seek(received, SeekOrigin.Begin);
                        await fileStream.WriteAsync(buffer);
                        received += buffer.Length;
                        await _redisProvider.HSetAsync($"{Const.REDIS_UPLOADFILE_CACHE_KEY}{userid}", uploadFileRequest.Current.FileId.ToString(), received);
                    } while (await uploadFileRequest.MoveNext());
                }

                fileObject.Location = Path.ChangeExtension(fileObject.TempFileLocation, Path.GetExtension(fileObject.Name));
                File.Move(fileObject.TempFileLocation, fileObject.Location);
                fileObject.Status = FileStatus.Normal;
                await _efBasicRepository.UpdateAsync(fileObject);

                return new UploadFileResponse() { Result = true, Message = "上传文件完成", Status = UploadFileResponseStatus.NormalStatus };
            }
            catch (IOException ex)
            {
                _logger.LogError(ex.ToString());
                var fileObject = await _efBasicRepository.GetAsync(uploadFileRequest.Current.FileId);
                fileObject.Status = FileStatus.UploadingSuspend;
                await _efBasicRepository.UpdateAsync(fileObject);

                return new UploadFileResponse() { Result = false, Message = "通讯中断", Status = UploadFileResponseStatus.ErrorStatus };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                var fileObject = await _efBasicRepository.GetAsync(uploadFileRequest.Current.FileId);
                fileObject.Status = FileStatus.Faild;
                await _efBasicRepository.UpdateAsync(fileObject);

                return new UploadFileResponse() { Result = false, Message = "上传文件服务器出现异常", Status = UploadFileResponseStatus.ErrorStatus };
            }
        }
    }
}
