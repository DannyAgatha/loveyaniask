using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BazaarServer.Managers;
using WingsAPI.Communication;
using WingsAPI.Communication.Bazaar;
using WingsAPI.Data.Bazaar;
using WingsEmu.Health;

namespace BazaarServer.Services
{
    public class BazaarService : IBazaarService
    {
        private readonly BazaarManager _bazaarManager;
        private readonly IMaintenanceManager _maintenanceManager;

        public BazaarService(BazaarManager bazaarManager, IMaintenanceManager maintenanceManager)
        {
            _bazaarManager = bazaarManager;
            _maintenanceManager = maintenanceManager;
        }

        private bool MaintenanceMode => _maintenanceManager.IsMaintenanceActive;

        public async ValueTask<BazaarItemResponse> GetBazaarItemById(BazaarGetItemByIdRequest request)
        {
            if (MaintenanceMode)
            {
                return new BazaarItemResponse
                {
                    ResponseType = RpcResponseType.MAINTENANCE_MODE
                };
            }

            BazaarItemDTO bazaarItem = await _bazaarManager.GetBazaarItemById(request.BazaarItemId);
            return new BazaarItemResponse
            {
                ResponseType = bazaarItem == null ? RpcResponseType.GENERIC_SERVER_ERROR : RpcResponseType.SUCCESS,
                BazaarItemDto = bazaarItem
            };
        }

        public async ValueTask<BazaarItemResponse> AddItemToBazaar(BazaarAddItemRequest request)
        {
            if (MaintenanceMode)
            {
                return new BazaarItemResponse
                {
                    ResponseType = RpcResponseType.MAINTENANCE_MODE
                };
            }

            if (request.MaximumListedItems <= _bazaarManager.GetItemsByCharacterId(request.BazaarItemDto.CharacterId)?.Count)
            {
                return new BazaarItemResponse
                {
                    ResponseType = RpcResponseType.GENERIC_SERVER_ERROR
                };
            }

            BazaarItemDTO bazaarItemDto = await _bazaarManager.SaveAsync(request.BazaarItemDto);
            return new BazaarItemResponse
            {
                ResponseType = RpcResponseType.SUCCESS,
                BazaarItemDto = bazaarItemDto
            };
        }

        public async ValueTask<BazaarItemResponse> RemoveItemFromBazaar(BazaarRemoveItemRequest request)
        {
            if (MaintenanceMode)
            {
                return new BazaarItemResponse
                {
                    ResponseType = RpcResponseType.MAINTENANCE_MODE
                };
            }

            BazaarItemDTO deletedItem = await _bazaarManager.DeleteItemWithDto(request.BazaarItemDto, request.RequesterCharacterId);

            if (deletedItem == null)
            {
                return new BazaarItemResponse
                {
                    ResponseType = RpcResponseType.GENERIC_SERVER_ERROR
                };
            }

            return new BazaarItemResponse
            {
                ResponseType = RpcResponseType.SUCCESS,
                BazaarItemDto = deletedItem
            };
        }

        public async ValueTask<BazaarItemResponse> ChangeItemPriceFromBazaar(BazaarChangeItemPriceRequest request)
        {
            if (MaintenanceMode)
            {
                return new BazaarItemResponse
                {
                    ResponseType = RpcResponseType.MAINTENANCE_MODE
                };
            }

            BazaarItemDTO updatedItem = await _bazaarManager.ChangeItemPriceWithDto(request.BazaarItemDto, request.ChangerCharacterId, request.NewPrice, request.NewSaleFee);
            if (updatedItem == null)
            {
                return new BazaarItemResponse
                {
                    ResponseType = RpcResponseType.GENERIC_SERVER_ERROR,
                    BazaarItemDto = null
                };
            }

            return new BazaarItemResponse
            {
                ResponseType = RpcResponseType.SUCCESS,
                BazaarItemDto = updatedItem
            };
        }

