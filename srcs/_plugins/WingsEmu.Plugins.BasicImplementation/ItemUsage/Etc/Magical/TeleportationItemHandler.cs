using System;
using System.Threading.Tasks;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsEmu.Core.Extensions;
using WingsEmu.DTOs.Maps;
using WingsEmu.DTOs.Respawns;
using WingsEmu.Game._enum;
using WingsEmu.Game._i18n;
using WingsEmu.Game._ItemUsage;
using WingsEmu.Game._ItemUsage.Event;
using WingsEmu.Game.Act4;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Helpers;
using WingsEmu.Game.Items;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Networking;
using WingsEmu.Game.RespawnReturn.Event;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;

namespace NosEmu.Plugins.BasicImplementations.ItemUsage.Etc.Magical;

/// <summary>
///     This handler is used for Return Wing, Return Amulet, Return Scroll, Miniland Bell, Lod Scroll
/// </summary>
public class TeleportationItemHandler : IItemHandler
{
    private const int ReturnWing = 0;
    private const int ReturnAmulet = 1;
    private const int MinilandBell = 2;
    private const int LodScroll = 4;
    private const int BaseTeleporter = 5;
    private const int ReturnScroll = 6;
    private readonly IAct4FlagManager _act4FlagManager;

    private readonly IDelayManager _delayManager;
    private readonly IGameLanguageService _languageService;
    private readonly IMapManager _mapManager;

    public TeleportationItemHandler(IGameLanguageService languageService, IDelayManager delayManager, IAct4FlagManager act4FlagManager, IMapManager mapManager)
    {
        _languageService = languageService;
        _delayManager = delayManager;
        _act4FlagManager = act4FlagManager;
        _mapManager = mapManager;
    }

    public ItemType ItemType => ItemType.Magical;
    public long[] Effects => new long[] { 1 };

