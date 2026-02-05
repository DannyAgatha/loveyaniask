using WingsAPI.Scripting.Enum.Dungeon;
using WingsAPI.Scripting.Object.Dungeon;

namespace WingsAPI.Scripting.ScriptManager
{
    public interface IDungeonScriptManager
    {
        SDungeon GetScriptedDungeon(SDungeonType raidType);
        void Load();
    }
}