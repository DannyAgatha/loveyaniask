using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using WingsEmu.Packets.ClientPackets;

namespace WingsEmu.Packets
{
    public sealed class PacketSerializer : IPacketSerializer
    {
        private static readonly bool _isInitialized;
        private static readonly Dictionary<Type, PacketSerializationInformation> _packetSerialization = new();

        static PacketSerializer()
        {
            IEnumerable<Type> packetTypes = typeof(GameStartPacket).Assembly.GetTypes().Where(s => !s.IsInterface && !s.IsAbstract && typeof(ServerPacket).IsAssignableFrom(s));

            if (_isInitialized)
            {
                return;
            }

            foreach (Type packetType in packetTypes)
            {
                if (!_packetSerialization.ContainsKey(packetType))
                {
                    _packetSerialization.Add(packetType, GenerateSerializationInformations(packetType));
                }
            }

            _isInitialized = true;
        }

        public string Serialize<T>(T packet) where T : IPacket
        {
            try
            {
                // load pregenerated serialization information
                PacketSerializationInformation serializationInformation = GetSerializationInformation(typeof(T));

                var deserializedPacket = new StringBuilder(serializationInformation.Header); // set header

                int lastIndex = 0;
                foreach ((PacketIndexAttribute index, PropertyInfo property) in serializationInformation.Properties)
                {
                    // check if we need to add a non mapped values (pseudovalues)
                    if (index.Index > lastIndex + 1)
                    {
                        int amountOfEmptyValuesToAdd = index.Index - (lastIndex + 1);

                        for (int i = 0; i < amountOfEmptyValuesToAdd; i++)
                        {
                            deserializedPacket.Append(" 0");
                        }
                    }

                    // add value for current configuration
                    deserializedPacket.Append(SerializeValue(property.PropertyType, property.GetValue(packet), index));

                    // check if the value should be serialized to end
                    if (index.SerializeToEnd)
                    {
                        // we reached the end
                        break;
                    }

                    // set new index
                    lastIndex = index.Index;
                }

                return deserializedPacket.ToString();
            }
            catch (Exception e)
            {
                // Log.Warn("Wrong Packet Format!", e);
                return string.Empty;
            }
        }

        private static PacketSerializationInformation GenerateSerializationInformations(Type serializationType)
        {
            string header = serializationType.GetCustomAttribute<PacketHeaderAttribute>()?.Identification;

            if (string.IsNullOrEmpty(header))
            {
                throw new Exception($"Packet header cannot be empty. PacketType: {serializationType.Name}");
            }

            var packetsForPacketDefinition = new Dictionary<PacketIndexAttribute, PropertyInfo>();

            foreach (PropertyInfo packetBasePropertyInfo in serializationType.GetProperties().Where(x => x.GetCustomAttributes(false).OfType<PacketIndexAttribute>().Any()))
            {
                PacketIndexAttribute indexAttribute = packetBasePropertyInfo.GetCustomAttributes(false).OfType<PacketIndexAttribute>().FirstOrDefault();

                if (indexAttribute != null)
                {
                    packetsForPacketDefinition.Add(indexAttribute, packetBasePropertyInfo);
                }
            }

            // order by index
            IOrderedEnumerable<KeyValuePair<PacketIndexAttribute, PropertyInfo>> keyValuePairs = packetsForPacketDefinition.OrderBy(p => p.Key.Index);
            return new PacketSerializationInformation(header, keyValuePairs.Select(s => (s.Key, s.Value)).ToArray());
        }

        private PacketSerializationInformation GetSerializationInformation(Type serializationType)
        {
            if (!_packetSerialization.TryGetValue(serializationType, out PacketSerializationInformation infos))
            {
                infos = GenerateSerializationInformations(serializationType);
                _packetSerialization[serializationType] = infos;
            }

            return infos;
        }

        private void GenerateSerializationInformations<TPacketDefinition>()
        where TPacketDefinition : ServerPacket
        {
            // Iterate thru all PacketDefinition implementations
            foreach (Type packetBaseType in typeof(TPacketDefinition).Assembly.GetTypes().Where(p => !p.IsInterface && !p.IsAbstract && typeof(TPacketDefinition).BaseType.IsAssignableFrom(p)))
            {
                // add to serialization informations
                GenerateSerializationInformations(packetBaseType);
            }
        }

