using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsAPI.Data.Character;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsAPI.Game.Extensions.ItemExtension.Item;
using WingsEmu.Game._ECS.Systems;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Items;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums.Chat;

namespace NosEmu.Plugins.BasicImplementations.Event.Characters;

public class DailyRewardEventHandler : IAsyncEventProcessor<DailyRewardEvent>
{
    private readonly DailyRewardsConfiguration _dailyRewardsConfiguration;
    private readonly IGameItemInstanceFactory _gameItemInstanceFactory;
    private readonly IGameLanguageService _languageService;

    public DailyRewardEventHandler(DailyRewardsConfiguration dailyRewardsConfiguration, IGameItemInstanceFactory gameItemInstanceFactory, IGameLanguageService languageService)
    {
        _dailyRewardsConfiguration = dailyRewardsConfiguration;
        _gameItemInstanceFactory = gameItemInstanceFactory;
        _languageService = languageService;
    }

    public async Task HandleAsync(DailyRewardEvent e, CancellationToken cancellation)
    {
        IClientSession session = e.Sender;

        // items
        foreach (DailyRewardItem reward in _dailyRewardsConfiguration.Rewards.Items)
        {
            if (!TryParseRewardTime(reward.Time, out TimeSpan rewardTimeSpan) || e.TimeSinceGameStart < rewardTimeSpan)
            {
                continue;
            }

            if (!AddLogs(session, e.CharacterSystem, reward.Time, "items"))
            {
                continue;
            }

            GameItemInstance r = _gameItemInstanceFactory.CreateItem(reward.ItemVnum, reward.Amount);
            await session.AddNewItemToInventory(r, sendGiftIsFull: true);
            
            string itemName = r.GameItem.GetItemName(_languageService, e.Sender.UserLanguage);
            string message = _languageService.GetLanguageFormat(GameDialogKey.CONGRATS_RECEIVED_ITEMS, e.Sender.UserLanguage, itemName, reward.Amount);
            session.SendChatMessage(message, ChatMessageColorType.Green);
        }

        // reputation
        foreach (DailyRewardReputation reward in _dailyRewardsConfiguration.Rewards.Reputations)
        {
            if (!TryParseRewardTime(reward.Time, out TimeSpan rewardTimeSpan) || e.TimeSinceGameStart < rewardTimeSpan)
            {
                continue;
            }

            if (!AddLogs(session, e.CharacterSystem, reward.Time, "reputation"))
            {
                continue;
            }

            await session.EmitEventAsync(new GenerateReputationEvent
            {
                Amount = reward.Amount,
                SendMessage = true
            });
            
            string message = _languageService.GetLanguageFormat(GameDialogKey.CONGRATS_RECEIVED_REPUTATION, e.Sender.UserLanguage, reward.Amount);
            session.SendChatMessage(message, ChatMessageColorType.Green);
        }

        // gold
        foreach (DailyRewardGold reward in _dailyRewardsConfiguration.Rewards.Golds)
        {
            if (!TryParseRewardTime(reward.Time, out TimeSpan rewardTimeSpan) || e.TimeSinceGameStart < rewardTimeSpan)
            {
                continue;
            }

            if (!AddLogs(session, e.CharacterSystem, reward.Time,"gold"))
            {
                continue;
            }

            await session.EmitEventAsync(new GenerateGoldEvent(reward.Amount, fallBackToBank: true));
            string message = _languageService.GetLanguageFormat(GameDialogKey.CONGRATS_RECEIVED_GOLD, e.Sender.UserLanguage, reward.Amount);
            session.SendChatMessage(message, ChatMessageColorType.Green);
        }
    }

    private bool AddLogs(IClientSession session, ICharacterSystem characterSystem, string time, string type)
    {
        bool exist = characterSystem.GetCharacters().Any(s => s.DailyRewardDto.Any(x => x.IpAddress == session.IpAddress && x.Time == time && x.Type == type));

        if (exist)
        {
            return false;
        }

        session.PlayerEntity.DailyRewardDto.Add(new DailyRewardDto
        {
            IpAddress = session.IpAddress,
            Time = time,
            Type = type
        });
        return true;
    }

    private bool TryParseRewardTime(string time, out TimeSpan timeSpan) => TimeSpan.TryParse(time, out timeSpan);
}