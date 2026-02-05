using Plugin.PlayerLogs.Core;
using Plugin.PlayerLogs.Messages.LevelUp;
using WingsAPI.Game.Extensions.CharacterExtensions;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Packets.Enums;

namespace Plugin.PlayerLogs.Enrichers.LevelUp
{
    public sealed class LogLevelUpCharacterMessageEnricher : ILogMessageEnricher<LevelUpEvent, LogLevelUpCharacterMessage>
    {
        public void Enrich(LogLevelUpCharacterMessage message, LevelUpEvent e)
        {
            message.LevelType = e.LevelType.ToString();
            message.Level = e.LevelType switch
            {
                LevelType.Level => e.Sender.PlayerEntity.Level,
                LevelType.JobLevel => e.Sender.PlayerEntity.JobLevel,
                LevelType.Fairy => e.Sender.PlayerEntity.Fairy.ElementRate,
                LevelType.Heroic => e.Sender.PlayerEntity.HeroLevel,
                LevelType.SpJobLevel => e.Sender.PlayerEntity.Specialist.SpLevel
            };
            message.ItemVnum = e.ItemVnum;
            message.Location = e.Sender.GetLocation();
        }
    }
}