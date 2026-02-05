using System;
using System.Collections.Generic;
using System.Linq;
using WingsEmu.Game._enum;

namespace WingsEmu.Game.Configurations.MysteryBox
{
    public class MysteryBoxRewardsConfiguration
    {
        public List<MysteryBoxEvent> MysteryBoxEvents { get; set; }
        private List<RewardCategory> _currentRewards = [];
        
        public void UpdateCurrentRewards()
        {
            DateTime currentTime = DateTime.UtcNow;
            MysteryBoxEvent activeEvent = MysteryBoxEvents
                .FirstOrDefault(e => currentTime >= e.StartDateTime && currentTime <= e.EndDateTime);
            
            _currentRewards = activeEvent != null ? activeEvent.Rewards : [];
        }
        
        public List<RewardCategory> GetCurrentRewards() => _currentRewards;
        public void ClearCurrentRewards() => _currentRewards = [];
    }
    
    public class MysteryBoxEvent
    {
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public List<RewardCategory> Rewards { get; set; }
    }
    
    public class RewardCategory
    {
        public MysteryBoxRewardType RewardType { get; set; }
        public double  MysteryChance { get; set; }
        public List<RewardItem> Rewards { get; set; }
    }
    
    public class RewardItem
    {
        public int ItemVnum { get; set; }
        public int Amount { get; set; }
    }
}