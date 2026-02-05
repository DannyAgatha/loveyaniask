// NosEmu
// 


using System.Collections.Generic;
using WingsEmu.DTOs.BCards;
using WingsEmu.Game;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Helpers.Damages;
using WingsEmu.Game.Managers.StaticData;
using WingsEmu.Game.Mates;
using WingsEmu.Packets.Enums;

namespace NosEmu.Plugins.BasicImplementations.BCards.Handlers;

public class BCardEffectSummonHandler : IBCardEffectAsyncHandler
{
    private readonly IBuffFactory _buffFactory;
    private readonly ICardsManager _cards;
    private readonly IGameLanguageService _gameLanguage;
    private readonly IRandomGenerator _randomGenerator;

    public BCardEffectSummonHandler(IRandomGenerator randomGenerator, IBuffFactory buffFactory, IGameLanguageService gameLanguage, ICardsManager cards)
    {
        _randomGenerator = randomGenerator;
        _buffFactory = buffFactory;
        _gameLanguage = gameLanguage;
        _cards = cards;
    }

    public BCardType HandledType => BCardType.EffectSummon;

    public void Execute(IBCardEffectContext ctx)
    {
        IBattleEntity sender = ctx.Sender;
        IBattleEntity target = ctx.Target;
        BCardDTO bCard = ctx.BCard;

        switch (bCard.SubType)
        {
            case (byte)AdditionalTypes.EffectSummon.TeamEffectAppliedChance:
                int randomNumber = _randomGenerator.RandomNumber();

                if (randomNumber > ctx.BCard.FirstData)
                {
                    return;
                }
                Buff buff = _buffFactory.CreateBuff(ctx.BCard.SecondData, sender);
                
                if (sender is IMonsterEntity monster && monster.MonsterVNum == 2349)
                {
                    IEnumerable<IBattleEntity> allyInRange = monster.GetAlliesInRange(monster, 5);
                    foreach (IBattleEntity ally in allyInRange)
                    {
                        ally.AddBuffAsync(buff).ConfigureAwait(false).GetAwaiter().GetResult();
                    }
                    return;
                }

                if (target is not IPlayerEntity playerEntity)
                {
                    return;
                }

                foreach (IPlayerEntity member in playerEntity.GetGroup().Members)
                {
                    if (member.Position.GetDistance(playerEntity.Position) <= 5)
                    {
                        member.AddBuffAsync(buff).ConfigureAwait(false).GetAwaiter().GetResult();
                    }

                    foreach (IMateEntity mate in member.MateComponent.GetMates(x => x.IsTeamMember))
                    {
                        if (mate.Position.GetDistance(member.Position) <= 5)
                        {
                            mate.AddBuffAsync(buff).ConfigureAwait(false).GetAwaiter().GetResult();
                        }
                    }
                }
                playerEntity.AddBuffAsync(buff).ConfigureAwait(false).GetAwaiter().GetResult();
                break;
            
            case (byte)AdditionalTypes.EffectSummon.BlockNegativeEffect:
            {
                sender.BCardDataComponent.BlockBadBuff = 0;
            }
                break;
        }
    }
}