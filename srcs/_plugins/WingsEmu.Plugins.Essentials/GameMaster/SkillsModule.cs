using System;
using System.Threading.Tasks;
using PhoenixLib.Events;
using Qmmands;
using WingsEmu.Commands.Checks;
using WingsEmu.Commands.Entities;
using WingsEmu.DTOs.Account;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Extensions.Mates;
using WingsEmu.Game.Helpers.Damages;
using WingsEmu.Game.Managers.StaticData;
using WingsEmu.Game.Mates;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Skills;
using WingsEmu.Packets.Enums;

namespace WingsEmu.Plugins.Essentials.GameMaster;

[Name("Super Game Master")]
[Description("Module related to skills Super Game Master commands.")]
[RequireAuthority(AuthorityType.SGM)]
public class SkillsModule : SaltyModuleBase
{
    private readonly IAsyncEventPipeline _eventPipeline;
    private readonly ISkillsManager _skillsManager;

    public SkillsModule(IAsyncEventPipeline eventPipeline, ISkillsManager skillsManager)
    {
        _eventPipeline = eventPipeline;
        _skillsManager = skillsManager;
    }

    [Command("spcd")]
    [Description("Remove SP cooldown.")]
    public async Task<SaltyCommandResult> SpcdAsync()
    {
        IClientSession session = Context.Player;

        session.PlayerEntity.SpCooldownEnd = DateTime.UtcNow;
        session.ResetSpCooldownUi();
        return new SaltyCommandResult(true, "Specialist Cooldown transform has been removed.");
    }

    [Command("kill")]
    [Description("Kill yourself.")]
    public async Task<SaltyCommandResult> Kill() => AnonymousKill(Context.Player.PlayerEntity);

    private SaltyCommandResult AnonymousKill(IBattleEntity suicidalEntity)
    {
        if (suicidalEntity == default)
        {
            return new SaltyCommandResult(false, "The target provided is not valid");
        }

        var algorithmResult = new DamageAlgorithmResult(int.MaxValue, HitType.Critical, true, false);
        _eventPipeline.ProcessEventAsync(new ApplyHitEvent(suicidalEntity, algorithmResult, new HitInformation(suicidalEntity, _skillsManager.GetSkill(299).GetInfo())));

        return new SaltyCommandResult(true);
    }

    [Command("skillreset", "resetcd", "resetCooldown", "sr")]
    [Description("Resets all the skills cooldown")]
    public async Task<SaltyCommandResult> ResetCooldown()
    {
        Context.Player.PlayerEntity.ClearSkillCooldowns();

        foreach (IBattleEntitySkill skill in Context.Player.PlayerEntity.Skills)
        {
            skill.LastUse = DateTime.MinValue;
            Context.Player.SendSkillCooldownReset(skill.Skill.CastId);
        }

        return new SaltyCommandResult(true, "All skills cooldown have been reset.");
    }

    [Command("partnercd")]
    [Description("Remove Partner SP cooldown.")]
    public async Task<SaltyCommandResult> PartnerSpcdAsync()
    {
        IClientSession session = Context.Player;
        IMateEntity mateEntity = session.PlayerEntity.MateComponent.GetMate(m => m.IsTeamMember && m.MateType == MateType.Partner);

        if (mateEntity == null)
        {
            return new SaltyCommandResult(false);
        }

        mateEntity.SpCooldownEnd = null;
        session.SendMateSpCooldown(mateEntity, 0);
        return new SaltyCommandResult(true, "Partner Specialist Cooldown has been removed.");
    }
}