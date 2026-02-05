using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WingsAPI.Data.Character;
using WingsAPI.Packets.Enums;
using WingsAPI.Packets.Enums.BattlePass;
using WingsAPI.Packets.Enums.Chat;
using WingsEmu.DTOs.Account;
using WingsEmu.DTOs.BCards;
using WingsEmu.DTOs.Bonus;
using WingsEmu.DTOs.Items;
using WingsEmu.DTOs.Maps;
using WingsEmu.DTOs.Titles;
using WingsEmu.Game._enum;
using WingsEmu.Game._i18n;
using WingsEmu.Game._ItemUsage.Configuration;
using WingsEmu.Game.Battle;
using WingsEmu.Game.BattlePass;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Extensions.Mates;
using WingsEmu.Game.Groups;
using WingsEmu.Game.Inventory;
using WingsEmu.Game.Inventory.Event;
using WingsEmu.Game.Items;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Managers.StaticData;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Maps.Event;
using WingsEmu.Game.Mates;
using WingsEmu.Game.Mates.Events;
using WingsEmu.Game.Miniland;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Networking.Broadcasting;
using WingsEmu.Game.Npcs;
using WingsEmu.Game.Raids;
using WingsEmu.Game.RespawnReturn.Event;
using WingsEmu.Game.Skills;
using WingsEmu.Game.Warehouse;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Battle;
using WingsEmu.Packets.Enums.Character;
using WingsEmu.Packets.Enums.Chat;

namespace WingsEmu.Game.Extensions;

public enum VisualRankType
{
    User = 0,
    GameMaster = 2,
    Admin = 3,
    Developer = 6,
    CommunityManager = 7
}

public static class CharacterExtension
{
    private static IBuffFactory _buffFactory => StaticBuffFactory.Instance;

    public static bool IsQuestExpired(this BattlePassQuest quest, DateTime endSeason)
    {
        if (quest == null)
        {
            return true;
        }

        return GetTotalMinutesBeforeQuestEnd(quest, endSeason) <= 0;
    }

    public static double GetTotalMinutesBeforeQuestEnd(this BattlePassQuest quest, DateTime endSeason)
    {
        TimeSpan timeSpan = TimeSpan.Zero;

        switch (quest.FrequencyType)
        {
            case FrequencyType.Daily:
                timeSpan = quest.Start.AddDays(1) - DateTime.Now;
                break;

            case FrequencyType.Weekly:
                timeSpan = quest.Start.AddDays(7) - DateTime.Now;
                break;

            case FrequencyType.Seasonal:
                timeSpan = endSeason - DateTime.Now;
                break;
        }

        if (quest.Start > DateTime.Now)
        {
            timeSpan = TimeSpan.Zero;
        }
        return timeSpan.TotalMinutes;
    }
    
    public static int GetTrainerQuestAchievement(this IClientSession session, PetTrainerMissionType missionType)
    {
        TrainerQuestDto quest = session.PlayerEntity.TrainerQuestDto.FirstOrDefault(s => s.MissionType == (byte)missionType);
        return quest?.Achievement ?? 0;
    }

    public static bool IsActionForbidden(this IClientSession session)
    {
        if (session.Account.Authority != AuthorityType.GM)
        {
            return false;
        }

        session.SendMsg("As a Game Master, you can't do that.", MsgMessageType.Middle);
        return true;

    }
    
    public static bool IsGameMaster(this IClientSession session) => session.Account.Authority >= AuthorityType.GM;

    public static byte IngameVisualRank(this IClientSession session)
    {
        return session.Account.Authority switch
        {
            AuthorityType.GM or AuthorityType.SGM => (byte)VisualRankType.GameMaster,
            AuthorityType.CM => (byte)VisualRankType.CommunityManager,
            AuthorityType.GA => (byte)VisualRankType.Developer,
            AuthorityType.Owner or AuthorityType.DEV => (byte)VisualRankType.Admin,
            _ => 0,
        };
    }

    public static bool IsInGroupOf(this IPlayerEntity character, IPlayerEntity target)
    {
        if (!character.IsInGroup())
        {
            return false;
        }

        if (!target.IsInGroup())
        {
            return false;
        }

        PlayerGroup characterGroup = character.GetGroup();
        PlayerGroup targetGroup = target.GetGroup();
        return characterGroup.GroupId == targetGroup.GroupId;
    }

    public static string CharacterName(this IClientSession session) => session.PlayerEntity.Name;

    public static bool IsUsingFairyBooster(this IPlayerEntity character) => character.HasBuff(BuffVnums.FAIRY_BOOSTER);

    public static bool CanReceiveMate(this IPlayerEntity character, MateType type) =>
        type == MateType.Pet
            ? character.MaxPetCount > character.MateComponent.GetMates(s => s.MateType == MateType.Pet).Count
            : character.MaxPartnerCount > character.MateComponent.GetMates(s => s.MateType == MateType.Partner).Count;

    public static int GetSpCooldown(this IPlayerEntity character) => character.SpCooldownEnd == null ? 0 : (int)(character.SpCooldownEnd.Value - DateTime.UtcNow).TotalSeconds;

    public static bool IsSpCooldownElapsed(this IPlayerEntity character)
    {
        if (!character.SpCooldownEnd.HasValue)
        {
            return true;
        }

        return character.SpCooldownEnd.Value < DateTime.UtcNow;
    }

    public static void RemovePetBuffs(this IClientSession session, IMateEntity mateEntity, IMateBuffConfigsContainer buffConfigs)
    {
        MateBuffIndividualConfig buffs = buffConfigs.GetMateBuffInfo(mateEntity.NpcMonsterVNum);
        if (buffs != null)
        {
            foreach (int buffId in buffs.BuffIds)
            {
                Buff buff = session.PlayerEntity.BuffComponent.GetBuff(buffId);
                if (buff == null)
                {
                    continue;
                }

                session.PlayerEntity.RemoveBuffAsync(true, buff).ConfigureAwait(false).GetAwaiter().GetResult();
            }
        }
    }

    public static async Task AddPetBuff(this IClientSession session, IMateEntity mateEntity, IMateBuffConfigsContainer buffConfigs, IBuffFactory buffFactory)
    {
        MateBuffIndividualConfig buffs = buffConfigs.GetMateBuffInfo(mateEntity.NpcMonsterVNum);
        if (buffs != null)
        {
            foreach (int cardId in buffs.BuffIds)
            {
                Buff removeBuff = session.PlayerEntity.BuffComponent.GetBuff(cardId);
                await session.PlayerEntity.RemoveBuffAsync(true, removeBuff);
                await session.PlayerEntity.AddBuffAsync(buffFactory.CreateBuff(cardId, mateEntity, BuffFlag.PARTNER));
            }
        }
    }

    public static void ChangeMap(this IClientSession session, IMapInstance mapInstance, short? x = null, short? y = null, bool isMarathonMode = false, RaidParty party = null)
    {
        session.EmitEvent(new JoinMapEvent(mapInstance, x, y, isMarathonMode, party));
    }

