using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using PhoenixLib.Logging;
using PhoenixLib.Scheduler;
using WingsAPI.Scripting.Object.Timespace;
using WingsEmu.DTOs.BCards;
using WingsEmu.Game._enum;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Act4;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Battle.Managers;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Configurations.Prestige;
using WingsEmu.Game.Configurations.WorldBoss;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.InterChannel;
using WingsEmu.Game.Items;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Mates;
using WingsEmu.Game.Monster.Event;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Quests.Event;
using WingsEmu.Game.Raids.Events;
using WingsEmu.Game.RainbowBattle.Event;
using WingsEmu.Game.Skills;
using WingsEmu.Game.TimeSpaces;
using WingsEmu.Game.TimeSpaces.Events;
using WingsEmu.Game.Triggers;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;

namespace NosEmu.Plugins.BasicImplementations.Event.Monster;

public class MonsterDeathEventHandler : IAsyncEventProcessor<MonsterDeathEvent>
{
    private readonly IAct4FlagManager _act4FlagManager;
    private readonly IAsyncEventPipeline _asyncEventPipeline;
    private readonly IBCardEffectHandlerContainer _bCardEffectHandlerContainer;
    private readonly IPhantomPositionManager _phantomPositionManager;
    private readonly ITimeSpaceManager _timeSpaceManager;
    private readonly IGameItemInstanceFactory _gameItemInstanceFactory;
    private readonly ISpyOutManager _spyOutManager;
    private readonly RainbowBattleConfiguration _rainbowBattleConfiguration;
    private readonly ISessionManager _sessionManager;
    private readonly IGameLanguageService _languageService;
    private readonly IBuffFactory _buffFactory;
    private readonly PrestigeConfiguration _prestigeConfiguration;
    private readonly IScheduler _scheduler;
    public MonsterDeathEventHandler(IBCardEffectHandlerContainer bCardEffectHandlerContainer, IAsyncEventPipeline asyncEventPipeline,
        ITimeSpaceManager timeSpaceManager, IAct4FlagManager act4FlagManager, IPhantomPositionManager phantomPositionManager, IGameItemInstanceFactory gameItemInstanceFactory, 
        ISpyOutManager spyOutManager, RainbowBattleConfiguration rainbowBattleConfiguration, ISessionManager sessionManager, IGameLanguageService languageService, IBuffFactory buffFactory, PrestigeConfiguration prestigeConfiguration, IScheduler scheduler)
    {
        _bCardEffectHandlerContainer = bCardEffectHandlerContainer;
        _asyncEventPipeline = asyncEventPipeline;
        _timeSpaceManager = timeSpaceManager;
        _act4FlagManager = act4FlagManager;
        _phantomPositionManager = phantomPositionManager;
        _gameItemInstanceFactory = gameItemInstanceFactory;
        _spyOutManager = spyOutManager;
        _rainbowBattleConfiguration = rainbowBattleConfiguration;
        _sessionManager = sessionManager;
        _languageService = languageService;
        _buffFactory = buffFactory;
        _prestigeConfiguration = prestigeConfiguration;
        _scheduler = scheduler;
    }

