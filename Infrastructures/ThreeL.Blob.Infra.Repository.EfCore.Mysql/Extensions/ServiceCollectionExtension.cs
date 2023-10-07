using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ThreeL.Blob.Infra.Repository.EfCore.Repositories;
using ThreeL.Blob.Infra.Repository.IRepositories;

namespace ThreeL.Blob.Infra.Repository.EfCore.Mysql.Extensions
{
    public static class ServiceCollectionExtension
    {
        public static IServiceCollection AddInfraEfCoreMySql(this IServiceCollection services, Action<DbContextOptionsBuilder> optionsBuilder)
        {
            services.TryAddScoped<IUnitOfWork, MySqlUnitOfWork<MySqlDbContext>>();
            services.TryAddScoped(typeof(IEfBasicRepository<,>), typeof(EfBasicRepository<,>));
            services.AddDbContext<DbContext, MySqlDbContext>(optionsBuilder);

            return services;
        }
    }
}
