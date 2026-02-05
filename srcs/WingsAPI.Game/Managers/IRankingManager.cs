using System.Collections.Generic;
using System.Threading.Tasks;
using WingsAPI.Data.Character;
using WingsAPI.Data.Families;

namespace WingsEmu.Game.Managers;

public class StaticRankingManager
{
    public static IRankingManager Instance { get; private set; }

    public static void Initialize(IRankingManager generator)
    {
        Instance = generator;
    }
}

public interface IRankingManager
{
    IReadOnlyList<CharacterDTO> TopCompliment { get; }
    IReadOnlyList<CharacterDTO> TopPoints { get; }
    IReadOnlyList<CharacterDTO> TopReputation { get; }
    IReadOnlyList<FamilyDTO> FamilyRank { get; }
    
    long? MonthlyExpFamilyId { get; }
    long? MonthlyPvpAngelFamilyId { get; }
    long? MonthlyPvpDemonFamilyId { get; }
    long? MonthlyRainbowBattleFamilyId { get; }

    Task TryRefreshRanking();
    void RefreshRanking(IReadOnlyList<CharacterDTO> topComplimented, IReadOnlyList<CharacterDTO> topPoints, IReadOnlyList<CharacterDTO> topReputation);
}