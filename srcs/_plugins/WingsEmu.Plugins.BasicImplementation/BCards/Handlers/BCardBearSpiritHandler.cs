using PhoenixLib.Events;
using WingsAPI.Packets.Enums;
using WingsEmu.Game._enum;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Extensions;
using WingsEmu.Packets.Enums;

namespace NosEmu.Plugins.BasicImplementations.BCards.Handlers;

public class BCardBearSpiritHandler : IBCardEffectAsyncHandler
{
    public BCardType HandledType => BCardType.BearSpirit;

    public void Execute(IBCardEffectContext ctx)
    {
        IBattleEntity sender = ctx.Sender;
        IBattleEntity target = ctx.Target;

        switch (ctx.BCard.SubType)
        {
            case (byte)AdditionalTypes.BearSpirit.DamageNextSkillIncreased:
                sender.BCardDataComponent.VoodooDamageStored = 0;
                break;
            
            case (byte)AdditionalTypes.BearSpirit.MerlingTransformation:

                if (!target.IsAlive())
                {
                    return;
                }

                if (target is not IPlayerEntity player)
                {
                    return;
                }

                player.BCardDataComponent.MerlingHit = 0;
                player.BCardDataComponent.OldMorph = player.Morph;
                player.BCardDataComponent.OldMorphUpgrade = player.MorphUpgrade;
                player.BCardDataComponent.OldMorphUpgrade2 = player.MorphUpgrade2;
                player.IsMorphed = true;
                player.Morph = (int)MorphType.Merling;
                player.MorphUpgrade = 0;
                player.MorphUpgrade2 = 0;
                player.Session.BroadcastCMode();
                break;
        }
    }
}