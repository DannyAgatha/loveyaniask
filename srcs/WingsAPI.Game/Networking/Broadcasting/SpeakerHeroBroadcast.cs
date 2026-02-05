namespace WingsEmu.Game.Networking.Broadcasting;

public class SpeakerHeroBroadcast : IBroadcastRule
{
    public bool Match(IClientSession session) => !session.PlayerEntity.HeroChatBlocked;
}