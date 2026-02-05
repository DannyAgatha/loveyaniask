using Plugin.PlayerLogs.Core;
using Plugin.PlayerLogs.Messages.HeroicLevelUp;
using WingsEmu.Game.Characters.Events;

namespace Plugin.PlayerLogs.Enrichers.HeroicLevelUp
{
    public class LogHeroicLevelUpNosMateMessageEnricher : ILogMessageEnricher<HeroicLevelUpMateEvent, LogHeroicLevelUpNosMateMessage>
    {
        public void Enrich(LogHeroicLevelUpNosMateMessage message, HeroicLevelUpMateEvent e)
        {
            message.HeroLevel = e.HeroLevel;
            message.Location = e.Location;
            message.LevelUpType = e.LevelUpType.ToString();
            message.ItemVnum = e.ItemVnum;
            message.NosMateMonsterVnum = e.NosMateMonsterVnum;
        }
    }
}