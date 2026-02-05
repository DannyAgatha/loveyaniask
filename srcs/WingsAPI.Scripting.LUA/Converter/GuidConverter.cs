using System;
using MoonSharp.Interpreter;

namespace WingsAPI.Scripting.LUA.Converter
{
    public class GuidConverter : Converter<Guid>
    {
        public override DataType DataType { get; } = DataType.String;

        protected override DynValue ToLuaObject(Script script, Guid value) => DynValue.NewString(value.ToString());

        protected override Guid ToCSharpObject(DynValue value) => Guid.Parse(value.ToObject<string>());
    }
}