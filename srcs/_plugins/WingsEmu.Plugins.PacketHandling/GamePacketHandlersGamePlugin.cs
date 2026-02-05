using System;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using PhoenixLib.Logging;
using WingsAPI.Packets.Handling;
using WingsAPI.Plugins;
using WingsEmu.Game._packetHandling;
using WingsEmu.Packets;

namespace WingsEmu.Plugins.PacketHandling;

public class GamePacketHandlersGamePlugin : IGamePlugin
{
    private readonly IServiceProvider _container;
    private readonly IPacketHandlerContainer<IGamePacketHandler> _handlers;

    public GamePacketHandlersGamePlugin(IPacketHandlerContainer<IGamePacketHandler> handlers, IServiceProvider container)
    {
        _handlers = handlers;
        _container = container;
    }

    public string Name => nameof(GamePacketHandlersGamePlugin);

    public void OnLoad()
    {
        foreach (RegisteredPacketHandler registeredPacketHandler in _container.GetServices<RegisteredPacketHandler>())
        {
            try
            {
                Type handlerType = registeredPacketHandler.HandlerType;
                object tmp = _container.GetService(handlerType);
                if (tmp is not IGamePacketHandler handler)
                {
                    continue;
                }

                Type type = handlerType.BaseType?.GenericTypeArguments[0];

                _handlers.Register(type, handler);
                Log.Info($"[GAME_HANDLERS][ADD_HANDLER] {type.GetCustomAttribute<PacketHeaderAttribute>().Identification}");
            }
            catch (Exception e)
            {
                Log.Error("[GAME_HANDLERS][FAIL_ADD]", e);
                // ignored
            }
        }
    }
}