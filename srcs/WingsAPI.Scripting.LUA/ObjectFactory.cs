using System;
using System.Reflection;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Serialization.Json;
using PhoenixLib.Logging;

namespace WingsAPI.Scripting.LUA
{
    public static class ObjectFactory
    {
        public static object ToCSharpObject(Type type, DynValue value)
        {
            object obj = Activator.CreateInstance(type);
            foreach (PropertyInfo property in type.GetProperties())
            {
                try
                {
                    DynValue tableValue = value.Table.Get(property.Name);
                    object clrValue = tableValue.ToObject(property.PropertyType);
                    property.SetValue(obj, clrValue);
                }
                catch (Exception e)
                {
                    Log.Error($"[LUA_MAPPING] type: {type.Name} | {property.Name} could not be mapped: {value.Table.Get(property.Name).Table.TableToJson()}", e);
                    throw;
                }
            }

            return obj;
        }

        public static DynValue ToLuaObject(Script script, object value)
        {
            var table = new Table(script);
            foreach (PropertyInfo property in value.GetType().GetProperties())
            {
                table[property.Name] = DynValue.FromObject(script, property.GetValue(value));
            }

            return DynValue.NewTable(table);
        }
    }
}