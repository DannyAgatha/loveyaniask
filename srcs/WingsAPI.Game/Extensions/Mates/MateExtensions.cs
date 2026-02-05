using System;
using System.Threading.Tasks;
using WingsEmu.DTOs.Maps;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Helpers.Damages;
using WingsEmu.Game.Items;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Mates;
using WingsEmu.Game.Mates.Events;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Battle;

namespace WingsEmu.Game.Extensions.Mates;

public static class MateExtensions
{
    public static int GetModifier(this IMateEntity mate)
    {
        return mate.AttackType switch
        {
            AttackType.Melee => mate.MeleeHpFactor,
            AttackType.Ranged => mate.RangeDodgeFactor,
            AttackType.Magical => mate.MagicMpFactor,
            _ => 0
        };
    }


    public static void TeleportNearCharacter(this IMateEntity mateEntity)
    {
        IClientSession session = mateEntity.Owner?.Session;
        if (session?.CurrentMapInstance == null)
        {
            return;
        }

        mateEntity.ChangePosition(new Position((short)(session.PlayerEntity.PositionX + (mateEntity.MateType == MateType.Partner ? -1 : 1)), (short)(session.PlayerEntity.PositionY + 1)));

        if (mateEntity.MapInstance.HasMapFlag(MapFlags.IS_BASE_MAP))
        {
            mateEntity.MapX = mateEntity.Position.X;
            mateEntity.MapY = mateEntity.Position.Y;
        }

        if (mateEntity.MapInstance.MapInstanceType == MapInstanceType.Miniland && mateEntity.MapInstance.Id == mateEntity.Owner.Miniland.Id)
        {
            mateEntity.MinilandX = mateEntity.Position.X;
            mateEntity.MinilandY = mateEntity.Position.Y;
        }

        bool isBlocked = session.PlayerEntity.MapInstance.IsBlockedZone(mateEntity.PositionX, mateEntity.PositionY);

        if (!isBlocked)
        {
            return;
        }

        mateEntity.ChangePosition(new Position(session.PlayerEntity.PositionX, session.PlayerEntity.PositionY));

        if (mateEntity.MapInstance.MapInstanceType == MapInstanceType.Miniland && mateEntity.MapInstance.Id == mateEntity.Owner.Miniland.Id)
        {
            mateEntity.MinilandX = mateEntity.Position.X;
            mateEntity.MinilandY = mateEntity.Position.Y;
        }

        if (!mateEntity.MapInstance.HasMapFlag(MapFlags.IS_BASE_MAP))
        {
            return;
        }

        mateEntity.MapX = mateEntity.Position.X;
        mateEntity.MapY = mateEntity.Position.Y;
    }

    public static void TeleportToCharacter(this IMateEntity mateEntity)
    {
        IClientSession session = mateEntity.Owner?.Session;
        if (session == null || session.PlayerEntity.IsOnVehicle)
        {
            return;
        }


        mateEntity.ChangePosition(new Position(session.PlayerEntity.PositionX, session.PlayerEntity.PositionY));
        session.BroadcastMateTeleport(mateEntity);
    }

    public static bool IsInCombat(this IMateEntity mateEntity, DateTime date) => mateEntity.LastDefence.AddSeconds(4) > date || mateEntity.LastSkillUse.AddSeconds(2) > date;

    public static bool CanAttack(this IMateEntity mateEntity) => !mateEntity.Owner.IsOnVehicle && mateEntity.Loyalty != 0 && mateEntity.CanPerformAttack();

    public static bool CanMove(this IMateEntity mateEntity)
    {
        if (mateEntity.Loyalty <= 0)
        {
            return false;
        }

        return mateEntity.CanPerformMove();
    }

    public static void RemoveLoyalty(this IMateEntity mateEntity, short loyalty, GameMinMaxConfiguration minMaxConfiguration, IGameLanguageService languageService)
    {
        if (mateEntity.Loyalty < minMaxConfiguration.MinMateLoyalty)
        {
            mateEntity.Loyalty = minMaxConfiguration.MinMateLoyalty;
            return;
        }

        mateEntity.Loyalty -= Math.Abs(loyalty);

        if (mateEntity.Loyalty < minMaxConfiguration.MinMateLoyalty)
        {
            mateEntity.Loyalty = minMaxConfiguration.MinMateLoyalty;
        }

        mateEntity.Owner.Session.SendPetInfo(mateEntity, languageService);
    }

