using ThreeL.Blob.Infra.Repository.Entities;
using ThreeL.Blob.Shared.Domain.Entities;
using ThreeL.Blob.Shared.Domain.Metadata.User;

namespace ThreeL.Blob.Domain.Aggregate.User
{
    public class User : AggregateRoot<long>, ISoftDelete, IBasicAuditInfo<long>
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public bool IsDeleted { get; set; }
        public long CreateBy { get; set; }
        public DateTime CreateTime { get; set; }
        public string? Avatar { get; set; }
        public Role Role { get; set; }
        public DateTime? LastLoginTime { get; set; }
        public long UploadMaxSizeLimit { get; set; }
        public long DaliyUploadMaxSizeLimit { get; set; }
        public long TodayUploadMaxSize { get; set; }
        public long? DownloadSpeedLimit { get; set; }
        public long? MaxSpaceSize { get; set; }
        public string Location { get; set; }
    }
}
