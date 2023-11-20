using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ThreeL.Blob.Application.Contract.Dtos;
using ThreeL.Blob.Application.Contract.Protos;
using ThreeL.Blob.Application.Contract.Services;
using ThreeL.Blob.Domain.Aggregate.User;
using ThreeL.Blob.Infra.Redis;
using ThreeL.Blob.Infra.Repository.IRepositories;
using ThreeL.Blob.Shared.Application.Contract.Configurations;
using ThreeL.Blob.Shared.Application.Contract.Services;

namespace ThreeL.Blob.Application.Services
{
    public class RelationService : IRelationService, IAppService, IPreheatService
    {
        private readonly IMapper _mapper;
        private readonly IEfBasicRepository<FriendApply, long> _friendApplyEfBasicRepository;
        private readonly IEfBasicRepository<FriendRelation, long> _friendRelationEfBasicRepository;
        private readonly IEfBasicRepository<User, long> _userEfBasicRepository;
        private readonly IConfiguration _configuration;
        private readonly IChatGrpcService _chatGrpcService;
        private readonly IRedisProvider _redisProvider;
        public RelationService(IEfBasicRepository<FriendRelation, long> friendRelationEfBasicRepository,
                               IEfBasicRepository<FriendApply, long> friendApplyEfBasicRepository,
                               IEfBasicRepository<User, long> userEfBasicRepository,
                               IMapper mapper,
                               IConfiguration configuration,
                               IChatGrpcService chatGrpcService,
                               IRedisProvider redisProvider)
        {
            _mapper = mapper;
            _redisProvider = redisProvider;
            _chatGrpcService = chatGrpcService;
            _configuration = configuration;
            _friendRelationEfBasicRepository = friendRelationEfBasicRepository;
            _friendApplyEfBasicRepository = friendApplyEfBasicRepository;
            _userEfBasicRepository = userEfBasicRepository;
        }

        public async Task<ServiceResult> AddFriendApplyAsync(long userId, long target)
        {
            var friend = await _userEfBasicRepository.GetAsync(target);
            if (friend == null)
            {
                return new ServiceResult(System.Net.HttpStatusCode.BadRequest, "对方账号异常");
            }
            var relation = await _friendRelationEfBasicRepository
                .FirstOrDefaultAsync(x => (x.Passiver == userId && x.Activer == target) || (x.Passiver == target && x.Activer == userId));

            if (relation != null)
            {
                return new ServiceResult(System.Net.HttpStatusCode.BadRequest, "重复添加好友");
            }

            var apply = await _friendApplyEfBasicRepository
                .FirstOrDefaultAsync(x => ((x.Passiver == userId && x.Activer == target) || (x.Passiver == target && x.Activer == userId)) && x.Status == Shared.Domain.Metadata.User.FriendApplyStatus.Unhandled);

            if (apply != null)
            {
                return new ServiceResult(System.Net.HttpStatusCode.BadRequest, "存在相同的未处理的好友请求");
            }

            var newApply = new FriendApply()
            {
                Activer = userId,
                Passiver = target,
                Status = Shared.Domain.Metadata.User.FriendApplyStatus.Unhandled
            };
            await _friendApplyEfBasicRepository.InsertAsync(newApply);

            await _chatGrpcService.AddFriendApplyAsync(new AddFriendApplyRequest
            {
                ApplyId = newApply.Id,
                ApplyToId = target,
            });

            return new ServiceResult();
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

        public async Task PreheatAsync()
        {
            var relations = await _friendRelationEfBasicRepository.AllAsync();
            List<string> fRelations = new List<string>();
            foreach (var relation in relations)
            {
                var min = Math.Min(relation.Activer, relation.Passiver);
                var max = Math.Max(relation.Activer, relation.Passiver);
                fRelations.Add($"{min}-{max}");
            }

            await _redisProvider.SetAddAsync(Const.REDIS_FRIEND_RELATIONS, fRelations.ToArray());
        }

        public async Task<ServiceResult<IEnumerable<RelationBriefDto>>> QueryRelationsByKeywordAsync(long userId, string key)
        {
            var users = await _userEfBasicRepository.Where(x => x.UserName.Contains(key)).ToListAsync();
            List<RelationBriefDto> friendRelationBriefDtos = new List<RelationBriefDto>();
            users.ForEach(async x =>
            {
                var min = Math.Min(userId, x.Id);
                var max = Math.Max(userId, x.Id);
                if (!await _redisProvider.SetIsMemberAsync(Const.REDIS_FRIEND_RELATIONS, $"{min}-{max}"))
                {
                    friendRelationBriefDtos.Add(_mapper.Map<RelationBriefDto>(x));
                }
            });

            return new ServiceResult<IEnumerable<RelationBriefDto>>(friendRelationBriefDtos);
        }
    }
}
