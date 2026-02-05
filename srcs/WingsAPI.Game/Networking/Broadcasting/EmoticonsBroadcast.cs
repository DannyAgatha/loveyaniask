namespace WingsEmu.Game.Networking.Broadcasting;

public class EmoticonsBroadcast : IBroadcastRule
{
    public bool Match(IClientSession session) => !session.PlayerEntity.EmoticonsBlocked;
}