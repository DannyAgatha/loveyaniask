using PhoenixLib.Events;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Helpers.Damages;

namespace WingsEmu.Game.Battle;

public class ApplyHitEvent : IAsyncEvent
{
    public ApplyHitEvent(IBattleEntity target, DamageAlgorithmResult processResults, HitInformation hitInformation)
    {
        Target = target;
        ProcessResults = processResults;
        HitInformation = hitInformation;
    }

    public IBattleEntity Target { get; }
    public DamageAlgorithmResult ProcessResults { get; }
    public HitInformation HitInformation { get; }
}