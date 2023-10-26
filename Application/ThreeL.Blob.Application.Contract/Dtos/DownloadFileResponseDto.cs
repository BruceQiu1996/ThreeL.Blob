namespace ThreeL.Blob.Application.Contract.Dtos
{
    public class DownloadFileResponseDto
    {
        public long FileId { get; set; }
        public string Code { get; set; }
        public string TaskId { get; set; }
        public long Size { get; set; }
        public string FileName { get; set; }
    }
}
