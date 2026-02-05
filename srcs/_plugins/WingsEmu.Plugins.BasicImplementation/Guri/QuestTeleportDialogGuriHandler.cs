using System.Linq;
using System.Threading.Tasks;
using WingsEmu.DTOs.Quests;
using WingsEmu.Game._Guri;
using WingsEmu.Game._Guri.Event;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Quests;

namespace NosEmu.Plugins.BasicImplementations.Guri;

public class QuestTeleportDialogGuriHandler : IGuriHandler
{
    private readonly IMapManager _mapManager;
    private readonly IQuestManager _questManager;

    private readonly QuestTeleportDialogConfiguration _questTeleportDialogConfiguration;

    public QuestTeleportDialogGuriHandler(QuestTeleportDialogConfiguration questTeleportDialogConfiguration, IMapManager mapManager, IQuestManager questManager)
    {
        _questTeleportDialogConfiguration = questTeleportDialogConfiguration;
        _mapManager = mapManager;
        _questManager = questManager;
    }

    public long GuriEffectId => 1000;

    public async Task ExecuteAsync(IClientSession session, GuriEvent e)
    {
        if (e.Packet.Length != 6)
        {
            return;
        }

        if (!int.TryParse(e.Packet[3], out int mapId))
        {
            return;
        }

        if (!int.TryParse(e.Packet[4], out int posX))
        {
            return;
        }

        if (!int.TryParse(e.Packet[5], out int posY))
        {
            return;
        }

        QuestTeleportDialogInfo teleportDialogInfo = _questTeleportDialogConfiguration.FirstOrDefault(s => s.MapId == mapId && s.PositionX == posX && s.PositionY == posY);
        if (teleportDialogInfo == null)
        {
            return;
        }

        TutorialDto scriptWithTp = _questManager.GetScriptsTutorialByType(TutorialActionType.RUN).FirstOrDefault(s => s.Data == teleportDialogInfo.RunId);
        if (scriptWithTp == null)
        {
            return;
        }

        TutorialDto completedScript = _questManager.GetScriptTutorialById(scriptWithTp.Id - 1);
        if (!session.PlayerEntity.HasCompletedScriptByIndex(completedScript.ScriptId, completedScript.ScriptIndex))
        {
            return;
        }

        IMapInstance mapInstance = _mapManager.GetBaseMapInstanceByMapId(teleportDialogInfo.MapId);
        session.ChangeMap(mapInstance.Id, teleportDialogInfo.PositionX, teleportDialogInfo.PositionY);
    }
}