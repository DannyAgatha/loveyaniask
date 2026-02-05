using PhoenixLib.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WingsAPI.Data.Character;
using WingsAPI.Data.Families;
using WingsAPI.Packets.Enums;
using WingsAPI.Packets.Enums.BattlePass;
using WingsAPI.Packets.Enums.Chat;
using WingsAPI.Packets.Enums.PartnerFusion;
using WingsEmu.DTOs.Account;
using WingsEmu.DTOs.Bonus;
using WingsEmu.DTOs.Maps;
using WingsEmu.DTOs.Titles;
using WingsEmu.Game._enum;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Algorithm;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Configurations.Miniland;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Extensions.Mates;
using WingsEmu.Game.Families;
using WingsEmu.Game.Inventory;
using WingsEmu.Game.Items;
using WingsEmu.Game.LandOfDeath;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Mates;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Networking.Broadcasting;
using WingsEmu.Game.Raids;
using WingsEmu.Game.Revival;
using WingsEmu.Game.Skills;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Character;
using WingsEmu.Packets.Enums.Chat;
using WingsEmu.Packets.Enums.Families;
using WingsEmu.Packets.Enums.Mails;
using WingsEmu.Packets.Enums.Titles;
using WingsEmu.Packets.ServerPackets;
using WingsEmu.Packets.ServerPackets.Titles;

namespace WingsEmu.Game.Extensions;

public static class CharacterPacketExtension
{
    private static readonly HashSet<int> FairyMorphsWithoutBoostChange = new()
    {
        0, //Normal
        4, // Solaris
        9, // Azuris
        14, // Magmaris
        15 // Vacuris
    };

    private static IGameLanguageService GameLanguage => StaticGameLanguageService.Instance;
    private static IMapManager MapManager => StaticMapManager.Instance;

    
    # region Send Packets

    public static void RefreshStat(this IClientSession session, bool isTransform = false) => session.SendPacket(session.GenerateStatPacket(isTransform));
    public static void RefreshStatInfo(this IClientSession session) => session.SendPacket(session.GenerateStatInfoPacket());
    public static void RefreshStatChar(this IClientSession session, bool refreshHpMp = true) => session.SendPacket(session.GenerateStatCharPacket(refreshHpMp));
    public static void RefreshGold(this IClientSession session) => session.SendPacket(session.GenerateGoldPacket());

    public static void RefreshReputation(this IClientSession session, IReputationConfiguration reputationConfiguration, IReadOnlyList<CharacterDTO> topReputation)
    {
        session.SendPacket(session.GenerateFdPacket(reputationConfiguration, topReputation));
        session.EmitEvent(new IncreaseBattlePassObjectiveEvent(MissionType.ReachXReputation, session.PlayerEntity.Reput));
    }

    public static void RefreshLevel(this IClientSession session, ICharacterAlgorithm characterAlgorithm) => session.SendPacket(session.GenerateLevPacket(characterAlgorithm));
    public static void SendTitlePacket(this IClientSession session) => session.SendPacket(session.GenerateTitlePacket());
    public static void SendCondPacket(this IClientSession session) => session.SendPacket(session.GenerateCondPacket());

    public static void SendTeleportPacket(this IClientSession session) =>
        session.SendPacket(session.PlayerEntity.GenerateTeleportPacket(session.PlayerEntity.PositionX, session.PlayerEntity.PositionY));

    public static void SendFcPacket(this IClientSession session, Act4Status act4Status) => session.SendPacket(UiPacketExtension.GenerateFcPacket(session.PlayerEntity.Faction, act4Status));
    public static void SendTargetTitInfoPacket(this IClientSession session, IClientSession target) => session.SendPacket(target.GenerateTitInfoPacket());
    public static void SendTitInfoPacket(this IClientSession session) => session.SendPacket(session.GenerateTitInfoPacket());
    public static void SendEffect(this IClientSession session, EffectType effectType) => session.SendPacket(session.GenerateEffectPacket(effectType));
    public static void SendMateEffect(this IClientSession session, IMateEntity entity, EffectType effectType) => session.SendPacket(entity.GenerateEffectPacket(effectType));
    public static void SendNpcEffect(this IClientSession session, INpcEntity entity, EffectType effectType) => session.SendPacket(entity.GenerateEffectPacket(effectType));
    public static void RefreshFaction(this IClientSession session) => session.SendPacket(session.GenerateFactionPacket());
    public static void ShowInventoryExtensions(this IClientSession session) => session.SendPacket(session.GenerateExtsPacket());
    public static void SendLevelUp(this IClientSession session) => session.SendPacket(session.GenerateLevelUpPacket());
    public static void SendCModePacket(this IClientSession session) => session.SendPacket(session.GenerateCModePacket());
    public static void SendEqPacket(this IClientSession session) => session.SendPacket(session.GenerateEqPacket());
    public static void RefreshEquipment(this IClientSession session) => session.SendPacket(session.GenerateEquipPacket());
    public static void SendInventoryRemovePacket(this IClientSession session, InventoryType type, int slot) => session.SendPacket(session.GenerateInventoryRemovePacket(type, slot));

    public static void RefreshInventorySlot(this IClientSession session, short slot, InventoryType type)
    {
        InventoryItem inventory = session.PlayerEntity.GetItemBySlotAndType(slot, type);
        if (inventory == null)
        {
            return;
        }

        session.SendInventoryAddPacket(inventory);
    }

    public static void SendInventoryAddPacket(this IClientSession session, InventoryItem itemInstance)
    {
        if (itemInstance == null)
        {
            return;
        }

        if (itemInstance.InventoryType == InventoryType.EquippedItems)
        {
            return;
        }

        session.SendPacket(itemInstance.GenerateInventoryAdd());
    }

    public static void SendInventoryRemovePacket(this IClientSession session, InventoryItem itemInstance)
    {
        if (itemInstance?.InventoryType == null)
        {
            return;
        }

        if (itemInstance.InventoryType == InventoryType.EquippedItems)
        {
            return;
        }

        session.SendPacket(itemInstance.GenerateInventoryRemove());
    }

    public static void SendAtPacket(this IClientSession session) => session.SendPacket(session.GenerateAtPacket());
    public static void SendCMapPacket(this IClientSession session, bool isEntering) => session.SendPacket(session.GenerateCMapPacket(isEntering));

    public static void SendCInfoPacket(this IClientSession session, IFamily family, IReputationConfiguration reputationConfiguration, IReadOnlyList<CharacterDTO> topReputation) =>
        session.SendPacket(session.GenerateCInfoPacket(family, reputationConfiguration, topReputation));

    public static void SendDelay(this IClientSession session, int delay, GuriType type, string argument) => session.SendPacket(session.GenerateDelayPacket(delay, type, argument));
    public static void RefreshMateStats(this IClientSession session) => session.SendPackets(session.GeneratePstPackets());
    public static void RefreshFairy(this IClientSession session) => session.SendPacket(session.GeneratePairyPacket());
    
    public static void RefreshMiniPet(this IClientSession session) => session.SendPacket(session.GenerateMiniPetPacket());
    public static void SendMapOutPacket(this IClientSession session) => session.SendPacket(session.GenerateMapOutPacket());
    public static void SendScnPackets(this IClientSession session) => session.SendPackets(session.GenerateScnPackets());
    public static void SendScpPackets(this IClientSession session, byte? page = null) => session.SendPackets(session.GenerateScpPackets(page));
    public static void SendPClearPacket(this IClientSession session) => session.SendPacket(session.GeneratePClearPacket());
    public static void SendScpStcPacket(this IClientSession session) => session.SendPacket(session.GenerateScpStcPacket());
    public static void SendRdiPacket(this IClientSession session, int vnum, int amount) => session.SendPacket(session.GenerateRdiPacket(vnum, amount));

    public static void RefreshSkillList(this IClientSession session) => session.SendPacket(session.GenerateSkillListPacket());
    public static void SendTitPacket(this IClientSession session) => session.SendPacket(session.GenerateTitPacket());
    public static void SendBfPacket(this IBattleEntity entity, Buff buff, int time, int charge = 0)
    {
        if (entity == null || buff == null)
        {
            return;
        }

        entity.MapInstance?.Broadcast(entity.GenerateBfPacket(buff, time, charge));
    }

    public static void SendBfLeftPacket(this IBattleEntity entity, Buff buff) => entity.MapInstance.Broadcast(entity.GenerateBfLeftPacket(buff));
    public static void SendStaticBuffUiPacket(this IClientSession session, Buff buff, int time) => session.SendPacket(session.GenerateStaticBuffPacket(buff, time, true));
    public static void SendEmptyStaticBuffUiPacket(this IClientSession session, Buff buff) => session.SendPacket(session.GenerateStaticBuffPacket(buff, 0, false));
    public static void UpdateVisibility(this IClientSession session) => session.Broadcast(session.GenerateClPacket());
    public static void SendBfPacket(this IBattleEntity entity, Buff buff) => entity.MapInstance.Broadcast(entity.GenerateBfPacket(buff));
    public static void SendWopenPacket(this IClientSession session, byte type, int data = 0, int data2 = 0, int data3 = 0) => session.SendPacket(session.GenerateWopenPacket(type, data, data2, data3));
    public static void OpenNosBazaarUi(this IClientSession session, MedalType medal, int time) => session.SendPacket(session.GenerateWopenBazaar(medal, time));
    public static void CloseNosBazaarUi(this IClientSession session)
    {
        string mapName = GameLanguage.GetMapName(MapManager.GetMapByMapId(session.PlayerEntity.MapId), session);
        session.SendDiscordRpcPacket($"{session.GetLanguage(GameDialogKey.PLAYING_IN_MAP_RPC)} {mapName}");
        session.PlayerEntity.HasNosBazaarOpen = false;
        session.SendWopenPacket((byte)WindowType.CLOSE_UI);
    }

