using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;

namespace ThreeL.Blob.Infra.Repository.Entities.Mongo
{
    public abstract class MongoEntity : IEntity<string>
    {
        [BsonId(IdGenerator = typeof(StringObjectIdGenerator))]
        [BsonRepresentation(BsonType.ObjectId)]
        public virtual string Id { get; set; } = default!;
    }
}
