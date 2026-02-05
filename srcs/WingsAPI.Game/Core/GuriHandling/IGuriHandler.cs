using System.Threading.Tasks;
using WingsEmu.Game._Guri.Event;
using WingsEmu.Game.Networking;

namespace WingsEmu.Game._Guri;

public interface IGuriHandler
{
    long GuriEffectId { get; }

    Task ExecuteAsync(IClientSession session, GuriEvent e);
}