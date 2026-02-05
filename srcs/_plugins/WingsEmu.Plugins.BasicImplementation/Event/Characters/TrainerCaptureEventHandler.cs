using System.Collections.Generic;
using PhoenixLib.Events;
using System.Threading;
using System.Threading.Tasks;
using WingsAPI.Packets.Enums.Chat;
using WingsEmu.Game;
using WingsEmu.Game._enum;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Entities.Extensions;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Managers.StaticData;
using WingsEmu.Game.Mates;
using WingsEmu.Game.Mates.Events;
using WingsEmu.Game.Monster.Event;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Npcs;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;

namespace NosEmu.Plugins.BasicImplementations.Event.Characters;

public class TrainerCaptureEventHandler : IAsyncEventProcessor<TrainerCaptureEvent>
{
    private readonly IRandomGenerator _randomGenerator;
    private readonly IGameLanguageService _gameLanguage;
    private readonly IAsyncEventPipeline _asyncEventPipeline;
    private readonly INpcMonsterManager _npcMonsterManager;
    private readonly IMateEntityFactory _mateEntityFactory;

    public TrainerCaptureEventHandler(IRandomGenerator randomGenerator, IGameLanguageService gameLanguage,
        IAsyncEventPipeline asyncEventPipeline, INpcMonsterManager npcMonsterManager, IMateEntityFactory mateEntityFactory)
    {
        _randomGenerator = randomGenerator;
        _gameLanguage = gameLanguage;
        _asyncEventPipeline = asyncEventPipeline;
        _npcMonsterManager = npcMonsterManager;
        _mateEntityFactory = mateEntityFactory;
    }

    public async Task HandleAsync(TrainerCaptureEvent e, CancellationToken cancellation)
    {
        IMonsterEntity monsterEntityToCapture = e.Target;
        IClientSession session = e.Sender;
        SkillInfo skill = e.Skill;
        bool isSkill = e.IsSkill;

        if (_randomGenerator.RandomNumber() > e.CaptureRate)
        {
            session.SendMsgi(MessageType.Default, Game18NConstString.TamingFailed);
            if (!isSkill)
            {
                return;
            }

            e.Sender.CurrentMapInstance.Broadcast(session.PlayerEntity.GenerateSuCapturePacket(monsterEntityToCapture, skill, true));
            return;
        }

        session.BroadcastEffect(EffectType.CatchSuccess);
        int monsterVnum = monsterEntityToCapture.MonsterVNum;
        int level = monsterEntityToCapture.Level - 15 < 1 ? 1 : monsterEntityToCapture.Level - 15;
        IMateEntity currentMateEntity = session.PlayerEntity.MateComponent.GetTeamMember(m => m.MateType == MateType.Pet);

        if (isSkill)
        {
            session.CurrentMapInstance.Broadcast(session.PlayerEntity.GenerateSuCapturePacket(monsterEntityToCapture, skill, false));
        }

        monsterEntityToCapture.MapInstance.Broadcast(monsterEntityToCapture.GenerateOut());
        await _asyncEventPipeline.ProcessEventAsync(new MonsterDeathEvent(monsterEntityToCapture));

        var mateNpc = new MonsterData(_npcMonsterManager.GetNpc(monsterVnum));
        IMateEntity newMate = _mateEntityFactory.CreateMateEntity(session.PlayerEntity, mateNpc, MateType.Pet, (byte)level, monsterEntityToCapture.HeroLevel, new List<int>());
        newMate.Stars = monsterEntityToCapture.Stars;

        await session.EmitEventAsync(new MateInitializeEvent
        {
            MateEntity = newMate
        });

        if (currentMateEntity == null)
        {
            await session.EmitEventAsync(new MateJoinTeamEvent
            {
                MateEntity = newMate,
                IsNewCreated = true
            });
        }
        else
        {
            await session.EmitEventAsync(new MateLeaveTeamEvent { MateEntity = newMate });
        }

        session.SendMsgi(MessageType.Default, Game18NConstString.TamingSuccessful);
    }
}