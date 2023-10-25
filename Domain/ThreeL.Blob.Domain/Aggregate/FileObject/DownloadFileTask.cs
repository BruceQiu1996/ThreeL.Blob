using ThreeL.Blob.Infra.Repository.Entities;
using ThreeL.Blob.Shared.Domain.Entities;
using ThreeL.Blob.Shared.Domain.Metadata.FileObject;

namespace ThreeL.Blob.Domain.Aggregate.FileObject
{
    public class DownloadFileTask : DomainEntity<string>, IBasicAuditInfo<long>
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public long FileId { get; set; }
        public long CreateBy { get; set; }
        public DateTime CreateTime { get; set ; }
        public DownloadTaskStatus Status { get; set; }
    }
}
