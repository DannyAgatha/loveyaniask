using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PhoenixLib.Scheduler;
using WingsAPI.Game.Extensions.Quicklist;
using WingsEmu.DTOs.Skills;
using WingsEmu.Game;
using WingsEmu.Game._enum;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Fish;
using WingsEmu.Game.GameEvent;
using WingsEmu.Game.Helpers.Damages;
using WingsEmu.Game.Items;
using WingsEmu.Game.Managers.StaticData;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Skills;
using WingsEmu.Packets.ClientPackets;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Battle;
using WingsEmu.Packets.Enums.Character;
using WingsEmu.Packets.Enums.Chat;
using WingsEmu.Plugins.GameEvents.DataHolder;

namespace WingsEmu.Plugins.PacketHandling.Game.Battle;

public class UseAtSkillPacketHandler : GenericGamePacketHandlerBase<UseAtSkillPacket>
{
    private readonly ICardsManager _cardsManager;
    private readonly IGameLanguageService _gameLanguage;
    private readonly RainbowBattleConfiguration _rainbowBattleConfiguration;
    private readonly ISkillsManager _skillsManager;
    private readonly IFishManager _fishManager;
    private readonly IRandomGenerator _randomGenerator;

    public UseAtSkillPacketHandler(IGameLanguageService gameLanguage, ICardsManager cardsManager, 
        ISkillsManager skillsManager, RainbowBattleConfiguration rainbowBattleConfiguration, 
        IFishManager fishManager, IRandomGenerator randomGenerator)
    {
        _gameLanguage = gameLanguage;
        _cardsManager = cardsManager;
        _skillsManager = skillsManager;
        _rainbowBattleConfiguration = rainbowBattleConfiguration;
        _fishManager = fishManager;
        _randomGenerator = randomGenerator;
    }

