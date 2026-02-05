using WingsEmu.Game.Battle;

namespace WingsEmu.Game.Buffs;

public static class StaticMeditationManager
{
    public static IMeditationManager Instance { get; private set; }

    public static void Initialize(IMeditationManager meditationManager)
    {
        Instance = meditationManager;
    }
}