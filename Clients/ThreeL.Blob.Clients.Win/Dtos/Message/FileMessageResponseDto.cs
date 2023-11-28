namespace ThreeL.Blob.Clients.Win.Dtos.Message
{
    public class FileMessageResponseDto : MessageDto
    {
        public long FileObjectId { get; set; }
        public string FileName { get; set; }
        public long Size { get; set; }
        public string Token { get; set; }
    }
}