    public static void ChangeMap(this IClientSession session, Guid mapInstanceGuid, short? x = null, short? y = null)
    {
        session.EmitEvent(new JoinMapEvent(mapInstanceGuid, x, y));
    }

    public static void ChangeMap(this IClientSession session, int mapInstanceId, short? x = null, short? y = null)
    {
        session.EmitEvent(new JoinMapEvent(mapInstanceId, x, y));
    }

    public static void ChangeToLastBaseMap(this IClientSession session, bool isMarathonMode = false, RaidParty party = null)
    {
        if (party != null)
        {
            session.SendHcscorePacket(party);
        }
        session.EmitEvent(new JoinMapEvent(session.PlayerEntity.MapId, session.PlayerEntity.MapX, session.PlayerEntity.MapY, isMarathonMode, party));
    }

    public static void CharacterInvisible(this IPlayerEntity character, bool triggerAmbush = false)
    {
        character.TriggerAmbush = triggerAmbush;
        foreach (IMateEntity mate in character.MateComponent.TeamMembers())
        {
            if (!character.Session.HasCurrentMapInstance)
            {
                continue;
            }

            character.Session.CurrentMapInstance.Broadcast(mate.GenerateOut());
        }
    }

    public static void RefreshPassiveBCards(this IClientSession session)
    {
        session.PlayerEntity.StatisticsComponent.RefreshPassives();
        session.RefreshStatChar();
        session.RefreshStat();
        session.SendCondPacket();
    }

    public static void RefreshTitleBCards(this IPlayerEntity character, IItemsManager itemsManager, CharacterTitleDto title, IBCardEffectHandlerContainer bCardHandler, bool removeBCards = false)
    {
        IGameItem item = itemsManager.GetItem(title.ItemVnum);
        if (item == null)
        {
            return;
        }

        foreach (BCardDTO bCard in item.BCards)
        {
            if (removeBCards)
            {
                character.BCardComponent.RemoveBCard(bCard);
                continue;
            }

            character.BCardComponent.AddBCard(bCard);
        }
    }

    public static void RefreshEquipmentValues(this IPlayerEntity session, GameItemInstance itemInstance, bool cleanValues = false)
    {
        EquipmentType equipmentType = itemInstance.GameItem.EquipmentSlot;

        if (cleanValues)
        {
            switch (equipmentType)
            {
                case EquipmentType.Armor:
                case EquipmentType.MainWeapon:
                case EquipmentType.SecondaryWeapon:
                    session.ClearShells(equipmentType == EquipmentType.Armor ? EquipmentOptionType.ARMOR_SHELL : EquipmentOptionType.WEAPON_SHELL,
                        equipmentType == EquipmentType.MainWeapon);
                    session.BCardComponent.ClearEquipmentBCards(equipmentType);

                    if (equipmentType != EquipmentType.Armor)
                    {
                        session.BCardComponent.ClearShellTrigger(equipmentType == EquipmentType.MainWeapon);
                    }

                    session.SpecialistComponent.RefreshSlStats(0);
                    break;
                case EquipmentType.Ring:
                case EquipmentType.Necklace:
                case EquipmentType.Bracelet:
                    session.BCardComponent.ClearEquipmentBCards(itemInstance.GameItem.EquipmentSlot);
                    session.ClearCellon(equipmentType);
                    break;
                default:
                    session.BCardComponent.ClearEquipmentBCards(itemInstance.GameItem.EquipmentSlot);
                    break;
            }

            return;
        }

        switch (equipmentType)
        {
            case EquipmentType.Armor:
            case EquipmentType.MainWeapon:
            case EquipmentType.SecondaryWeapon:
                session.AddShells(equipmentType == EquipmentType.Armor ? EquipmentOptionType.ARMOR_SHELL : EquipmentOptionType.WEAPON_SHELL,
                    itemInstance.EquipmentOptions, equipmentType == EquipmentType.MainWeapon);

                session.BCardComponent.AddEquipmentBCards(equipmentType, itemInstance.GameItem.BCards);
                
                if (itemInstance.CarvedRunes?.BCards != null)
                {
                    session.BCardComponent.AddEquipmentBCards(equipmentType, itemInstance.CarvedRunes.BCards);
                }

                if (equipmentType != EquipmentType.Armor)
                {
                    session.TryAddShellBuffs(itemInstance);
                }

                session.SpecialistComponent.RefreshSlStats(0);
                break;
            case EquipmentType.Ring:
            case EquipmentType.Necklace:
            case EquipmentType.Bracelet:
                session.BCardComponent.AddEquipmentBCards(equipmentType, itemInstance.GameItem.BCards);
                session.AddCellon(equipmentType, itemInstance.EquipmentOptions);
                break;
            case EquipmentType.Sp:
                if (!session.UseSp)
                {
                    return;
                }

                session.BCardComponent.AddEquipmentBCards(equipmentType, itemInstance.GameItem.BCards);
                break;
            default:
                session.BCardComponent.AddEquipmentBCards(equipmentType, itemInstance.GameItem.BCards);
                break;
        }
    }

    public static bool CanFight(this IPlayerEntity character) => !character.IsSitting && !character.IsInExchange() && character.MapInstance?.MapInstanceType is not MapInstanceType.UnderWaterShowdown;

    public static int GetTopReputationPlace(this IPlayerEntity character, IReadOnlyList<CharacterDTO> topReputations)
    {
        var sortedTopReputations = topReputations
            .OrderByDescending(c => c.Reput)
            .ToList();

        for (int i = 0; i < sortedTopReputations.Count; i++)
        {
            if (sortedTopReputations[i].Id == character.Id)
            {
                return i + 1;
            }
        }
        return int.MaxValue;
    }

    public static BankRankType GetBankRank(this IPlayerEntity character, IReputationConfiguration reputationConfiguration, IBankReputationConfiguration bankReputationConfiguration,
        IReadOnlyList<CharacterDTO> topReputation)
    {
        if (character.HaveStaticBonus(StaticBonusType.CuarryBankMedal))
        {
            return BankRankType.VIP;
        }

        ReputationType reputationType = character.GetReputationIcon(reputationConfiguration, topReputation);
        BankRankInfo bankRankInfo = bankReputationConfiguration.GetBankRankInfo(reputationType);
        return bankRankInfo?.BankRank ?? 0;
    }

    public static int GetBankPenalty(this IPlayerEntity character, IReputationConfiguration reputationConfiguration, IBankReputationConfiguration bankReputationConfiguration,
        IReadOnlyList<CharacterDTO> topReputation)
    {
        if (character.HaveStaticBonus(StaticBonusType.CuarryBankMedal))
        {
            return 0;
        }

        ReputationType reputationType = character.GetReputationIcon(reputationConfiguration, topReputation);
        BankPenaltyInfo bankPenaltyInfo = bankReputationConfiguration.GetBankPenaltyInfo(reputationType);
        return bankPenaltyInfo?.GoldCost ?? 0;
    }

