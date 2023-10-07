using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Data;
using ThreeL.Blob.Infra.Repository.EfCore.Repositories;

namespace ThreeL.Blob.Infra.Repository.EfCore.Mysql
{
    public class MySqlUnitOfWork<TDbContext> : UnitOfWork<TDbContext> where TDbContext : MySqlDbContext
    {
        public MySqlUnitOfWork(TDbContext context): base(context)
        {
        }

        protected override IDbContextTransaction GetDbContextTransaction(
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted
            , bool distributed = false)
        {
            return _dbContext.Database.BeginTransaction(isolationLevel);
        }
    }
}
