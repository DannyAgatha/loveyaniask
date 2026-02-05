using System.Threading.Tasks;
using WingsEmu.DTOs.Maps;
using WingsEmu.Game._i18n;
using WingsEmu.Game._ItemUsage;
using WingsEmu.Game._ItemUsage.Event;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Networking;
using WingsEmu.Game.TimeSpaces.Events;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;

namespace NosEmu.Plugins.BasicImplementations.ItemUsage.Main.Special;

public class TimeSpaceStoneItemHandler : IItemHandler
{
    public ItemType ItemType => ItemType.Special;
    public long[] Effects => new long[] { 140 };

    public async Task HandleAsync(IClientSession session, InventoryUseItemEvent e)
    {
        if (session.PlayerEntity.IsInGroup())
        {
            session.SendMsg(session.GetLanguage(GameDialogKey.INFORMATION_SHOUTMESSAGE_NEED_LEAVE_GROUP), MsgMessageType.Middle);
            return;
        }

        if (session.CurrentMapInstance.HasMapFlag(MapFlags.ACT_4) || !session.CurrentMapInstance.HasMapFlag(MapFlags.IS_BASE_MAP))
        {
            session.SendMsg(session.GetLanguage(GameDialogKey.INFORMATION_SHOUTMESSAGE_MUST_BE_IN_CLASSIC_MAP), MsgMessageType.Middle);
            return;
        }

        await session.EmitEventAsync(new TimeSpacePartyCreateEvent(e.Item.ItemInstance.GameItem.Data[2], e.Item));
        if (!session.PlayerEntity.TimeSpaceComponent.IsInTimeSpaceParty)
        {
            return;
        }

        await session.EmitEventAsync(new TimeSpaceInstanceStartEvent());
    }
}