using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Foundatio.Utility;
using PhoenixLib.Events;
using PhoenixLib.Scheduler;
using WingsAPI.Data.Character;
using WingsAPI.Data.Fish;
using WingsAPI.Packets.Enums;
using WingsEmu.DTOs.BCards;
using WingsEmu.DTOs.Buffs;
using WingsEmu.DTOs.Skills;
using WingsEmu.Game._enum;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Fish;
using WingsEmu.Game.Inventory;
using WingsEmu.Game.Inventory.Event;
using WingsEmu.Game.Items;
using WingsEmu.Game.Managers.StaticData;
using WingsEmu.Game.Monster.Event;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Skills;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Battle;
using WingsEmu.Packets.Enums.Chat;
using WingsEmu.Packets.ServerPackets.Battle;

namespace WingsEmu.Game.Extensions;

public static class SkillExtension
{
    public static SkillInfo GetInfo(this SkillDTO skill, PartnerSkill partnerSkill = null, IBattleEntity battleEntity = null)
    {
        var dictionary = new Dictionary<SkillCastType, HashSet<BCardDTO>>();
        foreach (BCardDTO bCard in skill.BCards)
        {
            var key = (SkillCastType)bCard.CastType;
            if (!dictionary.TryGetValue(key, out HashSet<BCardDTO> hashSet))
            {
                hashSet = new HashSet<BCardDTO>();
                dictionary[key] = hashSet;
            }

            hashSet.Add(bCard);
        }

        if (battleEntity != null && !battleEntity.IsPlayer())
        {
            IReadOnlyList<BCardDTO> onAttackBCards = battleEntity.BCardComponent.GetTriggerBCards(BCardTriggerType.ATTACK);
            foreach (BCardDTO bCard in onAttackBCards)
            {
                var key = (SkillCastType)bCard.CastType;
                if (!dictionary.TryGetValue(key, out HashSet<BCardDTO> hashSet))
                {
                    hashSet = new HashSet<BCardDTO>();
                    dictionary[key] = hashSet;
                }

                hashSet.Add(bCard);
            }
        }

        return new SkillInfo
        {
            Vnum = skill.Id,
            AttackType = skill.AttackType,
            AoERange = skill.AoERange,
            BCards = skill.BCards,
            CastAnimation = skill.CtAnimation,
            CastEffect = skill.CtEffect,
            SkillType = skill.SkillType,
            CastTime = skill.CastTime,
            Cooldown = skill.Cooldown,
            CastId = skill.CastId,
            Element = skill.Element,
            HitAnimation = skill.SuAnimation,
            HitEffect = skill.SuEffect,
            HitType = skill.HitType,
            TargetType = skill.TargetType,
            Combos = skill.Combos,
            TargetAffectedEntities = skill.TargetAffectedEntities,
            Range = skill.Range,
            IsUsingSecondWeapon = skill.IsUsingSecondWeapon,
            IsComboSkill = skill.SpecialCost == 999,
            ManaCost = skill.MpCost,
            PartnerSkillRank = partnerSkill?.Rank,
            BCardsType = dictionary,
            Class = skill.Class
        };
    }

