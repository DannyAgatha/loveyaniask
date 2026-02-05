using System.Collections.Generic;
using WingsEmu.Game.Maps;

namespace NosEmu.Plugins.BasicImplementations.Managers;

public interface IMapAttributeFactory
{
    IEnumerable<IMapAttribute> CreateMapAttributes(Dictionary<string, object> attributesKeyValue);
}