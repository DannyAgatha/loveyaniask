using WingsEmu.Game.Configurations;
using WingsEmu.Game.Helpers.Damages;
using WingsEmu.Game.Portals;

namespace Plugin.CoreImpl.Entities
{
    public class TimeSpacePortalFactory : ITimeSpacePortalFactory
    {
        public ITimeSpacePortalEntity CreateTimeSpacePortal(TimeSpaceFileConfiguration timeSpaceFileConfiguration, Position position) =>
            CreateTimeSpacePortal(timeSpaceFileConfiguration, position, null);

        public ITimeSpacePortalEntity CreateTimeSpacePortal(TimeSpaceFileConfiguration timeSpaceFileConfiguration, Position position, long? groupId) =>
            new TimeSpacePortalEntity(timeSpaceFileConfiguration, position, groupId);
    }
}