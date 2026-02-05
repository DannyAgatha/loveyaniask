using WingsEmu.DTOs.BCards;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Mates;
using WingsEmu.Packets.Enums;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Extensions;

namespace NosEmu.Plugins.BasicImplementations.BCards.Handlers
{
    public class BCardIncreaseDamageVersusHandler : IBCardEffectAsyncHandler
    {
        public BCardType HandledType => BCardType.IncreaseDamageVersus;

        public void Execute(IBCardEffectContext ctx)
        {
            IBattleEntity sender = ctx.Sender;
            IBattleEntity target = ctx.Target;
            BCardDTO bCard = ctx.BCard;

            int firstData = bCard.FirstDataValue(sender.Level);
            int secondData = bCard.SecondDataValue(sender.Level);
            
            switch (bCard.SubType)
            {
                case (byte)AdditionalTypes.IncreaseDamageVersus.IncreaseHolyIfTargetHasLessHpPercent:
                {
                    if (target is IMateEntity)
                    {
                        return;
                    }
                    
                    if (sender is not IPlayerEntity playerSender)
                    {
                        return;
                    }

                    if (target.GetHpPercentage() >= firstData)
                    {
                        return;
                    }

                    playerSender.UpdateEnergyBar(secondData).ConfigureAwait(false).GetAwaiter().GetResult();
                }
                    break;
            }
        }
    }
}