    public static ReputationType GetReputationIcon(this IPlayerEntity character, IReputationConfiguration reputationConfiguration, IReadOnlyList<CharacterDTO> topReputation)
    {
        var sortedTopReputations = topReputation
            .OrderByDescending(c => c.Reput)
            .ToList();

        int place = character.GetTopReputationPlace(sortedTopReputations);

        ReputationInfo reputationInfo = reputationConfiguration.GetReputationInfo(character.Reput, place);

        return reputationInfo?.Rank ?? 0;
    }

    public static void SendTargetEq(this IClientSession session, IPlayerEntity target)
    {
        string inv0 = "inv 0",
            inv1 = "inv 1",
            inv2 = "inv 2",
            inv3 = "inv 3",
            inv6 = "inv 6",
            inv7 = "inv 7"; // inv 3 used for miniland objects

        foreach (InventoryItem invItem in target.GetAllPlayerInventoryItems().OrderBy(x => x.Slot))
        {
            GameItemInstance inv = invItem.ItemInstance;
            switch (invItem.InventoryType)
            {
                case InventoryType.Equipment:
                    if (inv.GameItem.EquipmentSlot == EquipmentType.Sp)
                    {
                        if (inv.Type != ItemInstanceType.SpecialistInstance)
                        {
                            continue;
                        }

                        inv0 += $" {invItem.Slot}.{inv.ItemVNum}.{inv.Rarity}.{inv.Upgrade}.{inv.SpStoneUpgrade}.0.0";
                    }
                    else
                    {
                        inv0 +=
                            $" {invItem.Slot}.{inv.ItemVNum}.{inv.Rarity}.{(inv.GameItem.IsColorable ? inv.Design : inv.Upgrade)}.{(inv.IsBound ? 1 : 0)}.{inv.GetCarvedRunesInformation(true)}.0";
                    }

                    break;

                case InventoryType.Main:
                    inv1 += $" {invItem.Slot}.{inv.ItemVNum}.{inv.Amount}.0";
                    break;

                case InventoryType.Etc:
                    inv2 += $" {invItem.Slot}.{inv.ItemVNum}.{inv.Amount}.0";
                    break;

                case InventoryType.Miniland:
                    inv3 += $" {invItem.Slot}.{inv.ItemVNum}.{inv.Amount}";
                    break;

                case InventoryType.Specialist:
                    if (inv.Type != ItemInstanceType.SpecialistInstance)
                    {
                        continue;
                    }

                    inv6 += $" {invItem.Slot}.{inv.ItemVNum}.{inv.Rarity}.{inv.Upgrade}.{inv.SpStoneUpgrade}";

                    break;

                case InventoryType.Costume:
                    if (inv.Type != ItemInstanceType.WearableInstance)
                    {
                        continue;
                    }

                    inv7 += $" {invItem.Slot}.{inv.ItemVNum}.{inv.Rarity}.{inv.Upgrade}.0";

                    break;
            }
        }

        session.SendPacket(inv0);
        session.SendPacket(inv1);
        session.SendPacket(inv2);
        session.SendPacket(inv3);
        session.SendPacket(inv6);
        session.SendPacket(inv7);
        session.SendPacket(session.GenerateMlObjListPacket());
    }

    public static bool IsMuted(this IClientSession session) => session.PlayerEntity.MuteRemainingTime.HasValue;

    public static void SendMuteMessage(this IClientSession session)
    {
        if (!session.PlayerEntity.MuteRemainingTime.HasValue)
        {
            return;
        }

        TimeSpan? remainingTime = session.PlayerEntity.MuteRemainingTime;
        GameDialogKey messageType = session.PlayerEntity.Gender == GenderType.Female ? GameDialogKey.MUTE_MESSAGE_FEMALE : GameDialogKey.MUTE_MESSAGE_MALE;

        session.CurrentMapInstance?.Broadcast(s =>
            session.GenerateSayPacket(s.GetLanguage(messageType), ChatMessageColorType.PlayerSay));

        string timeLeft = remainingTime.Value.ToString(@"hh\:mm\:ss");
        session.SendChatMessage(session.GetLanguageFormat(GameDialogKey.MUTE_CHATMESSAGE_TIME_LEFT, timeLeft), ChatMessageColorType.Red);
    }

    public static void SendCharConstBuffEffect(this IClientSession session)
    {
        IReadOnlyList<Buff> buffs = session.PlayerEntity.BuffComponent.GetAllBuffs(b => b.IsConstEffect);
        foreach (Buff buff in buffs)
        {
            session.PlayerEntity.BroadcastConstBuffEffect(buff, (int)buff.Duration.TotalMilliseconds);
        }
    }

    public static void NotifyRarifyResult(this IClientSession session, IGameLanguageService gameLanguage, short rare)
    {
        session.SendChatMessage(gameLanguage.GetLanguageFormat(GameDialogKey.GAMBLING_MESSAGE_SUCCESS, session.UserLanguage, rare), ChatMessageColorType.Green);
        session.SendMsg(gameLanguage.GetLanguageFormat(GameDialogKey.GAMBLING_MESSAGE_SUCCESS, session.UserLanguage, rare), MsgMessageType.Middle);
        session.BroadcastEffect(3005, new RangeBroadcast(session.PlayerEntity.PositionX, session.PlayerEntity.PositionY));
        session.SendShopEndPacket(ShopEndType.Npc);
    }

    public static async Task Respawn(this IClientSession session)
    {
        await session.EmitEventAsync(new RespawnPlayerEvent());
    }

    /// <summary>
    ///     Returns True if the gold was removed or False if it wasn't.
    /// </summary>
    /// <param name="character"></param>
    /// <param name="gold"></param>
    /// <returns></returns>
    public static bool RemoveGold(this IPlayerEntity character, long gold)
    {
        long goldToRemove = Math.Abs(gold);
        if (goldToRemove > character.Gold)
        {
            return false;
        }

        character.Gold -= goldToRemove;
        character.Session.RefreshGold();
        return true;
    }

    /// <summary>
    ///     Returns True if the dignity was added or False if it wasn't.
    /// </summary>
    /// <param name="character"></param>
    /// <param name="dignity"></param>
    /// <param name="minMaxConfiguration"></param>
    /// <returns></returns>
    public static bool AddDignity(this IPlayerEntity character, float dignity, GameMinMaxConfiguration minMaxConfiguration, IGameLanguageService languageService,
        IReputationConfiguration reputationConfiguration, IReadOnlyList<CharacterDTO> topReputation)
    {
        if (character.Dignity >= minMaxConfiguration.MaxDignity)
        {
            return false;
        }

        float oldDignity = character.Dignity;
        character.Dignity += Math.Abs(dignity);

        if (character.Dignity > minMaxConfiguration.MaxDignity)
        {
            character.Dignity = minMaxConfiguration.MaxDignity;
        }

        character.Session.RefreshReputation(reputationConfiguration, topReputation);
        character.Session.SendChatMessage(languageService.GetLanguageFormat(GameDialogKey.DIGNITY_CHATMESSAGE_RESTORE, character.Session.UserLanguage, character.Dignity - oldDignity),
            ChatMessageColorType.Green);
        return true;
    }

