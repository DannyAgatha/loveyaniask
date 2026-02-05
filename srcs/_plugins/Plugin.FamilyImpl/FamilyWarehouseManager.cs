using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Caching;
using PhoenixLib.Logging;
using WingsAPI.Communication;
using WingsAPI.Communication.Families.Warehouse;
using WingsAPI.Data.Families;
using WingsAPI.Game.Extensions;
using WingsEmu.Core.Extensions;
using WingsEmu.DTOs.Items;
using WingsEmu.Game._enum;

namespace Plugin.FamilyImpl
{
    public class FamilyWarehouseManager : IFamilyWarehouseManager
    {
        private const int MaximumAmountOfLogs = 200;
        private static readonly TimeSpan LifeTime = TimeSpan.FromMinutes(15);

        private readonly ILongKeyCachedRepository<Dictionary<short, FamilyWarehouseItemDto>> _cachedFamilyItems;

        private readonly ILongKeyCachedRepository<List<FamilyWarehouseLogEntryDto>> _cachedFamilyLogs;
        private readonly ConcurrentDictionary<long, SemaphoreSlim> _familyItemLocks = new();
        private readonly ConcurrentDictionary<long, SemaphoreSlim> _familyLogsLocks = new();

        private readonly IFamilyWarehouseService _familyWarehouseService;

        public FamilyWarehouseManager(ILongKeyCachedRepository<Dictionary<short, FamilyWarehouseItemDto>> cachedFamilyItems,
            ILongKeyCachedRepository<List<FamilyWarehouseLogEntryDto>> cachedFamilyLogs, IFamilyWarehouseService familyWarehouseService)
        {
            _cachedFamilyItems = cachedFamilyItems;
            _cachedFamilyLogs = cachedFamilyLogs;
            _familyWarehouseService = familyWarehouseService;
        }

        public async Task<(IList<FamilyWarehouseLogEntryDto>, ManagerResponseType?)> GetWarehouseLogs(long familyId, long characterId)
        {
            SemaphoreSlim familyLogSemaphore = GetFamilyLogSemaphore(familyId);

            await familyLogSemaphore.WaitAsync();
            try
            {
                return await GetWarehouseLogsWithoutLock(familyId, characterId);
            }
            finally
            {
                familyLogSemaphore.Release();
            }
        }

        public async Task<(IDictionary<short, FamilyWarehouseItemDto> familyWarehouseItemDtos, ManagerResponseType?)> GetWarehouse(long familyId, long characterId)
        {
            SemaphoreSlim familyItemSemaphore = GetFamilyItemSemaphore(familyId);

            await familyItemSemaphore.WaitAsync();
            try
            {
                return await GetWarehouseWithoutLock(familyId, characterId);
            }
            finally
            {
                familyItemSemaphore.Release();
            }
        }

        public async Task<(FamilyWarehouseItemDto, ManagerResponseType?)> GetWarehouseItem(long familyId, short slot, long characterId)
        {
            (IDictionary<short, FamilyWarehouseItemDto> familyWarehouseItemDtos, ManagerResponseType? responseType) = await GetWarehouse(familyId, characterId);
            return (familyWarehouseItemDtos.GetOrDefault(slot), responseType);
        }

        public async Task<ManagerResponseType?> AddWarehouseItem(FamilyWarehouseItemDto warehouseItemDtoToAdd, long characterId, string characterName)
        {
            FamilyWarehouseAddItemResponse response = null;

            try
            {
                response = await _familyWarehouseService.AddItem(new FamilyWarehouseAddItemRequest
                {
                    CharacterId = characterId,
                    CharacterName = characterName,
                    Item = warehouseItemDtoToAdd
                });
            }
            catch (Exception ex)
            {
                Log.Error("[FAMILY_WAREHOUSE_MANAGER][ADD_ITEM] ", ex);
            }

            return response?.ResponseType.ToManagerType();
        }

        public async Task<(ItemInstanceDTO, ManagerResponseType?)> WithdrawWarehouseItem(FamilyWarehouseItemDto warehouseItemDtoToWithdraw, int amount, long characterId, string characterName)
        {
            FamilyWarehouseWithdrawItemResponse response = null;

            try
            {
                response = await _familyWarehouseService.WithdrawItem(new FamilyWarehouseWithdrawItemRequest
                {
                    ItemToWithdraw = warehouseItemDtoToWithdraw,
                    Amount = amount,
                    CharacterId = characterId,
                    CharacterName = characterName
                });
            }
            catch (Exception ex)
            {
                Log.Error("[FAMILY_WAREHOUSE_MANAGER][WITHDRAW_ITEM] ", ex);
            }

            return (response?.WithdrawnItem, response?.ResponseType.ToManagerType());
        }

