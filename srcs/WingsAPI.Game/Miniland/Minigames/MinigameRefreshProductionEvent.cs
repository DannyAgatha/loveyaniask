// NosEmu
// 


using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.Miniland.Minigames;

public class MinigameRefreshProductionEvent : PlayerEvent
{
    public MinigameRefreshProductionEvent(bool force) => Force = force;

    public bool Force { get; }
}