    public static void SendShopEndPacket(this IClientSession session, ShopEndType type) => session.SendPacket(session.GenerateShopEnd(type));
    public static void SendExcClosePacket(this IClientSession session, ExcCloseType type) => session.SendPacket(session.GenerateExcClosePacket(type));
    public static void SendMzPacket(this IClientSession session, string ip, short port) => session.SendPacket(session.GenerateMzPacket(ip, port));
    public static void SendItPacket(this IClientSession session, ItModeType mode) => session.SendPacket(session.GenerateItPacket(mode));
    public static void SendTaClosePacket(this IClientSession session) => session.SendPacket(session.GenerateTaClosePacket());
    public static void SendTalentCameraPacket(this IClientSession session) => session.SendPacket(session.GenerateTalentCameraPacket());
    public static void SendTalentArenaTimerPacket(this IClientSession session) => session.SendPacket(session.GenerateTalentArenaTimerPacket());

    public static void SendClinitPacket(this IClientSession session, IEnumerable<CharacterDTO> characters) => session.SendPacket(session.GenerateTopComplimentPacket("clinit", characters));
    public static void SendFlinitPacket(this IClientSession session, IEnumerable<CharacterDTO> characters) => session.SendPacket(session.GenerateTopReputationPacket("flinit", characters));
    public static void SendKdlinitPacket(this IClientSession session, IEnumerable<CharacterDTO> characters) => session.SendPacket(session.GenerateTopPvPPointsPacket("kdlinit", characters));

    public static void SendBsInfoPacket(this IClientSession session, BsInfoType bsInfoType, GameType gameType, ushort time, QueueWindow window) =>
        session.SendPacket(session.GenerateBsInfoPacket(bsInfoType, gameType, time, window));

    public static void SendPdtiPacket(this IClientSession session, PdtiType type, int vnum, int amount, short wearSlot, ushort upgrade, short rare) =>
        session.SendPacket(session.GeneratePdtiPacket(type, vnum, amount, wearSlot, upgrade, rare));

    public static void SendParcelPacket(this IClientSession session, ParcelActionType actionType, MailType mailType, int giftId) =>
        session.SendPacket(session.GenerateParcelPacket(actionType, mailType, giftId));

    public static void SendTwkPacket(this IClientSession session) => session.SendPacket(session.GenerateTwkPacket());
    public static void SendZzimPacket(this IClientSession session) => session.SendPacket(session.GenerateZzimPacket());
    public static void SendRecipeNpcList(this IClientSession session, IEnumerable<Recipe> recipes) => session.SendPacket(session.GenerateRecipeNpcList(recipes));
    public static void SendRecipeItemList(this IClientSession session, IEnumerable<Recipe> recipes, IGameItem gameItem) => session.SendPacket(session.GenerateRecipeItemList(recipes, gameItem));
    public static void SendRecipeCraftItemList(this IClientSession session, Recipe recipe) => session.SendPacket(session.GenerateRecipeCraftItemList(recipe));
    
    public static void SendRecipeCraftSkillList(this IClientSession session, Recipe recipe, IReadOnlyList<Recipe> recipes) => session.SendPacket(session.GenerateRecipeCraftSkillList(recipe, recipes));
    public static void SendMessageUnderChat(this IClientSession session)
    {
        string key = "MESSAGE_UNDER_CHAT_";
        for (short i = 0; i < 10; i++)
        {
            if (!Enum.TryParse($"{key}{i}", out GameDialogKey gameDialogKey))
            {
                continue;
            }

            string message = session.GetLanguage(gameDialogKey);
            session.SendPacket(session.GenerateMessageUnderChat(i, message));
        }
    }

    public static void SendSound(this IClientSession session, SoundType type) => session.SendPacket(session.GenerateSound(type));
    
    public static void SendSound(this IClientSession session, IBattleEntity entity, SoundType type) => session.SendPacket(entity.GenerateSound(type));

    public static void BroadcastSoundInRange(this IClientSession session, short type) =>
        session.Broadcast(session.GenerateSound(type), new RangeBroadcast(session.PlayerEntity.PositionX, session.PlayerEntity.PositionY));

    public static void SendStaticBonuses(this IClientSession session) => session.SendPacket(session.GenerateStaticBonusPacket());
    public static void SendPstPackets(this IClientSession session) => session.SendPackets(session.GeneratePstPackets());

    public static void SendDancePacket(this IClientSession session, bool isOn)
    {
        if (!isOn)
        {
            return;
        }

        session.SendPacket(session.GenerateDance(true));
    }

    public static void SendMapMusicPacket(this IClientSession session, short music) => session.SendPacket(session.GenerateMapMusic(music));

    public static void TrySendScalPacket(this IClientSession session, IBattleEntity battleEntity = default)
    {
        IBattleEntity entityToScal = battleEntity ?? session.PlayerEntity;
        if (!entityToScal.ShouldSendScal())
        {
            return;
        }

        session.SendPacket(entityToScal.GenerateScal());
    }

    public static void RefreshZoom(this IClientSession session)
    {
        session.SendGuriPacket(15, session.PlayerEntity.SkillComponent.Zoom, session.PlayerEntity.Id);
    }

    public static void SendMinigameStart(this IClientSession session, MinigameType minigameType) => session.SendPacket(GenerateMinigameStart(minigameType));

    public static void SendStPacket(this IClientSession session, IBattleEntity entity) => session.SendPacket(entity.GenerateStPacket());

    public static void SendEmptySuPacket(this IClientSession session, SkillInfo skill) => session.SendPacket(session.GenerateEmptySuPacket(skill));

    public static void BroadcastGetPacket(this IClientSession session, long dropId) => session.Broadcast(session.PlayerEntity.GenerateGet(dropId));

    public static void BroadcastMateGetPacket(this IMateEntity mate, long dropId) => mate.MapInstance.Broadcast(mate.GenerateGet(dropId));

    public static void SendGbPacket(this IClientSession session, BankType type, IReputationConfiguration reputationConfiguration, IBankReputationConfiguration bankReputationConfiguration,
        IReadOnlyList<CharacterDTO> topReputation) =>
        session.SendPacket(session.GenerateGb(type, reputationConfiguration, bankReputationConfiguration, topReputation));

    public static void SendAmuletBuffPacket(this IClientSession session, GameItemInstance item) => session.SendPacket(session.PlayerEntity.GenerateAmuletBuff(item));

    public static void SendEmptyAmuletBuffPacket(this IClientSession session) => session.SendPacket(session.PlayerEntity.GenerateEmptyAmuletBuff());

    public static void SendEmptyRecipeCraftItem(this IClientSession session) => session.SendPacket(session.GenerateEmptyRecipeCraftItemPacket());
    public static void SendDiscordRpcPacket(this IClientSession session, string gameDialogKey = null) 
        => session.SendPacket(session.GenerateDiscordRpcPacket(gameDialogKey));
    public static void SendPtspInsertPacket(this IClientSession session, int pspLevel, long percentage, int pspNewLevel, long newPercentage)
        => session.SendPacket(session.GeneratePtspInsertPacket(pspLevel, percentage, pspNewLevel, newPercentage));
    public static void SendPtspUpdatePacket(this IClientSession session, long gold, int itemVNum, int amount)
        => session.SendPacket(session.GeneratePtspUpdatePacket(gold, itemVNum, amount));
    public static void SendPtspUpgradePacket(this IClientSession session, int pspLevel, long percentage, short slot)
        => session.SendPacket(session.GeneratePtspUpgradePacket(pspLevel, percentage, slot));
    
    public static void SendHcscorePacket(this IClientSession session, RaidParty raidParty) => session.SendPacket(session.GenerateHcscorePacket(raidParty));

    #endregion

    #region Generate Packets
    
    public static string GenerateHcscorePacket(this IClientSession session, RaidParty raidParty)
    {
        var raidMembersList = raidParty.Members
            .Select(member => member.PlayerEntity)
            .Where(player => player != null && player.HardcoreComponent.TotalRaidDamage > 0)
            .OrderByDescending(player => player.HardcoreComponent.TotalRaidDamage)
            .ToList();

        int totalRaidDamage = raidMembersList.Sum(player => player.HardcoreComponent.TotalRaidDamage);
        
        IEnumerable<string> topMembers = raidMembersList
            .Take(3)
            .Select(member =>
            {
                int totalDamage = member.HardcoreComponent.TotalRaidDamage;
                double percentage = totalRaidDamage > 0 ? (double)totalDamage / totalRaidDamage * 100 : 0;
                return $"{member.Morph}.{(byte)member.Class}.{member.Name}.{(byte)member.Gender}.{totalDamage / 10}.{percentage:F2}";
            });
        
        return $"hcscore {string.Join(" ", topMembers)}";
    }

