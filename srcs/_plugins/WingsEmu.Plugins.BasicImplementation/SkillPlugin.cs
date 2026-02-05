using PhoenixLib.Extensions;
using PhoenixLib.Logging;
using System;
using WingsAPI.Plugins;
using WingsEmu.Game.Core.SkillHandling;

namespace NosEmu.Plugins.BasicImplementations;

public class SkillPlugin : IGamePlugin
{
    private readonly IServiceProvider _container;
    private readonly ISkillHandlerContainer _handlers;

    public SkillPlugin(ISkillHandlerContainer handlers, IServiceProvider container)
    {
        _handlers = handlers;
        _container = container;
    }

    public string Name => nameof(SkillPlugin);

    public void OnLoad()
    {
        foreach (Type handlerType in typeof(GuriPlugin).Assembly.GetTypesImplementingInterface<ISkillHandler>())
        {
            try
            {
                object tmp = _container.GetService(handlerType);
                if (tmp is not ISkillHandler real)
                {
                    continue;
                }

                Log.Debug($"[SKILL][ADD_HANDLER] {handlerType}");
                _handlers.Register(real);
            }
            catch (Exception e)
            {
                Log.Error("[SKILL][FAIL_ADD]", e);
            }
        }
    }
}