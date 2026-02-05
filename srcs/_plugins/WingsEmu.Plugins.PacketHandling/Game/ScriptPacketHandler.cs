using System;
using System.Threading.Tasks;
using PhoenixLib.Logging;
using Qommon.Collections.ReadOnly;
using WingsAPI.Communication;
using WingsAPI.Communication.DbServer.AccountService;
using WingsAPI.Game.Extensions.Quests;
using WingsEmu.DTOs.Quests;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Quests;
using WingsEmu.Game.Quests.Event;
using WingsEmu.Packets.ClientPackets;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;
using static WingsEmu.Packets.Enums.AdditionalTypes;

namespace WingsEmu.Plugins.PacketHandling.Game;

public class ScriptPacketHandler : GenericGamePacketHandlerBase<ScriptPacket>
{
    private readonly IQuestManager _questManager;
    private readonly IAccountService _accountService;
    private readonly IPeriodicQuestsConfiguration _periodicQuestsConfiguration;
    private readonly INpcRunTypeQuestsConfiguration _npcRunTypeQuestsConfiguration;
    private readonly IGameLanguageService _gameLanguageService;
    public ScriptPacketHandler(IQuestManager questManager, IAccountService accountService, IPeriodicQuestsConfiguration periodicQuestsConfiguration,
        INpcRunTypeQuestsConfiguration npcRunTypeQuestsConfiguration, IGameLanguageService gameLanguageService)
    {
        _questManager = questManager;
        _accountService = accountService;
        _periodicQuestsConfiguration = periodicQuestsConfiguration;
        _npcRunTypeQuestsConfiguration = npcRunTypeQuestsConfiguration;
        _gameLanguageService = gameLanguageService;
    }

