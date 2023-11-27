using AutoMapper;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;
using ThreeL.Blob.Application.Contract.Dtos;
using ThreeL.Blob.Application.Contract.Protos;
using ThreeL.Blob.Application.Contract.Services;
using ThreeL.Blob.Domain.Aggregate.FileObject;
using ThreeL.Blob.Domain.Aggregate.User;
using ThreeL.Blob.Infra.Core.Utils;
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
        private readonly IEfBasicRepository<FileObject, long> _fileObjectEfBasicRepository;
        private readonly IEfBasicRepository<FileObjectShareRecord, long> _fileObjectShareRecordEfBasicRepository;
        private readonly IConfiguration _configuration;
        private readonly IRedisProvider _redisProvider;

        public RelationService(IEfBasicRepository<FriendRelation, long> friendRelationEfBasicRepository,
                               IEfBasicRepository<FriendApply, long> friendApplyEfBasicRepository,
                               IEfBasicRepository<User, long> userEfBasicRepository,
                               IEfBasicRepository<FileObject, long> fileObjectEfBasicRepository,
                               IEfBasicRepository<FileObjectShareRecord, long> fileObjectShareRecordEfBasicRepository,
                               IMapper mapper,
                               IConfiguration configuration,
                               IRedisProvider redisProvider)
        {
            _mapper = mapper;
            _redisProvider = redisProvider;
            _configuration = configuration;
            _friendRelationEfBasicRepository = friendRelationEfBasicRepository;
            _friendApplyEfBasicRepository = friendApplyEfBasicRepository;
            _fileObjectEfBasicRepository = fileObjectEfBasicRepository;
            _fileObjectShareRecordEfBasicRepository = fileObjectShareRecordEfBasicRepository;
            _userEfBasicRepository = userEfBasicRepository;
        }

        public async Task<CommonResponse> AddFriendApplyAsync(AddFriendApplyRequest request, ServerCallContext serverCallContext)
        {
            var userName = serverCallContext.GetHttpContext().User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Sid)?.Value;
            var userId = long.Parse(serverCallContext.GetHttpContext().User.Identity.Name!);
            var friend = await _userEfBasicRepository.GetAsync(request.Target);
            if (friend == null)
            {
                return new CommonResponse()
                {
                    Success = false,
                    Message = "好友不存在"
                };
            }

            var relation = await _friendRelationEfBasicRepository
                .FirstOrDefaultAsync(x => (x.Passiver == userId && x.Activer == request.Target) || (x.Passiver == request.Target && x.Activer == userId));

            if (relation != null)
            {
                return new CommonResponse()
                {
                    Message = "无法重复添加好友",
                    Success = false
                };
            }

            var apply = await _friendApplyEfBasicRepository
                .FirstOrDefaultAsync(x => ((x.Passiver == userId && x.Activer == request.Target) || (x.Passiver == request.Target && x.Activer == userId)) && x.Status == Shared.Domain.Metadata.User.FriendApplyStatus.Unhandled);

            if (apply != null)
            {
                return new CommonResponse()
                {
                    Message = "存在相同的未处理的好友请求",
                    Success = false
                };
            }

            var newApply = new FriendApply()
            {
                Activer = userId,
                Passiver = request.Target,
                PassiverName = friend.UserName,
                ActiverName = userName,
                CreateTime = DateTime.Now,
                UpdateTime = DateTime.Now,
                Status = Shared.Domain.Metadata.User.FriendApplyStatus.Unhandled
            };
            await _friendApplyEfBasicRepository.InsertAsync(newApply);

            return new CommonResponse()
            {
                Success = true,
                Message = "好友请求已发送"
            };
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

        public async Task<ServiceResult<RelationBriefDto>> GetRelationAsync(long userId, long target)
        {
            //获取所有的好友
            var relation = await _friendRelationEfBasicRepository
                .FirstOrDefaultAsync(x =>(x.Passiver == userId && x.Activer == target) || (x.Activer == userId && x.Passiver == target));

            if (relation == null) 
            {
                return new ServiceResult<RelationBriefDto>(System.Net.HttpStatusCode.BadRequest,"好友关系异常")
                {
                    Value = null
                };
            }

            var friend = await _userEfBasicRepository.GetAsync(target);
            if (friend == null)
            {
                return new ServiceResult<RelationBriefDto>(System.Net.HttpStatusCode.BadRequest, "好友不存在")
                {
                    Value = null
                };
            }

            var friendBrief = _mapper.Map<RelationBriefDto>(friend);
            if (friendBrief.Avatar != null)
            {
                friendBrief.Avatar = friendBrief.Avatar.Replace(_configuration.GetSection("FileStorage:AvatarImagesLocation").Value!, null);
            }

            return new ServiceResult<RelationBriefDto>()
            {
                Value = friendBrief
            };
        }

        public async Task<HandleAddFriendApplyResponse> HandleAddFriendApplyAsync(HandleAddFriendApplyRequest request, ServerCallContext serverCallContext)
        {
            var userId = long.Parse(serverCallContext.GetHttpContext().User.Identity.Name!);
            var apply = await _friendApplyEfBasicRepository.GetAsync(request.ApplyId);
            if (apply == null || apply.Status != Shared.Domain.Metadata.User.FriendApplyStatus.Unhandled)
            {
                return new HandleAddFriendApplyResponse()
                {
                    Success = false,
                    Message = "好友请求不存在或已处理"
                };
            }

            if (apply.Passiver != userId)
            {
                return new HandleAddFriendApplyResponse()
                {
                    Success = false,
                    Message = "无权处理该好友请求"
                };
            }

            if (request.IsAgree)
            {
                apply.Status = Shared.Domain.Metadata.User.FriendApplyStatus.Agreed;
                apply.UpdateTime = DateTime.Now;
                var relation = new FriendRelation()
                {
                    Activer = apply.Activer,
                    Passiver = apply.Passiver,
                    CreateTime = DateTime.Now,
                    ActiverName = apply.ActiverName,
                    PassiverName = apply.PassiverName,
                };
                await _friendApplyEfBasicRepository.UpdateAsync(apply);
                await _friendRelationEfBasicRepository.InsertAsync(relation);
            }
            else
            {
                apply.Status = Shared.Domain.Metadata.User.FriendApplyStatus.Rejected;
                apply.UpdateTime = DateTime.Now;
                await _friendApplyEfBasicRepository.UpdateAsync(apply);
            }

            return new HandleAddFriendApplyResponse()
            {
                Success = true,
                Message = "处理成功",
                Activer = apply.Activer,
                Passiver = apply.Passiver,
            };
        }

        public async Task PreheatAsync()
        {
            await _redisProvider.KeyDelAsync(Const.REDIS_FRIEND_RELATIONS);
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

        public async Task<ServiceResult<IEnumerable<ApplyDto>>> QueryApplysAsync(long userId)
        {
            var applys = await _friendApplyEfBasicRepository
                .Where(x => x.Passiver == userId || x.Activer == userId).ToListAsync();

            return new ServiceResult<IEnumerable<ApplyDto>>(applys.Select(_mapper.Map<ApplyDto>));
        }

        public async Task<ServiceResult<IEnumerable<RelationBriefDto>>> QueryRelationsByKeywordAsync(long userId, string key)
        {
            var users = await _userEfBasicRepository.Where(x => x.UserName.Contains(key)).ToListAsync();
            if (users.Count < 2)
            {

            }
            List<RelationBriefDto> friendRelationBriefDtos = new List<RelationBriefDto>();
            //users.ForEach(async x =>
            //{
            //    var min = Math.Min(userId, x.Id);
            //    var max = Math.Max(userId, x.Id);
            //    if (!await _redisProvider.SetIsMemberAsync(Const.REDIS_FRIEND_RELATIONS, $"{min}-{max}"))
            //    {
            //        x.Avatar = x.Avatar == null ? x.Avatar : x.Avatar.Replace(_configuration.GetSection("FileStorage:AvatarImagesLocation").Value!, null);
            //        friendRelationBriefDtos.Add(_mapper.Map<RelationBriefDto>(x));
            //    }
            //});


            foreach (var user in users)
            {
                var min = Math.Min(userId, user.Id);
                var max = Math.Max(userId, user.Id);
                if (!await _redisProvider.SetIsMemberAsync(Const.REDIS_FRIEND_RELATIONS, $"{min}-{max}"))
                {
                    user.Avatar = user.Avatar == null ? user.Avatar : user.Avatar.Replace(_configuration.GetSection("FileStorage:AvatarImagesLocation").Value!, null);
                    friendRelationBriefDtos.Add(_mapper.Map<RelationBriefDto>(user));
                }
            }

            return new ServiceResult<IEnumerable<RelationBriefDto>>(friendRelationBriefDtos);
        }

        public async Task<SendFileResponse> SendFileAsync(SendFileRequest request, ServerCallContext serverCallContext)
        {
            var userId = long.Parse(serverCallContext.GetHttpContext().User.Identity.Name!);
            var file = await _fileObjectEfBasicRepository.GetAsync(request.FileId);
            if (file == null || file.CreateBy != userId || file.Status != Shared.Domain.Metadata.FileObject.FileStatus.Normal)
            {
                return new SendFileResponse()
                {
                    Success = false,
                    Message = "文件不存在或文件状态异常"
                };
            }

            var token = TokenGenerator.GenerateToken(32);
            var record = new FileObjectShareRecord(token,file.Id,userId, request.Target);
            record.CreateTime = DateTime.Now;
            record.ExpireTime = DateTime.Now.AddDays(3);//TODO配置
            await _fileObjectShareRecordEfBasicRepository.InsertAsync(record);

            return new SendFileResponse()
            {
                Success = true,
                Token = token
            };
        }
    }
}
