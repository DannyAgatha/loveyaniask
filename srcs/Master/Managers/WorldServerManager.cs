// NosEmu
// 


using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Master.Datas;
using Master.Extensions;
using Master.Proxies;
using Master.Services;
using WingsAPI.Communication.ServerApi.Protocol;
using WingsEmu.Core.Extensions;
using WingsEmu.Game;

namespace Master.Managers
{
    public class WorldServerManager
    {
        private readonly ConcurrentDictionary<int, WorldServer> _worldServerById = new();
        private readonly ConcurrentDictionary<string, List<WorldServer>> _worldServersByWorldGroup = new();

        public IEnumerable<WorldServer> GetWorlds()
        {
            return _worldServerById.Select(keyValuePair => keyValuePair.Value);
        }

        public WorldServer GetWorldById(int channelId) => _worldServerById.TryGetValue(channelId, out WorldServer server) ? server : null;

        public IEnumerable<WorldServer> GetWorldsByWorldGroup(string requestWorldGroup) => _worldServersByWorldGroup.TryGetValue(requestWorldGroup, out List<WorldServer> servers) ? servers : null;

        public bool UnregisterWorld(int channelId)
        {
            if (!_worldServerById.Remove(channelId, out WorldServer value))
            {
                return false;
            }

            if (!_worldServersByWorldGroup.TryGetValue(value.WorldGroup, out List<WorldServer> servers))
            {
                return false;
            }

            servers.RemoveAll(s => s.ChannelId == channelId);
            return true;
        }


        public void RegisterWorldServer(SerializableGameServer serialized)
        {
            var worldServer = serialized.ToWorldServer();
            worldServer.LastPulse = DateTime.UtcNow;

            _worldServerById[worldServer.ChannelId] = worldServer;
            _worldServersByWorldGroup.GetOrSetDefault(worldServer.WorldGroup, new List<WorldServer>()).Add(worldServer);
        }
    }
}