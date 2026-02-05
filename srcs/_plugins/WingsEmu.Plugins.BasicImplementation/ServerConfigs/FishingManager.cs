using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WingsAPI.Data.Fish;
using WingsAPI.Data.GameData;
using WingsEmu.Game;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Fish;

namespace NosEmu.Plugins.BasicImplementations.ServerConfigs
{
    public class FishingManager : IFishManager
    {
        private readonly IResourceLoader<FishingSpotDto> _fishingLoader;
        private readonly FishConfiguration _fishConfiguration;
        private IReadOnlyList<FishingSpotDto> _fishingSpotDtos;
        private readonly IRandomGenerator _randomGenerator;

        public FishingManager(IResourceLoader<FishingSpotDto> fishingLoader, FishConfiguration fishConfiguration, IRandomGenerator randomGenerator)
        {
            _fishingLoader = fishingLoader;
            _fishConfiguration = fishConfiguration;
            _randomGenerator = randomGenerator;
        }

        public async Task Initialize()
        {
            _fishingSpotDtos = await _fishingLoader.LoadAsync();
        }
        
        public List<FishingRewardsDto> GetAllRewardsFromEachSpotByIndex()
        {
            var rewardsList = new List<FishingRewardsDto>();
            foreach (FishingRewardsDto reward in from FishingSpotDto e in GetAllFishSpotByIndex()
                                                 from FishingRewardsDto reward in e.Rewards
                                                 select reward)
            {
                if (reward.IsMaterial || rewardsList.Any(s => s.RewardsVnum == reward.RewardsVnum))
                {
                    continue;
                }

                rewardsList.Add(reward);
            }

            return rewardsList;
        }

        public List<FishingRewardsDto> GetAllRewardsFromEachSpot()
        { 
            var rewardsList = new List<FishingRewardsDto>();
            foreach (FishingRewardsDto reward in from FishingSpotDto e in GetAllFishSpot()
                                                 from FishingRewardsDto reward in e.Rewards
                                                 select reward)
            {
                if (rewardsList.Any(s => s.RewardsVnum == reward.RewardsVnum))
                {
                    continue;
                }

                rewardsList.Add(reward);
            }

            return rewardsList;
        }
        
        public List<FishingSpotDto> GetAllFishSpotByIndex()
        {
            var spotsList = new List<FishingSpotDto>();
            foreach (FishingSpotDto o in _fishingSpotDtos)
            {
                if (spotsList.Any(s => s.FishVnum == o.FishVnum))
                {
                    continue;
                }

                spotsList.Add(o);
            }
            return spotsList;
        }

        public List<FishingSpotDto> GetAllFishSpot()
        {
            return _fishingSpotDtos.ToList();
        }

        public FishingSpotDto GetFishSpotBySpotId(long spotId)
        {
            return _fishingSpotDtos.FirstOrDefault(s => s.FishVnum == spotId);
        }

        public FishingSpotDto GetFishSpotByMapId(long mapId)
        {
            return _fishingSpotDtos.FirstOrDefault(s => s.MapId == mapId);
        }

        public List<FishingRewardsDto> GetRewardsListBySpot(FishingSpotDto spot)
        {
            return spot.Rewards.Where(s => !s.IsMaterial).ToList();
        }

        public FishingRewardsDto GetRewardsBySpot(FishingSpotDto spot, RewardFishType rewardsType)
        {
            IEnumerable<FishingRewardsDto> listRewards = rewardsType == RewardFishType.Items ? spot.Rewards.Where(s => s.IsMaterial) :
                    rewardsType == RewardFishType.RareFish ? GetFishRewards(spot, true) : GetFishRewards(spot, false);

            return GetRandomRewardsFromList(listRewards);
        }

        private IEnumerable<FishingRewardsDto> GetFishRewards(FishingSpotDto spot, bool isRareFish)
        {
            var rewardsList = new List<FishingRewardsDto>();

            foreach (FishingRewardsDto i in spot.Rewards.Where(s => !s.IsMaterial))
            {
                FishInfo fishItem = _fishConfiguration.FishInfo[i.RewardsVnum];

                if ((!isRareFish || !fishItem.IsRareRewards) &&
                    (isRareFish || fishItem.IsRareRewards))
                {
                    continue;
                }
                rewardsList.Add(i);
            }

            return rewardsList;
        }

        private FishingRewardsDto GetRandomRewardsFromList(IEnumerable<FishingRewardsDto> items)
        {
            // Calculate the summa of all portions.
            IEnumerable<FishingRewardsDto> fishingRewardsDtos = items.ToList();
            double poolSize = fishingRewardsDtos.Aggregate<FishingRewardsDto, double>(0, (current, t) => current + (int)t.RewardsPercent);

            // Get a random double from 0 to PoolSize.
            double randomNumber = _randomGenerator.RandomNumber(0, poolSize);

            // Detect the item, which corresponds to current random number.
            double accumulatedProbability = 0;
            foreach (FishingRewardsDto t in fishingRewardsDtos)
            {
                accumulatedProbability += (int)t.RewardsPercent;
                if (randomNumber <= accumulatedProbability)
                {
                    return t;
                }
            }

            return fishingRewardsDtos.First();
        }
    }
}