    public static SkillInfo GetUpgradedSkill(this IPlayerEntity playerEntity, SkillDTO originalSkill, ICardsManager cardsManager, ISkillsManager skillsManager)
    {
        if (!playerEntity.SkillComponent.SkillUpgrades.TryGetValue((short)originalSkill.Id, out HashSet<IBattleEntitySkill> hashSet))
        {
            return null;
        }

        if (!hashSet.Any())
        {
            return null;
        }

        hashSet = hashSet.OrderBy(x => x.Skill.UpgradeType).ToHashSet();

        BCardDTO[] upgradesBCards = hashSet.SelectMany(x => x.Skill.BCards).ToArray();

        SkillDTO getBasicSkill = skillsManager.GetSkill(originalSkill.Id).DeepClone();

        var bCards = new List<BCardDTO>(getBasicSkill.BCards);
        var dictionary = new Dictionary<SkillCastType, HashSet<BCardDTO>>();

        foreach (BCardDTO bCard in bCards)
        {
            var key = (SkillCastType)bCard.CastType;
            if (!dictionary.TryGetValue(key, out HashSet<BCardDTO> hashSetBCards))
            {
                hashSetBCards = new HashSet<BCardDTO>();
                dictionary[key] = hashSetBCards;
            }

            hashSetBCards.Add(bCard);
        }

        foreach (BCardDTO upgradeBCard in upgradesBCards)
        {
            var key = (SkillCastType)upgradeBCard.CastType;
            if (!dictionary.TryGetValue(key, out HashSet<BCardDTO> hashSetBCards))
            {
                hashSetBCards = new HashSet<BCardDTO>();
                dictionary[key] = hashSetBCards;
            }

            if (upgradeBCard.Type != (short)BCardType.Buff)
            {
                if (upgradeBCard.FirstDataScalingType == BCardScalingType.NORMAL_VALUE && upgradeBCard.SecondDataScalingType == BCardScalingType.NORMAL_VALUE)
                {
                    BCardDTO findOriginalBCard = getBasicSkill.BCards.FirstOrDefault(x => x.Type == upgradeBCard.Type && x.SubType == upgradeBCard.SubType);
                    if (findOriginalBCard == null)
                    {
                        bCards.Add(upgradeBCard);
                        if (!hashSetBCards.Contains(upgradeBCard))
                        {
                            hashSetBCards.Add(upgradeBCard);
                        }

                        continue;
                    }

                    findOriginalBCard.FirstData += upgradeBCard.FirstData;
                    findOriginalBCard.SecondData += upgradeBCard.SecondData;
                    hashSetBCards.Add(findOriginalBCard);
                    continue;
                }

                BCardDTO originalBCard = getBasicSkill.BCards.FirstOrDefault(x => x.Type == upgradeBCard.Type && x.SubType == upgradeBCard.SubType);
                if (originalBCard == null)
                {
                    bCards.Add(upgradeBCard);
                    hashSetBCards.Add(upgradeBCard);
                    continue;
                }

                hashSetBCards.Remove(originalBCard);
                bCards.Remove(originalBCard);
                bCards.Add(upgradeBCard);
                if (!hashSetBCards.Contains(upgradeBCard))
                {
                    hashSetBCards.Add(upgradeBCard);
                }

                continue;
            }

            BCardDTO[] buffBCards = getBasicSkill.BCards.Where(x => x.Type == (short)BCardType.Buff && x.SubType == (byte)AdditionalTypes.Buff.ChanceCausing).ToArray();

            bool buffsExist = false;

            if (buffBCards.Any())
            {
                buffsExist = true;
                foreach (BCardDTO buffBCard in buffBCards)
                {
                    int first = buffBCard.SecondData;
                    int second = upgradeBCard.SecondData;

                    Card firstBuff = cardsManager.GetCardByCardId(first);
                    Card secondBuff = cardsManager.GetCardByCardId(second);

                    if (firstBuff == null || secondBuff == null)
                    {
                        bCards.Add(upgradeBCard);
                        hashSetBCards.Add(upgradeBCard);
                        continue;
                    }

                    if (firstBuff.GroupId != secondBuff.GroupId)
                    {
                        bCards.Add(upgradeBCard);
                        hashSetBCards.Add(upgradeBCard);
                        continue;
                    }

                    hashSetBCards.Remove(buffBCard);
                    bCards.Remove(buffBCard);
                    bCards.Add(upgradeBCard);
                    hashSetBCards.Add(upgradeBCard);
                }
            }

            if (buffsExist)
            {
                continue;
            }

            bCards.Add(upgradeBCard);
            hashSetBCards.Add(upgradeBCard);
        }

        SkillDTO[] skills = hashSet.Select(x => x.Skill).ToArray();
        int manaCost = originalSkill.MpCost + skills.Sum(x => x.MpCost);
        byte aoeRange = (byte)(originalSkill.AoERange + skills.Sum(x => x.AoERange));
        byte range = (byte)(originalSkill.Range + skills.Sum(x => x.Range));
        short castTime = (short)(originalSkill.CastTime + skills.Sum(x => x.CastTime));
        short cooldown = (short)(originalSkill.Cooldown + skills.Sum(x => x.Cooldown));
        SkillDTO element = skills.LastOrDefault(x => x.Element != 0);
        SkillDTO ctAnimation = skills.LastOrDefault(x => x.CtAnimation != -1);
        SkillDTO ctEffect = skills.LastOrDefault(x => x.CtEffect != -1);
        SkillDTO suAnimation = skills.LastOrDefault(x => x.SuAnimation != 0);
        SkillDTO suEffect = skills.LastOrDefault(x => x.SuEffect != -1);

        return new SkillInfo
        {
            Vnum = originalSkill.Id,
            AttackType = originalSkill.AttackType,
            AoERange = aoeRange,
            BCards = bCards,
            CastAnimation = ctAnimation?.CtAnimation ?? originalSkill.CtAnimation,
            CastEffect = ctEffect?.CtEffect ?? originalSkill.CtEffect,
            SkillType = originalSkill.SkillType,
            CastTime = castTime,
            Cooldown = cooldown,
            CastId = originalSkill.CastId,
            Element = element?.Element ?? originalSkill.Element,
            HitAnimation = suAnimation?.SuAnimation ?? originalSkill.SuAnimation,
            HitEffect = suEffect?.SuEffect ?? originalSkill.SuEffect,
            HitType = originalSkill.HitType,
            TargetType = originalSkill.TargetType,
            Combos = originalSkill.Combos,
            TargetAffectedEntities = originalSkill.TargetAffectedEntities,
            Range = range,
            IsUsingSecondWeapon = originalSkill.IsUsingSecondWeapon,
            IsComboSkill = originalSkill.SpecialCost == 999,
            ManaCost = manaCost,
            BCardsType = dictionary,
            Class = originalSkill.Class
        };
    }

