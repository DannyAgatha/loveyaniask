using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.Miniland.Events;

public class MinilandIntroEvent : PlayerEvent
{
    public MinilandIntroEvent(string requestedMinilandIntro) => RequestedMinilandIntro = requestedMinilandIntro;

    public string RequestedMinilandIntro { get; }
}