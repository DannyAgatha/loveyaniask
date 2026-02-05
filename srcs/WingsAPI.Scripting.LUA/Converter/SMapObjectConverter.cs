using System;
using MoonSharp.Interpreter;
using WingsAPI.Scripting.Enum;
using WingsAPI.Scripting.LUA.Extension;
using WingsAPI.Scripting.Object.Common.Map;

namespace WingsAPI.Scripting.LUA.Converter.Object.Common.Map
{
    public class SMapObjectConverter : Converter<SMapObject>
    {
        protected override DynValue ToLuaObject(Script script, SMapObject value) => throw new NotImplementedException();

        protected override SMapObject ToCSharpObject(DynValue value)
        {
            Table table = value.Table;

            MapObjectType type = table.GetValue<MapObjectType>("ObjectType");
            switch (type)
            {
                case MapObjectType.Button:
                    return table.GetValue<SButton>("Parameters");
                case MapObjectType.Item:
                    return table.GetValue<SItem>("Parameters");
            }

            throw new InvalidOperationException();
        }
    }
}