using Grpc.Core;
using ThreeL.Blob.Application.Contract.Protos;

namespace ThreeL.Blob.Application.Contract.Services
{
    public interface IGrpcService
    {
        Task<UploadFileResponse> UploadFileAsync(UploadFileRequest uploadFileRequest, ServerCallContext context);
    }
}
