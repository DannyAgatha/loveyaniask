using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using PhoenixLib.Logging;
using WingsAPI.Communication.Player;

namespace Master.Managers
{
    public class ClusterCharacterManager
    {
        private readonly SynchronizedCollection<ClusterCharacterInfo> _allCharacters = new();
        private readonly ConcurrentDictionary<long, ClusterCharacterInfo> _characterById = new();
        private readonly ConcurrentDictionary<string, ClusterCharacterInfo> _characterByName = new();
        private readonly ConcurrentDictionary<byte, List<ClusterCharacterInfo>> _charactersByChannel = new();

        public void AddClusterCharacter(ClusterCharacterInfo characterInfo)
        {
            if (!characterInfo.ChannelId.HasValue)
            {
                return;
            }

            _allCharacters.Add(characterInfo);
            _charactersByChannel.GetOrAdd(characterInfo.ChannelId.Value, new List<ClusterCharacterInfo>()).Add(characterInfo);
            _characterById[characterInfo.Id] = characterInfo;
            _characterByName[characterInfo.Name] = characterInfo;
        }

        public void RemoveClusterCharacter(long characterId)
        {
            if (!_characterById.TryGetValue(characterId, out ClusterCharacterInfo characterInfo) || !characterInfo.ChannelId.HasValue)
            {
                Log.Warn("[CLUSTER_CHARACTER_MANAGER] Tried to remove a character that wasn't in the Manager.");
                return;
            }

            _allCharacters.Remove(characterInfo);
            _charactersByChannel.GetOrAdd(characterInfo.ChannelId.Value, new List<ClusterCharacterInfo>()).Remove(characterInfo);
            _characterById[characterInfo.Id] = characterInfo;
            _characterByName[characterInfo.Name] = characterInfo;
        }

        public ClusterCharacterInfo GetCharacterById(long id) => _characterById.GetValueOrDefault(id, null);

        public ClusterCharacterInfo GetCharacterByName(string name) => _characterByName.GetValueOrDefault(name, null);

        public IReadOnlyList<ClusterCharacterInfo> GetCharactersByChannelId(byte channelId) => _charactersByChannel.GetValueOrDefault(channelId, null);

        public IReadOnlyCollection<KeyValuePair<byte, List<ClusterCharacterInfo>>> GetCharactersSortedByChannel() => _charactersByChannel;

        public IReadOnlyList<ClusterCharacterInfo> GetCharacters() => _allCharacters.ToList();
    }
}