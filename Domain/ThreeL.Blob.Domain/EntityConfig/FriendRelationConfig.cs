using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ThreeL.Blob.Domain.Aggregate.User;
using ThreeL.Blob.Shared.Domain.Entities;

namespace ThreeL.Blob.Domain.EntityConfig
{
    public class FriendRelationConfig : AbstractEntityTypeConfiguration<FriendRelation, long>
    {
        public override void Configure(EntityTypeBuilder<FriendRelation> builder)
        {
            base.Configure(builder);

            builder.Property(x => x.ActiverName).HasMaxLength(16);
            builder.Property(x => x.PassiverName).HasMaxLength(16);
        }
    }
}
