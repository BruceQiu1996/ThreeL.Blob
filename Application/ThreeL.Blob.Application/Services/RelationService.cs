using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ThreeL.Blob.Application.Contract.Dtos;
using ThreeL.Blob.Application.Contract.Services;
using ThreeL.Blob.Domain.Aggregate.User;
using ThreeL.Blob.Infra.Repository.IRepositories;
using ThreeL.Blob.Shared.Application.Contract.Services;

namespace ThreeL.Blob.Application.Services
{
    public class RelationService : IRelationService,IAppService
    {
        private readonly IMapper _mapper;
        private readonly IEfBasicRepository<FriendRelation, long> _friendRelationEfBasicRepository;
        private readonly IEfBasicRepository<User, long> _userEfBasicRepository;
        public RelationService(IEfBasicRepository<FriendRelation, long> friendRelationEfBasicRepository,
                               IEfBasicRepository<User, long> userEfBasicRepository,
                               IMapper mapper)
        {
            _mapper = mapper;
            _friendRelationEfBasicRepository =  friendRelationEfBasicRepository;
            _userEfBasicRepository = userEfBasicRepository;
        }
        public async Task<IEnumerable<RelationBriefDto>> GetRelarionsDto(long userId)
        {
            //获取所有的好友
            var relations = await _friendRelationEfBasicRepository.Where(x => x.Passiver == userId || x.Activer == userId).ToListAsync();
            if (relations.Count <= 0)
            {
                return new List<RelationBriefDto>();
            }
            List<long> friends = new List<long>();
            //获取好友
            relations.ForEach(x =>
            {
                friends.Add(x.Passiver == userId ? x.Activer : x.Passiver);
            });

            var friendList = await _userEfBasicRepository.Where(x => friends.Contains(x.Id)).ToListAsync();
            var friendBriefs = friendList.Select(_mapper.Map<RelationBriefDto>);

            //TODO以后做群组需要加
            return friendBriefs;
        }
    }
}
