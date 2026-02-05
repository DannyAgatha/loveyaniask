using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.ServiceBus;
using Plugin.FamilyImpl.Messages;

namespace Plugin.FamilyImpl.Consumers
{
    public class FamilyWarehouseLogAddMessageConsumer : IMessageConsumer<FamilyWarehouseLogAddMessage>
    {
        private readonly IFamilyWarehouseManager _familyWarehouseManager;

        public FamilyWarehouseLogAddMessageConsumer(IFamilyWarehouseManager familyWarehouseManager) => _familyWarehouseManager = familyWarehouseManager;

        public async Task HandleAsync(FamilyWarehouseLogAddMessage notification, CancellationToken token)
        {
            await _familyWarehouseManager.AddWarehouseLog(notification.FamilyId, notification.LogToAdd);
        }
    }
}