using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using ThreeL.Blob.Application.Contract.Protos;
using ThreeL.Blob.Application.Contract.Services;

namespace ThreeL.Blob.Server.Controllers
{
    public class GrpcController : FileGrpcService.FileGrpcServiceBase
    {
        private readonly IGrpcService _grpcService;
        public GrpcController(IGrpcService grpcService)
        {
            _grpcService = grpcService;
        }

        [Authorize]
        public async override Task<UploadFileResponse> UploadFile(IAsyncStreamReader<UploadFileRequest> request, ServerCallContext context)
        {
            return await _grpcService.UploadFileAsync(request, context);
        }

        [Authorize]
        public async override Task DownloadFile(DownloadFileRequest request, IServerStreamWriter<DownloadFileResponse> responseStream, ServerCallContext context)
        {
            await _grpcService.DownloadFileAsync(request, responseStream, context);
        }
    }
}
