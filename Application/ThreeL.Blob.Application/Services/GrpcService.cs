using Google.Protobuf;
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
        private readonly IEfBasicRepository<DownloadFileTask, string> _downloadFileTaskEfBasicRepository;
        private readonly ILogger<GrpcService> _logger;
        public GrpcService(IRedisProvider redisProvider,
                           IEfBasicRepository<FileObject, long> efBasicRepository,
                           IEfBasicRepository<DownloadFileTask, string> downloadFileTaskEfBasicRepository,
                           ILogger<GrpcService> logger)
        {
            _logger = logger;
            _redisProvider = redisProvider;
            _efBasicRepository = efBasicRepository;
            _downloadFileTaskEfBasicRepository = downloadFileTaskEfBasicRepository;
        }

        public async Task DownloadFileAsync(DownloadFileRequest request, IServerStreamWriter<DownloadFileResponse> responseStream, ServerCallContext context)
        {
            try
            {
                var userIdentity = context.GetHttpContext().User.Identity?.Name;
                var userid = long.Parse(userIdentity!);
                //寻找临时文件位置
                var task = await _downloadFileTaskEfBasicRepository.GetAsync(request.TaskId);
                var cacheTask = await _redisProvider.HGetAsync($"{Const.REDIS_DOWNLOADTASK_CACHE_KEY}{userid}", request.TaskId);

                if (task == null || cacheTask == null || request.Start != cacheTask)
                {
                    _logger.LogError($"下载异常,任务id:{request.TaskId}");
                    await responseStream.WriteAsync(new DownloadFileResponse()
                    {
                        Type = DownloadFileResponseStatus.DownloadErrorStatus,
                        Message = "下载数据异常",
                        TaskId = request.TaskId
                    });

                    return;
                }

                if (task.Status!=DownloadTaskStatus.Wait && task.Status !=DownloadTaskStatus.DownloadingSuspend)
                {
                    _logger.LogError($"下载异常,任务id:{request.TaskId}");
                    await responseStream.WriteAsync(new DownloadFileResponse()
                    {
                        Type = DownloadFileResponseStatus.DownloadErrorStatus,
                        Message = "下载数据异常",
                        TaskId = request.TaskId
                    });

                    return;
                }

                var fileObject = await _efBasicRepository.GetAsync(task.FileId);
                if (fileObject == null || !File.Exists(fileObject.Location) || fileObject.Status != FileStatus.Normal)
                {
                    _logger.LogError($"文件异常,任务id:{request.TaskId}");
                    await responseStream.WriteAsync(new DownloadFileResponse()
                    {
                        Type = DownloadFileResponseStatus.DownloadErrorStatus,
                        Message = "文件异常",
                        TaskId = request.TaskId
                    });

                    return;
                }

                task.Status = DownloadTaskStatus.Downloading;
                await _downloadFileTaskEfBasicRepository.UpdateAsync(task);

                using (var fileStream = File.OpenRead(fileObject.Location))
                {
                    fileStream.Seek(request.Start, SeekOrigin.Begin);
                    var sended = request.Start;
                    var totalLength = fileStream.Length - sended;

                    Memory<byte> buffer = new byte[1024 * 100];
                    while (sended < totalLength)
                    {
                        await Task.Delay(100);
                        var length = await fileStream.ReadAsync(buffer);
                        sended += length;
                        var response = new DownloadFileResponse()
                        {
                            Content = ByteString.CopyFrom(buffer.Slice(0, length).ToArray()),
                            TaskId = request.TaskId,
                            Type = DownloadFileResponseStatus.DownloadData
                        };

                        await responseStream.WriteAsync(response);
                    }
                }

                //更新状态
                task.Status = DownloadTaskStatus.Finished;
                task.FinishTime = DateTime.UtcNow;
                await _downloadFileTaskEfBasicRepository.UpdateAsync(task);
                //下载完成
                await responseStream.WriteAsync(new DownloadFileResponse() 
                {
                    TaskId = request.TaskId,
                    Type = DownloadFileResponseStatus.DownloadFinishStatus
                });
            }
            catch (InvalidOperationException ex) //由于客户端断连等原因
            {
                _logger.LogError(ex.ToString());
                var task = await _downloadFileTaskEfBasicRepository.GetAsync(request.TaskId);
                if (task.Status == DownloadTaskStatus.Cancelled)
                {
                    task.FinishTime = DateTime.UtcNow;
                    task.Status = DownloadTaskStatus.Failed;
                }
                else 
                {
                    task.Status = DownloadTaskStatus.DownloadingSuspend;
                }
                
                await _downloadFileTaskEfBasicRepository.UpdateAsync(task);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                var task = await _downloadFileTaskEfBasicRepository.GetAsync(request.TaskId);
                task.FinishTime = DateTime.UtcNow;
                task.Status = DownloadTaskStatus.Failed;
                await _downloadFileTaskEfBasicRepository.UpdateAsync(task);
            }
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

                return new UploadFileResponse() { Result = false, Message = "通讯流中断", Status = UploadFileResponseStatus.PauseStatus };
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