    public static string GenerateTitPacket(this IClientSession session)
    {
        return $"tit {35 + (short)session.PlayerEntity.Class} {session.PlayerEntity.Name}";
    }

    public static string GenerateMinigameStart(MinigameType minigameType) => $"mlo_st {(byte)minigameType}";

    public static string GenerateSound(this IClientSession session, SoundType type) => $"guri 19 1 {session.PlayerEntity.Id} {(short)type} 0";
    public static string GenerateSound(this IClientSession session, short type) => $"guri 19 1 {session.PlayerEntity.Id} {type} 0";
    public static string GenerateSound(this IBattleEntity battleEntity, SoundType type) => $"guri 19 {(byte)battleEntity.Type} {battleEntity.Id} {(short)type}";

    public static string GenerateTaFcPacket(this IClientSession session, byte type) => $"ta_fc {type} {session.PlayerEntity.Id}";

    public static string GenerateMessageUnderChat(this IClientSession session, short count, string message) => $"bn {count} {message.Replace(" ", "^")}";

    public static string GenerateRecipeNpcList(this IClientSession session, IEnumerable<Recipe> recipes)
    {
        string recipeList = "m_list 2";

        recipeList = recipes.Aggregate(recipeList, (current, s) => s.Amount > 0 ? current + $" {s.ProducedItemVnum}" : string.Empty);
        recipeList += " -100";
        return recipeList;
    }

    public static string GenerateRecipeItemList(this IClientSession session, IEnumerable<Recipe> recipes, IGameItem gameItem)
    {
        string recipeList = "m_list 2";

        recipeList = recipes.Where(s => s.Amount > 0).Aggregate(recipeList, (current, s) => current + $" {s.ProducedItemVnum}");
        return recipeList + (gameItem.EffectValue is <= 111 and >= 109 ? " 999" : string.Empty);
    }

    public static string GenerateRecipeCraftItemList(this IClientSession session, Recipe recipe)
    {
        string recList = $"m_list 3 {recipe.Amount.ToString()}";
        recList = recipe.Items.Aggregate(recList, (current, ite) => ite.Amount > 0 ? current + $" {ite.ItemVNum.ToString()} {ite.Amount.ToString()}" : string.Empty);
        recList += " -1";
        return recList;
    }
    
    public static string GenerateRecipeCraftSkillList(this IClientSession session, Recipe recipe, IReadOnlyList<Recipe> recipes)
    {
        long craftTotal = -999;
        CharacterCookingDto log = session.PlayerEntity.CharacterCookingDto.FirstOrDefault(s => s.RecipeVnum == recipe.ProducedItemVnum);
        
        if (recipe.BearingChef == 1 && log != null)
        {
            craftTotal = log.Amount;
        }

        switch (recipe.BearingChef)
        {
            case 1 when log == null:
                craftTotal = 0;
                break;
            case 2:
            {
                if (session.PlayerEntity.HasUnlockedDesiredBearing(recipes, 1) && log != null)
                {
                    craftTotal = log.Amount;
                }

                if (session.PlayerEntity.HasUnlockedDesiredBearing(recipes, 1) && log == null)
                {
                    craftTotal = 0;
                }

                break;
            }
            case 3:
            {
                if (session.PlayerEntity.HasUnlockedDesiredBearing(recipes, 2) && log != null)
                {
                    craftTotal = log.Amount;
                } 

                if (session.PlayerEntity.HasUnlockedDesiredBearing(recipes, 2) && log == null)
                {
                    craftTotal = 0;
                }

                break;
            }
        }

        string recList = $"m_list 6 {craftTotal} {recipe.Amount}";
        recList = recipe.Items.Aggregate(recList, (current, ite) => ite.Amount > 0 ? current + $" {ite.ItemVNum} {ite.Amount}" : string.Empty);
        recList += " -1";
        return recList;
    }

    public static string GenerateEmptyRecipeCraftItemPacket(this IClientSession session) => "m_list 3 0 -1";
    
    public static string GeneratePtspInsertPacket(this IClientSession session, int pspLevel, long percentage, int pspNewLevel, long newPercentage)
    {
        var packet = new StringBuilder("ptsp_data ");

        packet.Append((byte)PtspDataType.Insert).Append(' ')
            .Append(pspLevel).Append(' ')
            .Append(percentage).Append(' ')
            .Append(pspNewLevel).Append(' ')
            .Append(newPercentage);

        return packet.ToString();
    }

    public static string GeneratePtspUpgradePacket(this IClientSession session, int pspLevel, long percentage, short slot)
    {
        var packet = new StringBuilder("ptsp_data ");

        packet.Append((byte)PtspDataType.Upgrade).Append(' ')
            .Append(pspLevel).Append(' ')
            .Append(percentage).Append(' ')
            .Append(slot).Append(' ')
            .Append(0);

        return packet.ToString();
    }

    public static string GeneratePtspUpdatePacket(this IClientSession session, long gold, int itemVNum, int amount)
    {
        var packet = new StringBuilder("ptsp_data ");

        packet.Append((byte)PtspDataType.Update).Append(' ')
            .Append(gold).Append(' ')
            .Append(itemVNum).Append(' ')
            .Append(amount).Append(' ')
            .Append(0);

        return packet.ToString();
    }

    public static string GenerateClPacket(this IClientSession session) =>
        $"cl {session.PlayerEntity.Id} {(session.PlayerEntity.Invisible ? 1 : 0)} {(session.PlayerEntity.CheatComponent.IsInvisible ? 1 : 0)}";

    public static string GenerateClForcedPacket(this IClientSession session, bool Forced = true) =>
        $"cl {session.PlayerEntity.Id} {(Forced ? 1 : 0)} {(session.PlayerEntity.CheatComponent.IsInvisible ? 1 : 0)}";

    public static string GenerateBfPacket(this IBattleEntity entity, Buff buff, int time, int charge) =>
        $"bf {(byte)entity.Type} {entity.Id} {charge}.{buff.CardId}.{(charge == 0 ? (int)buff.Duration.TotalMilliseconds == 0 ? time : (int)(buff.Duration.TotalMilliseconds / 100) : charge)} {buff.CasterLevel}";

    public static string GenerateBfLeftPacket(this IBattleEntity entity, Buff buff) =>
        $"bf {(byte)entity.Type} {entity.Id} 0.{buff.CardId}.{buff.RemainingTimeInMilliseconds() / 100} {buff.CasterLevel}";

    public static string GenerateStaticBuffPacket(this IClientSession session, Buff buff, int time, bool isActive) =>
        $"vb {buff.CardId} {(isActive ? 1 : 0)} {(!buff.IsNoDuration() ? time / 100 : -1)}";
    
    public static string GenerateFamilyBuffPacket(int buffId, int time, bool isActive) => $"vb {buffId} {(isActive ? 1 : 0)} {time / 100}";

    public static string GenerateBfPacket(this IBattleEntity entity, Buff buff) => $"bf {(byte)entity.Type} {entity.Id} 0.{buff.CardId}.0 {buff.CasterLevel}";

    public static string GenerateRdiPacket(this IClientSession session, int vnum, int amount) => $"rdi {vnum} {amount}";

    public static string GenerateScpStcPacket(this IClientSession session) => $"sc_p_stc {session.PlayerEntity.MaxPetCount / 10 - 1} {session.PlayerEntity.MaxPartnerCount - 3}";

    public static string GeneratePClearPacket(this IClientSession session) => "p_clear";

    public static string GenerateWopenPacket(this IClientSession session, byte type, int data = 0, int data2 = 0, int data3 = 0) => $"wopen {type} {data} {data2} {data3}";

    public static string GenerateWopenBazaar(this IClientSession session, MedalType medal, int time) => $"wopen 32 {(byte)medal} {time}";

    public static string GenerateShopEnd(this IClientSession session, ShopEndType type) => $"shop_end {(byte)type}";

    public static string GenerateExcClosePacket(this IClientSession session, ExcCloseType type) => $"exc_close {(byte)type}";

    public static string GenerateMzPacket(this IClientSession session, string ip, short port) => $"mz {ip} {port} {session.PlayerEntity.Slot}";

    public static string GenerateItPacket(this IClientSession session, ItModeType mode) => $"it {(byte)mode}";

    public static string GenerateTaClosePacket(this IClientSession session) => "ta_close";

    public static string GenerateTalentCameraPacket(this IClientSession session) => "ta_sv 0";

    public static string GenerateTalentArenaTimerPacket(this IClientSession session) => "ta_s";

    public static string GenerateBsInfoPacket(this IClientSession session, BsInfoType bsInfoType, GameType gameType, ushort time, QueueWindow window) =>
        $"bsinfo {(byte)bsInfoType} {(byte)gameType} {time} {(byte)window}";

