using ThreeL.Blob.Application.Contract.Dtos;

namespace ThreeL.Blob.Application.Contract.Services
{
    public interface IRelationService
    {
        Task<IEnumerable<RelationBriefDto>> GetRelarionsDto(long userId);
    }
}
