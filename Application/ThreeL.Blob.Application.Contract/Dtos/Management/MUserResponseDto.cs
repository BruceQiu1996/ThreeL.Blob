namespace ThreeL.Blob.Application.Contract.Dtos.Management
{
    public class MUserResponseDto
    {
        public long Id { get; set; }
        public string UserName { get; set; }
        public string Role { get; set; }
        public string AccessToken { get; set; }
    }
}
