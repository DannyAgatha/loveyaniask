using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using PhoenixLib.Caching;
using PhoenixLib.Logging;
using WingsAPI.Data.TimeSpace;

namespace DatabaseServer.Managers
{
    public class TimeSpaceManager : BackgroundService, ITimeSpaceManager
    {
        private static readonly TimeSpan Interval = TimeSpan.FromMinutes(Convert.ToUInt32(Environment.GetEnvironmentVariable(EnvironmentConsts.TsServerSaveIntervalMinutes) ?? "5"));
        private readonly ConcurrentQueue<TimeSpaceRecordDto> _queue = new();
        private readonly ILongKeyCachedRepository<TimeSpaceRecordDto> _records;

        private readonly ITimeSpaceRecordDao _timeSpaceRecordDao;

        public TimeSpaceManager(ITimeSpaceRecordDao timeSpaceRecordDao, ILongKeyCachedRepository<TimeSpaceRecordDto> records)
        {
            _timeSpaceRecordDao = timeSpaceRecordDao;
            _records = records;
        }

        public async Task<TimeSpaceRecordDto> GetRecordByTimeSpaceId(long tsId)
        {
            TimeSpaceRecordDto cached = _records.Get(tsId);
            if (cached != null)
            {
                return cached;
            }

            TimeSpaceRecordDto record = await _timeSpaceRecordDao.GetRecordById(tsId);
            if (record == null)
            {
                return null;
            }

            _records.Set(tsId, record);
            return record;
        }

        public async Task FlushTimeSpaceRecords()
        {
            if (_queue.IsEmpty)
            {
                return;
            }

            try
            {
                while (_queue.TryDequeue(out TimeSpaceRecordDto record))
                {
                    TimeSpaceRecordDto currentRecord = await GetRecordByTimeSpaceId(record.TimeSpaceId);
                    if (currentRecord == null)
                    {
                        _records.Set(record.TimeSpaceId, record);
                        await _timeSpaceRecordDao.SaveRecord(record);
                        continue;
                    }

                    if (currentRecord.Record >= record.Record)
                    {
                        continue;
                    }

                    _records.Set(record.TimeSpaceId, record);
                    await _timeSpaceRecordDao.SaveRecord(record);
                }
            }
            catch (Exception e)
            {
                Log.Error("FlushTimeSpaceRecords", e);
            }
        }

        public async Task Initialize()
        {
            IEnumerable<TimeSpaceRecordDto> records = await _timeSpaceRecordDao.GetAllRecords();
            int counter = 0;
            foreach (TimeSpaceRecordDto recordDto in records)
            {
                counter++;
                _records.Set(recordDto.TimeSpaceId, recordDto);
            }

            Log.Info($"Initialized {counter} time-space records.");
        }

        public void TryAddNewRecord(TimeSpaceRecordDto record)
        {
            _queue.Enqueue(record);
        }

        public async Task<bool> IsNewRecord(long tsId, long points)
        {
            TimeSpaceRecordDto currentRecord = await GetRecordByTimeSpaceId(tsId);
            if (currentRecord == null)
            {
                return true;
            }

            return currentRecord.Record < points;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Initialize();
            Log.Info("[TIME_SPACE_MANAGER] Initialized!");
            while (!stoppingToken.IsCancellationRequested)
            {
                await FlushTimeSpaceRecords();
                await Task.Delay(Interval, stoppingToken);
            }
        }
    }
}