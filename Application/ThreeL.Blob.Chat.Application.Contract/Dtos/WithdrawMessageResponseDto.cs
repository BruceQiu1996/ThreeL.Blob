namespace ThreeL.Blob.Chat.Application.Contract.Dtos
{
    public class WithdrawMessageResponseDto
    {
        public string MessageId { get; set; }
        public long From { get; set; }
        public long To { get; set; }
    }
}
