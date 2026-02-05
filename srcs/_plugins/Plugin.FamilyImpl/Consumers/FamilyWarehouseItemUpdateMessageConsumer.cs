using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.ServiceBus;
using Plugin.FamilyImpl.Messages;
using WingsAPI.Data.Families;
using WingsAPI.Game.Extensions.Families;
using WingsAPI.Packets.Enums.Families;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Managers.StaticData;
using WingsEmu.Game.Networking;

namespace Plugin.FamilyImpl.Consumers
{
    public class FamilyWarehouseItemUpdateMessageConsumer : IMessageConsumer<FamilyWarehouseItemUpdateMessage>
    {
        private readonly IFamilyWarehouseManager _familyWarehouseManager;
        private readonly IItemsManager _itemsManager;
        private readonly ISessionManager _sessionManager;

        public FamilyWarehouseItemUpdateMessageConsumer(ISessionManager sessionManager, IItemsManager itemsManager, IFamilyWarehouseManager familyWarehouseManager)
        {
            _sessionManager = sessionManager;
            _itemsManager = itemsManager;
            _familyWarehouseManager = familyWarehouseManager;
        }

        public async Task HandleAsync(FamilyWarehouseItemUpdateMessage notification, CancellationToken token)
        {
            await _familyWarehouseManager.UpdateWarehouseItem(notification.FamilyId, notification.UpdatedItems);

            foreach (IClientSession session in _sessionManager.Sessions)
            {
                if (!session.PlayerEntity.IsFamilyWarehouseOpen || session.PlayerEntity.Family?.Id != notification.FamilyId)
                {
                    continue;
                }

                if (!session.CheckPutWithdrawPermission(FamilyWarehouseAuthorityType.Put))
                {
                    continue;
                }

                foreach ((FamilyWarehouseItemDto dto, short slot) in notification.UpdatedItems)
                {
                    if (dto == null)
                    {
                        session.SendFamilyWarehouseRemoveItem(slot);
                    }
                    else
                    {
                        session.SendFamilyWarehouseAddItem(_itemsManager, dto);
                    }
                }
            }
        }
    }
}