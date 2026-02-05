using System.Linq;
using System.Threading.Tasks;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Chat;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Groups;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.ClientPackets;
using WingsEmu.Packets.Enums.Chat;
using ChatType = WingsEmu.Game._playerActionLogs.ChatType;

namespace WingsEmu.Plugins.PacketHandling.Game.Group;

public class GroupSayPacketHandler : GenericGamePacketHandlerBase<GroupSayPacket>
{
    protected override async Task HandlePacketAsync(IClientSession session, GroupSayPacket groupSayPacket)
    {
        if (string.IsNullOrEmpty(groupSayPacket.Message))
        {
            return;
        }

        if (session.PlayerEntity.IsInRaidParty)
        {
            foreach (IClientSession target in session.PlayerEntity.Raid.Members)
            {
                session.SendSpeakToTarget(target, groupSayPacket.Message, SpeakType.Group);
            }

            await session.EmitEventAsync(new ChatGenericEvent
            {
                Message = groupSayPacket.Message,
                ChatType = ChatType.GroupChat
            });
            return;
        }

        if (!session.PlayerEntity.IsInGroup())
        {
            return;
        }

        PlayerGroup group = session.PlayerEntity.GetGroup();
        foreach (IPlayerEntity member in group.Members.ToArray())
        {
            session.SendSpeakToTarget(member.Session, groupSayPacket.Message, SpeakType.Group);
        }

        await session.EmitEventAsync(new ChatGenericEvent
        {
            Message = groupSayPacket.Message,
            ChatType = ChatType.GroupChat
        });
    }
}