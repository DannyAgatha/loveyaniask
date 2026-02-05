using System.Threading;
using System.Threading.Tasks;
using MailServer.RecurrentJobs;
using PhoenixLib.ServiceBus;
using WingsAPI.Communication.Services.Messages;

namespace MailServer.Consumers
{
    public class ServiceFlushAllMessageConsumer : IMessageConsumer<ServiceFlushAllMessage>
    {
        private readonly MailSystem _mailSystem;

        public ServiceFlushAllMessageConsumer(MailSystem mailSystem) => _mailSystem = mailSystem;

        public async Task HandleAsync(ServiceFlushAllMessage notification, CancellationToken token)
        {
            await _mailSystem.ProcessMain();
        }
    }
}