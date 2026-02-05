using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using PhoenixLib.Events;
using WingsEmu.Game.Warehouse;
using WingsEmu.Game.Warehouse.Events;

namespace NosEmu.Plugins.BasicImplementations.Warehouse;

public static class WarehouseExtensions
{
    public static void AddWarehouseModule(this IServiceCollection services)
    {
        services.TryAddSingleton<IAccountWarehouseManager, AccountWarehouseManager>();
        services.TryAddTransient<IWarehouseFactory, WarehouseFactory>();
        services.TryAddTransient<IAsyncEventProcessor<AccountWarehouseAddItemEvent>, AccountWarehouseAddEventHandler>();
        services.TryAddTransient<IAsyncEventProcessor<AccountWarehouseWithdrawItemEvent>, AccountWarehouseWithdrawEventHandler>();
    }
}