    public static string GeneratePdtiPacket(this IClientSession session, PdtiType type, int vnum, int amount, short wearSlot, ushort upgrade, short rare) =>
        $"pdti {(byte)type} {vnum} {amount} {wearSlot} {upgrade} {rare}";

    public static string GenerateParcelPacket(this IClientSession session, ParcelActionType actionType, MailType mailType, int giftId) => $"parcel {(byte)actionType} {(byte)mailType} {giftId}";

    public static string GenerateTwkPacket(this IClientSession session) => $"twk 1 {session.PlayerEntity.Id} {session.Account.Name} {session.PlayerEntity.Name} shtmxpdlfeoqkr " +
        $"{session.Account.Language.ToString().ToLower()} {session.Account.Language.ToString().ToLower()}"; //the 1 is server id

    public static string GenerateZzimPacket(this IClientSession session) => "zzim";

    public static string GenerateEventAsk(this IClientSession session, QnamlType type, string packet, string message) => $"qnaml {(byte)type} #{packet.Replace(' ', '^')} {message}";
    
    public static string GenerateEventAsk(this IClientSession session, QnamlType type, string packet, Game18NConstString game18NConstString, I18NArgumentType i18NArgumentType, int goldAmount)
    {
        return $"qnamli {(byte)type} #{packet.Replace(' ', '^')} {(int)game18NConstString} {(byte)i18NArgumentType} {goldAmount} 0";
    }

    public static string GenerateTopComplimentPacket(this IClientSession session, string basicSeed, IEnumerable<CharacterDTO> characters)
        => characters.Aggregate(basicSeed, (current, dto) => current + $" {dto.Id.ToString()}|{dto.Level.ToString()}|{dto.HeroLevel.ToString()}|{dto.Compliment.ToString()}|{dto.Name}");

    public static string GenerateTopReputationPacket(this IClientSession session, string basicSeed, IEnumerable<CharacterDTO> characters)
        => characters.Aggregate(basicSeed, (current, dto) => current + $" {dto.Id.ToString()}|{dto.Level.ToString()}|{dto.HeroLevel.ToString()}|{dto.Reput.ToString()}|{dto.Name}");

    public static string GenerateTopPvPPointsPacket(this IClientSession session, string basicSeed, IEnumerable<CharacterDTO> characters)
        => characters.Aggregate(basicSeed, (current, dto) => current + $" {dto.Id.ToString()}|{dto.Level.ToString()}|{dto.HeroLevel.ToString()}|{dto.Act4Points.ToString()}|{dto.Name}");

    public static IEnumerable<string> GenerateScnPackets(this IClientSession session)
    {
        var list = new List<string>();
        foreach (IMateEntity mate in session.PlayerEntity.MateComponent.GetMates(x => x.MateType == MateType.Partner))
        {
            list.Add(mate.GenerateScPacket(GameLanguage, session.UserLanguage));
        }

        return list;
    }

    public static IEnumerable<string> GenerateScpPackets(this IClientSession session, byte? page)
    {
        var list = new List<string>();

        if (!page.HasValue)
        {
            foreach (IMateEntity s in session.PlayerEntity.MateComponent.GetMates(x => x.MateType == MateType.Pet))
            {
                list.Add(s.GenerateScPacket(GameLanguage, session.UserLanguage));
            }
        }
        else
        {
            foreach (IMateEntity s in session.PlayerEntity.MateComponent.GetMates(x => x.MateType == MateType.Pet).Skip(page.Value * 10).Take(10))
            {
                list.Add(s.GenerateScPacket(GameLanguage, session.UserLanguage));
            }
        }

        return list;
    }

    public static string GenerateMapOutPacket(this IClientSession session) => "mapout";

    public static string GenerateOutPacket(this IClientSession session) => $"out 1 {session.PlayerEntity.Id}";

    public static string GeneratePairyPacket(this IClientSession session)
    {
        bool isBuffed = session.PlayerEntity.HasBuff(BuffVnums.FAIRY_BOOSTER);

        GameItemInstance fairy = session.PlayerEntity.Fairy;
        return fairy == null
            ? $"pairy 1 {session.PlayerEntity.Id} 0 0 0 0"
            : $"pairy 1 {session.PlayerEntity.Id} 4 {session.PlayerEntity.Element} {fairy.ElementRate + fairy.GameItem.ElementRate + (session.PlayerEntity.Family?.UpgradeValues.GetValueOrDefault(FamilyUpgradeType.FAIRY_ELEMENT_BOOST).Item1 ?? 0)} {fairy.GameItem.Morph + (isBuffed && !FairyMorphsWithoutBoostChange.Contains(fairy.GameItem.Morph) ? 5 : 0)}";
    }
    
    public static string GenerateMiniPetPacket(this IClientSession session)
    {
        GameItemInstance miniPet = session.PlayerEntity.MiniPet;
        return $"minipet 1 {session.PlayerEntity.Id} {miniPet?.ItemVNum ?? 0}";
    }

    public static IEnumerable<string> GeneratePstPackets(this IClientSession session) => session.PlayerEntity.MateComponent.TeamMembers().OrderBy(s => s.MateType).Select(s => s.GeneratePst());

    public static string GenerateDelayPacket(this IClientSession session, int delay, GuriType type, string argument) => $"delay {delay} {(byte)type} #{argument.Replace(' ', '^')}";

    public static string GenerateAtPacket(this IClientSession session) =>
        "at " +
        $"{session.PlayerEntity.Id} " +
        $"{session.PlayerEntity.MapInstance.MapVnum} " +
        $"{session.PlayerEntity.PositionX} " +
        $"{session.PlayerEntity.PositionY} " +
        "2 " +
        $"{(byte)(session.IsGameMaster() ? 2 : 0)} " +
        $"{session.PlayerEntity.MapInstance.MapMusic ?? session.PlayerEntity.MapInstance.Music} " +
        $"1 " +
        "-1";

    public static string GenerateInventoryRemovePacket(this IClientSession session, InventoryType type, int slot) => $"ivn {(byte)type} {slot}.-1.0.0.0.0";

    public static string GenerateEqPacket(this IClientSession session)
    {
        int color = (byte)session.PlayerEntity.HairColor;
        GameItemInstance head = session.PlayerEntity.Hat;

        if (head != null && head.GameItem.IsColorable)
        {
            color = head.Design;
        }

        byte gmMode = 0;

        if (session.IsGameMaster())
        {
            gmMode = 2;

            if (session.PlayerEntity.CheatComponent.IsInvisible)
            {
                gmMode = 6;
            }
        }

        return
            $"eq {session.PlayerEntity.Id} {gmMode} {(byte)session.PlayerEntity.Gender} {(byte)session.PlayerEntity.HairStyle} {color} {(byte)session.PlayerEntity.Class} {session.GenerateEqListForPacket()} {session.GenerateEqRareUpgradeForPacket()}";
    }

    public static string GenerateLevelUpPacket(this IClientSession session) => $"levelup {session.PlayerEntity.Id}";

    public static string GenerateExtsPacket(this IClientSession session)
    {
        byte type = 0;
        if (session.PlayerEntity.HaveStaticBonus(StaticBonusType.EreniaMedal))
        {
            type += (byte)SpecialMedalType.Erenia;
        }

        if (session.PlayerEntity.HaveStaticBonus(StaticBonusType.AdventurerMedal))
        {
            type += (byte)SpecialMedalType.Adventurer;
        }

        if (session.PlayerEntity.HaveStaticBonus(StaticBonusType.SpecialistMedal))
        {
            type += (byte)SpecialMedalType.Specialist;
        }

        if (session.PlayerEntity.HaveStaticBonus(StaticBonusType.FriendshipMedal))
        {
            type += (byte)SpecialMedalType.Friendship;
        }

        return $"exts {type} {session.PlayerEntity.GetInventorySlots(false, InventoryType.Equipment)} {session.PlayerEntity.GetInventorySlots(false, InventoryType.Main)} {session.PlayerEntity.GetInventorySlots(false, InventoryType.Etc)}";
    }

