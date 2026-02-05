using System.Threading.Tasks;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Networking;

namespace WingsEmu.Game.Miniland;

public interface IMinilandManager
{
    IMapInstance CreateMinilandByCharacterSession(IClientSession session);

    IMapInstance GetMinilandByCharacterId(long characterId);

    IClientSession GetSessionByMiniland(IMapInstance mapInstance);

    void SaveMinilandInvite(long senderId, long targetId);

    bool ContainsMinilandInvite(long senderId);

    bool ContainsTargetInvite(long senderId, long targetId);

    void RemoveMinilandInvite(long senderId, long targetId);

    int GetMinilandMaximumCapacity(long characterId);

    void RelativeUpdateMinilandCapacity(long characterId, int valueToAdd);
    Task IncreaseMinilandVisitCounter(long characterId);
    Task<bool> CanRefreshDailyVisitCounter(long characterId);
    Task<int> GetMinilandVisitCounter(long characterId);

    Configurations.Miniland.Miniland GetMinilandConfiguration(IMapInstance mapInstance);

    void RemoveMiniland(long characterId);
}