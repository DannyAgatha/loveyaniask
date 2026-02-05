using System.Threading.Tasks;
using WingsEmu.Game._Guri;
using WingsEmu.Game._Guri.Event;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Networking;

namespace NosEmu.Plugins.BasicImplementations.Guri;

public class FourthSceneGuriHandler : IGuriHandler
{
    public long GuriEffectId => 43;
    public async Task ExecuteAsync(IClientSession session, GuriEvent e) => session.SendScene(43, true);
}