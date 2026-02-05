using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsAPI.Game.Extensions.Families;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsAPI.Game.Extensions.ItemExtension.Item;
using WingsAPI.Game.Extensions.PacketGeneration;
using WingsAPI.Packets.Enums;
using WingsAPI.Packets.Enums.BattlePass;
using WingsEmu.DTOs.Items;
using WingsEmu.Game;
using WingsEmu.Game._enum;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Inventory;
using WingsEmu.Game.Items;
using WingsEmu.Game.Managers.StaticData;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Pity;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;
using WingsEmu.Packets.Enums.Families;

namespace NosEmu.Plugins.BasicImplementations.Event.Characters;

public class UpgradeItemEventHandler : IAsyncEventProcessor<UpgradeItemEvent>
{
    private readonly IGameLanguageService _gameLanguage;
    private readonly IItemsManager _itemManager;
    private readonly IRandomGenerator _randomGenerator;
    private readonly UpgradeNormalItemConfiguration _upgradeNormalItemConfiguration;
    private readonly UpgradePhenomenalItemConfiguration _upgradePhenomenalItemConfiguration;
    private readonly IEvtbConfiguration _evtbConfiguration;
    private readonly PityConfiguration _pityConfiguration;

    public UpgradeItemEventHandler(IGameLanguageService gameLanguage, IItemsManager itemManager, IRandomGenerator randomGenerator, UpgradeNormalItemConfiguration upgradeNormalItemConfiguration,
        UpgradePhenomenalItemConfiguration upgradePhenomenalItemConfiguration, IEvtbConfiguration evtbConfiguration, PityConfiguration pityConfiguration)
    {
        _gameLanguage = gameLanguage;
        _itemManager = itemManager;
        _randomGenerator = randomGenerator;
        _upgradeNormalItemConfiguration = upgradeNormalItemConfiguration;
        _upgradePhenomenalItemConfiguration = upgradePhenomenalItemConfiguration;
        _evtbConfiguration = evtbConfiguration;
        _pityConfiguration = pityConfiguration;
    }

