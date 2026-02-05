using System.Threading.Tasks;
using WingsEmu.Game._Guri;
using WingsEmu.Game._Guri.Event;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Networking;

namespace NosEmu.Plugins.BasicImplementations.Guri;

public class FifthSceneGuriHandler : IGuriHandler
{
    public long GuriEffectId => 44;
    public async Task ExecuteAsync(IClientSession session, GuriEvent e) => session.SendScene(44, true);
}