using WingsAPI.Data.Miniland;
using WingsEmu.Game.Inventory;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Miniland;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;

namespace NosEmu.Plugins.BasicImplementations.Factories;

public interface IMapDesignObjectFactory
{
    MapDesignObject CreateGameObject(long characterId, CharacterMinilandObjectDto dto);
}

public class MapDesignObjectFactory : IMapDesignObjectFactory
{
    private readonly ISessionManager _sessionManager;

    public MapDesignObjectFactory(ISessionManager sessionManager) => _sessionManager = sessionManager;

    public MapDesignObject CreateGameObject(long characterId, CharacterMinilandObjectDto dto)
    {
        IClientSession session = _sessionManager.GetSessionByCharacterId(characterId);

        InventoryItem item = session.PlayerEntity.GetItemBySlotAndType(dto.InventorySlot, InventoryType.Miniland);
        if (item == null)
        {
            return null;
        }

        return new MapDesignObject
        {
            Id = dto.Id,
            CharacterId = characterId,
            InventorySlot = item.Slot,
            InventoryItem = item,
            Level1BoxAmount = dto.Level1BoxAmount,
            Level2BoxAmount = dto.Level2BoxAmount,
            Level3BoxAmount = dto.Level3BoxAmount,
            Level4BoxAmount = dto.Level4BoxAmount,
            Level5BoxAmount = dto.Level5BoxAmount,
            MapX = dto.MapX,
            MapY = dto.MapY
        };
    }
}