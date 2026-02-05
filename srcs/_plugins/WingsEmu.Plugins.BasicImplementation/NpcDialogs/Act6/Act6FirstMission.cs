using System.Threading.Tasks;
using WingsEmu.DTOs.Quests;
using WingsEmu.Game._NpcDialog;
using WingsEmu.Game._NpcDialog.Event;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Quests.Event;
using WingsEmu.Packets.Enums;

namespace NosEmu.Plugins.BasicImplementations.NpcDialogs.Act6;

public class Act6FirstMission : INpcDialogAsyncHandler
{
    public NpcRunType[] NpcRunTypes => new[] { NpcRunType.ACT61_MISSION_FIRST };

    public async Task Execute(IClientSession session, NpcDialogEvent e)
    {
        INpcEntity npcEntity = session.CurrentMapInstance.GetNpcById(e.NpcId);

        if (npcEntity == null)
        {
            return;
        }
        
        await session.EmitEventAsync(new AddQuestEvent(6040, QuestSlotType.SECONDARY));
    }
}