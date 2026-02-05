using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PhoenixLib.Scheduler;
using WingsAPI.Data.Fish;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsAPI.Game.Extensions.ItemExtension.Item;
using WingsAPI.Game.Extensions.Quicklist;
using WingsEmu.DTOs.BCards;
using WingsEmu.DTOs.Skills;
using WingsEmu.Game;
using WingsEmu.Game._enum;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Core.SkillHandling;
using WingsEmu.Game.Core.SkillHandling.Event;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Extensions.Mates;
using WingsEmu.Game.Fish;
using WingsEmu.Game.GameEvent;
using WingsEmu.Game.Helpers.Damages;
using WingsEmu.Game.Items;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Managers.ServerData;
using WingsEmu.Game.Managers.StaticData;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Mates;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Raids;
using WingsEmu.Game.Skills;
using WingsEmu.Packets.ClientPackets;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Battle;
using WingsEmu.Packets.Enums.Character;
using WingsEmu.Packets.Enums.Chat;
using WingsEmu.Plugins.GameEvents.DataHolder;

namespace WingsEmu.Plugins.PacketHandling.Game.Battle;

public class UseSkillPacketHandler : GenericGamePacketHandlerBase<UseSkillPacket>
{
    private readonly ICardsManager _cardsManager;
    private readonly IDelayManager _delayManager;
    private readonly IGameLanguageService _gameLanguage;
    private readonly IItemsManager _items;
    private readonly RainbowBattleConfiguration _rainbowBattleConfiguration;
    private readonly ISkillsManager _skillsManager;
    private readonly IBuffFactory _buffFactory;
    private readonly IFishManager _fishManager;
    private readonly IRandomGenerator _randomGenerator;
    private readonly IRecipeManager _recipeManager;

    public UseSkillPacketHandler(IGameLanguageService gameLanguage, IDelayManager delayManager, IItemsManager items, ICardsManager cardsManager, ISkillsManager skillsManager,
        RainbowBattleConfiguration rainbowBattleConfiguration, IBuffFactory buffFactory, IFishManager fishManager, IRandomGenerator randomGenerator, IRecipeManager recipeManager)
    {
        _gameLanguage = gameLanguage;
        _delayManager = delayManager;
        _items = items;
        _cardsManager = cardsManager;
        _skillsManager = skillsManager;
        _rainbowBattleConfiguration = rainbowBattleConfiguration;
        _buffFactory = buffFactory;
        _fishManager = fishManager;
        _randomGenerator = randomGenerator;
        _recipeManager = recipeManager;
    }

