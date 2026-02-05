using System.Linq;
using System.Threading.Tasks;
using WingsEmu.DTOs.Maps;
using WingsEmu.DTOs.Quests;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Quests;
using WingsEmu.Game.Quests.Event;

namespace Plugin.QuestImpl.RunScriptHandlers
{
    public class TeleportRunScriptHandler : IRunScriptHandler
    {
        private readonly IGameLanguageService _gameLanguageService;
        private readonly IMapManager _mapManager;
        private readonly IQuestManager _questManager;

        private readonly QuestTeleportDialogConfiguration _questTeleportDialogConfiguration;

        public TeleportRunScriptHandler(QuestTeleportDialogConfiguration questTeleportDialogConfiguration, IQuestManager questManager, IGameLanguageService gameLanguageService, IMapManager mapManager)
        {
            _questTeleportDialogConfiguration = questTeleportDialogConfiguration;
            _questManager = questManager;
            _gameLanguageService = gameLanguageService;
            _mapManager = mapManager;
        }

        public int[] RunIds => new[] { 200, 201, 202, 203, 204 };

        public async Task ExecuteAsync(IClientSession session, RunScriptEvent e)
        {
            QuestTeleportDialogInfo questTeleport = _questTeleportDialogConfiguration.FirstOrDefault(s => s.RunId == e.RunId);
            if (questTeleport == null)
            {
                return;
            }

            TutorialDto scriptWithTp = _questManager.GetScriptsTutorialByType(TutorialActionType.RUN).FirstOrDefault(s => s.Data == e.RunId);
            if (scriptWithTp == null)
            {
                return;
            }

            TutorialDto completedScript = _questManager.GetScriptTutorialById(scriptWithTp.Id - 1);
            if (!session.PlayerEntity.HasCompletedScriptByIndex(completedScript.ScriptId, completedScript.ScriptIndex))
            {
                return;
            }

            if (!session.CurrentMapInstance.HasMapFlag(MapFlags.IS_BASE_MAP))
            {
                return;
            }

            if (session.CantPerformActionOnAct4())
            {
                return;
            }

            if (questTeleport.AskForTeleport)
            {
                session.SendDialog($"guri 1000 {questTeleport.MapId} {questTeleport.PositionX} {questTeleport.PositionY}", "guri 9999",
                    _gameLanguageService.GetLanguage(GameDialogKey.QUEST_DIALOG_TELEPORT_TO_OBJECTIVE, session.UserLanguage));
                return;
            }

            IMapInstance mapInstance = _mapManager.GetBaseMapInstanceByMapId(questTeleport.MapId);
            session.ChangeMap(mapInstance.Id, questTeleport.PositionX, questTeleport.PositionY);
        }
    }
}