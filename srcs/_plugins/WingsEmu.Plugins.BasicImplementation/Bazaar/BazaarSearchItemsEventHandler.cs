using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsAPI.Communication;
using WingsAPI.Communication.Bazaar;
using WingsAPI.Game.Extensions.PacketGeneration;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Algorithm;
using WingsEmu.Game.Bazaar;
using WingsEmu.Game.Bazaar.Configuration;
using WingsEmu.Game.Bazaar.Events;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Managers.StaticData;

namespace NosEmu.Plugins.BasicImplementations.Bazaar;

public class BazaarSearchItemsEventHandler : IAsyncEventProcessor<BazaarSearchItemsEvent>
{
    private readonly BazaarConfiguration _bazaarConfiguration;
    private readonly IBazaarManager _bazaarManager;
    private readonly ICharacterAlgorithm _characterAlgorithm;
    private readonly IItemsManager _itemsManager;

    public BazaarSearchItemsEventHandler(BazaarConfiguration bazaarConfiguration, IBazaarManager bazaarManager, IItemsManager itemsManager, ICharacterAlgorithm characterAlgorithm)
    {
        _bazaarConfiguration = bazaarConfiguration;
        _bazaarManager = bazaarManager;
        _itemsManager = itemsManager;
        _characterAlgorithm = characterAlgorithm;
    }

    public async Task HandleAsync(BazaarSearchItemsEvent e, CancellationToken cancellation)
    {
        (IReadOnlyCollection<BazaarItem> items, RpcResponseType rpcResponseType) = await _bazaarManager.SearchBazaarItems(new BazaarSearchContext
        {
            Index = e.Index,
            AmountOfItemsPerIndex = _bazaarConfiguration.MaximumListedItems,
            CategoryFilterType = e.CategoryFilterType,
            ItemVNumFilter = e.ItemVNumFilter,
            LevelFilter = e.LevelFilter,
            OrderFilter = e.OrderFilter,
            RareFilter = e.RareFilter,
            SubTypeFilter = e.SubTypeFilter,
            UpgradeFilter = e.UpgradeFilter
        });

        if (rpcResponseType != RpcResponseType.SUCCESS)
        {
            if (rpcResponseType != RpcResponseType.MAINTENANCE_MODE)
            {
                return;
            }

            e.Sender.SendInfo(e.Sender.GetLanguage(GameDialogKey.BAZAAR_INFO_MAINTENANCE_MODE));
            return;
        }

        e.Sender.SendSearchResponseBazaarItems(e.Index, items, _itemsManager, _characterAlgorithm, _bazaarConfiguration);
    }
}