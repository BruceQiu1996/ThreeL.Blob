using Grpc.Core;
using ThreeL.Blob.Application.Contract.Protos;

namespace ThreeL.Blob.Application.Contract.Services
{
    public interface IGrpcService
    {
        Task<UploadFileResponse> UploadFileAsync(IAsyncStreamReader<UploadFileRequest> uploadFileRequest, ServerCallContext context);
        Task DownloadFileAsync(DownloadFileRequest request, IServerStreamWriter<DownloadFileResponse> responseStream, ServerCallContext context);
    }
}