    protected override async Task HandlePacketAsync(IClientSession session, ScriptPacket packet)
    {
        short type = packet.Type;
        string data = packet.Data;

        Log.Debug($"Type: {type.ToString()} ; Data: {data}");
        int[] parameters = Array.ConvertAll(data.Split(), int.Parse);

        // Depending on the script you are sending, it follows a few formats:
        // script 0 <scriptId> <scriptSubId>              -> finishes the script and starts the next one
        // script 4 <questId> <scriptId> <scriptSubId>    -> script made for starting quests.
        TutorialDto actualScript;
        switch (type)
        {
            case 0:
                if (parameters.Length != 2)
                {
                    break;
                }

                actualScript = _questManager.GetScriptTutorialByIndex(parameters[0], parameters[1]);
                if (actualScript == null)
                {
                    break;
                }

                CompletedScriptsDto lastCompletedScript = session.PlayerEntity.GetLastCompletedScript();
                if (lastCompletedScript == null && actualScript.ScriptId != 1 && actualScript.ScriptIndex != 10) // First script
                {
                    await session.NotifyStrangeBehavior(StrangeBehaviorSeverity.SEVERE_ABUSE,
                        $"Tried to send a script different to the first one on a new character | ScriptId: {actualScript.ScriptId}, ScriptIndex: {actualScript.ScriptIndex}");
                    return;
                }

                if (lastCompletedScript != null)
                {
                    TutorialDto lastCompletedTutorial = _questManager.GetScriptTutorialByIndex(lastCompletedScript.ScriptId, lastCompletedScript.ScriptIndex);
                    if (Math.Abs(lastCompletedTutorial.Id - actualScript.Id) > 1)
                    {
                        // We check first it it's a bugged WAIT_FOR_QUEST_COMPLETION that was not sent to complete
                        TutorialDto expectedTutorial = _questManager.GetScriptTutorialById(lastCompletedTutorial.Id + 1);
                        if (expectedTutorial.Type == TutorialActionType.WAIT_FOR_QUEST_COMPLETION && session.PlayerEntity.HasCompletedQuest(expectedTutorial.Data))
                        {
                            session.PlayerEntity.SaveScript(expectedTutorial.ScriptId, expectedTutorial.ScriptIndex, expectedTutorial.Type, DateTime.UtcNow);
                            await ProcessNextScriptLogic(session, expectedTutorial);
                            return;
                        }

                        await session.NotifyStrangeBehavior(StrangeBehaviorSeverity.SEVERE_ABUSE,
                            $"Tried to send a script bigger than it should be | ExpectedId: {lastCompletedTutorial.Id + 1} | SentId: {actualScript.Id}");
                        return;
                    }

                    // Checking if it's trying to manually send a script 0 packet
                    if (actualScript.Type == TutorialActionType.WAIT_FOR_QUEST_COMPLETION && !session.PlayerEntity.IsQuestCompleted(session.PlayerEntity.GetQuestById(actualScript.Data)))
                    {
                        CharacterQuest activeQuest = session.PlayerEntity.GetQuestById(actualScript.Data);
                        if (activeQuest != null && !session.PlayerEntity.IsQuestCompleted(activeQuest) && activeQuest?.Quest.QuestType != QuestType.DIALOG &&
                            activeQuest?.Quest.QuestType != QuestType.DIALOG_2)
                        {
                            await session.NotifyStrangeBehavior(StrangeBehaviorSeverity.SEVERE_ABUSE,
                                $"Tried to send manually a script 0 for its completion | ScriptId: {actualScript.ScriptId} | ScriptIndex. {actualScript.ScriptIndex}");
                            return;
                        }
                    }
                }


                session.PlayerEntity.SaveScript(actualScript.ScriptId, actualScript.ScriptIndex, actualScript.Type, DateTime.UtcNow);
                await ProcessNextScriptLogic(session, actualScript);
                break;
            case 1:
                if (parameters.Length != 1)
                {
                    break;
                }

                await session.EmitEventAsync(new RunScriptEvent
                {
                    RunId = parameters[0]
                });
                break;
            case 4:
                if (parameters.Length != 3)
                {
                    break;
                }

                CompletedScriptsDto lastCompletedScript2 = session.PlayerEntity.GetLastCompletedScript();
                actualScript = _questManager.GetScriptTutorialByIndex(parameters[1], parameters[2]);

                if (actualScript == null)
                {
                    break;
                }

                QuestDto quest = _questManager.GetQuestById(parameters[0]);

                if (quest == null)
                {
                    break;
                }

                bool hasCompletedPeriodicQuest = await session.HasCompletedPeriodicQuest(quest, _questManager, _npcRunTypeQuestsConfiguration, _periodicQuestsConfiguration);
                if (_periodicQuestsConfiguration.IsDailyQuest(quest) && hasCompletedPeriodicQuest)
                {
                    await session.NotifyStrangeBehavior(StrangeBehaviorSeverity.SEVERE_ABUSE,
                                            $"Tried to start a completed quest multiple times | QuestId: {parameters[0]}");

                    var banRequest = new BanAccountRequest
                    {
                        AccountId = session.Account.Id,
                        Reason = "Tried to start a completed quest multiple times"
                    };

                    AccountBanSaveResponse banResponse = await _accountService.BanAccount(banRequest);

                    if (banResponse.ResponseType != RpcResponseType.SUCCESS)
                    {
                        return;
                    }

                    Log.Info($"Account ID [{session.Account.Id}] has been banned for tried to start a completed quest multiple times");
                    session.ForceDisconnect();
                    return;
                }

                if (lastCompletedScript2 != null)
                {
                    TutorialDto lastCompletedTutorial = _questManager.GetScriptTutorialByIndex(lastCompletedScript2.ScriptId, lastCompletedScript2.ScriptIndex);
                    if (Math.Abs(lastCompletedTutorial.Id - actualScript.Id) > 1)
                    {
                        TutorialDto expectedTutorial = _questManager.GetScriptTutorialById(lastCompletedTutorial.Id + 1);
                        if (expectedTutorial.Type == TutorialActionType.WAIT_FOR_QUEST_COMPLETION && session.PlayerEntity.HasCompletedQuest(expectedTutorial.Data))
                        {
                            session.PlayerEntity.SaveScript(expectedTutorial.ScriptId, expectedTutorial.ScriptIndex, expectedTutorial.Type, DateTime.UtcNow);
                            await ProcessNextScriptLogic(session, expectedTutorial);
                            return;
                        }
                        
                        await session.NotifyStrangeBehavior(StrangeBehaviorSeverity.SEVERE_ABUSE,
                            $"Tried to send a script bigger than it should be | ExpectedId: {lastCompletedTutorial.Id + 1} | SentId: {actualScript.Id}");
                        return;
                    }
                    
                    if (actualScript.Type == TutorialActionType.WAIT_FOR_QUEST_COMPLETION && !session.PlayerEntity.IsQuestCompleted(session.PlayerEntity.GetQuestById(actualScript.Data)))
                    {
                        CharacterQuest activeQuest = session.PlayerEntity.GetQuestById(actualScript.Data);
                        if (activeQuest != null && !session.PlayerEntity.IsQuestCompleted(activeQuest) && activeQuest?.Quest.QuestType != QuestType.DIALOG &&
                            activeQuest?.Quest.QuestType != QuestType.DIALOG_2)
                        {
                            await session.NotifyStrangeBehavior(StrangeBehaviorSeverity.SEVERE_ABUSE,
                                $"Tried to send manually a script 0 for its completion | ScriptId: {actualScript.ScriptId} | ScriptIndex. {actualScript.ScriptIndex}");
                            return;
                        }
                    }
                }

                await session.EmitEventAsync(new AddQuestEvent(parameters[0], QuestSlotType.MAIN));
                break;

        }
    }

    private async Task ProcessNextScriptLogic(IClientSession session, TutorialDto actualScript)
    {
        TutorialDto nextScript = _questManager.GetScriptTutorialById(actualScript.Id + 1);
        if (nextScript == null)
        {
            return;
        }

        // If it's the end of the script, the player has to take the next quest from an NPC
        if (nextScript.ScriptId != actualScript.ScriptId)
        {
            QuestNpcDto questNpc = _questManager.GetQuestNpcByScriptId(nextScript.ScriptId);
            session.SendQnpcPacket(questNpc.Level, questNpc.NpcVnum, questNpc.MapId);
            session.SendChatMessage(session.GetLanguage(GameDialogKey.QUEST_CHATMESSAGE_NEW_MISSION_FROM_NPC), ChatMessageColorType.Green);
        }
        else
        {
            session.SendScriptPacket(nextScript.ScriptId, nextScript.ScriptIndex);
        }
    }
}