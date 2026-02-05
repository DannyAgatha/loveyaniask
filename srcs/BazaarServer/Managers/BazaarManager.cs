using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Caching;
using WingsAPI.Communication.Bazaar;
using WingsAPI.Data.Bazaar;
using WingsAPI.Game.Extensions.Bazaar;
using WingsAPI.Packets.Enums.Bazaar;
using WingsEmu.Core.Extensions;
using WingsEmu.Core.Generics;
using WingsEmu.DTOs.Bazaar;

namespace BazaarServer.Managers
{
    public class BazaarManager
    {
        private readonly IBazaarItemDAO _bazaarItemDao;
        private readonly BazaarSearchManager _bazaarSearchManager;

        private readonly ILongKeyCachedRepository<BazaarItemDTO> _itemsCache;
        private readonly ConcurrentDictionary<long, ThreadSafeHashSet<long>> _itemsIdsByCharacterId = new();

        private readonly SemaphoreSlim _semaphoreSlim = new(1, 1);

        public BazaarManager(IBazaarItemDAO bazaarItemDao, ILongKeyCachedRepository<BazaarItemDTO> itemsCache, BazaarSearchManager bazaarSearchManager)
        {
            _bazaarItemDao = bazaarItemDao;
            _itemsCache = itemsCache;
            _bazaarSearchManager = bazaarSearchManager;
        }

        public async Task<long> CacheAllItemsInDb()
        {
            IEnumerable<BazaarItemDTO> items = await _bazaarItemDao.GetAllNonDeletedBazaarItems();
            int count = 0;
            foreach (BazaarItemDTO item in items)
            {
                AddToCache(item);
                count++;
            }

            await _bazaarSearchManager.Initialize(items);

            return count;
        }

        private void AddToCache(BazaarItemDTO item)
        {
            _itemsCache.Set(item.Id, item);
            _itemsIdsByCharacterId.GetOrAdd(item.CharacterId, new ThreadSafeHashSet<long>()).Add(item.Id);
        }

        private void RemoveFromCache(BazaarItemDTO item)
        {
            _itemsCache.Remove(item.Id);
            _itemsIdsByCharacterId.GetOrDefault(item.CharacterId)?.Remove(item.Id);
        }

        public async Task<BazaarItemDTO> SaveAsync(BazaarItemDTO item)
        {
            await _semaphoreSlim.WaitAsync();
            try
            {
                BazaarItemDTO dto = await _bazaarItemDao.SaveAsync(item);
                AddToCache(dto);
                _bazaarSearchManager.AddItem(dto);
                return dto;
            }
            finally
            {
                _semaphoreSlim.Release();
            }
        }

        public async Task<BazaarItemDTO> GetBazaarItemById(long bazaarItemId)
        {
            await _semaphoreSlim.WaitAsync();
            try
            {
                return _itemsCache.Get(bazaarItemId);
            }
            finally
            {
                _semaphoreSlim.Release();
            }
        }

        public ICollection<BazaarItemDTO> GetItemsByCharacterId(long characterId)
        {
            return _itemsIdsByCharacterId.GetOrDefault(characterId)?.Select(s => _itemsCache.Get(s)).ToList();
        }

        public async Task<BazaarItemDTO> DeleteItemWithDto(BazaarItemDTO item, long requesterCharacterId)
        {
            await _semaphoreSlim.WaitAsync();
            try
            {
                BazaarItemDTO cachedItem = _itemsCache.Get(item.Id);
                if (cachedItem == null || cachedItem.CharacterId != requesterCharacterId)
                {
                    return null;
                }

                await _bazaarItemDao.DeleteByIdAsync(item.Id);
                RemoveFromCache(item);
                _bazaarSearchManager.RemoveItem(item);
                return cachedItem;
            }
            finally
            {
                _semaphoreSlim.Release();
            }
        }

        public async Task<BazaarItemDTO> ChangeItemPriceWithDto(BazaarItemDTO bazaarItemDto, long characterId, long price, long saleFee)
        {
            await _semaphoreSlim.WaitAsync();
            try
            {
                BazaarItemDTO cachedItem = _itemsCache.Get(bazaarItemDto.Id);
                if (cachedItem == null || cachedItem.SoldAmount > 0 || characterId != cachedItem.CharacterId || cachedItem.GetBazaarItemStatus() != BazaarListedItemType.ForSale
                    || BazaarExtensions.PriceOrAmountExceeds(bazaarItemDto.UsedMedal, price, bazaarItemDto.Amount))
                {
                    return null;
                }

                cachedItem.PricePerItem = price;
                cachedItem.SaleFee = saleFee;
                BazaarItemDTO savedItem = await _bazaarItemDao.SaveAsync(cachedItem);
                return savedItem;
            }
            finally
            {
                _semaphoreSlim.Release();
            }
        }

        public IReadOnlyCollection<BazaarItemDTO> SearchBazaarItems(BazaarSearchContext bazaarSearchContext) => _bazaarSearchManager.SearchBazaarItems(bazaarSearchContext);

        public async Task<BazaarItemDTO> BuyItemWithExpectedValues(long bazaarItemId, long buyerCharacterId, int  amount, long pricePerItem)
        {
            await _semaphoreSlim.WaitAsync();
            try
            {
                BazaarItemDTO cachedItem = _itemsCache.Get(bazaarItemId);
                if (cachedItem == null || buyerCharacterId == cachedItem.CharacterId || amount < 1 || cachedItem.Amount - cachedItem.SoldAmount < amount || pricePerItem != cachedItem.PricePerItem
                    || cachedItem.IsPackage && amount != cachedItem.Amount || cachedItem.GetBazaarItemStatus() != BazaarListedItemType.ForSale)
                {
                    return null;
                }

                cachedItem.SoldAmount += amount;
                BazaarItemDTO savedItem = await _bazaarItemDao.SaveAsync(cachedItem);
                return savedItem;
            }
            finally
            {
                _semaphoreSlim.Release();
            }
        }

        public async Task<List<BazaarItemDTO>> UnlistItemsWithVnums(List<BazaarItemDTO> items)
        {
            await _semaphoreSlim.WaitAsync();
            try
            {
                foreach (BazaarItemDTO bazaarItemDto in items)
                {
                    bazaarItemDto.ExpiryDate = DateTime.UtcNow.AddDays(-1);
                    BazaarItemDTO dto = await _bazaarItemDao.SaveAsync(bazaarItemDto);
                    AddToCache(dto);
                    _bazaarSearchManager.AddItem(dto);
                }

                return items;
            }
            finally
            {
                _semaphoreSlim.Release();
            }
        }
    }
}