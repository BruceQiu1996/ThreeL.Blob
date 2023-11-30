using Microsoft.Extensions.Options;
using ThreeL.Blob.Clients.Win.Configurations;

namespace ThreeL.Blob.Clients.Win.Request
{
    public class ApiHttpRequest : HttpRequest
    {
        public ApiHttpRequest(IOptions<RemoteOptions> options) : base(options.Value.APIHost, options.Value.APIPort)
        {
        }
    }
}
