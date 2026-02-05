using System.Collections.Generic;
using System.Threading.Tasks;
using PhoenixLib.Caching;
using PhoenixLib.Logging;
using WingsAPI.Data.GameData;
using WingsEmu.DTOs.Items;

namespace DiscordNotifier.Managers
{
    public class ItemManager
    {
        private readonly ILongKeyCachedRepository<ItemDTO> _cachedItems;
        private readonly IResourceLoader<ItemDTO> _itemDao;

        public ItemManager(IResourceLoader<ItemDTO> itemDao, ILongKeyCachedRepository<ItemDTO> cachedItems)
        {
            _itemDao = itemDao;
            _cachedItems = cachedItems;
        }

        public async Task CacheClientItems()
        {
            Log.Info("[ITEM_MANAGER] Caching items from DB");
            IEnumerable<ItemDTO> items = await _itemDao.LoadAsync();

            int count = 0;
            foreach (ItemDTO item in items)
            {
                _cachedItems.Set(item.Id, item);
                count++;
            }

            Log.Info($"[ITEM_MANAGER] Cached: {count.ToString()} items");
        }

        public int GetItemIconIdByItemId(int itemId)
        {
            ItemDTO cachedItem = _cachedItems.Get(itemId);
            return cachedItem?.IconId ?? default;
        }

        public ItemDTO GetItemDtoByItemId(int itemId) => _cachedItems.Get(itemId);
    }
}