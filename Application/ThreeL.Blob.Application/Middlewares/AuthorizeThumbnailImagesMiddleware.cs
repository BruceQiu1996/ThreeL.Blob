using Microsoft.AspNetCore.Http;

namespace ThreeL.Blob.Application.Middlewares
{
    /// <summary>
    /// 访问缩略路的中间件
    /// </summary>
    public class AuthorizeThumbnailImagesMiddleware
    {
        RequestDelegate _next;
        private readonly string _url;

        public AuthorizeThumbnailImagesMiddleware(RequestDelegate next, string folder)
        {
            _next = next;
            _url = folder;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.Request.Path.StartsWithSegments(_url))
            {
                if (context.User.Identity.IsAuthenticated)
                {
                    var userId = context.User.Identity.Name;
                    if (!context.Request.Path.Value.Contains($"/{userId}/"))
                    {
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    }
                    else
                    {
                        await _next(context);
                    }
                }
                else
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                }
            }
            else
            {
                await _next(context);
            }
        }
    }
}
