using PhoenixLib.Events;
using System.Collections.Generic;
using System.Numerics;
using WingsEmu.DTOs.BCards;
using WingsEmu.Game;
using WingsEmu.Game._enum;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Helpers.Damages;
using WingsEmu.Game.Mates;
using WingsEmu.Game.Mates.Events;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Battle;

namespace NosEmu.Plugins.BasicImplementations.BCards.Handlers;

public class BCardVoodooPriestHandler : IBCardEffectAsyncHandler
{
    private readonly IAsyncEventPipeline _asyncEvent;
    private readonly IRandomGenerator _randomGenerator;
    private readonly IBuffFactory _buffFactory;

    public BCardVoodooPriestHandler(IRandomGenerator randomGenerator, IAsyncEventPipeline asyncEvent, IBuffFactory buffFactory)
    {
        _randomGenerator = randomGenerator;
        _asyncEvent = asyncEvent;
        _buffFactory = buffFactory;
    }

    public BCardType HandledType => BCardType.VoodooPriest;

    public void Execute(IBCardEffectContext ctx)
    {
        IBattleEntity sender = ctx.Sender;
        IBattleEntity target = ctx.Target;
        BCardDTO bCard = ctx.BCard;
        int firstData = ctx.BCard.FirstDataValue(sender.Level);
        int secondData = ctx.BCard.SecondDataValue(sender.Level);
        int damage = ctx.DamageDealt;

        switch (bCard.SubType)
        {
            case (byte)AdditionalTypes.VoodooPriest.ChanceTransformInto:
            {
                if (sender is not IPlayerEntity player)
                {
                    return;
                }

                if (!sender.IsSucceededChance(firstData))
                {
                    return;
                }
                

                switch (player.Morph)
                {
                    default:
                        player.Morph = (int)MorphType.FlameDruidLeopardStance;
                        player.IsFlameDruidTransformed = true;
                        player.Session.BroadcastCMode();
                        player.Session.BroadcastEffect(EffectType.Transform);
                        player.Session.SendCancelPacket(CancelType.InCombatMode);
                        break;
                }
            }
                break;

            case (byte)AdditionalTypes.VoodooPriest.TakeDamageXTimesBuffDisappear:
            {
                if (sender is not IPlayerEntity character)
                {
                    return;
                }

                character.BCardDataComponent.MerlingHit++;

                if (character.BCardDataComponent.MerlingHit >= firstData)
                {
                    Buff buff = character.BuffComponent.GetBuff(secondData);
                    character.RemoveBuffAsync(false, buff).ConfigureAwait(false).GetAwaiter().GetResult();
                }
            }
                break;
        }
    }
}