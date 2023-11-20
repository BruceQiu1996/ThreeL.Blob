using ThreeL.Blob.Application.Contract.Dtos;
using ThreeL.Blob.Shared.Application.Contract.Services;

namespace ThreeL.Blob.Application.Contract.Services
{
    public interface IRelationService
    {
        Task<ServiceResult<IEnumerable<RelationBriefDto>>> GetRelationsAsync(long userId);
        Task<ServiceResult> AddFriendApplyAsync(long userId,long target);
    }
}
