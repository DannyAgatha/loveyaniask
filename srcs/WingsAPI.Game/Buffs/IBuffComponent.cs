using System;
using System.Collections.Generic;
using WingsEmu.Game._enum;

namespace WingsEmu.Game.Buffs;

public interface IBuffComponent
{
    bool HasAnyBuff();
    IReadOnlyList<Buff> GetAllBuffs();
    ICollection<int> GetAllBuffsId();
    IReadOnlyList<Buff> GetAllBuffs(Func<Buff, bool> predicate);
    void AddBuff(Buff buff);
    Buff GetBuff(int cardId);
    Buff GetBuffByGroupId(int groupId);
    bool HasBuff(BuffGroup buffType);
    bool HasBuff(int cardId);
    bool HasBuff(Guid buffId);
    void RemoveBuff(Guid buffId);
    void ClearNonPersistentBuffs();
}