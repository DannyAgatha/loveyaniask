using System;
using System.Linq;
using System.Threading.Tasks;
using WingsAPI.Game.Extensions.ItemExtension.Item;
using WingsEmu.DTOs.Items;
using WingsEmu.Game.Algorithm;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Exchange;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Families.Event;
using WingsEmu.Game.Inventory;
using WingsEmu.Game.Items;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Managers.StaticData;
using WingsEmu.Game.Mates;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Shops;
using WingsEmu.Game.Warehouse;
using WingsEmu.Game.Warehouse.Events;
using WingsEmu.Packets.ClientPackets;
using WingsEmu.Packets.Enums;

namespace WingsEmu.Plugins.PacketHandling.Game.Inventory;

public class EquipmentInfoPacketHandler : GenericGamePacketHandlerBase<EquipmentInfoPacket>
{
    private readonly ICharacterAlgorithm _algorithm;
    private readonly IGameItemInstanceFactory _instanceFactory;
    private readonly IItemsManager _itemManager;
    private readonly ISessionManager _sessionManager;

    public EquipmentInfoPacketHandler(IItemsManager itemsManager, ISessionManager sessionManager,
        ICharacterAlgorithm algorithm, IGameItemInstanceFactory instanceFactory)
    {
        _itemManager = itemsManager;
        _sessionManager = sessionManager;
        _algorithm = algorithm;
        _instanceFactory = instanceFactory;
    }

