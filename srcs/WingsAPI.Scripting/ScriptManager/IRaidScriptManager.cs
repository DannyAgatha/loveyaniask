using WingsAPI.Scripting.Enum.Raid;
using WingsAPI.Scripting.Object.Raid;

namespace WingsAPI.Scripting.ScriptManager
{
    public interface IRaidScriptManager
    {
        SRaid GetScriptedRaid(SRaidType raidType);
        void Load();
    }
}