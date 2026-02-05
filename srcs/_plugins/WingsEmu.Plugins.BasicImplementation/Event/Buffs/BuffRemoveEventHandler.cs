using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NosEmu.Plugins.BasicImplementations.Vehicles;
using PhoenixLib.Events;
using WingsAPI.Game.Extensions.Groups;
using WingsAPI.Game.Extensions.Quicklist;
using WingsEmu.DTOs.BCards;
using WingsEmu.Game._enum;
using WingsEmu.Game._i18n;
using WingsEmu.Game._ItemUsage.Configuration;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Buffs.Events;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Extensions.Mates;
using WingsEmu.Game.Helpers.Damages;
using WingsEmu.Game.Mates;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Skills;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Battle;
using WingsEmu.Packets.Enums.Chat;

namespace NosEmu.Plugins.BasicImplementations.Event.Buffs;

public class BuffRemoveEventHandler : IAsyncEventProcessor<BuffRemoveEvent>
{
    private readonly IBuffFactory _buffFactory;
    private readonly IGameLanguageService _gameLanguage;
    private readonly ISacrificeManager _sacrificeManager;
    private readonly ISpPartnerConfiguration _spPartner;
    private readonly ITeleportManager _teleportManager;
    private readonly IVehicleConfigurationProvider _vehicle;

    public BuffRemoveEventHandler(ISpPartnerConfiguration spPartner, IGameLanguageService gameLanguage, IBuffFactory buffFactory, ITeleportManager teleportManager,
        ISacrificeManager sacrificeManager, IVehicleConfigurationProvider vehicle)
    {
        _spPartner = spPartner;
        _gameLanguage = gameLanguage;
        _buffFactory = buffFactory;
        _teleportManager = teleportManager;
        _sacrificeManager = sacrificeManager;
        _vehicle = vehicle;
    }

