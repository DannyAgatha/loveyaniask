using System.Threading.Tasks;
using PhoenixLib.Logging;
using WingsEmu.DTOs.Bonus;
using WingsEmu.Game._Guri;
using WingsEmu.Game._Guri.Event;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Warehouse;
using WingsEmu.Game.Warehouse.Events;

namespace NosEmu.Plugins.BasicImplementations.Guri;

public class PetBasketGuriHandler : IGuriHandler
{
    private readonly IAccountWarehouseManager _accountWarehouseManager;

    public PetBasketGuriHandler(IAccountWarehouseManager accountWarehouseManager) => _accountWarehouseManager = accountWarehouseManager;

    public long GuriEffectId => 201;

    public async Task ExecuteAsync(IClientSession session, GuriEvent guriPacket)
    {
        if (!session.PlayerEntity.HaveStaticBonus(StaticBonusType.PetBasket))
        {
            Log.Debug($"No static bonus for PetBasket on CharacterId : {session.PlayerEntity.Id}. GuriEffectId : {GuriEffectId}");
            return;
        }

        await session.EmitEventAsync(new AccountWarehouseOpenEvent());
    }
}