using ThreeL.Blob.Shared.Domain.Entities;
using ThreeL.Blob.Shared.Domain.Metadata.User;

namespace ThreeL.Blob.Domain.Aggregate.User
{
    public class FriendApply : DomainEntity<long>
    {
        public long Activer { get; set; }
        public long Passiver { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime UpdateTime { get; set; }
        public string ActiverName { get; set; }
        public string PassiverName { get; set; }
        public FriendApplyStatus Status { get; set; }
    }
}
