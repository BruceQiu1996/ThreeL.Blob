using System.Threading.Tasks;
using ThreeL.Blob.Application.Contract.Dtos;
using ThreeL.Blob.Shared.Application.Contract.Services;

namespace ThreeL.Blob.Application.Contract.Services
{
    public interface IRelationService
    {
        Task<ServiceResult<IEnumerable<RelationBriefDto>>> GetRelationsAsync(long userId);
        Task<ServiceResult> AddFriendApplyAsync(long userId, string userName, long target, string token);
        Task<ServiceResult<IEnumerable<RelationBriefDto>>> QueryRelationsByKeywordAsync(long userId, string key);
        Task<ServiceResult<IEnumerable<ApplyDto>>> QueryApplysAsync(long userId);
    }
}
