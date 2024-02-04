using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ThreeL.Blob.Domain.Aggregate.User;
using ThreeL.Blob.Shared.Domain.Entities;

namespace ThreeL.Blob.Domain.EntityConfig
{
    public class UserConfig : AbstractEntityTypeConfiguration<User, long>
    {
        public UserConfig()
        {
            
        }
        public override void Configure(EntityTypeBuilder<User> builder)
        {
            base.Configure(builder);

            builder.Property(x => x.UserName).HasMaxLength(16);
            builder.Property(x => x.Avatar).HasMaxLength(500);
            builder.Property(x => x.Password).HasMaxLength(128);
            builder.Property(x => x.UploadMaxSizeLimit).HasDefaultValue(1024 * 1024 * 1024); //默认每人单文件1G
            builder.Property(x => x.DaliyUploadMaxSizeLimit).HasDefaultValue((long)1024 * 1024 * 1024 * 10); //默认每人每日文件10G
            builder.HasData(new User()
            {
                Id = 1,
                CreateTime = DateTime.Now,
                UserName = "admin",
                Password = "87NLwc3m69ImImDt4PMbig==.cxgVDtWaQO7Z0p/2iAuPXQ6C1jM5Udt/qtW9m0ARpCY=",
                Role = Shared.Domain.Metadata.User.Role.Admin,
                Location = "D:\\ThreeL_blob\\admin"
            });
        }
    }
}
