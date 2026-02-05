using System.Collections.Generic;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Helpers.Damages;

namespace WingsEmu.Game.Battle;

public class HitRequest
{
    public HitRequest(IEnumerable<(IBattleEntity, DamageAlgorithmResult)> algorithmResult, HitInformation eHitInformation, IBattleEntity mainTarget)
    {
        Targets = algorithmResult;
        EHitInformation = eHitInformation;
        MainTarget = mainTarget;
    }

    public IBattleEntity MainTarget { get; }
    public IEnumerable<(IBattleEntity target, DamageAlgorithmResult result)> Targets { get; }
    public HitInformation EHitInformation { get; }
}