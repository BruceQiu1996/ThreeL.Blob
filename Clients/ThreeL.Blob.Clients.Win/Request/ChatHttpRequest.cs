using Microsoft.Extensions.Options;
using ThreeL.Blob.Clients.Win.Configurations;

namespace ThreeL.Blob.Clients.Win.Request
{
    public class ChatHttpRequest : HttpRequest
    {
        public ChatHttpRequest(IOptions<RemoteOptions> options) : base(options.Value.ChatHost, options.Value.ChatPort)
        {
        }
    }
}
