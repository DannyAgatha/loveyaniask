using Plugin.PlayerLogs.Core;
using Plugin.PlayerLogs.Messages.LevelUp;
using WingsEmu.Game.Characters.Events;

namespace Plugin.PlayerLogs.Enrichers.LevelUp
{
    public class LogLevelUpNosMateMessageEnricher : ILogMessageEnricher<LevelUpMateEvent, LogLevelUpNosMateMessage>
    {
        public void Enrich(LogLevelUpNosMateMessage message, LevelUpMateEvent e)
        {
            message.Level = e.Level;
            message.Location = e.Location;
            message.LevelUpType = e.LevelUpType.ToString();
            message.ItemVnum = e.ItemVnum;
            message.NosMateMonsterVnum = e.NosMateMonsterVnum;
        }
    }
}