namespace ThreeL.Blob.Chat.Application.Contract.Dtos
{
    public class QueryChatRecordResponseDto
    {
        public int Count { get; set; }
        public IEnumerable<ChatRecordResponseDto> Records { get; set; }
    }
}
