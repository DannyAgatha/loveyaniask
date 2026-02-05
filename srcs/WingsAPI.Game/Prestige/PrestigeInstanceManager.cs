using System.Collections.Generic;
using WingsEmu.Game.Maps;

namespace WingsEmu.Game.Prestige;

public static class PrestigeInstanceManager
{
    public static readonly Dictionary<IMapInstance, PrestigeInstance> PrestigeInstances = new();
}
