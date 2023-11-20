using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using ThreeL.Blob.Chat.Application.Contract.Protos;
using ThreeL.Blob.Chat.Application.Contract.Services;
using ThreeL.Blob.Shared.Application.Contract.Services;

namespace ThreeL.Blob.Chat.Server.Controllers
{
    public class GrpcController : ChatService.ChatServiceBase, IAppService
    {
        private readonly IGrpcService _grpcService;
        public GrpcController(IGrpcService grpcService)
        {
            _grpcService = grpcService;
        }

        [Authorize]
        public async override Task<AddFriendApplyResponse> AddFriendApply(AddFriendApplyRequest request, ServerCallContext context)
        {
            return await _grpcService.SendAddFriendApply(request);
        }
    }
}
