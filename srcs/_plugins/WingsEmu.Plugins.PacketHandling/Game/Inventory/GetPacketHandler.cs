using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WingsEmu.Game.Inventory.Event;
using WingsEmu.Game.Mates;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.ClientPackets;
using WingsEmu.Packets.Enums;

namespace WingsEmu.Plugins.PacketHandling.Game.Inventory;

public class GetPacketHandler : GenericGamePacketHandlerBase<GetPacket>
{
    protected override async Task HandlePacketAsync(IClientSession session, GetPacket getPacket)
    {
        if (!Enum.TryParse(getPacket.PickerType.ToString(), out VisualType type))
        {
            return;
        }

        if (type is VisualType.Npc)
        {
            IMateEntity mate = session.PlayerEntity.MateComponent.GetTeamMember(x => x.MateType == MateType.Pet && x.HasDhaPremium);
            if (mate is not null)
            {
                List<long> savedDrops = mate.SavedDrops;
                int savedDropsCount = savedDrops.Count;

                for (int i = 0; i < savedDropsCount; i++)
                {
                    long drops = savedDrops[i];
                    await session.EmitEventAsync(new InventoryPickUpItemEvent(type, getPacket.PickerId, drops));
                    savedDrops.RemoveAll(s => s == drops);
                    i--;
                    savedDropsCount--;
                }
                
                return;
            }
        }

        await session.EmitEventAsync(new InventoryPickUpItemEvent(type, getPacket.PickerId, getPacket.TransportId));
    }
}