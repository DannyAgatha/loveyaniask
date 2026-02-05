using System;
using System.Linq;
using System.Threading.Tasks;
using WingsAPI.Packets.Enums.Chat;
using WingsEmu.Game._enum;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Algorithm;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Items;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Networking.Broadcasting;
using WingsEmu.Packets.ClientPackets;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;

namespace WingsEmu.Plugins.PacketHandling.Game.Inventory;

public class SpTransformPacketHandler : GenericGamePacketHandlerBase<SpTransformPacket>
{
    private readonly ICharacterAlgorithm _algorithm;
    private readonly IDelayManager _delayManager;
    private readonly IGameLanguageService _language;

    public SpTransformPacketHandler(IGameLanguageService language, IDelayManager delayManager, ICharacterAlgorithm algorithm)
    {
        _language = language;
        _delayManager = delayManager;
        _algorithm = algorithm;
    }

    protected override async Task HandlePacketAsync(IClientSession session, SpTransformPacket spTransformPacket)
    {
        GameItemInstance specialistInstance = session.PlayerEntity.Specialist;

        if (specialistInstance == null)
        {
            session.SendMsgi(MessageType.Default, Game18NConstString.NoSpecialistCardEquipped);
            return;
        }

        if (!session.PlayerEntity.IsAlive())
        {
            return;
        }

        if (session.PlayerEntity.IsSeal)
        {
            return;
        }

        if (spTransformPacket.Type == 10)
        {
            short specialistDamage = spTransformPacket.SpecialistDamage;
            short specialistDefense = spTransformPacket.SpecialistDefense;
            short specialistElement = spTransformPacket.SpecialistElement;
            short specialistHealth = spTransformPacket.SpecialistHp;
            int transportId = spTransformPacket.TransportId;
            if (transportId != specialistInstance.TransportId)
            {
                return;
            }

            if (specialistDamage < 0 || specialistDefense < 0 || specialistElement < 0 || specialistHealth < 0)
            {
                return;
            }

            if (specialistInstance.SlDamage + specialistDamage + specialistInstance.SlElement + specialistElement +
                specialistInstance.SlHp + specialistHealth + specialistInstance.SlDefence + specialistDefense > specialistInstance.SpPointsBasic())
            {
                return;
            }

            specialistInstance.SlDamage += specialistDamage;
            specialistInstance.SlDefence += specialistDefense;
            specialistInstance.SlElement += specialistElement;
            specialistInstance.SlHp += specialistHealth;

            session.PlayerEntity.SpecialistComponent.RefreshSlStats(specialistInstance.CurrentActiveSpecialistSlot);

            session.RefreshStatChar();
            session.RefreshStat();
            session.SendSpecialistCardInfo(specialistInstance, _algorithm);
            session.SendMsgi(MessageType.Default, Game18NConstString.StatusApplied);
            return;
        }

        if (session.PlayerEntity.IsSitting)
        {
            await session.RestAsync();
        }

        if (session.PlayerEntity.BuffComponent.HasBuff(BuffGroup.Bad))
        {
            session.SendMsgi(MessageType.Default, Game18NConstString.CanNotTransformBadEffect);
            session.PlayerEntity.BroadcastEndDancingGuriPacket();
            return;
        }

        if (session.PlayerEntity.IsOnVehicle)
        {
            session.SendMsgi(MessageType.Default, Game18NConstString.CantUseInVehicle);
            return;
        }

        if (session.PlayerEntity.IsMorphed)
        {
            session.PlayerEntity.BroadcastEndDancingGuriPacket();
            return;
        }
        
        if (!session.PlayerEntity.UseSp && session.PlayerEntity.Skills.Any(s => !session.PlayerEntity.SkillCanBeUsedSp(s)))
        {
            session.SendMsgi(MessageType.Default, Game18NConstString.CanTransformWithCooldownComplete);
            session.PlayerEntity.BroadcastEndDancingGuriPacket();
            return;
        }

        if (session.PlayerEntity.UseSp)
        {
            if (session.PlayerEntity.IsCastingSkill)
            {
                return;
            }

            if (session.PlayerEntity.LastSkillUse.AddSeconds(3) > DateTime.UtcNow)
            {
                return;
            }

            await session.EmitEventAsync(new SpUntransformEvent());
            return;
        }

        if (!session.PlayerEntity.IsSpCooldownElapsed())
        {
            session.SendMsgi(MessageType.Default, Game18NConstString.CantTrasformWithSideEffect, 4, session.PlayerEntity.GetSpCooldown());
            return;
        }

        if (session.PlayerEntity.LastSkillUse.AddSeconds(2) >= DateTime.UtcNow)
        {
            return;
        }

        if (spTransformPacket.Type == 1)
        {
            bool canWearSp = await _delayManager.CanPerformAction(session.PlayerEntity, DelayedActionType.WearSp);
            if (!canWearSp)
            {
                return;
            }

            await _delayManager.CompleteAction(session.PlayerEntity, DelayedActionType.WearSp);
            await session.EmitEventAsync(new SpTransformEvent
            {
                Specialist = specialistInstance
            });
        }
        else
        {
            DateTime waitUntil = await _delayManager.RegisterAction(session.PlayerEntity, DelayedActionType.WearSp);
            session.SendDelay((int)(waitUntil - DateTime.UtcNow).TotalMilliseconds, GuriType.Transforming, "sl 1");
            session.BroadcastGuri(2, 1, 0, new RangeBroadcast(session.PlayerEntity.PositionX, session.PlayerEntity.PositionY));
        }
    }
}