using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using PhoenixLib.Logging;
using PhoenixLib.ServiceBus;
using Plugin.FamilyImpl.Logs;
using Plugin.FamilyImpl.Messages;
using WingsAPI.Data.Families;

namespace Plugin.FamilyImpl.RecurrentJob
{
    public class FamilyLogSystem : BackgroundService
    {
        private static readonly TimeSpan Interval = TimeSpan.FromSeconds(10);
        private readonly IFamilyLogManager _familyLogManager;
        private readonly IMessagePublisher<FamilyDeclareLogsMessage> _messagePublisher;

        public FamilyLogSystem(IMessagePublisher<FamilyDeclareLogsMessage> messagePublisher, IFamilyLogManager familyLogManager)
        {
            _messagePublisher = messagePublisher;
            _familyLogManager = familyLogManager;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Log.Info("[FAMILY_LOG_SYSTEM] Started!");
            while (!stoppingToken.IsCancellationRequested)
            {
                IReadOnlyList<FamilyLogDto> logs = _familyLogManager.GetFamilyLogsInBuffer();

                if (logs?.Count > 0)
                {
                    Log.Info($"[FAMILY_LOG_SYSTEM] Sent: {logs.Count.ToString()} logs");
                    await _messagePublisher.PublishAsync(new FamilyDeclareLogsMessage
                    {
                        Logs = logs
                    }, stoppingToken);
                }

                await Task.Delay(Interval, stoppingToken);
            }
        }
    }
}