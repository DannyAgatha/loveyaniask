using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;

namespace Plugin.MongoLogs.Extensions
{
    public class IgnoreDefaultPropertiesConvention : IMemberMapConvention
    {
        public string Name => "Ignore default properties for all classes";

        public void Apply(BsonMemberMap memberMap)
        {
            memberMap.SetIgnoreIfDefault(true);
        }
    }

    public class TypedIgnoreDefaultPropertiesConvention<T> : IMemberMapConvention
    {
        public string Name => $"Ignore Default Properties for {typeof(T)}";

        public void Apply(BsonMemberMap memberMap)
        {
            if (memberMap.ClassMap.ClassType == typeof(T))
            {
                memberMap.SetIgnoreIfDefault(true);
            }
        }
    }
}