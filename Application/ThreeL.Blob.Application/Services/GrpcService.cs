using Grpc.Core;
using ThreeL.Blob.Application.Contract.Protos;
using ThreeL.Blob.Application.Contract.Services;
using ThreeL.Blob.Shared.Application.Contract.Services;

namespace ThreeL.Blob.Application.Services
{
    public class GrpcService : IGrpcService, IAppService
    {
        public GrpcService()
        {
            
        }

        public async Task<UploadFileResponse> UploadFileAsync(UploadFileRequest uploadFileRequest, ServerCallContext context)
        {
            throw new NotImplementedException();
        }
    }
}
