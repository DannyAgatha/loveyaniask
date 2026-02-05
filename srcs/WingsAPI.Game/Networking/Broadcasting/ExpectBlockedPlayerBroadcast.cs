namespace WingsEmu.Game.Networking.Broadcasting;

public class ExpectBlockedPlayerBroadcast : IBroadcastRule
{
    private readonly long _senderId;
    public ExpectBlockedPlayerBroadcast(long senderId) => _senderId = senderId;

    public bool Match(IClientSession session) => !session.PlayerEntity.IsBlocking(_senderId);
}