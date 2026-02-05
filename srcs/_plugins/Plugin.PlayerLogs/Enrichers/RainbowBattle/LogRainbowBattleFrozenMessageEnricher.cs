using Plugin.PlayerLogs.Core;
using Plugin.PlayerLogs.Messages.RainbowBattle;
using WingsEmu.Game.RainbowBattle.Event;

namespace Plugin.PlayerLogs.Enrichers.RainbowBattle
{
    public class LogRainbowBattleFrozenMessageEnricher : ILogMessageEnricher<RainbowBattleFrozenEvent, LogRainbowBattleFrozenMessage>
    {
        public void Enrich(LogRainbowBattleFrozenMessage message, RainbowBattleFrozenEvent e)
        {
            message.RainbowBattleId = e.Id;
            message.Killer = e.Killer;
            message.Killed = e.Killed;
        }
    }
}