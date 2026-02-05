using System.Threading.Tasks;
using WingsEmu.DTOs.Maps;
using WingsEmu.Game._i18n;
using WingsEmu.Game._NpcDialog;
using WingsEmu.Game._NpcDialog.Event;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Ship.Configuration;
using WingsEmu.Game.Ship.Event;
using WingsEmu.Packets.Enums;

namespace NosEmu.Plugins.BasicImplementations.NpcDialogs.Act4_Act5;

public class Act4EnterShipHandler : INpcDialogAsyncHandler
{
    private readonly IGameLanguageService _languageService;

    public Act4EnterShipHandler(IGameLanguageService languageService) => _languageService = languageService;

    public NpcRunType[] NpcRunTypes => new[] { NpcRunType.ACT4_ENTER_SHIP };

    public async Task Execute(IClientSession session, NpcDialogEvent e)
    {
        INpcEntity npcEntity = session.CurrentMapInstance.GetNpcById(e.NpcId);
        if (npcEntity == null)
        {
            return;
        }

        if (!session.CurrentMapInstance.HasMapFlag(MapFlags.IS_BASE_MAP))
        {
            return;
        }

        if (session.PlayerEntity.IsInRaidParty)
        {
            return;
        }

        if (session.PlayerEntity.TimeSpaceComponent.IsInTimeSpaceParty)
        {
            return;
        }

        if (session.PlayerEntity.Faction == FactionType.Neutral)
        {
            session.SendErrorChatMessage(_languageService.GetLanguage(GameDialogKey.ACT4_NEED_FACTION, session.UserLanguage));
            return;
        }

        ShipType shipType = session.PlayerEntity.Faction == FactionType.Angel ? ShipType.Act4Angels : ShipType.Act4Demons;

        await e.Sender.EmitEventAsync(new ShipEnterEvent(shipType));
    }
}