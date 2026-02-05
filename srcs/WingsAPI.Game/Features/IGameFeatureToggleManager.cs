using System.Threading.Tasks;

namespace WingsEmu.Game.Features;

public interface IGameFeatureToggleManager
{
    Task<bool> IsDisabled(GameFeature serviceName);
    Task Disable(GameFeature serviceName);
    Task Enable(GameFeature serviceName);
}