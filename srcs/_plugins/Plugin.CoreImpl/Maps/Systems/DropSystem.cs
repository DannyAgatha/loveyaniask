using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using WingsEmu.Core.Extensions;
using WingsEmu.Game;
using WingsEmu.Game._ECS;
using WingsEmu.Game._ECS.Systems;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Maps;

namespace Plugin.CoreImpl.Maps.Systems
{
    public sealed class DropSystem : IMapSystem, IDropSystem
    {
        private const int MAX_DROPS = 200;
        private const int MESSAGE_PERCENTAGE_CHANCE = 10;
        private readonly ConcurrentDictionary<long, MapItem> _drops;
        private readonly IMapInstance _mapInstance;
        private readonly IRandomGenerator _randomGenerator;

        public DropSystem(IMapInstance mapInstance, IRandomGenerator randomGenerator)
        {
            _mapInstance = mapInstance;
            _randomGenerator = randomGenerator;
            _drops = new ConcurrentDictionary<long, MapItem>();
        }


        public IReadOnlyList<MapItem> Drops => _drops.Values.ToArray();

        public void AddDrop(MapItem item)
        {
            if (!_drops.TryAdd(item.TransportId, item))
            {
                return;
            }

            if (_drops.Count <= MAX_DROPS)
            {
                return;
            }

            MapItem first = _drops.Values.OrderBy(x => x.CreatedDate).FirstOrDefault();
            if (first == null)
            {
                return;
            }

            first.BroadcastOut();
            RemoveDrop(first.TransportId);
        }

        public bool RemoveDrop(long dropId) => _drops.TryRemove(dropId, out MapItem item);
        public bool HasDrop(long dropId) => _drops.ContainsKey(dropId);
        public MapItem GetDrop(long dropId) => _drops.GetOrDefault(dropId);

        public string Name => nameof(DropSystem);

        public void ProcessTick(DateTime date, bool isTickRefresh = false)
        {
            if (_mapInstance.MapInstanceType == MapInstanceType.Miniland)
            {
                return;
            }

            foreach (KeyValuePair<long, MapItem> drop in _drops)
            {
                Update(date, drop.Value);
            }
        }

        public void PutIdleState()
        {
        }

        public void Clear()
        {
            _drops.Clear();
        }

        private void Update(DateTime date, MapItem mapItem)
        {
            if (mapItem.CreatedDate == null)
            {
                return;
            }

            DateTime createdDate = mapItem.CreatedDate.Value;
            if (createdDate.AddSeconds(150) > date)
            {
                return;
            }

            if (createdDate.AddSeconds(180) > date)
            {
                if (mapItem.ShowMessageEasterEgg.AddSeconds(3) > date)
                {
                    return;
                }

                mapItem.ShowMessageEasterEgg = DateTime.UtcNow;
                int randomNumber = _randomGenerator.RandomNumber(10000);
                int chance = _randomGenerator.RandomNumber(MESSAGE_PERCENTAGE_CHANCE);

                if (chance > randomNumber)
                {
                    mapItem.BroadcastSayDrop();
                }

                return;
            }

            mapItem.BroadcastOut();
            RemoveDrop(mapItem.TransportId);
        }
    }
}