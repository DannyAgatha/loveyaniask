using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Loaders;
using PhoenixLib.Logging;
using WingsAPI.Scripting.Attribute;
using WingsAPI.Scripting.LUA.Converter;
using WingsAPI.Scripting.LUA.Converter.Object.Common.Map;
using WingsAPI.Scripting.LUA.Extension;

namespace WingsAPI.Scripting.LUA
{
    public class LuaScriptFactory : IScriptFactory
    {
        /// <summary>
        ///     Specific converter
        /// </summary>
        private static readonly IConverter[] SpecialConverters =
        {
            new SEventConverter(),
            new GuidConverter(),
            new SMapObjectConverter()
        };

        private readonly ScriptFactoryConfiguration _configuration;

        public LuaScriptFactory(ScriptFactoryConfiguration configuration)
        {
            _configuration = configuration;

            RegisterAllScriptingObjectsInAssembly(typeof(IScriptFactory).Assembly);

            foreach (IConverter converter in SpecialConverters)
            {
                RegisterConverter(converter);
            }
        }

        public void RegisterAllScriptingObjectsInAssembly(Assembly assembly)
        {
            IEnumerable<Type> scriptObjects = assembly.GetTypesWithAttribute<ScriptObjectAttribute>();
            foreach (Type scriptObject in scriptObjects)
            {
                RegisterType(scriptObject);
            }
        }

        public void RegisterType<T>()
        {
            RegisterType(typeof(T));
        }

        public T LoadScript<T>(string name)
        {
            if (!File.Exists(name))
            {
                throw new IOException($"Couldn't find file {name}");
            }

            var script = new Script
            {
                Options =
                {
                    DebugPrint = Log.Debug,
                    ScriptLoader = new FileSystemScriptLoader
                    {
                        ModulePaths = new[]
                        {
                            $"{_configuration.LibDirectory}/?.lua",
                            $"{_configuration.LibDirectory}/enum/?.lua"
                        }
                    }
                }
            };

            DynValue scriptReturn = script.DoFile(name);
            return scriptReturn.ToObject<T>();
        }

        public void RegisterConverter(IConverter converter)
        {
            Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(converter.DataType, converter.ObjectType, converter.ToCSharpObject);
            Script.GlobalOptions.CustomConverters.SetClrToScriptCustomConversion(converter.ObjectType, converter.ToLuaObject);
        }

        private void RegisterType(Type scriptObject)
        {
            Log.Debug($"[LUA_SCRIPT_FACTORY] {scriptObject.Name} registered");

            UserData.RegisterType(scriptObject);

            Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(DataType.Table, scriptObject, value => ObjectFactory.ToCSharpObject(scriptObject, value));
            Script.GlobalOptions.CustomConverters.SetClrToScriptCustomConversion(scriptObject, ObjectFactory.ToLuaObject);
        }
    }
}