    public static string GenerateSkillListPacket(this IClientSession session)
    {
        List<CharacterSkill> characterSkills = session.PlayerEntity.UseSp
            ? session.PlayerEntity.SkillsSp.Values.Where(s => s.Skill != null).OrderBy(s => s.Skill.CastId).ToList()
                .Concat(session.PlayerEntity.CharacterSkills.Values.Where(s => s.Skill != null)
                    .OrderBy(s => s.Skill.CastId)
                    .Where(s => s.Skill.CastId is >= 38 and <= 39))
                .Where(s => s.Skill.CastId is < 40 or > 44)
                .ToList()
            : session.PlayerEntity.CharacterSkills.Values.Where(s => s.Skill != null)
                .OrderBy(s => s.Skill.CastId)
                .Where(s => s.Skill.CastId is < 40 or > 44)
                .ToList();

        string skibase = string.Empty;
        if (session.PlayerEntity.UseSp && session.PlayerEntity.Specialist != null)
        {
            skibase = $"{characterSkills.ElementAt(0).SkillVNum} {characterSkills.ElementAt(0).SkillVNum}";
            characterSkills.AddRange(session.PlayerEntity.CharacterSkills.Where(s => s.Value.Skill.IsPassiveSkill()).Select(s => s.Value));
        }
        else
        {
            skibase = $"{200 + 20 * (byte)session.PlayerEntity.Class} {201 + 20 * (byte)session.PlayerEntity.Class}";

            if (session.PlayerEntity.Class == ClassType.MartialArtist)
            {
                skibase = "1525 1529";
            }
        }

        IEnumerable<CharacterSkill> tattooSkills = session.PlayerEntity.CharacterSkills.Values
            .Where(s => s.Skill.CastId is >= 40 and <= 44)
            .ToList();

        string tattooString = tattooSkills.Any()
            ? tattooSkills.Aggregate(string.Empty, (current, ski) => current + $" {ski.Skill?.Id}|{ski.UpgradeSkill}")
            : string.Empty;

        string generatedSkills = characterSkills?.Aggregate(string.Empty, (current, ski) => current + $" {ski.SkillVNum}");
        return $"ski 0 {skibase}{tattooString}{generatedSkills}";
    }

    public static string GenerateStaticBonusPacket(this IClientSession session)
    {
        string bonusList = string.Empty;
        foreach (CharacterStaticBonusDto bonus in session.PlayerEntity.GetStaticBonuses())
        {
            int time = bonus.DateEnd == null ? -1 : Math.Abs((int)(DateTime.UtcNow - bonus.DateEnd.Value).TotalHours);
            bonusList += $"{bonus.ItemVnum}.{time} ";
        }

        return $"umi {bonusList}";
    }

    public static string GenerateStatPacket(this IClientSession session, bool isTransform)
    {
        IPlayerEntity character = session.PlayerEntity;
        double option =
            (character.WhisperBlocked ? Math.Pow(2, (int)CharacterOption.WhisperBlocked - 1) : 0)
            + (character.FamilyRequestBlocked ? Math.Pow(2, (int)CharacterOption.FamilyRequestBlocked - 1) : 0)
            + (!character.MouseAimLock ? Math.Pow(2, (int)CharacterOption.MouseAimLock - 1) : 0)
            + (character.MinilandInviteBlocked ? Math.Pow(2, (int)CharacterOption.MinilandInviteBlocked - 1) : 0)
            + (character.ExchangeBlocked ? Math.Pow(2, (int)CharacterOption.ExchangeBlocked - 1) : 0)
            + (character.FriendRequestBlocked ? Math.Pow(2, (int)CharacterOption.FriendRequestBlocked - 1) : 0)
            + (character.EmoticonsBlocked ? Math.Pow(2, (int)CharacterOption.EmoticonsBlocked - 1) : 0)
            + (character.HpBlocked ? Math.Pow(2, (int)CharacterOption.HpBlocked - 1) : 0)
            + (character.BuffBlocked ? Math.Pow(2, (int)CharacterOption.BuffBlocked - 1) : 0)
            + (character.GroupRequestBlocked ? Math.Pow(2, (int)CharacterOption.GroupRequestBlocked - 1) : 0)
            + (character.HeroChatBlocked ? Math.Pow(2, (int)CharacterOption.HeroChatBlocked - 1) : 0)
            + (character.QuickGetUp ? Math.Pow(2, (int)CharacterOption.QuickGetUp - 1) : 0)
            + (character.HideHat ? Math.Pow(2, (int)CharacterOption.HideHat - 1) : 0)
            + (character.UiBlocked ? Math.Pow(2, (int)CharacterOption.UiBlocked - 1) : 0)
            + (character.HideCD ? Math.Pow(2, (int)CharacterOption.HideCD + 1) : 0)
            + (character.HideHPMP ? Math.Pow(2, (int)CharacterOption.HideHPMP + 1) : 0)
            + (!character.IsPetAutoRelive ? 64 : 0)
            + (!character.IsPartnerAutoRelive ? 128 : 0)
            + (!character.CanPerformAttack() ? 131072 : 0)
            + (!character.CanPerformMove() ? 262144 : 0);

        if (character.GameStartDate.AddSeconds(5) > DateTime.UtcNow)
        {
            return $"stat {character.Hp} {character.MaxHp} {character.Mp} {character.MaxMp} {(isTransform ? 1 : 0)} {option}";
        }

        if (character.Hp < 0)
        {
            character.Hp = 0;
        }

        if (character.Mp < 0)
        {
            character.Mp = 0;
        }

        if (character.Hp > character.MaxHp)
        {
            character.Hp = character.MaxHp;
        }

        if (character.Mp > character.MaxMp)
        {
            character.Mp = character.MaxMp;
        }

        return $"stat {character.Hp} {character.MaxHp} {character.Mp} {character.MaxMp} {(isTransform ? 1 : 0)} {option}";
    }

    public static string GenerateStatInfoPacket(this IClientSession session) =>
        $"st 1 {session.PlayerEntity.Id} {session.PlayerEntity.Level} {session.PlayerEntity.HeroLevel} {session.PlayerEntity.GetHpPercentage()} {session.PlayerEntity.GetMpPercentage()} {session.PlayerEntity.Hp} {session.PlayerEntity.Mp} {session.PlayerEntity.MaxHp} {session.PlayerEntity.MaxMp} {session.PlayerEntity.BuffComponent.GetAllBuffs().Aggregate(string.Empty, (current, buff) => current + $" {buff.CardId}")}";

    public static string GenerateStatCharPacket(this IClientSession session, bool refreshHpMp = true)
    {
        IPlayerEntity playerEntity = session.PlayerEntity;

        int mainAttackType = 0;
        int secondAttackType = 0;

        switch (playerEntity.Class)
        {
            case (byte)ClassType.Adventurer:
                mainAttackType = 0;
                secondAttackType = 1;
                break;

            case ClassType.Magician:
                mainAttackType = 2;
                secondAttackType = 1;
                break;

            case ClassType.Swordman:
                mainAttackType = 0;
                secondAttackType = 1;
                break;

            case ClassType.Archer:
                mainAttackType = 1;
                secondAttackType = 0;
                break;
        }

        GameItemInstance mainWeapon = session.PlayerEntity.MainWeapon;
        GameItemInstance secondWeapon = session.PlayerEntity.SecondaryWeapon;
        GameItemInstance armor = session.PlayerEntity.Armor;

        int weaponUpgrade = mainWeapon?.Upgrade ?? 0;
        int secondaryUpgrade = secondWeapon?.Upgrade ?? 0;
        int armorUpgrade = armor?.Upgrade ?? 0;

        session.PlayerEntity.RefreshCharacterStats(refreshHpMp);

        return
            "sc " +
            $"{mainAttackType} " +
            $"{weaponUpgrade.ToString()} " +
            $"{playerEntity.StatisticsComponent.MinDamage.ToString()} " +
            $"{playerEntity.StatisticsComponent.MaxDamage.ToString()} " +
            $"{playerEntity.StatisticsComponent.HitRate.ToString()} " +
            $"{playerEntity.StatisticsComponent.CriticalChance.ToString()} " +
            $"{playerEntity.StatisticsComponent.CriticalDamage.ToString()} " +
            $"{secondAttackType} " +
            $"{secondaryUpgrade.ToString()} " +
            $"{playerEntity.StatisticsComponent.SecondMinDamage.ToString()} " +
            $"{playerEntity.StatisticsComponent.SecondMaxDamage.ToString()} " +
            $"{playerEntity.StatisticsComponent.SecondHitRate.ToString()} " +
            $"{playerEntity.StatisticsComponent.SecondCriticalChance.ToString()} " +
            $"{playerEntity.StatisticsComponent.SecondCriticalDamage.ToString()} " +
            $"{armorUpgrade.ToString()} " +
            $"{playerEntity.StatisticsComponent.MeleeDefense.ToString()} " +
            $"{playerEntity.StatisticsComponent.MeleeDodge.ToString()} " +
            $"{playerEntity.StatisticsComponent.RangeDefense.ToString()} " +
            $"{playerEntity.StatisticsComponent.RangeDodge.ToString()} " +
            $"{playerEntity.StatisticsComponent.MagicDefense.ToString()} " +
            $"{playerEntity.StatisticsComponent.FireResistance.ToString()} " +
            $"{playerEntity.StatisticsComponent.WaterResistance.ToString()} " +
            $"{playerEntity.StatisticsComponent.LightResistance.ToString()} " +
            $"{playerEntity.StatisticsComponent.ShadowResistance.ToString()}";
    }

    public static string GenerateRevive(this IPlayerEntity character) =>
        $"revive 1 {character.Id} {(character.TimeSpaceComponent.IsInTimeSpaceParty ? character.TimeSpaceComponent.TimeSpace.Instance.Lives < 0 ? 1 : character.TimeSpaceComponent.TimeSpace.Instance.Lives : 0)}";

