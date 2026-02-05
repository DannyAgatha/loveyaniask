namespace WingsEmu.Game.Buffs;

public static class StaticBuffFactory
{
    public static IBuffFactory Instance { get; private set; }

    public static void Initialize(IBuffFactory factory)
    {
        Instance = factory;
    }
}