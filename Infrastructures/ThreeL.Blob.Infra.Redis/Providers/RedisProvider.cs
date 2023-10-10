using StackExchange.Redis;
using System.Text.Json;
using ThreeL.Blob.Infra.Core.Serializers;

namespace ThreeL.Blob.Infra.Redis.Providers
{
    public class RedisProvider : IRedisProvider, IDistributedLocker
    {
        private readonly IEnumerable<IServer> _servers;
        private readonly IDatabase _redisDb;
        private readonly JsonSerializerOptions _jsonOptions = SystemTextJsonSerializer.GetDefaultOptions();

        public RedisProvider(DefaultDatabaseProvider dbProvider)
        {
            this._redisDb = dbProvider.GetDatabase();
            this._servers = dbProvider.GetServerList();
        }

        public async Task<bool> HExistsAsync(string cacheKey, string field)
        {
            return await _redisDb.HashExistsAsync(cacheKey, field);
        }

        public async Task<bool> HSetAsync<T>(string cacheKey, string cacheValue, T value, TimeSpan? expiration = null, When when = When.Always)
        {
            var data = JsonSerializer.Serialize(value, _jsonOptions);
            await _redisDb.HashSetAsync(cacheKey, cacheValue, data);
            if (expiration != null)
            {
                await _redisDb.KeyExpireAsync(cacheKey, expiration);
            }

            return true;
        }

        public async Task<T> HGetAsync<T>(string cacheKey, string field)
        {
            var val = await _redisDb.HashGetAsync(cacheKey, field);
            if (val.HasValue)
            {
                return JsonSerializer.Deserialize<T>(val, _jsonOptions);
            }
            else
            {
                return default;
            }
        }

        public async Task<bool> KeyDelAsync(string key)
        {
            return await _redisDb.KeyDeleteAsync(key);
        }

        public async Task<bool> KeyExistsAsync(string cacheKey)
        {
            var flag = await _redisDb.KeyExistsAsync(cacheKey);
            return flag;
        }

        public async Task<string> StringGetAsync(string cacheKey)
        {
            return await _redisDb.StringGetAsync(cacheKey);
        }

        public async Task<bool> StringSetAsync(string cacheKey, string cacheValue, TimeSpan? expiration = null, When when = When.Always)
        {
            bool flag = await _redisDb.StringSetAsync(cacheKey, cacheValue, expiration, when);
            return flag;
        }

        public async Task<Dictionary<string, T>> HGetAllAsync<T>(string cacheKey)
        {
            var dict = new Dictionary<string, T>();
            var vals = await _redisDb.HashGetAllAsync(cacheKey);

            foreach (var item in vals)
            {
                if (!dict.ContainsKey(item.Name))
                    dict.Add(item.Name, JsonSerializer.Deserialize<T>(item.Value, _jsonOptions));
            }

            return dict;
        }

        public async Task SetAddAsync(string cacheKey, string[] cacheValues, TimeSpan? expiration = null)
        {
            var list = new List<RedisValue>();

            foreach (var item in cacheValues)
            {
                list.Add(item);
            }

            await _redisDb.SetAddAsync(cacheKey, list.ToArray());

            if (expiration.HasValue)
            {
                _redisDb.KeyExpire(cacheKey, expiration.Value);
            }
        }

        public async Task<bool> SetIsMemberAsync(string cacheKey, string cacheValue)
        {
            return await _redisDb.SetContainsAsync(cacheKey, cacheValue);
        }

        public async Task<List<string>> SetGetAsync(string cacheKey)
        {
            var values = await _redisDb.SetMembersAsync(cacheKey);
            List<string> ids = new List<string>();
            foreach (var item in values)
            {
                ids.Add(item.ToString());
            }

            return ids;
        }

        #region locker
        public async Task<(bool Success, string LockValue)> LockAsync(string cacheKey, int timeoutSeconds = 5, bool autoDelay = false)
        {
            var lockKey = GetLockKey(cacheKey);
            var lockValue = Guid.NewGuid().ToString();
            var timeoutMilliseconds = timeoutSeconds * 1000;
            var expiration = TimeSpan.FromMilliseconds(timeoutMilliseconds);
            bool flag = await _redisDb.StringSetAsync(lockKey, lockValue, expiration, When.NotExists);
            if (flag && autoDelay)
            {
                var refreshMilliseconds = (int)(timeoutMilliseconds / 2.0);
                var autoDelayTimer = new Timer(timerState => Delay(_redisDb, lockKey, lockValue, timeoutMilliseconds), null, refreshMilliseconds, refreshMilliseconds);
                var addResult = AutoDelayTimers.Instance.TryAdd(lockKey, autoDelayTimer);
                if (!addResult)
                {
                    autoDelayTimer?.Dispose();
                    await SafedUnLockAsync(cacheKey, lockValue);
                    return (false, string.Empty);
                }
            }
            return (flag, flag ? lockValue : string.Empty);
        }

        public async Task<bool> SafedUnLockAsync(string cacheKey, string lockValue)
        {
            var lockKey = GetLockKey(cacheKey);
            AutoDelayTimers.Instance.CloseTimer(lockKey);

            var script = @"local invalue = @value
                                    local currvalue = redis.call('get',@key)
                                    if(invalue==currvalue) then redis.call('del',@key)
                                        return 1
                                    else
                                        return 0
                                    end";
            var parameters = new { key = lockKey, value = lockValue };
            var prepared = LuaScript.Prepare(script);
            var result = (int)await _redisDb.ScriptEvaluateAsync(prepared, parameters);

            return result == 1;
        }

        public string GetLockKey(string cacheKey)
        {
            return $"ThreeL:locker:{cacheKey}";
        }

        private void Delay(IDatabase redisDb, string key, string value, int milliseconds)
        {
            if (!AutoDelayTimers.Instance.ContainsKey(key))
                return;

            var script = @"local val = redis.call('GET', @key)
                                    if val==@value then
                                        redis.call('PEXPIRE', @key, @milliseconds)
                                        return 1
                                    end
                                    return 0";
            object parameters = new { key, value, milliseconds };
            var prepared = LuaScript.Prepare(script);
            var result = redisDb.ScriptEvaluateAsync(prepared, parameters, CommandFlags.None).GetAwaiter().GetResult();
            if ((int)result == 0)
            {
                AutoDelayTimers.Instance.CloseTimer(key);
            }
            return;
        }
        #endregion
    }
}
