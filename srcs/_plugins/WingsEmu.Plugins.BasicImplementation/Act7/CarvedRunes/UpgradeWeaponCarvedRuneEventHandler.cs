using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsAPI.Data.CarvedRune;
using WingsAPI.Game.Extensions.ItemExtension;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsAPI.Packets.Enums;
using WingsAPI.Packets.Enums.Chat;
using WingsEmu.DTOs.BCards;
using WingsEmu.Game;
using WingsEmu.Game._enum;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Act7.CarvedRunes;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Configurations.Act7.CarvedRunes;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Inventory;
using WingsEmu.Game.Items;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;

namespace NosEmu.Plugins.BasicImplementations.Act7.CarvedRunes;

public class UpgradeWeaponCarvedRuneEventHandler : IAsyncEventProcessor<UpgradeWeaponCarvedRuneEvent>
{
    private readonly CarvedRuneUpgradeConfiguration _carvedRuneUpgradeConfiguration;
    private readonly IGameLanguageService _gameLanguageService;
    private readonly IRandomGenerator _randomGenerator;
    private readonly WeaponRuneCardConfiguration _weaponRuneCardConfiguration;
    private readonly IEvtbConfiguration _evtbConfiguration;

    public UpgradeWeaponCarvedRuneEventHandler(CarvedRuneUpgradeConfiguration carvedRuneUpgradeConfiguration,
        IGameLanguageService gameLanguageService, IRandomGenerator randomGenerator,
        WeaponRuneCardConfiguration weaponRuneCardConfiguration, IEvtbConfiguration evtbConfiguration)
    {
        _carvedRuneUpgradeConfiguration = carvedRuneUpgradeConfiguration;
        _gameLanguageService = gameLanguageService;
        _randomGenerator = randomGenerator;
        _weaponRuneCardConfiguration = weaponRuneCardConfiguration;
        _evtbConfiguration = evtbConfiguration;
    }

