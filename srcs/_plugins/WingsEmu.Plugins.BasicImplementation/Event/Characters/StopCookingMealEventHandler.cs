using System.Collections.Generic;
using System.Linq;
using PhoenixLib.Events;
using System.Threading;
using System.Threading.Tasks;
using WingsAPI.Communication.Sessions.Model;
using WingsEmu.Game;
using WingsEmu.Game._enum;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Managers.ServerData;

namespace NosEmu.Plugins.BasicImplementations.Event.Characters
{
    public class StopCookingMealEventHandler : IAsyncEventProcessor<StopCookingMealEvent>
    {
        public async Task HandleAsync(StopCookingMealEvent e, CancellationToken cancellation)
        {
            if (e.Sender.PlayerEntity != null)
            {
                e.Sender.PlayerEntity.FirstChefAction?.Dispose();
                e.Sender.PlayerEntity.SecondChefAction?.Dispose();
                e.Sender.PlayerEntity.ThirdChefAction?.Dispose();
                e.Sender.PlayerEntity.CanCollectMeal = false;
                e.Sender.PlayerEntity.IsCraftingItem = false;

                if (!e.SendEffsPacket)
                {
                    return;
                }

                e.Sender.CurrentMapInstance.Broadcast(e.Sender.PlayerEntity.GenerateEffectS(EffectType.CookingNormalMeal, 2));
                e.Sender.SendMsCPacket(1);
            }
        }
    }
}