using System;
using ThreeL.Blob.Shared.Domain.Metadata.FileObject;

namespace ThreeL.Blob.Clients.Win.Entities
{
    public class DownloadFileRecord
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public long UserId { get; set; }
        public string TaskId { get; set; }
        public long FileId { get; set; }
        public string FileName { get; set; }
        public string Code { get; set; }
        public string TempFileLocation { get; set; }
        public string? Location { get; set; }
        public long Size { get; set; }
        public long TransferBytes { get; set; }
        public DateTime CreateTime { get; set; } = DateTime.UtcNow;
        public DateTime DownloadFinishTime { get; set; }
        public FileDownloadingStatus Status { get; set; }
    }
}
