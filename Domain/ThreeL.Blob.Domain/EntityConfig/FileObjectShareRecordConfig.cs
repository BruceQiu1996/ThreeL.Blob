using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ThreeL.Blob.Domain.Aggregate.FileObject;
using ThreeL.Blob.Shared.Domain.Entities;

namespace ThreeL.Blob.Domain.EntityConfig
{
    internal class FileObjectShareRecordConfig : AbstractEntityTypeConfiguration<FileObjectShareRecord, long>
    {
        public override void Configure(EntityTypeBuilder<FileObjectShareRecord> builder)
        {
            base.Configure(builder);

            builder.Property(x => x.Token).HasMaxLength(50);
        }
    }
}
