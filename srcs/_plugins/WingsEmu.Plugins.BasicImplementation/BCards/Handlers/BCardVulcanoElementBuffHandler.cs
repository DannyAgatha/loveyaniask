using WingsEmu.DTOs.BCards;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Entities;
using WingsEmu.Packets.Enums;

namespace NosEmu.Plugins.BasicImplementations.BCards.Handlers;

public class BCardVulcanoElementBuffHandler : IBCardEffectAsyncHandler
{
    public BCardType HandledType => BCardType.VulcanoElementBuff;
    
    public void Execute(IBCardEffectContext ctx)
    {
        IBattleEntity sender = ctx.Sender;
        BCardDTO bCard = ctx.BCard;
        int firstData = ctx.BCard.FirstDataValue(sender.Level);
        int secondData = ctx.BCard.SecondDataValue(sender.Level);

        switch (bCard.SubType)
        {
            case (byte)AdditionalTypes.VulcanoElementBuff.CriticalDefenceTimes:
            {
                if (sender is not IPlayerEntity player)
                {
                    return;
                }

                player.BCardStackComponent.AddStackBCard(((short)bCard.Type, bCard.SubType), secondData);
            }
                break;
        }
    }
}