using ThreeL.Blob.Infra.Repository.Entities;

namespace ThreeL.Blob.Domain.Entities
{
    public class UploadRecord : IEntity<string>
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public long Size { get; set; }
        public long UploadedSize { get; set; }
        public string UploadUserName { get; set; }
        public string FileName { get; set; }
        public string Code { get; set; }
        public DateTime UploadTime { get; set; }
    }
}
