using System;
using WingsEmu.Game._enum;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Managers.StaticData;
using WingsEmu.Packets.Enums;

namespace WingsEmu.Game.Buffs;

public class BuffFactory : IBuffFactory
{
    private readonly IBuffsDurationConfiguration _buffsDurationConfiguration;
    private readonly ICardsManager _cardsManager;
    private readonly IRandomGenerator _randomGenerator;

    public BuffFactory(ICardsManager cardsManager, IRandomGenerator randomGenerator, IBuffsDurationConfiguration buffsDurationConfiguration)
    {
        _cardsManager = cardsManager;
        _randomGenerator = randomGenerator;
        _buffsDurationConfiguration = buffsDurationConfiguration;
    }

    public Buff CreateBuff(int cardId, IBattleEntity caster, bool forceCreationStats = false) => CreateBuff(cardId, caster, BuffFlag.NORMAL);

    public Buff CreateBuff(int cardId, IBattleEntity caster, TimeSpan duration, BuffFlag buffFlag, bool forceCreationStats = false)
    {
        Card card = _cardsManager.GetCardByCardId((short)cardId);
        return card == null ? null : CreateBuff(cardId, caster, caster.Level, duration, buffFlag, BuffGroup.Neutral, BuffCategory.GeneralEffect, forceCreationStats);
    }

    public Buff CreateBuff(int cardId, IBattleEntity caster, BuffGroup buffGroup, BuffFlag buffFlag, bool forceCreationStats = false)
    {
        Card card = _cardsManager.GetCardByCardId((short)cardId);
        return card == null ? null : CreateBuff(cardId, caster, caster.Level, TimeSpan.FromMilliseconds(card.Duration * 100), buffFlag, buffGroup, card.BuffCategory, forceCreationStats);
    }

    public Buff CreateBuff(int cardId, IBattleEntity caster, BuffFlag buffFlags, bool forceCreationStats = false)
    {
        Card card = _cardsManager.GetCardByCardId((short)cardId);
        return card == null
            ? null
            : CreateBuff(cardId, caster, caster.Level, TimeSpan.FromMilliseconds(card.Duration * 100), buffFlags, (BuffGroup)card.BuffType, card.BuffCategory, forceCreationStats);
    }

    public Buff CreateBuff(int cardId, IBattleEntity caster, BuffGroup buffGroup, BuffCategory buffCategory, bool forceCreationStats = false)
    {
        Card card = _cardsManager.GetCardByCardId((short)cardId);
        return card == null ? null : CreateBuff(cardId, caster, caster.Level, TimeSpan.FromMilliseconds(card.Duration * 100), BuffFlag.NORMAL, buffGroup, buffCategory, forceCreationStats);
    }

    public Buff CreateBuff(int cardId, IBattleEntity caster, int level, TimeSpan duration, BuffFlag buffCategory, bool forceCreationStats = false) =>
        CreateBuff(cardId, caster, level, duration, buffCategory, BuffGroup.Neutral, BuffCategory.GeneralEffect, forceCreationStats);

    public Buff CreateBuff(int cardId, IBattleEntity caster, TimeSpan duration, bool forceCreationStats = false)
    {
        Card card = _cardsManager.GetCardByCardId((short)cardId);
        return card == null ? null : CreateBuff(cardId, caster, caster.Level, duration, BuffFlag.NORMAL, (BuffGroup)card.BuffType, card.BuffCategory, forceCreationStats);
    }

    public Buff CreateBuff(int cardId, IBattleEntity caster, int level, TimeSpan duration, BuffFlag buffFlags, BuffGroup buffGroup, BuffCategory buffCategory, bool forceCreationStats)
    {
        Card card = _cardsManager.GetCardByCardId((short)cardId);
        if (card == null)
        {
            return null;
        }

        BuffDuration randomDuration = _buffsDurationConfiguration.GetBuffDurationById(cardId);
        if (randomDuration != null && !forceCreationStats)
        {
            duration = TimeSpan.FromMilliseconds(_randomGenerator.RandomNumber(randomDuration.MinDuration, randomDuration.MaxDuration + 1));
            buffFlags = randomDuration.IsPermanent ? BuffFlag.BIG | BuffFlag.NO_DURATION : buffFlags;
        }

        return new Buff(
            Guid.NewGuid(),
            card.Id,
            card.Level,
            duration,
            card.EffectId,
            card.Name,
            card.TimeoutBuff,
            buffGroup,
            card.TimeoutBuffChance,
            card.SecondBCardsDelay,
            card.GroupId,
            buffCategory,
            DateTime.UtcNow,
            buffFlags,
            card.BCards,
            card.IsConstEffect,
            (ElementType)card.ElementType,
            level,
            caster,
            true
        );
    }

    public Buff CreateBuff(int cardId, int level, TimeSpan duration, BuffFlag buffCategory, bool forceCreationStats = false) =>
        CreateBuff(cardId, null, level, duration, buffCategory, BuffGroup.Neutral, BuffCategory.GeneralEffect, forceCreationStats);
}