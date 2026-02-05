using System;
using WingsEmu.Game._enum;
using WingsEmu.Game.Entities;
using WingsEmu.Packets.Enums;

namespace WingsEmu.Game.Buffs;

public interface IBuffFactory
{
    Buff CreateBuff(int cardId, IBattleEntity caster, bool forceCreationStats = false);
    Buff CreateBuff(int cardId, IBattleEntity caster, TimeSpan duration, bool forceCreationStats = false);
    Buff CreateBuff(int cardId, IBattleEntity caster, BuffFlag buffFlags, bool forceCreationStats = false);
    Buff CreateBuff(int cardId, IBattleEntity caster, TimeSpan duration, BuffFlag buffFlag, bool forceCreationStats = false);
    Buff CreateBuff(int cardId, IBattleEntity caster, BuffGroup buffGroup, BuffFlag buffFlag, bool forceCreationStats = false);
    Buff CreateBuff(int cardId, IBattleEntity caster, BuffGroup buffGroup, BuffCategory buffCategory, bool forceCreationStats = false);
    Buff CreateBuff(int cardId, IBattleEntity caster, int level, TimeSpan duration, BuffFlag buffCategory, bool forceCreationStats = false);
    Buff CreateBuff(int cardId, IBattleEntity caster, int level, TimeSpan duration, BuffFlag buffFlags, BuffGroup buffGroup, BuffCategory buffType, bool forceCreationStats);
}