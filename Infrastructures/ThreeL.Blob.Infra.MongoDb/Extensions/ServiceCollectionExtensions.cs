using Microsoft.Extensions.DependencyInjection;
using ThreeL.Blob.Infra.MongoDb.Configuration;
using ThreeL.Blob.Infra.MongoDb.Repositories;
using ThreeL.Blob.Infra.Repository.IRepositories;

namespace ThreeL.Blob.Infra.MongoDb.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddInfraMongo<TContext>(this IServiceCollection services, Action<MongoOptions> configuraton) where TContext : IMongoContext
        {
            services.Configure(configuraton);
            services.AddSingleton(typeof(IMongoContext), typeof(TContext));
            services.AddTransient(typeof(IMongoRepository<>), typeof(MongoRepository<>));
        }
    }
}
