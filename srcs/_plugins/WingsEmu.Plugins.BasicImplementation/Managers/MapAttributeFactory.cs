using System;
using System.Collections.Generic;
using WingsEmu.Game.Maps;

namespace NosEmu.Plugins.BasicImplementations.Managers;

public class MapAttributeFactory : IMapAttributeFactory
{
    private readonly Dictionary<string, Type> _mapAttributesTypes = new();

    public IEnumerable<IMapAttribute> CreateMapAttributes(Dictionary<string, object> attributesKeyValue) => throw new NotImplementedException();
}