// NosEmu
// 


using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace WingsEmu.Packets
{
    public class PacketDeserializer : IPacketDeserializer
    {
        private static readonly IEnumerable<Type> EnumerableOfAcceptedTypes = new[]
        {
            typeof(int),
            typeof(double),
            typeof(long),
            typeof(short),
            typeof(int?),
            typeof(double?),
            typeof(long?),
            typeof(short?)
        };

        private readonly Dictionary<string, Type> _headersToType = new();
        private readonly Dictionary<Type, Dictionary<PacketIndexAttribute, PropertyInfo>> _packetSerializationInformations = new();

        public PacketDeserializer(IEnumerable<ClientPacketRegistered> registeredPackets)
        {
            // Iterate thru all PacketDefinition implementations
            foreach (ClientPacketRegistered registeredPacket in registeredPackets)
            {
                try
                {
                    GenerateSerializationInformations(registeredPacket.PacketType);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error registering : {registeredPacket} : {e.Message}");
                }

                // add to serialization informations
            }
        }

        /// <summary>
        ///     Deserializes a string into a PacketDefinition
        /// </summary>
        /// <param name="packetContent">The content to deseralize</param>
        /// <param name="packetType">The type of the packet to deserialize to</param>
        /// <param name="includesKeepAliveIdentity">
        ///     Include the keep alive identity or exclude it
        /// </param>
        /// <returns>The deserialized packet.</returns>
        public (IClientPacket, Type) Deserialize(string packetContent, bool includesKeepAliveIdentity = false)
        {
            try
            {
                string packetHeader = packetContent.Split(' ')[includesKeepAliveIdentity ? 1 : 0];
                Type packetType = GetPacketTypeByHeader(packetHeader.StartsWith("#") ? packetHeader.Substring(1) : packetHeader);

                if (packetType == null)
                {
                    var unresolvedPacket = new UnresolvedPacket
                    {
                        OriginalHeader = packetHeader,
                        OriginalContent = packetContent
                    };
                    return (unresolvedPacket, typeof(UnresolvedPacket));
                }

                Dictionary<PacketIndexAttribute, PropertyInfo> serializationInformation = GetSerializationInformation(packetType);
                var deserializedPacket = (ClientPacket)Activator.CreateInstance(packetType); // reflection is bad, improve?
                SetDeserializationInformation(deserializedPacket, packetContent, packetHeader);
                deserializedPacket = Deserialize(packetContent, deserializedPacket, serializationInformation, includesKeepAliveIdentity);
                return (deserializedPacket, packetType);
            }
            catch (Exception e)
            {
                Console.WriteLine($"The serialized packet has the wrong format. Packet: {packetContent}");
                Console.WriteLine(e);
                return (null, null);
            }
        }

        private Type GetPacketTypeByHeader(string packetHeader)
        {
            if (_headersToType.TryGetValue(packetHeader, out Type packetType))
            {
                return packetType;
            }

            return null;
        }


        private ClientPacket Deserialize(string packetContent, ClientPacket deserializedPacket, Dictionary<PacketIndexAttribute, PropertyInfo> serializationInformation, bool includesKeepAliveIdentity)
        {
            MatchCollection matches = Regex.Matches(packetContent, @"([^\040]+[\.][^\040]+[\040]?)+((?=\040)|$)|([^\040]+)((?=\040)|$)");

            if (matches.Count <= 0)
            {
                return deserializedPacket;
            }

            foreach (KeyValuePair<PacketIndexAttribute, PropertyInfo> packetBasePropertyInfo in serializationInformation)
            {
                int currentIndex = packetBasePropertyInfo.Key.Index + (includesKeepAliveIdentity ? 2 : 1); // adding 2 because we need to skip incrementing number and packet header

                if (currentIndex < matches.Count)
                {
                    if (packetBasePropertyInfo.Key.SerializeToEnd)
                    {
                        // get the value to the end and stop deserialization
                        string valueToEnd = packetContent.Substring(matches[currentIndex].Index, packetContent.Length - matches[currentIndex].Index);
                        packetBasePropertyInfo.Value.SetValue(deserializedPacket,
                            DeserializeValue(packetBasePropertyInfo.Value.PropertyType, valueToEnd, packetBasePropertyInfo.Key, matches, includesKeepAliveIdentity));
                        break;
                    }


                    string currentValue = matches[currentIndex].Value;

                    if (packetBasePropertyInfo.Value.PropertyType == typeof(string) && string.IsNullOrEmpty(currentValue))
                    {
                        throw new NullReferenceException();
                    }

                    // set the value & convert currentValue
                    packetBasePropertyInfo.Value.SetValue(deserializedPacket,
                        DeserializeValue(packetBasePropertyInfo.Value.PropertyType, currentValue, packetBasePropertyInfo.Key, matches, includesKeepAliveIdentity));
                }
                else
                {
                    break;
                }
            }

            return deserializedPacket;
        }

        /// <summary>
        ///     Converts for instance -1.12.1.8.-1.-1.-1.-1.-1 to eg. List<byte?> </summary>
        /// <param name="currentValues">String to convert</param>
        /// <param name="genericListType">
        ///     Type
        ///     of the property to convert
        /// </param>
        /// <returns>The string as converted List</returns>
        private IList DeserializeSimpleList(string currentValues, Type genericListType)
        {
            var subpackets = (IList)Convert.ChangeType(Activator.CreateInstance(genericListType), genericListType);
            foreach (string currentValue in currentValues.Split('.'))
            {
                object value = DeserializeValue(genericListType.GenericTypeArguments[0], currentValue, null, null);
                subpackets.Add(value);
            }

            return subpackets;
        }

        private object DeserializeSubpacket(string currentSubValues, Type packetBasePropertyType, Dictionary<PacketIndexAttribute, PropertyInfo> subpacketSerializationInfo,
            bool isReturnPacket = false)
        {
            string[] subpacketValues = currentSubValues.Split(isReturnPacket ? '^' : '.');
            object newSubpacket = Activator.CreateInstance(packetBasePropertyType);

            foreach (KeyValuePair<PacketIndexAttribute, PropertyInfo> subpacketPropertyInfo in subpacketSerializationInfo)
            {
                int currentSubIndex = isReturnPacket ? subpacketPropertyInfo.Key.Index + 1 : subpacketPropertyInfo.Key.Index; // return packets do include header
                string currentSubValue = subpacketValues[currentSubIndex];

                subpacketPropertyInfo.Value.SetValue(newSubpacket, DeserializeValue(subpacketPropertyInfo.Value.PropertyType, currentSubValue, subpacketPropertyInfo.Key, null));
            }

            return newSubpacket;
        }

        /// <summary>
        ///     Converts a Sublist of Packets, For instance 0.4903.5.0.0 2.340.0.0.0
        ///     3.720.0.0.0 5.4912.6.0.0 9.227.0.0.0 10.803.0.0.0 to
        /// </summary>
        /// <param name="currentValue">The value as String</param>
        /// <param name="packetBasePropertyType">Type of the Property to convert to</param>
        /// <param name="shouldRemoveSeparator"></param>
        /// <param name="packetMatchCollections"></param>
        /// <param name="currentIndex"></param>
        /// <param name="includesKeepAliveIdentity"></param>
        /// <returns></returns>
        private IList DeserializeSubpackets(string currentValue, Type packetBasePropertyType, bool shouldRemoveSeparator, MatchCollection packetMatchCollections, int? currentIndex,
            bool includesKeepAliveIdentity)
        {
            // split into single values
            var splitSubPacket = currentValue.Split(' ').ToList();

            // generate new list
            var subPackets = (IList)Convert.ChangeType(Activator.CreateInstance(packetBasePropertyType), packetBasePropertyType);

            Type subPacketType = packetBasePropertyType.GetGenericArguments()[0];
            Dictionary<PacketIndexAttribute, PropertyInfo> subpacketSerializationInfo = GetSerializationInformation(subPacketType);

            // handle subpackets with separator
            if (shouldRemoveSeparator)
            {
                if (!currentIndex.HasValue || packetMatchCollections == null)
                {
                    return subPackets;
                }

                var splittedSubpacketParts = packetMatchCollections.Select(m => m.Value).ToList();
                splitSubPacket = new List<string>();

                string generatedPseudoDelimitedString = string.Empty;
                int subPacketTypePropertiesCount = subpacketSerializationInfo.Count;

                // check if the amount of properties can be serialized properly
                if (((splittedSubpacketParts.Count + (includesKeepAliveIdentity ? 1 : 0))
                        % subPacketTypePropertiesCount) == 0) // amount of properties per subpacket does match the given value amount in %
                {
                    for (int i = currentIndex.Value + 1 + (includesKeepAliveIdentity ? 1 : 0); i < splittedSubpacketParts.Count; i++)
                    {
                        int j;
                        for (j = i; j < i + subPacketTypePropertiesCount; j++)
                        {
                            // add delimited value
                            generatedPseudoDelimitedString += splittedSubpacketParts[j] + ".";
                        }

                        i = j - 1;

                        //remove last added separator
                        generatedPseudoDelimitedString = generatedPseudoDelimitedString.Substring(0, generatedPseudoDelimitedString.Length - 1);

                        // add delimited values to list of values to serialize
                        splitSubPacket.Add(generatedPseudoDelimitedString);
                        generatedPseudoDelimitedString = string.Empty;
                    }
                }
                else
                {
                    throw new Exception("The amount of splitted subpacket values without delimiter do not match the % property amount of the serialized type.");
                }
            }

            foreach (string subpacket in splitSubPacket)
            {
                subPackets.Add(DeserializeSubpacket(subpacket, subPacketType, subpacketSerializationInfo));
            }

            return subPackets;
        }

        private object DeserializeValue(Type packetPropertyType, string currentValue, PacketIndexAttribute packetIndexAttribute, MatchCollection packetMatches,
            bool includesKeepAliveIdentity = false)
        {
            // check for empty value and cast it to null
            if (currentValue == "-1" || currentValue == "-")
            {
                currentValue = null;
            }

            // enum should be casted to number
            if (packetPropertyType.BaseType != null && packetPropertyType.BaseType == typeof(Enum))
            {
                object convertedValue = null;
                try
                {
                    if (currentValue != null && packetPropertyType.IsEnumDefined(Enum.Parse(packetPropertyType, currentValue)))
                    {
                        convertedValue = Enum.Parse(packetPropertyType, currentValue);
                    }
                }
                catch (Exception)
                {
                    //Log.Warn($"Could not convert value {currentValue} to type {packetPropertyType.Name}");
                }

                return convertedValue;
            }

            if (packetPropertyType == typeof(bool)) // handle boolean values
            {
                return currentValue != "0";
            }

            if (packetPropertyType.BaseType != null && packetPropertyType.BaseType == typeof(ClientPacket)) // subpacket
            {
                Dictionary<PacketIndexAttribute, PropertyInfo> subpacketSerializationInfo = GetSerializationInformation(packetPropertyType);
                return DeserializeSubpacket(currentValue, packetPropertyType, subpacketSerializationInfo, packetIndexAttribute?.IsReturnPacket ?? false);
            }

            if (packetPropertyType.IsGenericType && packetPropertyType.GetGenericTypeDefinition().IsAssignableFrom(typeof(List<>)) // subpacket list
                && packetPropertyType.GenericTypeArguments[0].BaseType == typeof(ClientPacket))
            {
                return DeserializeSubpackets(currentValue, packetPropertyType, packetIndexAttribute?.RemoveSeparator ?? false, packetMatches, packetIndexAttribute?.Index, includesKeepAliveIdentity);
            }

            if (packetPropertyType.IsGenericType && packetPropertyType.GetGenericTypeDefinition().IsAssignableFrom(typeof(List<>))) // simple list
            {
                return DeserializeSimpleList(currentValue, packetPropertyType);
            }

            if (Nullable.GetUnderlyingType(packetPropertyType) != null && string.IsNullOrEmpty(currentValue)) // empty nullable value
            {
                return null;
            }

            if (Nullable.GetUnderlyingType(packetPropertyType) == null)
            {
                return Convert.ChangeType(currentValue, packetPropertyType); // cast to specified type
            }

            if (packetPropertyType.GenericTypeArguments[0]?.BaseType == typeof(Enum))
            {
                return Enum.Parse(packetPropertyType.GenericTypeArguments[0], currentValue);
            }

            if (!EnumerableOfAcceptedTypes.Contains(packetPropertyType))
            {
                return Convert.ChangeType(currentValue, packetPropertyType.GenericTypeArguments[0]);
            }

            switch (packetPropertyType)
            {
                case Type _ when packetPropertyType == typeof(long):
                case Type _ when packetPropertyType == typeof(int):
                case Type _ when packetPropertyType == typeof(double):
                case Type _ when packetPropertyType == typeof(short):
                    if (int.TryParse(currentValue, out int b) && b < 0)
                    {
                        currentValue = "0";
                    }

                    break;

                case Type _ when packetPropertyType == typeof(long?):
                case Type _ when packetPropertyType == typeof(int?):
                case Type _ when packetPropertyType == typeof(double?):
                case Type _ when packetPropertyType == typeof(short?):
                    if (currentValue == null)
                    {
                        currentValue = "0";
                    }

                    if (int.TryParse(currentValue, out int c) && c < 0)
                    {
                        currentValue = "0";
                    }

                    break;
            }

            return Convert.ChangeType(currentValue, packetPropertyType.GenericTypeArguments[0]);
        }

        private Dictionary<PacketIndexAttribute, PropertyInfo> GenerateSerializationInformations(Type serializationType)
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

            _headersToType.Add(header, serializationType);
            _packetSerializationInformations.Add(serializationType, packetsForPacketDefinition);
            return _packetSerializationInformations[serializationType];
        }

        private Dictionary<PacketIndexAttribute, PropertyInfo> GetSerializationInformation(Type serializationType) =>
            _packetSerializationInformations.TryGetValue(serializationType, out Dictionary<PacketIndexAttribute, PropertyInfo> infos)
                ? infos
                : GenerateSerializationInformations(serializationType);

        private static void SetDeserializationInformation(ClientPacket packet, string packetContent, string packetHeader)
        {
            packet.OriginalContent = packetContent;
            packet.OriginalHeader = packetHeader;
            packet.IsReturnPacket = packetHeader.StartsWith("#");
        }
    }
}