namespace ThreeL.Blob.Infra.Core.Utils
{
    public class ExpressionWaiter
    {
        public static async Task<bool> WaitInTime(Func<bool> expression, int seconds = 5)
        {
            var result = false;
            var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(seconds));
            var task = Task.Run(() =>
            {
                while (!expression.Invoke())
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        result = false;
                    }
                }

                result = true;
            }, cancellationToken.Token);
            await task;

            return result;
        }
    }
}
