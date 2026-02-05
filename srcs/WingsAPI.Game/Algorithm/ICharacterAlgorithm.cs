using WingsEmu.Game.Characters;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Character;

namespace WingsEmu.Game.Algorithm;

public class StaticCharacterAlgorithmService
{
    public static ICharacterAlgorithm Instance { get; private set; }

    public static void Initialize(ICharacterAlgorithm instance)
    {
        Instance = instance;
    }
}

public interface ICharacterAlgorithm
{
    long GetLevelXp(short level, bool isMate = false, MateType mateType = 0);
    int GetSpecialistJobXp(short level, bool isFunSpecialist = false);
    int GetHeroLevelXp(short level);
    int GetJobXp(short level, bool isAdventurer = false);
    int GetFairyXp(short level);
    int GetRegenHp(IPlayerEntity character, ClassType type, bool isSiting);
    int GetRegenMp(IPlayerEntity character, ClassType type, bool isSiting);
}