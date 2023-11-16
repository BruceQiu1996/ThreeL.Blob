using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ThreeL.Blob.Infra.Core.Extensions.Microsoft;
using ThreeL.Blob.Infra.Redis.Extensions;
using ThreeL.Blob.Infra.Repository.EfCore.Mysql.Configuration;
using ThreeL.Blob.Infra.Repository.EfCore.Mysql.Extensions;
using ThreeL.Blob.Infra.Repository.Entities;
using ThreeL.Blob.Shared.Application.Contract;
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
            AddJwtServiceCache();
            AddHelpers();
        }

        public void AddChatInfraService()
        {
            AddRedisCache();
            AddAutoMapper();
            AddJwtServiceCache();
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

        internal void AddJwtServiceCache()
        {
            _services.AddScoped<IJwtService,JwtService>();
        }
    }
}
