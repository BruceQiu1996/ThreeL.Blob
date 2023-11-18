using Microsoft.Extensions.Logging;
using ThreeL.Blob.Application.Contract.Services;
using ThreeL.Blob.Domain.Aggregate.User;
using ThreeL.Blob.Infra.Redis;
using ThreeL.Blob.Infra.Repository.IRepositories;
using ThreeL.Blob.Shared.Application.Contract.Services;

namespace ThreeL.Blob.Application.Services
{
    //与聊天服务器之间的通信
    public class ChatGrpcService : IChatGrpcService, IAppService
    {
        private readonly IRedisProvider _redisProvider;
        private readonly IEfBasicRepository<User, long> _efBasicRepository;
        private readonly IEfBasicRepository<FriendRelation, long> _friendRelationEfBasicRepository;
        private readonly ILogger<ChatGrpcService> _logger;
        public ChatGrpcService(IRedisProvider redisProvider,
                               IEfBasicRepository<User, long> efBasicRepository,
                               IEfBasicRepository<FriendRelation, long> friendRelationEfBasicRepository,
                               ILogger<ChatGrpcService> logger)
        {
            _logger = logger;
            _redisProvider = redisProvider;
            _efBasicRepository = efBasicRepository;
            _friendRelationEfBasicRepository = friendRelationEfBasicRepository;
        }
    }
}