    public static async Task RemoveDignity(this IPlayerEntity character, float dignity, GameMinMaxConfiguration minMaxConfiguration, IGameLanguageService languageService,
        IReputationConfiguration reputationConfiguration, IReadOnlyList<CharacterDTO> topReputation)
    {
        if (character.Dignity < minMaxConfiguration.MinDignity)
        {
            character.Dignity = minMaxConfiguration.MinDignity;
            return;
        }

        float oldDignity = character.Dignity;
        character.Dignity -= Math.Abs(dignity);

        if (character.Dignity < minMaxConfiguration.MinDignity)
        {
            character.Dignity = minMaxConfiguration.MinDignity;
        }

        character.Session.RefreshReputation(reputationConfiguration, topReputation);
        character.Session.SendChatMessage(languageService.GetLanguageFormat(GameDialogKey.DIGNITY_CHATMESSAGE_LOSS, character.Session.UserLanguage, oldDignity - character.Dignity),
            ChatMessageColorType.Red);
        if (character.GetDignityIco() == 6)
        {
            foreach (IMateEntity teamMember in character.MateComponent.TeamMembers())
            {
                await character.Session.EmitEventAsync(new MateLeaveTeamEvent
                {
                    MateEntity = teamMember
                });

                if (teamMember.IsAlive())
                {
                    continue;
                }

                teamMember.Hp = 1;
                teamMember.Mp = 1;
            }

            character.Session.SendMsg(character.Session.GetLanguage(GameDialogKey.PET_SHOUTMESSAGE_DIGNITY_LOW), MsgMessageType.Middle);
        }
    }

    /// <summary>
    ///     Returns true if the reputation was removed.
    /// </summary>
    /// <param name="character"></param>
    /// <param name="reputation"></param>
    /// <returns></returns>
    public static bool RemoveReputation(this IPlayerEntity character, long reputation, bool sendMessage = true)
    {
        long value = Math.Abs(reputation);
        if (character.Reput < value)
        {
            return false;
        }

        character.Session.EmitEventAsync(new GenerateReputationEvent
        {
            Amount = -value,
            SendMessage = sendMessage
        });
        return true;
    }

    public static async Task Restore(this IPlayerEntity character, bool restorePlayer = true, bool restoreMates = true,
        bool restoreHealth = true, bool restoreMana = true, bool removeBuffs = true)
    {
        var entitiesToHeal = new List<IBattleEntity>();

        if (restorePlayer)
        {
            entitiesToHeal.Add(character);
        }

        if (restoreMates)
        {
            entitiesToHeal.AddRange(character.MateComponent.TeamMembers());
        }

        foreach (IBattleEntity entity in entitiesToHeal)
        {
            if (restoreHealth)
            {
                entity.Hp = entity.MaxHp;
            }

            if (restoreMana)
            {
                entity.Mp = entity.MaxMp;
            }

            if (removeBuffs)
            {
                await entity.RemoveBuffsOnDeathAsync();
            }
        }

        if (restorePlayer)
        {
            character.Session.RefreshStat();
            character.Session.RefreshStatInfo();
            character.Session.SendCondPacket();
        }

        if (restoreMates)
        {
            character.Session.RefreshMateStats();
        }
    }

    public static bool IsNotStackableInventoryType(this IGameItem gameItem)
    {
        InventoryType inventoryType = gameItem.Type;
        return inventoryType is InventoryType.Costume or InventoryType.Equipment or InventoryType.Specialist or InventoryType.Miniland;
    }

    public static bool HaveStaticBonus(this IPlayerEntity playerEntity, StaticBonusType bonusType) => playerEntity.Bonus.Any(x => x.StaticBonusType == bonusType);

    public static short GetInventorySlots(this IPlayerEntity character, bool removeInventory = false, InventoryType? inventoryType = null)
    {
        byte size = 48;
        bool hasBackPack = character.HaveStaticBonus(StaticBonusType.Backpack);
        bool hasInventoryExtension = character.HaveStaticBonus(StaticBonusType.InventoryExpansion);
        bool hasEreniaMedal = character.HaveStaticBonus(StaticBonusType.EreniaMedal);
        bool hasAdventurerMedal = character.HaveStaticBonus(StaticBonusType.AdventurerMedal);

        if (hasBackPack)
        {
            size += 12;
        }

        if (hasInventoryExtension)
        {
            size += 60;
        }

        if (hasEreniaMedal)
        {
            size += 8;
        }

        if (hasAdventurerMedal)
        {
            size += 4;
        }

        switch (inventoryType)
        {
            case null:
                break;
            case InventoryType.Etc:
                size += 36;
                break;
            case InventoryType.Specialist:
                size = 45;
                break;
            case InventoryType.Costume:
                size = 60;
                break;
            case InventoryType.Miniland:
                size = 50;
                break;
        }

        return (short)(removeInventory ? size - 1 : size);
    }

    public static bool HaveSlotInSpecialInventoryType(this IPlayerEntity character, InventoryType type)
    {
        short slots = character.GetInventorySlots(false, type);
        for (short i = 0; i < slots; i++)
        {
            InventoryItem item = character.GetItemBySlotAndType(i, type);
            if (item?.ItemInstance == null)
            {
                return true;
            }
        }

        return false;
    }

    public static short GetNextInventorySlot(this IPlayerEntity character, InventoryType type)
    {
        short slot = 0;
        switch (type)
        {
            case InventoryType.Costume:
            case InventoryType.Specialist:
                IOrderedEnumerable<InventoryItem> items = character.GetItemsByInventoryType(type).OrderBy(x => x?.ItemInstance.ItemVNum);
                for (short i = 0; i < items.Count(); i++)
                {
                    InventoryItem item = items.ElementAt(i);
                    if (item == null)
                    {
                        continue;
                    }

                    if (item.Slot != i)
                    {
                        character.Session.SendInventoryRemovePacket(item);
                    }

                    item.Slot = i;
                }

                break;
            default:
                for (slot = 0; slot < character.GetInventorySlots(inventoryType: type); slot++)
                {
                    InventoryItem anotherItem = character.GetItemBySlotAndType(slot, type);
                    if (anotherItem != null)
                    {
                        continue;
                    }

                    break;
                }

                break;
        }

        return slot;
    }

    public static byte? GetNextPartnerWarehouseSlot(this IPlayerEntity playerEntity)
    {
        byte? slot = null;

        for (byte warehouseSlot = 0; warehouseSlot < playerEntity.GetPartnerWarehouseSlotsWithoutBackpack(); warehouseSlot++)
        {
            PartnerWarehouseItem anotherItem = playerEntity.GetPartnerWarehouseItem(warehouseSlot);
            if (anotherItem != null)
            {
                continue;
            }

            slot = warehouseSlot;
            break;
        }

        return slot;
    }

