using System.Collections.Generic;
using System.Linq;
using WingsEmu.DTOs.Quests;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Quests;

namespace WingsAPI.Game.Extensions.Quests
{
    public static class QuestScriptExtensions
    {
        public static string GenerateNextQuestScriptPacket(this IClientSession session, CharacterQuest quest, IQuestManager questManager)
        {
            IReadOnlyCollection<TutorialDto> scripts = questManager.GetScriptsTutorialByType(TutorialActionType.WAIT_FOR_REWARDS_CLAIM);

            TutorialDto actualScript = scripts.FirstOrDefault(s => s.Data == quest.QuestId);
            if (actualScript == null)
            {
                return string.Empty;
            }

            TutorialDto nextScript = questManager.GetScriptTutorialById(actualScript.Id + 1);
            if (nextScript == null || actualScript.ScriptId != nextScript.ScriptId) // End of a script ID. Gotta pick quest from NPC
            {
                return string.Empty;
            }

            return $"script {nextScript.ScriptId} {nextScript.ScriptIndex}";
        }

        public static string GenerateScriptPacket(this IClientSession session, int scriptId, int index) => $"script {scriptId.ToString()} {index.ToString()}";

        public static void SendNextQuestScriptPacket(this IClientSession session, CharacterQuest quest, IQuestManager questManager) =>
            session.SendPacket(session.GenerateNextQuestScriptPacket(quest, questManager));

        public static void SendScriptPacket(this IClientSession session, int scriptId, int index) => session.SendPacket(session.GenerateScriptPacket(scriptId, index));
    }
}