    public async Task HandleAsync(BuffRemoveEvent e, CancellationToken cancellation)
    {
        if (e.Buffs == null)
        {
            return;
        }

        IBattleEntity battleEntity = e.Entity;

        foreach (Buff buff in e.Buffs)
        {
            if (buff == null)
            {
                continue;
            }

            if (!battleEntity.BuffComponent.HasBuff(buff.BuffId))
            {
                continue;
            }

            if (buff.IsSavingOnDisconnect() && buff.RemainingTimeInMilliseconds() > 0)
            {
                continue;
            }

            foreach (BCardDTO bCard in buff.BCards)
            {
                switch (bCard.Type)
                {
                    case (byte)BCardType.EffectSummon:
                        if (bCard.SubType == (byte)AdditionalTypes.EffectSummon.BlockNegativeEffect)
                        {
                            battleEntity.BCardDataComponent.BlockBadBuff = null;
                        }
                        break;
                    case (byte)BCardType.ReflectDamage:
                        if (bCard.SubType == (byte)AdditionalTypes.ReflectDamage.CriticalAttackIncrease)
                        {
                            battleEntity.BCardDataComponent.CriticalDamageIncreased = null;
                        }
                        break;
                    case (byte)BCardType.Block:
                        if (bCard.SubType == (byte)AdditionalTypes.Block.DecreaseFinalCriticalDamagePerHit)
                        {
                            battleEntity.BCardDataComponent.CriticalDamageDecreased = null;
                        }
                        break;
                    case (byte)BCardType.VulcanoElementBuff:
                        if (bCard.SubType == (byte)AdditionalTypes.VulcanoElementBuff.CriticalDefenceTimes)
                        {
                            battleEntity.BCardDataComponent.MaxCriticals = null;
                        }
                        break;
                    case (byte)BCardType.FourthGlacernonFamilyRaid:
                        switch (bCard.SubType)
                        {
                            case (byte)AdditionalTypes.FourthGlacernonFamilyRaid.IncreaseMovementSpeedTick:
                                battleEntity.BCardDataComponent.IncreaseSpeedTick = null;
                                break;
                            case (byte)AdditionalTypes.FourthGlacernonFamilyRaid.DecreaseMovementSpeedTick:
                                battleEntity.BCardDataComponent.DecreaseSpeedTick = null;
                                break;
                        }

                        break;
                    case (byte)BCardType.BearSpirit:
                        switch (bCard.SubType)
                        {
                            case (byte)AdditionalTypes.BearSpirit.MerlingTransformation:
                            {
                                if (battleEntity is IPlayerEntity playerEntity)
                                {
                                    playerEntity.Morph = playerEntity.BCardDataComponent.OldMorph.Value;
                                    playerEntity.MorphUpgrade = playerEntity.BCardDataComponent.OldMorphUpgrade.Value;
                                    playerEntity.MorphUpgrade2 = playerEntity.BCardDataComponent.OldMorphUpgrade2.Value;
                                    playerEntity.IsMorphed = false;
                                    playerEntity.Session.BroadcastCMode();
                                    playerEntity.BCardDataComponent.MerlingHit = null;
                                    playerEntity.BCardDataComponent.OldMorph = null;
                                    playerEntity.BCardDataComponent.OldMorphUpgrade = null;
                                    playerEntity.BCardDataComponent.OldMorphUpgrade2 = null;
                                }

                                break;
                            }
                            case (byte)AdditionalTypes.BearSpirit.DamageNextSkillIncreased:
                                battleEntity.BCardDataComponent.VoodooDamageStored = null;
                                break;
                        }
                        break;
                    case (byte)BCardType.Absorption:
                        if (bCard.SubType == (byte)AdditionalTypes.Absorption.AbsordMaxHPAsDamage)
                        {
                            battleEntity.BCardDataComponent.AbsorptionDamage = null;
                        }
                        break;
                    case (byte)BCardType.Drain:
                        if (bCard.SubType == (byte)AdditionalTypes.Drain.AttackedBySunWolfIncreaseEffectDuration)
                        {
                            battleEntity.BCardDataComponent.SunWolfChanceIncreaseBuffDuration = null;
                        }
                        break;
                    
                    case (byte)BCardType.Damage:
                        battleEntity.HitsReceived = bCard.SubType switch
                        {
                            (byte)AdditionalTypes.Damage.HPIncreasedEveryAttackReceived => 0,
                            (byte)AdditionalTypes.Damage.MPIncreasedEveryAttackReceived => 0,
                            _ => battleEntity.HitsReceived
                        };
                        break;
                    
                    case (byte)BCardType.MineralTokenEffects:
                        battleEntity.HitsReceived = bCard.SubType switch
                        {
                            (byte)AdditionalTypes.MineralTokenEffects.DamageTakenIncreasesGauge => 0,
                            _ => battleEntity.HitsReceived
                        };

                        break;
                }
            }

            switch (buff.CardId)
            {
                case (short)BuffVnums.SPIRIT_OF_SACRIFICE:
                    {
                        IBattleEntity target = _sacrificeManager.GetTarget(battleEntity);
                        if (target != null)
                        {
                            _sacrificeManager.RemoveSacrifice(battleEntity, target);
                        }

                        break;
                    }
                case (short)BuffVnums.NOBLE_GESTURE:
                    {
                        IBattleEntity caster = _sacrificeManager.GetCaster(battleEntity);
                        if (caster != null)
                        {
                            _sacrificeManager.RemoveSacrifice(caster, battleEntity);
                        }

                        break;
                    }
                case (short)BuffVnums.MIND_SINK:
                    battleEntity.HitsReceived = 0;
                    break;
            }

            battleEntity.BuffComponent.RemoveBuff(buff.BuffId);
            battleEntity.BCardComponent.RemoveBuffBCards(buff);
            battleEntity.ShadowAppears(true, buff);
            ProcessEndBuffDamage(battleEntity, buff);

            if (buff.IsConstEffect)
            {
                battleEntity.BroadcastConstBuffEffect(buff, 0);
            }

            if ((buff.IsPartnerBuff() || buff.IsBigBuff() && !buff.IsPartnerBuff() || buff.IsNoDuration() || buff.IsRefreshAtExpiration()) && !e.RemovePermanentBuff)
            {
                Buff newBuff = _buffFactory.CreateBuff(buff.CardId, buff.Caster, buff.Duration, buff.BuffFlags);
                await battleEntity.AddBuffAsync(newBuff);
                continue;
            }

            if (battleEntity is IPlayerEntity chara)
            {
                switch (buff.CardId)
                {
                    case (short)BuffVnums.TART_HAPENDAM_NO_HERO when chara.Level < 85:
                        await chara.AddBuffAsync(_buffFactory.CreateBuff((short)BuffVnums.TART_HAPENDAM_NO_HERO, chara));
                        break;
                    case (short)BuffVnums.TART_HAPENDAM_HERO when chara.HeroLevel != 0 && chara.HeroLevel < 30:
                        await chara.AddBuffAsync(_buffFactory.CreateBuff((short)BuffVnums.TART_HAPENDAM_HERO, chara));
                        break;
                }
            }

            if (battleEntity is not IPlayerEntity character)
            {
                bool buffRunAway = buff.BCards.Any(x => x.Type == (short)BCardType.SpecialActions && x.SubType == (byte)AdditionalTypes.SpecialActions.RunAway);

                switch (battleEntity)
                {
                    case INpcEntity npcEntity:

                        if (buffRunAway)
                        {
                            npcEntity.IsRunningAway = false;
                        }

                        break;
                    case IMonsterEntity monsterEntity:
                        monsterEntity.RefreshStats();

                        if (buffRunAway)
                        {
                            monsterEntity.IsRunningAway = false;
                        }

                        break;
                    
                    // TODO: Check how to rework this. - Dazynnn
                    case IMateEntity mateEntity:
                        mateEntity.RefreshStatistics();
                        mateEntity.Owner?.Session.SendPetInfo(mateEntity, _gameLanguage);
                        mateEntity.Owner?.Session.SendCondMate(mateEntity);
                        
                        switch (buff.CardId)
                        {
                            case (short)BuffVnums.GIANTISM_F:
                            case (short)BuffVnums.GIANTISM_E:
                            case (short)BuffVnums.GIANTISM_D:
                            case (short)BuffVnums.GIANTISM_C:
                            case (short)BuffVnums.GIANTISM_B:
                            case (short)BuffVnums.GIANTISM_A:
                            case (short)BuffVnums.GIANTISM_S:
                            {
                                if (mateEntity.Specialist == null || !mateEntity.IsUsingSp)
                                {
                                    break;
                                }
                                mateEntity.MapInstance.Broadcast(mateEntity.GenerateCMode(mateEntity.Specialist.GameItem.Morph));
                                break;
                            }
                            
                            case (short)BuffVnums.BERSERKER_FRENZY:
                                mateEntity.BroadcastEffectInRange(EffectType.LittleUntransformation);
                                mateEntity.SpecialSkillVnum = null;
                                mateEntity.Owner?.Session.SendMateSkillPacket(mateEntity);
                                mateEntity.Owner?.Session.SendMateSkillCooldown(mateEntity);
                                mateEntity.MapInstance.Broadcast(mateEntity.GenerateCMode(-1));
                                break;
                            
                            case (short)BuffVnums.BATTLE_READY:
                                mateEntity.SpecialSkillVnum = null;
                                mateEntity.Owner?.Session.SendMateSkillPacket(mateEntity);
                                mateEntity.Owner?.Session.SendMateSkillCooldown(mateEntity);
                                break;
                            
                            case (short)BuffVnums.BEEDY_BYES:
                                mateEntity.SpecialSkillVnum = null;
                                mateEntity.Owner?.Session.SendMateSkillPacket(mateEntity);
                                mateEntity.Owner?.Session.SendMateSkillCooldown(mateEntity);
                                break;
                        }
                        break;
                }

                continue;
            }

            if (battleEntity is IPlayerEntity entity)
            {
                bool neliaBuff = buff.BCards.Any(x => x.Type == (short)BCardType.ChangingPlace && x.SubType == (byte)AdditionalTypes.ChangingPlace.ReplaceTargetPosition);
                
                if (entity.MapInstance != null && entity.SkillComponent != null)
                {
                    IBattleEntity? nelia = entity.MapInstance.GetBattleEntity(VisualType.Npc, entity.SkillComponent.NeliaId);
                    
                    if (nelia != null && neliaBuff)
                    {
                        nelia.ChangePosition(entity.Position);
                        nelia.TeleportOnMap(nelia.PositionX, nelia.PositionY);
                        entity.ChangePosition(entity.SkillComponent.PlayerPosition);
                        entity.TeleportOnMap(entity.PositionX, entity.PositionY);
                    }
                }
            }

            IClientSession session = character.Session;
            bool refreshHpMp = true;

            switch (buff.CardId)
            {
                case (int)BuffVnums.FISH_LINE:
                {
                    if (character.HasCaughtFish || character.HasFishingLineBroke || character.HasBadLuck)
                    {
                        character.CanCollectFish = false;
                        character.IsRareFish = false;
                        character.HasCaughtFish = false;
                        character.HasFishingLineBroke = false;
                        character.HasBadLuck = false;
                    }
                    else
                    {
                        character.Session.SendSayi(ChatMessageColorType.White, Game18NConstString.FishAitBait);
                        character.CanCollectFish = false;
                        character.IsRareFish = false;
                        character.HasCaughtFish = false;
                        character.HasFishingLineBroke = false;
                        character.HasBadLuck = false;
                    }
                    
                    character?.FishingFirstFish?.Dispose();
                    character?.FishingSecondFish?.Dispose();
                    character.Session.CurrentMapInstance.Broadcast(character.Session.GenerateGuriPacket(6, 1, character.Id, 0));
                    break;
                }

                case (int)BuffVnums.GATHERING_PETALS:
                case (int)BuffVnums.MOONLIGHT_ABSORPTION:
                case (int)BuffVnums.ULTIMATE_STANCE:
                case (int)BuffVnums.ULTIMATE_AURA_I:
                case (int)BuffVnums.ULTIMATE_AURA_II:
                case (int)BuffVnums.ULTIMATE_AURA_III:
                {
                    session.RefreshQuicklist();
                    break;
                }
                
                case (int)BuffVnums.COMBAT_READINESS:
                {
                    session.PlayerEntity.UsageSkillWithoutCd = 0;
                    break;
                }
                
                case (int)BuffVnums.TRANSFORMATION_DRAGON:
                {
                    session.PlayerEntity.IsDraconicMorphed = false;
                    session.PlayerEntity.Morph = (byte)MorphType.DraconicFist;
                    session.PlayerEntity.Session.BroadcastCMode();
                    session.PlayerEntity.Session.BroadcastEffect(EffectType.Transform);
                    session.PlayerEntity.Session.SendCancelPacket(CancelType.InCombatMode);
                    break;
                }
                case (short)BuffVnums.MISTIFYING_F:
                case (short)BuffVnums.MISTIFYING_E:
                case (short)BuffVnums.MISTIFYING_D:
                case (short)BuffVnums.MISTIFYING_C:
                case (short)BuffVnums.MISTIFYING_B:
                case (short)BuffVnums.MISTIFYING_A:
                case (short)BuffVnums.MISTIFYING_S:
                    {
                        session.PlayerEntity.BlockAllAttack = false;
                        if (session.PlayerEntity.Specialist != null && !session.PlayerEntity.UseSp && session.PlayerEntity.WasMorphedPreviously)
                        {
                            await session.EmitEventAsync(new SpTransformEvent
                            {
                                Forced = true,
                                Specialist = session.PlayerEntity.Specialist
                            });
                            session.RefreshStat();
                        }
                        break;
                    }
                case (short)BuffVnums.PRAYER_OF_DEFENCE:
                case (short)BuffVnums.ENERGY_ENHANCEMENT:
                case (short)BuffVnums.BEAR_SPIRIT:
                case (short)BuffVnums.BEAR_SPIRIT_CAPSULE:
                    if (e.ShowMessage)
                    {
                        break;
                    }

                    refreshHpMp = false;
                    break;
                case (short)BuffVnums.FAIRY_BOOSTER:
                    session.RefreshFairy();
                    break;
                case (short)BuffVnums.SPEED_BOOSTER when session.PlayerEntity.IsOnVehicle:
                    session.PlayerEntity.VehicleSpeed -= (byte)BuffVehicle(session.PlayerEntity);
                    break;
                case (short)BuffVnums.MAGIC_SPELL:
                    session.SendMsCPacket(0);
                    session.PlayerEntity.RemoveAngelElement();
                    character.CleanComboState();
                    for (int i = (int)BuffVnums.FLAME; i < (int)BuffVnums.DARKNESS; i++)
                    {
                        if (!session.PlayerEntity.BuffComponent.HasBuff(i))
                        {
                            continue;
                        }

                        session.PlayerEntity.RemoveBuffAsync(false, session.PlayerEntity.BuffComponent.GetBuff(i)).ConfigureAwait(false).GetAwaiter().GetResult();
                    }

                    break;
                case (short)BuffVnums.AMBUSH_PREPARATION_1:
                case (short)BuffVnums.AMBUSH_PREPARATION_2:
                    if (buff.RemainingTimeInMilliseconds() <= 0)
                    {
                        character.ChangeScoutState(ScoutStateType.None);
                    }

                    break;
                case (short)BuffVnums.AMBUSH_RAID:
                    character.ChangeScoutState(ScoutStateType.None);
                    break;
                case (short)BuffVnums.AMBUSH:
                    character.TriggerAmbush = false;

                    if (buff.RemainingTimeInMilliseconds() <= 0)
                    {
                        character.ChangeScoutState(ScoutStateType.None);
                    }
                    break;
                case (short)BuffVnums.MAGMA_PLATING:
                case (short)BuffVnums.LIFE_SHIELD:
                    character.SkillComponent.MaxCriticals = null;
                    break;
                case (short)BuffVnums.WATERFALL_FRENZY:
                    if (!character.PreventEnergyRemove)
                    {
                        await session.PlayerEntity.UpdateEnergyBar(-100);
                    }
                    break;
            }


            if (session.PlayerEntity.Morph == (byte)MorphType.FlameDruid && session.PlayerEntity.HasBuff(BuffVnums.RED_LEOPARD_ENERGY))
            {
                session.PlayerEntity.RemoveBuffAsync((int)BuffVnums.RED_LEOPARD_ENERGY, true).ConfigureAwait(false).GetAwaiter().GetResult();
                break;
            }
            
            if (!buff.IsBigBuff())
            {
                session.PlayerEntity.SendBfPacket(buff);
            }
            else
            {
                session.SendEmptyStaticBuffUiPacket(buff);
            }

            if (e.ShowMessage)
            {
                string message = _gameLanguage.GetLanguage(GameDialogKey.BUFF_CHATMESSAGE_EFFECT_TERMINATED, session.UserLanguage);
                string cardName = _gameLanguage.GetLanguage(GameDataType.Card, buff.Name, session.UserLanguage);

                session.SendChatMessage(string.Format(message, cardName), ChatMessageColorType.Buff);
            }

            session.RefreshStatChar(refreshHpMp);
            session.RefreshStat();
            session.SendCondPacket();
            
            foreach (BCardDTO buffBCard in buff.BCards)
            {
                session.PlayerEntity.BCardStackComponent.RemoveStackBCard(((short)buffBCard.Type, buffBCard.SubType));
            }

            if (buff.BCards.Any(x => x.Type == (byte)BCardType.FearSkill && x.SubType == (byte)AdditionalTypes.FearSkill.AttackRangedIncreased))
            {
                session.SendIncreaseRange();
            }

            Position position = _teleportManager.GetPosition(session.PlayerEntity.Id);
            if (position.X != 0 && position.Y != 0 && buff.CardId == (short)BuffVnums.MEMORIAL)
            {
                short savedX = _teleportManager.GetPosition(character.Id).X;
                short savedY = _teleportManager.GetPosition(character.Id).Y;
                _teleportManager.RemovePosition(session.PlayerEntity.Id);
                character.BroadcastEffectGround(EffectType.ArchmageTeleportSet, savedX, savedY, true);
            }

            await character.CheckAct52Buff(_buffFactory);

            if (buff.BCards.Any(x => x.Type == (short)BCardType.FearSkill && x.SubType == (byte)AdditionalTypes.FearSkill.MoveAgainstWill))
            {
                session.SendOppositeMove(false);
            }

            if (!buff.BCards.Any(s =>
                    s.Type == (short)BCardType.SpecialActions && s.SubType == (byte)AdditionalTypes.SpecialActions.Hide)
                && buff.CardId != (short)BuffVnums.AMBUSH && buff.CardId != (short)BuffVnums.AMBUSH_RAID && buff.CardId != (short)BuffVnums.SWARM_OF_BATS &&
                !buff.BCards.Any(s => s.Type == (short)BCardType.LordBerios && s.SubType == (byte)AdditionalTypes.LordBerios.InvisibleStateUnchangedOnDefence))
            {
                continue;
            }

            if (buff.CardId == (short)BuffVnums.AMBUSH && character.TriggerAmbush)
            {
                continue;
            }

            if (!session.PlayerEntity.IsOnVehicle)
            {
                session.BroadcastInTeamMembers(_gameLanguage, _spPartner);
                session.RefreshParty(_spPartner);
            }

            session.UpdateVisibility();
        }
    }

