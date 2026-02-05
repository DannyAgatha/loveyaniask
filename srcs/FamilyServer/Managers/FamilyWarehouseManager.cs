using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Mapster;
using PhoenixLib.Caching;
using PhoenixLib.Logging;
using WingsAPI.Communication.Families;
using WingsAPI.Data.Families;
using WingsAPI.Game.Extensions.Families;
using WingsAPI.Packets.Enums.Families;
using WingsEmu.Core.Extensions;
using WingsEmu.DTOs.Items;
using WingsEmu.Game.Configurations;

namespace FamilyServer.Managers
{
    public class FamilyWarehouseManager : IFamilyWarehouseManager
    {
        private static readonly TimeSpan LifeTime = TimeSpan.FromMinutes(Convert.ToUInt32(Environment.GetEnvironmentVariable(EnvironmentConsts.FamilyServerSaveIntervalMinutes) ?? "5") * 3);

        private readonly ILongKeyCachedRepository<Dictionary<short, FamilyWarehouseItemDto>> _cachedFamilyItems;
        private readonly SemaphoreSlim _familyLock = new(1, 1);
        private readonly IFamilyService _familyService;

        private readonly IFamilyWarehouseItemDao _familyWarehouseItemDao;
        private readonly FamilyWarehouseLogManager _familyWarehouseLogManager;

        private readonly Dictionary<long, Dictionary<short, (FamilyWarehouseItemDto dto, bool remove)>> _itemChanges = new();
        private readonly SemaphoreSlim _itemChangesSemaphore = new(1, 1);
        private readonly GeneralServerConfiguration _generalServerConfiguration;

        public FamilyWarehouseManager(IFamilyWarehouseItemDao familyWarehouseItemDao, ILongKeyCachedRepository<Dictionary<short, FamilyWarehouseItemDto>> cachedFamilyItems,
            IFamilyService familyService,
            FamilyWarehouseLogManager familyWarehouseLogManager, GeneralServerConfiguration generalServerConfiguration)
        {
            _familyWarehouseItemDao = familyWarehouseItemDao;
            _cachedFamilyItems = cachedFamilyItems;
            _familyService = familyService;
            _familyWarehouseLogManager = familyWarehouseLogManager;
            _generalServerConfiguration = generalServerConfiguration;
        }

        public async Task<IList<FamilyWarehouseLogEntryDto>> GetWarehouseLogs(long familyId, long? characterId = null)
        {
            if (!await CheckLogHistoryPermission(familyId, characterId))
            {
                return null;
            }

            return await _familyWarehouseLogManager.GetLogs(familyId);
        }

        public async Task<IEnumerable<FamilyWarehouseItemDto>> GetWarehouse(long familyId, long? characterId = null)
        {
            if (!await CheckPutWithdrawPermission(familyId, characterId, FamilyWarehouseAuthorityType.Put))
            {
                return null;
            }

            await _familyLock.WaitAsync();
            try
            {
                return (await GetFamilyWarehouse(familyId))?.Values;
            }
            finally
            {
                _familyLock.Release();
            }
        }

        public async Task<FamilyWarehouseItemDto> GetWarehouseItem(long familyId, short slot, long? characterId = null)
        {
            if (!await CheckPutWithdrawPermission(familyId, characterId, FamilyWarehouseAuthorityType.Put))
            {
                return null;
            }

            await _familyLock.WaitAsync();
            try
            {
                return (await GetFamilyWarehouse(familyId))?.GetValueOrDefault(slot);
            }
            finally
            {
                _familyLock.Release();
            }
        }

