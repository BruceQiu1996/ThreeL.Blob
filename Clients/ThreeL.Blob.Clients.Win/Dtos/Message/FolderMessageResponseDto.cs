namespace ThreeL.Blob.Clients.Win.Dtos.Message
{
    internal class FolderMessageResponseDto : MessageDto
    {
        public long FileObjectId { get; set; }
        public string FileName { get; set; }
        public string Token { get; set; }
    }
}
