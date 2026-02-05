using PhoenixLib.Events;
using PhoenixLib.Scheduler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsEmu.DTOs.Maps;
using WingsEmu.Game;
using WingsEmu.Game._enum;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Core.SkillHandling;
using WingsEmu.Game.Core.SkillHandling.Event;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Monster.Event;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;

namespace NosEmu.Plugins.BasicImplementations.Skills;

public class SpawnTrainerSpecialistSkillHandler : ISkillHandler
{
    private readonly ITrainerSpecialistConfiguration _trainerSpecialistConfiguration;
    private readonly IAsyncEventPipeline _eventPipeline;
    private readonly IScheduler _scheduler;
    private readonly IRandomGenerator _randomGenerator;

    public SpawnTrainerSpecialistSkillHandler(ITrainerSpecialistConfiguration trainerSpecialistConfiguration, IAsyncEventPipeline eventPipeline,
        IScheduler scheduler, IRandomGenerator randomGenerator)
    {
        _trainerSpecialistConfiguration = trainerSpecialistConfiguration;
        _eventPipeline = eventPipeline;
        _scheduler = scheduler;
        _randomGenerator = randomGenerator;
    }

    public long[] SkillId => new long[] { 1788, 1792, 1794 };

    public async Task ExecuteAsync(IClientSession session, SkillEvent e)
    {
        if (!session.PlayerEntity.MapInstance.HasMapFlag(MapFlags.IS_MINILAND_MAP))
        {
            e.SkillInfo.IsCanceled = true;
            return;
        }
        
        if (session.PlayerEntity.Miniland.Id != session.CurrentMapInstance.Id)
        {
            e.SkillInfo.IsCanceled = true;
            session.SendSayi(ChatMessageColorType.Yellow, Game18NConstString.OnlyForYourOwnMiniland, 0, 0);
            return;
        }

        if (!session.PlayerEntity.HasItem((int)ItemVnums.SPARRING_MONSTER_CAGE))
        {
            e.SkillInfo.IsCanceled = true;
            session.SendSayi(ChatMessageColorType.Yellow, Game18NConstString.FollowingItemRequired, 2, (int)ItemVnums.SPARRING_MONSTER_CAGE);
            return;
        }

        IReadOnlyList<int> monsters = _trainerSpecialistConfiguration.GetMonstersBySkillVnum(e.SkillId);

        if (monsters == null || monsters.Count == 0)
        {
            e.SkillInfo.IsCanceled = true;
            return;
        }

        IReadOnlyList<IMonsterEntity> sparringMonsterCount = session.CurrentMapInstance.GetAliveMonsters(x => x != null && x.IsAlive() &&
            x.SummonerId == session.PlayerEntity.Id && x.SummonerType == VisualType.Player && x.IsSparringMonster);
        int? maxMonsterPerMap = _trainerSpecialistConfiguration.GetMaxMonsterPerMap(e.SkillId);

        if (sparringMonsterCount.Count >= maxMonsterPerMap)
        {
            foreach (IMonsterEntity monster in sparringMonsterCount)
            {
                if (monsters.Contains(monster.MonsterVNum))
                {
                    await _eventPipeline.ProcessEventAsync(new MonsterDeathEvent(monster));
                }
            }
        }

        List<ToSummon> summons = new()
        {
            new ToSummon
            {
                VNum = monsters[_randomGenerator.RandomNumber(0, monsters.Count)],
                SpawnCell = session.PlayerEntity.Position,
                IsMoving = true,
                IsHostile = false,
                IsSparringMonster = true
            }
        };

        _scheduler.Schedule(TimeSpan.FromMilliseconds(e.SkillInfo.CastTime * 100), async s =>
        {
            await _eventPipeline.ProcessEventAsync(new MonsterSummonEvent(session.PlayerEntity.MapInstance, summons, session.PlayerEntity));
        });

        await session.RemoveItemFromInventory((int)ItemVnums.SPARRING_MONSTER_CAGE);
    }
}