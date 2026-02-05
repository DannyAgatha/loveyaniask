using System.Collections.Generic;
using WingsEmu.Game._enum;
using WingsEmu.Game.Configurations.Miniland;
using WingsEmu.Game.Miniland;

namespace WingsAPI.Game.Extensions.MinilandExtensions
{
    public static class MinigameUtilitiesExtensions
    {
        public static bool IsMinigameInventoryRewardsFull(this MapDesignObject mapObj, MinigameConfiguration minigameConfiguration) =>
            mapObj.Level1BoxAmount == minigameConfiguration.Configuration.MinigameMaximumRewards ||
            mapObj.Level2BoxAmount == minigameConfiguration.Configuration.MinigameMaximumRewards ||
            mapObj.Level3BoxAmount == minigameConfiguration.Configuration.MinigameMaximumRewards ||
            mapObj.Level4BoxAmount == minigameConfiguration.Configuration.MinigameMaximumRewards ||
            mapObj.Level5BoxAmount == minigameConfiguration.Configuration.MinigameMaximumRewards;

        public static bool ShowMinigameDurabilityWarning(this MapDesignObject mapObj, MinigameConfiguration minigameConfiguration) =>
            mapObj.InventoryItem.ItemInstance.DurabilityPoint < minigameConfiguration.Configuration.DurabilityWarning;

        public static IEnumerable<MinigameReward> GetYieldRewardEnumerable(this MapDesignObject mapObject) =>
            new List<MinigameReward>
            {
                new()
                {
                    Vnum = mapObject.Level1BoxAmount < 1 ? 0 : (int)ItemVnums.MINIGAME_REWARD_CHEST_1,
                    Amount = mapObject.Level1BoxAmount
                },
                new()
                {
                    Vnum = mapObject.Level2BoxAmount < 1 ? 0 : (int)ItemVnums.MINIGAME_REWARD_CHEST_2,
                    Amount = mapObject.Level2BoxAmount
                },
                new()
                {
                    Vnum = mapObject.Level3BoxAmount < 1 ? 0 : (int)ItemVnums.MINIGAME_REWARD_CHEST_3,
                    Amount = mapObject.Level3BoxAmount
                },
                new()
                {
                    Vnum = mapObject.Level4BoxAmount < 1 ? 0 : (int)ItemVnums.MINIGAME_REWARD_CHEST_4,
                    Amount = mapObject.Level4BoxAmount
                },
                new()
                {
                    Vnum = mapObject.Level5BoxAmount < 1 ? 0 : (int)ItemVnums.MINIGAME_REWARD_CHEST_5,
                    Amount = mapObject.Level5BoxAmount
                }
            };
    }
}