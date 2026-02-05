using System;
using PhoenixLib.Extensions;
using PhoenixLib.Logging;
using WingsAPI.Plugins;
using WingsEmu.Game._Guri;

namespace NosEmu.Plugins.BasicImplementations;

public class GuriPlugin : IGamePlugin
{
    private readonly IServiceProvider _container;
    private readonly IGuriHandlerContainer _handlers;

    public GuriPlugin(IGuriHandlerContainer handlers, IServiceProvider container)
    {
        _handlers = handlers;
        _container = container;
    }

    public string Name => nameof(GuriPlugin);


    public void OnLoad()
    {
        foreach (Type handlerType in typeof(GuriPlugin).Assembly.GetTypesImplementingInterface<IGuriHandler>())
        {
            try
            {
                object tmp = _container.GetService(handlerType);
                if (tmp is not IGuriHandler real)
                {
                    continue;
                }

                Log.Debug($"[GURI][ADD_HANDLER] {handlerType}");
                _handlers.Register(real);
            }
            catch (Exception e)
            {
                Log.Error("[GURI][FAIL_ADD]", e);
            }
        }
    }
}