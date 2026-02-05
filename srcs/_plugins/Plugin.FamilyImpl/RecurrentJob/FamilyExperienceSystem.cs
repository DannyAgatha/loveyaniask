using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using PhoenixLib.Logging;
using PhoenixLib.ServiceBus;
using Plugin.FamilyImpl.Messages;
using WingsEmu.Game.Families;

namespace Plugin.FamilyImpl.RecurrentJob
{
    public class FamilyExperienceSystem : BackgroundService
    {
        private static readonly TimeSpan Interval = TimeSpan.FromSeconds(10);
        private readonly IFamilyExperienceManager _familyExperienceManager;
        private readonly IMessagePublisher<FamilyDeclareExperienceGainedMessage> _messagePublisher;

        public FamilyExperienceSystem(IMessagePublisher<FamilyDeclareExperienceGainedMessage> messagePublisher, IFamilyExperienceManager familyExperienceManager)
        {
            _messagePublisher = messagePublisher;
            _familyExperienceManager = familyExperienceManager;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Log.Info("[FAMILY_EXPERIENCE_SYSTEM] Started!");
            while (!stoppingToken.IsCancellationRequested)
            {
                IReadOnlyCollection<ExperienceGainedSubMessage> experiencesInBuffer = _familyExperienceManager.GetFamilyExperiencesInBuffer();

                if (experiencesInBuffer?.Count > 0)
                {
                    Log.Info($"[FAMILY_EXPERIENCE_SYSTEM] Sent: {experiencesInBuffer.Count.ToString()} experiences");
                    await _messagePublisher.PublishAsync(new FamilyDeclareExperienceGainedMessage
                    {
                        Experiences = experiencesInBuffer
                    }, stoppingToken);
                }

                await Task.Delay(Interval, stoppingToken);
            }
        }
    }
}