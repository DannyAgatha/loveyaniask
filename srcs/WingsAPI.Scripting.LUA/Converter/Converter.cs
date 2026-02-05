using System;
using MoonSharp.Interpreter;

namespace WingsAPI.Scripting.LUA.Converter
{
    /// <summary>
    ///     Converter used to convert CSharp object to Lua object & Lua object to CSharp object
    /// </summary>
    public interface IConverter
    {
        /// <summary>
        ///     Type of lua data
        /// </summary>
        DataType DataType { get; }

        /// <summary>
        ///     Type of the object created by this converted
        /// </summary>
        Type ObjectType { get; }

        /// <summary>
        ///     Create lua object from csharp object
        /// </summary>
        /// <param name="script">Script used to create this object</param>
        /// <param name="value">Value to convert</param>
        /// <returns>Converted value</returns>
        DynValue ToLuaObject(Script script, object value);

        /// <summary>
        ///     Create csharp object from lua object
        /// </summary>
        /// <param name="value">Lua object to convert</param>
        /// <returns>Converted value</returns>
        object ToCSharpObject(DynValue value);
    }

    public abstract class Converter<T> : IConverter
    {
        /// <summary>
        ///     Empty object used to get correct properties name using nameof(Object.MyProperty)
        /// </summary>
        protected static readonly T Object = Activator.CreateInstance<T>();

        public virtual DataType DataType { get; } = DataType.Table;
        public Type ObjectType { get; } = typeof(T);

        public DynValue ToLuaObject(Script script, object value) => ToLuaObject(script, (T)value);

        object IConverter.ToCSharpObject(DynValue value) => ToCSharpObject(value);

        protected abstract DynValue ToLuaObject(Script script, T value);
        protected abstract T ToCSharpObject(DynValue value);
    }
}