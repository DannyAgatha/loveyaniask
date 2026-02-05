using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsAPI.Game.Extensions.Quicklist;
using WingsEmu.Core.Extensions;
using WingsEmu.DTOs.BCards;
using WingsEmu.Game._enum;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Buffs.Events;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Extensions.Mates;
using WingsEmu.Game.Mates;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Skills;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;
using WingsEmu.Packets.ServerPackets;

namespace NosEmu.Plugins.BasicImplementations.Event.Buffs;

public class BuffAddEventHandler : IAsyncEventProcessor<BuffAddEvent>
{
    private readonly IBCardEffectHandlerContainer _bCardEffectHandlerContainer;
    private readonly IBuffFactory _buffFactory;
    private readonly IGameLanguageService _gameLanguage;

    public BuffAddEventHandler(IBuffFactory buffFactory, IGameLanguageService gameLanguage, IBCardEffectHandlerContainer bCardEffectHandlerContainer)
    {
        _buffFactory = buffFactory;
        _gameLanguage = gameLanguage;
        _bCardEffectHandlerContainer = bCardEffectHandlerContainer;
    }

    public async Task HandleAsync(BuffAddEvent e, CancellationToken cancellation)
    {
        IBattleEntity battleEntity = e.Entity;
        foreach (Buff buff in e.Buffs)
        {
            if (buff == null)
            {
                continue;
            }

            if (buff.ElementType != ElementType.Neutral && (ElementType)battleEntity.Element != buff.ElementType)
            {
                continue;
            }

            switch (battleEntity)
            {
                case IMonsterEntity { CanBeDebuffed: false } when !buff.IsBigBuff() && battleEntity != buff.Caster:
                    continue;
            }
            
            foreach (BCardDTO bCard in buff.BCards)
            {
                switch (bCard.Type)
                {
                    case (byte)BCardType.VulcanoElementBuff:
                        if (bCard.SubType == (byte)AdditionalTypes.VulcanoElementBuff.CriticalDefenceTimes)
                        {
                            battleEntity.BCardDataComponent.MaxCriticals = buff.BCards.FirstOrDefault(x => x.Type == (short)BCardType.VulcanoElementBuff && x.SubType == (byte)AdditionalTypes.VulcanoElementBuff.CriticalDefenceTimes)?.SecondData ?? 0;
                        }
                        break;
                    case (byte)BCardType.FourthGlacernonFamilyRaid:
                        switch (bCard.SubType)
                        {
                            case (byte)AdditionalTypes.FourthGlacernonFamilyRaid.IncreaseMovementSpeedTick:
                                battleEntity.BCardDataComponent.IncreaseSpeedTick = 0;
                                break;
                            case (byte)AdditionalTypes.FourthGlacernonFamilyRaid.DecreaseMovementSpeedTick:
                                battleEntity.BCardDataComponent.DecreaseSpeedTick = 0;
                                break;
                        }

                        break;
                    case (byte)BCardType.BearSpirit:
                        switch (bCard.SubType)
                        {
                            case (byte)AdditionalTypes.BearSpirit.DamageNextSkillIncreased:
                                battleEntity.BCardDataComponent.VoodooDamageStored = 0;
                                break;
                            case (byte)AdditionalTypes.BearSpirit.MerlingTransformation:
                                battleEntity.BCardDataComponent.MerlingHit = 0;
                                break;
                        }

                        break;
                    case (byte)BCardType.Drain:
                        if (bCard.SubType == (byte)AdditionalTypes.Drain.AttackedBySunWolfIncreaseEffectDuration)
                        {
                            battleEntity.BCardDataComponent.SunWolfChanceIncreaseBuffDuration = 0;
                        }
                        break;
                }
            }
            
            if (battleEntity.BCardComponent.HasBCard(BCardType.EffectSummon, (byte)AdditionalTypes.EffectSummon.BlockNegativeEffect))
            {
                (int firstData, int secondData) = battleEntity.BCardComponent.GetAllBCardsInformation(BCardType.EffectSummon, (byte)AdditionalTypes.EffectSummon.BlockNegativeEffect, battleEntity.Level);
                if (buff.Level <= secondData && buff.BuffGroup is BuffGroup.Bad && battleEntity.BCardDataComponent.BlockBadBuff.HasValue)
                {
                    if (battleEntity.BCardDataComponent.BlockBadBuff < firstData)
                    {
                        battleEntity.BCardDataComponent.BlockBadBuff++;
                        continue;
                    }
                }
            }

            switch (buff.CardId)
            {
                case >= (short)BuffVnums.AGILITY_POWER and <= (short)BuffVnums.AGILITY_POWER_4:
                    battleEntity.MapInstance.Broadcast(battleEntity.GenerateSelfEffect(EffectType.AgilityPower));
                    break;
                case (short)BuffVnums.AGILITY_POWER_5 or (short)BuffVnums.AGILITY_POWER_6:
                    battleEntity.MapInstance.Broadcast(battleEntity.GenerateSelfEffect(EffectType.AgilityPowerEvolved));
                    break;
                case >= (short)BuffVnums.REFLECTION_POWER and <= (short)BuffVnums.REFLECTION_POWER_4:
                    battleEntity.MapInstance.Broadcast(battleEntity.GenerateSelfEffect(EffectType.BladeShield));
                    break;
                case (short)BuffVnums.REFLECTION_POWER_5 or (short)BuffVnums.REFLECTION_POWER_6:
                    battleEntity.MapInstance.Broadcast(battleEntity.GenerateSelfEffect(EffectType.ReflectionPowerEvolved));
                    break;
                case >= (short)BuffVnums.CURSE_POWER and <= (short)BuffVnums.CURSE_POWER_4:
                    battleEntity.MapInstance.Broadcast(battleEntity.GenerateSelfEffect(EffectType.CursePower));
                    break;
                case (short)BuffVnums.CURSE_POWER_5 or (short)BuffVnums.CURSE_POWER_6:
                    battleEntity.MapInstance.Broadcast(battleEntity.GenerateSelfEffect(EffectType.CursePowerEvolved));
                    break;
            }

            Buff soundFlowerBuff = battleEntity.BuffComponent.GetBuff((short)BuffVnums.SOUND_FLOWER_BLESSING_BETTER);
            if (soundFlowerBuff != null && buff.CardId == (short)BuffVnums.SOUND_FLOWER_BLESSING)
            {
                // Refresh sound flower buff duration
                soundFlowerBuff.SetBuffDuration(soundFlowerBuff.Duration);

                if (battleEntity is IPlayerEntity playerEntity)
                {
                    playerEntity.SendBfPacket(soundFlowerBuff, 0);
                }

                continue;
            }

            bool showMessage = true;
            Buff existingBuffWithSameId = battleEntity.BuffComponent.GetBuff(buff.CardId);
            if (existingBuffWithSameId != null && buff.IsNormal() && !buff.IsPartnerBuff())
            {
                await battleEntity.EmitEventAsync(new BuffRemoveEvent
                {
                    Entity = battleEntity,
                    Buffs = Lists.Create(existingBuffWithSameId),
                    RemovePermanentBuff = false,
                    ShowMessage = false
                });

                showMessage = false;
            }

            Buff existingBuffByGroupId = battleEntity.BuffComponent.GetBuffByGroupId(buff.GroupId);
            if (existingBuffByGroupId != null && !existingBuffByGroupId.IsBigBuff() && !buff.IsBigBuff())
            {
                showMessage = false;
                if (existingBuffByGroupId.Level > buff.Level)
                {
                    continue;
                }

                var buffVNum = new List<int> { 67, 72, 75, 89, 91, 134, 138, 139, 152, 153, 155, 157, 512, 556 };

                if (existingBuffByGroupId.Level == buff.Level && !buffVNum.Contains(buff.CardId))
                {
                    existingBuffByGroupId.SetBuffDuration(buff.Duration);

                    if (battleEntity is IPlayerEntity playerEntity)
                    {
                        playerEntity.SendBfPacket(existingBuffByGroupId, 0);
                    }

                    continue;
                }

                await battleEntity.EmitEventAsync(new BuffRemoveEvent
                {
                    Entity = battleEntity,
                    Buffs = Lists.Create(existingBuffByGroupId),
                    RemovePermanentBuff = false,
                    ShowMessage = false,
                    RemoveFromGroupId = false
                });
            }

            foreach (BCardDTO bCard in buff.BCards.Where(x => (!x.IsSecondBCardExecution.HasValue || !x.IsSecondBCardExecution.Value) && !x.TickPeriod.HasValue))
            {
                _bCardEffectHandlerContainer.Execute(battleEntity, buff.Caster, bCard);
            }

            // TODO: Check how to rework this. - Dazynnn
            switch (battleEntity)
            {
                case IMateEntity mateEntity:
                {
                    switch (buff.CardId)
                    {
                        case (short)BuffVnums.GIANTISM_F:
                        case (short)BuffVnums.GIANTISM_E:
                        case (short)BuffVnums.GIANTISM_D:
                        case (short)BuffVnums.GIANTISM_C:
                        case (short)BuffVnums.GIANTISM_B:
                        case (short)BuffVnums.GIANTISM_A:
                        case (short)BuffVnums.GIANTISM_S:
                            if (mateEntity.Specialist == null || !mateEntity.IsUsingSp)
                            {
                                break;
                            }

                            mateEntity.MapInstance.Broadcast(mateEntity.GenerateCMode((short)(mateEntity.Specialist.GameItem.Morph + 1)));
                            break;
                        case (short)BuffVnums.BERSERKER_FRENZY:
                            mateEntity.BroadcastEffectInRange(EffectType.LittleTransformation);
                            mateEntity.SpecialSkillVnum = (short)SkillsVnums.LUNGE;
                            mateEntity.Owner?.Session.SendEmptyMateSkillPacket();
                            mateEntity.Owner?.Session.SendMateSkillPacket(mateEntity);
                            mateEntity.Owner?.Session.SendMateSkillCooldown(mateEntity);
                            mateEntity.MapInstance.Broadcast(mateEntity.GenerateCMode(2713));
                            break;

                        case (short)BuffVnums.BATTLE_READY:
                            mateEntity.SpecialSkillVnum = (short)SkillsVnums.SMACKEROO;
                            mateEntity.Owner?.Session.SendEmptyMateSkillPacket();
                            mateEntity.Owner?.Session.SendMateSkillPacket(mateEntity);
                            mateEntity.Owner?.Session.SendMateSkillCooldown(mateEntity);
                            break;

                        case (short)BuffVnums.BEEDY_BYES:
                            mateEntity.SpecialSkillVnum = (short)SkillsVnums.BEARLY_AWAKE;
                            mateEntity.Owner?.Session.SendEmptyMateSkillPacket();
                            mateEntity.Owner?.Session.SendMateSkillPacket(mateEntity);
                            mateEntity.Owner?.Session.SendMateSkillCooldown(mateEntity);
                            break;
                    }
                }
                    break;

                case IPlayerEntity character:
                {
                    IClientSession session = character.Session;

                    switch (buff.CardId)
                    {
                        case (int)BuffVnums.SWARM_OF_BATS:
                        {
                            foreach (IMateEntity mate in character.MateComponent.TeamMembers())
                            {
                                if (!character.Session.HasCurrentMapInstance)
                                {
                                    continue;
                                }
                                character.Session.CurrentMapInstance.Broadcast(mate.GenerateOut());
                            }
                            break;
                        }
                        case (int)BuffVnums.COMBAT_READINESS:
                        {
                            session.PlayerEntity.UsageSkillWithoutCd = 3;
                            break;
                        }

                        case (int)BuffVnums.TRANSFORMATION_DRAGON:
                        {
                            session.PlayerEntity.LastDraconicMorph = DateTime.Now.AddSeconds(180);
                            break;
                        }

                        case (int)BuffVnums.MOONLIGHT_ABSORPTION:
                        {
                            await session.PlayerEntity.RemoveBuffAsync((int)BuffVnums.GATHERING_PETALS);
                            session.RefreshQuicklist();
                            break;
                        }
                        
                        case (int)BuffVnums.GATHERING_PETALS:
                        {
                            await session.PlayerEntity.RemoveBuffAsync((int)BuffVnums.MOONLIGHT_ABSORPTION);
                            session.RefreshQuicklist(true);
                            break;
                        }
                        
                        case (int)BuffVnums.ULTIMATE_STANCE:
                        {
                            session.RefreshQuicklist(true);
                            break;
                        }

                        case (short)BuffVnums.MAGICAL_FETTERS:
                            if (character.HasBuff(BuffVnums.MAGIC_SPELL))
                            {
                                await character.RemoveBuffAsync(false, character.BuffComponent.GetBuff((short)BuffVnums.MAGIC_SPELL));
                            }

                            break;
                        case (short)BuffVnums.AMBUSH_PREPARATION_1:
                            character.ChangeScoutState(ScoutStateType.FirstState);
                            break;
                        case (short)BuffVnums.AMBUSH_PREPARATION_2:
                            character.ChangeScoutState(ScoutStateType.SecondState);
                            break;
                        case (short)BuffVnums.MAGMA_PLATING:
                        case (short)BuffVnums.LIFE_SHIELD:
                            character.SkillComponent.MaxCriticals =
                                buff.BCards.FirstOrDefault(x => x.Type == (short)BCardType.VulcanoElementBuff && x.SubType == (byte)AdditionalTypes.VulcanoElementBuff.CriticalDefenceTimes)
                                    ?.SecondData ?? 0;
                            break;
                        case (short)BuffVnums.AMBUSH:
                            switch (character.ScoutStateType)
                            {
                                case ScoutStateType.FirstState:
                                    Buff toAdd = _buffFactory.CreateBuff((short)BuffVnums.AMBUSH_POSITION_1, character, buff.Duration);
                                    await character.AddBuffAsync(toAdd);

                                    Buff buffToRemove = character.BuffComponent.GetBuff((short)BuffVnums.AMBUSH_PREPARATION_1);
                                    await character.RemoveBuffAsync(false, buffToRemove);
                                    break;
                                case ScoutStateType.SecondState:
                                    Buff toAddSecond = _buffFactory.CreateBuff((short)BuffVnums.AMBUSH_POSITION_2, character, buff.Duration);
                                    await character.AddBuffAsync(toAddSecond);

                                    Buff secondBuffToRemove = character.BuffComponent.GetBuff((short)BuffVnums.AMBUSH_PREPARATION_2);
                                    await character.RemoveBuffAsync(false, secondBuffToRemove);
                                    break;
                            }

                            break;
                        case (short)BuffVnums.AMBUSH_RAID:
                            switch (character.ScoutStateType)
                            {
                                case ScoutStateType.FirstState:
                                    Buff toAdd = _buffFactory.CreateBuff((short)BuffVnums.SNIPER_POSITION_1, character, buff.Duration);
                                    await character.AddBuffAsync(toAdd);

                                    Buff buffToRemove = character.BuffComponent.GetBuff((short)BuffVnums.AMBUSH_POSITION_1);
                                    await character.RemoveBuffAsync(false, buffToRemove);
                                    break;
                                case ScoutStateType.SecondState:
                                    Buff toAddSecond = _buffFactory.CreateBuff((short)BuffVnums.SNIPER_POSITION_2, character, buff.Duration);
                                    await character.AddBuffAsync(toAddSecond);

                                    Buff secondBuffToRemove = character.BuffComponent.GetBuff((short)BuffVnums.AMBUSH_POSITION_2);
                                    await character.RemoveBuffAsync(false, secondBuffToRemove);
                                    break;
                            }

                            break;
                    }

                    if (!buff.IsBigBuff())
                    {
                        if (session.PlayerEntity == null)
                        {
                            return;
                        }
                        
                        switch (buff.CardId)
                        {
                            case (short)BuffVnums.CHARGE when session.PlayerEntity.BCardComponent.GetChargeBCards().Any():
                                int sum = session.PlayerEntity.BCardComponent.GetChargeBCards().Sum(x => x.FirstDataValue(session.PlayerEntity.Level));
                                session.PlayerEntity.SendBfPacket(buff, sum, sum);
                                break;
                            case (short)BuffVnums.CHARGE:
                                session.PlayerEntity.SendBfPacket(buff, session.PlayerEntity.ChargeComponent.GetCharge(), session.PlayerEntity.ChargeComponent.GetCharge());
                                break;
                            default:
                                session.PlayerEntity.SendBfPacket(buff, 0);
                                break;
                        }
                    }
                    else
                    {
                        session.SendStaticBuffUiPacket(buff, buff.RemainingTimeInMilliseconds());
                    }

                    if (showMessage)
                    {
                        string message = _gameLanguage.GetLanguage(GameDialogKey.BUFF_CHATMESSAGE_UNDER_EFFECT, session.UserLanguage);
                        string cardName = _gameLanguage.GetLanguage(GameDataType.Card, buff.Name, session.UserLanguage);

                        session.SendChatMessage(string.Format(message, cardName), !buff.IsSavingOnDisconnect() ? ChatMessageColorType.Buff : ChatMessageColorType.Red);
                    }

                    break;
                }
            }

            if (buff.IsConstEffect)
            {
                battleEntity.BroadcastConstBuffEffect(buff, 0);
                battleEntity.BroadcastConstBuffEffect(buff, (int)buff.Duration.TotalMilliseconds);
            }

            if (buff is { EffectId: > 0, IsConstEffect: false })
            {
                if (battleEntity?.MapInstance != null)
                {
                    var effect = new EffectServerPacket
                    {
                        EffectType = (byte)battleEntity.Type,
                        CharacterId = battleEntity.Id,
                        Id = buff.EffectId
                    };

                    battleEntity.MapInstance.Broadcast(effect);

                    battleEntity.SendBfPacket(buff, 0);
                }
            }
            
            battleEntity.BuffComponent.AddBuff(buff);
            battleEntity.BCardComponent.AddBuffBCards(buff);

            battleEntity.ShadowAppears(false, buff);
            switch (battleEntity)
            {
                case IPlayerEntity c:
                    await c.CheckAct52Buff(_buffFactory);
                    c.Session.RefreshStatChar();
                    c.Session.RefreshStat();
                    c.Session.SendCondPacket();
                    c.Session.SendIncreaseRange();
                    c.Session.UpdateVisibility();
                    break;
                case IMonsterEntity monsterEntity:
                    monsterEntity.RefreshStats();
                    break;
                case IMateEntity mateEntity:
                    mateEntity.Owner?.Session.SendPetInfo(mateEntity, _gameLanguage);
                    mateEntity.Owner?.Session.SendCondMate(mateEntity);
                    break;
            }
        }
    }
}