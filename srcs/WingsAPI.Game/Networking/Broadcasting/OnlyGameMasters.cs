using WingsEmu.DTOs.Account;

namespace WingsEmu.Game.Networking.Broadcasting;

public class OnlyGameMasters : IBroadcastRule
{
    public bool Match(IClientSession session) => session.PlayerEntity.Authority >= AuthorityType.GM && session.GmMode;
}