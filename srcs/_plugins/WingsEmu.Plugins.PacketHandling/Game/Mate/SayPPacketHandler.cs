using System.Threading.Tasks;
using WingsEmu.DTOs.Maps;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Entities.Extensions;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Mates;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Networking.Broadcasting;
using WingsEmu.Packets.ClientPackets;
using WingsEmu.Packets.Enums.Chat;

namespace WingsEmu.Plugins.PacketHandling.Game.Mate;

public class SayPPacketHandler : GenericGamePacketHandlerBase<SayPPacket>
{
    private readonly IGameLanguageService _gameLanguage;

    public SayPPacketHandler(IGameLanguageService gameLanguage) => _gameLanguage = gameLanguage;

    protected override async Task HandlePacketAsync(IClientSession session, SayPPacket packet)
    {
        if (string.IsNullOrEmpty(packet.Message))
        {
            return;
        }

        if (session.IsMuted())
        {
            session.SendMuteMessage();
            return;
        }

        string message = packet.Message;

        if (message.Length > 60)
        {
            message = message.Substring(0, 60);
        }

        IMateEntity mateEntity = session.PlayerEntity.MateComponent.GetMate(s => s.Id == packet.PetId);
        if (mateEntity == null)
        {
            return;
        }

        if (session.CurrentMapInstance.HasMapFlag(MapFlags.ACT_4))
        {
            session.CurrentMapInstance.Broadcast(mateEntity.GenerateSayPacket(message.Trim(), ChatMessageColorType.Mate),
                new FactionBroadcast(session.PlayerEntity.Faction));
            return;
        }

        session.CurrentMapInstance.Broadcast(mateEntity.GenerateSayPacket(message, ChatMessageColorType.Mate));
    }
}