    public async Task HandleAsync(UpgradeWeaponCarvedRuneEvent e, CancellationToken cancellation)
    {
        IClientSession session = e.Sender;
        GameItemInstance item = e.InventoryItem.ItemInstance;
        bool isProtected = false;

        if (!session.PlayerEntity.HasItem(e.InventoryItem.ItemInstance.ItemVNum))
        {
            // PacketLogger
            return;
        }

        if (item.GameItem.EquipmentSlot != EquipmentType.MainWeapon)
        {
            // Cannot upgrade if it's not MainWeapon.
            return;
        }

        if (item.GameItem.LevelMinimum > 80)
        {
            // Cannot upgrade if equipment is under Level 80.
            return;
        }

        item.CarvedRunes ??= new CarvedRunesDto();

        if (item.IsBound && item.BoundCharacterId != session.PlayerEntity.Id)
        {
            session.SendMsgi(MessageType.Default, Game18NConstString.CannotCarveNotYourWeapon);
            return;
        }

        if (item.CarvedRunes.IsDamaged)
        {
            session.SendMsgi(MessageType.Default, Game18NConstString.WeaponIsDamaged, 2, (short)ItemVnums.REPAIR_RUNE_ANVIL);
            return;
        }

        if (item.CarvedRunes.Upgrade == _carvedRuneUpgradeConfiguration.MaxUpgrade)
        {
            session.SendMsgi(MessageType.Default, Game18NConstString.CannotCarveAnyMoreRunesAsItIsAlreadyTheHighestLevel);
            return;
        }

        CarvedRuneUpgrade carvedRuneUpgrade = _carvedRuneUpgradeConfiguration.RuneUpgradeItem.FirstOrDefault(s => s.Upgrade == (item.CarvedRunes.Upgrade + 1));

        if (carvedRuneUpgrade == null)
        {
            return;
        }

        List<CarvedRuneUpgradeItem> upgradeItems = carvedRuneUpgrade.ItemsWeapon;

        if (upgradeItems.Any(materials => !session.PlayerEntity.HasItem(materials.Vnum, materials.Quantity)))
        {
            return;
        }

        CarvedRunesUpgradeProtection protectionType = e.UpgradeProtection;

        if (protectionType != CarvedRunesUpgradeProtection.NONE)
        {
            isProtected = true;
        }
        
        if (!session.PlayerEntity.RemoveGold(protectionType != CarvedRunesUpgradeProtection.PREMIUM_RUNIC_UPGRADE_SCROLL ? carvedRuneUpgrade.Gold : carvedRuneUpgrade.Gold / 2))
        {
            session.SendChatMessage(session.GetLanguage(GameDialogKey.INTERACTION_MESSAGE_NOT_ENOUGH_GOLD), ChatMessageColorType.PlayerSay);
            return;
        }

        if (isProtected)
        {
            var protectionToItemVnum = new Dictionary<CarvedRunesUpgradeProtection, ItemVnums>
            {
                {CarvedRunesUpgradeProtection.PREMIUM_RUNE_OF_FORTUNE_SCROLL, ItemVnums.PREMIUM_RUNE_OF_FORTUNE_SCROLL},
                {CarvedRunesUpgradeProtection.RUNE_OF_FORTUNE_SCROLL, session.PlayerEntity.HasItem((short)ItemVnums.RUNE_OF_FORTUNE_SCROLL_EVENT_LIMITED) ? ItemVnums.RUNE_OF_FORTUNE_SCROLL_EVENT_LIMITED : ItemVnums.RUNE_OF_FORTUNE_SCROLL},
                {CarvedRunesUpgradeProtection.PREMIUM_RUNIC_UPGRADE_SCROLL, session.PlayerEntity.HasItem((short)ItemVnums.PREMIUM_RUNIC_UPGRADE_SCROLL_EVENT_LIMITED) ? ItemVnums.PREMIUM_RUNIC_UPGRADE_SCROLL_EVENT_LIMITED : session.PlayerEntity.HasItem((short)ItemVnums.PREMIUM_RUNIC_UPGRADE_SCROLL_LIMITED) ? ItemVnums.PREMIUM_RUNIC_UPGRADE_SCROLL_LIMITED : ItemVnums.PREMIUM_RUNIC_UPGRADE_SCROLL},
            };

            short requiredItemVnum = (short)protectionToItemVnum[protectionType];

            if (!session.PlayerEntity.HasItem(requiredItemVnum))
            {
                session.SendSayi(ChatMessageColorType.Yellow, Game18NConstString.NoRuneScrolls);
                return;
            }

            await session.RemoveItemFromInventory(requiredItemVnum);
        }

        await CarvedRuneUpgrade(session, e.InventoryItem, item, protectionType, isProtected);
    }

    private async Task CarvedRuneUpgrade(IClientSession session, InventoryItem inventoryItem, GameItemInstance weapon,
    CarvedRunesUpgradeProtection protectionType, bool isProtected)
    {
        CarvedRuneUpgrade carvedRuneUpgrade = _carvedRuneUpgradeConfiguration.RuneUpgradeItem.FirstOrDefault(s => s.Upgrade == (weapon.CarvedRunes.Upgrade + 1));

        if (carvedRuneUpgrade == null)
        {
            return;
        }

        if (protectionType != CarvedRunesUpgradeProtection.NONE)
        {
            isProtected = true;
        }

        byte originalUpgrade = weapon.CarvedRunes.Upgrade;
        var randomBag = new RandomBag<CarvedRuneUpgradeResult>(_randomGenerator);

        randomBag.AddEntry(CarvedRuneUpgradeResult.Succeed, (int)((carvedRuneUpgrade.Success + protectionType switch {
            CarvedRunesUpgradeProtection.PREMIUM_RUNIC_UPGRADE_SCROLL => 30,
            CarvedRunesUpgradeProtection.PREMIUM_RUNE_OF_FORTUNE_SCROLL => 2,
            _ => 0
        }) * (1 + _evtbConfiguration.GetValueForEventType(EvtbType.INCREASE_CHANCE_UPGRADE_RUNES) * 0.01)));
        
        randomBag.AddEntry(CarvedRuneUpgradeResult.Fail, 100 - carvedRuneUpgrade.Success - carvedRuneUpgrade.Damage);

        if (carvedRuneUpgrade.Damage > 0)
        {
            randomBag.AddEntry(CarvedRuneUpgradeResult.Damaged, carvedRuneUpgrade.Damage);
        }

        CarvedRuneUpgradeResult upgradeResult = randomBag.GetRandom();

        switch (upgradeResult)
        {
            case CarvedRuneUpgradeResult.Damaged when isProtected:
                await HandleDamagedButProtectedResult(session, carvedRuneUpgrade, protectionType);
                break;
            case CarvedRuneUpgradeResult.Damaged:
                await HandleDamagedResult(session, inventoryItem, weapon, carvedRuneUpgrade);
                break;
            case CarvedRuneUpgradeResult.Succeed:
                await HandleSucceedResult(session, inventoryItem, weapon, carvedRuneUpgrade);
                AddCarvedBCard(session, weapon, protectionType);
                break;
            case CarvedRuneUpgradeResult.Fail when isProtected:
                await HandleFailButProtectedResult(session, carvedRuneUpgrade, protectionType);
                break;
            case CarvedRuneUpgradeResult.Fail:
                await HandleFailResult(session, carvedRuneUpgrade);
                break;
        }

        await session.EmitEventAsync(new WeaponCarvedRuneUpgradedEvent
        {
            Weapon = weapon,
            IsProtected = isProtected,
            OriginalUpgrade = originalUpgrade,
            UpgradeResult = upgradeResult
        });

        session.SendShopEndPacket(ShopEndType.Item);
    }

