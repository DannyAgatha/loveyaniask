// NosEmu
// 


using System;

namespace WingsEmu.Packets
{
    public class PacketHeaderAttribute : Attribute
    {
        public PacketHeaderAttribute(string identification) => Identification = identification;


        /// <summary>
        ///     Unique identification of the Packet
        /// </summary>
        public string Identification { get; set; }

        /// <summary>
        ///     Pass the packet to handler method even if the serialization has failed.
        /// </summary>
        public bool PassNonParseablePacket { get; set; }
    }
}