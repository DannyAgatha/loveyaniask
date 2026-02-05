using WingsEmu.Game.Extensions;

namespace WingsEmu.Game.Networking.Broadcasting;

public class NotMutedBroadcast : IBroadcastRule
{
    public bool Match(IClientSession session) => !session.IsMuted();
}