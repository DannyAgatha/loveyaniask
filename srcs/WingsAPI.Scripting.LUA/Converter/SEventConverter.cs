using System;
using System.Collections.Generic;
using System.Reflection;
using MoonSharp.Interpreter;
using WingsAPI.Scripting.Attribute;
using WingsAPI.Scripting.Event;
using WingsAPI.Scripting.LUA.Extension;

namespace WingsAPI.Scripting.LUA.Converter
{
    public class SEventConverter : Converter<SEvent>
    {
        private static readonly Dictionary<string, Type> TypeByEventName = new();

        static SEventConverter()
        {
            foreach (Type type in typeof(SEvent).Assembly.GetTypes())
            {
                if (!typeof(SEvent).IsAssignableFrom(type))
                {
                    continue;
                }

                if (type.IsAbstract || type.IsInterface)
                {
                    continue;
                }

                ScriptEventAttribute attribute = type.GetCustomAttribute<ScriptEventAttribute>();
                if (attribute == null)
                {
                    throw new InvalidOperationException($"Missing ScriptEvent attribute on {type.Name}");
                }

                TypeByEventName[attribute.Name] = type;
            }
        }

        protected override DynValue ToLuaObject(Script script, SEvent value) => throw new NotImplementedException();

        protected override SEvent ToCSharpObject(DynValue value)
        {
            Table table = value.Table;

            string eventName = table.GetValue<string>("Name");
            DynValue parameters = table.Get("Parameters");

            Type eventType = TypeByEventName.GetValueOrDefault(eventName);
            if (eventType == null)
            {
                throw new NotImplementedException($"Event {eventName} is not yet implemented");
            }

            return (SEvent)parameters.ToObject(eventType);
        }
    }
}