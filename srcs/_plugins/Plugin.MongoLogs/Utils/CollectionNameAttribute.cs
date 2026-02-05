using System;

namespace Plugin.MongoLogs.Utils
{
    [AttributeUsage(AttributeTargets.Class)]
    public class CollectionNameAttribute : Attribute
    {
        public CollectionNameAttribute(string collectionName, string displayName)
        {
            CollectionName = collectionName;
            DisplayName = displayName;
        }

        public string CollectionName { get; }
        public string DisplayName { get; }
    }
}