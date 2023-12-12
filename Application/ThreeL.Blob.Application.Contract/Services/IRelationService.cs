using Grpc.Core;
using ThreeL.Blob.Application.Contract.Dtos;
using ThreeL.Blob.Application.Contract.Protos;
using ThreeL.Blob.Shared.Application.Contract.Services;

namespace ThreeL.Blob.Application.Contract.Services
{
    public interface IRelationService
    {
        Task<ServiceResult<IEnumerable<RelationBriefDto>>> GetRelationsAsync(long userId);
        Task<ServiceResult<RelationBriefDto>> GetRelationAsync(long userId,long target);
        Task<ServiceResult<IEnumerable<RelationBriefDto>>> QueryRelationsByKeywordAsync(long userId, string key);
        Task<ServiceResult<IEnumerable<ApplyDto>>> QueryApplysAsync(long userId);
        //grpc
        Task<CommonResponse> AddFriendApplyAsync(AddFriendApplyRequest request, ServerCallContext serverCallContext);
        Task<HandleAddFriendApplyResponse> HandleAddFriendApplyAsync(HandleAddFriendApplyRequest request, ServerCallContext serverCallContext);
        Task<SendFileResponse> SendFileAsync(SendFileRequest request, ServerCallContext serverCallContext);
        Task<SendFolderResponse> SendFolderAsync(SendFolderRequest request, ServerCallContext serverCallContext);
        Task<CancelSendFileResponse> CancelSendFileAsync(CancelSendFileRequest request, ServerCallContext serverCallContext);
    }
}