    public static void CancelCastingSkill(this IBattleEntity entity)
    {
        entity.RemoveCastingSkill();
        if (entity is not IPlayerEntity character)
        {
            return;
        }

        character.Session.SendCancelPacket(CancelType.NotInCombatMode);
    }

    public static void SendSpecialistGuri(this IClientSession session, int castId)
    {
        if (session.PlayerEntity.Specialist == null)
        {
            return;
        }

        #region Types

        switch (session.PlayerEntity.Specialist.GameItem.Morph)
        {
            case 1: // Pajama
                int type = 0;
                if (castId == 1)
                {
                    type = 14;
                }

                if (castId >= 2 && castId <= 4)
                {
                    type = 18 + castId;
                }

                if (castId == 5)
                {
                    type = 26;
                }

                if (castId >= 6 && castId <= 10)
                {
                    type = 24 + castId;
                }

                session.CurrentMapInstance.Broadcast(session.GenerateGuriPacket(6, 1, session.PlayerEntity.Id, type));
                break;
            case 16: // Pirate
                session.CurrentMapInstance.Broadcast(session.GenerateGuriPacket(6, 1, session.PlayerEntity.Id, 43));
                break;
        }

        #endregion
    }

    public static List<CharacterStaticBuffDto> GetSavedBuffs(this IPlayerEntity character)
    {
        var staticBuffList = new List<CharacterStaticBuffDto>();
        if (!character.BuffComponent.HasAnyBuff())
        {
            return staticBuffList;
        }

        IReadOnlyList<Buff> savedBuffs = character.BuffComponent.GetAllBuffs(x => x.BuffFlags is BuffFlag.BIG_AND_KEEP_ON_LOGOUT or BuffFlag.SAVING_ON_DISCONNECT);

        if (!savedBuffs.Any())
        {
            return staticBuffList;
        }

        foreach (Buff buff in savedBuffs)
        {
            var newBuff = new CharacterStaticBuffDto
            {
                CardId = buff.CardId,
                CharacterId = character.Id,
                RemainingTime = buff.RemainingTimeInMilliseconds()
            };

            staticBuffList.Add(newBuff);
        }

        return staticBuffList;
    }