        private string SerializeValue(Type propertyType, object value, PacketIndexAttribute packetIndexAttribute = null)
        {
            if (propertyType == null)
            {
                return string.Empty;
            }

            // check for nullable without value or string
            if (propertyType == typeof(string) && string.IsNullOrEmpty(Convert.ToString(value)))
            {
                return " -";
            }

            if (Nullable.GetUnderlyingType(propertyType) != null && string.IsNullOrEmpty(Convert.ToString(value)))
            {
                return " -1";
            }

            // enum should be casted to number
            if (propertyType.BaseType != null && propertyType.BaseType == typeof(Enum))
            {
                return $" {Convert.ToInt16(value)}";
            }

            if (propertyType == typeof(bool))
            {
                // bool is 0 or 1 not True or False
                return Convert.ToBoolean(value) ? " 1" : " 0";
            }

            if (propertyType.BaseType != null && propertyType.BaseType == typeof(ServerPacket))
            {
                PacketSerializationInformation subpacketSerializationInfo = GetSerializationInformation(propertyType);
                return SerializeSubpacket(value, subpacketSerializationInfo, packetIndexAttribute?.IsReturnPacket ?? false, packetIndexAttribute?.RemoveSeparator ?? false);
            }

            if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition().IsAssignableFrom(typeof(List<>))
                && propertyType.GenericTypeArguments[0].BaseType == typeof(ServerPacket))
            {
                return SerializeSubpackets((IList)value, propertyType, packetIndexAttribute?.RemoveSeparator ?? false);
            }

            if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition().IsAssignableFrom(typeof(List<>))) //simple list
            {
                return SerializeSimpleList((IList)value, propertyType);
            }

            return $" {value}";
        }

        private string SerializeSubpackets(ICollection listValues, Type packetBasePropertyType, bool shouldRemoveSeparator)
        {
            string serializedSubPacket = string.Empty;
            PacketSerializationInformation tmp = GetSerializationInformation(packetBasePropertyType.GetGenericArguments()[0]);

            if (listValues.Count > 0)
            {
                serializedSubPacket = listValues.Cast<object>().Aggregate(serializedSubPacket,
                    (current, listValue) => current + SerializeSubpacket(listValue, tmp, false, shouldRemoveSeparator));
            }

            return serializedSubPacket;
        }

        /// <summary>
        ///     Converts for instance List<byte?> to -1.12.1.8.-1.-1.-1.-1.-1
        /// </summary>
        /// <param
        ///     name="listValues">
        ///     Values in List of simple type.
        /// </param>
        /// <param name="propertyType">
        ///     The
        ///     simple type.
        /// </param>
        /// <returns></returns>
        private string SerializeSimpleList(IList listValues, Type propertyType)
        {
            string resultListPacket = string.Empty;
            int listValueCount = listValues.Count;
            if (listValueCount <= 0)
            {
                return resultListPacket;
            }

            resultListPacket += SerializeValue(propertyType.GenericTypeArguments[0], listValues[0]);

            for (int i = 1; i < listValueCount; i++)
            {
                resultListPacket += $".{SerializeValue(propertyType.GenericTypeArguments[0], listValues[i]).Replace(" ", "")}";
            }

            return resultListPacket;
        }

        private string SerializeSubpacket(object value, PacketSerializationInformation subpacketSerializationInfo, bool isReturnPacket,
            bool shouldRemoveSeparator)
        {
            string serializedSubpacket = isReturnPacket ? $" #{subpacketSerializationInfo.Header}^" : " ";

            // iterate thru configure subpacket properties
            foreach ((PacketIndexAttribute index, PropertyInfo property) in subpacketSerializationInfo.Properties)
            {
                // first element
                if (index.Index != 0)
                {
                    serializedSubpacket += isReturnPacket ? "^" :
                        shouldRemoveSeparator ? " " : ".";
                }

                serializedSubpacket += SerializeValue(property.PropertyType, property.GetValue(value)).Replace(" ", "");
            }

            return serializedSubpacket;
        }
    }
}