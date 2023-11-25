namespace ThreeL.Blob.Application.Contract.Dtos.Management
{
    public class MQueryUsersResponseDto
    {
        public int Count { get; set; }
        public IEnumerable<MUserBriefResponseDto> Users { get; set; }
    }
}
