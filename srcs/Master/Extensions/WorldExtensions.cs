// NosEmu
// 


using System;
using Master.Datas;
using Master.Proxies;
using Master.Services;
using WingsAPI.Communication.ServerApi.Protocol;
using WingsEmu.Game;

namespace Master.Extensions
{
    internal static class WorldExtensions
    {
        public static WorldServer ToWorldServer(this SerializableGameServer serialized)
        {
            return new WorldServer
            {
                WorldGroup = serialized.WorldGroup,
                ChannelId = serialized.ChannelId,
                ChannelType = serialized.ChannelType,
                AccountLimit = serialized.AccountLimit,
                SessionsCount = serialized.SessionCount,
                AuthorityRequired = serialized.Authority,
                EndPointIp = serialized.EndPointIp,
                EndPointPort = serialized.EndPointPort,
                RegistrationDate = DateTime.UtcNow
            };
        }

        public static SerializableGameServer ToSerializableWorldServer(this WorldServer world)
        {
            return new SerializableGameServer
            {
                WorldGroup = world.WorldGroup,
                ChannelId = world.ChannelId,
                ChannelType = world.ChannelType,
                AccountLimit = world.AccountLimit,
                SessionCount = world.SessionsCount,
                Authority = world.AuthorityRequired,
                EndPointIp = world.EndPointIp,
                EndPointPort = world.EndPointPort,
                RegistrationDate = world.RegistrationDate
            };
        }
    }
}