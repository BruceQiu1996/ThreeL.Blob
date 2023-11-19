using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ThreeL.Blob.Application.Contract.Dtos;
using ThreeL.Blob.Application.Contract.Services;
using ThreeL.Blob.Domain.Aggregate.User;
using ThreeL.Blob.Infra.Repository.IRepositories;
using ThreeL.Blob.Shared.Application.Contract.Services;

namespace ThreeL.Blob.Application.Services
{
    public class RelationService : IRelationService, IAppService
    {
        private readonly IMapper _mapper;
        private readonly IEfBasicRepository<FriendRelation, long> _friendRelationEfBasicRepository;
        private readonly IEfBasicRepository<User, long> _userEfBasicRepository;
        private readonly IConfiguration _configuration;
        public RelationService(IEfBasicRepository<FriendRelation, long> friendRelationEfBasicRepository,
                               IEfBasicRepository<User, long> userEfBasicRepository,
                               IMapper mapper,
                               IConfiguration configuration)
        {
            _mapper = mapper;
            _configuration = configuration;
            _friendRelationEfBasicRepository = friendRelationEfBasicRepository;
            _userEfBasicRepository = userEfBasicRepository;
        }

        public async Task<ServiceResult<IEnumerable<RelationBriefDto>>> GetRelationsAsync(long userId)
        {
            //获取所有的好友
            var relations = await _friendRelationEfBasicRepository.Where(x => x.Passiver == userId || x.Activer == userId).ToListAsync();
            if (relations.Count <= 0)
            {
                return new ServiceResult<IEnumerable<RelationBriefDto>>()
                {
                    Value = new List<RelationBriefDto>()
                };
            }
            List<long> friends = new List<long>();
            //获取好友
            relations.ForEach(x =>
            {
                friends.Add(x.Passiver == userId ? x.Activer : x.Passiver);
            });

            var friendList = await _userEfBasicRepository.Where(x => friends.Contains(x.Id)).ToListAsync();
            var friendBriefs = friendList.Select(x =>
            {
                var dto = _mapper.Map<RelationBriefDto>(x);
                if (dto.Avatar != null)
                {
                    dto.Avatar = dto.Avatar.Replace(_configuration.GetSection("FileStorage:AvatarImagesLocation").Value!, null);
                }

                return dto;
            });

            //TODO以后做群组需要加
            return new ServiceResult<IEnumerable<RelationBriefDto>>()
            {
                Value = friendBriefs
            };
        }
    }
}