    public static bool WeaponLoaded(this IPlayerEntity character, IBattleEntitySkill ski, IGameLanguageService gameLanguage, bool removeAmmo)
    {
        if (ski == null)
        {
            return false;
        }

        GameItemInstance inv;
        switch (character.Class)
        {
            default:
                return false;

            case ClassType.Adventurer:
                if (!ski.Skill.IsUsingSecondWeapon)
                {
                    return true;
                }

                inv = character.SecondaryWeapon;
                if (inv == null)
                {
                    character.Session.SendMsg(gameLanguage.GetLanguage(GameDialogKey.INFORMATION_SHOUTMESSAGE_NO_WEAPON, character.Session.UserLanguage), MsgMessageType.Middle);
                    return false;
                }

                if (character.CountItemWithVnum((short)ItemVnums.AMMO_ADVENTURER) < 1 && inv.Ammo == 0)
                {
                    character.Session.SendMsg(gameLanguage.GetLanguage(GameDialogKey.INFORMATION_SHOUTMESSAGE_NO_AMMO_ADVENTURER, character.Session.UserLanguage), MsgMessageType.Middle);
                    return false;
                }

                if (!removeAmmo)
                {
                    return true;
                }

                if (inv.Ammo > 0)
                {
                    inv.Ammo--;
                    return true;
                }

                character.Session.EmitEvent(new InventoryRemoveItemEvent((short)ItemVnums.AMMO_ADVENTURER));
                inv.Ammo = 100;
                character.Session.SendChatMessage(gameLanguage.GetLanguage(GameDialogKey.AMMO_LOADED_ADVENTURER, character.Session.UserLanguage), ChatMessageColorType.Yellow);

                return true;


            case ClassType.Swordman:
                if (!ski.Skill.IsUsingSecondWeapon)
                {
                    return true;
                }

                inv = character.SecondaryWeapon;
                if (inv == null)
                {
                    character.Session.SendMsg(gameLanguage.GetLanguage(GameDialogKey.INFORMATION_SHOUTMESSAGE_NO_WEAPON, character.Session.UserLanguage), MsgMessageType.Middle);
                    return false;
                }

                if (character.CountItemWithVnum((short)ItemVnums.AMMO_SWORDSMAN) < 1 && inv.Ammo == 0)
                {
                    character.Session.SendMsg(gameLanguage.GetLanguage(GameDialogKey.INFORMATION_SHOUTMESSAGE_NO_AMMO_SWORDSMAN, character.Session.UserLanguage), MsgMessageType.Middle);
                    return false;
                }

                if (!removeAmmo)
                {
                    return true;
                }

                if (inv.Ammo > 0)
                {
                    inv.Ammo--;
                    return true;
                }

                character.Session.EmitEvent(new InventoryRemoveItemEvent((short)ItemVnums.AMMO_SWORDSMAN));
                inv.Ammo = 100;
                character.Session.SendChatMessage(gameLanguage.GetLanguage(GameDialogKey.AMMO_LOADED_SWORDSMAN, character.Session.UserLanguage), ChatMessageColorType.Yellow);

                return true;

            case ClassType.Archer:
                if (ski.Skill.AttackType != AttackType.Ranged)
                {
                    return true;
                }

                inv = character.MainWeapon;

                if (inv == null)
                {
                    character.Session.SendMsg(gameLanguage.GetLanguage(GameDialogKey.INFORMATION_SHOUTMESSAGE_NO_WEAPON, character.Session.UserLanguage), MsgMessageType.Middle);
                    return false;
                }

                if (character.CountItemWithVnum((short)ItemVnums.AMMO_ARCHER) < 1 && inv.Ammo == 0)
                {
                    character.Session.SendMsg(gameLanguage.GetLanguage(GameDialogKey.INFORMATION_SHOUTMESSAGE_NO_AMMO_ARCHER, character.Session.UserLanguage), MsgMessageType.Middle);
                    return false;
                }

                if (!removeAmmo)
                {
                    return true;
                }

                if (inv.Ammo > 0)
                {
                    inv.Ammo--;
                    return true;
                }

                character.Session.EmitEvent(new InventoryRemoveItemEvent((short)ItemVnums.AMMO_ARCHER));
                inv.Ammo = 100;
                character.Session.SendChatMessage(gameLanguage.GetLanguage(GameDialogKey.AMMO_LOADED_ARCHER, character.Session.UserLanguage), ChatMessageColorType.Yellow);

                return true;

            case ClassType.Magician:
                if (!ski.Skill.IsUsingSecondWeapon)
                {
                    return true;
                }

                inv = character.SecondaryWeapon;
                if (inv != null)
                {
                    return true;
                }

                character.Session.SendMsg(gameLanguage.GetLanguage(GameDialogKey.INFORMATION_SHOUTMESSAGE_NO_WEAPON, character.Session.UserLanguage), MsgMessageType.Middle);
                return false;

            case ClassType.MartialArtist:
                return true;
        }
    }

    public static void SendStartStartupInventory(this IClientSession session)
    {
        string inv0 = "inv 0",
            inv1 = "inv 1",
            inv2 = "inv 2",
            inv3 = "inv 3",
            inv6 = "inv 6",
            inv7 = "inv 7"; // inv 3 used for miniland objects

        foreach (InventoryItem invItem in session.PlayerEntity.GetAllPlayerInventoryItems().OrderBy(x => x.Slot))
        {
            GameItemInstance inv = invItem.ItemInstance;
            switch (invItem.InventoryType)
            {
                case InventoryType.Equipment:
                    if (inv.GameItem.EquipmentSlot == EquipmentType.Sp)
                    {
                        if (inv.Type != ItemInstanceType.SpecialistInstance)
                        {
                            inv0 += $" {invItem.Slot}.{inv.ItemVNum}.{inv.Rarity}.{(inv.GameItem.IsColorable ? inv.Design : inv.Upgrade)}.{inv.GetCarvedRunesInformation(true)}";
                            continue;
                        }

                        inv0 += $" {invItem.Slot}.{inv.ItemVNum}.{inv.Rarity}.{inv.Upgrade}.{inv.SpStoneUpgrade}";
                    }
                    else
                    {
                        if (inv.GameItem.ItemSubType == 7)
                        {
                            inv0 += $" {invItem.Slot}.{inv.ItemVNum}.{inv.Rarity}.{(inv.GameItem.IsColorable ? inv.Design : inv.Upgrade)}.{(inv.IsBound ? 1 : 0)}.0.0";
                            break;
                        }

                        inv0 += $" {invItem.Slot}.{inv.ItemVNum}.{inv.Rarity}.{(inv.GameItem.IsColorable ? inv.Design : inv.Upgrade)}.{(inv.IsBound ? 1 : 0)}.{inv.GetCarvedRunesInformation(true)}.0";
                    }

                    break;

                case InventoryType.Main:
                    inv1 += $" {invItem.Slot}.{inv.ItemVNum}.{inv.Amount}.0";
                    break;

                case InventoryType.Etc:
                    inv2 += $" {invItem.Slot}.{inv.ItemVNum}.{inv.Amount}.0";
                    break;

                case InventoryType.Miniland:
                    inv3 += $" {invItem.Slot}.{inv.ItemVNum}.{inv.Amount}";
                    break;

                case InventoryType.Specialist:
                    if (inv.Type != ItemInstanceType.SpecialistInstance)
                    {
                        continue;
                    }

                    inv6 += $" {invItem.Slot}.{inv.ItemVNum}.{inv.Rarity}.{inv.Upgrade}.{inv.SpStoneUpgrade}";

                    break;

                case InventoryType.Costume:
                    if (inv.Type != ItemInstanceType.WearableInstance)
                    {
                        continue;
                    }

                    inv7 += $" {invItem.Slot}.{inv.ItemVNum}.{inv.Rarity}.{inv.Upgrade}.0";

                    break;
            }
        }

        session.SendPacket(inv0);
        session.SendPacket(inv1);
        session.SendPacket(inv2);
        session.SendPacket(inv3);
        session.SendPacket(inv6);
        session.SendPacket(inv7);
        session.SendPacket(session.GenerateMlObjListPacket());
    }

