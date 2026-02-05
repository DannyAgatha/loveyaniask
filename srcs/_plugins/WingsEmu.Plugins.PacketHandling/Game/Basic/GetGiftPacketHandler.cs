using System;
using System.Threading.Tasks;
using WingsEmu.Game.Mails.Events;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.ClientPackets;

namespace WingsEmu.Plugins.PacketHandling.Game.Basic;

public class GetGiftPacketHandler : GenericGamePacketHandlerBase<GetGiftPacket>
{
    protected override async Task HandlePacketAsync(IClientSession session, GetGiftPacket getGiftPacket)
    {
        if (!Enum.TryParse(getGiftPacket.Type.ToString(), out GetGiftType type))
        {
            return;
        }

        switch (type)
        {
            case GetGiftType.OpenMail:
                await session.EmitEventAsync(new MailOpenEvent(getGiftPacket.GiftId));
                break;
            case GetGiftType.RemoveMail:
                await session.EmitEventAsync(new MailRemoveEvent(getGiftPacket.GiftId));
                break;
        }
    }
}