    public static string GenerateRevivalPacket(RevivalType revivalType) => $"revival {((int)revivalType).ToString()}";
    public static string GenerateGoldPacket(this IClientSession session) => $"gold {session.PlayerEntity.Gold} {session.Account.BankMoney / 100}";

    public static bool IsSpecialistCard(this GameItemInstance itemInstance) => itemInstance.GameItem.ItemType == ItemType.Specialist && !itemInstance.GameItem.IsPartnerSpecialist;
    
    public static BuffVnums? GetSpecialistBuff(this GameItemInstance itemInstance)
    {
        if (!itemInstance.IsSpecialistCard())
        {
            return null;
        }

        if (itemInstance.Upgrade < 20)
        {
            return null;
        }

        return (ElementType)itemInstance.SpGemElement switch
        {
            ElementType.Fire => BuffVnums.FIRE_DRAGON_BLESSING,
            ElementType.Water => BuffVnums.ICE_DRAGON_BLESSING,
            ElementType.Shadow => BuffVnums.SKY_DRAGON_BLESSING,
            ElementType.Light => BuffVnums.MOONLIGHT_DRAGON_BLESSING,
            _ => BuffVnums.NEUTRAL_DRAGON_BLESSING
        };
    }
    public static string GenerateLevPacket(this IClientSession session, ICharacterAlgorithm characterAlgorithm)
    {
        bool usingSp = session.PlayerEntity.Specialist != null && session.PlayerEntity.UseSp;
        bool isAdventurer = session.PlayerEntity.Class == ClassType.Adventurer && session.PlayerEntity.JobLevel < 20;
        return
            $"lev {session.PlayerEntity.Level}" +
            $" {(session.PlayerEntity.Level > 92 ? session.PlayerEntity.LevelXp / 1000 : session.PlayerEntity.LevelXp)}" +
            $" {(!usingSp ? session.PlayerEntity.JobLevel : session.PlayerEntity.Specialist.SpLevel)}" +
            $" {(!usingSp ? session.PlayerEntity.JobLevelXp : session.PlayerEntity.Specialist.Xp)}" +
            $" {(session.PlayerEntity.Level > 92 ? session.PlayerEntity.GetLevelXp(characterAlgorithm) / 1000 : session.PlayerEntity.GetLevelXp(characterAlgorithm))}" +
            $" {(!usingSp ? session.PlayerEntity.GetJobXp(characterAlgorithm, isAdventurer) : session.PlayerEntity.GetSpJobXp(characterAlgorithm, session.PlayerEntity.Specialist.IsFunSpecialist()))}" +
            $" {session.PlayerEntity.Reput} {session.PlayerEntity.GetCp()}" +
            $" {session.PlayerEntity.HeroXp}" +
            $" {session.PlayerEntity.HeroLevel}" +
            $" {session.PlayerEntity.GetHeroXp(characterAlgorithm)}" +
            " 0";
    }

    public static bool IsFunSpecialist(this GameItemInstance itemInstance) => itemInstance.ItemVNum is (short)ItemVnums.PIRATE_SP or 
        (short)ItemVnums.PIRATE_SP_EVENT or (short)ItemVnums.PYJAMA_SP or (short)ItemVnums.CHICKEN_SP;
    
    public static bool IsTrainerSpecialist(this GameItemInstance itemInstance) => itemInstance.ItemVNum is 
        (short)ItemVnums.TRAINER_SPECIALIST_LIMITED or (short)ItemVnums.TRAINER_SPECIALIST;
    
    public static bool IsChefSpecialist(this GameItemInstance itemInstance) => itemInstance.ItemVNum is 
        (short)ItemVnums.CHEF_SPECIALIST or (short)ItemVnums.CHEF_SPECIALIST_LIMITED;
    
    public static bool IsAnglerSpecialist(this GameItemInstance itemInstance) => itemInstance.ItemVNum is 
        (short)ItemVnums.ANGLER_SPECIALIST or (short)ItemVnums.ANGLER_SPECIALIST_LIMITED;

    public static bool IsLifestyleSpecialistCard(this GameItemInstance itemInstance) =>
        itemInstance.ItemVNum is
            (short)ItemVnums.PIRATE_SP or
            (short)ItemVnums.PIRATE_SP_EVENT or
            (short)ItemVnums.PYJAMA_SP or
            (short)ItemVnums.CHICKEN_SP or
            (short)ItemVnums.TRAINER_SPECIALIST_LIMITED or
            (short)ItemVnums.TRAINER_SPECIALIST or
            (short)ItemVnums.CHEF_SPECIALIST or
            (short)ItemVnums.CHEF_SPECIALIST_LIMITED or
            (short)ItemVnums.ANGLER_SPECIALIST or
            (short)ItemVnums.ANGLER_SPECIALIST_LIMITED;

    public static string GenerateFdPacket(this IClientSession session, IReputationConfiguration reputationConfiguration, IReadOnlyList<CharacterDTO> topReputation) =>
        $"fd {session.PlayerEntity.Reput} {(byte)session.PlayerEntity.GetReputationIcon(reputationConfiguration, topReputation)} {(int)session.PlayerEntity.Dignity} {Math.Abs(session.PlayerEntity.GetDignityIco())}";

    public static string GenerateCInfoPacket(this IClientSession session, IFamily family, IReputationConfiguration reputationConfiguration, IReadOnlyList<CharacterDTO> topReputation)
    {
        int morphOutput = (session.PlayerEntity.UseSp || session.PlayerEntity.IsOnVehicle ? session.PlayerEntity.Morph : 0);
        if (session.PlayerEntity.UseSp && session.PlayerEntity.Specialist.Skin != 0 && !session.PlayerEntity.IsOnVehicle)
        {
            morphOutput = session.PlayerEntity.Specialist.Skin;
        }
        
        string name = session.Account.Authority is > AuthorityType.User and <= AuthorityType.DEV ? $"[{session.Account.Authority}]{session.PlayerEntity.Name}" : session.PlayerEntity.Name;
        
        int complimentRank;
        switch (session.Account.Authority)
        {
            case AuthorityType.VIP:
            case AuthorityType.SUP:
                complimentRank = 70;
                break;
            case AuthorityType.VIP_E:
            case AuthorityType.SUP_E:
                complimentRank = 80;
                break;
            case AuthorityType.VIP_X:
            case AuthorityType.SUP_X:
            case AuthorityType.GS:
                complimentRank = 100;
                break;
            default:
                complimentRank = session.PlayerEntity.Compliment;
                break;
        }
        
        return "c_info" +
            $" {name}" +
            " -" +
            $" {(session.PlayerEntity.IsInGroup() ? session.PlayerEntity.GetGroup().GroupId : -1)}" +
            $" {(family != null ? $"{family.Id}.-1 " + $"{family.Name}({GameLanguage.GetLanguage(session.PlayerEntity.GetFamilyAuthority().GetMemberLanguageKey(), session.UserLanguage) ?? ""})" : "-1 -")}" +
            $" {session.PlayerEntity.Id}" +
            $" {(session.IsGameMaster() ? 3 : 0)}" +
            $" {(byte)session.PlayerEntity.Gender}" +
            $" {(byte)session.PlayerEntity.HairStyle}" +
            $" {(byte)session.PlayerEntity.HairColor}" +
            $" {(byte)session.PlayerEntity.Class}" +
            $" {(session.PlayerEntity.GetDignityIco() == 1 ? (byte)session.PlayerEntity.GetReputationIcon(reputationConfiguration, topReputation) : -session.PlayerEntity.GetDignityIco())}" +
            $" {complimentRank}" +
            $" {morphOutput}" +
            $" {(session.PlayerEntity.Invisible || session.PlayerEntity.CheatComponent.IsInvisible ? 1 : 0)}" +
            $" {family?.Level ?? 0}" +
            $" {(session.PlayerEntity.UseSp && session.PlayerEntity.Specialist != null ? session.PlayerEntity.Specialist.Upgrade : 0)}" +
            $" {(session.PlayerEntity.UseSp ? session.PlayerEntity.MorphUpgrade : 0)}" +
            $" {session.PlayerEntity.ArenaWinner}";
    }

    public static GameDialogKey GetMemberLanguageKey(this FamilyAuthority authority)
    {
        return authority switch
        {
            FamilyAuthority.Head => GameDialogKey.FAMILY_RANK_HEAD,
            FamilyAuthority.Deputy => GameDialogKey.FAMILY_RANK_DEPUTY,
            FamilyAuthority.Keeper => GameDialogKey.FAMILY_RANK_KEEPER,
            _ => GameDialogKey.FAMILY_RANK_MEMBER
        };
    }

    public static string GenerateCMapPacket(this IClientSession session, bool isEntering) =>
        $"c_map 0 {session.PlayerEntity.MapInstance?.MapNameId.ToString() ?? ""} {(isEntering ? 1 : 0)}";