    public static string GenerateMlObjListPacket(this IClientSession session)
    {
        string mlobjstring = "mlobjlst";
        foreach (InventoryItem invItem in session.PlayerEntity.GetAllPlayerInventoryItems()
                     .Where(s => s.InventoryType == InventoryType.Miniland)
                     .OrderBy(s => s.Slot))
        {
            GameItemInstance item = invItem.ItemInstance;
            MapDesignObject mp = session.PlayerEntity.MapInstance?.MapDesignObjects.FirstOrDefault(s => s.InventorySlot == invItem.Slot);
            bool used = mp != null;

            if (item.GameItem.IsWarehouse && used)
            {
                session.PlayerEntity.WareHouseSize = item.GameItem.MinilandObjectPoint;
            }

            mlobjstring +=
                $" {invItem.Slot}.{(used ? 1 : 0)}.{(used ? mp.MapX : 0)}.{(used ? mp.MapY : 0)}.{(item.GameItem.Width != 0 ? item.GameItem.Width : 1)}.{(item.GameItem.Height != 0 ? item.GameItem.Height : 1)}.{(used ? mp.InventoryItem.ItemInstance.DurabilityPoint : 0)}.100.0.1";
        }

        return mlobjstring;
    }

    public static bool IsInAct5(this IClientSession session)
    {
        if (session?.CurrentMapInstance == null)
        {
            return false;
        }

        return session.CurrentMapInstance.HasMapFlag(MapFlags.ACT_5_1) || session.CurrentMapInstance.HasMapFlag(MapFlags.ACT_5_2);
    }
    
    public static bool IsInAct6(this IClientSession session)
    {
        if (session?.CurrentMapInstance == null)
        {
            return false;
        }

        return session.CurrentMapInstance.HasMapFlag(MapFlags.ACT_6_1) || session.CurrentMapInstance.HasMapFlag(MapFlags.ACT_6_2);
    }
    
    public static bool IsInAct7(this IClientSession session) => session?.CurrentMapInstance != null && session.CurrentMapInstance.HasMapFlag(MapFlags.ACT_7);

    public static bool CantPerformActionOnAct4(this IClientSession session)
    {
        if (!session.CurrentMapInstance.HasMapFlag(MapFlags.ACT_4))
        {
            return false;
        }

        if (session.PlayerEntity.IsSeal)
        {
            return true;
        }

        session.SendMsg(session.GetLanguage(GameDialogKey.INFORMATION_SHOUTMESSAGE_MUST_BE_IN_CLASSIC_MAP), MsgMessageType.Middle);
        return true;
    }

    public static void BroadcastInTeamMembers(this IClientSession session, IGameLanguageService gameLanguageService, ISpPartnerConfiguration spPartner)
    {
        foreach (IMateEntity mate in session.PlayerEntity.MateComponent.TeamMembers())
        {
            if (!mate.IsAlive())
            {
                mate.Position = session.PlayerEntity.Position;
                continue;
            }

            mate.TeleportNearCharacter();
            session.SendCondMate(mate);
            session.PlayerEntity.MapInstance.Broadcast(x =>
            {
                bool isAnonymous = session.CurrentMapInstance.HasMapFlag(MapFlags.ACT_4) && x.PlayerEntity.Faction != session.PlayerEntity.Faction;
                string inPacket = mate.GenerateIn(gameLanguageService, x.UserLanguage, spPartner, isAnonymous);
                return inPacket;
            });
        }
    }
    