    protected override async Task HandlePacketAsync(IClientSession session, UseSkillPacket packet)
    {
        session.SendDebugMessage("[U_S] Start u_s");
        IPlayerEntity character = session.PlayerEntity;

        if (!BasicChecks(session, packet))
        {
            return;
        }

        List<IBattleEntitySkill> skills = session.PlayerEntity.Skills;
        var characterSkill = skills.FirstOrDefault(s => s.Skill?.CastId == packet.CastId
            && (s.Skill?.UpgradeSkill == 0 || s.Skill?.SkillType == SkillType.NormalPlayerSkill) && s.Skill.TargetType != TargetType.NonTarget) as CharacterSkill;
        SkillDTO skill = characterSkill?.Skill;

        IMateEntity partner = session.PlayerEntity.MateComponent.GetTeamMember(x => x.MateType == MateType.Partner);
        NpcMonsterSkill npcPartnerSkill = null;
        if (partner != null && packet.CastId is 38 or 39)
        {
            IBattleEntitySkill partnerSkill = partner.Skills?.FirstOrDefault(x => x?.Skill != null && partner.Level >= x.Skill.LevelMinimum && x.Skill.CastId == packet.CastId);
            npcPartnerSkill = partnerSkill as NpcMonsterSkill;
            if (npcPartnerSkill == null)
            {
                session.SendCancelPacket(CancelType.NotInCombatMode);
                return;
            }

            skill = npcPartnerSkill.Skill;
        }

        if (skill == null)
        {
            session.SendCancelPacket(CancelType.NotInCombatMode);
            session.SendDebugMessage("[U_S] Skill does not exist");
            return;
        }
        
        if (skill.BCards.Any(x => x.Type == (short)BCardType.TokenBasedAbilities && x.SubType == (byte)AdditionalTypes.TokenBasedAbilities.CanOnlyCastWithTokens))
        {
            BCardDTO tokenRequirement = skill.BCards.First(x => x.Type == (short)BCardType.TokenBasedAbilities && x.SubType == (byte)AdditionalTypes.TokenBasedAbilities.CanOnlyCastWithTokens);
            int tokensRequired = tokenRequirement.FirstDataValue(character.Level);

            if (character.TokenGauge < tokensRequired)
            {
                session.SendCancelPacket(CancelType.NotInCombatMode);
                session.SendDebugMessage("[U_S] Not enough tokens to cast the skill");
                return;
            }
        }

        if (skill.ItemVNum != 0 && !session.PlayerEntity.HasItem(skill.ItemVNum))
        {
            session.SendCancelPacket(CancelType.NotInCombatMode);
            IGameItem gameItem = _items.GetItem(skill.ItemVNum);
            if (gameItem == null)
            {
                return;
            }

            string itemName = gameItem.GetItemName(_gameLanguage, session.UserLanguage);
            session.SendInformationChatMessage(_gameLanguage.GetLanguageFormat(GameDialogKey.INVENTORY_SHOUTMESSAGE_NOT_ENOUGH_ITEMS, session.UserLanguage, 1, itemName));
            return;
        }

        SkillInfo skillInfo = session.PlayerEntity.GetUpgradedSkill(skill, _cardsManager, _skillsManager) ?? skill.GetInfo();

        if (skillInfo.AoERange != 0 && skill.CastId != 0 && skill.CastId != 1 && session.PlayerEntity.HasBuff(BuffVnums.EXPLOSIVE_ENCHACMENT))
        {
            (int firstData, int secondData) buff = session.PlayerEntity.BCardComponent.GetAllBCardsInformation(BCardType.FireCannoneerRangeBuff,
                (byte)AdditionalTypes.FireCannoneerRangeBuff.AOEIncreased, session.PlayerEntity.Level);
            skillInfo.AoERange += (byte)buff.firstData;
            skillInfo.HitEffect = session.PlayerEntity.GetCannoneerHitEffect(skill.CastId);
        }

        if (skillInfo.AoERange != 0)
        {
            int increaseAoE = character.BCardComponent.GetAllBCardsInformation(BCardType.TargetAreaAttackSkills, (byte)AdditionalTypes.TargetAreaAttackSkills.TargetAreaAttackSkillsIncrease, character.Level).firstData;

            skillInfo.AoERange += (short)increaseAoE;
        }

        skillInfo.Range += (byte)session.PlayerEntity.BCardComponent.GetAllBCardsInformation(BCardType.FearSkill, (byte)AdditionalTypes.FearSkill.AttackRangedIncreased, session.PlayerEntity.Level)
            .Item1;

        bool canBeUsed = true;
        bool comboSkill = skill.CastId > 10 && character.UseSp && skill.SpecialCost == 999 && character.Morph != (int)MorphType.MasterWolf && character.Morph != (int)MorphType.MysticArts;

        if (npcPartnerSkill != null)
        {
            if (!partner.SkillCanBeUsed(npcPartnerSkill, DateTime.UtcNow))
            {
                session.SendCancelPacket(CancelType.NotInCombatMode);
                return;
            }
        }
        else
        {
            canBeUsed = character.CharacterCanCastOrCancel(characterSkill, _gameLanguage, skillInfo, false, _fishManager, _randomGenerator);
        }

        if (!canBeUsed)
        {
            session.SendCancelPacket(CancelType.NotInCombatMode);
            session.SendDebugMessage("[U_S] !canBeUsed");
            return;
        }

        if (!session.PlayerEntity.CanPerformAttack(skillInfo))
        {
            session.SendCancelPacket(CancelType.NotInCombatMode);
            return;
        }

        if (character.BCardComponent.HasBCard(BCardType.AngerSkill, (byte)AdditionalTypes.AngerSkill.OnlyNormalAttacks) && (skill.Price < 1 && skill.CastId != 0))
        {
            session.SendCancelPacket(CancelType.InCombatMode);
            session.SendMsg(_gameLanguage.GetLanguage(GameDialogKey.CANNOT_BE_USED, session.UserLanguage), MsgMessageType.Middle);
            return;
        }

        if (comboSkill)
        {
            ComboSkillState comboSkillState = session.PlayerEntity.GetComboState();
            if (comboSkillState == null)
            {
                session.SendCancelPacket(CancelType.NotInCombatMode);
                session.SendDebugMessage("[U_S] comboSkills == null");
                return;
            }

            bool canCastComboSkill = comboSkillState.LastSkillByCastId == skillInfo.CastId;
            if (!canCastComboSkill)
            {
                session.SendCancelPacket(CancelType.NotInCombatMode);
                session.SendDebugMessage("[U_S] comboSkill && comboSkill == null");
                return;
            }

            if (session.PlayerEntity.AngelElement.HasValue)
            {
                if (!character.HasBuff(BuffVnums.MAGIC_SPELL))
                {
                    session.SendCancelPacket(CancelType.NotInCombatMode);
                    session.SendDebugMessage("[U_S] Character without MAGIC_SPELL");
                    return;
                }

                ElementType? elementType = character.GetBuffElementType((short)skill.Id);

                if (!elementType.HasValue)
                {
                    session.SendCancelPacket(CancelType.NotInCombatMode);
                    session.SendDebugMessage("[U_S] ElementType == null");
                    return;
                }

                if (session.PlayerEntity.AngelElement.Value != elementType.Value)
                {
                    session.SendCancelPacket(CancelType.NotInCombatMode);
                    session.SendDebugMessage("[U_S] Element != skill.Element");
                    return;
                }
            }
        }

        IBattleEntity target = character.MapInstance.GetBattleEntity(packet.VisualType, packet.MapMonsterId);

        if (await TargetChecks(session, target, skillInfo))
        {
            return;
        }

        if (session.PlayerEntity.RainbowBattleComponent.IsFrozen)
        {
            session.SendCancelPacket(CancelType.NotInCombatMode);
            return;
        }

        if (skillInfo.TargetType == TargetType.Self || (skillInfo.TargetType == TargetType.SelfOrTarget && target.Id == character.Id))
        {
            session.SendDebugMessage("[U_S] Target = character");
            target = character;
        }

        if (skillInfo.TargetAffectedEntities == TargetAffectedEntities.BuffForAllies && character.IsEnemyWith(target))
        {
            target = character;
        }

        int cellSizeBonus = target switch
        {
            IPlayerEntity => 7,
            _ => 3
        };

        if (target is INpcMonsterEntity npcMonsterEntity)
        {
            cellSizeBonus += npcMonsterEntity.CellSize;
        }

        if (!character.Position.IsInRange(target.Position, skillInfo.Range + cellSizeBonus) && skillInfo.AttackType != AttackType.Dash)
        {
            session.SendCancelPacket(CancelType.NotInCombatMode);
            session.SendDebugMessage($"[U_S] Out of range {character.Position.GetDistance(target.Position)} - {skill.Range}");
            return;
        }

        if (target is IMonsterEntity mob)
        {
            if (!session.PlayerEntity.CanMonsterBeAttacked(mob) && !mob.IsMateTrainer && !mob.IsSparringMonster)
            {
                session.SendCancelPacket(CancelType.NotInCombatMode);
                return;
            }

            if (mob.MonsterRaceType == MonsterRaceType.Fixed)
            {
                session.SendCancelPacket(CancelType.NotInCombatMode);
                return;
            }
        }

        // it should check before taking Mp/Hp
        if (character.IsAllyWith(target) && skillInfo.TargetType != TargetType.Self && skillInfo.TargetType != TargetType.SelfOrTarget && target.Id != character.Id &&
            skillInfo.TargetAffectedEntities != TargetAffectedEntities.BuffForAllies)
        {
            character.CancelCastingSkill();
            character.Session.SendDebugMessage("[U_S] !caster.IsEnemyWith(target) && caster.IsPlayer() && skill.TargetType != TargetType.Self");
            return;
        }

        if (!character.IsEnemyWith(target) && skillInfo.TargetType != TargetType.Self && skillInfo.TargetType != TargetType.SelfOrTarget)
        {
            if (skillInfo.TargetType == TargetType.Target && skillInfo.TargetAffectedEntities != TargetAffectedEntities.BuffForAllies)
            {
                character.CancelCastingSkill();
                return;
            }
        }

        Position positionAfterDash = default;

        IReadOnlyList<Recipe> recipes = _recipeManager.GetRecipesBySkillnum(skillInfo.Vnum);
        if (!packet.MapX.HasValue && !packet.MapY.HasValue && recipes != null)
        {
            character.LastSkillId = skillInfo.Vnum;

            if (skill.CastId != 1)
            {
                character.Session.SendPacket("m_list 9 2");
            }
            string list = recipes.Where(s => s.Amount > 0)
                .Aggregate("m_list 2", (current, s) => current + $" {s.ProducedItemVnum}");
            list += " -300";
            character.Session.SendPacket(list);
            character.Session.SendPacket($"wopen 98 {skill.CastId} 0 0");
            character.CancelCastingSkill();
            return;
        }

        if (packet.MapX.HasValue && packet.MapY.HasValue && recipes != null)
        {
            Console.WriteLine("Recipes: " + (recipes != null ? "Exists" : "Null"));
            Console.WriteLine("SkillInfo.Vnum: " + skillInfo.Vnum);
            
            if (character.LastSkillId != skillInfo.Vnum)
            {
                character.CancelCastingSkill();
                return;
            }

            Recipe recipe = recipes.FirstOrDefault(s => s.ProducedItemVnum == packet.MapX.Value);
            if (recipe == null)
            {
                return;
            }
            character.LastRecipeFromChefSp = recipe;
            await character.Session.EmitEventAsync(new CookingMealEvent(skillInfo.Vnum, packet.MapX.Value, packet.MapY.Value, recipes));
            return;
        }

        if (packet.MapX.HasValue && packet.MapY.HasValue)
        {
            if (skillInfo.AttackType != AttackType.Dash)
            {
                session.SendDebugMessage("[U_S] Skill.AttackType != Dash");
                session.SendCancelPacket(CancelType.NotInCombatMode);
                return;
            }

            if (character.MapInstance.IsBlockedZone((int)packet.MapX, (int)packet.MapY))
            {
                session.SendCancelPacket(CancelType.NotInCombatMode);
                return;
            }

            var newPosition = new Position((short)packet.MapX, (short)packet.MapY);
            if (!character.Position.IsInRange(newPosition, skillInfo.Range + 2))
            {
                session.SendDebugMessage("[U_S] newPosition !IsInRange");
                session.SendCancelPacket(CancelType.NotInCombatMode);
                return;
            }

            positionAfterDash = newPosition;
        }

        if (session.PlayerEntity.SkillComponent.BuddhaWordsActivated)
        {
            session.RemoveBuddha();

            if (skillInfo.Vnum == (short)SkillsVnums.BUDDHAS_WORDS)
            {
                session.SendCancelPacket(CancelType.NotInCombatMode);
                return;
            }
        }

        switch (skillInfo.Vnum)
        {
            case (short)SkillsVnums.DHA_PREMIUM:
                IMateEntity mate = session.PlayerEntity.MateComponent.GetTeamMember(x => x.MateType == MateType.Pet && x.HasDhaPremium);
                mate.IsDhaLootEnabled = !mate.IsDhaLootEnabled;
                break;
            case (short)SkillsVnums.HEALING when target.HpPercentage == 100:
                session.SendCancelPacket(CancelType.NotInCombatMode);
                return;
            
            case (short)SkillsVnums.HOLY_TOTEM when session.PlayerEntity.EnergyBar < 100:
                session.SendCancelPacket(CancelType.NotInCombatMode);
                return;
            
            case (short)SkillsVnums.ULTIMATE_SONIC_WAVE when session.PlayerEntity.EnergyBar < 1000:
            case (short)SkillsVnums.ULTIMATE_TORNADO_KICK when session.PlayerEntity.EnergyBar < 1000:
            case (short)SkillsVnums.ULTIMATE_UPPERCUT when session.PlayerEntity.EnergyBar < 1000:
                session.SendCancelPacket(CancelType.NotInCombatMode);
                return;
            
            case (short)SkillsVnums.ULTIMATE_TRI_COMBO when !session.PlayerEntity.BuffComponent.HasBuff((short)BuffVnums.ULTIMATE_AURA_III) && session.PlayerEntity.EnergyBar < 1000:
                session.SendCancelPacket(CancelType.NotInCombatMode);
                return;
        
            case (short)SkillsVnums.COVERT_FISHING:
                FishingSpotDto spot = _fishManager.GetFishSpotByMapId(session.PlayerEntity.MapInstance.MapId);
                if (spot == null)
                {
                    session.SendCancelPacket(CancelType.NotInCombatMode);
                    return;
                }
                break;
            
            case (int)SkillsVnums.VOILR:
                character.Session.CurrentMapInstance.Broadcast(character.Session.GenerateGuriPacket(6, 1, character.Id, 22));
                break;
            
            case (int)SkillsVnums.FINISH_COOKING when character.LastRecipeFromChefSp != null:
                character.CancelCastingSkill();
                await character.Session.EmitEventAsync(new FinishCookingMealEvent(character.LastSkillId, character.LastRecipeFromChefSp.ProducedItemVnum, 1, _recipeManager.GetRecipesBySkillnum(character.LastSkillId)));
                break;
        }
        
        foreach (BCardDTO bcard in skill.BCards)
        {
            switch (bcard.Type)
            {
                case (short)BCardType.SummonSkill when bcard.SubType == (byte)AdditionalTypes.SummonSkill.RevertTransformation:
                {
                    BCardDTO bearBuffTransformation = character.BuffComponent.GetAllBuffs()
                        .SelectMany(buff => buff.BCards)
                        .FirstOrDefault(b => b.Type == (short)BCardType.SummonSkill && b.SubType == (byte)AdditionalTypes.SummonSkill.BearTransformation);

                    if (bearBuffTransformation != null)
                    {
                        int cardId = (int)bearBuffTransformation.CardId;
                        character.RemoveBuffAsync(cardId).ConfigureAwait(false).GetAwaiter().GetResult();
                    }
                    break;
                }
                
                case (short)BCardType.MysticArtsTransformed when bcard.SubType == (byte)AdditionalTypes.MysticArtsTransformed.CanOnlyBeUsedWithBuff:
                {
                    int firstData = bcard.FirstDataValue(character.Level);
                    
                    if (!character.BuffComponent.HasBuff(firstData) && 
                        !character.BuffComponent.HasBuff((int)BuffVnums.HIGH_HEATING) && 
                        !character.BuffComponent.HasBuff((int)BuffVnums.ULTRA_HIGH_HEATING))
                    {
                        session.SendCancelPacket(CancelType.NotInCombatMode);
                        session.SendDebugMessage("[U_S] Required buff not present");
                        return;
                    }

                    break;
                }
            }
        }

        
        await session.EmitEventAsync(new SkillEvent
        {
            SkillId = skill.Id,
            Target = target,
            SkillInfo = skillInfo
        });
        
        if (target.IsSameEntity(character) && skillInfo.Vnum == (short)SkillsVnums.SACRIFICE)
        {
            session.SendCancelPacket(CancelType.NotInCombatMode);
            session.SendDebugMessage("[U_S] Sacrifice same entity");
            return;
        }

        if (skillInfo.IsCanceled)
        {
            session.SendCancelPacket(CancelType.NotInCombatMode);
            return;
        }
        
        if (skill.BCards.Any(x => x.Type == (short)BCardType.LordHatus && x.SubType == (byte)AdditionalTypes.LordHatus.CommandSunWolf))
        {
            IMateEntity sunWolf = character.MateComponent.GetMate(x => x.NpcMonsterVNum == (int)MonsterVnum.SUN_WOLF);

            if (sunWolf == null)
            {
                session.SendCancelPacket(CancelType.NotInCombatMode);
                session.SendDebugMessage("[U_S] Wolf is not present");
                return;
            }

            BCardDTO skillBCard = skill.BCards.FirstOrDefault(x => x.Type == (short)BCardType.LordHatus && x.SubType == (byte)AdditionalTypes.LordHatus.CommandSunWolf);

            if (skillBCard is null)
            {
                session.SendCancelPacket(CancelType.NotInCombatMode);
                return;
            }
            
            if (!sunWolf.IsSucceededChance(skillBCard.FirstData))
            {
                return;
            }
            
            Position newPosition = skillBCard.SkillVNum switch
            {
                (short)SkillsVnums.WOLF_CHARGE or (short)SkillsVnums.WOLF_PACK => character.Position,
                (short)SkillsVnums.BACK_KICK or (short)SkillsVnums.SOLAR_ERUPTION or (short)SkillsVnums.WHIRLING_WOLF => new Position((short)(target.Position.X - 1), (short)(target.Position.Y - 1)),
                _ => sunWolf.Position
            };
            
            if (newPosition != sunWolf.Position)
            {
                sunWolf.ChangePosition(newPosition);
                sunWolf.TeleportOnMap(newPosition.X, newPosition.Y);
                sunWolf.MapInstance.Broadcast(sunWolf.GenerateEffectPacket(EffectType.Respawn));
            }

            character.BroadcastEffectGround(EffectType.SunWolf, sunWolf.PositionX, sunWolf.PositionY, true);
            character.Session.SendAtctl(sunWolf, target);
            
            SkillDTO skillDto = _skillsManager.GetSkill(skillBCard.SecondData);
            SkillInfo getSkillInfo = skillDto.GetInfo();
            DateTime getCastTime = sunWolf.GenerateSkillCastTime(getSkillInfo);
            await sunWolf.EmitEventAsync(new BattleExecuteSkillEvent(sunWolf, target, getSkillInfo, getCastTime, target.Position));
        }

        await FinalChecks(session, skill, character, skillInfo, target, comboSkill);

        session.SendDebugMessage($"Hit {skillInfo.HitType} / Target {skillInfo.TargetType} / Attack Type {skillInfo.AttackType} / Affected entities {skillInfo.TargetAffectedEntities}");
        if (target is IPlayerEntity characterTarget)
        {
            session.SendDebugMessage($"Sender: {session.PlayerEntity.Name} -> Target: {characterTarget.Name} ");
        }

        character.SkillComponent.CanBeInterrupted = false;
        character.SkillComponent.IsSkillInterrupted = false;
        character.SkillComponent.CanBeInterrupted = character.CanBeInterrupted(skillInfo);
        if (npcPartnerSkill == null)
        {
            character.WeaponLoaded(characterSkill, _gameLanguage, true);
        }
        character.LastEntity = (target.Type, target.Id);
        DateTime castTime = character.GenerateSkillCastTime(skillInfo);
        session.SendDebugMessage("[U_S] IsCasting = true");
        await character.EmitEventAsync(new BattleExecuteSkillEvent(character, target, skillInfo, castTime, positionAfterDash));
    }

