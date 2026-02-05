using System;
using System.Linq;
using System.Threading.Tasks;
using PhoenixLib.Logging;
using WingsAPI.Communication.ServerApi.Protocol;
using WingsEmu.Game;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Maps.Event;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.ClientPackets;

namespace WingsEmu.Plugins.PacketHandling.Game.Basic;

public class PreqPacketHandler : GenericGamePacketHandlerBase<PreqPacket>
{
    private readonly IGameLanguageService _languageService;
    private readonly SerializableGameServer _serializableGameServer;

    public PreqPacketHandler(IGameLanguageService languageService, SerializableGameServer serializableGameServer)
    {
        _languageService = languageService;
        _serializableGameServer = serializableGameServer;
    }

    protected override async Task HandlePacketAsync(IClientSession session, PreqPacket packet)
    {
        bool isAct4 = _serializableGameServer.ChannelType == GameChannelType.ACT_4;
        if (session.PlayerEntity.LastMapChange + TimeSpan.FromSeconds(isAct4 ? 5 : 2) > DateTime.UtcNow)
        {
            session.SendInformationChatMessage(_languageService.GetLanguage(GameDialogKey.PORTAL_CHATMESSAGE_TOO_EARLY, session.UserLanguage));
            return;
        }

        if (session.PlayerEntity.IsSeal)
        {
            return;
        }

        if (!session.PlayerEntity.IsAlive())
        {
            return;
        }

        IPortalEntity portal = session.CurrentMapInstance.Portals.Concat(session.PlayerEntity.GetExtraPortal()).FirstOrDefault(s =>
            session.PlayerEntity.PositionY >= s.PositionY - 1 &&
            session.PlayerEntity.PositionY <= s.PositionY + 1 &&
            session.PlayerEntity.PositionX >= s.PositionX - 1 &&
            session.PlayerEntity.PositionX <= s.PositionX + 1);

        if (portal == null)
        {
            Log.Debug("Portal not found");
            return;
        }

        await session.EmitEventAsync(new PortalTriggerEvent
        {
            Portal = portal,
            Confirmed = packet.Confirmed
        });
    }
}