    public static void AddLoyalty(this IMateEntity mateEntity, short loyalty, GameMinMaxConfiguration minMaxConfiguration, IGameLanguageService languageService)
    {
        if (mateEntity.Loyalty == minMaxConfiguration.MaxMateLoyalty)
        {
            return;
        }

        if (mateEntity.Loyalty > minMaxConfiguration.MaxMateLoyalty)
        {
            mateEntity.Loyalty = minMaxConfiguration.MaxMateLoyalty;
            return;
        }

        mateEntity.Loyalty += Math.Abs(loyalty);

        if (mateEntity.Loyalty > minMaxConfiguration.MaxMateLoyalty)
        {
            mateEntity.Loyalty = minMaxConfiguration.MaxMateLoyalty;
        }

        mateEntity.Owner.Session.SendPetInfo(mateEntity, languageService);
    }

    public static bool CanWearItem(this IMateEntity entity, IGameItem gameItem)
    {
        if (gameItem.EquipmentSlot != EquipmentType.Sp && gameItem.EquipmentSlot != EquipmentType.Armor && gameItem.EquipmentSlot != EquipmentType.MainWeapon)
        {
            return true;
        }

        if (gameItem.IsPartnerSpecialist)
        {
            AttackType mateSpecialist = gameItem.PartnerClass switch
            {
                0 => AttackType.Melee,
                1 => AttackType.Ranged,
                2 => AttackType.Magical,
                _ => AttackType.Other
            };

            return mateSpecialist == entity.AttackType;
        }

        AttackType mateClass = gameItem.Class switch
        {
            0 => AttackType.Melee,
            2 => AttackType.Melee,
            4 => AttackType.Ranged,
            8 => AttackType.Magical,
            _ => AttackType.Other
        };

        return mateClass == entity.AttackType;
    }

    public static bool CanWearSpecialist(this IMateEntity entity, IGameItem gameItem) => gameItem.IsPartnerSpecialist && entity.AttackType == (AttackType)gameItem.PartnerClass;

    public static int GetSpCooldown(this IMateEntity mate) => !mate.SpCooldownEnd.HasValue ? 0 : (int)(mate.SpCooldownEnd.Value - DateTime.UtcNow).TotalSeconds;

    public static bool IsSpCooldownElapsed(this IMateEntity mate)
    {
        if (!mate.SpCooldownEnd.HasValue)
        {
            return true;
        }

        return mate.SpCooldownEnd.Value < DateTime.UtcNow;
    }

    public static bool HavePartnerSkill(this IMateEntity mateEntity, byte slot)
    {
        bool skill = slot switch
        {
            0 => mateEntity.Specialist.PartnerSkill1,
            1 => mateEntity.Specialist.PartnerSkill2,
            2 => mateEntity.Specialist.PartnerSkill3,
            _ => false
        };

        return skill;
    }

    public static async Task RemovePartnerSp(this IMateEntity mateEntity)
    {
        if (!mateEntity.IsUsingSp)
        {
            return;
        }

        await mateEntity.Owner.Session.EmitEventAsync(new MateSpUntransformEvent
        {
            MateEntity = mateEntity
        });
    }

    public static void RefreshPartnerSkills(this IMateEntity mateEntity)
    {
        if (mateEntity.MateType != MateType.Partner)
        {
            return;
        }
        
        if (mateEntity.Skills?.Count == 0)
        {
            return;
        }
        
        mateEntity.Owner.Session.RefreshSkillList();
    }

    public static bool SkillRankS(this IMateEntity mateEntity, byte slot) => mateEntity.Specialist?.PartnerSkills?.Find(s => s.Rank == 7 && s.Slot == slot) != null;

    public static byte GetFreeMateSlot(this IPlayerEntity player, bool isPartner)
    {
        byte slot;
        byte maxCount = isPartner ? player.MaxPartnerCount : player.MaxPetCount;
        for (slot = 0; slot < maxCount; slot++)
        {
            IMateEntity getMate = isPartner
                ? player.MateComponent.GetMate(m => m.PetSlot == slot && m.MateType == MateType.Partner)
                : player.MateComponent.GetMate(m => m.PetSlot == slot && m.MateType == MateType.Pet);

            if (getMate != null)
            {
                continue;
            }

            break;
        }

        return slot;
    }

    public static void RefreshEquipmentValues(this IMateEntity mateEntity, GameItemInstance gameItemInstance, bool clearValues)
    {
        if (clearValues)
        {
            mateEntity.BCardComponent.ClearEquipmentBCards(gameItemInstance.GameItem.EquipmentSlot);
            return;
        }

        mateEntity.BCardComponent.AddEquipmentBCards(gameItemInstance.GameItem.EquipmentSlot, gameItemInstance.GameItem.BCards);
    }
}