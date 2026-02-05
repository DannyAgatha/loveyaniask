namespace WingsEmu.Game.Miniland;

public class StaticMinilandManager
{
    public static IMinilandManager Instance { get; private set; }

    public static void Initialize(IMinilandManager manager)
    {
        Instance = manager;
    }
}