using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using PhoenixLib.Logging;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsAPI.Game.Extensions.PacketGeneration;
using WingsAPI.Packets.Enums;
using WingsEmu.DTOs.Items;
using WingsEmu.Game;
using WingsEmu.Game._enum;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Inventory;
using WingsEmu.Game.Inventory.Event;
using WingsEmu.Game.Items;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;

namespace NosEmu.Plugins.BasicImplementations.Event.Items;

public class ItemSumEventHandler : IAsyncEventProcessor<ItemSumEvent>
{
    private readonly IGameLanguageService _gameLanguage;
    private readonly ItemSumConfiguration _itemSumConfiguration;
    private readonly IRandomGenerator _randomGenerator;

    public ItemSumEventHandler(IRandomGenerator randomGenerator, IGameLanguageService gameLanguage, ItemSumConfiguration itemSumConfiguration)
    {
        _randomGenerator = randomGenerator;
        _gameLanguage = gameLanguage;
        _itemSumConfiguration = itemSumConfiguration;
    }

    public async Task HandleAsync(ItemSumEvent e, CancellationToken cancellation)
    {
        IClientSession session = e.Sender;

        if (e.LeftItem.ItemInstance.Type != ItemInstanceType.WearableInstance)
        {
            return;
        }

        if (e.RightItem.ItemInstance.Type != ItemInstanceType.WearableInstance)
        {
            return;
        }

        GameItemInstance leftItem = e.LeftItem.ItemInstance;
        GameItemInstance rightItem = e.RightItem.ItemInstance;

        const int sandVnum = (int)ItemVnums.DONA_RIVER_SAND;
        int totalUpgrade = leftItem.Upgrade + rightItem.Upgrade;

        // They are the same item
        if (leftItem == rightItem)
        {
            return;
        }

        // Items aren't compatible to sum
        if (totalUpgrade >= 6)
        {
            Log.Debug($"[ITEM_SUM] Sum total of {totalUpgrade.ToString()}");
            return;
        }

        // Neither gloves or boots
        if ((rightItem.GameItem.EquipmentSlot != EquipmentType.Gloves || leftItem.GameItem.EquipmentSlot != EquipmentType.Gloves)
            && (leftItem.GameItem.EquipmentSlot != EquipmentType.Boots || rightItem.GameItem.EquipmentSlot != EquipmentType.Boots))
        {
            Log.Debug("[ITEM_SUM] At least one of the items is neither boots or gloves.");
            return;
        }

        // They aren't the same type of equipment
        if (rightItem.GameItem.EquipmentSlot != leftItem.GameItem.EquipmentSlot)
        {
            Log.Debug("[ITEM_SUM] They are not the same type of equipment.");
            return;
        }

        ItemSumMats itemSumMats = _itemSumConfiguration.FirstOrDefault(i => i.SumUpgrade == totalUpgrade);
        if (itemSumMats == null)
        {
            return;
        }

        // No money
        if (session.PlayerEntity.Gold < itemSumMats.Gold)
        {
            Log.Debug($"[ITEM_SUM] Not enough gold. Required: {itemSumMats.Gold.ToString()}");
            return;
        }

        // No sand
        if (!session.PlayerEntity.HasItem(sandVnum, (short)itemSumMats.RiverSand))
        {
            Log.Debug($"[ITEM_SUM] Not enough sand. Required: {itemSumMats.RiverSand.ToString()}");
            return;
        }

        var nonSumItems = new List<int> { 4833, 4834, 4870, 4989, 4995, 4996, 4997, 4998, 25578, 25579, 25730, 25731 };

        if (nonSumItems.Contains(rightItem.ItemVNum) || nonSumItems.Contains(leftItem.ItemVNum))
        {
            session.SendMsg(_gameLanguage.GetLanguage(GameDialogKey.CANNOT_SUM_THIS_RESISTANCE, session.UserLanguage), MsgMessageType.Middle);
            return;
        }

        await session.RemoveItemFromInventory(sandVnum, (short)itemSumMats.RiverSand);
        session.PlayerEntity.Gold -= itemSumMats.Gold;

        InventoryItem rightItemToRemove = session.PlayerEntity.GetItemBySlotAndType(e.RightItem.Slot, e.RightItem.InventoryType);
        InventoryItem leftItemInstance = session.PlayerEntity.GetItemBySlotAndType(e.LeftItem.Slot, e.LeftItem.InventoryType);

        int rnd = _randomGenerator.RandomNumber();
        if (rnd < itemSumMats.SuccessChance)
        {
            await session.EmitEventAsync(new ItemSummedEvent
            {
                LeftItem = leftItem,
                RightItem = rightItem,
                Succeed = true,
                SumLevel = itemSumMats.SumUpgrade
            });

            leftItem.Upgrade += (byte)(rightItem.Upgrade + 1);
            leftItem.DarkResistance += (short)(rightItem.DarkResistance + rightItem.GameItem.DarkResistance);
            leftItem.LightResistance += (short)(rightItem.LightResistance + rightItem.GameItem.LightResistance);
            leftItem.WaterResistance += (short)(rightItem.WaterResistance + rightItem.GameItem.WaterResistance);
            leftItem.FireResistance += (short)(rightItem.FireResistance + rightItem.GameItem.FireResistance);

            await session.RemoveItemFromInventory(item: rightItemToRemove);

            session.SendPdtiPacket(PdtiType.ResistancesAreFused, leftItem.ItemVNum, 1, e.RightItem.Slot, leftItem.Upgrade, 0);

            session.SendMsg(_gameLanguage.GetLanguage(GameDialogKey.SUM_MESSAGE_SUCCESS, session.UserLanguage), MsgMessageType.Middle);
            session.SendChatMessage(_gameLanguage.GetLanguage(GameDialogKey.SUM_MESSAGE_SUCCESS, session.UserLanguage), ChatMessageColorType.Green);
            session.BroadcastEffectInRange(EffectType.UpgradeSuccess);
            session.SendSound(SoundType.CRAFTING_SUCCESS);

            session.SendInventoryAddPacket(leftItemInstance);
        }
        else
        {
            session.SendMsg(_gameLanguage.GetLanguage(GameDialogKey.SUM_MESSAGE_FAILED, session.UserLanguage), MsgMessageType.Middle);
            session.SendChatMessage(_gameLanguage.GetLanguage(GameDialogKey.SUM_MESSAGE_FAILED, session.UserLanguage), ChatMessageColorType.Red);
            session.SendSound(SoundType.CRAFTING_FAILED);

            await session.EmitEventAsync(new ItemSummedEvent
            {
                LeftItem = leftItem,
                RightItem = rightItem,
                Succeed = false,
                SumLevel = itemSumMats.SumUpgrade
            });
        }

        session.PlayerEntity.BroadcastEndDancingGuriPacket();
        session.RefreshGold();
        session.SendShopEndPacket(ShopEndType.Npc);
    }
}