        public async ValueTask<BazaarGetItemsByCharIdResponse> GetItemsByCharacterIdFromBazaar(BazaarGetItemsByCharIdRequest request)
        {
            if (MaintenanceMode)
            {
                return new BazaarGetItemsByCharIdResponse
                {
                    ResponseType = RpcResponseType.MAINTENANCE_MODE
                };
            }

            return new BazaarGetItemsByCharIdResponse
            {
                ResponseType = RpcResponseType.SUCCESS,
                BazaarItems = _bazaarManager.GetItemsByCharacterId(request.CharacterId)
            };
        }

        public async ValueTask<BazaarRemoveItemsByCharIdResponse> RemoveItemsByCharacterIdFromBazaar(BazaarRemoveItemsByCharIdRequest request)
        {
            if (MaintenanceMode)
            {
                return new BazaarRemoveItemsByCharIdResponse
                {
                    ResponseType = RpcResponseType.MAINTENANCE_MODE
                };
            }

            ICollection<BazaarItemDTO> items = _bazaarManager.GetItemsByCharacterId(request.CharacterId);

            if (items == null)
            {
                return new BazaarRemoveItemsByCharIdResponse
                {
                    ResponseType = RpcResponseType.SUCCESS
                };
            }

            foreach (BazaarItemDTO item in items)
            {
                await _bazaarManager.DeleteItemWithDto(item, request.CharacterId);
            }

            return new BazaarRemoveItemsByCharIdResponse
            {
                ResponseType = RpcResponseType.SUCCESS
            };
        }

        public async ValueTask<BazaarSearchBazaarItemsResponse> SearchBazaarItems(BazaarSearchBazaarItemsRequest request)
        {
            if (MaintenanceMode)
            {
                return new BazaarSearchBazaarItemsResponse
                {
                    ResponseType = RpcResponseType.MAINTENANCE_MODE
                };
            }

            return new BazaarSearchBazaarItemsResponse
            {
                ResponseType = RpcResponseType.SUCCESS,
                BazaarItemDtos = _bazaarManager.SearchBazaarItems(request.BazaarSearchContext)
            };
        }

        public async ValueTask<BazaarItemResponse> BuyItemFromBazaar(BazaarBuyItemRequest request)
        {
            if (MaintenanceMode)
            {
                return new BazaarItemResponse
                {
                    ResponseType = RpcResponseType.MAINTENANCE_MODE
                };
            }

            BazaarItemDTO cachedItem = await _bazaarManager.BuyItemWithExpectedValues(request.BazaarItemId, request.BuyerCharacterId, request.Amount, request.PricePerItem);
            if (cachedItem == null)
            {
                return new BazaarItemResponse
                {
                    BazaarItemDto = null,
                    ResponseType = RpcResponseType.GENERIC_SERVER_ERROR
                };
            }

            return new BazaarItemResponse
            {
                BazaarItemDto = cachedItem,
                ResponseType = RpcResponseType.SUCCESS
            };
        }

        public async ValueTask<UnlistItemFromBazaarResponse> UnlistItemsFromBazaarWithVnumAsync(UnlistItemFromBazaarRequest request)
        {
            IReadOnlyCollection<BazaarItemDTO> itemsToUnlist = _bazaarManager.SearchBazaarItems(new BazaarSearchContext
            {
                ItemVNumFilter = request.Vnum,
                Index = 0,
                AmountOfItemsPerIndex = 10000
            });
            List<BazaarItemDTO> unlistedItems = await _bazaarManager.UnlistItemsWithVnums(itemsToUnlist.ToList());

            return new UnlistItemFromBazaarResponse
            {
                UnlistedItems = unlistedItems.Count
            };
        }

        public async ValueTask<UnlistItemFromBazaarResponse> UnlistCharacterItemsFromBazaarAsync(UnlistCharacterItemsFromBazaarRequest request)
        {
            ICollection<BazaarItemDTO> itemsToUnlist = _bazaarManager.GetItemsByCharacterId(request.Id);

            List<BazaarItemDTO> unlistedItems = await _bazaarManager.UnlistItemsWithVnums(itemsToUnlist.ToList());

            return new UnlistItemFromBazaarResponse
            {
                UnlistedItems = unlistedItems.Count
            };
        }
    }
}