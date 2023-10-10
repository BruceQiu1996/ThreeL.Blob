using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ThreeL.Blob.Infra.Redis.Configurations;
using ThreeL.Blob.Infra.Redis.Providers;

namespace ThreeL.Blob.Infra.Redis.Extensions
{
    public static class ServiceCollectionExtension
    {
        public static void AddInfraRedis(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<RedisOptions>(configuration.GetSection("RedisOptions"));
            services.AddSingleton<DefaultDatabaseProvider>()
                .AddSingleton<RedisProvider>()
                .AddSingleton<IRedisProvider>(x => x.GetRequiredService<RedisProvider>()).AddSingleton<IDistributedLocker>(x => x.GetRequiredService<RedisProvider>());
        }
    }
}
