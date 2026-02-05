namespace WingsEmu.Packets
{
    public interface IPacketSerializer
    {
        string Serialize<T>(T packet) where T : IPacket;
    }
}