using WingsAPI.Scripting.Enum.Dungeon;
using WingsEmu.Game.Act4;

namespace Plugin.Act4.Extension;

public static class ScriptExtensions
{
    public static SDungeonType ToSDungeonType(this DungeonType raidType) => (SDungeonType)(byte)raidType;
}