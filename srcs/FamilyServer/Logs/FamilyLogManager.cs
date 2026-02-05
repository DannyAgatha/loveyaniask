using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using PhoenixLib.Caching;
using PhoenixLib.Logging;
using PhoenixLib.ServiceBus;
using Plugin.FamilyImpl.Messages;
using WingsAPI.Data.Families;

namespace FamilyServer.Logs
{
    public class FamilyLogManager : BackgroundService
    {
        private readonly ILongKeyCachedRepository<List<FamilyLogDto>> _cachedLogs;
        private readonly IFamilyLogDAO _familyLogDao;
        private readonly ReaderWriterLockSlim _lock = new();
        private readonly ConcurrentQueue<IReadOnlyList<FamilyLogDto>> _logs = new();
        private readonly IMessagePublisher<FamilyAcknowledgeLogsMessage> _messagePublisher;

        public FamilyLogManager(IFamilyLogDAO familyLogDao, ILongKeyCachedRepository<List<FamilyLogDto>> cachedLogs, IMessagePublisher<FamilyAcknowledgeLogsMessage> messagePublisher)
        {
            _familyLogDao = familyLogDao;
            _cachedLogs = cachedLogs;
            _messagePublisher = messagePublisher;
        }

        private static TimeSpan RefreshTime => TimeSpan.FromSeconds(Convert.ToInt32(Environment.GetEnvironmentVariable("FAMILY_LOGS_REFRESH_IN_SECONDS") ?? "5"));

        public void SaveFamilyLogs(IReadOnlyList<FamilyLogDto> logs)
        {
            _logs.Enqueue(logs);
        }

        public async Task<List<FamilyLogDto>> GetFamilyLogsByFamilyId(long familyId)
        {
            List<FamilyLogDto> cachedLogs = _cachedLogs.Get(familyId);
            if (cachedLogs != null)
            {
                return cachedLogs;
            }

            cachedLogs = await _familyLogDao.GetLogsByFamilyIdAsync(familyId);
            _cachedLogs.Set(familyId, cachedLogs);

            return cachedLogs;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                Dictionary<long, List<FamilyLogDto>> toUpdate = new();
                await ProcessLogs(toUpdate);

                if (toUpdate.Count > 0)
                {
                    await _messagePublisher.PublishAsync(new FamilyAcknowledgeLogsMessage
                    {
                        Logs = toUpdate
                    });
                }

                await Task.Delay(RefreshTime, stoppingToken);
            }
        }

        private async Task ProcessLogs(Dictionary<long, List<FamilyLogDto>> toUpdate)
        {
            if (_logs.IsEmpty)
            {
                return;
            }

            try
            {
                while (_logs.TryDequeue(out IReadOnlyList<FamilyLogDto> logs))
                {
                    IEnumerable<FamilyLogDto> savedLogs = await _familyLogDao.SaveAsync(logs);

                    foreach (FamilyLogDto log in savedLogs)
                    {
                        if (!toUpdate.TryGetValue(log.FamilyId, out List<FamilyLogDto> list))
                        {
                            toUpdate.Add(log.FamilyId, new List<FamilyLogDto>
                            {
                                log
                            });
                            continue;
                        }

                        list.Add(log);
                    }

                    _lock.EnterReadLock();
                    try
                    {
                        foreach (KeyValuePair<long, List<FamilyLogDto>> familyLogs in toUpdate)
                        {
                            List<FamilyLogDto> currentLogs = _cachedLogs.Get(familyLogs.Key);
                            if (currentLogs == null)
                            {
                                continue;
                            }

                            foreach (FamilyLogDto log in familyLogs.Value)
                            {
                                currentLogs.Add(log);
                            }

                            currentLogs.Sort((x, y) => DateTime.Compare(y.Timestamp, x.Timestamp)); // Newest -> Older

                            if (currentLogs.Count <= 200)
                            {
                                continue;
                            }

                            for (int i = 200; i < currentLogs.Count; i++)
                            {
                                currentLogs.RemoveAt(i);
                            }
                        }
                    }
                    finally
                    {
                        _lock.ExitReadLock();
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error("[FAMILY_LOGS_MANAGER]", e);
            }
        }
    }
}