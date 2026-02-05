using System;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.Core.Extensions;
using WingsEmu.DTOs.Maps;
using WingsEmu.Game;
using WingsEmu.Game._enum;
using WingsEmu.Game.Battle.Managers;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Entities.Event;
using WingsEmu.Game.Helpers.Damages;
using WingsEmu.Game.Managers.StaticData;
using WingsEmu.Game.Monster.Event;
using WingsEmu.Game.Npcs;
using WingsEmu.Game.Npcs.Event;
using WingsEmu.Game.Triggers;
using WingsEmu.Packets.Enums;

namespace NosEmu.Plugins.BasicImplementations.Event.Npcs;

public class NpcSummonEventHandler : IAsyncEventProcessor<NpcSummonEvent>
{
    private readonly INpcEntityFactory _npcEntityFactory;
    private readonly INpcMonsterManager _npcMonsterManager;
    private readonly IPhantomPositionManager _phantomPositionManager;

    public NpcSummonEventHandler(INpcEntityFactory npcEntityFactory, INpcMonsterManager npcMonsterManager, IPhantomPositionManager phantomPositionManager)
    {
        _npcEntityFactory = npcEntityFactory;
        _npcMonsterManager = npcMonsterManager;
        _phantomPositionManager = phantomPositionManager;
    }

    public async Task HandleAsync(NpcSummonEvent e, CancellationToken cancellation)
    {
        foreach (ToSummon summon in e.Npcs)
        {
            var npcMonster = new MonsterData(_npcMonsterManager.GetNpc(summon.VNum));
            INpcEntity mapNpc = _npcEntityFactory.CreateNpc(npcMonster, e.Map, npcAdditionalData: new NpcAdditionalData
            {
                CanMove = summon.IsMoving,
                CanAttack = summon.IsHostile,
                IsHostile = summon.IsHostile,
                NpcDirection = summon.Direction,
                NpcShouldRespawn = false,
                FactionType = summon.FactionType ?? (e.Map.HasMapFlag(MapFlags.ACT_4) ? FactionType.Angel : FactionType.Neutral)
            });

            mapNpc.Target = summon.Target;

            if (summon.TriggerEvents != null)
            {
                foreach ((string key, IAsyncEvent asyncEvent, bool removeOnUse) in summon.TriggerEvents)
                {
                    mapNpc.AddEvent(key, asyncEvent, removeOnUse);
                }
            }

            Position? phantomPosition = e.MonsterId.HasValue ? _phantomPositionManager.GetPosition(e.MonsterId.Value) : null;
            Position spawnCell = phantomPosition ?? summon.SpawnCell ?? e.Map.GetRandomPosition();

            await mapNpc.EmitEventAsync(new MapJoinNpcEntityEvent(mapNpc, spawnCell.X, spawnCell.Y));

            CheckBeriosPhantom(mapNpc);

            if (!summon.RemoveTick)
            {
                continue;
            }

            mapNpc.NextTick = DateTime.UtcNow.AddSeconds(-1000);
            mapNpc.NextAttackReady = DateTime.UtcNow.AddSeconds(-1000);
        }
    }

    private void CheckBeriosPhantom(INpcEntity mapNpc)
    {
        switch ((MonsterVnum)mapNpc.MonsterVNum)
        {
            case MonsterVnum.EMERALD_SHADOW_PHANTOM:

                mapNpc.AddEvent(BattleTriggers.OnDeath, new MonsterSummonEvent(mapNpc.MapInstance, Lists.Create(new ToSummon
                {
                    VNum = (short)MonsterVnum.EMERALD_PHANTOM,
                    SpawnCell = mapNpc.Position,
                    IsMoving = true,
                    IsHostile = true
                }))
                {
                    NpcId = mapNpc.UniqueId
                }, true);

                break;

            case MonsterVnum.SAPPHIRE_SHADOW_PHANTOM:

                mapNpc.AddEvent(BattleTriggers.OnDeath, new MonsterSummonEvent(mapNpc.MapInstance, Lists.Create(new ToSummon
                {
                    VNum = (short)MonsterVnum.SAPPHIRE_PHANTOM,
                    SpawnCell = mapNpc.Position,
                    IsMoving = true,
                    IsHostile = true
                }))
                {
                    NpcId = mapNpc.UniqueId
                }, true);

                break;

            case MonsterVnum.RUBY_SHADOW_PHANTOM:

                mapNpc.AddEvent(BattleTriggers.OnDeath, new MonsterSummonEvent(mapNpc.MapInstance, Lists.Create(new ToSummon
                {
                    VNum = (short)MonsterVnum.RUBY_PHANTOM,
                    SpawnCell = mapNpc.Position,
                    IsMoving = true,
                    IsHostile = true
                }))
                {
                    NpcId = mapNpc.UniqueId
                }, true);

                break;
        }
    }
}