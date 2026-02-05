using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsAPI.Game.Extensions.Quests;
using WingsEmu.DTOs.Quests;
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
using WingsEmu.Game.Quests;
using WingsEmu.Game.Quests.Event;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;

namespace NosEmu.Plugins.BasicImplementations.Event.Characters;

public class MonsterCaptureEventHandler : IAsyncEventProcessor<MonsterCaptureEvent>
{
    private const int CAPTURE_RATE = 50;
    private readonly IAsyncEventPipeline _asyncEventPipeline;
    private readonly IGameLanguageService _gameLanguage;
    private readonly IMateEntityFactory _mateEntityFactory;
    private readonly INpcMonsterManager _npcMonsterManager;
    private readonly IQuestManager _questManager;
    private readonly IRandomGenerator _randomGenerator;

    public MonsterCaptureEventHandler(IGameLanguageService gameLanguage, IRandomGenerator randomGenerator,
        INpcMonsterManager npcMonsterManager, IMateEntityFactory mateEntityFactory, IAsyncEventPipeline asyncEventPipeline, IQuestManager questManager)
    {
        _gameLanguage = gameLanguage;
        _randomGenerator = randomGenerator;
        _npcMonsterManager = npcMonsterManager;
        _mateEntityFactory = mateEntityFactory;
        _asyncEventPipeline = asyncEventPipeline;
        _questManager = questManager;
    }

    public async Task HandleAsync(MonsterCaptureEvent e, CancellationToken cancellation)
    {
        IMonsterEntity monsterEntityToCapture = e.Target;
        IClientSession session = e.Sender;
        SkillInfo skill = e.Skill;
        bool isSkill = e.IsSkill;

        int captureRate = session.PlayerEntity.Level < 20 ? 90 : CAPTURE_RATE;

        if (_randomGenerator.RandomNumber() > captureRate)
        {
            session.SendMsg(_gameLanguage.GetLanguage(GameDialogKey.SKILL_SHOUTMESSAGE_CAPTURE_FAILED, session.UserLanguage), MsgMessageType.Middle);
            if (!isSkill)
            {
                return;
            }

            e.Sender.CurrentMapInstance.Broadcast(session.PlayerEntity.GenerateSuCapturePacket(monsterEntityToCapture, skill, true));
            return;
        }

        session.BroadcastEffect(EffectType.CatchSuccess);
        if (session.PlayerEntity.HasQuestWithQuestType(QuestType.CAPTURE_AND_KEEP) || session.PlayerEntity.HasQuestWithQuestType(QuestType.CAPTURE_WITHOUT_KEEPING))
        {
            await HandleQuestCapture(session, monsterEntityToCapture, isSkill, skill);
        }
        else
        {
            await HandleNormalCapture(session, monsterEntityToCapture, isSkill, skill);
        }
    }

    private async Task HandleNormalCapture(IClientSession session, IMonsterEntity monsterEntityToCapture, bool isSkill, SkillInfo skill)
    {
        int monsterVnum = monsterEntityToCapture.MonsterVNum;
        byte level = monsterEntityToCapture.Level;
        IMateEntity currentMateEntity = session.PlayerEntity.MateComponent.GetTeamMember(m => m.MateType == MateType.Pet);

        if (isSkill)
        {
            session.CurrentMapInstance.Broadcast(session.PlayerEntity.GenerateSuCapturePacket(monsterEntityToCapture, skill, false));
        }

        monsterEntityToCapture.MapInstance.Broadcast(monsterEntityToCapture.GenerateOut());
        await _asyncEventPipeline.ProcessEventAsync(new MonsterDeathEvent(monsterEntityToCapture));

        var mateNpc = new MonsterData(_npcMonsterManager.GetNpc(monsterVnum));
        IMateEntity newMate = _mateEntityFactory.CreateMateEntity(session.PlayerEntity, mateNpc, MateType.Pet, level, monsterEntityToCapture.HeroLevel, new List<int>());
        
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

        session.SendMsg(_gameLanguage.GetLanguageFormat(GameDialogKey.SKILL_SHOUTMESSAGE_CAUGHT_PET, session.UserLanguage,
            _gameLanguage.GetLanguage(GameDataType.NpcMonster, mateNpc.Name, session.UserLanguage)), MsgMessageType.Middle);
    }

    private async Task HandleQuestCapture(IClientSession session, IMonsterEntity monsterEntityToCapture, bool isSkill, SkillInfo skill)
    {
        IEnumerable<CharacterQuest> characterQuests = session.PlayerEntity.GetCurrentQuests()
            .Where(s => s.Quest.QuestType == QuestType.CAPTURE_AND_KEEP || s.Quest.QuestType == QuestType.CAPTURE_WITHOUT_KEEPING);

        foreach (CharacterQuest characterQuest in characterQuests)
        {
            IEnumerable<QuestObjectiveDto> objectives = characterQuest.Quest.Objectives;
            foreach (QuestObjectiveDto objective in objectives)
            {
                if (monsterEntityToCapture.MonsterVNum != objective.Data0)
                {
                    continue;
                }

                CharacterQuestObjectiveDto questObjectiveDto = characterQuest.ObjectiveAmount[objective.ObjectiveIndex];
                switch (characterQuest.Quest.QuestType)
                {
                    case QuestType.CAPTURE_WITHOUT_KEEPING:
                        monsterEntityToCapture.MapInstance?.DespawnMonster(monsterEntityToCapture);

                        if (isSkill)
                        {
                            session.CurrentMapInstance.Broadcast(session.PlayerEntity.GenerateSuCapturePacket(monsterEntityToCapture, skill, false));
                        }

                        if (questObjectiveDto.CurrentAmount >= questObjectiveDto.RequiredAmount)
                        {
                            continue;
                        }

                        questObjectiveDto.CurrentAmount++;
                        await session.EmitEventAsync(new QuestObjectiveUpdatedEvent
                        {
                            CharacterQuest = characterQuest
                        });

                        session.RefreshQuestProgress(_questManager, characterQuest.QuestId);
                        if (session.PlayerEntity.IsQuestCompleted(characterQuest))
                        {
                            await session.EmitEventAsync(new QuestCompletedEvent(characterQuest));
                        }

                        break;

                    case QuestType.CAPTURE_AND_KEEP:
                        await HandleNormalCapture(session, monsterEntityToCapture, isSkill, skill);

                        if (questObjectiveDto.CurrentAmount >= questObjectiveDto.RequiredAmount)
                        {
                            continue;
                        }

                        questObjectiveDto.CurrentAmount++;
                        await session.EmitEventAsync(new QuestObjectiveUpdatedEvent
                        {
                            CharacterQuest = characterQuest
                        });

                        session.RefreshQuestProgress(_questManager, characterQuest.QuestId);
                        if (session.PlayerEntity.IsQuestCompleted(characterQuest))
                        {
                            await session.EmitEventAsync(new QuestCompletedEvent(characterQuest));
                        }

                        break;
                }

                // If it has already been captured, it ends the loop
                return;
            }
        }

        await HandleNormalCapture(session, monsterEntityToCapture, isSkill, skill);
    }
}