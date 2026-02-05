using System;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Inventory;
using WingsEmu.Game.Miniland;
using WingsEmu.Game.Miniland.Events;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;

namespace NosEmu.Plugins.BasicImplementations.Event.Miniland;

public class AddObjMinilandEventHandler : IAsyncEventProcessor<AddObjMinilandEvent>
{
    public async Task HandleAsync(AddObjMinilandEvent e, CancellationToken cancellation)
    {
        IClientSession session = e.Sender;
        IPlayerEntity character = e.Sender.PlayerEntity;
        if (character.MapInstanceId != character.Miniland.Id)
        {
            return;
        }

        if (character.MinilandState != MinilandState.LOCK)
        {
            session.SendMsg(session.GetLanguage(GameDialogKey.MINILAND_SHOUTMESSAGE_NEED_BE_LOCKED), MsgMessageType.Middle);
            return;
        }

        InventoryItem minilandItem = character.GetItemBySlotAndType(e.Slot, InventoryType.Miniland);
        if (minilandItem == null)
        {
            return;
        }

        var mapObject = new MapDesignObject
        {
            Id = Guid.NewGuid(),
            CharacterId = e.Sender.PlayerEntity.Id,
            InventoryItem = minilandItem,
            InventorySlot = minilandItem.Slot,
            MapX = e.X,
            MapY = e.Y
        };

        await e.Sender.EmitEventAsync(new AddObjMinilandEndLogicEvent(mapObject, e.Sender.PlayerEntity.Miniland));
    }
}