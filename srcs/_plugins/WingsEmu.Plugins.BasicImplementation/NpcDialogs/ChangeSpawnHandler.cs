using System.Threading.Tasks;
using WingsEmu.Game._i18n;
using WingsEmu.Game._NpcDialog;
using WingsEmu.Game._NpcDialog.Event;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Networking;
using WingsEmu.Game.RespawnReturn.Event;
using WingsEmu.Packets.Enums;

namespace NosEmu.Plugins.BasicImplementations.NpcDialogs;

public class ChangeSpawnHandler : INpcDialogAsyncHandler
{
    private readonly IGameLanguageService _langService;

    public ChangeSpawnHandler(IGameLanguageService langService) => _langService = langService;

    public NpcRunType[] NpcRunTypes => new[] { NpcRunType.SET_REVIVAL_SPAWN };

    public async Task Execute(IClientSession session, NpcDialogEvent e)
    {
        INpcEntity npcEntity = session.CurrentMapInstance.GetNpcById(e.NpcId);
        if (npcEntity == null)
        {
            return;
        }

        if (session.CantPerformActionOnAct4())
        {
            return;
        }

        if (e.VisualType == VisualType.Npc)
        {
            session.SendQnaPacket($"n_run^15^1^1^{npcEntity.Id}", _langService.GetLanguage(GameDialogKey.RESPAWN_DIALOG_ASK_CHANGE_SPAWN_LOCATION, session.UserLanguage));
            return;
        }

        await session.EmitEventAsync(new RespawnChangeEvent
        {
            MapId = npcEntity.MapInstance.MapId
        });
    }
}