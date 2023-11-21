using ThreeL.Blob.Application.Contract.Protos;

namespace ThreeL.Blob.Application.Contract.Services
{
    public interface IChatGrpcService
    {
        Task<AddFriendApplyResponse> AddFriendApplyAsync(string token, AddFriendApplyRequest addFriendApplyRequest);
    }
}
