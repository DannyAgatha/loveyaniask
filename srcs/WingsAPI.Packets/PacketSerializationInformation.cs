using System.Reflection;

namespace WingsEmu.Packets
{
    internal class PacketSerializationInformation
    {
        public PacketSerializationInformation(string header, (PacketIndexAttribute, PropertyInfo)[] propertyInfos)
        {
            Header = header;
            Properties = propertyInfos;
        }

        public string Header { get; }
        public (PacketIndexAttribute, PropertyInfo)[] Properties { get; }
    }
}