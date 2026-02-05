using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Foundatio.AsyncEx;
using PhoenixLib.Caching;
using PhoenixLib.Logging;
using PhoenixLib.ServiceBus;
using Plugin.FamilyImpl.Messages;
using WingsAPI.Data.Families;

namespace FamilyServer.Managers
{
    public class FamilyWarehouseLogManager
    {
        private const int MaximumAmountOfLogs = 200;
        private static readonly TimeSpan LifeTime = TimeSpan.FromMinutes(Convert.ToUInt32(Environment.GetEnvironmentVariable(EnvironmentConsts.FamilyServerSaveIntervalMinutes) ?? "5") * 3);

        private readonly ILongKeyCachedRepository<List<FamilyWarehouseLogEntryDto>> _cachedFamilyLogs;

        private readonly HashSet<long> _familiesRequiringSave = new();
        private readonly ConcurrentDictionary<long, AsyncReaderWriterLock> _familyLocks = new();

        private readonly IFamilyWarehouseLogDao _familyWarehouseLogDao;
        private readonly IMessagePublisher<FamilyWarehouseLogAddMessage> _messagePublisher;
        private readonly SemaphoreSlim _semaphoreSlim = new(1, 1);

        public FamilyWarehouseLogManager(IFamilyWarehouseLogDao familyWarehouseLogDao, ILongKeyCachedRepository<List<FamilyWarehouseLogEntryDto>> cachedFamilyLogs,
            IMessagePublisher<FamilyWarehouseLogAddMessage> messagePublisher)
        {
            _familyWarehouseLogDao = familyWarehouseLogDao;
            _cachedFamilyLogs = cachedFamilyLogs;
            _messagePublisher = messagePublisher;
        }

        private async Task<List<FamilyWarehouseLogEntryDto>> GetFamilyWarehouseLogs(long familyId)
        {
            List<FamilyWarehouseLogEntryDto> cachedLogs = _cachedFamilyLogs.Get(familyId);
            if (cachedLogs != null)
            {
                //Refresh TLL
                _cachedFamilyLogs.Set(familyId, cachedLogs, LifeTime);
                return cachedLogs;
            }

            IEnumerable<FamilyWarehouseLogEntryDto> returnedLogs = await _familyWarehouseLogDao.GetByFamilyIdAsync(familyId);
            cachedLogs = returnedLogs == null ? new List<FamilyWarehouseLogEntryDto>() : returnedLogs.ToList();

            _cachedFamilyLogs.Set(familyId, cachedLogs, LifeTime);
            return cachedLogs;
        }

        private AsyncReaderWriterLock GetFamilyLock(long familyId) => _familyLocks.GetOrAdd(familyId, new AsyncReaderWriterLock());

        public async Task AddLog(long familyId, FamilyWarehouseLogEntryDto logEntryDto)
        {
            if (logEntryDto == null)
            {
                return;
            }

            await _semaphoreSlim.WaitAsync();
            try
            {
                _familiesRequiringSave.Add(familyId);
            }
            finally
            {
                _semaphoreSlim.Release();
            }

            AsyncReaderWriterLock familyLock = GetFamilyLock(familyId);

            using (await familyLock.WriterLockAsync())
            {
                List<FamilyWarehouseLogEntryDto> logs = await GetFamilyWarehouseLogs(familyId);
                logs.Add(logEntryDto);
                if (logs.Count <= MaximumAmountOfLogs)
                {
                    return;
                }

                logs.RemoveRange(0, logs.Count - MaximumAmountOfLogs);
            }

            await _messagePublisher.PublishAsync(new FamilyWarehouseLogAddMessage
            {
                FamilyId = familyId,
                LogToAdd = logEntryDto
            });
        }

        public async Task<List<FamilyWarehouseLogEntryDto>> GetLogs(long familyId)
        {
            AsyncReaderWriterLock familyLock = GetFamilyLock(familyId);

            using (await familyLock.ReaderLockAsync())
            {
                return await GetFamilyWarehouseLogs(familyId);
            }
        }

        public async Task FlushLogSaves()
        {
            if (_familiesRequiringSave.Count < 1)
            {
                return;
            }

            await _semaphoreSlim.WaitAsync();
            try
            {
                List<long> unsavedFamilies = new();
                foreach (long familyId in _familiesRequiringSave)
                {
                    List<FamilyWarehouseLogEntryDto> logs = await GetLogs(familyId);
                    try
                    {
                        int countSavedLogs = await _familyWarehouseLogDao.SaveAsync(familyId, logs);
                        Log.Warn($"[FAMILY_WAREHOUSE_MANAGER][FLUSH_SAVES][FAMILY_ID: {familyId.ToString()}] Saved {countSavedLogs.ToString()} warehouseLogs");
                    }
                    catch (Exception e)
                    {
                        Log.Error($"[FAMILY_WAREHOUSE_MANAGER][FLUSH_SAVES][FAMILY_ID: {familyId.ToString()}] Error while trying to save {logs.Count.ToString()} warehouseLogs. Re-queueing. ", e);
                        unsavedFamilies.Add(familyId);
                    }
                }

                _familiesRequiringSave.Clear();

                foreach (long unsavedFamilyId in unsavedFamilies)
                {
                    _familiesRequiringSave.Add(unsavedFamilyId);
                }
            }
            finally
            {
                _semaphoreSlim.Release();
            }
        }
    }
}