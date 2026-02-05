using System;

namespace Plugin.MongoLogs.Utils
{
    [AttributeUsage(AttributeTargets.Class)]
    public class EntityForAttribute : Attribute
    {
        public EntityForAttribute(Type messageType) => MessageType = messageType;

        public Type MessageType { get; }
    }
}