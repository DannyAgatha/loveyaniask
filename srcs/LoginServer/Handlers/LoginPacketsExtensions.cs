using System.Collections.Generic;
using System.Text;
using LoginServer.Network;
using PhoenixLib.Logging;
using PhoenixLib.MultiLanguage;
using WingsAPI.Communication.ServerApi.Protocol;
using WingsEmu.Packets.Enums;

namespace LoginServer.Handlers
{
    internal static class LoginPacketsExtensions
    {
        internal static string GenerateFailcPacket(this LoginClientSession session, LoginFailType failType) => $"failc {((short)failType).ToString()}";

        internal static void SendChannelPacketList(this LoginClientSession session, int encryptionKey, string sessionId, RegionLanguageType region, IEnumerable<SerializableGameServer> worldServers,
            bool isOldLogin, byte playerInFirstServer)
        {
            string lastGroup = string.Empty;
            int worldGroupCount = 0;
            var packetBuilder = new StringBuilder();
            string worldList =
                // English
                $"1 {playerInFirstServer} -99 0 -99 0 -99 0 " +
                // German
                "-99 0 -99 0 -99 0 -99 0 " +
                // French
                "-99 0 -99 0 -99 0 -99 0 " +
                // Italian
                "-99 0 -99 0 -99 0 -99 0 " +
                // Polish
                "-99 0 -99 0 -99 0 -99 0 " +
                // Spanish
                "-99 0 -99 0 -99 0 -99 0 " +
                // Russian
                "-99 0 -99 0 -99 0 -99 0 " +
                // Czech
                "-99 0 -99 0 -99 0 -99 0 " +
                // Turkish
                "-99 0 -99 0 -99 0 -99 0 " +
                
                "0 -99 0 -99 0 -99 0 -99 0 -99 0 -99 0 -99 0 -99 0 -99 0 -99 0 -99 0 -99 0 -99 0 -99 0 -99 0 -99 0 -99 0 -99 0 -99 0 -99 0 -99 0 -99 0 -99 0 0 ";

            packetBuilder.AppendFormat("NsTeST {0} {1} ", (byte)region, sessionId);
            packetBuilder.AppendFormat("{0}", !isOldLogin ? "2 " : string.Empty);
            packetBuilder.AppendFormat("{0} {1} ", worldList, encryptionKey);

            foreach (SerializableGameServer world in worldServers)
            {
                if (lastGroup != world.WorldGroup)
                {
                    worldGroupCount++;
                }

                lastGroup = world.WorldGroup;
                int color = (int)(world.SessionCount / (double)world.AccountLimit * 20);
                packetBuilder.AppendFormat("{0}:{1}:{2}:{3}.{4}.{5} ", world.EndPointIp, world.EndPointPort, color, worldGroupCount, world.ChannelId, world.WorldGroup.Replace(' ', '^'));
            }

            packetBuilder.AppendFormat("-1:-1:-1:10000.10000.{0}", !isOldLogin ? "4" : "1");

            string packet = packetBuilder.ToString();

            Log.Info($"[CHANNEL_LIST] {packet}");

            session.SendPacket(packet);
        }
    }
}