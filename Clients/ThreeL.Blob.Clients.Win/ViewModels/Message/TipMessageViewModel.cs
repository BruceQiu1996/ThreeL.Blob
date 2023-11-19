using ThreeL.Blob.Shared.Domain.Metadata.Message;

namespace ThreeL.Blob.Clients.Win.ViewModels.Message
{
    public class TipMessageViewModel : MessageViewModel
    {
        public TipMessageViewModel() : base(MessageType.Tip)
        {
        }

        public string Tip { get; set; }
    }
}
