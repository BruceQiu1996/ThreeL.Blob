using ThreeL.Blob.Shared.Domain.Entities;

namespace ThreeL.Blob.Domain.Aggregate.User
{
    public class FriendRelation : DomainEntity<long>
    {
        public long Activer { get; set; }
        public long Passiver { get; set; }
        public DateTime CreateTime { get; set; } = DateTime.UtcNow;
        public string ActiverName { get; set; }
        public string PassiverName { get; set; }
    }
}
