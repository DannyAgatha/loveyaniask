namespace WingsEmu.Packets
{
    public abstract class ClientPacket : IClientPacket
    {
        /// <summary>
        ///     tells
        /// </summary>
        public bool IsReturnPacket { get; set; }

        public string OriginalContent { get; set; }

        public string OriginalHeader { get; set; }
    }
}