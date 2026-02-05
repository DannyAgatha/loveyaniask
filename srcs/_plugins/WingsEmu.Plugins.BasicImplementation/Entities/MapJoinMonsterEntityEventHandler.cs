using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using PhoenixLib.Scheduler;
using WingsEmu.DTOs.Maps;
using WingsEmu.Game;
using WingsEmu.Game._enum;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Entities.Event;
using WingsEmu.Game.Entities.Extensions;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Helpers.Damages;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Monster.Event;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums.Chat;

namespace NosEmu.Plugins.BasicImplementations.Entities;

public class MapJoinMonsterEntityEventHandler : IAsyncEventProcessor<MapJoinMonsterEntityEvent>
{
    private readonly IAsyncEventPipeline _asyncEventPipeline;
    private readonly IRandomGenerator _randomGenerator;
    private readonly IScheduler _scheduler;
    private readonly ISessionManager _sessionManager;
    private readonly IGameLanguageService _gameLanguageService;
    public MapJoinMonsterEntityEventHandler(IRandomGenerator randomGenerator, IAsyncEventPipeline asyncEventPipeline, IScheduler scheduler,
        ISessionManager sessionManager, IGameLanguageService gameLanguageService)
    {
        _randomGenerator = randomGenerator;
        _asyncEventPipeline = asyncEventPipeline;
        _scheduler = scheduler;
        _sessionManager = sessionManager;
        _gameLanguageService = gameLanguageService;
    }

    public async Task HandleAsync(MapJoinMonsterEntityEvent e, CancellationToken cancellation)
    {
        IMonsterEntity monsterEntity = e.MonsterEntity;

        short x = e.MapX ?? monsterEntity.PositionX;
        short y = e.MapY ?? monsterEntity.PositionY;

        if (monsterEntity.MapInstance.IsBlockedZone(x, y))
        {
            Position randomPosition = monsterEntity.MapInstance.GetRandomPosition();
            x = randomPosition.X;
            y = randomPosition.Y;
        }

        monsterEntity.ChangePosition(new Position(x, y));
        monsterEntity.FirstX = x;
        monsterEntity.FirstY = y;
        monsterEntity.NextTick = DateTime.UtcNow;
        monsterEntity.NextAttackReady = DateTime.UtcNow;
        monsterEntity.OnFirstDamageReceive = true;


        monsterEntity.NextTick += TimeSpan.FromMilliseconds(_randomGenerator.RandomNumber(1000));

        monsterEntity.MapInstance.AddMonster(monsterEntity);
        
        if (monsterEntity.MonsterVNum == 2349)
        {
            _scheduler.Schedule(TimeSpan.FromSeconds(20), () =>
            {
                monsterEntity.MapInstance.DespawnMonster(monsterEntity);
                monsterEntity.MapInstance.AddMonster(monsterEntity);
            });
        }

        if (monsterEntity.MonsterVNum == (short)MonsterVnum.GLACIAL_ICE_DRAGON)
        {
            var act4Sessions = _sessionManager.Sessions.Where(session => session.CurrentMapInstance.HasMapFlag(MapFlags.ACT_4)).ToList();

            foreach (IClientSession session in act4Sessions)
            {
                session.SendChatMessage(_gameLanguageService.GetLanguageFormat(GameDialogKey.ACT4_APPEAR_GLACIAL_ICE_DRAGON, session.UserLanguage), ChatMessageColorType.LightPurple);
                session.SendMsg(_gameLanguageService.GetLanguageFormat(GameDialogKey.ACT4_APPEAR_GLACIAL_ICE_DRAGON, session.UserLanguage), MsgMessageType.Middle);
            }
        }

        monsterEntity.ModeDeathsSinceRespawn = monsterEntity.MapInstance.MonsterDeathsOnMap();

        if (monsterEntity.ModeIsHpTriggered)
        {
            monsterEntity.ModeIsActive = false;
        }
        else
        {
            monsterEntity.MapInstance.ActivateMode(monsterEntity);
        }

        if (!monsterEntity.IsStillAlive || !monsterEntity.IsAlive())
        {
            monsterEntity.IsStillAlive = true;
            monsterEntity.Hp = monsterEntity.MaxHp;
            monsterEntity.Mp = monsterEntity.MaxMp;
        }
        if (monsterEntity.MonsterVNum == (short)MonsterVnum.FERNON_GREY_BLADE || monsterEntity.MonsterVNum == (short)MonsterVnum.FERNON_BLACK_BLADE)
        {
            monsterEntity.IgnoreSkillRange = true;
        }

        monsterEntity.MapInstance.Broadcast(monsterEntity.GenerateIn(e.ShowEffect));

        await _asyncEventPipeline.ProcessEventAsync(new MonsterRespawnedEvent
        {
            Monster = monsterEntity
        });
    }
}