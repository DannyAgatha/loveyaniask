using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.Game._enum;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Battle.Managers;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Npcs.Event;
using WingsEmu.Game.Raids.Events;
using WingsEmu.Game.TimeSpaces;
using WingsEmu.Game.TimeSpaces.Enums;
using WingsEmu.Game.TimeSpaces.Events;
using WingsEmu.Game.Triggers;

namespace NosEmu.Plugins.BasicImplementations.Event.Npcs;

public class MapNpcGenerateDeathEventHandler : IAsyncEventProcessor<MapNpcGenerateDeathEvent>
{
    private readonly IAsyncEventPipeline _asyncEventPipeline;
    private readonly IPhantomPositionManager _phantomPositionManager;
    private readonly ITimeSpaceManager _timeSpaceManager;
    private readonly IBuffFactory _buffFactory;

    public MapNpcGenerateDeathEventHandler(ITimeSpaceManager timeSpaceManager, IAsyncEventPipeline asyncEventPipeline, IPhantomPositionManager phantomPositionManager, IBuffFactory buffFactory)
    {
        _timeSpaceManager = timeSpaceManager;
        _asyncEventPipeline = asyncEventPipeline;
        _phantomPositionManager = phantomPositionManager;
        _buffFactory = buffFactory;
    }

    public async Task HandleAsync(MapNpcGenerateDeathEvent e, CancellationToken cancellation)
    {
        INpcEntity npc = e.NpcEntity;
        DateTime currentTime = DateTime.UtcNow;
        npc.IsStillAlive = false;
        npc.Hp = 0;
        npc.Mp = 0;
        npc.Death = currentTime;
        await npc.RemoveAllBuffsAsync(true);
        npc.Target = null;
        npc.Killer = e.Killer;

        if (npc.IsPhantom())
        {
            _phantomPositionManager.AddPosition(npc.UniqueId, npc.Position);
        }

        await npc.TriggerEvents(BattleTriggers.OnDeath);
        if (npc.MonsterVNum is (int)MonsterVnum.FERNON_LEFT_BLADE or (int)MonsterVnum.FERNON_RIGHT_BLADE && npc.CurrentCollection == 0)
        {
            BuffVnums buff = npc.MonsterVNum == (int)MonsterVnum.FERNON_LEFT_BLADE ? BuffVnums.LIGHT_ENERGY_OF_KREM : BuffVnums.SHADOW_ENERGY_OF_KREM;
            switch (buff)
            {
                case BuffVnums.LIGHT_ENERGY_OF_KREM:
                    IReadOnlyList<IPlayerEntity> characters = npc.MapInstance?.GetAliveCharacters();
                    foreach (IPlayerEntity character in characters)
                    {
                        await character.AddBuffAsync(_buffFactory.CreateBuff((int)buff, character));
                    }
                    break;
                case BuffVnums.SHADOW_ENERGY_OF_KREM:
                    await _asyncEventPipeline.ProcessEventAsync(new RaidAddBossBuffEvent((short)buff, npc.MapInstance));
                    break;
            }
            return;
        }
        
        if (npc.MapInstance.MapInstanceType != MapInstanceType.TimeSpaceInstance)
        {
            return;
        }

        if (!npc.IsProtected)
        {
            return;
        }

        TimeSpaceParty timeSpace = _timeSpaceManager.GetTimeSpaceByMapInstanceId(npc.MapInstance.Id);
        if (timeSpace == null)
        {
            return;
        }

        timeSpace.Instance.KilledProtectedNpcs++;

        if (!timeSpace.Instance.TimeSpaceObjective.ProtectNPC)
        {
            return;
        }

        await _asyncEventPipeline.ProcessEventAsync(new TimeSpaceInstanceFinishEvent(timeSpace, TimeSpaceFinishType.NPC_DIED));
    }
}