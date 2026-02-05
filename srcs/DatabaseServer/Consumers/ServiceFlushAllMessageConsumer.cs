using System.Threading;
using System.Threading.Tasks;
using DatabaseServer.Managers;
using PhoenixLib.ServiceBus;
using WingsAPI.Communication.Services.Messages;

namespace DatabaseServer.Consumers
{
    public class ServiceFlushAllMessageConsumer : IMessageConsumer<ServiceFlushAllMessage>
    {
        private readonly IAccountWarehouseManager _accountWarehouseManager;
        private readonly ICharacterManager _characterManager;
        private readonly ITimeSpaceManager _timeSpaceManager;

        public ServiceFlushAllMessageConsumer(ICharacterManager characterManager, IAccountWarehouseManager accountWarehouseManager, ITimeSpaceManager timeSpaceManager)
        {
            _characterManager = characterManager;
            _accountWarehouseManager = accountWarehouseManager;
            _timeSpaceManager = timeSpaceManager;
        }

        public async Task HandleAsync(ServiceFlushAllMessage notification, CancellationToken token)
        {
            await _characterManager.FlushCharacterSaves();
            await _accountWarehouseManager.FlushWarehouseSaves();
            await _timeSpaceManager.FlushTimeSpaceRecords();
        }
    }
}