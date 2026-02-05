using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsAPI.Game.Extensions.ItemExtension.Item;
using WingsAPI.Packets.Enums.Chat;
using WingsEmu.DTOs.Items;
using WingsEmu.Game._enum;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.CharacterSizeModifiers;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Inventory;
using WingsEmu.Game.Inventory.Event;
using WingsEmu.Game.Items;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;

namespace NosEmu.Plugins.BasicImplementations.Inventory;

public class InventoryTakeOffItemEventHandler : IAsyncEventProcessor<InventoryTakeOffItemEvent>
{
    private readonly IGameLanguageService _gameLanguage;

    private readonly HashSet<EquipmentType> _shells = new()
    {
        EquipmentType.Armor,
        EquipmentType.MainWeapon,
        EquipmentType.SecondaryWeapon
    };

    public InventoryTakeOffItemEventHandler(IGameLanguageService gameLanguage) => _gameLanguage = gameLanguage;

    public async Task HandleAsync(InventoryTakeOffItemEvent e, CancellationToken cancellation)
    {
        IClientSession session = e.Sender;
        
        if (session.PlayerEntity.IsInExchange() || !session.HasCurrentMapInstance)
        {
            return;
        }

        if (session.PlayerEntity.HasShopOpened || session.PlayerEntity.ShopComponent.Items != null)
        {
            session.SendChatMessage(_gameLanguage.GetLanguage(GameDialogKey.ITEM_MESSAGE_CANT_USE, session.UserLanguage), ChatMessageColorType.Yellow);
            return;
        }

        if (!Enum.TryParse(e.Slot.ToString(), out EquipmentType equipmentType))
        {
            return;
        }

        InventoryItem inv = session.PlayerEntity.GetInventoryItemFromEquipmentSlot(equipmentType);
        if (inv == null)
        {
            return;
        }

        if (inv.ItemInstance.Type != ItemInstanceType.WearableInstance && inv.ItemInstance.Type != ItemInstanceType.SpecialistInstance)
        {
            return;
        }

        GameItemInstance item = inv.ItemInstance;

        switch (e.Slot)
        {
            case (byte)EquipmentType.Sp when session.PlayerEntity.UseSp:
                if (session.PlayerEntity.LastSkillUse.AddSeconds(2) > DateTime.UtcNow)
                {
                    return;
                }

                if (session.PlayerEntity.IsSitting)
                {
                    await session.RestAsync();
                }

                if (session.PlayerEntity.BuffComponent.HasBuff(BuffGroup.Bad))
                {
                    session.SendMsg(_gameLanguage.GetLanguage(GameDialogKey.SPECIALIST_SHOUTMESSAGE_NO_REMOVE_DEBUFFS, session.UserLanguage), MsgMessageType.Middle);
                    return;
                }

                if (session.PlayerEntity.IsOnVehicle)
                {
                    session.SendMsg(_gameLanguage.GetLanguage(GameDialogKey.INFORMATION_SHOUTMESSAGE_REMOVE_VEHICLE, session.UserLanguage), MsgMessageType.Middle);
                    return;
                }

                if (session.PlayerEntity.IsMorphed)
                {
                    session.SendChatMessage(_gameLanguage.GetLanguage(GameDialogKey.ITEM_MESSAGE_CANT_USE, session.UserLanguage), ChatMessageColorType.Yellow);
                    return;
                }

                await session.EmitEventAsync(new SpUntransformEvent());
                break;
            
            case (byte)EquipmentType.Fairy when session.PlayerEntity.IsUsingFairyBooster():
                session.SendMsgi(MessageType.Default, Game18NConstString.CannotUnequipWhileFairyBoosted);
                return;
        }

        if (!session.PlayerEntity.HasSpaceFor(inv.ItemInstance.ItemVNum) && !e.ForceToRandomSlot)
        {
            session.SendMsg(_gameLanguage.GetLanguage(GameDialogKey.INTERACTION_MESSAGE_NOT_ENOUGH_PLACE, session.UserLanguage), MsgMessageType.Middle);
            session.SendChatMessage(_gameLanguage.GetLanguage(GameDialogKey.INTERACTION_MESSAGE_NOT_ENOUGH_PLACE, session.UserLanguage), ChatMessageColorType.Yellow);
            return;
        }

        if ((item.ItemDeleteTime.HasValue || item.DurabilityPoint != 0) && session.ShouldSendAmuletPacket(item.GameItem.EquipmentSlot))
        {
            session.SendEmptyAmuletBuffPacket();
        }

        session.PlayerEntity.RefreshEquipmentValues(item, true);
        session.PlayerEntity.TakeOffItem(equipmentType, e.ForceToRandomSlot ? short.MaxValue : null);
        
        session.RefreshStatChar();
        session.RefreshEquipment();
        session.SendInventoryAddPacket(inv);
        session.BroadcastEq();
        session.BroadcastPairy();
        session.BroadcastMiniPet();
        session.SendCondPacket();
        session.RefreshStat();
        session.SendIncreaseRange();
        
        await session.EmitEventAsync(new CharacterSizeModifierCheckEvent());
    }
}