using System;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsEmu.DTOs.Items;
using WingsEmu.Game._enum;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Extensions.Mates;
using WingsEmu.Game.Inventory;
using WingsEmu.Game.Inventory.Event;
using WingsEmu.Game.Items;
using WingsEmu.Game.Mates;
using WingsEmu.Game.Mates.Events;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;

namespace NosEmu.Plugins.BasicImplementations.Inventory;

public class PartnerInventoryTakeOffItemEventHandler : IAsyncEventProcessor<PartnerInventoryTakeOffItemEvent>
{
    private readonly IGameLanguageService _gameLanguage;

    public PartnerInventoryTakeOffItemEventHandler(IGameLanguageService gameLanguage) => _gameLanguage = gameLanguage;

    public async Task HandleAsync(PartnerInventoryTakeOffItemEvent e, CancellationToken cancellation)
    {
        IClientSession session = e.Sender;
        short petSlot = (short)(e.PetId - 1);
        byte slot = e.Slot;

        if (session.PlayerEntity.IsInExchange() || !session.HasCurrentMapInstance)
        {
            return;
        }

        if (session.PlayerEntity.HasShopOpened || session.PlayerEntity.ShopComponent.Items != null)
        {
            session.SendChatMessage(_gameLanguage.GetLanguage(GameDialogKey.ITEM_MESSAGE_CANT_USE, session.UserLanguage), ChatMessageColorType.Yellow);
            return;
        }

        IMateEntity mateEntity = session.PlayerEntity.MateComponent.GetMate(x => x.PetSlot == petSlot && x.MateType == MateType.Partner);
        if (mateEntity == null)
        {
            return;
        }

        if (!Enum.TryParse(slot.ToString(), out EquipmentType equipmentType))
        {
            return;
        }

        PartnerInventoryItem inv = session.PlayerEntity.PartnerGetEquippedItem(equipmentType, petSlot);
        if (inv == null)
        {
            return;
        }

        if (inv.ItemInstance.Type != ItemInstanceType.WearableInstance && inv.ItemInstance.Type != ItemInstanceType.SpecialistInstance)
        {
            return;
        }

        switch (e.Slot)
        {
            case (byte)EquipmentType.Sp when mateEntity.IsUsingSp:
                if (mateEntity.LastSkillUse.AddSeconds(2) > DateTime.UtcNow)
                {
                    return;
                }

                if (mateEntity.IsSitting)
                {
                    await session.EmitEventAsync(new MateRestEvent
                    {
                        MateEntity = mateEntity
                    });
                }

                if (mateEntity.BuffComponent.HasBuff(BuffGroup.Bad))
                {
                    session.SendMsg(_gameLanguage.GetLanguage(GameDialogKey.SPECIALIST_SHOUTMESSAGE_NO_REMOVE_DEBUFFS, session.UserLanguage), MsgMessageType.Middle);
                    return;
                }

                if (session.PlayerEntity.IsOnVehicle)
                {
                    session.SendMsg(_gameLanguage.GetLanguage(GameDialogKey.INFORMATION_SHOUTMESSAGE_REMOVE_VEHICLE, session.UserLanguage), MsgMessageType.Middle);
                    return;
                }

                await session.EmitEventAsync(new MateSpUntransformEvent
                {
                    MateEntity = mateEntity
                });

                break;
            case (byte)EquipmentType.Sp when !mateEntity.IsUsingSp && !mateEntity.IsSpCooldownElapsed():
                session.SendMsg(_gameLanguage.GetLanguageFormat(GameDialogKey.PARTNER_SHOUTMESSAGE_SP_IN_COOLDOWN, session.UserLanguage, mateEntity.GetSpCooldown()), MsgMessageType.Middle);
                return;
        }

        if (!session.PlayerEntity.HasSpaceFor(inv.ItemInstance.ItemVNum))
        {
            session.SendMsg(_gameLanguage.GetLanguage(GameDialogKey.INTERACTION_MESSAGE_NOT_ENOUGH_PLACE, session.UserLanguage), MsgMessageType.Middle);
            session.SendChatMessage(_gameLanguage.GetLanguage(GameDialogKey.INTERACTION_MESSAGE_NOT_ENOUGH_PLACE, session.UserLanguage), ChatMessageColorType.Yellow);
            return;
        }

        GameItemInstance itemInstance = inv.ItemInstance;
        await session.AddNewItemToInventory(itemInstance);
        session.PlayerEntity.PartnerTakeOffItem(equipmentType, petSlot);
        mateEntity.RefreshEquipmentValues(itemInstance, true);
        session.SendPetInfo(mateEntity, _gameLanguage);
        session.SendCondMate(mateEntity);
    }
}