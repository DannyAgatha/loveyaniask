using System.Threading.Tasks;
using WingsEmu.Game._Guri.Event;
using WingsEmu.Game.Networking;

namespace WingsEmu.Game._Guri;

public interface IGuriHandlerContainer
{
    void Register(IGuriHandler handler);

    void Unregister(long guriEffectId);

    void Handle(IClientSession player, GuriEvent args);

    Task HandleAsync(IClientSession player, GuriEvent args);
}