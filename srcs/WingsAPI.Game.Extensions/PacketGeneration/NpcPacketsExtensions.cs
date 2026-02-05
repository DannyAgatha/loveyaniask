using System.Collections.Generic;
using System.Linq;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Quests;

namespace WingsAPI.Game.Extensions.PacketGeneration
{
    public static class NpcPacketsExtensions
    {
        public static void SendNpcDialog(this IClientSession session, INpcEntity npcEntity) => session.SendPacket(session.GenerateNpcDialog(npcEntity));
        public static void SendNpcQuestDialog(this IClientSession session, INpcEntity npcEntity) => session.SendPacket(GenerateNpcQuestDialog(npcEntity));
        public static void SendNpcMessage(this IClientSession session, INpcEntity npcEntity, string message) => session.SendPacket(GenerateNpcMessage(npcEntity, message));

        public static string GenerateNpcDialog(this IClientSession session, INpcEntity npcEntity)
        {
            IReadOnlyCollection<CharacterQuest> specialDialogQuests =
                session.PlayerEntity.GetCurrentQuests().Where(s => s.Quest.TalkerVnum == npcEntity.NpcVNum && s.Quest.DialogDuring != -1).ToList();
            int dialogId = specialDialogQuests.Any() ? specialDialogQuests.First().Quest.DialogDuring : npcEntity.Dialog;
            return $"npc_req 2 {npcEntity.Id.ToString()} {dialogId}";
        }

        public static string GenerateNpcQuestDialog(this INpcEntity npcEntity) => $"npc_req 2 {npcEntity.Id.ToString()} {npcEntity.QuestDialog.ToString()}";
        public static string GenerateNpcMessage(this INpcEntity npcEntity, string message) => $"say 2 {npcEntity.Id} 0 {message}";
    }
}