    public static string GenerateCModePacket(this IClientSession session)
    {
        GameItemInstance wingsCostume = session.PlayerEntity.Wings;
        int morphOutput = (session.PlayerEntity.UseSp || session.PlayerEntity.IsOnVehicle || session.PlayerEntity.IsMorphed || 
            session.PlayerEntity.IsSeal ? session.PlayerEntity.Morph : 0);
        
        if (session.PlayerEntity.UseSp && session.PlayerEntity.Specialist.Skin != 0 && !session.PlayerEntity.IsOnVehicle && !session.PlayerEntity.IsSeal)
        {
            morphOutput = session.PlayerEntity.Specialist.Skin;
        }
        
        return $"c_mode 1 {session.PlayerEntity.Id} " +
            $"{morphOutput} " +
            $"{(session.PlayerEntity.UseSp ? session.PlayerEntity.MorphUpgrade : 0)} " +
            $"{(session.PlayerEntity.UseSp || !session.PlayerEntity.IsSeal ? session.PlayerEntity.MorphUpgrade2 : 0)} " +
            $"{session.PlayerEntity.ArenaWinner} {session.PlayerEntity.Size} {(wingsCostume == null ? 0 : wingsCostume.GameItem.Morph)}";
    }

    public static string GenerateCondPacket(this IClientSession session) =>
        $"cond 1 {session.PlayerEntity.Id} {(session.PlayerEntity.CanPerformAttack() == false ? 1 : 0)} {(session.PlayerEntity.CanPerformMove() == false ? 1 : 0)} {session.PlayerEntity.Speed}";

    public static string GenerateInPacket(this IClientSession session, IReputationConfiguration reputationConfiguration, IReadOnlyList<CharacterDTO> topReputation, bool foe = false,
        bool showInEffect = false)
    {
        IPlayerEntity character = session.PlayerEntity;

        bool isOnAct4 = session.CurrentMapInstance.HasMapFlag(MapFlags.ACT_4);
        byte faction = (byte)(isOnAct4 && !session.IsGameMaster() ? character.Faction == FactionType.Angel ? 3 : 4 : 0);
        
        string name = character.Name;
        if (foe && !session.IsGameMaster())
        {
            name = "!§$%&/()=?*+~#";
        }

        int color = (int)character.HairColor;
        GameItemInstance headWearable = character.Hat;
        if (headWearable?.GameItem.IsColorable == true)
        {
            color = headWearable.Design;
        }

        CharacterTitleDto titleVnum = session.PlayerEntity.Titles.FirstOrDefault(x => x.IsVisible);
        bool shouldChangeMorph = character.IsUsingFairyBooster() && character.Fairy?.GameItem.Morph > 4 && character.Fairy.GameItem.Morph != 9 && character.Fairy.GameItem.Morph != 14;
        IFamily family = character.Family;
        
        IRankingManager rankingManager = StaticRankingManager.Instance;
        bool isFirstPlace = rankingManager.MonthlyExpFamilyId.HasValue && rankingManager.MonthlyExpFamilyId.Value == family?.Id;
        bool isAngelFirstPlace = rankingManager.MonthlyPvpAngelFamilyId.HasValue && rankingManager.MonthlyPvpAngelFamilyId.Value == family?.Id;
        bool isDemonFirstPlace = rankingManager.MonthlyPvpDemonFamilyId.HasValue && rankingManager.MonthlyPvpDemonFamilyId.Value == family?.Id;
        bool isRainbowBattlePlace = rankingManager.MonthlyRainbowBattleFamilyId.HasValue && rankingManager.MonthlyRainbowBattleFamilyId.Value == family?.Id;

        string familyId = $"{(foe ? "-1" : family != null ? $"{family.Id}.-1" : -1)}";
        
        int morphOutput = (character.UseSp || character.IsOnVehicle || character.IsMorphed || character.IsSeal ? character.Morph : 0);
        if (character.UseSp && character.Specialist.Skin != 0 && !character.IsOnVehicle && !character.IsSeal)
        {
            morphOutput = character.Specialist.Skin;
        }
        
        return "in 1 " +
            $"{name} " +
            "- " +
            $"{character.Id} " +
            $"{character.PositionX} " +
            $"{character.PositionY} " +
            $"{character.Direction} " +
            $"{(session.IsGameMaster() ? 2 : 0)} " +
            $"{(byte)character.Gender} " +
            $"{(byte)character.HairStyle} " +
            $"{color} " +
            $"{(byte)character.Class} " +
            $"{session.GenerateEqListForPacket()} " +
            $"{character.GetHpPercentage()} " +
            $"{character.GetMpPercentage()} " +
            $"{(character.IsSitting ? 1 : 0)} " +
            $"{(character.IsInGroup() ? character.GetGroup().GroupId : -1)} " +
            $"{(character.Fairy != null ? 4 : 0)} " +
            $"{character.Element} " +
            $"{(character.IsUsingFairyBooster() ? 1 : 0)} " +
            $"{character.Fairy?.GameItem.Morph + (shouldChangeMorph ? 5 : 0) ?? 0} " +
            $"{(showInEffect ? 0 : 1)} " +
            $"{morphOutput} " +
            $"{session.GenerateEqRareUpgradeForPacket(true)} " +
            $"{familyId} " +
            $"{(foe ? name : family?.Name ?? "-")} " +
            $"{(character.GetDignityIco() == 1 ? (byte)character.GetReputationIcon(reputationConfiguration, topReputation) : -character.GetDignityIco())} " +
            $"{(character.Invisible ? 1 : 0)} " +
            $"{(character.UseSp ? character.MorphUpgrade : 0)} " +
            $"{faction} " +
            $"{(character.UseSp ? character.MorphUpgrade2 : 0)} " +
            $"{character.Level} " +
            $"{family?.Level ?? 0} " +
            $"{(isFirstPlace ? 1 : 0)}|{(isRainbowBattlePlace ? 1 : 0)}|{(isAngelFirstPlace ? 1 : isDemonFirstPlace ? 2 : 0)} " +
            $"{character.ArenaWinner} " +
            $"{character.Compliment} " +
            $"{character.Size} " +
            $"{character.HeroLevel} " +
            $"{(!isOnAct4 ? titleVnum?.ItemVnum ?? 0 : 0)} " + "" +
            "0";
    }

    public static TitInfoPacket GenerateTitInfoPacket(this IClientSession session) => new()
    {
        VisualType = session.PlayerEntity.Type,
        VisualId = session.PlayerEntity.Id,
        VisibleTitleVnum = session.PlayerEntity.Titles.FirstOrDefault(s => s.IsVisible)?.ItemVnum ?? 0,
        EffectTitleVnum = session.PlayerEntity.Titles.FirstOrDefault(s => s.IsEquipped)?.ItemVnum ?? 0
    };

    public static TitlePacket GenerateTitlePacket(this IClientSession session) => new()
    {
        Titles = session.PlayerEntity.Titles.Select(x => new TitleSubPacket
        {
            ItemVnum = x.TitleId,
            State = x.IsEquipped ? x.IsVisible ? TitleStatus.EquippedEffectAndVisible :
                TitleStatus.EquippedEffect :
                x.IsVisible ? TitleStatus.EquippedVisible : TitleStatus.Available
        }).ToList()
    };

    public static EffectServerPacket GenerateEffectPacket(this IClientSession session, EffectType effectType) => session.GenerateEffectPacket((short)effectType);

    public static EffectServerPacket GenerateEffectPacket(this IClientSession session, int effectId) => new()
    {
        EffectType = 1,
        CharacterId = session.PlayerEntity.Id,
        Id = effectId
    };

    public static string GenerateFactionPacket(this IClientSession session) => $"fs {(byte)session.PlayerEntity.Faction}";

    public static string GenerateEqListForPacket(this IClientSession session)
    {
        string[] inventoryArray = new string[18];
        for (short i = 0; i < 18; i++)
        {
            var equipmentType = (EquipmentType)i;
            InventoryItem invItem = session.PlayerEntity.GetInventoryItemFromEquipmentSlot(equipmentType);
            GameItemInstance item = invItem?.ItemInstance;
            if (item != null)
            {
                string itemVnum = equipmentType is EquipmentType.CostumeHat or EquipmentType.CostumeSuit && item.Skin != 0 ? item.Skin.ToString() : item.ItemVNum.ToString();
                inventoryArray[i] = itemVnum;
            }
            else
            {
                inventoryArray[i] = "-1";
            }
        }

        return
            $"{(session.PlayerEntity.HideHat ? "0" : inventoryArray[(byte)EquipmentType.Hat])}.{inventoryArray[(byte)EquipmentType.Armor]}.{inventoryArray[(byte)EquipmentType.MainWeapon]}.{inventoryArray[(byte)EquipmentType.SecondaryWeapon]}.{inventoryArray[(byte)EquipmentType.Mask]}.{inventoryArray[(byte)EquipmentType.Fairy]}.{inventoryArray[(byte)EquipmentType.CostumeSuit]}.{(session.PlayerEntity.HideHat ? "0" : inventoryArray[(byte)EquipmentType.CostumeHat])}.{inventoryArray[(byte)EquipmentType.WeaponSkin]}.{inventoryArray[(byte)EquipmentType.Wings]}.{inventoryArray[(byte)EquipmentType.MiniPet]}";
    }

