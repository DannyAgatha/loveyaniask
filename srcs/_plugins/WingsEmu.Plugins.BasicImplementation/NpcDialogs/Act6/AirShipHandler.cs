using System.Threading.Tasks;
using WingsEmu.Game._i18n;
using WingsEmu.Game._NpcDialog;
using WingsEmu.Game._NpcDialog.Event;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;

namespace NosEmu.Plugins.BasicImplementations.NpcDialogs.Act6
{
    public class AirShipHandler : INpcDialogAsyncHandler
    {
        private readonly IGameLanguageService _gameLanguage;
        private readonly IMapManager _mapManager;

        public AirShipHandler(IMapManager mapManager, IGameLanguageService gameLanguage)
        {
            _mapManager = mapManager;
            _gameLanguage = gameLanguage;
        }

        public NpcRunType[] NpcRunTypes => new[] { NpcRunType.ACT62_ENTER_TO_SHIP, NpcRunType.ACT62_ENTER_TO_SHIP_2, NpcRunType.ACT62_BACK_TO_CYLLOAN };

        public async Task Execute(IClientSession session, NpcDialogEvent e)
        {
            INpcEntity npcEntity = session.CurrentMapInstance.GetNpcById(e.NpcId);
            if (npcEntity == null)
            {
                return;
            }

            if (session.CurrentMapInstance.MapInstanceType != MapInstanceType.BaseMapInstance)
            {
                return;
            }

            if (session.PlayerEntity.HeroLevel == 0)
            {
                session.SendInformationChatMessage(_gameLanguage.GetLanguage(GameDialogKey.INFORMATION_CHATMESSAGE_NOT_REQUIERED_LEVEL, session.UserLanguage));
                return;
            }

            switch (e.NpcRunType)
            {
                case NpcRunType.ACT62_ENTER_TO_SHIP:
                case NpcRunType.ACT62_BACK_TO_CYLLOAN:
                    session.ChangeMap(2526, 27, 37);
                    break;
                case NpcRunType.ACT62_ENTER_TO_SHIP_2:
                    session.ChangeMap(2527, 27, 37);
                    break;
            }
        }
    }
}