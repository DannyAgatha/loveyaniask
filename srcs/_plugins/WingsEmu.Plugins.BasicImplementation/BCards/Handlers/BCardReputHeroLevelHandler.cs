using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Entities;
using WingsEmu.Game;
using WingsEmu.Packets.Enums;
using WingsEmu.Game.Extensions;
using WingsEmu.Game._enum;
using WingsEmu.Game.Battle;

namespace NosEmu.Plugins.BasicImplementations.BCards.Handlers
{
    public class BCardReputHeroLevelHandler : IBCardEffectAsyncHandler
    {
        private readonly IRandomGenerator _random;

        public BCardReputHeroLevelHandler(IRandomGenerator random) => _random = random;

        public BCardType HandledType => BCardType.ReputHeroLevel;
        
        public void Execute(IBCardEffectContext ctx)
        {
            IBattleEntity target = ctx.Target;
            byte subType = ctx.BCard.SubType;
            int firstData = ctx.BCard.FirstData;
            int secondData = ctx.BCard.SecondData;
            
            int randomNumber = _random.RandomNumber();
            if (randomNumber > firstData)
            {
                return;
            }
            
            switch (subType)
            {
                case (byte)AdditionalTypes.ReputHeroLevel.EnemyEffectDeleteChance:
                    IReadOnlyList<Buff> buffList = target.BuffComponent.GetAllBuffs(x => x.BuffGroup == BuffGroup.Good && x.IsNormal());
                    
                    if (buffList.Count > 0)
                    {
                        Buff randomBuff = buffList[_random.RandomNumber(buffList.Count)];
                        target.RemoveBuffAsync(randomBuff.CardId).ConfigureAwait(false).GetAwaiter().GetResult();
                    }
                    break;
            }
        }
    }
}