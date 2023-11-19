namespace ThreeL.Blob.Clients.Win.ViewModels.Message
{
    public class WithDrawMessageViewModel : MessageViewModel
    {
        public WithDrawMessageViewModel() : base(Shared.Domain.Metadata.Message.MessageType.Withdraw)
        {
        }
    }
}
