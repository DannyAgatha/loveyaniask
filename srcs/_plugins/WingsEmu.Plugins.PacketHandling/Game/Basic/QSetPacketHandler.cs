using System.Threading.Tasks;
using WingsEmu.DTOs.Quicklist;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Quicklist;
using WingsEmu.Packets.ClientPackets;

namespace WingsEmu.Plugins.PacketHandling.Game.Basic;

public class QSetPacketHandler : GenericGamePacketHandlerBase<QSetPacket>
{
    protected override async Task HandlePacketAsync(IClientSession session, QSetPacket qSetPacket)
    {
        short secondParam = 0;

        // first => secondParam
        // item => inventorySlot
        // skill =>
        //          0 => passiveSkill => ??
        //          1 => skill => skillTabSlot (ordered by castId)
        //          2 => skill upgrade => upgradeSlot (its ordered by parent skill priority and then how much you have)
        //          3 => emote/motion => vnum

        short inventoryOrSkillSlot = 0;
        short quicklistTab = qSetPacket.QuicklistTab;
        short quicklistSlot = qSetPacket.QuicklistSlot;
        QsetPacketType type = qSetPacket.Type;


        if (qSetPacket.DestinationType.HasValue)
        {
            secondParam = qSetPacket.DestinationType.Value;
        }

        if (qSetPacket.DestinationSlotOrVnum.HasValue)
        {
            inventoryOrSkillSlot = qSetPacket.DestinationSlotOrVnum.Value;
        }

        switch (type)
        {
            case QsetPacketType.SET_ITEM:
            case QsetPacketType.SET_SKILL:

                session.EmitEvent(new QuicklistAddEvent
                {
                    Tab = quicklistTab,
                    Slot = quicklistSlot,
                    Type = type switch { QsetPacketType.SET_ITEM => QuicklistType.ITEM, QsetPacketType.SET_SKILL => QuicklistType.SKILLS },
                    DestinationType = secondParam,
                    DestinationSlotOrVnum = inventoryOrSkillSlot
                });
                break;

            case QsetPacketType.SWAP:
                session.EmitEvent(new QuicklistSwapEvent
                {
                    Tab = quicklistTab,
                    FromSlot = inventoryOrSkillSlot,
                    ToSlot = quicklistSlot
                });

                break;

            case QsetPacketType.REMOVE:
                session.EmitEvent(new QuicklistRemoveEvent { Tab = quicklistTab, Slot = quicklistSlot });
                break;

            default:
                return;
        }
    }
}