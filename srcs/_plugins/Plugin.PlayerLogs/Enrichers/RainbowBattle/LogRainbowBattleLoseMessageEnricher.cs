using Plugin.PlayerLogs.Core;
using Plugin.PlayerLogs.Messages.RainbowBattle;
using WingsEmu.Game.RainbowBattle.Event;

namespace Plugin.PlayerLogs.Enrichers.RainbowBattle
{
    public class LogRainbowBattleLoseMessageEnricher : ILogMessageEnricher<RainbowBattleLoseEvent, LogRainbowBattleLoseMessage>
    {
        public void Enrich(LogRainbowBattleLoseMessage message, RainbowBattleLoseEvent e)
        {
            message.RainbowBattleId = e.Id;
            message.Players = e.Players;
        }
    }
}