    public async Task HandleAsync(MonsterDeathEvent e, CancellationToken cancellation)
    {
        IMonsterEntity monsterEntity = e.MonsterEntity;

        DateTime dateTimeNow = DateTime.UtcNow;
        monsterEntity.IsStillAlive = false;
        monsterEntity.Hp = 0;
        monsterEntity.Death = dateTimeNow;
        monsterEntity.Killer = e.Killer;

        if (monsterEntity.SummonerType is not VisualType.Player)
        {
            monsterEntity.MapInstance.IncreaseMonsterDeathsOnMap();
        }

        monsterEntity.MapInstance.DeactivateMode(monsterEntity);

        if (monsterEntity.DeathEffect != 0)
        {
            monsterEntity.BroadcastEffectInRange(monsterEntity.DeathEffect);
        }

        if (monsterEntity.IsPhantom())
        {
            _phantomPositionManager.AddPosition(monsterEntity.UniqueId, monsterEntity.Position);
        }

        await monsterEntity.TriggerEvents(BattleTriggers.OnDeath);
        if (!e.IsByCommand)
        {
            IEnumerable<BCardDTO> triggerBCard = monsterEntity.BCards.Where(b => b.TriggerType == BCardNpcMonsterTriggerType.ON_DEATH);
            foreach (BCardDTO bCard in triggerBCard)
            {
                _bCardEffectHandlerContainer.Execute(monsterEntity, monsterEntity, bCard, triggerType: BCardNpcMonsterTriggerType.ON_DEATH);
            }
        }

        if (monsterEntity.MapInstance.MapInstanceType == MapInstanceType.TimeSpaceInstance)
        {
            await CheckTargetMonstersInRoom(monsterEntity);
            await _asyncEventPipeline.ProcessEventAsync(new TimeSpaceCheckMonsterEvent(monsterEntity), cancellation);
        }

        monsterEntity.Mp = 0;

        switch (e.Killer)
        {
            case IPlayerEntity player:
                await player.Session.EmitEventAsync(new KillBonusEvent
                {
                    MonsterEntity = monsterEntity,
                    IsByButcherCommand = e.IsByCommand
                });
                
                (long targetId, VisualType targetType) = _spyOutManager.GetSpyOutTarget(player.Id);

                if (targetType == VisualType.Monster && targetId == monsterEntity.Id)
                {
                    _spyOutManager.RemoveSpyOutSkill(player.Id);
                    player.Session.SendObArPacket();
                }
                
                if (player.IsInGroup())
                {
                    foreach (IPlayerEntity member in player.GetGroup().Members.ToArray())
                    {
                        await member.Session.EmitEventAsync(new QuestMonsterDeathEvent { MonsterEntity = monsterEntity });
                    }
                }
                else
                {
                    await player.Session.EmitEventAsync(new QuestMonsterDeathEvent { MonsterEntity = monsterEntity });
                }

                player.HitsByMonsters.TryRemove(monsterEntity.Id, out _);
                break;
            case IMateEntity mate:
                await mate.Owner.Session.EmitEventAsync(new KillBonusEvent
                {
                    MonsterEntity = monsterEntity,
                    IsByButcherCommand = e.IsByCommand
                });

                if (mate.Owner.IsInGroup())
                {
                    foreach (IPlayerEntity member in mate.Owner.GetGroup().Members.ToArray())
                    {
                        await member.Session.EmitEventAsync(new QuestMonsterDeathEvent { MonsterEntity = monsterEntity });
                    }
                }
                else
                {
                    await mate.Owner.Session.EmitEventAsync(new QuestMonsterDeathEvent { MonsterEntity = monsterEntity });
                }

                break;
            case IMonsterEntity mapMonster:
                if (!mapMonster.SummonerId.HasValue)
                {
                    break;
                }

                if (mapMonster.IsMateTrainer)
                {
                    break;
                }
                
                if (mapMonster.IsSparringMonster)
                {
                    break;
                }

                if (mapMonster.SummonerType != VisualType.Player)
                {
                    break;
                }

                IClientSession summoner = mapMonster.MapInstance.GetCharacterById(mapMonster.SummonerId.Value)?.Session;
                if (summoner == null)
                {
                    break;
                }

                if (monsterEntity.MonsterVNum == (short)MonsterVnum.BOMB)
                {
                    summoner.PlayerEntity.SkillComponent.BombEntityId = null;
                }

                await summoner.EmitEventAsync(new KillBonusEvent
                {
                    MonsterEntity = monsterEntity,
                    IsByButcherCommand = e.IsByCommand
                });

                if (summoner.PlayerEntity.IsInGroup())
                {
                    foreach (IPlayerEntity member in summoner.PlayerEntity.GetGroup().Members.ToArray())
                    {
                        await member.Session.EmitEventAsync(new QuestMonsterDeathEvent { MonsterEntity = monsterEntity });
                    }
                }
                else
                {
                    await summoner.EmitEventAsync(new QuestMonsterDeathEvent { MonsterEntity = monsterEntity });
                }

                break;
        }

        switch ((MonsterVnum)monsterEntity.MonsterVNum)
        {
            case MonsterVnum.LAURENA_CALIGOR when monsterEntity.MapInstance is { MapInstanceType: MapInstanceType.RaidInstance}:
                await _asyncEventPipeline.ProcessEventAsync(new RaidRemoveBossBuffEvent((short)BuffVnums.DEVILS_BLESSING, monsterEntity.MapInstance));
                break;
            case MonsterVnum.BELIAL_SHADOW:
            case MonsterVnum.TWISTED_BEAST_KING_SHADOW:
            case MonsterVnum.TWISTED_SPIRIT_KING_SHADOW when monsterEntity.MapInstance is { MapInstanceType: MapInstanceType.RaidInstance}:
                await _asyncEventPipeline.ProcessEventAsync(new RaidRemoveBossBuffEvent((short)BuffVnums.SHADOW_SHIELD, monsterEntity.MapInstance));
                break;
            case MonsterVnum.DEMON_CAMP:
                _act4FlagManager.RemoveDemonFlag();
                break;
            case MonsterVnum.ANGEL_CAMP:
                _act4FlagManager.RemoveAngelFlag();
                break;
        }

        await monsterEntity.RemoveAllBuffsAsync(true);
        monsterEntity.MapInstance.ForgetAll(monsterEntity, dateTimeNow);
    }

    private async Task CheckTargetMonstersInRoom(IMonsterEntity monsterEntity)
    {
        if (!monsterEntity.IsTarget)
        {
            return;
        }

        Guid guid = monsterEntity.MapInstance.Id;
        TimeSpaceParty timeSpace = _timeSpaceManager.GetTimeSpaceByMapInstanceId(guid);
        if (timeSpace == null)
        {
            return;
        }

        if (timeSpace.Instance.TimeSpaceObjective.KillMonsterVnum.HasValue && monsterEntity.MonsterVNum == timeSpace.Instance.TimeSpaceObjective.KillMonsterVnum.Value)
        {
            timeSpace.Instance.TimeSpaceObjective.KilledMonsterAmount++;
            await _asyncEventPipeline.ProcessEventAsync(new TimeSpaceRefreshObjectiveProgressEvent
            {
                MapInstanceId = guid
            });
        }

        if (monsterEntity.MapInstance.GetAliveMonsters(x => x.IsTarget).Any())
        {
            return;
        }

        TimeSpaceSubInstance timeSpaceSubInstance = _timeSpaceManager.GetSubInstance(guid);
        if (timeSpaceSubInstance == null)
        {
            return;
        }

        await timeSpaceSubInstance.TriggerEvents(TimespaceConstEventKeys.OnAllTargetMobsDead);
    }
}