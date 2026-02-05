using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.Characters.Events;

public class InviteJoinMinilandEvent : PlayerEvent
{
    public InviteJoinMinilandEvent(string target, bool isFirstStep, bool isByFriend = false)
    {
        Target = target;
        IsFirstStep = isFirstStep;
        IsByFriend = isByFriend;
    }

    public string Target { get; }
    public bool IsFirstStep { get; }
    public bool IsByFriend { get; }
}