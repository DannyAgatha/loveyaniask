using WingsEmu.Game._packetHandling;
using WingsEmu.Game.Entities;

namespace WingsEmu.Game.Alzanor.Events;

public class AlzanorRefreshScoreEvent : PlayerEvent, IBattleEntityEvent
{
    public AlzanorParty AlzanorParty { get; init; }
    public IBattleEntity Entity { get; }
}