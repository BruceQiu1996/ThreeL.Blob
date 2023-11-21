using ThreeL.Blob.Shared.Domain.Metadata.User;

namespace ThreeL.Blob.Application.Contract.Dtos
{
    public class ApplyDto
    {
        public long Id { get; set; }
        public long Activer { get; set; }
        public long Passiver { get; set; }
        public DateTime CreateTime { get; set; }
        public string ActiverName { get; set; }
        public string PassiverName { get; set; }
        public FriendApplyStatus Status { get; set; }
    }
}
