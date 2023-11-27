using ThreeL.Blob.Infra.Repository.Entities;
using ThreeL.Blob.Shared.Domain.Entities;

namespace ThreeL.Blob.Domain.Aggregate.FileObject
{
    public class FileObjectShareRecord : DomainEntity<long>, IBasicAuditInfo<long>
    {
        public string? Token { get; set; }
        public long FileObjectId { get; set; }
        public long CreateBy { get; set; }
        public long? Target { get; set; } //分享的目标，如果为null，则任何人都可以访问
        public DateTime CreateTime { get; set; } = DateTime.Now;
        public DateTime? ExpireTime { get; set; }

        public FileObjectShareRecord(string token, long fileObjectId, long createBy, long target)
        {
            Token = token;
            FileObjectId = fileObjectId;
            CreateBy = createBy;
            Target = target;
        }

        public FileObjectShareRecord()
        {
            
        }
    }
}
