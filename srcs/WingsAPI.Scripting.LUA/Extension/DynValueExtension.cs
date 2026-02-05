using System.Collections.Generic;
using MoonSharp.Interpreter;

namespace WingsAPI.Scripting.LUA.Extension
{
    public static class DynValueExtension
    {
        public static T GetValue<T>(this Table table, string key) => table.Get(key).ToObject<T>();

        public static IEnumerable<T> GetValues<T>(this Table table, string key) => table.Get(key).ToObject<IEnumerable<T>>();
    }
}