        public async Task<ManagerResponseType?> MoveWarehouseItem(FamilyWarehouseItemDto warehouseItemDtoToMove, int amount, short newSlot, long characterId)
        {
            FamilyWarehouseMoveItemResponse response = null;

            try
            {
                response = await _familyWarehouseService.MoveItem(new FamilyWarehouseMoveItemRequest
                {
                    WarehouseItemDtoToMove = warehouseItemDtoToMove,
                    Amount = amount,
                    NewSlot = newSlot,
                    CharacterId = characterId
                });
            }
            catch (Exception ex)
            {
                Log.Error("[FAMILY_WAREHOUSE_MANAGER][MOVE_ITEM] ", ex);
            }

            return response?.ResponseType.ToManagerType();
        }

        public async Task UpdateWarehouseItem(long familyId, IEnumerable<(FamilyWarehouseItemDto, short)> warehouseItemDtosToUpdate)
        {
            SemaphoreSlim familyItemSemaphore = GetFamilyItemSemaphore(familyId);

            await familyItemSemaphore.WaitAsync();
            try
            {
                (IDictionary<short, FamilyWarehouseItemDto> items, ManagerResponseType? responseType) = await GetWarehouseWithoutLock(familyId);
                if (responseType != ManagerResponseType.Success)
                {
                    return;
                }

                foreach ((FamilyWarehouseItemDto dto, short slot) in warehouseItemDtosToUpdate)
                {
                    if (dto == null)
                    {
                        items?.Remove(slot);
                        continue;
                    }

                    items ??= new Dictionary<short, FamilyWarehouseItemDto>();
                    items[slot] = dto;
                }
            }
            finally
            {
                familyItemSemaphore.Release();
            }
        }

        public async Task AddWarehouseLog(long familyId, FamilyWarehouseLogEntryDto log)
        {
            SemaphoreSlim familyItemSemaphore = GetFamilyLogSemaphore(familyId);

            await familyItemSemaphore.WaitAsync();
            try
            {
                (List<FamilyWarehouseLogEntryDto> logs, ManagerResponseType? responseType) = await GetWarehouseLogsWithoutLock(familyId);
                if (responseType != ManagerResponseType.Success)
                {
                    return;
                }

                logs.Add(log);
                if (logs.Count <= MaximumAmountOfLogs)
                {
                    return;
                }

                logs.RemoveRange(0, logs.Count - MaximumAmountOfLogs);
            }
            finally
            {
                familyItemSemaphore.Release();
            }
        }

        private SemaphoreSlim GetFamilyItemSemaphore(long familyId) => _familyItemLocks.GetOrAdd(familyId, new SemaphoreSlim(1, 1));

        private SemaphoreSlim GetFamilyLogSemaphore(long familyId) => _familyLogsLocks.GetOrAdd(familyId, new SemaphoreSlim(1, 1));

        private async Task<(List<FamilyWarehouseLogEntryDto>, ManagerResponseType?)> GetWarehouseLogsWithoutLock(long familyId, long? characterId = null)
        {
            List<FamilyWarehouseLogEntryDto> retrievedLogs = _cachedFamilyLogs.Get(familyId);

            if (retrievedLogs != null)
            {
                return (retrievedLogs, ManagerResponseType.Success);
            }

            FamilyWarehouseGetLogsResponse response = null;

            try
            {
                response = await _familyWarehouseService.GetLogs(new FamilyWarehouseGetLogsRequest
                {
                    FamilyId = familyId,
                    CharacterId = characterId
                });
            }
            catch (Exception ex)
            {
                Log.Error("[FAMILY_WAREHOUSE_MANAGER][GET_LOGS] ", ex);
            }

            var logs = response?.Logs?.ToList();

            if (response?.ResponseType == RpcResponseType.SUCCESS)
            {
                _cachedFamilyLogs.Set(familyId, logs ?? new List<FamilyWarehouseLogEntryDto>(), LifeTime);
            }

            return (logs, response?.ResponseType.ToManagerType());
        }

        private async Task<(IDictionary<short, FamilyWarehouseItemDto>, ManagerResponseType?)> GetWarehouseWithoutLock(long familyId, long? characterId = null)
        {
            Dictionary<short, FamilyWarehouseItemDto> retrievedItems = _cachedFamilyItems.Get(familyId);

            if (retrievedItems != null)
            {
                _cachedFamilyItems.Set(familyId, retrievedItems, LifeTime);
                return (retrievedItems, ManagerResponseType.Success);
            }

            FamilyWarehouseGetItemsResponse response = null;

            try
            {
                response = await _familyWarehouseService.GetItems(new FamilyWarehouseGetItemsRequest
                {
                    FamilyId = familyId,
                    CharacterId = characterId
                });
            }
            catch (Exception ex)
            {
                Log.Error("[FAMILY_WAREHOUSE_MANAGER][GET_ITEMS] ", ex);
            }

            Dictionary<short, FamilyWarehouseItemDto> dictionary = response?.Items?.ToDictionary(x => x.Slot) ?? new Dictionary<short, FamilyWarehouseItemDto>();

            if (response?.ResponseType == RpcResponseType.SUCCESS)
            {
                _cachedFamilyItems.Set(familyId, dictionary, LifeTime);
            }

            return (dictionary, response?.ResponseType.ToManagerType());
        }
    }
}