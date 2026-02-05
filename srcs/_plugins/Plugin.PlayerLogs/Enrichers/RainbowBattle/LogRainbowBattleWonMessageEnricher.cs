using Plugin.PlayerLogs.Core;
using Plugin.PlayerLogs.Messages.RainbowBattle;
using WingsEmu.Game.RainbowBattle.Event;

namespace Plugin.PlayerLogs.Enrichers.RainbowBattle
{
    public class LogRainbowBattleWonMessageEnricher : ILogMessageEnricher<RainbowBattleWonEvent, LogRainbowBattleWonMessage>
    {
        public void Enrich(LogRainbowBattleWonMessage message, RainbowBattleWonEvent e)
        {
            message.Players = e.Players;
        }
    }
}