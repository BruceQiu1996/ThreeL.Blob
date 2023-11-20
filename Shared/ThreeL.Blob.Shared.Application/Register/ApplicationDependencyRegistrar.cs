using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ThreeL.Blob.Infra.Core.Extensions.Microsoft;
using ThreeL.Blob.Infra.MongoDb;
using ThreeL.Blob.Infra.MongoDb.Configuration;
using ThreeL.Blob.Infra.MongoDb.Extensions;
using ThreeL.Blob.Infra.Redis;
using ThreeL.Blob.Infra.Redis.Extensions;
using ThreeL.Blob.Infra.Repository.EfCore.Mysql.Configuration;
using ThreeL.Blob.Infra.Repository.EfCore.Mysql.Extensions;
using ThreeL.Blob.Infra.Repository.Entities;
using ThreeL.Blob.Shared.Application.Contract;
using ThreeL.Blob.Shared.Application.Contract.Configurations;
using ThreeL.Blob.Shared.Application.Contract.Services;
using ThreeL.Blob.Shared.Application.Services;

namespace ThreeL.Blob.Shared.Application.Register
{
    public partial class ApplicationDependencyRegistrar
    {
        private readonly IApplicationAssemblyInfo _applicationAssemblyInfo;
        private readonly IServiceCollection _services;
        public ApplicationDependencyRegistrar(IApplicationAssemblyInfo applicationAssemblyInfo, IServiceCollection services)
        {
            _applicationAssemblyInfo = applicationAssemblyInfo;
            _services = services;
        }

        public void AddBlobInfraService()
        {
            AddEntitiesInfo();
            AddEfCoreContext();
            AddRedisCache();
            AddFluentValidator();
            AddAutoMapper();
            AddJwtServiceCache(true);
            AddHelpers();
        }

        public void AddChatInfraService()
        {
            AddRedisCache();
            AddAutoMapper();
            AddMongo();
            AddJwtServiceCache(false);
        }

        internal void AddEntitiesInfo()
        {
            var serviceType = typeof(IEntityInfo);
            var implType = _applicationAssemblyInfo.DomainAssembly.ExportedTypes.FirstOrDefault(type => type.IsAssignableTo(serviceType) && !type.IsAbstract);
            if (implType is null)
                throw new NotImplementedException(nameof(IEntityInfo));
            else
                _services.AddScoped(serviceType, implType);
        }

        internal void AddEfCoreContext()
        {
            var mysqlConfig = _services.GetConfiguration().GetMysqlSection().Get<MysqlOptions>();
            var serverVersion = new MariaDbServerVersion(new Version(8, 0, 29));
            _services.AddInfraEfCoreMySql(options =>
            {
                options.UseMySql(mysqlConfig.ConnectionString, serverVersion, optionsBuilder =>
                {
                    optionsBuilder.MinBatchSize(4).MigrationsAssembly(_applicationAssemblyInfo.MigrationsAssembly.FullName)
                                                .UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                });
            });
        }

        internal void AddRedisCache()
        {
            _services.AddInfraRedis(_services.GetConfiguration());
        }

        internal void AddMongo()
        {
            var config = _services.GetConfiguration().GetSection("MongoOptions").Get<MongoOptions>();
            _services.AddInfraMongo<MongoContext>(options =>
            {
                options.ConnectionString = config.ConnectionString;
                options.PluralizeCollectionNames = config.PluralizeCollectionNames;
            });
        }

        internal void AddJwtServiceCache(bool generateKey)
        {
            _services.AddScoped<IJwtService, JwtService>(x => 
            {
                return new JwtService(x.GetRequiredService<IOptions<JwtOptions>>(), 
                    x.GetRequiredService<IOptions<SystemOptions>>(), 
                    x.GetRequiredService<IRedisProvider>(), generateKey);
            });
        }
    }
}
