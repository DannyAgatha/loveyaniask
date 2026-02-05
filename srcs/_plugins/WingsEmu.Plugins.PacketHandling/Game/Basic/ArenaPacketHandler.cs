using System;
using System.Threading.Tasks;
using WingsEmu.DTOs.Maps;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Arena;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.ClientPackets;
using WingsEmu.Packets.Enums.Chat;

namespace WingsEmu.Plugins.PacketHandling.Game.Basic;

public class ArenaPacketHandler : GenericGamePacketHandlerBase<ArenaPacket>
{
    private readonly IArenaManager _arenaManager;
    private readonly IGameLanguageService _gameLanguage;
    private readonly IMapManager _mapManager;

    public ArenaPacketHandler(IGameLanguageService gameLanguage, IMapManager mapManager, IArenaManager arenaManager)
    {
        _gameLanguage = gameLanguage;
        _mapManager = mapManager;
        _arenaManager = arenaManager;
    }

    protected override async Task HandlePacketAsync(IClientSession session, ArenaPacket packet)
    {
        byte arenaType = packet.ArenaType;

        if (arenaType > 1)
        {
            return;
        }

        double timeSpanSinceLastPortal = (DateTime.UtcNow - session.PlayerEntity.LastPortal).TotalSeconds;

        if (timeSpanSinceLastPortal < 4 || !session.HasCurrentMapInstance)
        {
            session.SendChatMessage(_gameLanguage.GetLanguage(GameDialogKey.PORTAL_CHATMESSAGE_TOO_EARLY, session.UserLanguage), ChatMessageColorType.Yellow);
            return;
        }

        if (session.CantPerformActionOnAct4())
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

        if (!session.PlayerEntity.IsAlive())
        {
            return;
        }

        if (session.PlayerEntity.Gold < 500 * (1 + arenaType))
        {
            session.SendChatMessage(_gameLanguage.GetLanguage(GameDialogKey.INTERACTION_MESSAGE_NOT_ENOUGH_GOLD, session.UserLanguage), ChatMessageColorType.Yellow);
            return;
        }

        session.PlayerEntity.LastPortal = DateTime.UtcNow;
        session.PlayerEntity.Gold -= 500 * (1 + arenaType);
        session.RefreshGold();
        await _mapManager.TeleportOnRandomPlaceInMapAsync(session, arenaType == 0 ? _arenaManager.ArenaInstance : _arenaManager.FamilyArenaInstance);
    }
}