    public static short GetCannoneerHitEffect(this IPlayerEntity character, int castId)
    {
        return castId switch
        {
            4 => 4522,
            6 => 4561,
            7 => 4573,
            8 => -1,
            10 => 4572
        };
    }
    
    public static bool ContainsAllItems(List<short> a, List<short> b) => a.All(b.Contains);
    public static bool CanCollectFish(this IBattleEntity e, CharacterSkill s)
    {
        if (e is not IPlayerEntity player)
        {
            return true;
        }
        
        if (s.SkillVNum != (int)SkillsVnums.REEL_IN)
        {
            return true;
        }

        if (e.BuffComponent.HasBuff((int)BuffVnums.FISH_LINE))
        {
            return true;
        }

        player.Session.SendSayi(ChatMessageColorType.White, Game18NConstString.OnlyAvailableWhileFishing);
        return false;
    }

    public static bool CanFish(this IBattleEntity entity, CharacterSkill characterSkill, IFishManager fishManager, IRandomGenerator randomGenerator)
    {
        if (characterSkill.SkillVNum != (int)SkillsVnums.CAST_LINE && characterSkill.SkillVNum != (int)SkillsVnums.CAST_LINE_PRO)
        {
            return true;
        }

        if (entity is not IPlayerEntity player)
        {
            return true;
        }

        if (entity.BuffComponent.HasBuff((int)BuffVnums.FISH_LINE))
        {
            player.Session.SendSayi(ChatMessageColorType.White, Game18NConstString.AlreadyFishing);
            return false;
        }

        BCardDTO skill = characterSkill.Skill.BCards.Find(s =>
                    s.Type.Equals((byte)BCardType.Buff) &&
                    s.SubType.Equals((byte)AdditionalTypes.Buff.ChanceCausing) &&
                    s.SecondData.Equals((int)BuffVnums.FISH_LINE));
        
        if (skill == null)
        {
            player.Session.SendSayi(ChatMessageColorType.White, Game18NConstString.OnlyAvailableWhileFishing);
            return true;
        }

        FishingSpotDto fishType = fishManager.GetFishSpotByMapId(entity.MapInstance.MapId);

        if (fishType == null)
        {
            player.Session.SendSayi(ChatMessageColorType.White, Game18NConstString.CanNotFishHere);
            return false;
        }

        if (player.Specialist != null && player.Specialist.SpLevel < fishType.MinLvl)
        {
            player.Session.SendSayi(ChatMessageColorType.White, Game18NConstString.FishingLevelTooLow);
            return false;
        }

        FishingPathDto findPath = fishType.Paths.Find(s => s.X == entity.PositionX && s.Y == entity.PositionY);

        if (findPath == null)
        {
            player.Session.SendSayi(ChatMessageColorType.White, Game18NConstString.CanNotFishHere);
            return false;
        }

        if (!player.HasItem((int)ItemVnums.BAIT))
        {
            player.Session.SendSayi(ChatMessageColorType.White, Game18NConstString.NoBaitForFishing);
            return false;
        }


        if (!player.HasItem((int)ItemVnums.FISHING_LINE))
        {
            player.Session.SendSayi(ChatMessageColorType.White, Game18NConstString.NoFishingLine);
            return false;
        }

        entity.Direction = (byte)findPath.Dir;
        entity.MapInstance.Broadcast(entity.GenerateDir());
        int percent = player.BCardComponent.GetAllBCardsInformation(BCardType.Fishing, (byte)AdditionalTypes.Fishing.MoreChanceToDontUseBait, player.Level).firstData;
        
        if (percent != 0 && randomGenerator.RandomNumber() < percent)
        {
            return true;
        }

        player.Session.EmitEvent(new InventoryRemoveItemEvent((int)ItemVnums.BAIT));
        return true;
    }
    