    private void AddCarvedBCard(IClientSession session, GameItemInstance weapon, CarvedRunesUpgradeProtection protectionType)
    {
        weapon.CarvedRunes.BCards ??= new List<BCardDTO>();
        List<BCardDTO> bCardData = new();
        bool isUpgraded = false;

        int effectBCardChance = 0;

        switch (protectionType)
        {
            case CarvedRunesUpgradeProtection.PREMIUM_RUNE_OF_FORTUNE_SCROLL:
            case CarvedRunesUpgradeProtection.RUNE_OF_FORTUNE_SCROLL:
            case CarvedRunesUpgradeProtection.PREMIUM_RUNIC_UPGRADE_SCROLL:
                effectBCardChance = 5;
                break;
        }

        bool isEffectBCard = (weapon.CarvedRunes.Upgrade % 3) == 0 && _randomGenerator.RandomNumber(90, 90 + effectBCardChance) >= _randomGenerator.RandomNumber();
        int randomIndex = isEffectBCard
            ? _randomGenerator.RandomNumber(_weaponRuneCardConfiguration.Cards.Count(s => s.IsRunePower))
            : _randomGenerator.RandomNumber(_weaponRuneCardConfiguration.Cards.Count(s => !s.IsRunePower));

        List<WeaponRuneCard> selectedCard = isEffectBCard
            ? _weaponRuneCardConfiguration.Cards.Where(card => card.IsRunePower).ToList()
            : _weaponRuneCardConfiguration.Cards.Where(card => !card.IsRunePower).ToList();

        WeaponRuneCard runeBCard = selectedCard[randomIndex];
        
        BCardDTO existingBCard = weapon.CarvedRunes.BCards.FirstOrDefault(bCard => bCard.Type == runeBCard.Type && bCard.SubType == runeBCard.SubType);
        
        int runePowerCount = weapon.CarvedRunes.BCards.Count(s => s.IsRunePower);
        int normalBCardCount = weapon.CarvedRunes.BCards.Count(s => !s.IsRunePower);

        if ((isEffectBCard && runePowerCount < 2) || (!isEffectBCard && normalBCardCount < 5))
        {
            // If the BCard generated already exists, then we upgrade it.
            if (existingBCard != null && existingBCard.BCardLevel != 6)
            {
                isUpgraded = true;
                existingBCard.BCardLevel++;
                existingBCard.FirstData = runeBCard.FirstData[existingBCard.BCardLevel];

                if (existingBCard.SecondData != 0)
                {
                    existingBCard.SecondData = runeBCard.SecondData[existingBCard.BCardLevel];
                }

                bCardData.Add(new BCardDTO
                {
                    Type = runeBCard.Type,
                    SubType = runeBCard.SubType,
                    FirstData = runeBCard.FirstData[existingBCard.BCardLevel],
                    SecondData = runeBCard.SecondData[existingBCard.BCardLevel],
                    FirstDataScalingType = runeBCard.FirstDataScalingType
                });
            }
            else
            {
                weapon.CarvedRunes.BCards.Add(new BCardDTO
                {
                    Type = runeBCard.Type,
                    SubType = runeBCard.SubType,
                    FirstData = runeBCard.FirstData[0],
                    SecondData = runeBCard.SecondData[0],
                    FirstDataScalingType = runeBCard.FirstDataScalingType,
                    IsRunePower = isEffectBCard && runeBCard.IsRunePower,
                    BCardLevel = 0,
                    CastType = runeBCard.CastType
                });
                bCardData.Add(new BCardDTO
                {
                    Type = runeBCard.Type,
                    SubType = runeBCard.SubType,
                    FirstData = runeBCard.FirstData[0],
                    SecondData = runeBCard.SecondData[0],
                    FirstDataScalingType = runeBCard.FirstDataScalingType
                });
            }
        }
        else
        {
            List<BCardDTO> bCards = isEffectBCard
                ? weapon.CarvedRunes.BCards.Where(s => s.BCardLevel < 6 && s.IsRunePower).ToList()
                : weapon.CarvedRunes.BCards.Where(s => s.BCardLevel < 6 && !s.IsRunePower).ToList();
            
            int randomNumber = _randomGenerator.RandomNumber(0, bCards.Count);
            
            BCardDTO bCard = bCards[randomNumber];
            WeaponRuneCard weaponBCard = _weaponRuneCardConfiguration.Cards.FirstOrDefault(s => s.Type == bCard.Type && s.SubType == bCard.SubType);
            
            if (bCard != null && weaponBCard != null)
            {
                isUpgraded = true;
                bCard.BCardLevel++;
                bCard.FirstData = weaponBCard.FirstData[bCard.BCardLevel];

                if (bCard.SecondData != 0)
                {
                    bCard.SecondData = weaponBCard.SecondData[bCard.BCardLevel];
                }
                
                bCardData.Add(new BCardDTO
                {
                    Type = weaponBCard.Type,
                    SubType = weaponBCard.SubType,
                    FirstData = weaponBCard.FirstData[bCard.BCardLevel],
                    SecondData = weaponBCard.SecondData[bCard.BCardLevel],
                    FirstDataScalingType = weaponBCard.FirstDataScalingType
                });
            }
        }

        weapon.CarvedRunes.CanUseRuneSolvent = true;
        SendRuneSuccess(session, weapon, bCardData, isUpgraded);
    }
    
