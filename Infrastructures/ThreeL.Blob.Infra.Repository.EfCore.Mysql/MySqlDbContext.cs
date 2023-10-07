using Microsoft.EntityFrameworkCore;
using ThreeL.Blob.Infra.Repository.Entities;

namespace ThreeL.Blob.Infra.Repository.EfCore.Mysql
{
    public class MySqlDbContext : BlobDbContext
    {
        public MySqlDbContext(DbContextOptions options,IEntityInfo entityInfo)
            : base(options, entityInfo)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //System.Diagnostics.Debugger.Launch();
            modelBuilder.HasCharSet("utf8mb4 ");
            base.OnModelCreating(modelBuilder);
        }
    }
}