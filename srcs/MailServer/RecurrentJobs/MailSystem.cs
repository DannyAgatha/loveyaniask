using System;
using System.Threading;
using System.Threading.Tasks;
using MailServer.Managers;
using Microsoft.Extensions.Hosting;

namespace MailServer.RecurrentJobs
{
    public class MailSystem : BackgroundService
    {
        private static readonly TimeSpan Interval = TimeSpan.FromSeconds(Convert.ToUInt32(Environment.GetEnvironmentVariable("MAIL_SERVER_SAVE_INTERVAL_SECONDS") ?? "10"));
        private readonly MailManager _mailManager;

        public MailSystem(MailManager mailManager) => _mailManager = mailManager;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await ProcessMain();
                await Task.Delay(Interval, stoppingToken);
            }
        }

        public async Task ProcessMain()
        {
            await _mailManager.FlushAll();
        }
    }
}