using System;
using PhoenixLib.Extensions;
using PhoenixLib.Logging;
using WingsAPI.Plugins;
using WingsEmu.Game._packetHandling;

namespace WingsEmu.Plugins.PacketHandling;

public class CharScreenPacketHandlerGamePlugin : IGamePlugin
{
    private readonly IServiceProvider _container;
    private readonly IPacketHandlerContainer<ICharacterScreenPacketHandler> _handlers;

    public CharScreenPacketHandlerGamePlugin(IPacketHandlerContainer<ICharacterScreenPacketHandler> handlers, IServiceProvider container)
    {
        _handlers = handlers;
        _container = container;
    }

    public string Name => nameof(CharScreenPacketHandlerGamePlugin);

    public void OnLoad()
    {
        foreach (Type handlerType in typeof(CharScreenPacketHandlerGamePlugin).Assembly.GetTypesImplementingGenericClass(typeof(GenericCharScreenPacketHandlerBase<>)))
        {
            try
            {
                object tmp = _container.GetService(handlerType);
                if (tmp is not ICharacterScreenPacketHandler handler)
                {
                    continue;
                }

                Type type = handlerType.BaseType?.GenericTypeArguments[0];

                Log.Info($"[CHARSCREEN_HANDLERS][ADD_HANDLER] {handlerType}");
                _handlers.Register(type, handler);
            }
            catch (Exception e)
            {
                Log.Error("[CHARSCREEN_HANDLERS][FAIL_ADD]", e);
            }
        }
    }
}