    private void SendRuneSuccess(IClientSession session, GameItemInstance item, List<BCardDTO> bCardData, bool isUpgraded = false)
    {
        int type = 0;
        int subType = 0;
        int firstData = 0;
        int secondData = 0;
        BCardScalingType bCardScalingType = BCardScalingType.NORMAL_VALUE;
        foreach (BCardDTO bCard in bCardData)
        {
            type = bCard.Type;
            subType = bCard.SubType;
            firstData = bCard.FirstData; 
            secondData = bCard.SecondData;
            bCardScalingType = bCard.FirstDataScalingType;
        }
        session.SendPacket($"ru_suc {(isUpgraded ? 1 : 0)} {type}.{subType / 10 - 1}.{((subType % 10) == 1 ? firstData * 4 : firstData * 4 * -1)}.{secondData * 4}.{(byte)bCardScalingType} {(short)Game18NConstString.RuneUpgraded} {item.GameItem.Id} {item.CarvedRunes.Upgrade}");
    }

    private async Task HandleDamagedButProtectedResult(IClientSession session, CarvedRuneUpgrade carvedRuneUpgrade, CarvedRunesUpgradeProtection protectionType)
    {
        var protectionToItemVnum = new Dictionary<CarvedRunesUpgradeProtection, ItemVnums>
        {
            {CarvedRunesUpgradeProtection.PREMIUM_RUNE_OF_FORTUNE_SCROLL, ItemVnums.PREMIUM_RUNE_OF_FORTUNE_SCROLL},
            {CarvedRunesUpgradeProtection.RUNE_OF_FORTUNE_SCROLL, ItemVnums.RUNE_OF_FORTUNE_SCROLL},
            {CarvedRunesUpgradeProtection.PREMIUM_RUNIC_UPGRADE_SCROLL, ItemVnums.PREMIUM_RUNIC_UPGRADE_SCROLL},
        };

        short scrollVnum = (short)protectionToItemVnum[protectionType];
        
        if (_randomGenerator.RandomNumber() >= 50)
        {
            session.SendModali(Game18NConstString.RuneUpFailButProtectedMaterials, 2, scrollVnum);
            return;
        }
        
        foreach (CarvedRuneUpgradeItem requiredItem in carvedRuneUpgrade.ItemsWeapon)
        {
            await session.RemoveItemFromInventory(requiredItem.Vnum, requiredItem.Quantity);
        }
        session.SendMsgi(MessageType.Default, Game18NConstString.RuneUpFailWeaponNoDamaged, 2, scrollVnum);
        session.SendSayi(ChatMessageColorType.Red, Game18NConstString.RuneUpFailWeaponNoDamaged, 2, scrollVnum);
        session.SendEffect(EffectType.UpgradeFail);
    }

