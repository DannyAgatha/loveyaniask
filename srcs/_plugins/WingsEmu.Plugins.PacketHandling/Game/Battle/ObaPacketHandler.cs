// NosEmu
// 


using System;
using System.Threading.Tasks;
using WingsEmu.DTOs.Skills;
using WingsEmu.Game._enum;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Helpers.Damages;
using WingsEmu.Game.Managers.StaticData;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Skills;
using WingsEmu.Packets.ClientPackets;
using WingsEmu.Packets.Enums;

namespace WingsEmu.Plugins.PacketHandling.Game.Battle;

public class ObaPacketHandler : GenericGamePacketHandlerBase<ObaPacket>
{
    private readonly IGameLanguageService _gameLanguage;
    private readonly ISkillsManager _skillsManager;
    private readonly ISpyOutManager _spyOutManager;

    public ObaPacketHandler(IGameLanguageService gameLanguage, ISkillsManager skillsManager, ISpyOutManager spyOutManager)
    {
        _gameLanguage = gameLanguage;
        _skillsManager = skillsManager;
        _spyOutManager = spyOutManager;
    }

    protected override async Task HandlePacketAsync(IClientSession session, ObaPacket packet)
    {
        if (!session.PlayerEntity.UseSp)
        {
            return;
        }

        if (session.PlayerEntity.Specialist == null)
        {
            return;
        }

        if (!session.PlayerEntity.IsAlive())
        {
            _spyOutManager.RemoveSpyOutSkill(session.PlayerEntity.Id);
            return;
        }

        if (!_spyOutManager.ContainsSpyOut(session.PlayerEntity.Id))
        {
            return;
        }

        (long targetId, VisualType targetType) = _spyOutManager.GetSpyOutTarget(session.PlayerEntity.Id);
        _spyOutManager.RemoveSpyOutSkill(session.PlayerEntity.Id);

        if (!session.PlayerEntity.CanPerformAttack())
        {
            return;
        }

        if (session.IsMuted())
        {
            session.SendMuteMessage();
            return;
        }

        if (session.PlayerEntity.IsOnVehicle)
        {
            return;
        }

        session.SendObArPacket();
        IBattleEntity targetEntity = session.CurrentMapInstance.GetBattleEntity(targetType, targetId);
        if (targetEntity == null)
        {
            return;
        }

        if (!targetEntity.IsAlive())
        {
            _spyOutManager.RemoveSpyOutSkill(session.PlayerEntity.Id);
            return;
        }

        if (session.CurrentMapInstance.IsPvp && targetEntity.IsInPvpZone())
        {
            return;
        }

        SkillDTO skill = _skillsManager.GetSkill((short)SkillsVnums.SPY_OUT_SKILL);
        SkillInfo skillInfo = skill.GetInfo();
        skillInfo.Vnum = -1;
        skillInfo.CastId = -1;
        session.SendEffectObject(targetEntity, false, EffectType.Sp6ArcherTargetFalcon);
        targetEntity.MapInstance.Broadcast(targetEntity.GenerateHawkPacket());
        await session.PlayerEntity.EmitEventAsync(new BattleExecuteSkillEvent(session.PlayerEntity, targetEntity, skillInfo, DateTime.UtcNow.AddSeconds(-10)));
    }
}