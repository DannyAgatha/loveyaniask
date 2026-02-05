using System.Linq;
using Plugin.PlayerLogs.Core;
using Plugin.PlayerLogs.Messages.Act4;
using WingsEmu.Game.Act4.Event;

namespace Plugin.PlayerLogs.Enrichers.Act4
{
    public class LogAct4FamilyDungeonWonMessageEnricher : ILogMessageEnricher<Act4FamilyDungeonWonEvent, LogAct4FamilyDungeonWonMessage>
    {
        public void Enrich(LogAct4FamilyDungeonWonMessage message, Act4FamilyDungeonWonEvent e)
        {
            message.FamilyId = e.FamilyId;
            message.DungeonType = e.DungeonType;
            message.DungeonMembers = e.Members.Select(x => x.PlayerEntity.Id);
        }
    }
}