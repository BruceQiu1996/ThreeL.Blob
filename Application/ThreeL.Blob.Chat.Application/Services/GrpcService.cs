using ThreeL.Blob.Chat.Application.Channels;
using ThreeL.Blob.Chat.Application.Contract.Protos;
using ThreeL.Blob.Chat.Application.Contract.Services;
using ThreeL.Blob.Shared.Application.Contract.Services;

namespace ThreeL.Blob.Chat.Application.Services
{
    public class GrpcService : IGrpcService, IAppService
    {
        private readonly PushMessageToClientChannel _channel;
        public GrpcService(PushMessageToClientChannel channel)
        {
            _channel = channel;
        }

        public async Task<AddFriendApplyResponse> SendAddFriendApply(AddFriendApplyRequest addFriendApplyRequest)
        {
            await _channel.WriteMessageAsync((addFriendApplyRequest.ApplyToId, "NewAddFriendApply", new
            {
                addFriendApplyRequest.ApplyId
            }));

            return new AddFriendApplyResponse();
        }
    }
}
