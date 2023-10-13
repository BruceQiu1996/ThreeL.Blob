using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using ThreeL.Blob.Application.Contract.Protos;
using ThreeL.Blob.Application.Contract.Services;
using ThreeL.Blob.Shared.Application.Contract.Services;

namespace ThreeL.Blob.Server.Controllers
{
    public class GrpcController : FileGrpcService.FileGrpcServiceBase, IAppService
    {
        private readonly IGrpcService _grpcService;
        public GrpcController(IGrpcService grpcService)
        {
            _grpcService = grpcService;
        }

        [Authorize]
        public async override Task<UploadFileResponse> UploadFile(IAsyncStreamReader<UploadFileRequest> request, ServerCallContext context)
        {
            try
            {
                return await _grpcService.UploadFileAsync(request, context);
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}
