using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.Families.Event;

public class FamilySendInviteEvent : PlayerEvent
{
    public FamilySendInviteEvent(string receiverNickname) => ReceiverNickname = receiverNickname;

    public string ReceiverNickname { get; }
}