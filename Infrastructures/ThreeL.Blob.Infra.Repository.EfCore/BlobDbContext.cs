using Microsoft.EntityFrameworkCore;
using ThreeL.Blob.Infra.Repository.Entities;

namespace ThreeL.Blob.Infra.Repository.EfCore
{
    public class BlobDbContext : DbContext
    {
        private readonly IEntityInfo _entityInfo;

        protected BlobDbContext(DbContextOptions options, IEntityInfo entityInfo) : base(options)
        {
            _entityInfo = entityInfo;
            Database.AutoTransactionBehavior = AutoTransactionBehavior.Never;
            //ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var flag = false;
            if (Database.AutoTransactionBehavior == AutoTransactionBehavior.Never
                && ChangeTracker.Entries().Count() > 1
                && Database.CurrentTransaction == null)
            {
                Database.AutoTransactionBehavior = AutoTransactionBehavior.Always;
                flag = true;
            }

            var result = base.SaveChangesAsync(cancellationToken);
            if (flag)
                Database.AutoTransactionBehavior = AutoTransactionBehavior.Never;

            return result;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder) => _entityInfo.OnModelCreating(modelBuilder);
    }
}