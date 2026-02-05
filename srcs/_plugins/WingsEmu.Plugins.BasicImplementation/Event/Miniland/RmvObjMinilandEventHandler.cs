using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsAPI.Game.Extensions.MinilandExtensions;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Miniland;
using WingsEmu.Game.Miniland.Events;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;

namespace NosEmu.Plugins.BasicImplementations.Event.Miniland;

public class RmvObjMinilandEventHandler : IAsyncEventProcessor<RmvObjMinilandEvent>
{
    private readonly IGameLanguageService _languageService;
    private readonly IMinilandManager _minilandManager;

    public RmvObjMinilandEventHandler(IMinilandManager minilandManager, IGameLanguageService languageService)
    {
        _minilandManager = minilandManager;
        _languageService = languageService;
    }

    public async Task HandleAsync(RmvObjMinilandEvent e, CancellationToken cancellation)
    {
        IClientSession session = e.Sender;
        IPlayerEntity character = e.Sender.PlayerEntity;
        if (character.MapInstanceId != character.Miniland.Id)
        {
            return;
        }

        if (character.MinilandState != MinilandState.LOCK)
        {
            session.SendMsg(session.GetLanguage(GameDialogKey.MINILAND_SHOUTMESSAGE_NEED_BE_LOCKED), MsgMessageType.Middle);
            return;
        }

        MapDesignObject mapObject = character.Miniland.MapDesignObjects.FirstOrDefault(x => x.InventoryItem.Slot == e.Slot);
        if (mapObject == default)
        {
            return;
        }

        if (mapObject.InventoryItem.ItemInstance.GameItem.IsWarehouse)
        {
            character.WareHouseSize = 0;
        }

        if (mapObject.InventoryItem.ItemInstance.GameItem.ItemType == ItemType.House)
        {
            _minilandManager.RelativeUpdateMinilandCapacity(e.Sender.PlayerEntity.Id, -mapObject.InventoryItem.ItemInstance.GameItem.MinilandObjectPoint);
        }

        character.Miniland.MapDesignObjects.Remove(mapObject);
        character.Miniland.Broadcast(mapObject.GenerateEffect(true));
        character.Miniland.Broadcast(mapObject.GenerateMinilandObject(true));
    }
}