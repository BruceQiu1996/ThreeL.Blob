namespace ThreeL.Blob.Application.Contract.Dtos
{
    public class FileObjectShareRecordDto
    {
        public long Id { get; set; }
        public string? Token { get; set; }
        public long FileObjectId { get; set; }
        public string FileObjectName { get; set; }
        public long? FileSize { get; set; }
        public string TargetName { get; set; }
        public DateTime CreateTime { get; set; } = DateTime.Now;
        public DateTime? ExpireTime { get; set; }
        public bool IsFolder { get; set; }
    }
}
