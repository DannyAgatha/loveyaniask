using PhoenixLib.Events;
using WingsAPI.Packets.Enums.Prestige;
using WingsEmu.Game.Networking;

namespace WingsEmu.Game.Prestige;

public class PrestigeProgressEvent : IAsyncEvent
{
    public IClientSession Session { get; }
    public PrestigeTaskType TaskType { get; }
    public int? ItemVnum { get; }
    public int? MonsterVnum { get; }
    public long Amount { get; }
    public int Level { get; }

    public PrestigeProgressEvent(IClientSession session, PrestigeTaskType taskType, int? itemVnum = null, int? monsterVnum = null, long amount = 0, int level = 0)
    {
        Session = session;
        TaskType = taskType;
        ItemVnum = itemVnum;
        MonsterVnum = monsterVnum;
        Amount = amount;
        Level = level;
    }
}