using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.Game._enum;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Extensions;
using WingsEmu.Packets.Enums;
using System;
using WingsEmu.Game;
using PhoenixLib.Scheduler;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Managers.StaticData;

namespace NosEmu.Plugins.BasicImplementations.Event.Characters
{
    public class CastFishingLineEventHandler : IAsyncEventProcessor<CastFishingLineEvent>
    {
        private readonly IScheduler _scheduler;
        private readonly IRandomGenerator _randomGenerator;
        private readonly ICardsManager _cardsManager;

        public CastFishingLineEventHandler(IScheduler scheduler, IRandomGenerator randomGenerator, ICardsManager cardsManager)
        {
            _scheduler = scheduler;
            _randomGenerator = randomGenerator;
            _cardsManager = cardsManager;
        }

        public async Task HandleAsync(CastFishingLineEvent e, CancellationToken cancellation)
        {
            IPlayerEntity character = e.Sender.PlayerEntity;
            Card buff = _cardsManager.GetCardByCardId((int)BuffVnums.FISH_LINE);
            int duration = buff.Duration / 10;
            int value = _randomGenerator.RandomNumber(1, duration - 5);
            int value2 = _randomGenerator.RandomNumber(0, 100);
            int percentToGetRare = 5;

            double multiplier = 1 +
                (character.BCardComponent.GetAllBCardsInformation(BCardType.IncreaseElementFairy,
                        (byte)AdditionalTypes.IncreaseElementFairy.RareFishIncreaseChance, character.Level).firstData * 0.01 -
                    character.BCardComponent.GetAllBCardsInformation(BCardType.IncreaseElementFairy,
                        (byte)AdditionalTypes.IncreaseElementFairy.RareFishDecreaseChance, character.Level).firstData * 0.01);
            
            character?.FishingFirstFish?.Dispose();
            character?.FishingSecondFish?.Dispose();

            percentToGetRare = (int)(percentToGetRare * multiplier);
            
            _scheduler.Schedule(TimeSpan.FromSeconds(value), () =>
            {
                if (!character.HasBuff(BuffVnums.FISH_LINE))
                {
                    return;
                }
                
                character.CanCollectFish = true;
                
                if (value2 < percentToGetRare)
                {
                    character.IsRareFish = true;
                }
                
                character.Session.CurrentMapInstance.Broadcast(character.Session.GenerateGuriPacket(6, 1, character.Id, character.IsRareFish ? 31 : 30));
                
                _scheduler.Schedule(TimeSpan.FromSeconds(5), () =>
                {

                    if (!character.HasBuff(BuffVnums.FISH_LINE))
                    {
                        return;
                    }
                    
                    character.CanCollectFish = false;
                    character.IsRareFish = false;
                });
            });
        }
    }
}