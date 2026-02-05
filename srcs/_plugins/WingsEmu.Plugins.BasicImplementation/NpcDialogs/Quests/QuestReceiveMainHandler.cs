using System.Linq;
using System.Threading.Tasks;
using Serilog;
using WingsAPI.Game.Extensions.Quests;
using WingsEmu.DTOs.Quests;
using WingsEmu.Game._i18n;
using WingsEmu.Game._NpcDialog;
using WingsEmu.Game._NpcDialog.Event;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Quests;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;

namespace NosEmu.Plugins.BasicImplementations.NpcDialogs.Quests;

public class QuestReceiveMainHandler : INpcDialogAsyncHandler
{
    private readonly IGameLanguageService _gameLanguage;
    private readonly IQuestManager _questManager;

    public QuestReceiveMainHandler(IGameLanguageService gameLanguage, IQuestManager questManager)
    {
        _gameLanguage = gameLanguage;
        _questManager = questManager;
    }

    public NpcRunType[] NpcRunTypes => new[] { NpcRunType.QUEST_RECEIVE_MAIN };

    public async Task Execute(IClientSession session, NpcDialogEvent e)
    {
        INpcEntity npcEntity = session.CurrentMapInstance.GetNpcById(e.NpcId);
        if (npcEntity == null)
        {
            return;
        }

        if (e.Confirmation == 1)
        {
            QuestNpcDto questNpc = _questManager.GetQuestNpcByScriptId(e.Argument);
            if (questNpc == null)
            {
                Log.Debug($"A QuestNpc with StartingScriptId {e.Argument} was not found.");
                return;
            }

            if (session.PlayerEntity.GetCurrentQuests().Any(s => s.SlotType == QuestSlotType.MAIN))
            {
                session.SendMsg(_gameLanguage.GetLanguage(GameDialogKey.QUEST_SHOUTMESSAGE_ALREADY_MAIN_QUEST, session.UserLanguage), MsgMessageType.Middle);
                return;
            }

            if (session.PlayerEntity.Level < questNpc.Level)
            {
                session.SendMsg(_gameLanguage.GetLanguage(GameDialogKey.QUEST_SHOUTMESSAGE_LOW_LEVEL, session.UserLanguage), MsgMessageType.Middle);
                return;
            }

            if (session.PlayerEntity.GetLastCompletedScript().ScriptId < questNpc.RequiredCompletedScript)
            {
                session.SendMsg(_gameLanguage.GetLanguage(GameDialogKey.QUEST_SHOUTMESSAGE_INCOMPLETE_QUESTS, session.UserLanguage), MsgMessageType.Middle);
                return;
            }

            if (session.PlayerEntity.GetLastCompletedScript().ScriptId > questNpc.RequiredCompletedScript)
            {
                session.SendMsg(_gameLanguage.GetLanguage(GameDialogKey.QUEST_SHOUTMESSAGE_ALREADY_COMPLETED_MAIN, session.UserLanguage), MsgMessageType.Middle);
                return;
            }


            session.SendScriptPacket(e.Argument, 10);
        }

        else
        {
            session.SendQnaPacket($"n_run {(short)NpcRunType.QUEST_RECEIVE_MAIN} {e.Argument} {(byte)e.VisualType} {e.NpcId} 1",
                _gameLanguage.GetLanguage(GameDialogKey.QUEST_DIALOG_START_MAIN_QUEST, session.UserLanguage));
        }
    }
}