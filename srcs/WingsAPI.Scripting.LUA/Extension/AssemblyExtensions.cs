using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace WingsAPI.Scripting.LUA.Extension
{
    public static class AssemblyExtensions
    {
        public static IEnumerable<Type> GetTypesWithAttribute<T>(this Assembly assembly) where T : System.Attribute
        {
            return assembly.GetTypes().Where(x => x.GetCustomAttribute<T>() != null);
        }
    }
}