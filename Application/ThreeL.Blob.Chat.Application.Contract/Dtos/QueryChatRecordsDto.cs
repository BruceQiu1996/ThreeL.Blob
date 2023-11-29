namespace ThreeL.Blob.Chat.Application.Contract.Dtos
{
    public class QueryChatRecordsDto
    {
        public long Target { get; set; }
        public DateTime? LastTime { get; set; }
    }
}
