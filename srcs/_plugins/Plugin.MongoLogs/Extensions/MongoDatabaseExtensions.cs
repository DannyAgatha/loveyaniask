using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MongoDB.Bson;
using Newtonsoft.Json;
using Plugin.MongoLogs.Entities;
using Plugin.MongoLogs.Entities.Player;
using Plugin.MongoLogs.Utils;

namespace Plugin.MongoLogs.Extensions
{
    public static class MongoDatabaseExtensions
    {
        public static string ToValidJson(this List<BsonDocument> collection)
        {
            if (!collection.Any())
            {
                return string.Empty;
            }

            foreach (BsonDocument bson in collection.Where(bson => bson.Contains("_id")))
            {
                bson.Remove("_id");
            }

            return JsonConvert.SerializeObject(collection.ConvertAll(BsonTypeMapper.MapToDotNetValue));
        }

        public static Dictionary<string, (string, string)> GetCollectionNames()
        {
            Dictionary<string, (string, string)> res = new();

            // todo rework this shit, it's completely dumb
            var collectionNamesFields = typeof(CollectionNames).GetFields(BindingFlags.Public | BindingFlags.Default | BindingFlags.Static).ToList();
            var displayNamesFields = typeof(DisplayCollectionNames).GetFields(BindingFlags.Public | BindingFlags.Default | BindingFlags.Static).ToList();
            foreach (Type entityType in typeof(IPlayerLogEntity).Assembly.GetTypes().Where(x => typeof(IPlayerLogEntity).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract))
            {
                CollectionNameAttribute attribute = entityType.GetCustomAttribute<CollectionNameAttribute>();
                if (attribute == null)
                {
                    continue;
                }

                if (entityType == typeof(PlayerDisconnectedLogEntity))
                {
                }

                FieldInfo field = collectionNamesFields.Find(x => x.GetValue(null).ToString() == attribute.CollectionName);

                if (field == null)
                {
                    continue;
                }

                // this is the completely dumb part, comparing...
                if (!displayNamesFields.Exists(x => string.Equals(x.Name, field.Name, StringComparison.InvariantCultureIgnoreCase)))
                {
                    continue;
                }

                res[field.Name] = (attribute.CollectionName, attribute.DisplayName);
            }

            return res;
        }
    }
}