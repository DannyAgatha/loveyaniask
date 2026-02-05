using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Mapster;
using Microsoft.Extensions.Hosting;
using PhoenixLib.Caching;
using PhoenixLib.Logging;
using WingsAPI.Data.Account;
using WingsAPI.Data.Warehouse;
using WingsEmu.Core.Extensions;
using WingsEmu.DTOs.Items;
using WingsEmu.Game.Configurations;

namespace DatabaseServer.Managers
{
    public class AccountWarehouseManager : BackgroundService, IAccountWarehouseManager
    {
        private static readonly TimeSpan Interval = TimeSpan.FromMinutes(Convert.ToUInt32(Environment.GetEnvironmentVariable(EnvironmentConsts.DbServerSaveIntervalMinutes) ?? "5"));
        private static readonly TimeSpan LifeTime = Interval * 3;

        private readonly IAccountWarehouseItemDao _accountWarehouseItemDao;

        private readonly ILongKeyCachedRepository<Dictionary<short, AccountWarehouseItemDto>> _cachedWarehouseItems;

        private readonly Dictionary<long, Dictionary<short, (AccountWarehouseItemDto dto, bool remove)>> _itemChanges = new();
        private readonly SemaphoreSlim _itemChangesSemaphore = new(1, 1);
        private readonly ConcurrentDictionary<long, SemaphoreSlim> _warehouseLocks = new();
        private static GeneralServerConfiguration _generalServerConfiguration;
        
        public AccountWarehouseManager(IAccountWarehouseItemDao accountWarehouseItemDao, ILongKeyCachedRepository<Dictionary<short, AccountWarehouseItemDto>> cachedWarehouseItems, GeneralServerConfiguration generalServerConfiguration)
        {
            _accountWarehouseItemDao = accountWarehouseItemDao;
            _cachedWarehouseItems = cachedWarehouseItems;
            _generalServerConfiguration = generalServerConfiguration;
        }

        public async Task<IEnumerable<AccountWarehouseItemDto>> GetWarehouse(long accountId)
        {
            SemaphoreSlim accountLock = GetAccountLock(accountId);
            await accountLock.WaitAsync();
            try
            {
                return (await GetAccountWarehouse(accountId))?.Values;
            }
            finally
            {
                accountLock.Release();
            }
        }

        public async Task<AccountWarehouseItemDto> GetWarehouseItem(long accountId, short slot)
        {
            SemaphoreSlim accountLock = GetAccountLock(accountId);
            await accountLock.WaitAsync();
            try
            {
                return (await GetAccountWarehouse(accountId))?.GetValueOrDefault(slot);
            }
            finally
            {
                accountLock.Release();
            }
        }

        public async Task<AddWarehouseItemResult> AddWarehouseItem(AccountWarehouseItemDto warehouseItemDtoToAdd)
        {
            long accountId = warehouseItemDtoToAdd.AccountId;

            if (warehouseItemDtoToAdd.ItemInstance.Amount < 1 || warehouseItemDtoToAdd.ItemInstance.Amount > _generalServerConfiguration.MaxItemAmount)
            {
                return new AddWarehouseItemResult
                {
                    Success = false
                };
            }

            SemaphoreSlim accountLock = GetAccountLock(accountId);
            await accountLock.WaitAsync();
            try
            {
                Dictionary<short, AccountWarehouseItemDto> familyWarehouse = await GetAccountWarehouse(accountId);
                if (familyWarehouse == null)
                {
                    return new AddWarehouseItemResult
                    {
                        Success = false
                    };
                }

                AccountWarehouseItemDto alreadyExistentItem = familyWarehouse.GetValueOrDefault(warehouseItemDtoToAdd.Slot);

                if (alreadyExistentItem == null)
                {
                    familyWarehouse[warehouseItemDtoToAdd.Slot] = warehouseItemDtoToAdd;
                    await SetItemChangeWithLock(warehouseItemDtoToAdd, false);
                    return new AddWarehouseItemResult
                    {
                        Success = true,
                        UpdatedItem = warehouseItemDtoToAdd
                    };
                }

                if (warehouseItemDtoToAdd.ItemInstance.ItemVNum != alreadyExistentItem.ItemInstance.ItemVNum)
                {
                    return new AddWarehouseItemResult
                    {
                        Success = false
                    };
                }

                if (warehouseItemDtoToAdd.ItemInstance.Type != ItemInstanceType.NORMAL_ITEM || alreadyExistentItem.ItemInstance.Type != ItemInstanceType.NORMAL_ITEM
                    || warehouseItemDtoToAdd.ItemInstance.Amount + alreadyExistentItem.ItemInstance.Amount > _generalServerConfiguration.MaxItemAmount)
                {
                    return new AddWarehouseItemResult
                    {
                        Success = false
                    };
                }

                alreadyExistentItem.ItemInstance.Amount += warehouseItemDtoToAdd.ItemInstance.Amount;

                await SetItemChangeWithLock(alreadyExistentItem, false);
                return new AddWarehouseItemResult
                {
                    Success = true,
                    UpdatedItem = alreadyExistentItem
                };
            }
            finally
            {
                accountLock.Release();
            }
        }

