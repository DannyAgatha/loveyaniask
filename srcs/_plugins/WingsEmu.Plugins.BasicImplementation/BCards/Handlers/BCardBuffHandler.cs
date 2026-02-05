// NosEmu
// 


using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using WingsEmu.DTOs.Bonus;
using WingsEmu.DTOs.Maps;
using WingsEmu.DTOs.Skills;
using WingsEmu.Game;
using WingsEmu.Game._enum;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Entities.Extensions;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Managers.StaticData;
using WingsEmu.Game.Mates;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Skills;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Battle;
using WingsEmu.Packets.Enums.Chat;

namespace NosEmu.Plugins.BasicImplementations.BCards.Handlers;

public class BCardBuffHandler : IBCardEffectAsyncHandler
{
    private readonly IBuffFactory _buffFactory;
    private readonly ICardsManager _cards;
    private readonly IGameLanguageService _gameLanguage;
    private readonly IRandomGenerator _randomGenerator;
    
    private readonly List<int> _potionExtended = [
        (short)BuffVnums.ATTACK_POWER_INCREASED, 
        (short)BuffVnums.ARMOUR_ENHANCEMENT_INCREASED, 
        (short)BuffVnums.ENERGY_ENHANCEMENT, 
        (short)BuffVnums.EXPERIENCE_ENHANCEMENT
    ];

    public BCardBuffHandler(IRandomGenerator randomGenerator, IBuffFactory buffFactory, IGameLanguageService gameLanguage, ICardsManager cards)
    {
        _randomGenerator = randomGenerator;
        _buffFactory = buffFactory;
        _gameLanguage = gameLanguage;
        _cards = cards;
    }

    public BCardType HandledType => BCardType.Buff;