    public static async Task UpdateEnergyBar(this IPlayerEntity character, int value)
    {
        character.EnergyBar += value;
        character.EnergyBar = character.Morph switch
        {
            (byte)MorphType.MasterWolf => Math.Max(0, Math.Min(3000, character.EnergyBar)),
            (byte)MorphType.DragonKnight => Math.Max(0, Math.Min(300, character.EnergyBar)),
            _ => Math.Max(0, Math.Min(100, character.EnergyBar))
        };
        character.Session.SendSpFtptPacket();

        switch (character.Morph)
        {
            case (byte)MorphType.HolyMage:
                switch (character.EnergyBar)
                {
                    case >= 81 when !character.HasBuff(BuffVnums.UNLEASHED_HOLY_POWER):
                        await character.AddBuffAsync(_buffFactory.CreateBuff((short)BuffVnums.UNLEASHED_HOLY_POWER, character));
                        await character.RemoveBuffAsync(true, character.BuffComponent.GetBuff((int)BuffVnums.AWAKENED_HOLY_POWER));
                        break;
                    case < 81 when character.HasBuff(BuffVnums.UNLEASHED_HOLY_POWER):
                        await character.RemoveBuffAsync(true, character.BuffComponent.GetBuff((int)BuffVnums.UNLEASHED_HOLY_POWER));
                        break;
                    case >= 51 when !character.HasBuff(BuffVnums.AWAKENED_HOLY_POWER):
                        await character.AddBuffAsync(_buffFactory.CreateBuff((short)BuffVnums.AWAKENED_HOLY_POWER, character));
                        await character.RemoveBuffAsync(true, character.BuffComponent.GetBuff((int)BuffVnums.WEAK_HOLY_POWER));
                        break;
                    case < 51 when character.HasBuff(BuffVnums.AWAKENED_HOLY_POWER):
                        await character.RemoveBuffAsync(true, character.BuffComponent.GetBuff((int)BuffVnums.AWAKENED_HOLY_POWER));
                        break;
                    case >= 20 when !character.HasBuff(BuffVnums.WEAK_HOLY_POWER):
                        await character.AddBuffAsync(_buffFactory.CreateBuff((short)BuffVnums.WEAK_HOLY_POWER, character));
                        break;
                    case < 20 when character.HasBuff(BuffVnums.WEAK_HOLY_POWER):
                        await character.RemoveBuffAsync(true, character.BuffComponent.GetBuff((int)BuffVnums.WEAK_HOLY_POWER));
                        break;
                }

                break;
            case (byte)MorphType.WaterfallBerserker:
                switch (character.EnergyBar)
                {
                    case >= 90 when !character.HasBuff(BuffVnums.WATERFALL_FRENZY):
                        await character.AddBuffAsync(_buffFactory.CreateBuff((short)BuffVnums.WATERFALL_FRENZY, character));
                        await character.RemoveBuffAsync(true, character.BuffComponent.GetBuff((int)BuffVnums.WATERFALL_FURY));
                        break;
                    case < 90 when character.HasBuff(BuffVnums.WATERFALL_FRENZY):
                        if (!character.PreventEnergyRemove)
                        {
                            await character.RemoveBuffAsync(true, character.BuffComponent.GetBuff((int)BuffVnums.WATERFALL_FRENZY));
                        }
                        else
                        {
                            await character.AddBuffAsync(_buffFactory.CreateBuff((short)BuffVnums.WATERFALL_FURY, character));
                        }

                        break;
                    case >= 62 when !character.HasBuff(BuffVnums.WATERFALL_FURY):
                        await character.AddBuffAsync(_buffFactory.CreateBuff((short)BuffVnums.WATERFALL_FURY, character));
                        await character.RemoveBuffAsync(true, character.BuffComponent.GetBuff((int)BuffVnums.WATERFALL_ANGER));
                        break;
                    case < 62 when character.HasBuff(BuffVnums.WATERFALL_FURY):
                        await character.RemoveBuffAsync(true, character.BuffComponent.GetBuff((int)BuffVnums.WATERFALL_FURY));
                        break;
                    case >= 42 when !character.HasBuff(BuffVnums.WATERFALL_ANGER):
                        await character.AddBuffAsync(_buffFactory.CreateBuff((short)BuffVnums.WATERFALL_ANGER, character));
                        await character.RemoveBuffAsync(true, character.BuffComponent.GetBuff((int)BuffVnums.WATEFALL_IRE));
                        break;
                    case < 42 when character.HasBuff(BuffVnums.WATERFALL_ANGER):
                        await character.RemoveBuffAsync(true, character.BuffComponent.GetBuff((int)BuffVnums.WATERFALL_ANGER));
                        break;
                    case >= 22 when !character.HasBuff(BuffVnums.WATEFALL_IRE):
                        await character.AddBuffAsync(_buffFactory.CreateBuff((short)BuffVnums.WATEFALL_IRE, character));
                        break;
                    case < 22 when character.HasBuff(BuffVnums.WATEFALL_IRE):
                        await character.RemoveBuffAsync(true, character.BuffComponent.GetBuff((int)BuffVnums.WATEFALL_IRE));
                        break;
                }

                break;
            case (byte)MorphType.DragonKnight:
                switch (character.EnergyBar)
                {
                    case >= 300 when !character.HasBuff(BuffVnums.SHARP_LANCE):
                        await character.AddBuffAsync(_buffFactory.CreateBuff((short)BuffVnums.SHARP_LANCE, character));
                        await character.RemoveBuffAsync(true, character.BuffComponent.GetBuff((int)BuffVnums.POINTED_LANCE));
                        break;
                    case < 300 when character.HasBuff(BuffVnums.SHARP_LANCE):
                        await character.RemoveBuffAsync(true, character.BuffComponent.GetBuff((int)BuffVnums.SHARP_LANCE));
                        break;
                    case >= 180 when !character.HasBuff(BuffVnums.POINTED_LANCE):
                        await character.AddBuffAsync(_buffFactory.CreateBuff((short)BuffVnums.POINTED_LANCE, character));
                        await character.RemoveBuffAsync(true, character.BuffComponent.GetBuff((int)BuffVnums.POLISHED_LANCE));
                        break;
                    case < 180 when character.HasBuff(BuffVnums.POINTED_LANCE):
                        await character.RemoveBuffAsync(true, character.BuffComponent.GetBuff((int)BuffVnums.POINTED_LANCE));
                        break;
                    case >= 65 when !character.HasBuff(BuffVnums.POLISHED_LANCE):
                        await character.AddBuffAsync(_buffFactory.CreateBuff((short)BuffVnums.POLISHED_LANCE, character));
                        break;
                    case < 65 when character.HasBuff(BuffVnums.POLISHED_LANCE):
                        await character.RemoveBuffAsync(true, character.BuffComponent.GetBuff((int)BuffVnums.POLISHED_LANCE));
                        break;
                }

                break;
            case (byte)MorphType.Blaster:
                switch (character.EnergyBar)
                {
                    case >= 100 when !character.HasBuff(BuffVnums.ULTRA_HIGH_HEATING):
                        await character.AddBuffAsync(_buffFactory.CreateBuff((short)BuffVnums.ULTRA_HIGH_HEATING, character));
                        await character.RemoveBuffAsync(true, character.BuffComponent.GetBuff((int)BuffVnums.HIGH_HEATING));
                        break;
                    case < 100 when character.HasBuff(BuffVnums.ULTRA_HIGH_HEATING):
                        await character.RemoveBuffAsync(true, character.BuffComponent.GetBuff((int)BuffVnums.ULTRA_HIGH_HEATING));
                        break;
                    case >= 70 when !character.HasBuff(BuffVnums.HIGH_HEATING):
                        await character.AddBuffAsync(_buffFactory.CreateBuff((short)BuffVnums.HIGH_HEATING, character));
                        await character.RemoveBuffAsync(true, character.BuffComponent.GetBuff((int)BuffVnums.HEATING));
                        break;
                    case < 70 when character.HasBuff(BuffVnums.HIGH_HEATING):
                        await character.RemoveBuffAsync(true, character.BuffComponent.GetBuff((int)BuffVnums.HIGH_HEATING));
                        break;
                    case >= 40 when !character.HasBuff(BuffVnums.HEATING):
                        await character.AddBuffAsync(_buffFactory.CreateBuff((short)BuffVnums.HEATING, character));
                        await character.RemoveBuffAsync(true, character.BuffComponent.GetBuff((int)BuffVnums.PRE_HEATING));
                        break;
                    case < 40 when character.HasBuff(BuffVnums.HEATING):
                        await character.RemoveBuffAsync(true, character.BuffComponent.GetBuff((int)BuffVnums.HEATING));
                        break;
                    case >= 20 when !character.HasBuff(BuffVnums.PRE_HEATING):
                        await character.AddBuffAsync(_buffFactory.CreateBuff((short)BuffVnums.PRE_HEATING, character));
                        break;
                    case < 20 when character.HasBuff(BuffVnums.PRE_HEATING):
                        await character.RemoveBuffAsync(true, character.BuffComponent.GetBuff((int)BuffVnums.PRE_HEATING));
                        break;
                }

                break;
            
            case (byte)MorphType.HydraulicFist:
                switch (character.EnergyBar)
                {
                    case < 4 when character.HasBuff(BuffVnums.IMPROVED_EXCESS_FUEL):
                        await character.RemoveBuffAsync(true, character.BuffComponent.GetBuff((int)BuffVnums.IMPROVED_EXCESS_FUEL));
                        await character.AddBuffAsync(_buffFactory.CreateBuff((short)BuffVnums.EXCESS_FUEL, character));
                        break;
                }

                break;
        }
    }

