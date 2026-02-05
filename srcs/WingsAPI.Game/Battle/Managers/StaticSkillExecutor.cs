namespace WingsEmu.Game.Battle;

public static class StaticSkillExecutor
{
    public static ISkillExecutor Instance { get; private set; }

    public static void Initialize(ISkillExecutor skillExecutor)
    {
        if (Instance != null)
        {
            return;
        }

        Instance = skillExecutor;
    }
}