        public async Task<WithdrawWarehouseItemResult> WithdrawWarehouseItem(AccountWarehouseItemDto warehouseItemDtoToWithdraw, int amount)
        {
            long accountId = warehouseItemDtoToWithdraw.AccountId;

            if (amount < 1 || _generalServerConfiguration.MaxItemAmount < amount)
            {
                return new WithdrawWarehouseItemResult
                {
                    Success = false
                };
            }

            SemaphoreSlim accountLock = GetAccountLock(accountId);
            await accountLock.WaitAsync();
            try
            {
                Dictionary<short, AccountWarehouseItemDto> familyWarehouse = await GetAccountWarehouse(accountId);

                if (familyWarehouse == null)
                {
                    return new WithdrawWarehouseItemResult
                    {
                        Success = false
                    };
                }

                AccountWarehouseItemDto alreadyExistentItem = familyWarehouse.GetValueOrDefault(warehouseItemDtoToWithdraw.Slot);
                if (alreadyExistentItem == null || alreadyExistentItem.ItemInstance.Amount < amount)
                {
                    return new WithdrawWarehouseItemResult
                    {
                        Success = false
                    };
                }

                alreadyExistentItem.ItemInstance.Amount -= amount;

                bool toRemove = alreadyExistentItem.ItemInstance.Amount == 0;
                if (toRemove)
                {
                    familyWarehouse.Remove(warehouseItemDtoToWithdraw.Slot);
                }

                await SetItemChangeWithLock(alreadyExistentItem, toRemove);

                ItemInstanceDTO newItemInstance = alreadyExistentItem.ItemInstance.Adapt<ItemInstanceDTO>();
                newItemInstance.Amount = amount;

                return new WithdrawWarehouseItemResult
                {
                    Success = true,
                    UpdatedItem = alreadyExistentItem.ItemInstance.Amount == 0 ? null : alreadyExistentItem,
                    WithdrawnItem = newItemInstance
                };
            }
            finally
            {
                accountLock.Release();
            }
        }

