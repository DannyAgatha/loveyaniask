using System.Collections.Generic;
using System.Threading.Tasks;
using PhoenixLib.Logging;
using WingsEmu.Core.Extensions;
using WingsEmu.Game._i18n;
using WingsEmu.Game._ItemUsage;
using WingsEmu.Game._ItemUsage.Event;
using WingsEmu.Game.Core.ItemHandling.Event;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Inventory;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;

namespace NosEmu.Plugins.BasicImplementations;

public class ItemHandlerContainer : IItemHandlerContainer
{
    private readonly Dictionary<ItemType, Dictionary<long, IItemHandler>> _handlers = new();
    private readonly Dictionary<long, IItemUsageByVnumHandler> _handlersByVnum = new();
    private readonly IItemUsageManager _itemUsageManager;
    private readonly IItemUsageToggleManager _itemUsageToggleManager;

    public ItemHandlerContainer(IItemUsageManager itemUsageManager, IItemUsageToggleManager itemUsageToggleManager)
    {
        _itemUsageManager = itemUsageManager;
        _itemUsageToggleManager = itemUsageToggleManager;
    }

    public async Task RegisterItemHandler(IItemHandler handler)
    {
        Dictionary<long, IItemHandler> handlers = _handlers.GetOrDefault(handler.ItemType);
        if (handlers == null)
        {
            handlers = new Dictionary<long, IItemHandler>();
            _handlers[handler.ItemType] = handlers;
        }

        foreach (long effect in handler.Effects)
        {
            handlers.Add(effect, handler);
        }

        Log.Debug($"[ITEM][REGISTER_HANDLER] UI_EFFECT : {handler.Effects} && TYPE : {handler.ItemType} REGISTERED !");
    }

    public async Task RegisterItemHandler(IItemUsageByVnumHandler handler)
    {
        foreach (long vnum in handler.Vnums)
        {
            _handlersByVnum.TryAdd(vnum, handler);
            Log.Debug($"[ITEM][REGISTER_HANDLER] VNUM : {vnum}");
        }
    }

    public async Task UnregisterAsync(IItemHandler handler)
    {
        Dictionary<long, IItemHandler> handlers = _handlers.GetOrDefault(handler.ItemType);
        if (handlers == null)
        {
            return;
        }

        foreach (long effect in handler.Effects)
        {
            handlers.Remove(effect);
        }
    }

    public async Task UnregisterAsync(IItemUsageByVnumHandler handler)
    {
        foreach (long vnum in handler.Vnums)
        {
            _handlersByVnum.Remove(vnum);
        }
    }

    public void Handle(IClientSession player, InventoryUseItemEvent e)
    {
        HandleAsync(player, e).ConfigureAwait(false).GetAwaiter().GetResult();
    }

    public async Task HandleAsync(IClientSession player, InventoryUseItemEvent e)
    {
        if (await _itemUsageToggleManager.IsItemBlocked(e.Item.ItemInstance.ItemVNum))
        {
            player.SendMsg(GameDialogKey.ITEM_USAGE_MSG_ITEM_DISABLED_TEMPORARILY, MsgMessageType.Middle);
            return;
        }

        IItemUsageByVnumHandler vnumHandler = _handlersByVnum.GetValueOrDefault(e.Item.ItemInstance.ItemVNum);
        if (vnumHandler == null)
        {
            Dictionary<long, IItemHandler> handlers = _handlers.GetOrDefault(e.Item.ItemInstance.GameItem.ItemType);
            if (handlers == null)
            {
                Log.Debug($"[ITEM][HANDLER_NOT_FOUND] VNUM: {e.Item.ItemInstance.ItemVNum}");
                return;
            }

            IItemHandler handler = handlers.GetOrDefault(e.Item.ItemInstance.GameItem.Effect);
            if (handler == null)
            {
                Log.Debug($"[ITEM][HANDLER_NOT_FOUND] VNUM: {e.Item.ItemInstance.ItemVNum}");
                return;
            }

            Log.Debug($"[ITEM][HANDLE] Effect: {e.Item.ItemInstance.GameItem.Effect} EffectValue: {e.Item.ItemInstance.GameItem.EffectValue} ItemType: {e.Item.ItemInstance.GameItem.ItemType}");
            _itemUsageManager.SetLastItemUsed(player.PlayerEntity.Id, e.Item.ItemInstance.ItemVNum);
            await handler.HandleAsync(player, e);
            return;
        }

        Log.Debug($"[ITEM][HANDLE] VNUM: {e.Item.ItemInstance.ItemVNum}");
        _itemUsageManager.SetLastItemUsed(player.PlayerEntity.Id, e.Item.ItemInstance.ItemVNum);
        await vnumHandler.HandleAsync(player, e);
        await player.EmitEventAsync(new InventoryUsedItemEvent { Item = e.Item });
    }
}