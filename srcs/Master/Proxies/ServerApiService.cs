// NosEmu
// 


using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Master.Datas;
using Master.Extensions;
using Master.Managers;
using PhoenixLib.Logging;
using WingsAPI.Communication;
using WingsAPI.Communication.ServerApi;
using WingsAPI.Communication.ServerApi.Protocol;

namespace Master.Proxies
{
    public class ServerApiService : IServerApiService
    {
        private readonly WorldServerManager _worldManager;

        public ServerApiService(WorldServerManager worldManager)
        {
            _worldManager = worldManager;
        }

        public async ValueTask<BasicRpcResponse> IsMasterOnline(EmptyRpcRequest request) =>
            new BasicRpcResponse
            {
                ResponseType = RpcResponseType.SUCCESS
            };

        /*
         * World
         */
        public async ValueTask<BasicRpcResponse> RegisterWorldServer(RegisterWorldServerRequest request)
        {
            try
            {
                SerializableGameServer serialized = request.GameServer;
                Log.Info($"[SERVER_API_SERVICE][WORLD_SERVER][REGISTER] {serialized.WorldGroup}:{serialized.ChannelId}:{serialized.ChannelType} - {serialized.EndPointIp}:{serialized.EndPointPort}");
                _worldManager.RegisterWorldServer(serialized);
                return new BasicRpcResponse
                {
                    ResponseType = RpcResponseType.SUCCESS
                };
            }
            catch (Exception e)
            {
                Log.Error("[SERVER_API_SERVICE][WORLD_SERVER][REGISTER] Unexpected error: ", e);
                return new BasicRpcResponse
                {
                    ResponseType = RpcResponseType.GENERIC_SERVER_ERROR
                };
            }
        }

        public async ValueTask<BasicRpcResponse> PulseWorldServer(PulseWorldServerRequest request)
        {
            int channelId = request.ChannelId;
            WorldServer tmp = _worldManager.GetWorldById(channelId);
            if (tmp == null)
            {
                Log.Warn($"[SERVER_API_SERVICE][WORLD_SERVER][PULSE] Pulse from: {channelId.ToString()} invalid");
                return new BasicRpcResponse
                {
                    ResponseType = RpcResponseType.GENERIC_SERVER_ERROR
                };
            }

            Log.Debug($"[SERVER_API_SERVICE][WORLD_SERVER][PULSE] Pulse received from {tmp.WorldGroup}:{tmp.ChannelId.ToString()}");
            tmp.LastPulse = DateTime.UtcNow;
            tmp.SessionsCount = request.SessionsCount;
            return new BasicRpcResponse
            {
                ResponseType = RpcResponseType.SUCCESS
            };
        }

        public async ValueTask<BasicRpcResponse> UnregisterWorldServer(UnregisterWorldServerRequest request)
        {
            Log.Debug($"[SERVER_API_SERVICE][WORLD_SERVER][UNREGISTER] {request.ChannelId.ToString()}");
            bool removed = _worldManager.UnregisterWorld(request.ChannelId);
            return new BasicRpcResponse
            {
                ResponseType = removed ? RpcResponseType.SUCCESS : RpcResponseType.GENERIC_SERVER_ERROR
            };
        }

        public async ValueTask<BasicRpcResponse> SetWorldServerVisibility(SetWorldServerVisibilityRequest request)
        {
            WorldServer requestedWorld = _worldManager.GetWorldById(request.ChannelId);
            if (requestedWorld == null || requestedWorld.WorldGroup != request.WorldGroup)
            {
                return new BasicRpcResponse
                {
                    ResponseType = RpcResponseType.GENERIC_SERVER_ERROR
                };
            }

            requestedWorld.AuthorityRequired = request.AuthorityRequired;
            return new BasicRpcResponse
            {
                ResponseType = RpcResponseType.SUCCESS
            };
        }

        public async ValueTask<GetChannelInfoResponse> GetChannelInfo(GetChannelInfoRequest request)
        {
            Log.Debug($"[SERVER_API_SERVICE][WORLD_SERVER][GET_CHANNEL_INFO] {request.WorldGroup} {request.ChannelId}");
            IEnumerable<WorldServer> worlds = _worldManager.GetWorldsByWorldGroup(request.WorldGroup);

            WorldServer world = worlds.FirstOrDefault(s => s.ChannelId == request.ChannelId);
            if (world == null)
            {
                return new GetChannelInfoResponse
                {
                    ResponseType = RpcResponseType.GENERIC_SERVER_ERROR
                };
            }

            Log.Debug($"[SERVER_API_SERVICE][WORLD_SERVER][GET_CHANNEL_INFO] {request.WorldGroup} {request.ChannelId} found");
            return new GetChannelInfoResponse
            {
                ResponseType = RpcResponseType.SUCCESS,
                GameServer = world.ToSerializableWorldServer()
            };
        }

        public async ValueTask<GetChannelInfoResponse> GetAct4ChannelInfo(GetAct4ChannelInfoRequest request)
        {
            Log.Debug($"[SERVER_API_SERVICE][WORLD_SERVER][GET_ACT4_CHANNEL_INFO] {request.WorldGroup}");
            IEnumerable<WorldServer> worlds = _worldManager.GetWorldsByWorldGroup(request.WorldGroup);

            if (worlds == null || !worlds.Any())
            {
                return new GetChannelInfoResponse
                {
                    ResponseType = RpcResponseType.GENERIC_SERVER_ERROR
                };
            }

            WorldServer world = worlds.SingleOrDefault(s => s.ChannelType == GameChannelType.ACT_4);
            if (world == null)
            {
                return new GetChannelInfoResponse
                {
                    ResponseType = RpcResponseType.GENERIC_SERVER_ERROR
                };
            }

            Log.Debug("[SERVER_API_SERVICE][WORLD_SERVER][GET_ACT4_CHANNEL_INFO] Act4 Channel found");
            return new GetChannelInfoResponse
            {
                ResponseType = RpcResponseType.SUCCESS,
                GameServer = world.ToSerializableWorldServer()
            };
        }


        public async ValueTask<RetrieveRegisteredWorldServersResponse> RetrieveRegisteredWorldServers(RetrieveRegisteredWorldServersRequest request)
        {
            Log.Debug("[SERVER_API_SERVICE][GAME_SERVER_LIST]");
            IEnumerable<WorldServer> worlds = _worldManager.GetWorlds().Where(s => s.AuthorityRequired <= request.RequesterAuthority && s.ChannelType != GameChannelType.ACT_4);

            var serverList = worlds.Select(s => s.ToSerializableWorldServer()).ToList();

            return new RetrieveRegisteredWorldServersResponse
            {
                WorldServers = serverList
            };
        }


        public Task<RetrieveRegisteredWorldServersResponse> RetrieveAllGameServers(EmptyRpcRequest request)
        {
            Log.Debug("[SERVER_API_SERVICE][GET_GAME_SERVERS]");
            IEnumerable<WorldServer> worlds = _worldManager.GetWorlds();

            var serverList = worlds.Select(s => s.ToSerializableWorldServer()).ToList();

            return Task.FromResult(new RetrieveRegisteredWorldServersResponse
            {
                WorldServers = serverList
            });
        }
    }
}