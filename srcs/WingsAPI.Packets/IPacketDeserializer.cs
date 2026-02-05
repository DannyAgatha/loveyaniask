using System;

namespace WingsEmu.Packets
{
    public interface IPacketDeserializer
    {
        (IClientPacket, Type) Deserialize(string serializedData, bool includeKeepAlive);
    }
}