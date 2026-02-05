using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsAPI.Game.Extensions.ItemExtension.Item;
using WingsEmu.Game._enum;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Managers.StaticData;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Revival;
using WingsEmu.Game.TimeSpaces;
using WingsEmu.Game.TimeSpaces.Events;
using WingsEmu.Packets.Enums.Chat;

namespace Plugin.TimeSpaces.Handlers;

public class TimeSpaceLeavePartyEventHandler : IAsyncEventProcessor<TimeSpaceLeavePartyEvent>
{
    private readonly IGameLanguageService _gameLanguage;
    private readonly GameMinMaxConfiguration _gameMinMaxConfiguration;
    private readonly IItemsManager _itemsManager;
    private readonly IRankingManager _rankingManager;
    private readonly IReputationConfiguration _reputationConfiguration;

    public TimeSpaceLeavePartyEventHandler(IItemsManager itemsManager, IGameLanguageService gameLanguage,
        GameMinMaxConfiguration gameMinMaxConfiguration, IReputationConfiguration reputationConfiguration, IRankingManager rankingManager)
    {
        _itemsManager = itemsManager;
        _gameLanguage = gameLanguage;
        _gameMinMaxConfiguration = gameMinMaxConfiguration;
        _reputationConfiguration = reputationConfiguration;
        _rankingManager = rankingManager;
    }

    public async Task HandleAsync(TimeSpaceLeavePartyEvent e, CancellationToken cancellation)
    {
        IClientSession session = e.Sender;

        if (!session.PlayerEntity.TimeSpaceComponent.IsInTimeSpaceParty)
        {
            return;
        }

        TimeSpaceParty timeSpace = session.PlayerEntity.TimeSpaceComponent.TimeSpace;
        if (timeSpace?.Instance == null)
        {
            return;
        }

        if (e.CheckFinished && !timeSpace.Finished)
        {
            return;
        }

        if (timeSpace.Started && e.CheckForSeeds)
        {
            TimeSpaceSubInstance getCurrentInstance = timeSpace.Instance.TimeSpaceSubInstances.TryGetValue(session.CurrentMapInstance.Id, out TimeSpaceSubInstance instance) ? instance : null;
            if (getCurrentInstance != null && timeSpace.Instance.SpawnInstance != getCurrentInstance)
            {
                if (!session.PlayerEntity.HasItem((short)ItemVnums.SEED_OF_POWER, 5))
                {
                    string itemName = _itemsManager.GetItem((short)ItemVnums.SEED_OF_POWER).GetItemName(_gameLanguage, session.UserLanguage);
                    session.SendChatMessage(session.GetLanguageFormat(GameDialogKey.INVENTORY_SHOUTMESSAGE_NOT_ENOUGH_ITEMS, 5, itemName), ChatMessageColorType.PlayerSay);
                    return;
                }

                await session.RemoveItemFromInventory((short)ItemVnums.SEED_OF_POWER, 5);
            }

            int dignityToRemove = session.PlayerEntity.Level < 20 ? session.PlayerEntity.Level : 20;
            await session.PlayerEntity.RemoveDignity(dignityToRemove, _gameMinMaxConfiguration, _gameLanguage, _reputationConfiguration, _rankingManager.TopReputation);
        }

        if (!session.PlayerEntity.IsAlive())
        {
            await session.EmitEventAsync(new RevivalReviveEvent());
        }

        if (timeSpace.Finished)
        {
            session.EmitEvent(new TimeSpaceSelectRewardEvent());
            timeSpace.RemoveMember(session);
            session.PlayerEntity.TimeSpaceComponent.RemoveTimeSpaceParty();
            session.ChangeToLastBaseMap();
            return;
        }

        if (e.RemoveLive)
        {
            await session.EmitEventAsync(new TimeSpaceDecreaseLiveEvent());
            if (!session.PlayerEntity.TimeSpaceComponent.IsInTimeSpaceParty)
            {
                return;
            }
        }

        timeSpace.RemoveMember(session);
        session.PlayerEntity.TimeSpaceComponent.RemoveTimeSpaceParty();
        session.ChangeToLastBaseMap();
    }
}