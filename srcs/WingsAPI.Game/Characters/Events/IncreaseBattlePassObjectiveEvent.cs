using WingsAPI.Packets.Enums.BattlePass;
using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.Characters.Events;

public class IncreaseBattlePassObjectiveEvent : PlayerEvent
{
    public IncreaseBattlePassObjectiveEvent(MissionType missionType, long amountToIncrease = 1, long firstData = 0)
    {
        MissionType = missionType;
        AmountToIncrease = amountToIncrease;
        FirstData = firstData;
    }

    public MissionType MissionType { get; set; }

    public long AmountToIncrease { get; set; }
    
    public long FirstData { get; set; }
}