    public static string GenerateEqRareUpgradeForPacket(this IClientSession session, bool isInPacket = false)
    {
        GameItemInstance mainWeapon = session.PlayerEntity.MainWeapon;
        GameItemInstance armor = session.PlayerEntity.Armor;

        short weaponRare = mainWeapon?.Rarity ?? 0;
        byte weaponUpgrade = mainWeapon?.Upgrade ?? 0;
        short armorRare = armor?.Rarity ?? 0;
        byte armorUpgrade = armor?.Upgrade ?? 0;

        if (!isInPacket)
        {
            return $"{weaponUpgrade}{weaponRare} {armorUpgrade}{armorRare}";
        }

        var mainWeaponString = new StringBuilder();
        var armorString = new StringBuilder();

        if (mainWeapon == null)
        {
            mainWeaponString.Append('0');
        }
        else
        {
            int upgrade = weaponUpgrade * 10;
            mainWeaponString.Append(upgrade == 0 ? $"{weaponRare}" : $"{weaponUpgrade}{weaponRare}");
        }

        if (armor == null)
        {
            armorString.Append('0');
        }
        else
        {
            int upgrade = armorUpgrade * 10;
            armorString.Append(upgrade == 0 ? $"{armorRare}" : $"{armorUpgrade}{armorRare}");
        }

        return $"{mainWeaponString} {armorString}";
    }

    public static string GenerateEquipPacket(this IClientSession session)
    {
        string equipments = string.Empty;
        short weaponRare = 0;
        byte weaponUpgrade = 0;
        short armorRare = 0;
        byte armorUpgrade = 0;

        foreach (InventoryItem invItem in session.PlayerEntity.EquippedItems)
        {
            GameItemInstance item = invItem?.ItemInstance;
            
            if (item == null)
            {
                continue;
            }
            int shellEffectUpgradeCount = (item.EquipmentOptions?.Count(shellEffect => shellEffect.ShellEffectUpgrade) ?? 0) is var resultat && resultat == 0 ? 0 : resultat + 80;

            switch (item.GameItem.EquipmentSlot)
            {
                case EquipmentType.Armor:
                    {
                        armorRare = item.Rarity;
                        armorUpgrade = item.Upgrade;
                        break;
                    }
                case EquipmentType.MainWeapon:
                    {
                        weaponRare = item.Rarity;
                        weaponUpgrade = item.Upgrade;
                        break;
                    }
            }

            equipments +=
                $" {(byte)item.GameItem.EquipmentSlot}.{item.GameItem.Id}.{item.Rarity}.{(item.GameItem.IsColorable ? item.Design : item.Upgrade)}.0.{item.GetCarvedRunesInformation(true)}.{shellEffectUpgradeCount}";
        }

        return $"equip {(weaponUpgrade == 0 ? string.Empty : weaponUpgrade.ToString())}{weaponRare} {(armorUpgrade == 0 ? string.Empty : armorUpgrade.ToString())}{armorRare}{equipments}";
    }


    public static string GenerateReqInfo(this IClientSession session, IReputationConfiguration reputationConfiguration, IReadOnlyList<CharacterDTO> topReputation)
    {
        GameItemInstance mainWeapon = session.PlayerEntity.MainWeapon;
        GameItemInstance secondWeapon = session.PlayerEntity.SecondaryWeapon;
        GameItemInstance armor = session.PlayerEntity.Armor;
        GameItemInstance specialist = session.PlayerEntity.Specialist;

        bool hasMainWeapon = mainWeapon != null;
        bool hasSecondWeapon = secondWeapon != null;
        bool hasArmor = armor != null;

        bool isPvpPrimary = hasMainWeapon && mainWeapon.GameItem.Name.Contains(": ");
        bool isPvpSecondary = hasSecondWeapon && secondWeapon.GameItem.Name.Contains(": ");
        bool isPvpArmor = hasArmor && armor.GameItem.Name.Contains(": ");

        IPlayerEntity character = session.PlayerEntity;
        IFamily family = character.Family;

        return "tc_info " +
            $"{character.Level} " +
            $"{character.Name} " +
            $"{character.Element} " +
            $"{character.ElementRate} " +
            $"{(byte)character.Class} " +
            $"{(byte)character.Gender} " +
            $"{family?.Id ?? -1} " +
            $"{(family == null ? "-" : family.Name)} " +
            $"{(byte)session.PlayerEntity.GetReputationIcon(reputationConfiguration, topReputation)} " +
            $"{session.PlayerEntity.GetDignityIco()} " +
            $"{(hasMainWeapon ? 1 : 0)} " +
            $"{(hasMainWeapon ? mainWeapon.Rarity : 0)} " +
            $"{(hasMainWeapon ? mainWeapon.Upgrade : 0)} " +
            $"{(hasSecondWeapon ? 1 : 0)} " +
            $"{(hasSecondWeapon ? secondWeapon.Rarity : 0)} " +
            $"{(hasSecondWeapon ? secondWeapon.Upgrade : 0)} " +
            $"{(hasArmor ? 1 : 0)} " +
            $"{(hasArmor ? armor.Rarity : 0)} " +
            $"{(hasArmor ? armor.Upgrade : 0)} " +
            $"{character.Act4Kill} " +
            $"{character.Act4Dead} " +
            $"{character.Reput} " +
            "0 " +
            "0 " +
            "0 " +
            $"{(character.UseSp && specialist != null ? specialist.GameItem.Morph : 0)} " +
            $"{character.TalentWin} " +
            $"{character.TalentLose} " +
            $"{character.TalentSurrender} " +
            $"{character.MasterPoints} " +
            $"{character.MasterTicket} " +
            $"{character.Compliment} " +
            $"{character.Act4Points} " +
            $"{(isPvpPrimary ? 1 : 0)} " +
            $"{(isPvpSecondary ? 1 : 0)} " +
            $"{(isPvpArmor ? 1 : 0)} " +
            $"{character.HeroLevel} " +
            $"0 " + //Fairy Upgrade Level
            $"{(character.Biography == null ? (int)Game18NConstString.NoIntroMessage : character.Biography)}";
    }

    public static string GenerateEmptySuPacket(this IClientSession session, SkillInfo skillInfo)
        => "su " +
            $"{(byte)session.PlayerEntity.Type} " +
            $"{session.PlayerEntity.Id} " +
            $"{(byte)session.PlayerEntity.Type} " +
            $"{session.PlayerEntity.Id} " +
            $"{skillInfo.Vnum} " +
            $"{skillInfo.Cooldown} " +
            $"{skillInfo.HitAnimation} " +
            "-1 " +
            "0 " +
            "1 " +
            $"{session.PlayerEntity.GetHpPercentage()} " +
            "0 " +
            "-2 " +
            "0";

    public static string GenerateGet(this IBattleEntity entity, long id) => $"get {(byte)entity.Type} {entity.Id} {id} 0";

    public static string GenerateAmuletBuff(this IBattleEntity entity, GameItemInstance item)
    {
        if (item.DurabilityPoint != 0)
        {
            return $"bf {(byte)entity.Type} {entity.Id} {item.DurabilityPoint}.62.{item.GameItem.LeftUsages} {entity.Level}";
        }

        return $"bf {(byte)entity.Type} {entity.Id} 0.62.{(item.ItemDeleteTime.HasValue ? (long)(item.ItemDeleteTime.Value - DateTime.UtcNow).TotalSeconds * 10 : 0)} {entity.Level}";
    }

    public static string GenerateEmptyAmuletBuff(this IBattleEntity entity) => $"bf {(byte)entity.Type} {entity.Id} 0.62.0 {entity.Level}";

    public static string GenerateDance(this IClientSession session, bool isOn) => $"dance {(isOn ? 2.ToString() : string.Empty)}";

    public static string GenerateMapMusic(this IClientSession session, short music) => $"bgm2 {music}";
    
    public static string GenerateDiscordRpcPacket(this IClientSession session, string gameDialogKey = null)
    {
        int characterClass = (int)session.PlayerEntity.Class;
        int characterGender = (int)session.PlayerEntity.Gender;
        int characterMorph = session.PlayerEntity.UseSp ? session.PlayerEntity.Morph : 0;
        string mapName = GameLanguage.GetMapName(MapManager.GetMapByMapId(session.PlayerEntity.MapId), session);
        MapInstanceType characterMapInstanceType = session.CurrentMapInstance.MapInstanceType;
        string characterName = session.PlayerEntity.Name;

        var mapInstanceActivities = new Dictionary<MapInstanceType, GameDialogKey>
        {
            { MapInstanceType.LandOfDeath, GameDialogKey.LAND_OF_DEATH_RPC },
        };
        
        string activity = !string.IsNullOrEmpty(gameDialogKey)
            ? gameDialogKey
            : mapInstanceActivities.TryGetValue(characterMapInstanceType, out GameDialogKey dialogKey)
                ? session.GetLanguage(dialogKey)
                : $"{session.GetLanguage(GameDialogKey.PLAYING_IN_MAP_RPC)} {mapName}";
        
        return $"discord_rpc {characterClass} {characterGender} {characterMorph} {characterName} {activity}";
    }

    #endregion
}