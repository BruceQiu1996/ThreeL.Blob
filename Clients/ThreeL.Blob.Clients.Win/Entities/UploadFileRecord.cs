using System;

namespace ThreeL.Blob.Clients.Win.Entities
{
    public class UploadFileRecord
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public long FileId { get; set; }
        public string FileName { get; set; }
        public string FileLocation { get; set; }
        public long Size { get; set; }
        public long TransferBytes { get; set; }
        public DateTime CreateTime { get; set; } = DateTime.UtcNow;
        public string Code { get; set; }
        public Status Status { get; set; }
    }

    public enum Status
    {
        Doing = 1,
        Completed = 2,
        Suspend = 3,
        Error = 4
    }
}
