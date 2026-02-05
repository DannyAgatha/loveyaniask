using WingsEmu.Game._i18n;

namespace WingsEmu.Plugins.DistributedGameEvents.BotMessages
{
    public class ScheduledBotMessageConfiguration : SchedulableConfiguration
    {
        public GameDialogKey Message { get; set; }
    }
}