namespace ThreeL.Blob.Application.Contract.Dtos.Management
{
    public class MUserBriefResponseDto
    {
        public long Id { get; set; }
        public string UserName { get; set; }
        public string Role { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime? LastLoginTime { get; set; }
        public bool IsDeleted { get; set; }
    }
}
