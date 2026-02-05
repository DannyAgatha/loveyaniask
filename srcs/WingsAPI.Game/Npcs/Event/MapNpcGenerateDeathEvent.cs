using PhoenixLib.Events;
using WingsEmu.Game.Entities;

namespace WingsEmu.Game.Npcs.Event;

public class MapNpcGenerateDeathEvent : IAsyncEvent
{
    public MapNpcGenerateDeathEvent(INpcEntity npcEntity, IBattleEntity killer)
    {
        NpcEntity = npcEntity;
        Killer = killer;
    }

    public INpcEntity NpcEntity { get; }

    public IBattleEntity Killer { get; }
}