        public async Task<MoveWarehouseItemResult> MoveWarehouseItem(AccountWarehouseItemDto warehouseItemDtoToMove, int amount, short newSlot)
        {
            long accountId = warehouseItemDtoToMove.AccountId;

            if (amount < 1 || _generalServerConfiguration.MaxItemAmount < amount)
            {
                return new MoveWarehouseItemResult
                {
                    Success = false
                };
            }

            SemaphoreSlim accountLock = GetAccountLock(accountId);
            await accountLock.WaitAsync();
            try
            {
                Dictionary<short, AccountWarehouseItemDto> familyWarehouse = await GetAccountWarehouse(accountId);
                if (familyWarehouse == null)
                {
                    return new MoveWarehouseItemResult
                    {
                        Success = false
                    };
                }

                AccountWarehouseItemDto toMoveItem = familyWarehouse.GetValueOrDefault(warehouseItemDtoToMove.Slot);
                AccountWarehouseItemDto toMergeItem = familyWarehouse.GetValueOrDefault(newSlot);
                if (toMoveItem == null || toMoveItem.ItemInstance.Amount < amount)
                {
                    return new MoveWarehouseItemResult
                    {
                        Success = false
                    };
                }

                if (toMergeItem == null)
                {
                    if (amount == toMoveItem.ItemInstance.Amount)
                    {
                        familyWarehouse.Remove(toMoveItem.Slot);
                        await SetItemChangeWithLock(new AccountWarehouseItemDto
                        {
                            AccountId = accountId,
                            Slot = toMoveItem.Slot
                        }, true);
                        toMoveItem.Slot = newSlot;
                        familyWarehouse[toMoveItem.Slot] = toMoveItem;
                        await SetItemChangeWithLock(toMoveItem, false);
                        return new MoveWarehouseItemResult
                        {
                            Success = true,
                            OldItem = null,
                            NewItem = toMoveItem
                        };
                    }

                    toMoveItem.ItemInstance.Amount -= amount;
                    await SetItemChangeWithLock(toMoveItem, false);
                    var newItem = new AccountWarehouseItemDto
                    {
                        AccountId = accountId,
                        ItemInstance = toMoveItem.ItemInstance.Adapt<ItemInstanceDTO>(),
                        Slot = newSlot
                    };
                    newItem.ItemInstance.Amount = amount;
                    familyWarehouse[newItem.Slot] = newItem;
                    await SetItemChangeWithLock(newItem, false);
                    return new MoveWarehouseItemResult
                    {
                        Success = true,
                        OldItem = toMoveItem,
                        NewItem = newItem
                    };
                }

                if (toMoveItem.ItemInstance.ItemVNum != toMergeItem.ItemInstance.ItemVNum)
                {
                    toMergeItem.Slot = toMoveItem.Slot;
                    toMoveItem.Slot = newSlot;
                    familyWarehouse[toMoveItem.Slot] = toMoveItem;
                    await SetItemChangeWithLock(toMoveItem, false);
                    familyWarehouse[toMergeItem.Slot] = toMergeItem;
                    await SetItemChangeWithLock(toMergeItem, false);
                    return new MoveWarehouseItemResult
                    {
                        Success = true,
                        OldItem = toMergeItem,
                        NewItem = toMoveItem
                    };
                }

                if (toMoveItem.ItemInstance.Type != ItemInstanceType.NORMAL_ITEM || toMergeItem.ItemInstance.Type != ItemInstanceType.NORMAL_ITEM || amount + toMergeItem.ItemInstance.Amount > _generalServerConfiguration.MaxItemAmount)
                {
                    return new MoveWarehouseItemResult
                    {
                        Success = false
                    };
                }

                toMoveItem.ItemInstance.Amount -= amount;
                toMergeItem.ItemInstance.Amount += amount;

                bool toRemove = toMoveItem.ItemInstance.Amount == 0;
                if (toRemove)
                {
                    familyWarehouse.Remove(toMoveItem.Slot);
                }

                await SetItemChangeWithLock(toMoveItem, toRemove);
                await SetItemChangeWithLock(toMergeItem, false);

                return new MoveWarehouseItemResult
                {
                    Success = true,
                    OldItem = toRemove ? null : toMoveItem,
                    NewItem = toMergeItem
                };
            }
            finally
            {
                accountLock.Release();
            }
        }

