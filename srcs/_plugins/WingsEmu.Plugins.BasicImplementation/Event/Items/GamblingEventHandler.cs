using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsAPI.Game.Extensions.PacketGeneration;
using WingsAPI.Packets.Enums;
using WingsAPI.Packets.Enums.Shells;
using WingsEmu.DTOs.Items;
using WingsEmu.Game;
using WingsEmu.Game._enum;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Algorithm;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Extensions.SubClass;
using WingsEmu.Game.Items;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Networking.Broadcasting;
using WingsEmu.Game.Pity;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;
using PityType = WingsAPI.Packets.Enums.PityType;

namespace NosEmu.Plugins.BasicImplementations.Event.Items;

public class GamblingEventHandler : IAsyncEventProcessor<GamblingEvent>
{
    private readonly IGamblingRarityConfiguration _gamblingRarityConfiguration;
    private readonly GamblingRarityInfo _gamblingRarityInfo;
    private readonly IEvtbConfiguration _evtbConfiguration;
    private readonly IGameLanguageService _gameLanguage;
    private readonly IRandomGenerator _randomGenerator;
    private readonly IShellGenerationAlgorithm _shellGenerationAlgorithm;
    private readonly PityConfiguration _pityConfiguration;

    public GamblingEventHandler(IGameLanguageService gameLanguage, IRandomGenerator randomGenerator, IGamblingRarityConfiguration gamblingRarityConfiguration,
        GamblingRarityInfo gamblingRarityInfo, IShellGenerationAlgorithm shellGenerationAlgorithm, IEvtbConfiguration evtbConfiguration, PityConfiguration pityConfiguration)
    {
        _gameLanguage = gameLanguage;
        _randomGenerator = randomGenerator;
        _gamblingRarityConfiguration = gamblingRarityConfiguration;
        _gamblingRarityInfo = gamblingRarityInfo;
        _shellGenerationAlgorithm = shellGenerationAlgorithm;
        _evtbConfiguration = evtbConfiguration;
        _pityConfiguration = pityConfiguration;
    }