    public async Task HandleAsync(IClientSession session, InventoryUseItemEvent e)
    {
        IGameItem gameItem = e.Item.ItemInstance.GameItem;

        if (session.PlayerEntity.IsOnVehicle)
        {
            session.SendChatMessage(_languageService.GetLanguage(GameDialogKey.ITEM_CHATMESSAGE_CANT_USE_THAT, session.UserLanguage), ChatMessageColorType.Yellow);
            return;
        }

        if (!session.PlayerEntity.IsAlive())
        {
            return;
        }

        if (session.PlayerEntity.IsInExchange())
        {
            return;
        }

        if (session.PlayerEntity.HasShopOpened)
        {
            return;
        }

        switch (gameItem.EffectValue)
        {
            /*
             * RETURN WING & RETURN SCROLL (ACT 5)
             */
            case ReturnWing or ReturnScroll when session.CantPerformActionOnAct4():
                return;
            case ReturnWing or ReturnScroll:
            {
                DelayedActionType actionType = gameItem.EffectValue == ReturnWing ? DelayedActionType.ReturnWing : DelayedActionType.ReturnScroll;

                bool isAct5Map = session.CurrentMapInstance.HasMapFlag(MapFlags.ACT_5_1) || session.CurrentMapInstance.HasMapFlag(MapFlags.ACT_5_2);

                if (isAct5Map && gameItem.EffectValue == ReturnWing || !isAct5Map && gameItem.EffectValue == ReturnScroll)
                {
                    session.SendChatMessage(_languageService.GetLanguage(GameDialogKey.ITEM_CHATMESSAGE_CANT_USE_THAT, session.UserLanguage), ChatMessageColorType.Yellow);
                    return;
                }

                if (e.Option == 0)
                {
                    session.SendDialog(
                        $"u_i 2 {session.PlayerEntity.Id} {(byte)e.Item.ItemInstance.GameItem.Type} {e.Item.Slot} 1",
                        $"u_i 2 {session.PlayerEntity.Id} {(byte)e.Item.ItemInstance.GameItem.Type} {e.Item.Slot} 2",
                        _languageService.GetLanguage(GameDialogKey.ITEM_DIALOG_WANT_TO_SAVE_POSITION, session.UserLanguage));
                    return;
                }

                if (!int.TryParse(e.Packet[6], out int usageType))
                {
                    return;
                }

                switch (usageType)
                {
                    case 1:
                    // 1 - Save position, 2 - Don't save position
                    case 2:
                    {
                        DateTime targetTime = await _delayManager.RegisterAction(session.PlayerEntity, actionType);
                        session.SendDelay(targetTime.GetTotalMillisecondUntilNow(), GuriType.UsingItem,
                            $"u_i 2 {session.PlayerEntity.Id} {(byte)e.Item.ItemInstance.GameItem.Type} {e.Item.Slot} {(usageType == 1 ? 3 : 4)}");
                        return;
                    }
                    case 3:
                    case 4:
                    {
                        bool canPerformAction = await _delayManager.CanPerformAction(session.PlayerEntity, actionType);
                        if (!canPerformAction)
                        {
                            return;
                        }

                        if (!session.PlayerEntity.MapInstance.HasMapFlag(MapFlags.IS_BASE_MAP))
                        {
                            session.SendChatMessage(_languageService.GetLanguage(GameDialogKey.ITEM_CHATMESSAGE_CANT_USE_THAT, session.UserLanguage), ChatMessageColorType.Yellow);
                            return;
                        }

                        await _delayManager.CompleteAction(session.PlayerEntity, actionType);

                        if (usageType == 3 && session.PlayerEntity.MapId != 10000)
                        {
                            await session.EmitEventAsync(new ReturnChangeEvent
                            {
                                MapId = session.PlayerEntity.MapId,
                                MapX = session.PlayerEntity.PositionX,
                                MapY = session.PlayerEntity.PositionY
                            });
                        }

                        await session.Respawn();

                        await session.RemoveItemFromInventory(item: e.Item);
                        break;
                    }
                }

                return;
            }
            /*
         * RETURN AMULET
         */
            case ReturnAmulet when session.CantPerformActionOnAct4():
                return;
            case ReturnAmulet:
            {
                if (!int.TryParse(e.Packet[6], out int usageType))
                {
                    return;
                }

                if (!session.PlayerEntity.MapInstance.HasMapFlag(MapFlags.IS_BASE_MAP))
                {
                    session.SendChatMessage(_languageService.GetLanguage(GameDialogKey.ITEM_CHATMESSAGE_CANT_USE_THAT, session.UserLanguage), ChatMessageColorType.Yellow);
                    return;
                }

                CharacterReturnDto returnPoint = session.PlayerEntity.HomeComponent.Return;

                if (returnPoint == null)
                {
                    session.PlayerEntity.HomeComponent.ChangeReturn(new CharacterReturnDto());
                    return;
                }

                IMapInstance returnMap = _mapManager.GetBaseMapInstanceByMapId(returnPoint.MapId);

                if (returnPoint.MapId != 0 && returnMap != null)
                {
                    bool isAct5 = returnMap.HasMapFlag(MapFlags.ACT_5_1) || returnMap.HasMapFlag(MapFlags.ACT_5_2);
                    if (!session.IsInAct5() && isAct5)
                    {
                        session.SendChatMessage(_languageService.GetLanguage(GameDialogKey.ACT5_MESSAGE_RETURN_DISABLED, session.UserLanguage), ChatMessageColorType.Yellow);
                        return;
                    }

                    if (session.IsInAct5() && !isAct5)
                    {
                        session.SendChatMessage(_languageService.GetLanguage(GameDialogKey.ACT5_MESSAGE_RETURN_DISABLED, session.UserLanguage), ChatMessageColorType.Yellow);
                        return;
                    }
                }

                switch (usageType)
                {
                    case 0:
                    {
                        if (returnPoint.MapId == 0)
                        {
                            return;
                        }

                        session.SendRpPacket(returnPoint.MapId, returnPoint.MapX, returnPoint.MapY, $"#u_i^2^{session.PlayerEntity.Id}^{(byte)e.Item.ItemInstance.GameItem.Type}^{e.Item.Slot}^1");
                        return;
                    }
                    case 1:
                    {
                        DateTime targetTime = await _delayManager.RegisterAction(session.PlayerEntity, DelayedActionType.ReturnAmulet);
                        session.SendDelay(targetTime.GetTotalMillisecondUntilNow(), GuriType.UsingItem, $"u_i 2 {session.PlayerEntity.Id} {(byte)e.Item.ItemInstance.GameItem.Type} {e.Item.Slot} 2");
                        return;
                    }
                    case 2:
                    {
                        bool canUseReturnAmulet = await _delayManager.CanPerformAction(session.PlayerEntity, DelayedActionType.ReturnAmulet);
                        if (!canUseReturnAmulet)
                        {
                            return;
                        }

                        await _delayManager.CompleteAction(session.PlayerEntity, DelayedActionType.ReturnAmulet);

                        if (returnPoint.MapId == 0)
                        {
                            return;
                        }

                        session.ChangeMap(returnPoint.MapId, returnPoint.MapX, returnPoint.MapY);
                        await session.RemoveItemFromInventory(item: e.Item);
                        break;
                    }
                }

                return;
            }
            case BaseTeleporter:
                if (!session.PlayerEntity.MapInstance.HasMapFlag(MapFlags.ACT_4))
                {
                    return;
                }

                if (!session.PlayerEntity.MapInstance.HasMapFlag(MapFlags.IS_BASE_MAP))
                {
                    return;
                }

                if (e.Option == 0)
                {
                    DateTime targetTime = await _delayManager.RegisterAction(session.PlayerEntity, DelayedActionType.BaseTeleporter);
                    session.SendDelay(targetTime.GetTotalMillisecondUntilNow(), GuriType.UsingItem, $"u_i 2 {session.PlayerEntity.Id} {(byte)e.Item.ItemInstance.GameItem.Type} {e.Item.Slot} 1");
                    return;
                }

                bool canPerformActionAction = await _delayManager.CanPerformAction(session.PlayerEntity, DelayedActionType.BaseTeleporter);
                if (!canPerformActionAction)
                {
                    return;
                }

                await _delayManager.CompleteAction(session.PlayerEntity, DelayedActionType.BaseTeleporter);

                FactionType faction = session.PlayerEntity.Faction;
                MapLocation map = faction == FactionType.Angel ? _act4FlagManager.AngelFlag : _act4FlagManager.DemonFlag;
                if (map == null)
                {
                    return;
                }

                session.ChangeMap(map.MapInstanceId, map.X, map.Y);
                await session.RemoveItemFromInventory(item: e.Item);

                break;
            /*
         * MINILAND BELL & LOD SCROLL
         */
            case MinilandBell:
                {

                    if (!session.PlayerEntity.MapInstance.HasMapFlag(MapFlags.IS_BASE_MAP))
                    {
                        session.SendChatMessage(_languageService.GetLanguage(GameDialogKey.ITEM_CHATMESSAGE_CANT_USE_THAT, session.UserLanguage), ChatMessageColorType.Yellow);
                        return;
                    }

                    DelayedActionType actionType = DelayedActionType.MinilandBell;
                    if (e.Option == 0)
                    {
                        DateTime targetTime = await _delayManager.RegisterAction(session.PlayerEntity, actionType);
                        session.SendDelay(targetTime.GetTotalMillisecondUntilNow(), GuriType.UsingItem, $"u_i 2 {session.PlayerEntity.Id} {(byte)e.Item.ItemInstance.GameItem.Type} {e.Item.Slot} 1");
                        return;
                    }

                    bool canPerformAction = await _delayManager.CanPerformAction(session.PlayerEntity, actionType);
                    if (!canPerformAction)
                    {
                        return;
                    }

                    await _delayManager.CompleteAction(session.PlayerEntity, actionType);

                    if (actionType != DelayedActionType.MinilandBell)
                    {
                        return;
                    }

                    if (session.CurrentMapInstance.HasMapFlag(MapFlags.ACT_4))
                    {
                        bool canEnter = session.PlayerEntity.Faction switch
                        {
                            FactionType.Angel => session.CurrentMapInstance.MapId == (int)MapIds.ACT4_ANGEL_CITADEL,
                            FactionType.Demon => session.CurrentMapInstance.MapId == (int)MapIds.ACT4_DEMON_CITADEL,
                            _ => false
                        };

                        if (!canEnter)
                        {
                            session.SendMsg(_languageService.GetLanguage(GameDialogKey.INFORMATION_SHOUTMESSAGE_MUST_BE_IN_CLASSIC_MAP, session.UserLanguage), MsgMessageType.Middle);
                            return;
                        }
                    }

                    session.ChangeMap(session.PlayerEntity.Miniland);
                    await session.RemoveItemFromInventory(item: e.Item);
                    break;
                }
            case LodScroll:
            {
                if (!session.PlayerEntity.MapInstance.HasMapFlag(MapFlags.IS_BASE_MAP))
                {
                    session.SendChatMessage(_languageService.GetLanguage(GameDialogKey.ITEM_CHATMESSAGE_CANT_USE_THAT, session.UserLanguage), ChatMessageColorType.Yellow);
                    return;
                }

                DelayedActionType actionType = DelayedActionType.LodScroll;
                if (e.Option == 0)
                {
                    DateTime targetTime = await _delayManager.RegisterAction(session.PlayerEntity, actionType);
                    session.SendDelay(targetTime.GetTotalMillisecondUntilNow(), GuriType.UsingItem, $"u_i 2 {session.PlayerEntity.Id} {(byte)e.Item.ItemInstance.GameItem.Type} {e.Item.Slot} 1");
                    return;
                }

                bool canPerformAction = await _delayManager.CanPerformAction(session.PlayerEntity, actionType);
                if (!canPerformAction)
                {
                    return;
                }

                await _delayManager.CompleteAction(session.PlayerEntity, actionType);

                if (actionType != DelayedActionType.LodScroll)
                {
                    return;
                }

                if (session.CurrentMapInstance.HasMapFlag(MapFlags.ACT_4))
                {
                        session.SendChatMessage(_languageService.GetLanguage(GameDialogKey.ITEM_CHATMESSAGE_CANT_USE_THAT, session.UserLanguage), ChatMessageColorType.Yellow);
                        return;
                }

                session.ChangeMap(98,11,26);
                await session.RemoveItemFromInventory(item: e.Item);
                break;
            }
        }
    }
}