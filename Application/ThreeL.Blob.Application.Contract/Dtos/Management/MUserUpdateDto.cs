namespace ThreeL.Blob.Application.Contract.Dtos.Management
{
    public class MUserUpdateDto
    {
        public string UserName { get; set; }
        public string Role { get; set; }
        public long? Size { get; set; }
        public bool IsDeleted { get; set; }
    }
}
