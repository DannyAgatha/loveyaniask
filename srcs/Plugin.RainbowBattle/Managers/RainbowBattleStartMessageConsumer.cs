using System;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.ServiceBus;
using WingsAPI.Communication.RainbowBattle;
using WingsEmu.Game.RainbowBattle;

namespace Plugin.RainbowBattle.Managers
{
    public class RainbowBattleStartMessageConsumer : IMessageConsumer<RainbowBattleStartMessage>
    {
        private readonly IRainbowBattleManager _rainbowBattleManager;

        public RainbowBattleStartMessageConsumer(IRainbowBattleManager rainbowBattleManager) => _rainbowBattleManager = rainbowBattleManager;

        public async Task HandleAsync(RainbowBattleStartMessage notification, CancellationToken token)
        {
            _rainbowBattleManager.RainbowBattleProcessTime = DateTime.UtcNow;
        }
    }
}