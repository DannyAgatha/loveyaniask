// NosEmu
// 


using System;
using System.Threading.Tasks;
using WingsEmu.Game.Networking;
using WingsEmu.Packets;

namespace WingsEmu.Game._packetHandling;

public interface IPacketHandlerContainer<in T> where T : IPacketHandler
{
    /// <summary>
    ///     Registers the given packet handler for the given packetType
    /// </summary>
    /// <param name="packetType"></param>
    /// <param name="handler"></param>
    void Register(Type packetType, T handler);

    /// <summary>
    ///     Unregisters the given packetType from the container
    /// </summary>
    /// <param name="packetType"></param>
    void Unregister(Type packetType);

    /// <summary>
    ///     Executes the given packet with the given packetType
    ///     assuming that the sender is the given session
    /// </summary>
    /// <param name="session"></param>
    /// <param name="packet"></param>
    /// <param name="packetType"></param>
    void Execute(IClientSession session, IClientPacket packet, Type packetType);

    /// <summary>
    ///     Asynchronously executes the given packet with the given packetType
    ///     assuming that the sender is the given session
    /// </summary>
    /// <param name="session"></param>
    /// <param name="packet"></param>
    /// <param name="packetType"></param>
    /// <returns></returns>
    Task ExecuteAsync(IClientSession session, IClientPacket packet, Type packetType);
}