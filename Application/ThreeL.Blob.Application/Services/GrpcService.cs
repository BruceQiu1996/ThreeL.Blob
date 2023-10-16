using Grpc.Core;
using Microsoft.Extensions.Logging;
using ThreeL.Blob.Application.Contract.Configurations;
using ThreeL.Blob.Application.Contract.Protos;
using ThreeL.Blob.Application.Contract.Services;
using ThreeL.Blob.Domain.Aggregate.FileObject;
using ThreeL.Blob.Infra.Core.Extensions.System;
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

        public async Task<UploadFileResponse> UploadFileAsync(IAsyncStreamReader<UploadFileRequest> uploadFileRequest, ServerCallContext context)
        {
            try
            {
                var userIdentity = context.GetHttpContext().User.Identity?.Name;
                var userid = long.Parse(userIdentity!);
                await uploadFileRequest.MoveNext();
                //寻找临时文件位置
                var file = await _redisProvider
                    .HGetAsync<FileObject>($"{Const.REDIS_UPLOADFILE_CACHE_KEY}{userid}", uploadFileRequest.Current.FileId.ToString());

                if (file == null || !File.Exists(file.TempFileLocation))
                {
                    _logger.LogError($"缓存文件异常{uploadFileRequest.Current.FileId}");
                    return new UploadFileResponse() { Result = false, Message = "上传文件失败" };
                }

                var fileObj = await _efBasicRepository.GetAsync(file.Id);
                if (fileObj == null)
                {
                    _logger.LogError($"文件异常{uploadFileRequest.Current.FileId}");
                    return new UploadFileResponse() { Result = false, Message = "上传文件失败" };
                }

                if (fileObj.Status != FileStatus.Wait && fileObj.Status != FileStatus.UploadSuspend) //上传暂停或者没有上传
                {
                    _logger.LogError($"文件状态异常{uploadFileRequest.Current.FileId}");
                    return new UploadFileResponse() { Result = false, Message = "上传文件状态异常" };
                }

                using (var fileStream = File.Open(file.TempFileLocation, FileMode.Append, FileAccess.Write))
                {
                    var received = 0L;
                    do
                    {
                        var request = uploadFileRequest.Current;
                        var buffer = request.Content.ToByteArray();
                        fileStream.Seek(received, SeekOrigin.Begin);
                        await fileStream.WriteAsync(buffer);
                        received += buffer.Length;
                    } while (await uploadFileRequest.MoveNext());
                }

                fileObj.Location = Path.ChangeExtension(file.TempFileLocation, Path.GetExtension(file.Name));
                fileObj.Status = FileStatus.Normal;
                await _efBasicRepository.UpdateAsync(fileObj);
                return new UploadFileResponse() { Result = true, Message = "上传文件完成" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return new UploadFileResponse() { Result = false, Message = "上传文件异常结束" };
            }
        }
    }
}
