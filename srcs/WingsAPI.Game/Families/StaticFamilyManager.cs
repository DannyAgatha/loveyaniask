namespace WingsEmu.Game.Families;

public static class StaticFamilyManager
{
    public static IFamilyManager Instance { get; private set; }

    public static void Initialize(IFamilyManager generator)
    {
        Instance = generator;
    }
}