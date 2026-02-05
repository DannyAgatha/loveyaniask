using WingsEmu.Game._packetHandling;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Networking;

namespace WingsEmu.Game.Quests.Event;
public class QuestEnemyDeathEvent : PlayerEvent
{
    public IClientSession KillerSession { get; set; }
    public IPlayerEntity KilledPlayer { get; set; }
}