    public static bool SkillCanBeUsed(this IBattleEntity entity, CharacterSkill skill)
    {
        if (entity is not IPlayerEntity player)
        {
            return skill.LastUse <= DateTime.UtcNow;
        }

        switch (player.Morph)
        {
            case (byte)MorphType.DraconicFist when skill.SkillVNum is < (int)SkillsVnums.JAB or > (int)SkillsVnums.TRANSFORMATION_DRAGON:
            case (byte)MorphType.DraconicFistTransformed when skill.SkillVNum is < (int)SkillsVnums.FLAMING_YO_YO or > (int)SkillsVnums.TRANSFORMATION_HAETAE:
                return false;
            default:
                return skill.LastUse <= DateTime.UtcNow;
        }
    }
    
    public static double GetSkillCooldownRealTime(this IBattleEntity entity, IBattleEntitySkill skill)
    {
        double cooldownTime = skill.Skill.Cooldown * 1000;
        double elapsed = (DateTime.UtcNow - skill.LastUse).TotalMilliseconds;
        double remainingCooldown = cooldownTime - elapsed;
        return Math.Max(0, remainingCooldown);
    }
    
    public static bool IsPassiveSkill(this SkillDTO skill) => skill.SkillType == SkillType.Passive;

    public static bool SkillCanBeUsed(this IBattleEntity entity, PartnerSkill skill) => skill != null && skill.LastUse <= DateTime.UtcNow;

    public static bool SkillCanBeUsed(this IBattleEntity entity, IBattleEntitySkill skill, in DateTime date) => skill != null && skill.LastUse <= date && entity.Mp >= skill.Skill.MpCost;
    
    public static bool SkillCanBeUsedSp(this IBattleEntity entity, IBattleEntitySkill skill) => skill != null && skill.LastUse <= DateTime.UtcNow;
    public static bool SkillCanBeUsed(this IBattleEntity entity, IBattleEntitySkill skill)
    {
        double cooldownTime = skill.Skill.Cooldown * 1000;
        double elapsed = (DateTime.UtcNow - skill.LastUse).TotalMilliseconds;
        return elapsed >= cooldownTime;
    }

    public static string GenerateOnyxGuriPacket(this IBattleEntity entity, short x, short y) => $"guri 31 {(byte)entity.Type} {entity.Id} {x} {y}";

    public static SkillInfo GetFakeTeleportSkill(this IBattleEntity entity) =>
        new()
        {
            Cooldown = 600,
            CastId = 8
        };

    public static SkillInfo GetFakeBuddhaSkill(this IBattleEntity entity) =>
    new()
    {
        Cooldown = 600,
        CastId = 9
    };

    public static SkillInfo GetFakeBombSkill(this IBattleEntity entity) =>
        new()
        {
            Cooldown = 600,
            CastId = 4
        };

    public static async Task TryDespawnBomb(this IPlayerEntity playerEntity, IAsyncEventPipeline asyncEventPipeline)
    {
        if (!playerEntity.SkillComponent.BombEntityId.HasValue)
        {
            return;
        }

        IMonsterEntity bomb = playerEntity.MapInstance?.GetMonsterById(playerEntity.SkillComponent.BombEntityId.Value);
        if (bomb?.MapInstance == null)
        {
            return;
        }

        await asyncEventPipeline.ProcessEventAsync(new MonsterDeathEvent(bomb));

        SkillInfo fakeBombSkill = playerEntity.GetFakeBombSkill();
        playerEntity.BroadcastSuPacket(playerEntity, fakeBombSkill, 0, SuPacketHitMode.NoDamageSuccess);
        playerEntity.Session.SendSkillCooldownResetAfter(fakeBombSkill.CastId, (short)playerEntity.ApplyCooldownReduction(fakeBombSkill));
        playerEntity.CancelCastingSkill();
        playerEntity.SetSkillCooldown(fakeBombSkill);
        playerEntity.SkillComponent.BombEntityId = null;
    }

    public static ElementType? GetBuffElementType(this IBattleEntity entity, short skillVnum)
    {
        return skillVnum switch
        {
            (short)SkillsVnums.FLAME => ElementType.Fire,
            (short)SkillsVnums.ICE => ElementType.Water,
            (short)SkillsVnums.HALO => ElementType.Light,
            (short)SkillsVnums.DARKNESS => ElementType.Shadow,
            (short)SkillsVnums.NO_ELEMENT => ElementType.Neutral,
            _ => null
        };
    }
}