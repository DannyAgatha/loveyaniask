using System.Linq;
using Plugin.PlayerLogs.Core;
using Plugin.PlayerLogs.Messages.Family;
using WingsEmu.Game.Families.Event;
using WingsEmu.Packets.Enums.Families;

namespace Plugin.PlayerLogs.Enrichers.Family
{
    public sealed class LogFamilyCreatedMessageEnricher : ILogMessageEnricher<FamilyCreatedEvent, LogFamilyCreatedMessage>
    {
        public void Enrich(LogFamilyCreatedMessage message, FamilyCreatedEvent e)
        {
            message.DeputiesIds = e.Sender.PlayerEntity.Family.Members.Where(s => s.Authority == FamilyAuthority.Deputy).Select(s => s.CharacterId).ToList();
            message.FamilyName = e.Sender.PlayerEntity.Family.Name;
            message.FamilyId = e.Sender.PlayerEntity.Family.Id;
            message.Level = e.Sender.PlayerEntity.Level;
            message.HeroLevel = e.Sender.PlayerEntity.HeroLevel;
            message.Class = e.Sender.PlayerEntity.Class;
        }
    }
}