    protected override async Task HandlePacketAsync(IClientSession session, UseAtSkillPacket packet)
    {
        IPlayerEntity character = session.PlayerEntity;

        if (!character.CanFight() || packet == null)
        {
            session.SendCancelPacket(CancelType.NotInCombatMode);
            session.SendDebugMessage("[U_AS] !canFight, packet null");
            return;
        }

        if (session.CurrentMapInstance.IsPvp && session.CurrentMapInstance.PvpZone(session.PlayerEntity.PositionX, session.PlayerEntity.PositionY) &&
            session.CurrentMapInstance.PvpZone(packet.MapX, packet.MapY))
        {
            session.SendCancelPacket(CancelType.NotInCombatMode);
            session.SendDebugMessage("[U_AS] Character no-PvP zone / !map.IsPvp");
            return;
        }

        if (session.CurrentMapInstance.IsBlockedZone(packet.MapX, packet.MapY))
        {
            session.SendDebugMessage("[U_AS] character.IsBlockedZone()");
            session.SendCancelPacket(CancelType.NotInCombatMode);
            return;
        }

        if (session.IsMuted())
        {
            session.SendMuteMessage();
            session.SendCancelPacket(CancelType.NotInCombatMode);
            return;
        }

        if (session.PlayerEntity.IsOnVehicle || session.PlayerEntity.CheatComponent.IsInvisible)
        {
            session.SendDebugMessage("[U_AS] character.IsVehicled, InvisibleGm");
            session.SendCancelPacket(CancelType.NotInCombatMode);
            return;
        }

        if (!session.PlayerEntity.CanPerformAttack())
        {
            session.SendDebugMessage("[U_AS] !CanPerformAttack");
            session.SendCancelPacket(CancelType.NotInCombatMode);
            return;
        }

        if ((DateTime.UtcNow - session.PlayerEntity.LastTransform).TotalSeconds < 3)
        {
            session.SendDebugMessage("[U_AS] Under transformation cooldown");
            session.SendCancelPacket(CancelType.NotInCombatMode);
            session.SendMsg(_gameLanguage.GetLanguage(GameDialogKey.SPECIALIST_SHOUTMESSAGE_CANT_ATTACK_YET, session.UserLanguage), MsgMessageType.Middle);
            return;
        }

        if (character.IsCastingSkill)
        {
            session.SendCancelPacket(CancelType.NotInCombatMode);
            session.SendDebugMessage("[U_AS] Already using a skill");
            return;
        }

        List<IBattleEntitySkill> skills = session.PlayerEntity.Skills;
        var characterSkill = skills.FirstOrDefault(s => s.Skill?.CastId == packet.CastId &&
            (s.Skill?.UpgradeSkill == 0 || s.Skill?.SkillType == SkillType.NormalPlayerSkill) && s.Skill.TargetType == TargetType.NonTarget) as CharacterSkill;
        SkillDTO skill = characterSkill?.Skill;

        if (skill == null)
        {
            session.SendDebugMessage("[U_AS] Skill does not exist");
            session.SendCancelPacket(CancelType.NotInCombatMode);
            return;
        }

        SkillInfo skillInfo = session.PlayerEntity.GetUpgradedSkill(skill, _cardsManager, _skillsManager) ?? skill.GetInfo();

        bool canBeUsed = session.PlayerEntity.CharacterCanCastOrCancel(characterSkill, _gameLanguage, skillInfo, false, _fishManager, _randomGenerator);
        if (!canBeUsed)
        {
            session.SendDebugMessage("[U_AS] !skill.canBeUsed");
            return;
        }

        if (!session.PlayerEntity.CanPerformAttack(skillInfo))
        {
            session.SendCancelPacket(CancelType.NotInCombatMode);
            return;
        }

        if (session.PlayerEntity.IsSitting)
        {
            await session.RestAsync();
        }

        var position = new Position(packet.MapX, packet.MapY);

        if (!session.PlayerEntity.Position.IsInRange(position, skillInfo.Range + 3))
        {
            session.SendDebugMessage("[U_AS] !character.IsInRange");
            session.SendCancelPacket(CancelType.NotInCombatMode);
            return;
        }

        if (session.PlayerEntity.RainbowBattleComponent.IsFrozen)
        {
            session.SendCancelPacket(CancelType.NotInCombatMode);
            return;
        }

        if (!session.PlayerEntity.CheatComponent.HasGodMode)
        {
            skillInfo.ManaCost = session.PlayerEntity.BCardComponent.HasBCard(BCardType.TimeCircleSkills, (byte)AdditionalTypes.TimeCircleSkills.DisableMPConsumption) ? 0 : skillInfo.ManaCost;
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
                if (session.PlayerEntity.Hp - hpRemoved <= 0)
                {
                    session.PlayerEntity.BroadcastDamage(session.PlayerEntity.Hp - 1);
                    session.PlayerEntity.Hp = 1;
                }
                else
                {
                    session.PlayerEntity.BroadcastDamage(hpRemoved);
                    session.PlayerEntity.Hp -= hpRemoved;
                }
            }

            session.RefreshStat();
        }

        session.SendDebugMessage($"Hit {skill.HitType} / Target {skill.TargetType} / Attack Type {skill.AttackType} / Affected entities {skill.TargetAffectedEntities}");

        session.PlayerEntity.CleanComboState();
        session.SendMsCPacket(0);
        session.RefreshQuicklist();
        session.RefreshStat();
        session.PlayerEntity.ClearFoodBuffer();

        if (session.PlayerEntity.RainbowBattleComponent.IsInRainbowBattle)
        {
            session.PlayerEntity.RainbowBattleComponent.ActivityPoints += _rainbowBattleConfiguration.UsingSkillActivityPoints;
        }

        character.WeaponLoaded(characterSkill, _gameLanguage, true);
        character.LastSkillUse = DateTime.UtcNow;
        character.LastSkillId = skill.Id;
        DateTime castTime = character.GenerateSkillCastTime(skillInfo);
        character.SkillComponent.CanBeInterrupted = character.CanBeInterrupted(skillInfo);

        await character.EmitEventAsync(new BattleExecuteSkillEvent(character, null, skillInfo, castTime, position));
    }
}