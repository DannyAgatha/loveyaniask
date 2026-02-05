using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsAPI.Game.Extensions.Families;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsAPI.Game.Extensions.ItemExtension.Item;
using WingsAPI.Packets.Enums;
using WingsAPI.Packets.Enums.BattlePass;
using WingsEmu.DTOs.Items;
using WingsEmu.Game;
using WingsEmu.Game._enum;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Algorithm;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Inventory;
using WingsEmu.Game.Items;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Managers.StaticData;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Pity;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;
using WingsEmu.Packets.Enums.Families;

namespace NosEmu.Plugins.BasicImplementations.Event.Characters;

public class SpUpgradeEventHandler : IAsyncEventProcessor<SpUpgradeEvent>
{
    private readonly IItemsManager _itemsManager;
    private readonly IGameLanguageService _languageService;
    private readonly IRandomGenerator _randomGenerator;
    private readonly SpUpgradeConfiguration _spConfiguration;
    private readonly SpPerfectEventHandler _spPerfectHandler;
    private readonly IEvtbConfiguration _evtbConfiguration;
    private readonly PityConfiguration _pityConfiguration;
    private readonly ISessionManager _sessionManager; 
    private readonly ICharacterAlgorithm _algorithm;

    public SpUpgradeEventHandler(IRandomGenerator randomGenerator, SpUpgradeConfiguration spConfiguration, SpPerfectEventHandler spPerfectHandler, IGameLanguageService languageService,
        IItemsManager itemsManager, IEvtbConfiguration evtbConfiguration, PityConfiguration pityConfiguration, ISessionManager sessionManager, ICharacterAlgorithm algorithm)
    {
        _randomGenerator = randomGenerator;
        _spConfiguration = spConfiguration;
        _spPerfectHandler = spPerfectHandler;
        _languageService = languageService;
        _itemsManager = itemsManager;
        _evtbConfiguration = evtbConfiguration;
        _pityConfiguration = pityConfiguration;
        _sessionManager = sessionManager;
        _algorithm = algorithm;
    }

