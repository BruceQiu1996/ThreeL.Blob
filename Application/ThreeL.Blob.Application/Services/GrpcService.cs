using Grpc.Core;
using ThreeL.Blob.Application.Contract.Configurations;
using ThreeL.Blob.Application.Contract.Protos;
using ThreeL.Blob.Application.Contract.Services;
using ThreeL.Blob.Domain.Aggregate.FileObject;
using ThreeL.Blob.Infra.Redis;
using ThreeL.Blob.Infra.Repository.IRepositories;
using ThreeL.Blob.Shared.Application.Contract.Services;

namespace ThreeL.Blob.Application.Services
{
    public class GrpcService : IGrpcService, IAppService
    {
        private readonly IRedisProvider _redisProvider;
        private readonly IEfBasicRepository<FileObject, long> _efBasicRepository;
        public GrpcService(IRedisProvider redisProvider, IEfBasicRepository<FileObject, long> efBasicRepository)
        {
            _redisProvider = redisProvider;
            _efBasicRepository = efBasicRepository;
        }

        public async Task<UploadFileResponse> UploadFileAsync(IAsyncStreamReader<UploadFileRequest> uploadFileRequest, ServerCallContext context)
        {
            var userIdentity = context.GetHttpContext().User.Identity?.Name;
            var userid = long.Parse(userIdentity!);
            await uploadFileRequest.MoveNext();
            //寻找临时文件位置
            var file = await _redisProvider
                .HGetAsync<FileObject>($"{Const.REDIS_UPLOADFILE_CACHE_KEY}{userid}", uploadFileRequest.Current.FileId.ToString());

            if (file == null || !File.Exists(file.TempFileLocation))
            {
                return new UploadFileResponse() { Result = false, Message = "上传文件失败" };
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

            return new UploadFileResponse() { Result = false, Message = "上传文件完成" };
        }
    }
}
