using StackExchange.Redis;

namespace ThreeL.Blob.Infra.Redis
{
    public interface IRedisProvider
    {
        Task<bool> KeyDelAsync(string key);
        Task<bool> KeyExistsAsync(string cacheKey);
        Task<bool> StringSetAsync(string cacheKey, string cacheValue, TimeSpan? expiration = null, When when = When.Always);
        Task<string> StringGetAsync(string cacheKey);
        Task<bool> HSetAsync<T>(string cacheKey, string cacheValue, T value, TimeSpan? expiration = null, When when = When.Always);
        Task<T> HGetAsync<T>(string cacheKey, string field);
        Task<Dictionary<string, T>> HGetAllAsync<T>(string cacheKey);
        Task<bool> HExistsAsync(string cacheKey, string field);
        Task SetAddAsync(string cacheKey, string[] cacheValues, TimeSpan? expiration = null);
        Task<List<string>> SetGetAsync(string cacheKey);
        Task<bool> SetIsMemberAsync(string cacheKey, string cacheValue);
    }
}