    public async Task HandleAsync(UpgradeItemEvent e, CancellationToken cancellation)
    {
        IClientSession session = e.Sender;
        GameItemInstance item = e.Inv.ItemInstance;
        if (item.Type != ItemInstanceType.WearableInstance)
        {
            session.PlayerEntity.CancelUpgrade = true;
            return;
        }

        FixedUpMode hasAmulet = e.HasAmulet;
        UpgradeMode mode = e.Mode;
        UpgradeProtection protection = e.Protection;
        bool isAutoUpgrade = e.IsAutoUpgrade;
        
        ItemVnums cella = ItemVnums.CELLA;
        ItemVnums gem = item.Upgrade < 5 ? ItemVnums.SOUL_GEM : ItemVnums.COMPLETE_SOUL_GEM;
        ItemVnums usedScroll;

        if (mode == UpgradeMode.Reduced)
        {
            usedScroll = ItemVnums.EQ_GOLD_SCROLL;
        }
        else
        {
            usedScroll = session.PlayerEntity.HasItem((int)ItemVnums.EQ_NORMAL_SCROLL_EVENT)
                ? ItemVnums.EQ_NORMAL_SCROLL_EVENT
                : ItemVnums.EQ_NORMAL_SCROLL;
        }

        double priceFactor = e.IsPremium ? 0.5 : mode == UpgradeMode.Reduced ? 0.5 : 1.0;

        if (!session.HasCurrentMapInstance)
        {
            session.PlayerEntity.CancelUpgrade = true;
            return;
        }

        if (item.Upgrade >= 10)
        {
            session.PlayerEntity.CancelUpgrade = true;
            session.PlayerEntity.BroadcastEndDancingGuriPacket();
            return;
        }

        if (item.Rarity > 8)
        {
            session.PlayerEntity.CancelUpgrade = true;
            session.PlayerEntity.BroadcastEndDancingGuriPacket();
            return;
        }

        // The item has different configurations depending if it's r8 or not
        // Not anymore.
        /*if (item.Rarity == 8)
        {
            upgradeItemConfiguration = _upgradePhenomenalItemConfiguration;
        }
        else
        {
            upgradeItemConfiguration = _upgradeNormalItemConfiguration;
        }*/
        UpgradeItemConfiguration upgradeItemConfiguration = _upgradeNormalItemConfiguration;

        UpgradeItemStats upgradeItemStats = upgradeItemConfiguration.FirstOrDefault(s => s.Upgrade == item.Upgrade);
        if (upgradeItemStats == null)
        {
            session.PlayerEntity.CancelUpgrade = true;
            session.PlayerEntity.BroadcastEndDancingGuriPacket();
            return;
        }

        if (item.IsFixed)
        {
            if (session.PlayerEntity.Amulet != null && session.PlayerEntity.Amulet.GameItem.Id == (short)ItemVnums.AMULET_OF_REINFORCEMENT)
            {
                hasAmulet = FixedUpMode.HasAmulet;
            }
            else
            {
                session.SendChatMessage(_gameLanguage.GetLanguage(GameDialogKey.GAMBLING_CHATMESSAGE_ITEM_IS_FIXED, session.UserLanguage), ChatMessageColorType.Yellow);
                session.SendShopEndPacket(ShopEndType.Npc);
                session.PlayerEntity.BroadcastEndDancingGuriPacket();
                session.PlayerEntity.CancelUpgrade = true;
                return;
            }
        }

        long totalPrice = (long)(upgradeItemStats.Gold * priceFactor);
        if (session.PlayerEntity.Gold < totalPrice)
        {
            session.SendChatMessage(_gameLanguage.GetLanguage(GameDialogKey.INTERACTION_MESSAGE_NOT_ENOUGH_GOLD, session.UserLanguage), ChatMessageColorType.Yellow);
            session.PlayerEntity.BroadcastEndDancingGuriPacket();
            session.PlayerEntity.CancelUpgrade = true;
            return;
        }

        if (session.PlayerEntity.CountItemWithVnum((short)cella) < upgradeItemStats.Cella * priceFactor)
        {
            session.SendChatMessage(
                _gameLanguage.GetLanguageFormat(GameDialogKey.INVENTORY_SHOUTMESSAGE_NOT_ENOUGH_ITEMS, session.UserLanguage, upgradeItemStats.Cella * priceFactor,
                    _itemManager.GetItem((short)cella).GetItemName(_gameLanguage, session.UserLanguage)),ChatMessageColorType.Yellow);
            session.PlayerEntity.BroadcastEndDancingGuriPacket();
            session.PlayerEntity.CancelUpgrade = true;
            return;
        }

        if (protection == UpgradeProtection.Protected && session.PlayerEntity.CountItemWithVnum((short)usedScroll) < 1)
        {
            session.SendChatMessage(
                _gameLanguage.GetLanguageFormat(GameDialogKey.INVENTORY_SHOUTMESSAGE_NOT_ENOUGH_ITEMS, session.UserLanguage, 1,
                    _itemManager.GetItem((short)usedScroll).GetItemName(_gameLanguage, session.UserLanguage)),ChatMessageColorType.Yellow);
            session.PlayerEntity.BroadcastEndDancingGuriPacket();
            session.PlayerEntity.CancelUpgrade = true;
            return;
        }

        if (session.PlayerEntity.CountItemWithVnum((short)gem) < upgradeItemStats.Gem)
        {
            session.SendChatMessage(
                _gameLanguage.GetLanguageFormat(GameDialogKey.INVENTORY_SHOUTMESSAGE_NOT_ENOUGH_ITEMS, session.UserLanguage, upgradeItemStats.Gem,
                    _itemManager.GetItem((short)gem).GetItemName(_gameLanguage, session.UserLanguage)), ChatMessageColorType.Yellow);
            session.PlayerEntity.BroadcastEndDancingGuriPacket();
            session.PlayerEntity.CancelUpgrade = true;
            return;
        }

        await session.RemoveItemFromInventory((short)gem, upgradeItemStats.Gem);

        if (protection == UpgradeProtection.Protected)
        {
            await session.RemoveItemFromInventory((short)usedScroll);
            session.SendShopEndPacket(session.PlayerEntity.CountItemWithVnum((short)usedScroll) < 1 ? ShopEndType.Item : ShopEndType.Player);
        }

        if (hasAmulet == FixedUpMode.HasAmulet && item.IsFixed)
        {
            GameItemInstance amulet = session.PlayerEntity.Amulet;
            InventoryItem amuletInEq = session.PlayerEntity.GetInventoryItemFromEquipmentSlot(EquipmentType.Amulet);
            amulet.DurabilityPoint -= 1;
            session.SendAmuletBuffPacket(amulet);
            if (amulet.DurabilityPoint <= 0)
            {
                await session.RemoveItemFromInventory(item: amuletInEq, isEquiped: true);
                session.RefreshEquipment();
                session.SendModal(_gameLanguage.GetLanguage(GameDialogKey.GAMBLING_INFO_AMULET_DESTROYED, session.UserLanguage), ModalType.Confirm);
            }
        }

        session.PlayerEntity.Gold -= totalPrice;
        await session.RemoveItemFromInventory((short)cella, (short)(upgradeItemStats.Cella * priceFactor));
        session.RefreshGold();
        await session.EmitEventAsync(new IncreaseBattlePassObjectiveEvent(MissionType.UpgradeSpCardOrGearXTime));
        var randomBag = new RandomBag<UpgradeResult>(_randomGenerator);
        
        randomBag.AddEntry(UpgradeResult.Succeed, (1000 - upgradeItemStats.UpFail - upgradeItemStats.UpFix) * (1 + _evtbConfiguration.GetValueForEventType(EvtbType.INCREASE_CHANCE_UPGRADE_EQUIPMENT) * 0.01));
        randomBag.AddEntry(UpgradeResult.Fail, item.IsFixed ? upgradeItemStats.UpFail + upgradeItemStats.UpFix : upgradeItemStats.UpFail);

        if (!item.IsFixed)
        {
            randomBag.AddEntry(UpgradeResult.Fixed, upgradeItemStats.UpFix);
        }

        UpgradeResult upgradeResult = randomBag.GetRandom();
        
        if (upgradeResult != UpgradeResult.Succeed)
        {
            if (item.IsPityUpgradeItem(PityType.Equipment, _pityConfiguration))
            {
                item.PityCounter[(int)PityType.Equipment] = 0;
                upgradeResult = UpgradeResult.Succeed;
                session.SendChatMessage(session.GetLanguage(GameDialogKey.PITY_CHATMESSAGE_SUCCESS), ChatMessageColorType.Green);
            }
            else
            {
                item.PityCounter[(int)PityType.Equipment]++;
                (int, int) maxFailCounter = item.ItemPityMaxFailCounter(PityType.Equipment, _pityConfiguration);
                session.SendChatMessage(session.GetLanguageFormat(GameDialogKey.PITY_CHATMESSAGE_FAIL, maxFailCounter.Item1, maxFailCounter.Item2), ChatMessageColorType.Green);
            }
        }
        else
        {
            item.PityCounter[(int)PityType.Equipment] = 0;
        }

        switch (upgradeResult)
        {
            case UpgradeResult.Fixed:
                item.IsFixed = true;
                if (!isAutoUpgrade)
                {
                    session.BroadcastEffectInRange(EffectType.UpgradeFail);
                    session.SendChatMessage(_gameLanguage.GetLanguage(GameDialogKey.UPGRADE_MESSAGE_FIXED, session.UserLanguage), ChatMessageColorType.Red);
                    session.SendMsg(_gameLanguage.GetLanguage(GameDialogKey.UPGRADE_MESSAGE_FIXED, session.UserLanguage), MsgMessageType.Middle);
                }

                await session.EmitEventAsync(new ItemUpgradedEvent
                {
                    Item = item,
                    Mode = e.Mode,
                    Protection = e.Protection,
                    HasAmulet = e.HasAmulet == FixedUpMode.HasAmulet,
                    OriginalUpgrade = item.Upgrade,
                    Result = UpgradeResult.Fixed,
                    TotalPrice = totalPrice
                });
                break;
            case UpgradeResult.Fail:
                if (protection == UpgradeProtection.None)
                {
                    if (!isAutoUpgrade)
                    {
                        session.SendChatMessage(_gameLanguage.GetLanguage(GameDialogKey.UPGRADE_MESSAGE_FAILED, session.UserLanguage), ChatMessageColorType.Red);
                        session.SendMsg(_gameLanguage.GetLanguage(GameDialogKey.UPGRADE_MESSAGE_FAILED, session.UserLanguage), MsgMessageType.Middle);
                    }
                    
                    await session.RemoveItemFromInventory(item: e.Inv);
                }
                else
                {
                    if (!isAutoUpgrade)
                    {
                        session.BroadcastEffectInRange(EffectType.UpgradeFail);
                        session.SendChatMessage(_gameLanguage.GetLanguage(GameDialogKey.INFORMATION_CHATMESSAGE_SCROLL_PROTECT_USED, session.UserLanguage), ChatMessageColorType.Red);
                        session.SendMsg(_gameLanguage.GetLanguage(GameDialogKey.UPGRADE_MESSAGE_FAILED_ITEM_SAVED, session.UserLanguage), MsgMessageType.Middle);
                    }
                }

                await session.EmitEventAsync(new ItemUpgradedEvent
                {
                    Item = item,
                    Mode = e.Mode,
                    Protection = e.Protection,
                    HasAmulet = e.HasAmulet == FixedUpMode.HasAmulet,
                    OriginalUpgrade = item.Upgrade,
                    Result = UpgradeResult.Fail,
                    TotalPrice = totalPrice
                });
                break;
            default:
            {
                if (!isAutoUpgrade)
                {
                    session.BroadcastEffectInRange(EffectType.UpgradeSuccess);
                    session.SendChatMessage(_gameLanguage.GetLanguage(GameDialogKey.UPGRADE_MESSAGE_SUCCESS, session.UserLanguage), ChatMessageColorType.Green);
                    session.SendMsg(_gameLanguage.GetLanguage(GameDialogKey.UPGRADE_MESSAGE_SUCCESS, session.UserLanguage), MsgMessageType.Middle);
                };
                
                item.Upgrade++;
                if (item.Upgrade > 4)
                {
                    await session.FamilyAddLogAsync(FamilyLogType.ItemUpgraded, session.PlayerEntity.Name, item.ItemVNum.ToString(), item.Upgrade.ToString());
                }

                InventoryItem itemInstance = session.PlayerEntity.GetItemBySlotAndType(e.Inv.Slot, InventoryType.Equipment);

                session.SendInventoryAddPacket(itemInstance);

                await session.EmitEventAsync(new ItemUpgradedEvent
                {
                    Item = item,
                    Mode = e.Mode,
                    Protection = e.Protection,
                    HasAmulet = e.HasAmulet == FixedUpMode.HasAmulet,
                    OriginalUpgrade = (short)(item.Upgrade - 1),
                    Result = UpgradeResult.Succeed,
                    TotalPrice = totalPrice
                });
                break;
            }
        }

        session.PlayerEntity.BroadcastEndDancingGuriPacket();
        session.SendShopEndPacket(ShopEndType.Npc);
    }
}