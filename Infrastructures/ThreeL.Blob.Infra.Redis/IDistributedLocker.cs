namespace ThreeL.Blob.Infra.Redis
{
    public interface IDistributedLocker
    {
        Task<(bool Success, string LockValue)> LockAsync(string cacheKey, int timeoutSeconds = 5, bool autoDelay = false);
        Task<bool> SafedUnLockAsync(string cacheKey, string cacheValue);
    }
}
