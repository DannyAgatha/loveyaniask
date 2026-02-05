using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;
using ProtoBuf;

namespace WingsAPI.Communication.Bazaar
{
    [ServiceContract]
    public interface IBazaarService
    {
        [OperationContract]
        ValueTask<BazaarItemResponse> GetBazaarItemById(BazaarGetItemByIdRequest request);

        [OperationContract]
        ValueTask<BazaarItemResponse> AddItemToBazaar(BazaarAddItemRequest request);

        [OperationContract]
        ValueTask<BazaarItemResponse> RemoveItemFromBazaar(BazaarRemoveItemRequest request);

        [OperationContract]
        ValueTask<BazaarItemResponse> ChangeItemPriceFromBazaar(BazaarChangeItemPriceRequest request);

        [OperationContract]
        ValueTask<BazaarGetItemsByCharIdResponse> GetItemsByCharacterIdFromBazaar(BazaarGetItemsByCharIdRequest request);

        [OperationContract]
        ValueTask<BazaarRemoveItemsByCharIdResponse> RemoveItemsByCharacterIdFromBazaar(BazaarRemoveItemsByCharIdRequest request);

        [OperationContract]
        ValueTask<BazaarSearchBazaarItemsResponse> SearchBazaarItems(BazaarSearchBazaarItemsRequest request);

        [OperationContract]
        ValueTask<BazaarItemResponse> BuyItemFromBazaar(BazaarBuyItemRequest request);

        [OperationContract]
        ValueTask<UnlistItemFromBazaarResponse> UnlistItemsFromBazaarWithVnumAsync(UnlistItemFromBazaarRequest request);

        [OperationContract]
        ValueTask<UnlistItemFromBazaarResponse> UnlistCharacterItemsFromBazaarAsync(UnlistCharacterItemsFromBazaarRequest request);
    }

    [ProtoContract]
    public class UnlistItemFromBazaarRequest
    {
        [ProtoMember(1)]
        public List<int> Vnum { get; set; }
    }

    [ProtoContract]
    public class UnlistCharacterItemsFromBazaarRequest
    {
        [ProtoMember(1)]
        public int Id { get; set; }
    }

    [ProtoContract]
    public class UnlistItemFromBazaarResponse
    {
        [ProtoMember(1)]
        public int UnlistedItems { get; set; }
    }
}