    public async Task HandleAsync(GamblingEvent e, CancellationToken cancellation)
    {
        IClientSession session = e.Sender;
        RarifyProtection protection = e.Protection;

        if (!session.HasCurrentMapInstance || e.Item.ItemInstance.Type != ItemInstanceType.WearableInstance || e.Item.ItemInstance.GameItem.EquipmentSlot == EquipmentType.MiniPet)
        {
            return;
        }

        GameItemInstance item = e.Item.ItemInstance;
        GameItemInstance amulet = e.Amulet?.ItemInstance;

        const int cellaVnum = (int)ItemVnums.CELLA;
        
        int scrollVnum = session.PlayerEntity.HasItem((int)ItemVnums.EQ_NORMAL_SCROLL_EVENT)
            ? (int)ItemVnums.EQ_NORMAL_SCROLL_EVENT
            : (int)ItemVnums.EQ_NORMAL_SCROLL;

        short originalRarity = item.Rarity;
        short maxRarity = (short)(item.GameItem.IsHeroic ? 8 : 7);
        switch (e.Mode)
        {
            case RarifyMode.Increase:
                if (item.Rarity >= maxRarity)
                {
                    item.Rarity -= 1;
                    session.SendInventoryAddPacket(e.Item);
                    session.NotifyRarifyResult(_gameLanguage, item.Rarity);
                    session.RefreshEquipment();
                    return;
                }

                if (IsChampion(amulet) && !item.GameItem.IsHeroic)
                {
                    session.SendChatMessage(_gameLanguage.GetLanguage(GameDialogKey.GAMBLING_CHATMESSAGE_ITEM_IS_NOT_HEROIC, session.UserLanguage), ChatMessageColorType.Yellow);
                    return;
                }

                if (!IsChampion(amulet) && item.GameItem.IsHeroic)
                {
                    session.SendChatMessage(_gameLanguage.GetLanguage(GameDialogKey.GAMBLING_MESSAGE_ITEM_IS_HEROIC, session.UserLanguage), ChatMessageColorType.Yellow);
                    return;
                }

                if (amulet == null)
                {
                    return;
                }

                item.Rarity += 1;
                item.SetRarityPoint(_randomGenerator);
                session.SendInventoryAddPacket(e.Item);
                session.NotifyRarifyResult(_gameLanguage, item.Rarity);
                session.RefreshEquipment();

                await session.EmitEventAsync(new ItemGambledEvent
                {
                    ItemVnum = item.ItemVNum,
                    Mode = e.Mode,
                    Protection = e.Protection,
                    Amulet = e.Amulet?.ItemInstance.ItemVNum,
                    Succeed = true,
                    OriginalRarity = originalRarity,
                    FinalRarity = item.Rarity
                });
                return;

            case RarifyMode.Normal:
                
                bool isHeroic = item.GameItem.IsHeroic;
                short basePrice = _gamblingRarityInfo.GoldPrice; 
                int levelCostMultiplier = isHeroic ? 1000 : 500; 
                int totalCost = basePrice + levelCostMultiplier * item.GameItem.LevelMinimum;
                
                totalCost = (int)Math.Round((double)totalCost);
                
                if (session.PlayerEntity.Gold < totalCost)
                {
                    session.SendInfoi(Game18NConstString.NotEnoughFounds);
                    session.PlayerEntity.BroadcastEndDancingGuriPacket();
                    session.SendShopEndPacket(ShopEndType.Npc);
                    return;
                }
                
                if (!session.PlayerEntity.HasItem(cellaVnum, _gamblingRarityInfo.CellaUsed))
                {
                    return;
                }
                
                switch (protection)
                {
                    case RarifyProtection.Scroll when !session.PlayerEntity.HasItem(scrollVnum):
                        return;
                    case RarifyProtection.Scroll or RarifyProtection.ProtectionAmulet or RarifyProtection.BlessingAmulet when isHeroic:
                        session.SendMsg(_gameLanguage.GetLanguage(GameDialogKey.GAMBLING_MESSAGE_ITEM_IS_HEROIC, session.UserLanguage), MsgMessageType.Middle);
                        return;
                    case RarifyProtection.HeroicAmulet or RarifyProtection.RandomHeroicAmulet when !isHeroic:
                        session.SendMsg(
                            _gameLanguage.GetLanguage(GameDialogKey.GAMBLING_CHATMESSAGE_ITEM_IS_NOT_HEROIC, session.UserLanguage), MsgMessageType.Middle);
                        return;
                }
                
                if (protection == RarifyProtection.Scroll)
                {
                    if (!session.PlayerEntity.HasItem(scrollVnum))
                    {
                        session.SendShopEndPacket(ShopEndType.Item);
                        return;
                    }

                    await session.RemoveItemFromInventory(scrollVnum);
                    session.SendShopEndPacket(ShopEndType.Player);
                }
                
                session.PlayerEntity.Gold -= totalCost;
                await session.RemoveItemFromInventory(cellaVnum, _gamblingRarityInfo.CellaUsed);
                session.RefreshGold();
                
                int extraFee = totalCost - _gamblingRarityInfo.GoldPrice;
                session.SendChatMessage("===============================", ChatMessageColorType.Green);
                session.SendChatMessage(
                    $"Burn gold system: An extra fee has been applied. Additional {extraFee} gold has been reduced from your inventory.",
                    ChatMessageColorType.Green);
                session.SendChatMessage("===============================", ChatMessageColorType.Green);

                if (isHeroic && item.Rarity == 8)
                {
                    item.Rarity -= 1;
                    item.SetRarityPoint(_randomGenerator);
                    session.SendInventoryAddPacket(e.Item);
                    session.NotifyRarifyResult(_gameLanguage, item.Rarity);
                    session.RefreshEquipment();
                    return;
                }

                break;


            default:
                throw new ArgumentOutOfRangeException(nameof(e.Mode), e.Mode, "The selected RarifyMode is not handled");
        }

        bool isSuccess = GamblingSuccess(item, amulet);
        bool forceMaxRarity = false;

        if (!isSuccess)
        {
            if (item.IsPityUpgradeItem(PityType.Betting, _pityConfiguration))
            {
                item.PityCounter[(int)PityType.Betting] = 0;
                forceMaxRarity = true;
                isSuccess = true;
                session.SendChatMessage(session.GetLanguage(GameDialogKey.PITY_CHATMESSAGE_SUCCESS), ChatMessageColorType.Green);
                
                int basePoints = session.PlayerEntity.SubClass.IsPveSubClass() ? 4 : session.PlayerEntity.SubClass.IsPvpAndPveSubClass() ? 2 : 0;
                session.AddTierExperience(basePoints, _gameLanguage, handleExpTimer: false);
            }
            else
            {
                item.PityCounter[(int)PityType.Betting]++;
                
                (int, int) maxFailCounter = item.ItemPityMaxFailCounter(PityType.Betting, _pityConfiguration);
                session.SendChatMessage(session.GetLanguageFormat(GameDialogKey.PITY_CHATMESSAGE_FAIL, maxFailCounter.Item1, maxFailCounter.Item2), ChatMessageColorType.Green);
            }
        }

        if (isSuccess)
        {
            short rarity = _gamblingRarityConfiguration.GetRandomRarity();
            
            if (rarity > maxRarity)
            {
                rarity = maxRarity;
            }
            
            bool isMaxRare = rarity switch
            {
                7 when !item.GameItem.IsHeroic => true,
                8 => true,
                _ => false
            };

            switch (isMaxRare)
            {
                case true:
                    item.PityCounter[(int)PityType.Betting] = 0;
                    break;
                case false when forceMaxRarity:
                    rarity = (short)(item.GameItem.IsHeroic ? 8 : 7);
                    break;
            }

            if (protection == RarifyProtection.Scroll && rarity > item.Rarity || protection != RarifyProtection.Scroll)
            {
                session.NotifyRarifyResult(_gameLanguage, rarity);
                session.BroadcastEffectInRange(EffectType.UpgradeSuccess);
                item.Rarity = rarity;
            }
            
            if (protection == RarifyProtection.RandomHeroicAmulet)
            {
                ShellType shellType = item.GameItem.ItemType == ItemType.Armor ? ShellType.PvpShellArmor : ShellType.PvpShellWeapon;

                if (item.EquipmentOptions != null && item.EquipmentOptions.Count != 0)
                {
                    item.EquipmentOptions.Clear();
                    item.ShellRarity = null;
                }

                item.EquipmentOptions ??= new List<EquipmentOptionDTO>();
                item.EquipmentOptions.AddRange(_shellGenerationAlgorithm.GenerateShell((byte)shellType, item.Rarity == 8 ? 7 : item.Rarity, 99).ToList());
            }

            item.SetRarityPoint(_randomGenerator);
            session.SendInventoryAddPacket(e.Item);

            await session.EmitEventAsync(new ItemGambledEvent
            {
                ItemVnum = item.ItemVNum,
                Mode = e.Mode,
                Protection = e.Protection,
                Amulet = e.Amulet?.ItemInstance.ItemVNum,
                Succeed = true,
                OriginalRarity = originalRarity,
                FinalRarity = item.Rarity
            });
        }
        else
        {
            switch (protection)
            {
                case RarifyProtection.ProtectionAmulet:
                case RarifyProtection.BlessingAmulet:
                case RarifyProtection.HeroicAmulet:
                case RarifyProtection.RandomHeroicAmulet:
                    if (amulet == null)
                    {
                        return;
                    }

                    amulet.DurabilityPoint -= 1;
                    session.SendAmuletBuffPacket(amulet);
                    if (amulet.DurabilityPoint <= 0)
                    {
                        await session.RemoveItemFromInventory(item: e.Amulet, isEquiped: true);
                        session.RefreshEquipment();
                        session.SendModal(_gameLanguage.GetLanguage(GameDialogKey.GAMBLING_INFO_AMULET_DESTROYED, session.UserLanguage), ModalType.Confirm);
                    }

                    session.SendChatMessage(_gameLanguage.GetLanguage(GameDialogKey.GAMBLING_MESSAGE_AMULET_FAIL_SAVED, session.UserLanguage), ChatMessageColorType.Red);
                    session.SendMsg(_gameLanguage.GetLanguage(GameDialogKey.GAMBLING_MESSAGE_AMULET_FAIL_SAVED, session.UserLanguage), MsgMessageType.Middle);

                    await session.EmitEventAsync(new ItemGambledEvent
                    {
                        ItemVnum = item.ItemVNum,
                        Mode = e.Mode,
                        Protection = e.Protection,
                        Amulet = e.Amulet.ItemInstance.ItemVNum,
                        Succeed = false,
                        OriginalRarity = originalRarity,
                        FinalRarity = item.Rarity
                    });
                    return;

                case RarifyProtection.Scroll:
                    session.SendChatMessage(_gameLanguage.GetLanguage(GameDialogKey.GAMBLING_MESSAGE_FAILED_ITEM_SAVED, session.UserLanguage), ChatMessageColorType.Red);
                    session.SendMsg(_gameLanguage.GetLanguage(GameDialogKey.GAMBLING_MESSAGE_FAILED_ITEM_SAVED, session.UserLanguage), MsgMessageType.Middle);
                    session.BroadcastEffect(EffectType.UpgradeFail, new RangeBroadcast(session.PlayerEntity.PositionX, session.PlayerEntity.PositionY));

                    if (!session.PlayerEntity.HasItem(scrollVnum))
                    {
                        session.SendShopEndPacket(ShopEndType.Item);
                    }

                    await session.EmitEventAsync(new ItemGambledEvent
                    {
                        ItemVnum = item.ItemVNum,
                        Mode = e.Mode,
                        Protection = e.Protection,
                        Amulet = e.Amulet?.ItemInstance.ItemVNum,
                        Succeed = false,
                        OriginalRarity = originalRarity,
                        FinalRarity = item.Rarity
                    });
                    return;

                case RarifyProtection.None:
                    await session.RemoveItemFromInventory(item: e.Item);
                    session.SendChatMessage(_gameLanguage.GetLanguage(GameDialogKey.GAMBLING_MESSAGE_FAILED, session.UserLanguage), ChatMessageColorType.Red);
                    session.SendMsg(_gameLanguage.GetLanguage(GameDialogKey.GAMBLING_MESSAGE_FAILED, session.UserLanguage), MsgMessageType.Middle);

                    await session.EmitEventAsync(new ItemGambledEvent
                    {
                        ItemVnum = item.ItemVNum,
                        Mode = e.Mode,
                        Protection = e.Protection,
                        Amulet = e.Amulet?.ItemInstance.ItemVNum,
                        Succeed = false,
                        OriginalRarity = originalRarity
                    });
                    return;
            }
        }
    }