    private void ProcessEndBuffDamage(IBattleEntity battleEntity, Buff buff)
    {
        if (!battleEntity.EndBuffDamages.Any())
        {
            return;
        }

        battleEntity.RemoveEndBuffDamage((short)buff.CardId);
    }

    private int BuffVehicle(IPlayerEntity c)
    {
        VehicleConfiguration vehicle = _vehicle.GetByMorph(c.Morph, c.Gender);

        if (vehicle?.VehicleBoostType == null)
        {
            return 0;
        }

        int speedToRemove = 0;

        foreach (VehicleBoost boost in vehicle.VehicleBoostType)
        {
            switch (boost.BoostType)
            {
                case BoostType.INCREASE_SPEED:
                    if (!boost.FirstValue.HasValue)
                    {
                        break;
                    }

                    speedToRemove = boost.FirstValue.Value;
                    break;
                case BoostType.CREATE_BUFF_ON_END:
                    if (!boost.FirstValue.HasValue)
                    {
                        break;
                    }

                    c.AddBuffAsync(_buffFactory.CreateBuff(boost.FirstValue.Value, c)).ConfigureAwait(false).GetAwaiter().GetResult();
                    break;
                case BoostType.DODGE_ALL_ATTACK:
                    c.NoDamageChance = 0;
                    break;
            }
        }

        return speedToRemove;
    }
}