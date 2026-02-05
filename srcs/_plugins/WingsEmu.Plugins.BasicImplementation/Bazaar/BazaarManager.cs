using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PhoenixLib.Caching;
using PhoenixLib.Logging;
using WingsAPI.Communication;
using WingsAPI.Communication.Bazaar;
using WingsAPI.Communication.DbServer.CharacterService;
using WingsAPI.Data.Bazaar;
using WingsEmu.Game.Bazaar;
using WingsEmu.Game.Items;

namespace NosEmu.Plugins.BasicImplementations.Bazaar;

public class BazaarManager : IBazaarManager
{
    private static readonly TimeSpan CacheLifeTime = TimeSpan.FromMinutes(Convert.ToInt32(Environment.GetEnvironmentVariable("BAZAAR_MANAGER_TTL_MINUTES") ?? "30"));

    private readonly IBazaarService _bazaarService;
    private readonly ICharacterService _characterService;
    private readonly IGameItemInstanceFactory _itemInstanceFactory;

    private readonly IKeyValueCache<string> _nameByKey;

    public BazaarManager(IBazaarService bazaarService, ICharacterService characterService, IGameItemInstanceFactory itemInstanceFactory, IKeyValueCache<string> nameByKey)
    {
        _bazaarService = bazaarService;
        _characterService = characterService;
        _itemInstanceFactory = itemInstanceFactory;
        _nameByKey = nameByKey;
    }

    public async Task<string> GetOwnerName(long characterId)
    {
        string name = _nameByKey.Get(GetKey(characterId));
        if (name != null)
        {
            SetName(characterId, name);
            return name;
        }

        DbServerGetCharacterResponse response = null;
        try
        {
            response = await _characterService.GetCharacterById(new DbServerGetCharacterByIdRequest
            {
                CharacterId = characterId
            });
        }
        catch (Exception e)
        {
            Log.Error("[BAZAAR_MANAGER][GetCharacterName] Unexpected error:", e);
        }

        if (response?.RpcResponseType != RpcResponseType.SUCCESS)
        {
            return string.Empty;
        }

        name = response.CharacterDto.Name;
        SetName(characterId, name);
        return name;
    }

    public async Task<BazaarItem> GetBazaarItemById(long bazaarItemId)
    {
        BazaarItemResponse response = null;
        try
        {
            response = await _bazaarService.GetBazaarItemById(new BazaarGetItemByIdRequest
            {
                BazaarItemId = bazaarItemId
            });
        }
        catch (Exception e)
        {
            Log.Error("[BAZAAR_MANAGER][GetBazaarItemById] Unexpected error:", e);
        }

        if (response?.ResponseType != RpcResponseType.SUCCESS)
        {
            return null;
        }

        BazaarItemDTO bazaarItemDto = response.BazaarItemDto;
        return new BazaarItem(bazaarItemDto, _itemInstanceFactory.CreateItem(bazaarItemDto.ItemInstance), await GetOwnerName(bazaarItemDto.CharacterId));
    }

    public async Task<IReadOnlyCollection<BazaarItem>> GetListedItemsByCharacterId(long characterId)
    {
        BazaarGetItemsByCharIdResponse response = null;
        try
        {
            response = await _bazaarService.GetItemsByCharacterIdFromBazaar(new BazaarGetItemsByCharIdRequest
            {
                CharacterId = characterId
            });
        }
        catch (Exception e)
        {
            Log.Error("[BAZAAR_MANAGER][GetListedItemsByCharacterId]", e);
        }

        if (response?.ResponseType != RpcResponseType.SUCCESS)
        {
            return null;
        }

        return await GetInstance(response.BazaarItems);
    }

    public async Task<(IReadOnlyCollection<BazaarItem>, RpcResponseType)> SearchBazaarItems(BazaarSearchContext bazaarSearchContext)
    {
        if (bazaarSearchContext == null)
        {
            return (null, RpcResponseType.UNKNOWN_ERROR);
        }

        BazaarSearchBazaarItemsResponse response;

        try
        {
            response = await _bazaarService.SearchBazaarItems(new BazaarSearchBazaarItemsRequest
            {
                BazaarSearchContext = bazaarSearchContext
            });
        }
        catch (Exception e)
        {
            Log.Error("[BAZAAR_MANAGER][SearchBazaarItems] Unexpected error during item search.", e);
            return (null, RpcResponseType.UNKNOWN_ERROR);
        }

        return response == null ? (null, RpcResponseType.UNKNOWN_ERROR) : (await GetInstance(response.BazaarItemDtos), response.ResponseType);
    }

    private static string GetKey(long characterId) => $"bazaar:name:character-id:{characterId.ToString()}";

    private void SetName(long characterId, string name)
    {
        _nameByKey.Set(GetKey(characterId), name, CacheLifeTime);
    }

    private void RemoveName(long characterId)
    {
        _nameByKey.Remove(GetKey(characterId));
    }

    private async Task<List<BazaarItem>> GetInstance(IEnumerable<BazaarItemDTO> bazaarItems)
    {
        if (bazaarItems == null)
        {
            return null;
        }

        List<BazaarItem> bazaarInstances = new();
        foreach (BazaarItemDTO bazaarItemDto in bazaarItems)
        {
            bazaarInstances.Add(new BazaarItem(bazaarItemDto, _itemInstanceFactory.CreateItem(bazaarItemDto.ItemInstance), await GetOwnerName(bazaarItemDto.CharacterId)));
        }

        return bazaarInstances;
    }
}