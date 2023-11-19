using System;
using ThreeL.Blob.Shared.Domain.Metadata.FileObject;

namespace ThreeL.Blob.Clients.Win.Entities
{
    public class UploadFileRecord
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public long UserId { get; set; }
        public long FileId { get; set; }
        public string FileName { get; set; }
        public string FileLocation { get; set; }
        public long Size { get; set; }
        public long TransferBytes { get; set; }
        public DateTime CreateTime { get; set; } = DateTime.Now;
        public DateTime UploadFinishTime { get; set; }
        public string Code { get; set; }
        public FileUploadingStatus Status { get; set; }
    }
}
