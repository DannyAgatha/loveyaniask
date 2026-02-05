using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WingsEmu.Game.Networking.Broadcasting;
using WingsEmu.Packets;

namespace WingsEmu.Game.Networking;

public interface IBroadcaster
{
    IReadOnlyList<IClientSession> Sessions { get; }

    void Broadcast(string packet);

    void Broadcast<T>(T packet, params IBroadcastRule[] rules) where T : IServerPacket;
    void Broadcast(string packet, params IBroadcastRule[] rules);

    void Broadcast(IEnumerable<string> packets);
    void Broadcast(IEnumerable<string> packets, params IBroadcastRule[] rules);


    void Broadcast(Func<IClientSession, string> generatePacketCallback);
    void Broadcast(Func<IClientSession, string> generatePacketCallback, params IBroadcastRule[] rules);


    Task BroadcastAsync(Func<IClientSession, Task<string>> lambdaAsync);
    Task BroadcastAsync(Func<IClientSession, Task<string>> lambdaAsync, params IBroadcastRule[] rules);

    void Broadcast<T>(T packet) where T : IPacket;
}