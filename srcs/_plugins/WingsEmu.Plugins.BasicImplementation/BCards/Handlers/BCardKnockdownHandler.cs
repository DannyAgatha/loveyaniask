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

public class BCardKnockdownHandler : IBCardEffectAsyncHandler
{
    private readonly IBuffFactory _buffFactory;
    private readonly ICardsManager _cards;
    private readonly IGameLanguageService _gameLanguage;
    private readonly IRandomGenerator _randomGenerator;

    public BCardKnockdownHandler(IRandomGenerator randomGenerator, IBuffFactory buffFactory, ICardsManager cards, IGameLanguageService gameLanguage)
    {
        _randomGenerator = randomGenerator;
        _buffFactory = buffFactory;
        _cards = cards;
        _gameLanguage = gameLanguage;
    }

    public BCardType HandledType => BCardType.TauntSkill;

    public void Execute(IBCardEffectContext ctx)
    {
        IBattleEntity sender = ctx.Sender;
        IBattleEntity target = ctx.Target;
        byte subType = ctx.BCard.SubType;
        int firstData = ctx.BCard.FirstData;
        int secondData = ctx.BCard.SecondData;

        if (_randomGenerator.RandomNumber() > firstData)
        {
            return;
        }

        switch (subType)
        {
            case (byte)AdditionalTypes.TauntSkill.TauntWhenKnockdown:
                if (!target.BuffComponent.HasBuff((short)BuffVnums.KNOCKDOWN))
                {
                    return;
                }

                Buff b = _buffFactory.CreateBuff(secondData, target);

                double debuffCounter = target.CheckForResistance(b, _cards, out double buffCounter, out double specializedResistance);

                int debuffRandomNumber = _randomGenerator.RandomNumber();
                int buffRandomNumber = _randomGenerator.RandomNumber();
                int specializedRandomNumber = _randomGenerator.RandomNumber();
                switch (target)
                {
                    case IMonsterEntity monster:
                        if (!monster.CanBeDebuffed)
                        {
                            return;
                        }

                        break;
                }

                if (specializedRandomNumber >= (int)(specializedResistance * 100))
                {
                    if (!(target is IPlayerEntity c))
                    {
                        return;
                    }

                    string message = _gameLanguage.GetLanguage(GameDialogKey.BUFF_CHATMESSAGE_EFFECT_RESISTANCE, c.Session.UserLanguage);
                    c.Session.SendChatMessage(message, ChatMessageColorType.Buff);
                    return;
                }

                switch (b.BuffGroup)
                {
                    case BuffGroup.Bad when debuffRandomNumber >= (int)(debuffCounter * 100):
                        {
                            if (!(target is IPlayerEntity c))
                            {
                                return;
                            }

                            string message = _gameLanguage.GetLanguage(GameDialogKey.BUFF_CHATMESSAGE_EFFECT_RESISTANCE, c.Session.UserLanguage);
                            c.Session.SendChatMessage(message, ChatMessageColorType.Buff);
                            return;
                        }
                    case BuffGroup.Good when buffRandomNumber >= (int)(buffCounter * 100):
                        {
                            if (!(target is IPlayerEntity c))
                            {
                                return;
                            }

                            string message = _gameLanguage.GetLanguage(GameDialogKey.BUFF_CHATMESSAGE_EFFECT_RESISTANCE, c.Session.UserLanguage);
                            c.Session.SendChatMessage(message, ChatMessageColorType.Buff);
                            return;
                        }
                }

                target.AddBuffAsync(b).ConfigureAwait(false).GetAwaiter().GetResult();
                break;

            case (byte)AdditionalTypes.TauntSkill.TauntWhenNormal:
                if (target.BuffComponent.HasBuff((short)BuffVnums.KNOCKDOWN))
                {
                    return;
                }

                b = _buffFactory.CreateBuff(secondData, target);

                debuffCounter = target.CheckForResistance(b, _cards, out buffCounter, out specializedResistance);
                debuffRandomNumber = _randomGenerator.RandomNumber();
                buffRandomNumber = _randomGenerator.RandomNumber();
                specializedRandomNumber = _randomGenerator.RandomNumber();
                switch (target)
                {
                    case IMonsterEntity monster:
                        if (!monster.CanBeDebuffed)
                        {
                            return;
                        }

                        break;
                }

                if (specializedRandomNumber >= (int)(specializedResistance * 100))
                {
                    if (!(target is IPlayerEntity c))
                    {
                        return;
                    }

                    string message = _gameLanguage.GetLanguage(GameDialogKey.BUFF_CHATMESSAGE_EFFECT_RESISTANCE, c.Session.UserLanguage);
                    c.Session.SendChatMessage(message, ChatMessageColorType.Buff);
                    return;
                }

                switch (b.BuffGroup)
                {
                    case BuffGroup.Bad when debuffRandomNumber >= (int)(debuffCounter * 100):
                        {
                            if (!(target is IPlayerEntity c))
                            {
                                return;
                            }

                            string message = _gameLanguage.GetLanguage(GameDialogKey.BUFF_CHATMESSAGE_EFFECT_RESISTANCE, c.Session.UserLanguage);
                            c.Session.SendChatMessage(message, ChatMessageColorType.Buff);
                            return;
                        }
                    case BuffGroup.Good when buffRandomNumber >= (int)(buffCounter * 100):
                        {
                            if (!(target is IPlayerEntity c))
                            {
                                return;
                            }

                            string message = _gameLanguage.GetLanguage(GameDialogKey.BUFF_CHATMESSAGE_EFFECT_RESISTANCE, c.Session.UserLanguage);
                            c.Session.SendChatMessage(message, ChatMessageColorType.Buff);
                            return;
                        }
                }

                target.AddBuffAsync(b).ConfigureAwait(false).GetAwaiter().GetResult();
                break;
        }
    }
}