// NosEmu
// 


using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace PhoenixLib.Events.Internal
{
    internal static class AssemblyExtensions
    {
        internal static bool IsSubclassOfRawGeneric(Type generic, Type toCheck)
        {
            if (generic == toCheck)
            {
                return false;
            }

            while (toCheck != null && toCheck != typeof(object))
            {
                Type cur = toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
                if (generic == cur)
                {
                    return true;
                }

                toCheck = toCheck.BaseType;
            }

            return false;
        }

        internal static Type[] GetTypesImplementingInterface(this Assembly assembly, params Type[] types)
        {
            var list = new List<Type>();
            foreach (Type type in types)
            {
                list.AddRange(assembly.GetTypesImplementingInterface(type));
            }

            return list.ToArray();
        }

        internal static Type[] GetTypesImplementingInterface<T>(this Assembly assembly) => assembly.GetTypesImplementingInterface(typeof(T));

        internal static Type[] GetTypesImplementingInterface(this Assembly assembly, Type type)
        {
            return assembly.GetTypes().Where(s => s.ImplementsInterface(type)).ToArray();
        }

        internal static bool ImplementsInterface<T>(this Type type) => type.IsAssignableFrom(typeof(T));


        internal static bool ImplementsInterface(this Type type, Type interfaceType)
        {
            if (interfaceType.IsGenericType)
            {
                return type.IsAssignableToGenericType(interfaceType);
            }

            return interfaceType.IsAssignableFrom(type);
        }

        internal static bool IsAssignableToGenericType(this Type givenType, Type genericType)
        {
            Type[] interfaceTypes = givenType.GetInterfaces();

            if (interfaceTypes.Any(it => it.IsGenericType && it.GetGenericTypeDefinition() == genericType))
            {
                return true;
            }

            if (givenType.IsGenericType && givenType.GetGenericTypeDefinition() == genericType)
            {
                return true;
            }

            Type baseType = givenType.BaseType;
            return baseType != null && IsAssignableToGenericType(baseType, genericType);
        }

        internal static Type[] GetTypesImplementingGenericClass(this Assembly assembly, params Type[] types)
        {
            var list = new List<Type>();
            foreach (Type type in types)
            {
                list.AddRange(assembly.GetTypesImplementingGenericClass(type));
            }

            return list.ToArray();
        }

        internal static Type[] GetTypesImplementingGenericClass(this Assembly assembly, Type type)
        {
            return assembly.GetTypes().Where(s => IsSubclassOfRawGeneric(type, s)).ToArray();
        }

        internal static Type[] GetTypesDerivedFrom<T>(this Assembly assembly)
        {
            return assembly.GetTypes().Where(s => s.IsSubclassOf(typeof(T))).ToArray();
        }
    }
}