    public static async Task UpdateBothEnergyBars(this IPlayerEntity character, int value, int secondValue)
    {
        character.EnergyBar += value;
        character.SecondEnergyBar += secondValue;
        character.EnergyBar = Math.Max(0, Math.Min(100, character.EnergyBar));
        character.SecondEnergyBar = Math.Max(0, Math.Min(100, character.SecondEnergyBar));
        character.Session.SendSpFtptPacket();

        switch (character.Morph)
        {
            case (byte)MorphType.Gravity:
                int equilibrium = character.EnergyBar - character.SecondEnergyBar;
                bool isEquilibrium = character.EnergyBar >= 30 && character.SecondEnergyBar >= 30 && equilibrium <= 10;
                if (!isEquilibrium)
                {
                    switch (character.EnergyBar)
                    {
                        case >= 90 when !character.HasBuff(BuffVnums.OVERLOADED_GRAVITATION_FIELD):
                            await character.AddBuffAsync(_buffFactory.CreateBuff((short)BuffVnums.OVERLOADED_GRAVITATION_FIELD, character));
                            await character.RemoveBuffAsync(true, character.BuffComponent.GetBuff((int)BuffVnums.EXCESSIVE_GRAVITATION_FIELD));
                            break;
                        case < 90 when character.HasBuff(BuffVnums.OVERLOADED_GRAVITATION_FIELD):
                            await character.RemoveBuffAsync(true, character.BuffComponent.GetBuff((int)BuffVnums.OVERLOADED_GRAVITATION_FIELD));
                            break;
                        case >= 30 when !character.HasBuff(BuffVnums.EXCESSIVE_GRAVITATION_FIELD):
                            await character.AddBuffAsync(_buffFactory.CreateBuff((short)BuffVnums.EXCESSIVE_GRAVITATION_FIELD, character));
                            break;
                        case < 30 when character.HasBuff(BuffVnums.EXCESSIVE_GRAVITATION_FIELD):
                            await character.RemoveBuffAsync(true, character.BuffComponent.GetBuff((int)BuffVnums.EXCESSIVE_GRAVITATION_FIELD));
                            break;
                    }

                    switch (character.SecondEnergyBar)
                    {
                        case >= 90 when !character.HasBuff(BuffVnums.OVERLOADED_ANTI_GRAVITATION_FIELD):
                            await character.AddBuffAsync(_buffFactory.CreateBuff((short)BuffVnums.OVERLOADED_ANTI_GRAVITATION_FIELD, character));
                            await character.RemoveBuffAsync(true, character.BuffComponent.GetBuff((int)BuffVnums.EXCESSIVE_ANTI_GRAVITATION_FIELD));
                            break;
                        case < 90 when character.HasBuff(BuffVnums.OVERLOADED_ANTI_GRAVITATION_FIELD):
                            await character.RemoveBuffAsync(true, character.BuffComponent.GetBuff((int)BuffVnums.OVERLOADED_ANTI_GRAVITATION_FIELD));
                            break;
                        case >= 30 when !character.HasBuff(BuffVnums.EXCESSIVE_ANTI_GRAVITATION_FIELD):
                            await character.AddBuffAsync(_buffFactory.CreateBuff((short)BuffVnums.EXCESSIVE_ANTI_GRAVITATION_FIELD, character));
                            break;
                        case < 30 when character.HasBuff(BuffVnums.EXCESSIVE_ANTI_GRAVITATION_FIELD):
                            await character.RemoveBuffAsync(true, character.BuffComponent.GetBuff((int)BuffVnums.EXCESSIVE_ANTI_GRAVITATION_FIELD));
                            break;
                    }
                }

                switch (isEquilibrium)
                {
                    case true:
                        if (!character.HasBuff(BuffVnums.GRAVITATIONAL_EQUILIBRIUM))
                        {
                            await character.AddBuffAsync(_buffFactory.CreateBuff((short)BuffVnums.GRAVITATIONAL_EQUILIBRIUM, character));
                        }
                        break;
                    default:
                        await character.RemoveBuffAsync(true, character.BuffComponent.GetBuff((int)BuffVnums.GRAVITATIONAL_EQUILIBRIUM));
                        break;
                }

                break;
        }
    }
    
    public static async Task UpdateTokenPointsBar(this IPlayerEntity character, int value)
    {
        if (character.TokenGauge >= 3 && value > 0)
        {
            return;
        }

        character.TokenEnergyBar += value;
        
        switch (character.TokenGauge)
        {
            case 2 when character.TokenEnergyBar >= 100:
                await character.UpdateTokenGauge(1);
                character.TokenEnergyBar = 0;
                break;
            default:
            {
                if (character.TokenEnergyBar >= 100)
                {
                    await character.UpdateTokenGauge(1);
                    character.TokenEnergyBar = 0;
                }
                else
                {
                    character.TokenEnergyBar = Math.Max(0, character.TokenEnergyBar);
                }

                break;
            }
        }

        if (character.TokenGauge >= 3)
        {
            character.TokenEnergyBar = 0;
        }

        character.Session.SendSpFtptPacket();
    }
    
    public static Task UpdateTokenGauge(this IPlayerEntity character, int value)
    {
        character.TokenGauge += value;
        character.TokenGauge = Math.Max(0, Math.Min(3, character.TokenGauge));
        character.Session.SendSpFtptPacket();
        return Task.CompletedTask;
    }
    
    public static void SendSuccessMissionMessage(this IClientSession session, ISessionManager sessionManager, Game18NConstString constString, I18NArgumentType argumentType, string firstData, int secondData)
    {
        sessionManager.Broadcast(x => session.GenerateSayi2Packet(EntityType.Player, ChatMessageColorType.Orange, constString, argumentType, firstData, secondData));
    }
    
    public static bool IsPvPMap(this IMapInstance mapInstance)
    {
        return (mapInstance.IsPvp && mapInstance.HasMapFlag(MapFlags.HAS_PVP_ENABLED)) || mapInstance.HasMapFlag(MapFlags.HAS_PVP_FACTION_ENABLED) || mapInstance.HasMapFlag(MapFlags.HAS_PVP_FAMILY_ENABLED);
    }

    public static bool IsNonPvPMap(this IMapInstance mapInstance)
    {
        return (!mapInstance.IsPvp && !mapInstance.HasMapFlag(MapFlags.HAS_PVP_ENABLED) && !mapInstance.HasMapFlag(MapFlags.HAS_PVP_FACTION_ENABLED)) || !mapInstance.HasMapFlag(MapFlags.HAS_PVP_FAMILY_ENABLED);
    }
}