    public void Execute(IBCardEffectContext ctx)
    {
        IBattleEntity sender = ctx.Sender;
        IBattleEntity target = ctx.Target;
        int firstData = ctx.BCard.FirstDataValue(sender.Level);
        int secondData =  ctx.BCard.SecondDataValue(sender.Level);
        SkillInfo skillInfo = ctx.Skill;
        
        switch (ctx.BCard.SubType)
        {
            case (byte)AdditionalTypes.Buff.ChanceCausing:
                
                if (sender == null)
                {
                    return;
                }
                
                bool dontAdd = false;
                
                if (ctx.Skill?.CastId is >= 40 and <= 44)
                {
                    if (sender is not IPlayerEntity entity)
                    {
                        return;
                    }

                    if (!entity.CharacterSkills.TryGetValue(ctx.Skill.Vnum, out CharacterSkill skill))
                    {
                        return;
                    }

                    secondData += skill.UpgradeSkill;
                }
                
                if (ctx.Skill is { SkillType: SkillType.PartnerSkill })
                {
                    if (sender is IMateEntity { IsUsingSp: true } partner)
                    {
                        var skill = (PartnerSkill)partner.Skills.FirstOrDefault(x => x.Skill.Id == ctx.Skill.Vnum);
                        if (skill is { Rank: > 0 })
                        {
                            secondData += skill.Rank-1;
                        }
                    }
                }

                Buff b = _buffFactory.CreateBuff(secondData, sender);

                if (b == null)
                {
                    return;
                }

                if (sender.IsPlayer())
                {
                    IClientSession session = ((IPlayerEntity)sender).Session;

                    if (_potionExtended.Contains(secondData))
                    {
                        if (session.PlayerEntity.BuffComponent.HasBuff((short)secondData))
                        {
                            b = _buffFactory.CreateBuff(secondData, sender, true);
                            Buff existingBuff = session.PlayerEntity.BuffComponent.GetBuff(secondData);

                            double minutes = (existingBuff.Start - DateTime.UtcNow + existingBuff.Duration).TotalMinutes;
                            var newer = TimeSpan.FromMinutes(Math.Min(60, b.Duration.TotalMinutes + minutes));
                            
                            if (minutes >= 60)
                            {
                                string buffNameReached = _gameLanguage.GetLanguage(GameDataType.Card, existingBuff.Name, session.UserLanguage);
                                string message = _gameLanguage.GetLanguageFormat(GameDialogKey.ITEM_CHATMESSAGE_BUFF_LIMIT_REACHED, session.UserLanguage, buffNameReached);
                                session.SendChatMessage(message, ChatMessageColorType.Red);
                            }

                            session.PlayerEntity.BuffComponent.GetBuff(secondData).SetBuffDuration(newer);
                            session.SendBuffsPacket();
                            
                            string buffName = _gameLanguage.GetLanguage(GameDataType.Card, existingBuff.Name, session.UserLanguage);
                            session.SendPacket(session.PlayerEntity.GenerateSayPacket(
                                _gameLanguage.GetLanguageFormat(GameDialogKey.ITEM_CHATMESSAGE_BUFF_EXTENDED, session.UserLanguage, buffName),
                                ChatMessageColorType.Yellow));

                            IReadOnlyList<IMateEntity> mates = session.PlayerEntity.MateComponent?.GetMates();
                            if (mates == null)
                            {
                                return;
                            }

                            foreach (IMateEntity mate1 in mates)
                            {
                                if (!mate1.BuffComponent.HasBuff(secondData))
                                {
                                    continue;
                                }

                                mate1.BuffComponent.GetBuff(secondData).SetBuffDuration(newer);
                            }

                            return;
                        }
                        
                        b = _buffFactory.CreateBuff(secondData, sender, true);

                        session.PlayerEntity.AddBuffAsync(b).ConfigureAwait(false).GetAwaiter().GetResult();
                        foreach (IMateEntity mate1 in session.PlayerEntity.MateComponent.GetMates())
                        {
                            mate1.AddBuffAsync(b).ConfigureAwait(false).GetAwaiter().GetResult();
                        }

                        return;
                    }

                    b = _buffFactory.CreateBuff(secondData, sender);
                }

                if (target.IsPlayer())
                {
                    IClientSession session = ((IPlayerEntity)target).Session;
                    
                    if (session.PlayerEntity.HaveStaticBonus(StaticBonusType.OrderOfDiscipline))
                    {
                        if (b.BuffGroup == BuffGroup.Bad && b.Level <= 4)
                        {
                            int randomChance = _randomGenerator.RandomNumber();

                            if (randomChance < 5)
                            {
                                return;
                            }
                        }
                    }
                }


                double debuffCounter = target.CheckForResistance(b, _cards, out double buffCounter, out double specializedResistance);

                int randomNumber = _randomGenerator.RandomNumber();
                int debuffRandomNumber = _randomGenerator.RandomNumber();
                int buffRandomNumber = _randomGenerator.RandomNumber();
                int specializedRandomNumber = _randomGenerator.RandomNumber();
                if (b.CardId == (short)BuffVnums.MEMORIAL && sender.BuffComponent.HasBuff((short)BuffVnums.MEMORIAL))
                {
                    return;
                }
                
                if (target.BCardComponent.HasBCard(BCardType.HideBarrelSkill, (byte)AdditionalTypes.HideBarrelSkill.ReduceChanceGettingDebuffPerStack))
                {
                    (int reduceChanceFirstData, int reduceChanceSecondData) = target.BCardComponent.GetAllBCardsInformation(BCardType.HideBarrelSkill, 
                        (byte)AdditionalTypes.HideBarrelSkill.ReduceChanceGettingDebuffPerStack, target.Level);
                    int debuffCount = target.BuffComponent.GetAllBuffs(x => x.BuffGroup == BuffGroup.Bad && x.Level <= reduceChanceSecondData).Count;
                    int badProbabilityPrevent = reduceChanceFirstData * debuffCount;

                    if (badProbabilityPrevent > 30)
                    {
                        badProbabilityPrevent = 30;
                    }
                    
                    if (b.BuffGroup is BuffGroup.Bad)
                    {
                        if (randomNumber > firstData + badProbabilityPrevent)
                        {
                            return;
                        }
                    }
                }

                
                if (target.BCardComponent.HasBCard(BCardType.HideBarrelSkill, (byte)AdditionalTypes.HideBarrelSkill.IncreaseChanceGettingDebuffPerStack))
                {
                    (int reduceChanceFirstData, int reduceChanceSecondData) = target.BCardComponent.GetAllBCardsInformation(BCardType.HideBarrelSkill, 
                        (byte)AdditionalTypes.HideBarrelSkill.IncreaseChanceGettingDebuffPerStack, target.Level);
                    int debuffCount = target.BuffComponent.GetAllBuffs(x => x.BuffGroup == BuffGroup.Bad && x.Level <= reduceChanceSecondData).Count;
                    int badProbabilityPrevent = reduceChanceFirstData * debuffCount;

                    if (badProbabilityPrevent > 30)
                    {
                        badProbabilityPrevent = 30;
                    }
                    
                    if (b.BuffGroup is BuffGroup.Bad)
                    {
                        if (randomNumber + badProbabilityPrevent > firstData)
                        {
                            return;
                        }
                    }
                }
                
                int percent = firstData;
                if (b.CardId == (short)BuffVnums.LOTUS_CURSE && sender.BuffComponent.HasBuff((int)BuffVnums.OPPORTUNITY_TO_ATTACK))
                {
                    sender.RemoveBuffAsync((int)BuffVnums.OPPORTUNITY_TO_ATTACK).ConfigureAwait(false).GetAwaiter().GetResult();
                    percent += 50;
                }
                
                if (randomNumber > percent)
                {
                    return;
                }

                if (specializedRandomNumber >= (int)(specializedResistance * 100))
                {
                    if (target is not IPlayerEntity c)
                    {
                        return;
                    }

                    string message = _gameLanguage.GetLanguage(GameDialogKey.BUFF_CHATMESSAGE_EFFECT_RESISTANCE, c.Session.UserLanguage);
                    c.Session.SendChatMessage(message, ChatMessageColorType.Buff);
                    return;
                }

                if (ctx.Skill?.Vnum is (short)SkillsVnums.FIRE_MINE or (short)SkillsVnums.BOMB)
                {
                    if (sender.IsSameEntity(target))
                    {
                        return;
                    }
                }

                switch (b.BuffGroup)
                {
                    case BuffGroup.Bad when debuffRandomNumber >= (int)(debuffCounter * 100):
                        {
                            if (target is not IPlayerEntity c)
                            {
                                return;
                            }

                            string message = _gameLanguage.GetLanguage(GameDialogKey.BUFF_CHATMESSAGE_EFFECT_RESISTANCE, c.Session.UserLanguage);
                            c.Session.SendChatMessage(message, ChatMessageColorType.Buff);
                            return;
                        }
                    case BuffGroup.Good when buffRandomNumber >= (int)(buffCounter * 100):
                        {
                            if (target is not IPlayerEntity c)
                            {
                                return;
                            }

                            string message = _gameLanguage.GetLanguage(GameDialogKey.BUFF_CHATMESSAGE_EFFECT_RESISTANCE, c.Session.UserLanguage);
                            c.Session.SendChatMessage(message, ChatMessageColorType.Buff);
                            return;
                        }
                    case BuffGroup.Bad when target is IMonsterEntity monsterEntity:
                        monsterEntity.MapInstance.AddEntityToTargets(monsterEntity, sender);
                        break;
                }

                switch (sender)
                {
                    case IMonsterEntity monster when monster.SummonerId != null && monster.SummonerId == target.Id && monster.SummonerType != null && monster.SummonerType == target.Type:
                        return;
                    case IMateEntity { IsUsingSp: true } mateEntity:
                        {
                            IBattleEntitySkill skill = mateEntity.LastUsedPartnerSkill;
                            if (skill is not PartnerSkill partnerSkill)
                            {
                                return;
                            }

                            int buffVnum = secondData;

                            Buff partnerBuff = _buffFactory.CreateBuff(buffVnum, sender);
                            target.AddBuffAsync(partnerBuff).ConfigureAwait(false).GetAwaiter().GetResult();

                            if (partnerSkill.Skill.Id == (short)SkillsVnums.IMP_HAT)
                            {
                                mateEntity.Owner.AddBuffAsync(partnerBuff).ConfigureAwait(false).GetAwaiter().GetResult();
                            }
                            return;
                        }
                }

                if (target is IMateEntity { IsUsingSp: true } mate)
                {
                    IBattleEntitySkill skill = mate.LastUsedPartnerSkill;

                    int buffVnum = secondData;
                    if (skill != null && skill.Skill.TargetType == TargetType.Self && sender.Id == target.Id && skill is PartnerSkill partnerSkill)
                    {
                        Buff partnerBuff = _buffFactory.CreateBuff(buffVnum + (buffVnum.IsPartnerRankBuff() ? partnerSkill.Rank - 1 : 0), sender);
                        target.AddBuffAsync(partnerBuff).ConfigureAwait(false).GetAwaiter().GetResult();
                        return;
                    }
                }

                switch (b.CardId)
                {
                    case (int)BuffVnums.SONG_OF_THE_SIRENS when target is IPlayerEntity:
                    {
                        Buff sirensBuff = _buffFactory.CreateBuff((int)BuffVnums.SONG_OF_THE_SIRENS_PVP, sender);
                        target.AddBuffAsync(sirensBuff).ConfigureAwait(false).GetAwaiter().GetResult();
                        return;
                    }
                    case (int)BuffVnums.MARK_OF_THE_MOON when sender.BuffComponent.HasBuff((int)BuffVnums.OPPORTUNITY_TO_ATTACK):
                    {
                        sender.RemoveBuffAsync((int)BuffVnums.OPPORTUNITY_TO_ATTACK).ConfigureAwait(false).GetAwaiter().GetResult();
                        Buff buffToAdd = _buffFactory.CreateBuff((int)BuffVnums.MARK_OF_THE_FULL_MOON, sender);
                        target.AddBuffAsync(buffToAdd).ConfigureAwait(false).GetAwaiter().GetResult();
                        return;
                    }
                    case (int)BuffVnums.WEDDING_BUFF when target is IPlayerEntity playerEntity && playerEntity.Morph != (int)MorphType.Wedding:
                        return;
                    case (int)BuffVnums.STRONG_TIME_VOID:
                    {
                        Buff wheelBuff = _buffFactory.CreateBuff((int)BuffVnums.STRONG_WHEEL_OF_FORTUNE, sender);
                        sender.AddBuffAsync(wheelBuff).ConfigureAwait(false).GetAwaiter().GetResult();
                        break;
                    }
                }

                switch (sender)
                {
                    case IPlayerEntity { Morph: (int)MorphType.FlameDruidLeopardStance } player:
                    {
                        player.Morph = (int)MorphType.FlameDruid;
                        player.IsFlameDruidTransformed = false;
                        player.Session.BroadcastCMode();
                        player.Session.BroadcastEffect(EffectType.Transform);
                        player.Session.SendCancelPacket(CancelType.InCombatMode);

                        if (player.HasBuff(BuffVnums.RED_LEOPARD_ENERGY))
                        {
                            player.RemoveBuffAsync((int)BuffVnums.RED_LEOPARD_ENERGY).ConfigureAwait(false).GetAwaiter().GetResult();
                        }

                        return;
                    }
                    case IPlayerEntity { Morph: (int)MorphType.FlameDruidBearStance } player:
                    {
                        if (skillInfo.Vnum == (int)SkillsVnums.FLAME_LEOPARD)
                        {
                            player.Morph = (int)MorphType.FlameDruidLeopardStance;
                            player.IsFlameDruidTransformed = true;
                        }
                        else
                        {
                            player.Morph = (int)MorphType.FlameDruid;
                            player.IsFlameDruidTransformed = false;
                        }

                        player.Session.BroadcastCMode();
                        player.Session.BroadcastEffect(EffectType.Transform);
                        player.Session.SendCancelPacket(CancelType.InCombatMode);

                        if (player.HasBuff(BuffVnums.BROWN_BEAR_ENERGY))
                        {
                            player.RemoveBuffAsync((int)BuffVnums.BROWN_BEAR_ENERGY).ConfigureAwait(false).GetAwaiter().GetResult();
                        }

                        break;
                    }
                }

                Buff buff = _buffFactory.CreateBuff(secondData, sender);
                int firstRandomNumber = _randomGenerator.RandomNumber();
                int secondRandomNumber = _randomGenerator.RandomNumber();
                if (target.BCardComponent.HasBCard(BCardType.TauntSkill, (byte)AdditionalTypes.TauntSkill.ReflectBadEffect) && firstRandomNumber <= secondRandomNumber &&
                    buff.BuffGroup == BuffGroup.Bad)
                {
                    if (sender is not IMonsterEntity { IsBoss: true })
                    {
                        sender.AddBuffAsync(buff).ConfigureAwait(false).GetAwaiter().GetResult();
                    }
                    return;
                }

                if (!dontAdd)
                {
                    if (target != null)
                    {
                        target.AddBuffAsync(buff).ConfigureAwait(false).GetAwaiter().GetResult();

                        if (target is IPlayerEntity player)
                        {
                            IReadOnlyList<IMateEntity> teamMembers = player.MateComponent?.TeamMembers();
                            if (teamMembers != null)
                            {
                                foreach (IMateEntity targetMate in teamMembers)
                                {
                                    if (targetMate != null && skillInfo != null && targetMate.IsInRange(target.Position.X, target.Position.Y, (byte)skillInfo.AoERange))
                                    {
                                        targetMate.AddBuffAsync(buff).ConfigureAwait(false).GetAwaiter().GetResult();
                                    }
                                }
                            }
                        }
                    }
                }

                break;
            case (byte)AdditionalTypes.Buff.ChanceRemoving:
                if (!target.BuffComponent.HasBuff(secondData))
                {
                    return;
                }

                if (_randomGenerator.RandomNumber() > firstData)
                {
                    return;
                }

                Buff chanceRemoving = target.BuffComponent.GetBuff(secondData);
                target.RemoveBuffAsync(false, chanceRemoving).ConfigureAwait(false).GetAwaiter().GetResult();
                break;
            case (byte)AdditionalTypes.Buff.CancelGroupOfEffects:

                int firstDataValue = ctx.BCard.FirstDataValue(target.Level);
                int secondDataValue = ctx.BCard.SecondDataValue(target.Level);

                target.RemoveBuffAsync(false,
                    target.BuffComponent.GetAllBuffs().Where(x => x.GroupId == firstDataValue && x.Level <= secondDataValue).ToArray()).ConfigureAwait(false).GetAwaiter().GetResult();

                break;
            case (byte)AdditionalTypes.Buff.CounteractPoison:

                firstDataValue = ctx.BCard.FirstDataValue(target.Level);
                secondDataValue = ctx.BCard.SecondDataValue(target.Level);

                if (!Enum.TryParse(firstDataValue.ToString(), out BuffCategory buffCategory))
                {
                    return;
                }

                target.RemoveBuffAsync(false,
                    target.BuffComponent.GetAllBuffs().Where(x => x.BuffCategory == buffCategory && x.Level <= secondDataValue).ToArray()).ConfigureAwait(false).GetAwaiter().GetResult();

                break;
        }
    }
}