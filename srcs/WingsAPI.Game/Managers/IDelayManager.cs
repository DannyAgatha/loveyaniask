using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WingsEmu.Core.Extensions;
using WingsEmu.Game.Entities;

namespace WingsEmu.Game.Managers;

public interface IDelayConfiguration
{
    TimeSpan GetDelayByAction(DelayedActionType type);
}

public class DelayConfiguration : IDelayConfiguration
{
    private static readonly TimeSpan Default = TimeSpan.FromSeconds(2);

    private readonly Dictionary<DelayedActionType, TimeSpan> _times = new()
    {
        [DelayedActionType.SummonPet] = Default,
        [DelayedActionType.KickPet] = Default,
        [DelayedActionType.EquipVehicle] = Default,
        [DelayedActionType.IcebreakerUnfreeze] = Default,
        [DelayedActionType.PartnerWearSp] = Default,
        [DelayedActionType.PartnerLearnSkill] = Default,
        [DelayedActionType.WearSp] = Default,
        [DelayedActionType.ReturnWing] = Default,
        [DelayedActionType.ReturnAmulet] = Default,
        [DelayedActionType.MinilandBell] = Default,
        [DelayedActionType.BaseTeleporter] = Default,
        [DelayedActionType.LodScroll] = Default,
        [DelayedActionType.PartnerResetSkill] = Default,
        [DelayedActionType.PartnerResetAllSkills] = Default,
        [DelayedActionType.WingOfFriendship] = Default,
        [DelayedActionType.ButtonSwitch] = Default,
        [DelayedActionType.Mining] = Default,
        [DelayedActionType.SealedVessel] = Default,
        [DelayedActionType.RainbowBattleCaptureFlag] = Default,
        [DelayedActionType.RainbowBattleUnfreeze] = Default,
        [DelayedActionType.ChangeSpecialistSlot] = Default
    };

    public TimeSpan GetDelayByAction(DelayedActionType type) => _times.GetOrDefault(type, Default);
}

public interface IDelayManager
{
    ValueTask<DateTime> RegisterAction(IBattleEntity entity, DelayedActionType action, TimeSpan time = default);
    ValueTask<bool> CanPerformAction(IBattleEntity entity, DelayedActionType type);
    ValueTask<bool> CompleteAction(IBattleEntity entity, DelayedActionType action);
}

public enum DelayedActionType
{
    KickPet,
    SummonPet,
    EquipVehicle,
    WearSp,
    ReturnWing,
    ReturnAmulet,
    MinilandBell,
    LodScroll,
    ReturnScroll,
    MorphScroll,
    UseTeleporter,
    BaseTeleporter,
    PartnerWearSp,
    PartnerLearnSkill,
    PartnerResetSkill,
    PartnerResetAllSkills,
    IcebreakerUnfreeze,
    WingOfFriendship,
    ButtonSwitch,
    Mining,
    SealedVessel,
    RainbowBattleCaptureFlag,
    RainbowBattleUnfreeze,
    ChangeSpecialistSlot
}