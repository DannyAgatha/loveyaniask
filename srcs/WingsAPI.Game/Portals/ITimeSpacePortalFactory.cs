using WingsEmu.Game.Configurations;
using WingsEmu.Game.Helpers.Damages;

namespace WingsEmu.Game.Portals;

public interface ITimeSpacePortalFactory
{
    ITimeSpacePortalEntity CreateTimeSpacePortal(TimeSpaceFileConfiguration timeSpaceFileConfiguration, Position position);
    ITimeSpacePortalEntity CreateTimeSpacePortal(TimeSpaceFileConfiguration timeSpaceFileConfiguration, Position position, long? groupId);
}