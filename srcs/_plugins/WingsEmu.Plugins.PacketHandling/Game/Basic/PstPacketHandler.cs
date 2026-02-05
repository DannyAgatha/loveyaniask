using System;
using System.Threading.Tasks;
using WingsEmu.Game.Mails.Events;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.ClientPackets;

namespace WingsEmu.Plugins.PacketHandling.Game.Basic;

public class PstPacketHandler : GenericGamePacketHandlerBase<PstPacket>
{
    protected override async Task HandlePacketAsync(IClientSession session, PstPacket pstPacket)
    {
        if (!Enum.TryParse(pstPacket.Argument.ToString(), out PstPacketType type))
        {
            return;
        }

        bool isSenderCopy = pstPacket.Type == 2;

        switch (type)
        {
            case PstPacketType.SendNote:
                string data = pstPacket.Data;
                string receiverName = pstPacket.Receiver;
                if (string.IsNullOrEmpty(data))
                {
                    return;
                }

                if (string.IsNullOrEmpty(receiverName))
                {
                    return;
                }

                string[] split = data.Split(' ');
                string title = split[0];
                string message = split[1];

                await session.EmitEventAsync(new NoteCreateEvent(receiverName, title, message));
                break;
            case PstPacketType.RemoveNote:
                await session.EmitEventAsync(new NoteRemoveEvent(pstPacket.Id, isSenderCopy));
                break;
            case PstPacketType.ReadNote:
                await session.EmitEventAsync(new NoteOpenEvent(pstPacket.Id, isSenderCopy));
                break;
        }
    }
}