    private async Task HandleDamagedResult(IClientSession session, InventoryItem inventoryItem, GameItemInstance weapon, CarvedRuneUpgrade carvedRuneUpgrade)
    {
        weapon.CarvedRunes.IsDamaged = true;
        session.SendModali(Game18NConstString.RuneUpFailMaterialsConsumed);
        session.SendMsgi(MessageType.Default, Game18NConstString.RuneUpFailWeaponDamaged);
        session.SendSayi(ChatMessageColorType.Red, Game18NConstString.RuneUpFailWeaponDamaged);
        session.SendInventoryAddPacket(inventoryItem);
        session.SendShopEndPacket(ShopEndType.Item);

        foreach (CarvedRuneUpgradeItem requiredItem in carvedRuneUpgrade.ItemsWeapon)
        {
            await session.RemoveItemFromInventory(requiredItem.Vnum, requiredItem.Quantity);
        }
    }

    private async Task HandleSucceedResult(IClientSession session, InventoryItem inventoryItem, GameItemInstance weapon, CarvedRuneUpgrade carvedRuneUpgrade)
    {
        weapon.CarvedRunes.Upgrade++;
        if (!weapon.IsBound)
        {
            weapon.BoundCharacterId = session.PlayerEntity.Id;
        }
        session.SendEffect(EffectType.UpgradeSuccess);
        session.SendInventoryAddPacket(inventoryItem);
        foreach (CarvedRuneUpgradeItem requiredItem in carvedRuneUpgrade.ItemsWeapon)
        {
            await session.RemoveItemFromInventory(requiredItem.Vnum, requiredItem.Quantity);
        }
    }

    private async Task HandleFailButProtectedResult(IClientSession session, CarvedRuneUpgrade carvedRuneUpgrade, CarvedRunesUpgradeProtection protectionType)
    {
        var protectionToItemVnum = new Dictionary<CarvedRunesUpgradeProtection, ItemVnums>
        {
            {CarvedRunesUpgradeProtection.PREMIUM_RUNE_OF_FORTUNE_SCROLL, ItemVnums.PREMIUM_RUNE_OF_FORTUNE_SCROLL},
            {CarvedRunesUpgradeProtection.RUNE_OF_FORTUNE_SCROLL, ItemVnums.RUNE_OF_FORTUNE_SCROLL},
            {CarvedRunesUpgradeProtection.PREMIUM_RUNIC_UPGRADE_SCROLL, ItemVnums.PREMIUM_RUNIC_UPGRADE_SCROLL},
        };

        short scrollVnum = (short)protectionToItemVnum[protectionType];
        
        if (_randomGenerator.RandomNumber() >= 50)
        {
            session.SendModali(Game18NConstString.RuneUpFailButProtectedMaterials, 2, scrollVnum);
            return;
        }
        
        session.SendModali(Game18NConstString.RuneUpFailMaterialsConsumed);

        foreach (CarvedRuneUpgradeItem requiredItem in carvedRuneUpgrade.ItemsWeapon)
        {
            await session.RemoveItemFromInventory(requiredItem.Vnum, requiredItem.Quantity);
        }
    }

    private async Task HandleFailResult(IClientSession session, CarvedRuneUpgrade carvedRuneUpgrade)
    {
        session.SendModali(Game18NConstString.RuneUpFailMaterialsConsumed);

        foreach (CarvedRuneUpgradeItem requiredItem in carvedRuneUpgrade.ItemsWeapon)
        {
            await session.RemoveItemFromInventory(requiredItem.Vnum, requiredItem.Quantity);
        }
    }
}