        public async Task FlushWarehouseSaves()
        {
            if (_itemChanges.Count < 1)
            {
                return;
            }

            await _itemChangesSemaphore.WaitAsync();
            try
            {
                List<(AccountWarehouseItemDto dto, bool remove)> unsavedChanges = new();

                var globalWatch = Stopwatch.StartNew();
                foreach ((long accountId, Dictionary<short, (AccountWarehouseItemDto dto, bool remove)> warehouseChanges) in _itemChanges)
                {
                    List<AccountWarehouseItemDto> itemsToSave = new();
                    List<AccountWarehouseItemDto> itemsToRemove = new();

                    foreach ((short _, (AccountWarehouseItemDto dto, bool remove)) in warehouseChanges)
                    {
                        (remove ? itemsToRemove : itemsToSave).Add(dto);
                    }

                    if (itemsToSave.Count > 0)
                    {
                        try
                        {
                            int countSavedItems = await _accountWarehouseItemDao.SaveAsync(itemsToSave);
                            Log.Warn($"[ACCOUNT_WAREHOUSE_MANAGER][FLUSH_SAVES][ACCOUNT_ID: {accountId.ToString()}] Saved {countSavedItems.ToString()} warehouseItems");
                        }
                        catch (Exception e)
                        {
                            Log.Error(
                                $"[ACCOUNT_WAREHOUSE_MANAGER][FLUSH_SAVES][ACCOUNT_ID: {accountId.ToString()}] Error while trying to save {itemsToSave.Count.ToString()} warehouseItems. Re-queueing. ",
                                e);
                            unsavedChanges.AddRange(itemsToSave.Select(x => (x, false)));
                        }
                    }

                    if (itemsToRemove.Count < 1)
                    {
                        continue;
                    }

                    try
                    {
                        await _accountWarehouseItemDao.DeleteAsync(itemsToRemove);
                        Log.Warn($"[ACCOUNT_WAREHOUSE_MANAGER][FLUSH_SAVES][ACCOUNT_ID: {accountId.ToString()}] Removed (at maximum) {itemsToRemove.Count.ToString()} warehouseItems");
                    }
                    catch (Exception e)
                    {
                        Log.Error(
                            $"[ACCOUNT_WAREHOUSE_MANAGER][FLUSH_SAVES][ACCOUNT_ID: {accountId.ToString()}] Error while trying to remove {itemsToRemove.Count.ToString()} warehouseItems. Re-queueing. ",
                            e);
                        unsavedChanges.AddRange(itemsToRemove.Select(x => (x, true)));
                    }
                }

                globalWatch.Stop();
                Log.Debug($"[ACCOUNT_WAREHOUSE_MANAGER][FLUSH_SAVES] Saving all warehouses took {globalWatch.ElapsedMilliseconds.ToString()}ms");

                _itemChanges.Clear();

                foreach ((AccountWarehouseItemDto dto, bool remove) in unsavedChanges)
                {
                    SetItemChange(dto, remove);
                }
            }
            finally
            {
                _itemChangesSemaphore.Release();
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await FlushWarehouseSaves();
                await Task.Delay(Interval, stoppingToken);
            }
        }

        private SemaphoreSlim GetAccountLock(long accountId) => _warehouseLocks.GetOrAdd(accountId, new SemaphoreSlim(1, 1));

        private async Task SetItemChangeWithLock(AccountWarehouseItemDto dto, bool remove)
        {
            await _itemChangesSemaphore.WaitAsync();
            try
            {
                SetItemChange(dto, remove);
            }
            finally
            {
                _itemChangesSemaphore.Release();
            }
        }

        /// <summary>
        ///     Not to be used outside SemaphoreSlim
        /// </summary>
        private void SetItemChange(AccountWarehouseItemDto dto, bool remove)
        {
            _itemChanges.GetOrSetDefault(dto.AccountId, new Dictionary<short, (AccountWarehouseItemDto dto, bool remove)>())[dto.Slot] = (dto, remove);
        }

        private async Task<Dictionary<short, AccountWarehouseItemDto>> GetAccountWarehouse(long accountId)
        {
            Dictionary<short, AccountWarehouseItemDto> cachedItems = _cachedWarehouseItems.Get(accountId);
            if (cachedItems != null)
            {
                return cachedItems;
            }

            cachedItems = (await _accountWarehouseItemDao.GetByAccountIdAsync(accountId))?.ToDictionary(x => x.Slot);
            _cachedWarehouseItems.Set(accountId, cachedItems ?? new Dictionary<short, AccountWarehouseItemDto>(), LifeTime);
            return cachedItems;
        }
    }
}