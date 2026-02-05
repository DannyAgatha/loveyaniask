using WingsEmu.Game._packetHandling;
using WingsEmu.Packets.Enums;

namespace WingsEmu.Game.Miniland.Events;

public class MinilandStateEvent : PlayerEvent
{
    public MinilandStateEvent(MinilandState desiredMinilandState) => DesiredMinilandState = desiredMinilandState;

    public MinilandState DesiredMinilandState { get; }
}