    private async Task<bool> TargetChecks(IClientSession session, IBattleEntity target, SkillInfo skillInfo)
    {
        if (target == null)
        {
            session.SendCancelPacket(CancelType.NotInCombatMode);
            session.SendDebugMessage("[U_S] No target");
            return true;
        }

        if (target.MapInstance.IsPvp && session.CurrentMapInstance.PvpZone(target.PositionX, target.PositionY))
        {
            session.SendDebugMessage("[U_S] Target no-PvP zone / !map.IsPvp");
            session.SendCancelPacket(CancelType.NotInCombatMode);
            return true;
        }

        switch (session.CurrentMapInstance.MapInstanceType)
        {
            case MapInstanceType.RaidInstance:

                if (!target.IsFrozenByGlacerus())
                {
                    break;
                }

                if (target.Position.GetDistance(session.PlayerEntity.Position) > 5)
                {
                    break;
                }

                session.SendCancelPacket(CancelType.NotInCombatMode);

                DateTime utcNow = DateTime.UtcNow;
                if (session.PlayerEntity.LastUnfreezedPlayer > utcNow)
                {
                    return true;
                }

                session.PlayerEntity.LastUnfreezedPlayer = utcNow.AddSeconds(2);
                DateTime wait = await _delayManager.RegisterAction(session.PlayerEntity, DelayedActionType.RainbowBattleUnfreeze);
                session.SendDelay((int)(wait - DateTime.UtcNow).TotalMilliseconds, GuriType.Unfreezing, $"guri 502 {target.Id}");
                return true;
            
            case MapInstanceType.RainbowBattle:

                if (target is not IPlayerEntity rainbowFrozen)
                {
                    break;
                }

                if (session.PlayerEntity.IsEnemyWith(rainbowFrozen))
                {
                    break;
                }

                if (!rainbowFrozen.RainbowBattleComponent.IsFrozen)
                {
                    break;
                }

                if (rainbowFrozen.RainbowBattleComponent.Team != session.PlayerEntity.RainbowBattleComponent.Team)
                {
                    break;
                }

                if (target.Position.GetDistance(session.PlayerEntity.Position) > 5)
                {
                    break;
                }

                session.SendCancelPacket(CancelType.NotInCombatMode);

                DateTime now = DateTime.UtcNow;
                if (session.PlayerEntity.LastUnfreezedPlayer > now)
                {
                    return true;
                }

                session.PlayerEntity.LastUnfreezedPlayer = now.AddSeconds(5);
                wait = await _delayManager.RegisterAction(session.PlayerEntity, DelayedActionType.RainbowBattleUnfreeze);
                session.SendDelay((int)(wait - DateTime.UtcNow).TotalMilliseconds, GuriType.Unfreezing, $"guri 505 {rainbowFrozen.Id}");
                return true;

            case MapInstanceType.Icebreaker:
                if (target is not IPlayerEntity icebreakerFrozen)
                {
                    break;
                }

                if (session.PlayerEntity.IsEnemyWith(icebreakerFrozen))
                {
                    break;
                }

                if (target.Position.GetDistance(session.PlayerEntity.Position) > 5)
                {
                    break;
                }

                session.SendCancelPacket(CancelType.NotInCombatMode);

                DateTime now2 = DateTime.UtcNow;
                if (session.PlayerEntity.LastUnfreezedPlayer > now2)
                {
                    return true;
                }

                session.PlayerEntity.LastUnfreezedPlayer = now2.AddSeconds(5);
                DateTime wait2 = await _delayManager.RegisterAction(session.PlayerEntity, DelayedActionType.IcebreakerUnfreeze);
                session.SendDelay((int)(wait2 - DateTime.UtcNow).TotalMilliseconds, GuriType.Unfreezing, $"guri 505 {icebreakerFrozen.Id}");
                return true;
        }

        if (target is INpcEntity or IMonsterEntity && skillInfo.Vnum == (short)SkillsVnums.SACRIFICE)
        {
            session.SendCancelPacket(CancelType.NotInCombatMode);
            session.SendDebugMessage("[U_S] MapNpc && Sacrifice");
            return true;
        }

        if (!target.IsAlive())
        {
            session.SendCancelPacket(CancelType.NotInCombatMode);
            session.SendDebugMessage("[U_S] Target is dead");
            return true;
        }

        if (target is not IMonsterEntity mob || mob.SummonerId == 0)
        {
            return false;
        }

        if (mob.SummonerType is not VisualType.Player)
        {
            return false;
        }

        if (session.PlayerEntity.CanMonsterBeAttacked(mob) && !mob.IsMateTrainer && !mob.IsSparringMonster)
        {
            return false;
        }

        session.SendDebugMessage("[U_S] mob.SummonerId != 0");
        session.SendCancelPacket(CancelType.NotInCombatMode);
        return true;
    }