    protected override async Task HandlePacketAsync(IClientSession session, EquipmentInfoPacket equipmentInfoPacket)
    {
        if (!session.HasSelectedCharacter)
        {
            return;
        }

        if (!session.HasCurrentMapInstance)
        {
            return;
        }

        InventoryItem inventory = null;
        switch (equipmentInfoPacket.Type)
        {
            case 0:
                if (!Enum.TryParse(equipmentInfoPacket.Slot.ToString(), out EquipmentType type))
                {
                    return;
                }

                inventory = session.PlayerEntity.GetInventoryItemFromEquipmentSlot(type);
                break;

            case 1:
                inventory = session.PlayerEntity.GetItemBySlotAndType(equipmentInfoPacket.Slot, InventoryType.Equipment);
                break;

            case 2:
                IGameItem npcGameItem = _itemManager.GetItem(equipmentInfoPacket.Slot);
                if (npcGameItem == null)
                {
                    return;
                }

                inventory = _instanceFactory.CreateInventoryItem(npcGameItem.Id);
                break;

            case 5:
                PlayerExchange playerExchange = session.PlayerEntity.GetExchange();
                if (playerExchange == null)
                {
                    return;
                }

                IClientSession targetSession = _sessionManager.GetSessionByCharacterId(playerExchange.TargetId);
                if (targetSession == null)
                {
                    break;
                }

                PlayerExchange targetExchange = targetSession.PlayerEntity.GetExchange();
                if (targetExchange == null)
                {
                    return;
                }

                if (!targetExchange.Items.Any())
                {
                    return;
                }

                if (targetExchange.Items.Count < equipmentInfoPacket.Slot)
                {
                    return;
                }

                ExchangeItem item = targetExchange.Items[equipmentInfoPacket.Slot];

                if (item is null)
                {
                    return;
                }

                inventory = targetSession.PlayerEntity.GetItemBySlotAndType(item.Slot, item.Type);
                break;

            case 6:
                if (!equipmentInfoPacket.ShopOwnerId.HasValue)
                {
                    return;
                }

                IPlayerEntity owner = session.CurrentMapInstance.GetCharacterById(equipmentInfoPacket.ShopOwnerId.Value);
                ShopPlayerItem shopPlayerItem = owner?.ShopComponent.GetItem(equipmentInfoPacket.Slot);
                if (shopPlayerItem is null)
                {
                    return;
                }

                inventory = owner.GetItemBySlotAndType(shopPlayerItem.InventorySlot, shopPlayerItem.InventoryType);

                break;

            case 7:
                if (equipmentInfoPacket.Slot == 0)
                {
                    return;
                }

                short partnerSlot = (short)(equipmentInfoPacket.Slot - 1);
                IMateEntity entity = session.PlayerEntity.MateComponent.GetMate(x => x.MateType == MateType.Partner && x.PetSlot == partnerSlot);
                if (entity == null)
                {
                    return;
                }

                if (!Enum.TryParse(equipmentInfoPacket.PartnerEqSlot.ToString(), out EquipmentType eqType))
                {
                    return;
                }

                PartnerInventoryItem partnerItem = session.PlayerEntity.PartnerGetEquippedItem(eqType, partnerSlot);

                if (partnerItem == null)
                {
                    return;
                }

                if (partnerItem.ItemInstance.Type != ItemInstanceType.WearableInstance && partnerItem.ItemInstance.Type != ItemInstanceType.SpecialistInstance)
                {
                    return;
                }

                GameItemInstance partnerInstance = partnerItem.ItemInstance;

                if (partnerInstance.GameItem.EquipmentSlot == EquipmentType.Sp)
                {
                    if (partnerInstance.GameItem.IsPartnerSpecialist)
                    {
                        session.SendPartnerSpecialistInfo(partnerInstance);
                    }
                    else
                    {
                        session.SendSpecialistCardInfo(partnerInstance, _algorithm);
                    }

                    return;
                }

                session.SendEInfoPacket(partnerInstance, _itemManager, _algorithm);
                return;

            case 8:
                await session.EmitEventAsync(new AccountWarehouseShowItemEvent
                {
                    Slot = equipmentInfoPacket.Slot
                });
                return;

            case 9:
                PartnerWarehouseItem partnerWarehouseItem = session.PlayerEntity.GetPartnerWarehouseItem(equipmentInfoPacket.Slot);

                if (partnerWarehouseItem == null)
                {
                    return;
                }

                GameItemInstance partnerWarehouseInstance = partnerWarehouseItem.ItemInstance;

                if (partnerWarehouseInstance.GameItem.EquipmentSlot == EquipmentType.Sp)
                {
                    if (partnerWarehouseInstance.GameItem.IsPartnerSpecialist)
                    {
                        session.SendPartnerSpecialistInfo(partnerWarehouseInstance);
                    }
                    else
                    {
                        session.SendSpecialistCardInfo(partnerWarehouseInstance, _algorithm);
                    }

                    return;
                }

                session.SendEInfoPacket(partnerWarehouseInstance, _itemManager, _algorithm);
                return;

            case 10:
                inventory = session.PlayerEntity.GetItemBySlotAndType(equipmentInfoPacket.Slot, InventoryType.Specialist);
                break;

            case 11:
                inventory = session.PlayerEntity.GetItemBySlotAndType(equipmentInfoPacket.Slot, InventoryType.Costume);
                break;

            case 12:
                await session.EmitEventAsync(new FamilyWarehouseShowItemEvent
                {
                    Slot = equipmentInfoPacket.Slot
                });
                return;
        }

        if (inventory == null)
        {
            return;
        }

        if (inventory.ItemInstance.GameItem.EquipmentSlot == EquipmentType.Sp)
        {
            if (inventory.ItemInstance.GameItem.IsPartnerSpecialist)
            {
                session.SendPartnerSpecialistInfo(inventory.ItemInstance);
            }
            else if(inventory.ItemInstance.GameItem.ItemSubType == (int)ItemInstanceType.BoxInstance && inventory.ItemInstance.GameItem.ItemType == ItemType.Box)
            {

                session.SendEInfoPacket(inventory.ItemInstance, _itemManager, _algorithm);
            }
            else
            {
                session.SendSpecialistCardInfo(inventory.ItemInstance, _algorithm);
            }

            return;
        }

        session.SendEInfoPacket(inventory.ItemInstance, _itemManager, _algorithm);
    }
}