    public async Task HandleAsync(SpUpgradeEvent e, CancellationToken cancellation)
    {
        IClientSession session = e.Sender;
        bool isPremiumScroll = e.IsPremium;
        
        if (e.InventoryItem.ItemInstance.Type != ItemInstanceType.SpecialistInstance)
        {
            session.PlayerEntity.CancelUpgrade = true;
            return;
        }

        GameItemInstance sp = e.InventoryItem.ItemInstance;

        if (sp.GameItem.IsPartnerSpecialist)
        {
            session.PlayerEntity.CancelUpgrade = true;
            return;
        }

        if (sp.Rarity == -2)
        {
            session.PlayerEntity.CancelUpgrade = true;
            return;
        }

        UpgradeConfiguration configuration = _spConfiguration.FirstOrDefault(upgradeConfiguration =>
            upgradeConfiguration.SpUpgradeRange.Minimum <= sp.Upgrade
            && sp.Upgrade < upgradeConfiguration.SpUpgradeRange.Maximum);

        if (configuration == null)
        {
            session.PlayerEntity.CancelUpgrade = true;
            return;
        }

        int itemVNum = sp.ItemVNum;

        if (e.IsFree)
        {
            await SpUpgrade(configuration, session, e.InventoryItem, sp, configuration.SpecialItemsNeeded, true, true);
            switch (sp.ItemVNum)
            {
                case (short)ItemVnums.CHICKEN_SP:
                    await session.RemoveItemFromInventory((short)ItemVnums.SCROLL_CHICKEN);
                    break;
                case (short)ItemVnums.PYJAMA_SP:
                    await session.RemoveItemFromInventory((short)ItemVnums.SCROLL_PYJAMA);
                    break;
                case (short)ItemVnums.PIRATE_SP:
                    await session.RemoveItemFromInventory((short)ItemVnums.SCROLL_PIRATE);
                    break;
            }

            return;
        }

        if (sp.SpLevel < configuration.SpLevelNeeded)
        {
            session.PlayerEntity.CancelUpgrade = true;
            session.SendMsg(_languageService.GetLanguageFormat(GameDialogKey.INFORMATION_SHOUTMESSAGE_SP_LVL_LOW, session.UserLanguage, configuration.SpLevelNeeded.ToString()), MsgMessageType.Middle);
            return;
        }
        
        long goldNeeded = isPremiumScroll ? configuration.GoldNeeded / 2 : configuration.GoldNeeded;

        if (session.PlayerEntity.Gold < goldNeeded)
        {
            session.PlayerEntity.CancelUpgrade = true;
            return;
        }
        
        int featherNeeded = isPremiumScroll ? configuration.FeatherNeeded / 2 : configuration.FeatherNeeded;

        if (!session.PlayerEntity.HasItem((int)ItemVnums.ANGEL_FEATHER, (short)featherNeeded))
        {
            session.PlayerEntity.CancelUpgrade = true;
            string itemName = _itemsManager.GetItem((int)ItemVnums.ANGEL_FEATHER).GetItemName(_languageService, session.UserLanguage);
            session.SendMsg(_languageService.GetLanguageFormat(GameDialogKey.INVENTORY_SHOUTMESSAGE_NOT_ENOUGH_ITEMS, session.UserLanguage, featherNeeded, itemName),
                MsgMessageType.Middle);
            return;
        }
        
        int fullMoonNeeded = isPremiumScroll ? configuration.FullMoonsNeeded / 2 : configuration.FullMoonsNeeded;

        if (!session.PlayerEntity.HasItem((int)ItemVnums.FULL_MOON_CRYSTAL, (short)fullMoonNeeded))
        {
            session.PlayerEntity.CancelUpgrade = true;
            string itemName = _itemsManager.GetItem((int)ItemVnums.FULL_MOON_CRYSTAL).GetItemName(_languageService, session.UserLanguage);
            session.SendMsg(_languageService.GetLanguageFormat(GameDialogKey.INVENTORY_SHOUTMESSAGE_NOT_ENOUGH_ITEMS, session.UserLanguage, fullMoonNeeded, itemName),
                MsgMessageType.Middle);
            return;
        }

        var specialItems = new List<SpecialItem>();

        foreach (SpecialItem specialItem in configuration.SpecialItemsNeeded)
        {
            if (specialItem.SpVnums.Count > 0 && !specialItem.SpVnums.Contains(sp.ItemVNum))
            {
                continue;
            }

            if (!session.PlayerEntity.HasItem(specialItem.ItemVnum, (short)specialItem.Amount))
            {
                session.PlayerEntity.CancelUpgrade = true;
                string itemName = _itemsManager.GetItem(specialItem.ItemVnum).GetItemName(_languageService, session.UserLanguage);
                session.SendMsg(_languageService.GetLanguageFormat(GameDialogKey.INVENTORY_SHOUTMESSAGE_NOT_ENOUGH_ITEMS, session.UserLanguage, specialItem.Amount, itemName),
                    MsgMessageType.Middle);
                return;
            }

            specialItems.Add(specialItem);
        }

        bool isProtected = e.UpgradeProtection == UpgradeProtection.Protected;

        int scrollVNum;

        if (!isPremiumScroll)
        {
            scrollVNum = configuration.ScrollVnum;
        }
        else
        {
            scrollVNum = configuration.ScrollVnum switch
            {
                (short)ItemVnums.LOWER_SP_SCROLL => (short)ItemVnums.LOWER_SP_PROTECTION_SCROLL_PREMIUM,
                (short)ItemVnums.HIGHER_SP_SCROLL => (short)ItemVnums.HIGHER_SP_PROTECTION_SCROLL_PREMIUM,
                (short)ItemVnums.DRAGON_SP_SCROLL => (short)ItemVnums.DRAGON_SP_PROTECTION_SCROLL_PREMIUM,
                _ => configuration.ScrollVnum
            };
        }

        scrollVNum = scrollVNum switch
        {
            (short)ItemVnums.LOWER_SP_SCROLL when session.PlayerEntity.HasItem((short)ItemVnums.LOWER_SP_SCROLL_LIMITED) => (short)ItemVnums.LOWER_SP_SCROLL_LIMITED,
            (short)ItemVnums.HIGHER_SP_SCROLL when session.PlayerEntity.HasItem((short)ItemVnums.HIGHER_SP_SCROLL_LIMITED) => (short)ItemVnums.HIGHER_SP_SCROLL_LIMITED,
            (short)ItemVnums.LOWER_SP_PROTECTION_SCROLL_PREMIUM when session.PlayerEntity.HasItem((short)ItemVnums.LOWER_SP_PROTECTION_SCROLL_PREMIUM_LIMITED) => (short)ItemVnums.LOWER_SP_PROTECTION_SCROLL_PREMIUM_LIMITED,
            (short)ItemVnums.HIGHER_SP_PROTECTION_SCROLL_PREMIUM when session.PlayerEntity.HasItem((short)ItemVnums.HIGHER_SP_PROTECTION_SCROLL_PREMIUM_LIMITED) => (short)ItemVnums.HIGHER_SP_PROTECTION_SCROLL_PREMIUM_LIMITED,
            (short)ItemVnums.DRAGON_SP_SCROLL when session.PlayerEntity.HasItem((short)ItemVnums.DRAGON_SP_SCROLL_LIMITED) => (short)ItemVnums.DRAGON_SP_SCROLL_LIMITED,
            (short)ItemVnums.DRAGON_SP_PROTECTION_SCROLL_PREMIUM when session.PlayerEntity.HasItem((short)ItemVnums.DRAGON_SP_PROTECTION_SCROLL_PREMIUM_LIMITED) => (short)ItemVnums.DRAGON_SP_PROTECTION_SCROLL_PREMIUM_LIMITED,
            _ => scrollVNum
        };

        if (isProtected && !session.PlayerEntity.HasItem(scrollVNum))
        {
            return;
        }
        
        session.PlayerEntity.RemoveGold(goldNeeded);
        if (!isProtected)
        {
            await session.EmitEventAsync(new IncreaseBattlePassObjectiveEvent(MissionType.SpendXGoldToNpc, goldNeeded));
        }
        await session.RemoveItemFromInventory((int)ItemVnums.ANGEL_FEATHER, (short)featherNeeded);
        await session.RemoveItemFromInventory((int)ItemVnums.FULL_MOON_CRYSTAL, (short)fullMoonNeeded);

        if (isProtected)
        {
            await session.RemoveItemFromInventory(scrollVNum);
        }

        await SpUpgrade(configuration, session, e.InventoryItem, sp, specialItems, isProtected, false);
    }