    private async Task FinalChecks(IClientSession session, SkillDTO skill, IPlayerEntity character, SkillInfo skillInfo, IBattleEntity target, bool comboSkill)
    {
        bool removeMana = !(skill.Id == (short)SkillsVnums.BUDDHAS_WORDS && session.PlayerEntity.SkillComponent.BuddhaWordsActivated);

        if (!session.PlayerEntity.CheatComponent.HasGodMode && removeMana)
        {
            skillInfo.ManaCost = session.PlayerEntity.BCardComponent.HasBCard(BCardType.TimeCircleSkills, (byte)AdditionalTypes.TimeCircleSkills.DisableMPConsumption) ? 0 : skillInfo.ManaCost;
            
            if (session.PlayerEntity.BCardComponent.HasBCard(BCardType.ConditionalEffects, (byte)AdditionalTypes.ConditionalEffects.IncreaseSkillMpConsumptionIfBuffActive))
            {
                (int firstData, int secondData) increaseData = session.PlayerEntity.BCardComponent.GetAllBCardsInformation(BCardType.ConditionalEffects, (byte)AdditionalTypes.ConditionalEffects.IncreaseSkillMpConsumptionIfBuffActive, session.PlayerEntity.Level);
                
                if (session.PlayerEntity.BuffComponent.HasBuff(increaseData.secondData))
                {
                    skillInfo.ManaCost += (int)(skillInfo.ManaCost * increaseData.firstData / 100.0);
                }
            }

            if (session.PlayerEntity.BCardComponent.HasBCard(BCardType.ConditionalEffects, (byte)AdditionalTypes.ConditionalEffects.ReduceSkillMpConsumptionIfBuffActive))
            {
                (int firstData, int secondData) reduceData = session.PlayerEntity.BCardComponent.GetAllBCardsInformation(BCardType.ConditionalEffects, (byte)AdditionalTypes.ConditionalEffects.ReduceSkillMpConsumptionIfBuffActive, session.PlayerEntity.Level);
                if (session.PlayerEntity.BuffComponent.HasBuff(reduceData.secondData))
                {
                    skillInfo.ManaCost -= (int)(skillInfo.ManaCost * reduceData.firstData / 100.0);
                }
            }
            
            if (session.PlayerEntity.AdditionalMp > 0)
            {
                int removedAdditionalMp;
                if (session.PlayerEntity.AdditionalMp > skillInfo.ManaCost)
                {
                    removedAdditionalMp = skillInfo.ManaCost;
                }
                else
                {
                    removedAdditionalMp = session.PlayerEntity.AdditionalMp;

                    int overflow = Math.Abs(session.PlayerEntity.AdditionalMp - skillInfo.ManaCost);
                    session.PlayerEntity.Mp -= overflow;
                }

                await session.EmitEventAsync(new RemoveAdditionalHpMpEvent
                {
                    Mp = removedAdditionalMp
                });
            }
            else
            {
                if (session.PlayerEntity.SubClass == SubClassType.ArcaneSage && !session.CurrentMapInstance.IsPvp)
                {
                    double manaReductionRate = session.PlayerEntity.TierLevel switch
                    {
                        1 => 0.15, // Reduce by 15% for Tier Level 1
                        2 => 0.16, // Reduce by 16% for Tier Level 2
                        3 => 0.17, // Reduce by 17% for Tier Level 3
                        4 => 0.18, // Reduce by 18% for Tier Level 4
                        5 => 0.19, // Reduce by 19% for Tier Level 5
                        _ => 0  // Default to 0% if no tier level specified
                    };
                    
                    skillInfo.ManaCost -= (int)(skillInfo.ManaCost * manaReductionRate);
                }
                
                session.PlayerEntity.RemoveEntityMp((short)skillInfo.ManaCost, skill);
            }

            (int firstDataNegative, int _) = session.PlayerEntity.BCardComponent.GetAllBCardsInformation(BCardType.HealingBurningAndCasting,
                (byte)AdditionalTypes.HealingBurningAndCasting.HPDecreasedByConsumingMP, session.PlayerEntity.Level);

            (int firstDataPositive, int _) = session.PlayerEntity.BCardComponent.GetAllBCardsInformation(BCardType.HealingBurningAndCasting,
                (byte)AdditionalTypes.HealingBurningAndCasting.HPIncreasedByConsumingMP, session.PlayerEntity.Level);

            int hpRemoved = (int)(firstDataPositive / 100.0 * skillInfo.ManaCost - firstDataNegative / 100.0 * skillInfo.ManaCost);

            if (hpRemoved > 0)
            {
                await session.PlayerEntity.EmitEventAsync(new BattleEntityHealEvent
                {
                    Entity = session.PlayerEntity,
                    HpHeal = hpRemoved
                });
            }
            else
            {
                if (session.PlayerEntity.Hp - -hpRemoved <= 0)
                {
                    session.PlayerEntity.BroadcastDamage(session.PlayerEntity.Hp - 1);
                    session.PlayerEntity.Hp = 1;
                }
                else
                {
                    session.PlayerEntity.BroadcastDamage(-hpRemoved);
                    session.PlayerEntity.Hp -= -hpRemoved;
                }
            }

            session.RefreshStat();
            session.SendDebugMessage($"[U_S] MpCost: {skillInfo.ManaCost}");
        }

        switch (skill.Id)
        {
            case (short)SkillsVnums.PLAY_DEAD:
                session.PlayerEntity.SkillComponent.PyjamaFakeDeadActivated = true;
                break;

            case (short)SkillsVnums.SPY_OUT:
                session.SendEffectObject(target, true, EffectType.Sp6ArcherTargetFalcon);
                break;

            case (short)SkillsVnums.VOLCANIC_ROAR:
            case (short)SkillsVnums.CATASTROPHIC_EARTHQUEAKE:
            case (short)SkillsVnums.HIDDEN_ART:
            case (short)SkillsVnums.PSYCHIC_CIRCLE:
            case (short)SkillsVnums.SHADOWLESS_LEGS:
            case (short)SkillsVnums.FLYING_KICK:
            case (short)SkillsVnums.TRI_COMBO:
            case (short)SkillsVnums.NOSEDIVE:
            case (short)SkillsVnums.LOTUS_LEAP:
                Position position = target.Position;
                session.PlayerEntity.ChangePosition(position);
                session.PlayerEntity.TeleportOnMap(session.PlayerEntity.PositionX, session.PlayerEntity.PositionY);
                break;
            
            case (short)SkillsVnums.ULTIMATE_TRI_COMBO:
                Position ultimatePosition = target.Position;
                session.PlayerEntity.ChangePosition(ultimatePosition);
                session.PlayerEntity.TeleportOnMap(session.PlayerEntity.PositionX, session.PlayerEntity.PositionY);
                session.PlayerEntity.UpdateEnergyBar(-1000).ConfigureAwait(false).GetAwaiter().GetResult();
                break;
                

            case (short)SkillsVnums.BUDDHAS_WORDS:
                session.PlayerEntity.SkillComponent.BuddhaWordsActivated = true;
                session.PlayerEntity.SkillComponent.LastBuddhaTick = DateTime.UtcNow.AddSeconds(5);
                break;
            
            case (short)SkillsVnums.ULTIMATE_SONIC_WAVE:
            case (short)SkillsVnums.ULTIMATE_TORNADO_KICK:
            case (short)SkillsVnums.ULTIMATE_UPPERCUT:
                session.PlayerEntity.UpdateEnergyBar(-1000).ConfigureAwait(false).GetAwaiter().GetResult();
                
                if (session.PlayerEntity.BuffComponent.HasBuff((short)BuffVnums.ULTIMATE_AURA_II))
                {
                    session.PlayerEntity.RemoveBuffAsync((short)BuffVnums.ULTIMATE_AURA_II).ConfigureAwait(false).GetAwaiter().GetResult();
                    Buff buff = _buffFactory.CreateBuff((short)BuffVnums.ULTIMATE_AURA_III, session.PlayerEntity);
                    session.PlayerEntity.AddBuffAsync(buff).ConfigureAwait(false).GetAwaiter().GetResult();
                }

                if (session.PlayerEntity.BuffComponent.HasBuff((short)BuffVnums.ULTIMATE_AURA_I))
                {
                    session.PlayerEntity.RemoveBuffAsync((short)BuffVnums.ULTIMATE_AURA_I).ConfigureAwait(false).GetAwaiter().GetResult();
                    Buff buff = _buffFactory.CreateBuff((short)BuffVnums.ULTIMATE_AURA_II, session.PlayerEntity);
                    session.PlayerEntity.AddBuffAsync(buff).ConfigureAwait(false).GetAwaiter().GetResult();
                }


                if (!session.PlayerEntity.BuffComponent.HasBuff((short)BuffVnums.ULTIMATE_AURA_I) &&
                    !session.PlayerEntity.BuffComponent.HasBuff((short)BuffVnums.ULTIMATE_AURA_II) &&
                    !session.PlayerEntity.BuffComponent.HasBuff((short)BuffVnums.ULTIMATE_AURA_III))
                {
                    Buff buff = _buffFactory.CreateBuff((short)BuffVnums.ULTIMATE_AURA_I, session.PlayerEntity);
                    session.PlayerEntity.AddBuffAsync(buff).ConfigureAwait(false).GetAwaiter().GetResult();
                }
                break;
            
             case (short)SkillsVnums.MOONLIGHT_ABSORPTION when session.PlayerEntity.BuffComponent.HasBuff((short)BuffVnums.ENLIGHTENMENT):
                {
                    session.PlayerEntity.RemoveBuffAsync((short)BuffVnums.ENLIGHTENMENT).ConfigureAwait(false).GetAwaiter().GetResult();
                    Buff buff = _buffFactory.CreateBuff((short)BuffVnums.BATHED_IN_MOONLIGHT, session.PlayerEntity);
                    session.PlayerEntity.AddBuffAsync(buff).ConfigureAwait(false).GetAwaiter().GetResult();
                }
                break;

            case (short)SkillsVnums.GATHERING_PETALS when session.PlayerEntity.BuffComponent.HasBuff((short)BuffVnums.ENLIGHTENMENT):
                {
                    session.PlayerEntity.RemoveBuffAsync((short)BuffVnums.ENLIGHTENMENT).ConfigureAwait(false).GetAwaiter().GetResult();
                    Buff buff = _buffFactory.CreateBuff((short)BuffVnums.BED_OF_LOTUS_FLOWERS, session.PlayerEntity);
                    session.PlayerEntity.AddBuffAsync(buff).ConfigureAwait(false).GetAwaiter().GetResult();
                }
                break;

            case (short)SkillsVnums.CRESCENT_MOON_DANCE when session.PlayerEntity.BuffComponent.HasBuff((short)BuffVnums.OPPORTUNITY_TO_ATTACK):
                {
                    session.PlayerEntity.RemoveBuffAsync((short)BuffVnums.OPPORTUNITY_TO_ATTACK).ConfigureAwait(false).GetAwaiter().GetResult();
                    Buff buff = _buffFactory.CreateBuff((short)BuffVnums.CRESCENT_MOONSHADE, session.PlayerEntity);
                    session.PlayerEntity.AddBuffAsync(buff).ConfigureAwait(false).GetAwaiter().GetResult();
                }
                break;

            case (short)SkillsVnums.LUNAR_SLICE when session.PlayerEntity.BuffComponent.HasBuff((short)BuffVnums.OPPORTUNITY_TO_ATTACK):
                {
                    session.PlayerEntity.RemoveBuffAsync((short)BuffVnums.OPPORTUNITY_TO_ATTACK).ConfigureAwait(false).GetAwaiter().GetResult();

                    int mpToIncrease = (int)(target.Mp * (20 * 0.01));
                    if (session.PlayerEntity.Mp + mpToIncrease < session.PlayerEntity.MaxMp)
                    {
                        session.PlayerEntity.Mp += mpToIncrease;
                    }
                    else
                    {
                        session.PlayerEntity.Mp = session.PlayerEntity.MaxMp;
                    }
                }
                break;

            case (short)SkillsVnums.BOUND_BY_MOONLIGHT when session.PlayerEntity.BuffComponent.HasBuff((short)BuffVnums.OPPORTUNITY_TO_ATTACK):
                {
                    session.PlayerEntity.RemoveBuffAsync((short)BuffVnums.OPPORTUNITY_TO_ATTACK).ConfigureAwait(false).GetAwaiter().GetResult();
                    session.PlayerEntity.RemoveNegativeBuffs(4).ConfigureAwait(false).GetAwaiter().GetResult();
                }
                break;
        }

        
        if (skill.Id >= (short)SkillsVnums.RYTHM_OF_LOVE && skill.Id <= (short)SkillsVnums.LOVING_POSE)
        {
            session.CurrentMapInstance.Broadcast(session.GenerateGuriPacket(6, 1, session.PlayerEntity.Id, skill.SuAnimation));
        }

        if (skill.ItemVNum != 0)
        {
            await session.RemoveItemFromInventory(skill.ItemVNum);
        }

        if (session.PlayerEntity.RainbowBattleComponent.IsInRainbowBattle)
        {
            session.PlayerEntity.RainbowBattleComponent.ActivityPoints += _rainbowBattleConfiguration.UsingSkillActivityPoints;
        }

        character.LastSkillUse = DateTime.UtcNow;
        character.ClearFoodBuffer();
        character.LastSkillId = skill.Id;
        ComboSkillState newState = null;
        if (!comboSkill && skill.CastId != 0)
        {
            newState = new ComboSkillState
            {
                State = 0
            };
        }

        if (skill.CastId < 11 && skill.CastId != 0 && newState != null)
        {
            newState.OriginalSkillCastId = (byte)skill.CastId;
        }

        session.RefreshStat();
        if (newState != null && !session.PlayerEntity.AngelElement.HasValue)
        {
            session.PlayerEntity.SaveComboSkill(newState);
        }

        if (skill.CastId == 0)
        {
            return;
        }

        ComboSkillState state = session.PlayerEntity.GetComboState();
        bool sendBuffIconWindow = session.PlayerEntity.AngelElement.HasValue && character.Specialist is { SpLevel: > 19 } && character.HasBuff(BuffVnums.MAGIC_SPELL);

        if (state != null)
        {
            if (state.State >= 10)
            {
                session.PlayerEntity.CleanComboState();
            }
            else
            {
                if (comboSkill)
                {
                    session.PlayerEntity.IncreaseComboState((byte)skill.CastId);
                }
                else if (newState == null)
                {
                    session.PlayerEntity.CleanComboState();
                }
            }

            if (sendBuffIconWindow)
            {
                character.Session.SendMSlotPacket(state.AngelSkillVnumId);
            }
        }

        if (sendBuffIconWindow)
        {
            session.RefreshQuicklist();
            return;
        }

        session.SendMsCPacket(0);
        session.RefreshQuicklist();
    }

