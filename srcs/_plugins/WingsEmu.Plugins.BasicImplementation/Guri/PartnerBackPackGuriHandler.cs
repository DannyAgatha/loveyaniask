using System.Threading.Tasks;
using WingsAPI.Game.Extensions.Warehouse;
using WingsEmu.Game._Guri;
using WingsEmu.Game._Guri.Event;
using WingsEmu.Game.Networking;
using WingsEmu.Game.TimeSpaces;

namespace NosEmu.Plugins.BasicImplementations.Guri;

public class PartnerBackPackGuriHandler : IGuriHandler
{
    public long GuriEffectId => 202;

    public async Task ExecuteAsync(IClientSession session, GuriEvent guriPacket)
    {
        if (session.PlayerEntity.IsSeal || session.IsInSpecialOrHiddenTimeSpace())
        {
            return;
        }

        session.RefreshPartnerWarehouseItems();
    }
}