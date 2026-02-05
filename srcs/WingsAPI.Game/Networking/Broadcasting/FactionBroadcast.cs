using WingsEmu.Game.Extensions;
using WingsEmu.Packets.Enums;

namespace WingsEmu.Game.Networking.Broadcasting;

public class FactionBroadcast : IBroadcastRule
{
    private readonly FactionType _faction;
    private readonly bool _gmAffected;

    public FactionBroadcast(FactionType faction, bool gmAffected = false)
    {
        _faction = faction;
        _gmAffected = gmAffected;
    }

    public bool Match(IClientSession session)
    {
        if (session == null)
        {
            return false;
        }

        if (_gmAffected || !session.IsGameMaster())
        {
            return session.PlayerEntity.Faction == _faction;
        }

        return true;
    }
}