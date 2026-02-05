namespace WingsEmu.Game.Managers;

public class StaticScriptedInstanceManager
{
    public static IScriptedInstanceManager Instance { get; private set; }

    public static void Initialize(IScriptedInstanceManager manager)
    {
        Instance = manager;
    }
}

public interface IScriptedInstanceManager
{
}