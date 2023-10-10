using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Data;
using ThreeL.Blob.Infra.Repository.IRepositories;

namespace ThreeL.Blob.Infra.Repository.EfCore.Repositories
{
    public abstract class UnitOfWork<TDbContext> : IUnitOfWork where TDbContext : DbContext
    {
        protected readonly TDbContext _dbContext;
        protected IDbContextTransaction? DbTransaction { get; set; }
        public bool IsStartingUow => _dbContext.Database.CurrentTransaction is not null;

        protected UnitOfWork(TDbContext context)
        {
            _dbContext = context;
        }

        protected abstract IDbContextTransaction GetDbContextTransaction(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted, bool distributed = false);

        public void BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted, bool distributed = false)
        {
            if (IsStartingUow)
                throw new ArgumentException($"UnitOfWork Error,{_dbContext.Database.CurrentTransaction}");
            else
                DbTransaction = GetDbContextTransaction();
        }

        public async Task CommitAsync(CancellationToken cancellationToken = default)
        {
            await DbTransaction?.CommitAsync();
        }

        public async Task RollbackAsync(CancellationToken cancellationToken = default)
        {
            await DbTransaction?.RollbackAsync();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (DbTransaction is not null)
                {
                    DbTransaction.Dispose();
                    DbTransaction = null;
                }
            }
        }
    }
}
