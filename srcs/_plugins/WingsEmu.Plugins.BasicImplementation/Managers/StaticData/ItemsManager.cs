using System.Collections.Generic;
using System.Linq;
using PhoenixLib.Caching;
using PhoenixLib.Logging;
using WingsAPI.Data.GameData;
using WingsEmu.Core.Extensions;
using WingsEmu.DTOs.Items;
using WingsEmu.Game.Items;
using WingsEmu.Game.Managers.StaticData;
using WingsEmu.Packets.Enums;

namespace NosEmu.Plugins.BasicImplementations.Managers.StaticData;

public class ItemsManager : IItemsManager
{
    private readonly ILongKeyCachedRepository<IGameItem> _cachedItems;
    private readonly ILongKeyCachedRepository<List<IGameItem>> _cachedItemsByType;

    private readonly IResourceLoader<ItemDTO> _itemResourceLoader;
    private readonly IKeyValueCache<List<IGameItem>> _itemsByName;
    private readonly Dictionary<int, int> _titleIdByItemVnum = new();

    public ItemsManager(ILongKeyCachedRepository<IGameItem> cachedItems, ILongKeyCachedRepository<List<IGameItem>> cachedItemsByType, IKeyValueCache<List<IGameItem>> itemsByName,
        IResourceLoader<ItemDTO> itemResourceLoader)
    {
        _cachedItems = cachedItems;
        _cachedItemsByType = cachedItemsByType;
        _itemsByName = itemsByName;
        _itemResourceLoader = itemResourceLoader;
    }

    public void Initialize()
    {
        IEnumerable<ItemDTO> items = _itemResourceLoader.LoadAsync().ConfigureAwait(false).GetAwaiter().GetResult();
        var item = new Dictionary<int, IGameItem>();
        foreach (ItemDTO itemDto in items)
        {
            var newGameItem = new GameItem(itemDto);

            _itemsByName.GetOrSet(itemDto.Name, () => new List<IGameItem>()).Add(newGameItem);
            _cachedItems.Set(itemDto.Id, newGameItem);
            item[itemDto.Id] = newGameItem;
        }

        foreach (IGrouping<ItemType, IGameItem> group in item.Values.GroupBy(x => x.ItemType))
        {
            _cachedItemsByType.Set((int)group.Key, group.ToList());
        }

        var titles = GetItemsByType(ItemType.Title).Select(x => x.Id).ToList();
        for (int i = 0; i < titles.Count; i++)
        {
            int itemVnum = titles[i];
            int titleId = itemVnum - 9300;
            _titleIdByItemVnum[titles[i]] = titleId;
        }

        Log.Info($"[DATABASE] Loaded {item.Count} items.");
    }

    public IGameItem GetItem(int vnum) => _cachedItems.Get(vnum);

    public IEnumerable<IGameItem> GetItemsByType(ItemType type) => _cachedItemsByType.Get((int)type);

    public int GetTitleId(int itemVnum) => _titleIdByItemVnum.GetOrDefault(itemVnum);

    public List<IGameItem> GetItem(string name) => _itemsByName.Get(name);
}