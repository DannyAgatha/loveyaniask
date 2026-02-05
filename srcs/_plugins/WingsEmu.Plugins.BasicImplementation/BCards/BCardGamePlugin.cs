using System;
using PhoenixLib.Extensions;
using PhoenixLib.Logging;
using WingsAPI.Plugins;
using WingsEmu.Game.Buffs;

namespace NosEmu.Plugins.BasicImplementations.BCards;

public class BCardGamePlugin : IGamePlugin
{
    private readonly IServiceProvider _container;
    private readonly IBCardEffectHandlerContainer _handlers;

    public BCardGamePlugin(IBCardEffectHandlerContainer handlers, IServiceProvider container)
    {
        _handlers = handlers;
        _container = container;
    }

    public string Name => nameof(BCardGamePlugin);

    public void OnLoad()
    {
        foreach (Type handlerType in typeof(BCardGamePlugin).Assembly.GetTypesImplementingInterface<IBCardEffectAsyncHandler>())
        {
            try
            {
                object tmp = _container.GetService(handlerType);
                if (tmp is not IBCardEffectAsyncHandler real)
                {
                    continue;
                }

                Log.Debug($"[BCARD][ADD_HANDLER] {handlerType}");
                _handlers.Register(real);
            }
            catch (Exception e)
            {
                Log.Error("[BCARD][FAIL_ADD]", e);
            }
        }
    }
}