using System.Collections.Generic;
using System.Linq;
using System.Text;
using WingsEmu.Game.Configurations.Miniland;
using WingsEmu.Game.Miniland;
using WingsEmu.Game.Networking;

namespace WingsAPI.Game.Extensions.MinilandExtensions
{
    public static class MinigamePacketExtensions
    {
        public static string GenerateMloPmg(this IClientSession session, MapDesignObject mlobj, IEnumerable<MinigameReward> itemsToShow, MinigameConfiguration minigameConfiguration)
        {
            var initialString = new StringBuilder(
                $"mlo_pmg {mlobj.InventoryItem.ItemInstance.ItemVNum.ToString()} {session.PlayerEntity.MinilandPoint.ToString()} {(mlobj.ShowMinigameDurabilityWarning(minigameConfiguration) ? 1 : 0).ToString()}" +
                $" {(mlobj.IsMinigameInventoryRewardsFull(minigameConfiguration) ? 1 : 0).ToString()}");

            var itemsToShowList = itemsToShow.ToList();

            for (int i = itemsToShowList.Count; i < 14; i++)
            {
                itemsToShowList.Add(new MinigameReward());
            }

            foreach (MinigameReward itemToShow in itemsToShowList)
            {
                initialString.Append($" {itemToShow.Vnum.ToString()} {itemToShow.Amount.ToString()}");
            }

            return initialString.ToString();
        }

        public static string GenerateMloMg(this IClientSession session, MapDesignObject mlobj, MinigameConfiguration minigameConfiguration) =>
            $"mlo_mg {mlobj.InventoryItem.ItemInstance.ItemVNum.ToString()} {session.PlayerEntity.MinilandPoint.ToString()} {(mlobj.ShowMinigameDurabilityWarning(minigameConfiguration) ? 1 : 0).ToString()}" +
            $" {(mlobj.IsMinigameInventoryRewardsFull(minigameConfiguration) ? 1 : 0).ToString()} {mlobj.InventoryItem.ItemInstance.DurabilityPoint.ToString()} {mlobj.InventoryItem.ItemInstance.GameItem.MinilandObjectPoint.ToString()}";

        public static string GenerateMinigamePoints(this IClientSession session, MinigameConfiguration minigameConfiguration)
            => $"mlpt {session.PlayerEntity.MinilandPoint.ToString()} {minigameConfiguration.Configuration.MinigamePointsCostPerMinigame.ToString()}";

        public static string GenerateMinigameReward(int vnum, int amount) => $"mlo_rw {vnum.ToString()} {amount.ToString()}";
        public static string GenerateMinigameRewardLevel(RewardLevel rewardLevel) => $"mlo_lv {((sbyte)rewardLevel).ToString()}";

        public static string GenerateMinigameInfo(this MapDesignObject mapObj, IClientSession session,
            MinigameConfiguration minigameConfiguration, MinigameScoresHolder minigameScoresHolder)
        {
            IOrderedEnumerable<ScoreHolder> scores = minigameScoresHolder.Scores.OrderBy(x => x.ScoreRange.Minimum);

            var packetBase = new StringBuilder(
                $"mlo_info {(session.CurrentMapInstance.Id == session.PlayerEntity.Miniland.Id ? 1 : 0).ToString()} {mapObj.InventoryItem.ItemInstance.ItemVNum.ToString()}" +
                $" {mapObj.InventorySlot.ToString()} {session.PlayerEntity.MinilandPoint.ToString()}" +
                $" {(mapObj.ShowMinigameDurabilityWarning(minigameConfiguration) ? 1 : 0).ToString()}" +
                $" {(mapObj.IsMinigameInventoryRewardsFull(minigameConfiguration) ? 1 : 0).ToString()}");

            foreach (ScoreHolder score in scores)
            {
                packetBase.Append($" {score.ScoreRange.Minimum.ToString()} {score.ScoreRange.Maximum.ToString()}");
            }

            return packetBase.ToString();
        }

        public static string GenerateMinilandObject(this MapDesignObject mapObj, bool deleted) =>
            $"mlobj {(deleted ? 0 : 1).ToString()} {mapObj.InventoryItem.Slot.ToString()} {mapObj.MapX.ToString()} {mapObj.MapY.ToString()}" +
            $" {mapObj.InventoryItem.ItemInstance.GameItem.Width.ToString()} {mapObj.InventoryItem.ItemInstance.GameItem.Height.ToString()} 0 {mapObj.InventoryItem.ItemInstance.DurabilityPoint.ToString()}" +
            $" 0 {(mapObj.InventoryItem.ItemInstance.GameItem.IsWarehouse ? 1 : 0).ToString()}";

        public static string GenerateEffect(this MapDesignObject mapObj, bool removed) =>
            $"eff_g {(mapObj.InventoryItem.ItemInstance.GameItem?.EffectValue ?? mapObj.InventoryItem.ItemInstance.Design).ToString()}" +
            $" {mapObj.MapX:00}{mapObj.MapY:00}" +
            $" {mapObj.MapX.ToString()} {mapObj.MapY.ToString()} {(removed ? 1 : 0).ToString()}";

        public static void SendMinilandYieldInfo(this IClientSession session, MapDesignObject mapObject, IEnumerable<MinigameReward> minigameRewards, MinigameConfiguration minigameConfiguration)
            => session.SendPacket(session.GenerateMloPmg(mapObject, minigameRewards, minigameConfiguration));

        public static void SendMinilandDurabilityInfo(this IClientSession session, MapDesignObject mapObject, MinigameConfiguration minigameConfiguration)
            => session.SendPacket(session.GenerateMloMg(mapObject, minigameConfiguration));

        public static void SendMinigamePoints(this IClientSession session, MinigameConfiguration minigameConfiguration) => session.SendPacket(session.GenerateMinigamePoints(minigameConfiguration));
        public static void SendMinigameReward(this IClientSession session, int vnum, int amount) => session.SendPacket(GenerateMinigameReward(vnum, amount));
        public static void SendMinigameRewardLevel(this IClientSession session, RewardLevel rewardLevel) => session.SendPacket(GenerateMinigameRewardLevel(rewardLevel));

        public static void SendMinigameInfo(this IClientSession session, MapDesignObject mapObj, MinigameConfiguration minigameConfiguration, MinigameScoresHolder minigameScoresHolder)
            => session.SendPacket(mapObj.GenerateMinigameInfo(session, minigameConfiguration, minigameScoresHolder));
    }
}