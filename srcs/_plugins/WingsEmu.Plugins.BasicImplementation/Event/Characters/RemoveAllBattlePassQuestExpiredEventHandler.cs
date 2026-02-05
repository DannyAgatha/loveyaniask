using PhoenixLib.Events;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WingsAPI.Data.BattlePass;
using WingsAPI.Packets.Enums.BattlePass;
using WingsEmu.Game.BattlePass;
using WingsEmu.Game.Characters.Events;

namespace NosEmu.Plugins.BasicImplementations.Event.Characters
{
    public class RemoveAllBattlePassQuestExpiredEventHandler : IAsyncEventProcessor<RemoveAllBattlePassQuestExpiredEvent>
    {
        private readonly BattlePassQuestConfiguration _battlepassQuestConfiguration;
        private readonly BattlePassConfiguration _battlepassConfiguration;

        public RemoveAllBattlePassQuestExpiredEventHandler(BattlePassQuestConfiguration battlePassQuestConfiguration, BattlePassConfiguration battlePassConfiguration)
        {
            _battlepassQuestConfiguration = battlePassQuestConfiguration;
            _battlepassConfiguration = battlePassConfiguration;
        }

        public static bool IsQuestExpired(BattlePassQuest quest, DateTime endSeason)
        {
            if (quest == null)
            {
                return true;
            }

            return GetTotalMinutesBeforeQuestEnd(quest, endSeason) <= 0;
        }

        public static double GetTotalMinutesBeforeQuestEnd(BattlePassQuest quest, DateTime endSeason)
        {
            TimeSpan timeSpan = TimeSpan.Zero;

            switch (quest.FrequencyType)
            {
                case FrequencyType.Daily:
                    timeSpan = quest.Start.AddDays(1) - DateTime.Now;
                    break;

                case FrequencyType.Weekly:
                    timeSpan = quest.Start.AddDays(7) - DateTime.Now;
                    break;

                case FrequencyType.Seasonal:
                    timeSpan = endSeason - DateTime.Now;
                    break;
            }

            if (quest.Start > DateTime.Now)
            {
                timeSpan = TimeSpan.Zero;
            }

            return timeSpan.TotalMinutes;
        }

        public async Task HandleAsync(RemoveAllBattlePassQuestExpiredEvent e, CancellationToken cancellation)
        {
            foreach (BattlePassQuest i in _battlepassQuestConfiguration.Quests.Where(s => IsQuestExpired(s, _battlepassConfiguration.EndSeason)))
            {
                BattlePassQuestDto questLog = e.Sender.PlayerEntity.BattlePassQuestDto.FirstOrDefault(s => s.QuestId == i.Id);

                if (questLog == null)
                {
                    continue;
                }

                e.Sender.PlayerEntity.BattlePassQuestDto.Remove(questLog);
            }
        }
    }
}