using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PhoenixLib.Logging;
using WingsEmu.Game._packetHandling;
using WingsEmu.Game.Networking;
using WingsEmu.Packets;

namespace WingsEmu.Plugins.PacketHandling;

public class GenericPacketHandlerContainer<T> : IPacketHandlerContainer<T> where T : IPacketHandler
{
    private readonly Dictionary<Type, T> _handlers = new();

    public void Register(Type packetType, T handler)
    {
        _handlers.Add(packetType, handler);
    }

    public void Unregister(Type packetType)
    {
        _handlers.Remove(packetType);
    }

    public void Execute(IClientSession session, IClientPacket packet, Type packetType)
    {
        ExecuteAsync(session, packet, packetType).ConfigureAwait(false).GetAwaiter().GetResult();
    }

    public async Task ExecuteAsync(IClientSession session, IClientPacket packet, Type packetType)
    {
        if (!_handlers.TryGetValue(packetType, out T handler))
        {
            Log.Info($"[PACKET_HANDLER] {packetType.Name} NO_HANDLER");
            return;
        }

        Log.Info(session.HasSelectedCharacter == false
            ? $"[PACKET_HANDLER] [Id: {session.Account?.Id} - {(session.Account == null ? "None" : "account: " + session.Account?.Name)}] Handling {packet.OriginalHeader}"
            : $"[PACKET_HANDLER] [Id: {session.Account?.Id} - {session.PlayerEntity?.Name}] Handling {packet.OriginalHeader}");
        await handler.HandleAsync(session, packet);
    }
}