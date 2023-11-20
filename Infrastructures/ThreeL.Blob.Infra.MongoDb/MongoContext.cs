using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System.Collections.Concurrent;
using ThreeL.Blob.Infra.MongoDb.Configuration;
using ThreeL.Blob.Infra.MongoDb.Extensions;
using ThreeL.Blob.Infra.Repository.Entities.Mongo;

namespace ThreeL.Blob.Infra.MongoDb
{
    public class MongoContext : IMongoContext, IDisposable
    {
        private readonly SemaphoreSlim _semaphore;
        private readonly IOptions<MongoOptions> _options;
        private readonly IMongoDatabase _database;
        private readonly ConcurrentBag<Type> _bootstrappedCollections = new ConcurrentBag<Type>();
        private bool _disposed;

        public MongoContext(IOptions<MongoOptions> options)
        {
            _options = options;

            var connectionString = options.Value.ConnectionString;
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException(nameof(connectionString), "Must provide a mongo connection string");
            }

            var url = new MongoUrl(connectionString);
            if (string.IsNullOrEmpty(url.DatabaseName))
            {
                throw new ArgumentNullException(nameof(connectionString), "Must provide a database name with the mongo connection string");
            }

            var clientSettings = MongoClientSettings.FromUrl(url);
            //clientSettings.ClusterConfigurator = cb => cb.Subscribe(new DiagnosticsActivityEventSubscriber());
            _semaphore = new SemaphoreSlim(1, 1);
            _database = new MongoClient(clientSettings).GetDatabase(url.DatabaseName);
        }

        public async Task<IMongoCollection<TEntity>> GetCollectionAsync<TEntity>(CancellationToken cancellationToken = default) where TEntity : MongoEntity
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(MongoContext));
            }

            var collectionName = _options.Value.GetCollectionName<TEntity>();
            var collection = _database.GetCollection<TEntity>(collectionName);

            if (_bootstrappedCollections.Contains(typeof(TEntity)))
            {
                return collection;
            }

            try
            {
                await _semaphore.WaitAsync(cancellationToken);
                _bootstrappedCollections.Add(typeof(TEntity));

                return collection;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }
            _disposed = true;

            _semaphore.Dispose();
        }
    }
}
