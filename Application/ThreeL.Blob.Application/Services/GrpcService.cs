﻿using Google.Protobuf;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using ThreeL.Blob.Application.Channels;
using ThreeL.Blob.Application.Contract.Protos;
using ThreeL.Blob.Application.Contract.Services;
using ThreeL.Blob.Domain.Aggregate.FileObject;
using ThreeL.Blob.Infra.Redis;
using ThreeL.Blob.Infra.Repository.IRepositories;
using ThreeL.Blob.Shared.Application.Contract.Configurations;
using ThreeL.Blob.Shared.Application.Contract.Services;
using ThreeL.Blob.Shared.Domain.Metadata.FileObject;
using ThreeL.Blob.Infra.Core.Extensions.System;

namespace ThreeL.Blob.Application.Services
{
    public class GrpcService : IGrpcService, IAppService
    {
        private readonly IRedisProvider _redisProvider;
        private readonly IEfBasicRepository<FileObject, long> _efBasicRepository;
        private readonly IEfBasicRepository<DownloadFileTask, string> _downloadFileTaskEfBasicRepository;
        private readonly ILogger<GrpcService> _logger;
        private readonly GenerateThumbnailChannel _generateThumbnailChannel;
        public GrpcService(IRedisProvider redisProvider,
                           IEfBasicRepository<FileObject, long> efBasicRepository,
                           GenerateThumbnailChannel generateThumbnailChannel,
                           IEfBasicRepository<DownloadFileTask, string> downloadFileTaskEfBasicRepository,
                           ILogger<GrpcService> logger)
        {
            _logger = logger;
            _redisProvider = redisProvider;
            _efBasicRepository = efBasicRepository;
            _generateThumbnailChannel = generateThumbnailChannel;
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
                var file = await _efBasicRepository.GetAsync(task.FileId);
                //网络问题导致的状态不一致

                if (task.Status == DownloadTaskStatus.Finished || request.Start == file?.Size)
                {
                    await responseStream.WriteAsync(new DownloadFileResponse()
                    {
                        TaskId = request.TaskId,
                        Type = DownloadFileResponseStatus.DownloadFinishStatus
                    });
                }

                if (task == null || cacheTask == null || request.Start > file?.Size)
                {
                    _logger.LogError($"下载异常,任务id:{request.TaskId}:{request.Start}:{cacheTask}");
                    await responseStream.WriteAsync(new DownloadFileResponse()
                    {
                        Type = DownloadFileResponseStatus.DownloadErrorStatus,
                        Message = "下载数据异常",
                        TaskId = request.TaskId
                    });

                    return;
                }

                //if (task.Status != DownloadTaskStatus.Wait && task.Status != DownloadTaskStatus.DownloadingSuspend && task.Status != DownloadTaskStatus.Finished)
                //{
                //    _logger.LogError($"下载状态异常,任务id:{request.TaskId}:{task.Status}");
                //    await responseStream.WriteAsync(new DownloadFileResponse()
                //    {
                //        Type = DownloadFileResponseStatus.DownloadErrorStatus,
                //        Message = "下载数据异常",
                //        TaskId = request.TaskId
                //    });

                //    return;
                //}

                var FileObject = await _efBasicRepository.GetAsync(task.FileId);
                if (FileObject == null || !File.Exists(FileObject.Location) || FileObject.Status != FileStatus.Normal)
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

                using (var fileStream = File.OpenRead(FileObject.Location))
                {
                    fileStream.Seek(request.Start, SeekOrigin.Begin);
                    var sended = request.Start;

                    Memory<byte> buffer = new byte[1024 * 100];
                    while (sended < fileStream.Length)
                    {
                        await Task.Delay(100);
                        var length = await fileStream.ReadAsync(buffer);
                        var response = new DownloadFileResponse()
                        {
                            Content = ByteString.CopyFrom(buffer.Slice(0, length).ToArray()),
                            TaskId = request.TaskId,
                            Type = DownloadFileResponseStatus.DownloadData
                        };

                        await responseStream.WriteAsync(response);
                        sended += length;
                        await _redisProvider.HSetAsync($"{Const.REDIS_DOWNLOADTASK_CACHE_KEY}{userid}", request.TaskId, sended);
                    }
                }

                //更新状态
                task.Status = DownloadTaskStatus.Finished;
                task.FinishTime = DateTime.Now;
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
                    task.FinishTime = DateTime.Now;
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
                task.FinishTime = DateTime.Now;
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
                var FileObject = await _efBasicRepository.GetAsync(uploadFileRequest.Current.FileId);
                var file = await _redisProvider
                    .HGetAsync($"{Const.REDIS_UPLOADFILE_CACHE_KEY}{userid}", uploadFileRequest.Current.FileId.ToString());

                if (file == null || FileObject == null || !File.Exists(FileObject.TempFileLocation) || file != new FileInfo(FileObject.TempFileLocation).Length)
                {
                    _logger.LogError($"文件数据异常{uploadFileRequest.Current.FileId}");
                    return new UploadFileResponse() { Result = false, Message = "文件数据异常或已过期", Status = UploadFileResponseStatus.ErrorStatus };
                }

                if (FileObject.Status != FileStatus.Wait && FileObject.Status != FileStatus.UploadingSuspend)
                {
                    _logger.LogError($"文件状态异常{uploadFileRequest.Current.FileId}");
                    return new UploadFileResponse() { Result = false, Message = "文件状态异常", Status = UploadFileResponseStatus.ErrorStatus };
                }

                FileObject.Status = FileStatus.Uploading;
                await _efBasicRepository.UpdateAsync(FileObject);
                using (var fileStream = File.Open(FileObject.TempFileLocation, FileMode.Append, FileAccess.Write))
                {
                    var received = file!.Value;
                    do
                    {
                        var request = uploadFileRequest.Current;
                        if (request.Type == UploadFileRequestType.Pause) //暂停
                        {
                            //更新数据库文件状态
                            FileObject.Status = FileStatus.UploadingSuspend;
                            await _efBasicRepository.UpdateAsync(FileObject);

                            return new UploadFileResponse() { Result = false, Message = "暂停文件上传", Status = UploadFileResponseStatus.PauseStatus };
                        }

                        if (request.Type == UploadFileRequestType.Cancel) //取消
                        {
                            //更新数据库文件状态
                            FileObject.Status = FileStatus.Cancel;
                            await _efBasicRepository.UpdateAsync(FileObject);

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

                FileInfo fileInfo = new FileInfo(FileObject.TempFileLocation);
                FileObject.Location = FileObject.Name.GetAvailableFileLocation(fileInfo.DirectoryName!);
                FileObject.Name = Path.GetFileName(FileObject.Location);
                File.Move(FileObject.TempFileLocation, FileObject.Location);
                FileObject.Status = FileStatus.Normal;
                await _efBasicRepository.UpdateAsync(FileObject);
                await _generateThumbnailChannel.WriteMessageAsync((userid, FileObject.Id)); //生成缩略图

                return new UploadFileResponse() { Result = true, Message = "上传文件完成", Status = UploadFileResponseStatus.NormalStatus };
            }
            catch (IOException ex)
            {
                _logger.LogError(ex.ToString());
                if (uploadFileRequest.Current != null)
                {
                    var FileObject = await _efBasicRepository.GetAsync(uploadFileRequest.Current.FileId);
                    FileObject.Status = FileStatus.UploadingSuspend;
                    await _efBasicRepository.UpdateAsync(FileObject);
                }

                return new UploadFileResponse() { Result = false, Message = "通讯流中断", Status = UploadFileResponseStatus.PauseStatus };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                if (uploadFileRequest.Current != null)
                {
                    var FileObject = await _efBasicRepository.GetAsync(uploadFileRequest.Current.FileId);
                    FileObject.Status = FileStatus.Faild;
                    await _efBasicRepository.UpdateAsync(FileObject);
                }

                return new UploadFileResponse() { Result = false, Message = "上传文件服务器出现异常", Status = UploadFileResponseStatus.ErrorStatus };
            }
        }
    }
}
