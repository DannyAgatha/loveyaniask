using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsAPI.Game.Extensions.ItemExtension.Item;
using WingsAPI.Game.Extensions.PacketGeneration;
using WingsEmu.Game;
using WingsEmu.Game._enum;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Configurations.LegendaryDrop;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Helpers.Damages;
using WingsEmu.Game.Inventory.Event;
using WingsEmu.Game.Items;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Managers.StaticData;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Mates;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Raids;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;

namespace NosEmu.Plugins.BasicImplementations.Event.Items;

public class ThrowItemEventHandler : IAsyncEventProcessor<ThrowItemEvent>
{
    private readonly IGameItemInstanceFactory _gameItem;
    private readonly IRandomGenerator _randomGenerator;

    public ThrowItemEventHandler(IGameItemInstanceFactory gameItem, IRandomGenerator randomGenerator)
    {
        _gameItem = gameItem;
        _randomGenerator = randomGenerator;
    }

    public async Task HandleAsync(ThrowItemEvent e, CancellationToken cancellation)
    {
        GameItemInstance newItem = _gameItem.CreateItem(e.ItemVnum, e.Quantity);

        int rndX = e.BattleEntity.PositionX + _randomGenerator.RandomNumber(e.MinimumDistance, e.MaximumDistance + 1) * (_randomGenerator.RandomNumber(0, 2) * 2 - 1);
        int rndY = e.BattleEntity.PositionY + _randomGenerator.RandomNumber(e.MinimumDistance, e.MaximumDistance + 1) * (_randomGenerator.RandomNumber(0, 2) * 2 - 1);

        var position = new Position((short)rndX, (short)rndY);

        var item = new MonsterMapItem(position.X, position.Y, newItem, e.BattleEntity.MapInstance);

        e.BattleEntity.MapInstance.AddDrop(item);
        e.BattleEntity.BroadcastThrow(item);
    }
}

public class DropItemEventHandler : IAsyncEventProcessor<DropMapItemEvent>
{
    private readonly IGameItemInstanceFactory _gameItem;
    private readonly ISessionManager _sessionManager; 
    private readonly LegendaryDropConfiguration _legendaryDropConfiguration;
    private readonly IGameLanguageService _gameLanguageService;
    private readonly IItemsManager _itemsManager;
    public DropItemEventHandler(IGameItemInstanceFactory gameItem, ISessionManager sessionManager, LegendaryDropConfiguration legendaryDropConfiguration, IGameLanguageService gameLanguageService,
        IItemsManager itemsManager)
    {
        _gameItem = gameItem;
        _sessionManager = sessionManager;
        _legendaryDropConfiguration = legendaryDropConfiguration;
        _gameLanguageService = gameLanguageService;
        _itemsManager = itemsManager;
    }

    public async Task HandleAsync(DropMapItemEvent e, CancellationToken cancellation)
    {
        IClientSession session = e.Session;
        IMapInstance map = e.Map;
        GameItemInstance newItem = _gameItem.CreateItem(e.Vnum, e.Amount, (byte)e.Upgrade, (sbyte)e.Rarity, (byte)e.Design);
        var item = new MonsterMapItem(e.Position.X, e.Position.Y, newItem, e.Map, e.OwnerId, e.IsQuest);
        IMateEntity mate = session.PlayerEntity.MateComponent.GetTeamMember(x => x.MateType == MateType.Pet && x.HasDhaPremium);
        mate?.SavedDrops.Add(item.TransportId);
        map.AddDrop(item);
        item.BroadcastDrop();
        
        if (_legendaryDropConfiguration.LegendaryItems.Any(group => group.ItemVnums.Contains(item.ItemVNum)))
        {
            string itemName = _itemsManager.GetItem((short)item.ItemVNum)?.GetItemName(_gameLanguageService, session.UserLanguage);

            if (!string.IsNullOrEmpty(itemName))
            {
                session.SendChatMessage(session.GetLanguageFormat(GameDialogKey.LEGENDARY_ITEM_DROPPED, itemName, item.Amount), ChatMessageColorType.Orange);
            }
        }
    }
}