    private async Task SpUpgrade(UpgradeConfiguration configuration, IClientSession session, InventoryItem spItem, ItemInstanceDTO sp,
        List<SpecialItem> specialItems, bool isProtected, bool isFree)
    {
        byte originalUpgrade = sp.Upgrade;

        var randomBag = new RandomBag<SpUpgradeResult>(_randomGenerator);
        
        randomBag.AddEntry(SpUpgradeResult.Succeed, configuration.SuccessChance * (1 + _evtbConfiguration.GetValueForEventType(EvtbType.INCREASE_CHANCE_UPGRADE_SPECIALIST) * 0.01));
        
        randomBag.AddEntry(SpUpgradeResult.Fail, 100 - configuration.SuccessChance - configuration.DestroyChance);
        
        if (configuration.DestroyChance > 0)
        {
            randomBag.AddEntry(SpUpgradeResult.Break, configuration.DestroyChance);
        }

        SpUpgradeResult upgradeResult = randomBag.GetRandom();
        
        if (upgradeResult != SpUpgradeResult.Succeed)
        {
            if (sp.IsPityUpgradeItem(PityType.Specialist, _pityConfiguration))
            {
                sp.PityCounter[(int)PityType.Specialist] = 0;
                upgradeResult = SpUpgradeResult.Succeed;
                session.SendChatMessage(session.GetLanguage(GameDialogKey.PITY_CHATMESSAGE_SUCCESS), ChatMessageColorType.Green);
            }
            else
            {
                sp.PityCounter[(int)PityType.Specialist]++;
                (int, int) maxFailCounter = sp.ItemPityMaxFailCounter(PityType.Specialist, _pityConfiguration);
                session.SendChatMessage(session.GetLanguageFormat(GameDialogKey.PITY_CHATMESSAGE_FAIL, maxFailCounter.Item1, maxFailCounter.Item2), ChatMessageColorType.Green);

            }
        }
        else
        {
            sp.PityCounter[(int)PityType.Specialist] = 0;
        }
        
        switch (upgradeResult)
        {
            case SpUpgradeResult.Break when isProtected:
                session.SendEffect(EffectType.UpgradeFail);
                _spPerfectHandler.SendBothMessages(session, _languageService.GetLanguage(GameDialogKey.UPGRADE_MESSAGE_SP_FAILED_SAVED, session.UserLanguage), true);
                break;
            case SpUpgradeResult.Break:
                sp.Rarity = -2;
                session.SendInventoryAddPacket(spItem);
                session.SendShopEndPacket(ShopEndType.Npc);
                _spPerfectHandler.SendBothMessages(session, _languageService.GetLanguage(GameDialogKey.UPGRADE_MESSAGE_SP_DESTROYED, session.UserLanguage), true);
                await RemoveSpecialItems(session, specialItems);
                break;
            case SpUpgradeResult.Succeed:
            {
                session.SendEffect(EffectType.UpgradeSuccess);
                sp.Upgrade++;
                session.SendInventoryAddPacket(spItem);
                session.SendEffect(EffectType.UpgradeSuccess);
                _spPerfectHandler.SendBothMessages(session, _languageService.GetLanguage(GameDialogKey.UPGRADE_MESSAGE_SP_SUCCESS, session.UserLanguage), false);

                if (sp.Upgrade > 7)
                {
                    await session.FamilyAddLogAsync(FamilyLogType.ItemUpgraded, session.PlayerEntity.Name, sp.ItemVNum.ToString(), sp.Upgrade.ToString());
                }
                
                if (sp.Upgrade >= 14)
                {
                    foreach (IClientSession activeSession in _sessionManager.Sessions)
                    {
                        activeSession?.SendMsgi2(ChatMessageColorType.OrangeWhisper, Game18NConstString.HasSuccessfullyUpgraded, 15, session.PlayerEntity.Name, sp.ItemVNum.ToString(), sp.Upgrade);
                        activeSession?.SendBroadcastUpgradeItemPacket(session, 16, 4, spItem.ItemInstance, _algorithm);
                    }
                }

                await RemoveSpecialItems(session, specialItems);
                break;
            }

            case SpUpgradeResult.Fail:
            {
                _spPerfectHandler.SendBothMessages(session, _languageService.GetLanguage(GameDialogKey.UPGRADE_MESSAGE_SP_FAILED, session.UserLanguage), true);
                if (!isProtected)
                {
                    await RemoveSpecialItems(session, specialItems);
                }

                break;
            }
        }
        
        await session.EmitEventAsync(new IncreaseBattlePassObjectiveEvent(MissionType.UpgradeSpCardOrGearXTime));

        await session.EmitEventAsync(new SpUpgradedEvent
        {
            IsProtected = isProtected,
            Sp = sp,
            OriginalUpgrade = originalUpgrade,
            UpgradeMode = isFree ? UpgradeMode.Free : UpgradeMode.Normal,
            UpgradeResult = upgradeResult
        });

        if (isProtected)
        {
            session.SendShopEndPacket(ShopEndType.Item);
            return;
        }

        session.SendShopEndPacket(ShopEndType.Npc);
    }

    private async Task RemoveSpecialItems(IClientSession session, List<SpecialItem> specialItems)
    {
        foreach (SpecialItem specialItem in specialItems)
        {
            await session.RemoveItemFromInventory(specialItem.ItemVnum, (short)specialItem.Amount);
        }
    }
}