    private bool BasicChecks(IClientSession session, UseSkillPacket packet)
    {
        IPlayerEntity character = session.PlayerEntity;

        if (!character.CanFight() || packet == null)
        {
            session.SendCancelPacket(CancelType.NotInCombatMode);
            session.SendDebugMessage("[U_S] !canFight, packet null");
            return false;
        }

        if (!session.PlayerEntity.CanPerformAttack())
        {
            session.SendDebugMessage("[U_S] !CanPerformAttack");
            session.SendCancelPacket(CancelType.NotInCombatMode);
            return false;
        }

        if (session.CurrentMapInstance.IsPvp && session.CurrentMapInstance.PvpZone(session.PlayerEntity.PositionX, session.PlayerEntity.PositionY))
        {
            session.SendCancelPacket(CancelType.NotInCombatMode);
            session.SendDebugMessage("[U_S] Character no-PvP zone / !map.IsPvp");
            return false;
        }

        if (session.IsMuted())
        {
            session.SendMuteMessage();
            session.SendCancelPacket(CancelType.NotInCombatMode);
            return false;
        }

        if (session.PlayerEntity.IsOnVehicle || session.PlayerEntity.CheatComponent.IsInvisible)
        {
            session.SendDebugMessage("[U_S] IsVehicled, InvisibleGm");
            session.SendCancelPacket(CancelType.NotInCombatMode);
            return false;
        }

        if ((DateTime.UtcNow - session.PlayerEntity.LastTransform).TotalSeconds < 3)
        {
            session.SendCancelPacket(CancelType.NotInCombatMode);
            session.SendDebugMessage("[U_S] Under transformation cooldown");
            session.SendMsg(_gameLanguage.GetLanguage(GameDialogKey.SPECIALIST_SHOUTMESSAGE_CANT_ATTACK_YET, session.UserLanguage), MsgMessageType.Middle);
            return false;
        }

        if (character.BlockAllAttack)
        {
            session.SendCancelPacket(CancelType.NotInCombatMode);
            session.SendDebugMessage("[U_S] Not allowed to do that while buff active");
            session.SendMsg(_gameLanguage.GetLanguage(GameDialogKey.SPECIALIST_SHOUTMESSAGE_CANT_ATTACK_YET, session.UserLanguage), MsgMessageType.Middle);
            return false;
        }

        if (!character.IsCastingSkill)
        {
            return true;
        }

        session.SendCancelPacket(CancelType.NotInCombatMode);
        session.SendDebugMessage("[U_S] Already using a skill");
        return false;
    }
}