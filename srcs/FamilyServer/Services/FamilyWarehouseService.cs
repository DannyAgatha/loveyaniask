using System.Collections.Generic;
using System.Threading.Tasks;
using FamilyServer.Managers;
using PhoenixLib.ServiceBus;
using Plugin.FamilyImpl.Messages;
using WingsAPI.Communication;
using WingsAPI.Communication.Families.Warehouse;
using WingsAPI.Data.Families;

namespace FamilyServer.Services
{
    public class FamilyWarehouseService : IFamilyWarehouseService
    {
        private readonly IFamilyWarehouseManager _familyWarehouseManager;
        private readonly IMessagePublisher<FamilyWarehouseItemUpdateMessage> _messagePublisher;

        public FamilyWarehouseService(IFamilyWarehouseManager familyWarehouseManager, IMessagePublisher<FamilyWarehouseItemUpdateMessage> messagePublisher)
        {
            _familyWarehouseManager = familyWarehouseManager;
            _messagePublisher = messagePublisher;
        }

        public async ValueTask<FamilyWarehouseGetLogsResponse> GetLogs(FamilyWarehouseGetLogsRequest request)
        {
            IList<FamilyWarehouseLogEntryDto> logs = await _familyWarehouseManager.GetWarehouseLogs(request.FamilyId, request.CharacterId);

            return new FamilyWarehouseGetLogsResponse
            {
                ResponseType = logs == null ? RpcResponseType.GENERIC_SERVER_ERROR : RpcResponseType.SUCCESS,
                Logs = logs
            };
        }

        public async ValueTask<FamilyWarehouseGetItemsResponse> GetItems(FamilyWarehouseGetItemsRequest request)
        {
            IEnumerable<FamilyWarehouseItemDto> warehouseItemDtos = await _familyWarehouseManager.GetWarehouse(request.FamilyId, request.CharacterId);

            return new FamilyWarehouseGetItemsResponse
            {
                ResponseType = warehouseItemDtos == null ? RpcResponseType.GENERIC_SERVER_ERROR : RpcResponseType.SUCCESS,
                Items = warehouseItemDtos
            };
        }

        public async ValueTask<FamilyWarehouseGetItemResponse> GetItem(FamilyWarehouseGetItemRequest request)
        {
            FamilyWarehouseItemDto warehouseItemDto = await _familyWarehouseManager.GetWarehouseItem(request.FamilyId, request.Slot, request.CharacterId);

            return new FamilyWarehouseGetItemResponse
            {
                ResponseType = warehouseItemDto == null ? RpcResponseType.GENERIC_SERVER_ERROR : RpcResponseType.SUCCESS,
                Item = warehouseItemDto
            };
        }

        public async ValueTask<FamilyWarehouseAddItemResponse> AddItem(FamilyWarehouseAddItemRequest request)
        {
            AddWarehouseItemResult result = await _familyWarehouseManager.AddWarehouseItem(request.Item, request.CharacterId, request.CharacterName);

            if (result.Success)
            {
                await _messagePublisher.PublishAsync(new FamilyWarehouseItemUpdateMessage
                {
                    FamilyId = request.Item.FamilyId,
                    UpdatedItems = new[]
                    {
                        (result.UpdatedItem, request.Item.Slot)
                    }
                });
            }

            return new FamilyWarehouseAddItemResponse
            {
                ResponseType = result.Success ? RpcResponseType.SUCCESS : RpcResponseType.GENERIC_SERVER_ERROR,
                Item = result.UpdatedItem
            };
        }

        public async ValueTask<FamilyWarehouseWithdrawItemResponse> WithdrawItem(FamilyWarehouseWithdrawItemRequest request)
        {
            WithdrawWarehouseItemResult result = await _familyWarehouseManager.WithdrawWarehouseItem(request.ItemToWithdraw, request.Amount, request.CharacterId, request.CharacterName);

            if (result.Success)
            {
                await _messagePublisher.PublishAsync(new FamilyWarehouseItemUpdateMessage
                {
                    FamilyId = request.ItemToWithdraw.FamilyId,
                    UpdatedItems = new[]
                    {
                        (result.UpdatedItem, request.ItemToWithdraw.Slot)
                    }
                });
            }

            return new FamilyWarehouseWithdrawItemResponse
            {
                ResponseType = result.Success ? RpcResponseType.SUCCESS : RpcResponseType.GENERIC_SERVER_ERROR,
                UpdatedItem = result.UpdatedItem,
                WithdrawnItem = result.WithdrawnItem
            };
        }

        public async ValueTask<FamilyWarehouseMoveItemResponse> MoveItem(FamilyWarehouseMoveItemRequest request)
        {
            MoveWarehouseItemResult result = await _familyWarehouseManager.MoveWarehouseItem(request.WarehouseItemDtoToMove, request.Amount, request.NewSlot, request.CharacterId);

            if (result.Success)
            {
                await _messagePublisher.PublishAsync(new FamilyWarehouseItemUpdateMessage
                {
                    FamilyId = request.WarehouseItemDtoToMove.FamilyId,
                    UpdatedItems = new[]
                    {
                        (result.OldItem, request.WarehouseItemDtoToMove.Slot),
                        (result.NewItem, request.NewSlot)
                    }
                });
            }

            return new FamilyWarehouseMoveItemResponse
            {
                ResponseType = result.Success ? RpcResponseType.SUCCESS : RpcResponseType.GENERIC_SERVER_ERROR,
                OldItem = result.OldItem,
                NewItem = result.NewItem
            };
        }
    }
}