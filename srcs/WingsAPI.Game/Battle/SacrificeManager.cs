using System.Collections.Generic;
using WingsEmu.Core.Extensions;
using WingsEmu.Game.Entities;
using WingsEmu.Packets.Enums;

namespace WingsEmu.Game.Battle;

public class SacrificeManager : ISacrificeManager
{
    // Caster | Target
    private readonly Dictionary<(VisualType, long), IBattleEntity> _savedSacrificeByCaster = new();

    // Target | Caster
    private readonly Dictionary<(VisualType, long), IBattleEntity> _savedSacrificeByTarget = new();

    public void SaveSacrifice(IBattleEntity caster, IBattleEntity target)
    {
        _savedSacrificeByCaster[(caster.Type, caster.Id)] = target;
        _savedSacrificeByTarget[(target.Type, target.Id)] = caster;
    }

    public IBattleEntity GetTarget(IBattleEntity caster) => _savedSacrificeByCaster.GetOrDefault((caster.Type, caster.Id));
    public IBattleEntity GetCaster(IBattleEntity target) => _savedSacrificeByTarget.GetOrDefault((target.Type, target.Id));

    public void RemoveSacrifice(IBattleEntity caster, IBattleEntity target)
    {
        _savedSacrificeByCaster.Remove((caster.Type, caster.Id));
        _savedSacrificeByTarget.Remove((target.Type, target.Id));
    }
}