    private bool GamblingSuccess(GameItemInstance item, GameItemInstance amulet)
    {
        if (item.Rarity < 0)
        {
            return true;
        }

        RaritySuccess raritySuccess = _gamblingRarityConfiguration.GetRaritySuccess((byte)item.Rarity);
        if (raritySuccess == null)
        {
            return false;
        }

        int rnd = _randomGenerator.RandomNumber(10000);
        return rnd < (IsEnhanced(amulet) ? raritySuccess.SuccessChance + 1000 : raritySuccess.SuccessChance) + (int)(raritySuccess.SuccessChance * _evtbConfiguration.GetValueForEventType(EvtbType.INCREASE_CHANCE_GAMBLING_EQUIPMENT) * 0.01);
    }

    private bool IsChampion(GameItemInstance amulet) =>
        amulet.ItemVNum is (short)ItemVnums.CHAMPION_AMULET or (short)ItemVnums.CHAMPION_AMULET_RANDOM;

    private bool IsEnhanced(GameItemInstance amulet) =>
        amulet != null && amulet.ItemVNum is (short)ItemVnums.BLESSING_AMULET or (short)ItemVnums.BLESSING_AMULET_DOUBLE or (short)ItemVnums.CHAMPION_AMULET or (short)ItemVnums.CHAMPION_AMULET_RANDOM
            or (short)ItemVnums.PROTECTION_AMULET;
}