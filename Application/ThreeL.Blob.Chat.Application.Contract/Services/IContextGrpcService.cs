using ThreeL.Blob.Chat.Application.Contract.Protos;

namespace ThreeL.Blob.Chat.Application.Contract.Services
{
    public interface IGrpcService
    {
        Task<AddFriendApplyResponse> SendAddFriendApply(AddFriendApplyRequest addFriendApplyRequest);
    }
}
