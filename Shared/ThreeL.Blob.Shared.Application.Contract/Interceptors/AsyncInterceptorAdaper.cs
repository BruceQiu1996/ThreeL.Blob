using Castle.DynamicProxy;

namespace ThreeL.Blob.Shared.Application.Contract.Interceptors
{
    public class AsyncInterceptorAdaper<TAsyncInterceptor> : AsyncDeterminationInterceptor where TAsyncInterceptor : IAsyncInterceptor
    {
        public AsyncInterceptorAdaper(TAsyncInterceptor asyncInterceptor)
            : base(asyncInterceptor)
        { }
    }
}
