namespace ThreeL.Blob.Chat.Application.Contract.Dtos
{
    public class MessageDto
    {
        public string MessageId { get; set; }
        public DateTime LocalCreateTime { get; set; }
        public DateTime RemoteTime { get; set; }
        public long From { get; set; }
        public long To { get; set; }
    }
}
