using System.Collections.Generic;
using System.Linq;
using WingsEmu.Game;
using WingsEmu.Game._enum;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Managers.StaticData;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;

namespace NosEmu.Plugins.BasicImplementations.BCards.Handlers;

public class BCardFocusEnemyAttentionSkillHandler : IBCardEffectAsyncHandler
{
    private readonly IBuffFactory _buffFactory;
    private readonly ICardsManager _cardsManager;
    private readonly IGameLanguageService _gameLanguage;
    private readonly IRandomGenerator _randomGenerator;

    public BCardFocusEnemyAttentionSkillHandler(IRandomGenerator randomGenerator, ICardsManager cardsManager, IBuffFactory buffFactory, IGameLanguageService gameLanguage)
    {
        _randomGenerator = randomGenerator;
        _cardsManager = cardsManager;
        _buffFactory = buffFactory;
        _gameLanguage = gameLanguage;
    }

    public BCardType HandledType => BCardType.FocusEnemyAttentionSkill;

    public void Execute(IBCardEffectContext ctx)
    {
        IBattleEntity sender = ctx.Sender;
        IBattleEntity target = ctx.Target;
        SkillInfo skillInfo = ctx.Skill;
        int firstData = ctx.BCard.FirstDataValue(sender.Level);
        int secondData = ctx.BCard.SecondDataValue(sender.Level);

        switch (ctx.BCard.SubType)
        {
            case (byte)AdditionalTypes.FocusEnemyAttentionSkill.AttractEnemyAttention:
                IEnumerable<IBattleEntity> enemiesInRange = sender.GetEnemiesInRange(sender, 5).ToList();
                foreach (IBattleEntity enemy in enemiesInRange)
                {
                    if (enemy is not IMonsterEntity monster)
                    {
                        continue;
                    }

                    if (monster.Targets.Contains(sender))
                    {
                        continue;
                    }

                    enemy.MapInstance.AddEntityToTargets(monster, sender);
                    monster.BroadcastEffectInRange(EffectType.Targeted);
                }

                break;
        }
    }
}