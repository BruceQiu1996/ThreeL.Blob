namespace ThreeL.Blob.Chat.Application.Contract.Dtos
{
    public class FolderMessageResponseDto : MessageDto
    {
        public long FileObjectId { get; set; }
        public string FileName { get; set; }
        public string Token { get; set; }
    }
}
