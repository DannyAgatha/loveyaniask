namespace WingsEmu.Game.Buffs;

public class StaticBCardEffectHandlerService
{
    public static IBCardEffectHandlerContainer Instance { get; private set; }

    public static void Initialize(IBCardEffectHandlerContainer instance)
    {
        Instance = instance;
    }
}