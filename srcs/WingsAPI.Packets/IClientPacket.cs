namespace WingsEmu.Packets
{
    /// <summary>
    ///     Packets that are sent by client
    /// </summary>
    public interface IClientPacket : IPacket
    {
        string OriginalContent { get; set; }

        string OriginalHeader { get; set; }
    }
}