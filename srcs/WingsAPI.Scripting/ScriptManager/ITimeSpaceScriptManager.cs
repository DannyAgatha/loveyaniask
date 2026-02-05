using WingsAPI.Scripting.Object.Timespace;

namespace WingsAPI.Scripting.ScriptManager
{
    public interface ITimeSpaceScriptManager
    {
        void Load();
        ScriptTimeSpace GetScriptedTimeSpace(long id);
    }
}