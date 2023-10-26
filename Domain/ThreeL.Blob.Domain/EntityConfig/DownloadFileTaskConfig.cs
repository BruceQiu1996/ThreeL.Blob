using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ThreeL.Blob.Domain.Aggregate.FileObject;
using ThreeL.Blob.Shared.Domain.Entities;

namespace ThreeL.Blob.Domain.EntityConfig
{
    public class DownloadFileTaskConfig : AbstractEntityTypeConfiguration<DownloadFileTask, string>
    {
        public override void Configure(EntityTypeBuilder<DownloadFileTask> builder)
        {
            base.Configure(builder);
        }
    }
}
