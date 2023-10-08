using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ThreeL.Blob.Domain.Aggregate.FileObject;
using ThreeL.Blob.Shared.Domain.Entities;

namespace ThreeL.Blob.Domain.EntityConfig
{
    public class FileObjectConfig : AbstractEntityTypeConfiguration<FileObject, long>
    {
        public override void Configure(EntityTypeBuilder<FileObject> builder)
        {
            base.Configure(builder);

            builder.Property(x => x.FileName).HasMaxLength(256);
            builder.Property(x => x.Location).HasMaxLength(500);
            builder.Property(x => x.Code).HasMaxLength(500);
        }
    }
}
