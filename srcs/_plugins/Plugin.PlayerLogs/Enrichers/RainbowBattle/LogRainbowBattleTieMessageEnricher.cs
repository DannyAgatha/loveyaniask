using Plugin.PlayerLogs.Core;
using Plugin.PlayerLogs.Messages.RainbowBattle;
using WingsEmu.Game.RainbowBattle.Event;

namespace Plugin.PlayerLogs.Enrichers.RainbowBattle
{
    public class LogRainbowBattleTieMessageEnricher : ILogMessageEnricher<RainbowBattleTieEvent, LogRainbowBattleTieMessage>
    {
        public void Enrich(LogRainbowBattleTieMessage message, RainbowBattleTieEvent e)
        {
            message.BlueTeam = e.BlueTeam;
            message.RedTeam = e.RedTeam;
        }
    }
}