using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Mapster;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using PhoenixLib.Logging;
using Plugin.MongoLogs.Entities;

namespace Plugin.MongoLogs.Utils
{
    internal static class MongoDatabaseHelper
    {
        static MongoDatabaseHelper()
        {
            BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));
        }

        public static async Task InsertLogAsync<T>(this IMongoDatabase database, T log)
        {
            IMongoCollection<BsonDocument> collection = database.GetCollection<BsonDocument>(MongoHelper<T>.CollectionName);

            if (MongoHelper<T>.EntityType == null)
            {
                await collection.InsertOneAsync(log.ToBsonDocument());
                return;
            }

            var document = log.Adapt(typeof(T), MongoHelper<T>.EntityType).ToBsonDocument();
            document.Remove("_t");
            await collection.InsertOneAsync(document);
        }

        private static class MongoHelper<T>
        {
            static MongoHelper()
            {
                Type entityType = typeof(MongoHelper<T>).Assembly.GetTypes()
                    .FirstOrDefault(s => typeof(IPlayerLogEntity).IsAssignableFrom(s) && s.GetCustomAttribute<EntityForAttribute>()?.MessageType == typeof(T));

                CollectionName = "NOT_HANDLED_PLZ_DEVS_WTF";
                if (entityType == null)
                {
                    Log.Warn($"{typeof(T)} has no mappable entity");
                    return;
                }

                EntityType = entityType;

                string collectionNameAttribute = entityType.GetCustomAttribute<CollectionNameAttribute>()?.CollectionName;
                if (collectionNameAttribute == null)
                {
                    Log.Warn($"{typeof(T)} has no defined collection");
                    return;
                }

                CollectionName = collectionNameAttribute;
            }

            internal static string CollectionName { get; }
            internal static Type EntityType { get; }
        }
    }
}
