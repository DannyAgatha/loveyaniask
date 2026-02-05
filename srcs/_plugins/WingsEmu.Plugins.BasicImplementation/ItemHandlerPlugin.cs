using System;
using PhoenixLib.Extensions;
using PhoenixLib.Logging;
using WingsAPI.Plugins;
using WingsEmu.Game._ItemUsage;

namespace NosEmu.Plugins.BasicImplementations;

public class ItemHandlerPlugin : IGamePlugin
{
    private readonly IServiceProvider _container;
    private readonly IItemHandlerContainer _handlers;

    public ItemHandlerPlugin(IItemHandlerContainer handlers, IServiceProvider container)
    {
        _handlers = handlers;
        _container = container;
    }

    public string Name => nameof(ItemHandlerPlugin);


    public void OnLoad()
    {
        foreach (Type handlerType in typeof(ItemHandlerPlugin).Assembly.GetTypesImplementingInterface<IItemHandler>())
        {
            try
            {
                object tmp = _container.GetService(handlerType);
                if (tmp is not IItemHandler real)
                {
                    continue;
                }

                Log.Debug($"[ITEM_USAGE][ADD_HANDLER] {handlerType}");
                _handlers.RegisterItemHandler(real).ConfigureAwait(false).GetAwaiter().GetResult();
            }
            catch (Exception e)
            {
                Log.Error($"[ITEM_USAGE][FAIL_ADD] Error while adding handler {handlerType.FullName}: {e.Message}", e);
            }
        }
        
        foreach (Type handlerType in typeof(ItemHandlerPlugin).Assembly.GetTypesImplementingInterface<IItemUsageByVnumHandler>())
        {
            try
            {
                object tmp = _container.GetService(handlerType);
                if (tmp is not IItemUsageByVnumHandler real)
                {
                    continue;
                }

                Log.Debug($"[ITEM_USAGE][ADD_HANDLER_VNUM] {handlerType}");
                _handlers.RegisterItemHandler(real).ConfigureAwait(false).GetAwaiter().GetResult();
            }
            catch (Exception e)
            {
                string vnumInfo = handlerType.GetProperty("Vnum")?.GetValue(null)?.ToString() ?? "Unknown Vnum";
                Log.Error($"[ITEM_USAGE][FAIL_ADD_VNUM] Error while adding handler {handlerType.FullName} with Vnum {vnumInfo}: {e.Message}", e);
            }
        }
    }
}