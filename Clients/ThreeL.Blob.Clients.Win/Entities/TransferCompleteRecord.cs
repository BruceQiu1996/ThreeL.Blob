using System;

namespace ThreeL.Blob.Clients.Win.Entities
{
    public class TransferCompleteRecord
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public long  UserId { get; set; }
        public string TaskId { get; set; }
        public long FileId { get; set; }
        public string FileName { get; set; }
        public string? FileLocation { get; set; }
        public DateTime BeginTime { get; set; }
        public DateTime FinishTime { get; set; }
        public string Description { get; set; }
        public bool IsUpload { get; set; }
        public bool Success { get; set; }
        public string? Reason { get; set; }
    }
}
