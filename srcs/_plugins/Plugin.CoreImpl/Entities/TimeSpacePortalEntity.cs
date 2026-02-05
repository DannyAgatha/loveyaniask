using System.Collections.Generic;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Helpers.Damages;
using WingsEmu.Game.Portals;

namespace Plugin.CoreImpl.Entities
{
    public class TimeSpacePortalEntity : ITimeSpacePortalEntity
    {
        public TimeSpacePortalEntity(TimeSpaceFileConfiguration timeSpaceFileConfiguration, Position position, long? groupId)
        {
            TimeSpaceId = timeSpaceFileConfiguration.TsId;
            Position = position;
            IsHero = timeSpaceFileConfiguration.IsHero;
            IsSpecial = timeSpaceFileConfiguration.IsSpecial;
            IsHidden = timeSpaceFileConfiguration.IsHidden;
            MinLevel = timeSpaceFileConfiguration.MinLevel;
            MaxLevel = timeSpaceFileConfiguration.MaxLevel;
            SeedsOfPowerRequired = timeSpaceFileConfiguration.SeedsOfPowerRequired;
            Name = timeSpaceFileConfiguration.Name;
            Description = timeSpaceFileConfiguration.Description;

            DrawRewards = new List<(short, int)>();
            SpecialRewards = new List<(short, int)>();
            BonusRewards = new List<(short, int)>();

            if (timeSpaceFileConfiguration.Rewards?.Draw != null)
            {
                foreach (TimeSpaceItemConfiguration draw in timeSpaceFileConfiguration.Rewards.Draw)
                {
                    DrawRewards.Add((draw.ItemVnum, draw.Amount));
                }
            }

            if (timeSpaceFileConfiguration.Rewards?.Special != null)
            {
                foreach (TimeSpaceItemConfiguration special in timeSpaceFileConfiguration.Rewards.Special)
                {
                    SpecialRewards.Add((special.ItemVnum, special.Amount));
                }
            }

            if (timeSpaceFileConfiguration.Rewards?.Bonus != null)
            {
                foreach (TimeSpaceItemConfiguration bonus in timeSpaceFileConfiguration.Rewards.Bonus)
                {
                    BonusRewards.Add((bonus.ItemVnum, bonus.Amount));
                }
            }

            GroupId = groupId;
        }

        public long TimeSpaceId { get; }
        public Position Position { get; }
        public bool IsHero { get; }
        public bool IsSpecial { get; }
        public bool IsHidden { get; }
        public byte MinLevel { get; }
        public byte MaxLevel { get; }
        public byte SeedsOfPowerRequired { get; }
        public string Name { get; }
        public string Description { get; }
        public List<(short, int)> DrawRewards { get; }
        public List<(short, int)> SpecialRewards { get; }
        public List<(short, int)> BonusRewards { get; }
        public long? GroupId { get; }
    }
}