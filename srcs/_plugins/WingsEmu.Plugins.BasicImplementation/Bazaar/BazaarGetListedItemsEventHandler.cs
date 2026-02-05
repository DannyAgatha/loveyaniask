using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsAPI.Game.Extensions.PacketGeneration;
using WingsEmu.Game.Algorithm;
using WingsEmu.Game.Bazaar;
using WingsEmu.Game.Bazaar.Configuration;
using WingsEmu.Game.Bazaar.Events;
using WingsEmu.Game.Managers.StaticData;

namespace NosEmu.Plugins.BasicImplementations.Bazaar;

public class BazaarGetListedItemsEventHandler : IAsyncEventProcessor<BazaarGetListedItemsEvent>
{
    private readonly BazaarConfiguration _bazaarConfiguration;
    private readonly IBazaarManager _bazaarManager;
    private readonly ICharacterAlgorithm _characterAlgorithm;
    private readonly IItemsManager _itemsManager;

    public BazaarGetListedItemsEventHandler(IItemsManager itemsManager, ICharacterAlgorithm characterAlgorithm, IBazaarManager bazaarManager, BazaarConfiguration bazaarConfiguration)
    {
        _itemsManager = itemsManager;
        _characterAlgorithm = characterAlgorithm;
        _bazaarManager = bazaarManager;
        _bazaarConfiguration = bazaarConfiguration;
    }

    public async Task HandleAsync(BazaarGetListedItemsEvent e, CancellationToken cancellation)
    {
        IReadOnlyCollection<BazaarItem> items = await _bazaarManager.GetListedItemsByCharacterId(e.Sender.PlayerEntity.Id);

        e.Sender.SendCharacterListedBazaarItems(e.Index, items, e.Filter, _itemsManager, _characterAlgorithm, _bazaarConfiguration);
    }
}