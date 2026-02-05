using Plugin.PlayerLogs.Core;
using Plugin.PlayerLogs.Messages.RainbowBattle;
using WingsEmu.Game.RainbowBattle.Event;

namespace Plugin.PlayerLogs.Enrichers.RainbowBattle
{
    public class LogRainbowBattleJoinMessageEnricher : ILogMessageEnricher<RainbowBattleJoinEvent, LogRainbowBattleJoinMessage>
    {
        public void Enrich(LogRainbowBattleJoinMessage message, RainbowBattleJoinEvent e)
        {
        }
    }
}