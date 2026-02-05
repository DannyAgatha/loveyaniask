using WingsEmu.Game.Characters;
using WingsEmu.Game.Warehouse;

namespace NosEmu.Plugins.BasicImplementations.Warehouse;

public interface IWarehouseFactory
{
    IWarehouse Create(IPlayerEntity entity);
}