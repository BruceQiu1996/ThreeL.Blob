using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ThreeL.Blob.Domain.Aggregate.User;
using ThreeL.Blob.Shared.Domain.Entities;

namespace ThreeL.Blob.Domain.EntityConfig
{
    public class UserConfig : AbstractEntityTypeConfiguration<User, long>
    {
        public override void Configure(EntityTypeBuilder<User> builder)
        {
            base.Configure(builder);

            builder.Property(x => x.UserName).HasMaxLength(16);
            builder.Property(x => x.Avatar).HasMaxLength(500);
            builder.Property(x => x.Password).HasMaxLength(128);
            builder.Property(x => x.UploadMaxSizeLimit).HasDefaultValue(1024 * 1024 * 1024); //默认每人单文件1G
            builder.Property(x => x.DaliyUploadMaxSizeLimit).HasDefaultValue((long)1024 * 1024 * 1024 * 10); //默认每人每日文件10G
        }
    }
}
