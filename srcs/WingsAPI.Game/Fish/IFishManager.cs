using System.Collections.Generic;
using System.Threading.Tasks;
using WingsAPI.Data.Fish;

namespace WingsEmu.Game.Fish;

public interface IFishManager
{
    Task Initialize();
    List<FishingSpotDto> GetAllFishSpot();
    
    List<FishingSpotDto> GetAllFishSpotByIndex();
    FishingSpotDto GetFishSpotBySpotId(long spotId);
    List<FishingRewardsDto> GetRewardsListBySpot(FishingSpotDto spot);
    FishingRewardsDto GetRewardsBySpot(FishingSpotDto spot, RewardFishType rewardsType);
    List<FishingRewardsDto> GetAllRewardsFromEachSpot();
    List<FishingRewardsDto> GetAllRewardsFromEachSpotByIndex();
    FishingSpotDto GetFishSpotByMapId(long mapId);
}