using System;
using PhoenixLib.Extensions;
using PhoenixLib.Logging;
using WingsAPI.Plugins;
using WingsEmu.Game._NpcDialog;

namespace NosEmu.Plugins.BasicImplementations;

public class NpcDialogPlugin : IGamePlugin
{
    private readonly IServiceProvider _container;
    private readonly INpcDialogHandlerContainer _handlers;

    public NpcDialogPlugin(INpcDialogHandlerContainer handlers, IServiceProvider container)
    {
        _handlers = handlers;
        _container = container;
    }

    public string Name => nameof(NpcDialogPlugin);

    public void OnLoad()
    {
        foreach (Type handlerType in typeof(NpcDialogPlugin).Assembly.GetTypesImplementingInterface<INpcDialogAsyncHandler>())
        {
            try
            {
                object tmp = _container.GetService(handlerType);
                if (tmp is not INpcDialogAsyncHandler real)
                {
                    continue;
                }

                Log.Debug($"[NPC_DIALOG][ADD_HANDLER] {handlerType.FullName}");
                _handlers.Register(real);
            }
            catch (Exception e)
            {
                Log.Error($"[NPC_DIALOG][FAIL_ADD] Error while adding handler {handlerType.FullName}: {e.Message}", e);
            }
        }
    }
}