        public async Task<AddWarehouseItemResult> AddWarehouseItem(FamilyWarehouseItemDto warehouseItemDtoToAdd, long? characterId = null, string characterName = null)
        {
            long familyId = warehouseItemDtoToAdd.FamilyId;

            if (warehouseItemDtoToAdd.ItemInstance.Amount < 1 || warehouseItemDtoToAdd.ItemInstance.Amount > _generalServerConfiguration.MaxItemAmount || !await CheckSlot(familyId, warehouseItemDtoToAdd.Slot) ||
                !await CheckPutWithdrawPermission(familyId, characterId, FamilyWarehouseAuthorityType.Put))
            {
                return new AddWarehouseItemResult
                {
                    Success = false
                };
            }

            await _familyLock.WaitAsync();
            try
            {
                Dictionary<short, FamilyWarehouseItemDto> familyWarehouse = await GetFamilyWarehouse(familyId);
                if (familyWarehouse == null)
                {
                    return new AddWarehouseItemResult
                    {
                        Success = false
                    };
                }

                FamilyWarehouseItemDto alreadyExistentItem = familyWarehouse.GetValueOrDefault(warehouseItemDtoToAdd.Slot);

                if (alreadyExistentItem == null)
                {
                    familyWarehouse[warehouseItemDtoToAdd.Slot] = warehouseItemDtoToAdd;
                    await SetItemChangeWithLock(warehouseItemDtoToAdd, false);
                    await AddLog(familyId, warehouseItemDtoToAdd.ItemInstance, FamilyWarehouseLogEntryType.List, characterId, characterName);
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
                await AddLog(familyId, warehouseItemDtoToAdd.ItemInstance, FamilyWarehouseLogEntryType.List, characterId, characterName);
                return new AddWarehouseItemResult
                {
                    Success = true,
                    UpdatedItem = alreadyExistentItem
                };
            }
            finally
            {
                _familyLock.Release();
            }
        }

        public async Task<WithdrawWarehouseItemResult> WithdrawWarehouseItem(FamilyWarehouseItemDto warehouseItemDtoToWithdraw, int amount, long? characterId = null, string characterName = null)
        {
            long familyId = warehouseItemDtoToWithdraw.FamilyId;

            if (amount < 1 || amount > _generalServerConfiguration.MaxItemAmount || !await CheckPutWithdrawPermission(familyId, characterId, FamilyWarehouseAuthorityType.PutAndWithdraw))
            {
                return new WithdrawWarehouseItemResult
                {
                    Success = false
                };
            }

            await _familyLock.WaitAsync();
            try
            {
                Dictionary<short, FamilyWarehouseItemDto> familyWarehouse = await GetFamilyWarehouse(familyId);

                if (familyWarehouse == null)
                {
                    return new WithdrawWarehouseItemResult
                    {
                        Success = false
                    };
                }

                FamilyWarehouseItemDto alreadyExistentItem = familyWarehouse.GetValueOrDefault(warehouseItemDtoToWithdraw.Slot);
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

                await AddLog(familyId, newItemInstance, FamilyWarehouseLogEntryType.Withdraw, characterId, characterName);

                return new WithdrawWarehouseItemResult
                {
                    Success = true,
                    UpdatedItem = alreadyExistentItem.ItemInstance.Amount == 0 ? null : alreadyExistentItem,
                    WithdrawnItem = newItemInstance
                };
            }
            finally
            {
                _familyLock.Release();
            }
        }

        public async Task<MoveWarehouseItemResult> MoveWarehouseItem(FamilyWarehouseItemDto warehouseItemDtoToMove, int amount, short newSlot, long? characterId = null)
        {
            long familyId = warehouseItemDtoToMove.FamilyId;

            if (amount < 1 || amount > _generalServerConfiguration.MaxItemAmount || !await CheckSlot(familyId, warehouseItemDtoToMove.Slot, newSlot) ||
                !await CheckPutWithdrawPermission(familyId, characterId, FamilyWarehouseAuthorityType.Put))
            {
                return new MoveWarehouseItemResult
                {
                    Success = false
                };
            }

            await _familyLock.WaitAsync();
            try
            {
                Dictionary<short, FamilyWarehouseItemDto> familyWarehouse = await GetFamilyWarehouse(familyId);
                if (familyWarehouse == null)
                {
                    return new MoveWarehouseItemResult
                    {
                        Success = false
                    };
                }

                FamilyWarehouseItemDto toMoveItem = familyWarehouse.GetValueOrDefault(warehouseItemDtoToMove.Slot);
                FamilyWarehouseItemDto toMergeItem = familyWarehouse.GetValueOrDefault(newSlot);
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
                        await SetItemChangeWithLock(new FamilyWarehouseItemDto
                        {
                            FamilyId = familyId,
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
                    var newItem = new FamilyWarehouseItemDto
                    {
                        FamilyId = familyId,
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
                _familyLock.Release();
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
                List<(FamilyWarehouseItemDto dto, bool remove)> unsavedChanges = new();

                var globalWatch = Stopwatch.StartNew();
                foreach ((long familyId, Dictionary<short, (FamilyWarehouseItemDto dto, bool remove)> warehouseChanges) in _itemChanges)
                {
                    List<FamilyWarehouseItemDto> itemsToSave = new();
                    List<FamilyWarehouseItemDto> itemsToRemove = new();

                    foreach ((short _, (FamilyWarehouseItemDto dto, bool remove)) in warehouseChanges)
                    {
                        (remove ? itemsToRemove : itemsToSave).Add(dto);
                    }

                    if (itemsToSave.Count > 0)
                    {
                        try
                        {
                            int countSavedItems = await _familyWarehouseItemDao.SaveAsync(itemsToSave);
                            Log.Warn($"[FAMILY_WAREHOUSE_MANAGER][FLUSH_SAVES][FAMILY_ID: {familyId.ToString()}] Saved {countSavedItems.ToString()} warehouseItems");
                        }
                        catch (Exception e)
                        {
                            Log.Error(
                                $"[FAMILY_WAREHOUSE_MANAGER][FLUSH_SAVES][FAMILY_ID: {familyId.ToString()}] Error while trying to save {itemsToSave.Count.ToString()} warehouseItems. Re-queueing. ",
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
                        await _familyWarehouseItemDao.DeleteAsync(itemsToRemove);
                        Log.Warn($"[FAMILY_WAREHOUSE_MANAGER][FLUSH_SAVES][FAMILY_ID: {familyId.ToString()}] Removed (at maximum) {itemsToRemove.Count.ToString()} warehouseItems");
                    }
                    catch (Exception e)
                    {
                        Log.Error(
                            $"[FAMILY_WAREHOUSE_MANAGER][FLUSH_SAVES][FAMILY_ID: {familyId.ToString()}] Error while trying to remove {itemsToRemove.Count.ToString()} warehouseItems. Re-queueing. ",
                            e);
                        unsavedChanges.AddRange(itemsToRemove.Select(x => (x, true)));
                    }
                }

                globalWatch.Stop();
                Log.Debug($"[FAMILY_WAREHOUSE_MANAGER][FLUSH_SAVES] Saving all warehouses took {globalWatch.ElapsedMilliseconds.ToString()}ms");

                _itemChanges.Clear();

                foreach ((FamilyWarehouseItemDto dto, bool remove) in unsavedChanges)
                {
                    SetItemChange(dto, remove);
                }
            }
            finally
            {
                _itemChangesSemaphore.Release();
            }

            await _familyWarehouseLogManager.FlushLogSaves();
        }

        private async Task<bool> CheckSlot(long familyId, short slot, short? slot2 = null)
        {
            FamilyIdResponse family = await _familyService.GetFamilyByIdAsync(new FamilyIdRequest
            {
                FamilyId = familyId
            });

            if (family == null)
            {
                return false;
            }

            short warehouseCapacity = family.Family.Upgrades.UpgradeValues.GetValueOrDefault(FamilyUpgradeType.INCREASE_FAMILY_WAREHOUSE).Item1;
            if (slot < 0 || warehouseCapacity <= slot)
            {
                return false;
            }

            if (slot2 < 0 || warehouseCapacity <= slot2)
            {
                return false;
            }

            return true;
        }

        private async Task<bool> CheckPutWithdrawPermission(long familyId, long? characterId, FamilyWarehouseAuthorityType authorityRequested)
        {
            if (characterId == null)
            {
                return true;
            }

            FamilyIdResponse family = await _familyService.GetFamilyByIdAsync(new FamilyIdRequest
            {
                FamilyId = familyId
            });

            FamilyMembershipDto member = family?.Members.Find(x => x.CharacterId == characterId);

            return member != null && member.CheckPutWithdrawPermission(family.Family, authorityRequested);
        }

        private async Task<bool> CheckLogHistoryPermission(long familyId, long? characterId)
        {
            if (characterId == null)
            {
                return true;
            }

            FamilyIdResponse family = await _familyService.GetFamilyByIdAsync(new FamilyIdRequest
            {
                FamilyId = familyId
            });

            FamilyMembershipDto member = family?.Members.Find(x => x.CharacterId == characterId);

            return member != null && member.CheckLogHistoryPermission(family.Family);
        }

        private async Task SetItemChangeWithLock(FamilyWarehouseItemDto dto, bool remove)
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
        private void SetItemChange(FamilyWarehouseItemDto dto, bool remove)
        {
            _itemChanges.GetOrSetDefault(dto.FamilyId, new Dictionary<short, (FamilyWarehouseItemDto dto, bool remove)>())[dto.Slot] = (dto, remove);
        }

        private async Task<Dictionary<short, FamilyWarehouseItemDto>> GetFamilyWarehouse(long familyId)
        {
            Dictionary<short, FamilyWarehouseItemDto> cachedItems = _cachedFamilyItems.Get(familyId);
            if (cachedItems != null)
            {
                return cachedItems;
            }

            cachedItems = (await _familyWarehouseItemDao.GetByFamilyIdAsync(familyId))?.ToDictionary(x => x.Slot);
            _cachedFamilyItems.Set(familyId, cachedItems ?? new Dictionary<short, FamilyWarehouseItemDto>(), LifeTime);
            return cachedItems;
        }

        private async Task AddLog(long familyId, ItemInstanceDTO item, FamilyWarehouseLogEntryType logEntryType, long? characterId, string characterName)
        {
            await _familyWarehouseLogManager.AddLog(familyId, new FamilyWarehouseLogEntryDto
            {
                CharacterId = characterId ?? -1,
                CharacterName = characterName,
                DateOfLog = DateTime.UtcNow,
                Type = logEntryType,
                ItemVnum = item.ItemVNum,
                Amount = item.Amount
            });
        }
    }
}