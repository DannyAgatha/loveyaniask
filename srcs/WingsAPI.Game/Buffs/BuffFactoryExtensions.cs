using System;
using WingsEmu.Game._enum;
using WingsEmu.Game.Entities;

namespace WingsEmu.Game.Buffs;

public static class BuffFactoryExtensions
{
    public static Buff CreateOneHourBuff(this IBuffFactory factory, IBattleEntity caster, int cardId, BuffFlag buffFlags)
        => factory.CreateBuff(cardId, caster, TimeSpan.FromHours(1), buffFlags);
}