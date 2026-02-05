using System.Threading.Tasks;
using WingsEmu.Game._i18n;
using WingsEmu.Game._NpcDialog;
using WingsEmu.Game._NpcDialog.Event;
using WingsEmu.Game.Items;
using WingsEmu.Game.Managers.StaticData;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;

namespace NosEmu.Plugins.BasicImplementations.NpcDialogs.SP8;

public class Sp8PowderProductionHandler : INpcDialogAsyncHandler
{
    private readonly IGameItemInstanceFactory _gameItemInstanceFactory;
    private readonly IGameLanguageService _gameLanguage;
    private readonly IItemsManager _itemsManager;

    public Sp8PowderProductionHandler(IGameLanguageService gameLanguage, IItemsManager itemsManager, IGameItemInstanceFactory gameItemInstanceFactory)
    {
        _gameLanguage = gameLanguage;
        _itemsManager = itemsManager;
        _gameItemInstanceFactory = gameItemInstanceFactory;
    }

    public NpcRunType[] NpcRunTypes => new[] { NpcRunType.SP8_PRODUCTION_SOUL_SLIVER_3_POWDER };

    public async Task Execute(IClientSession session, NpcDialogEvent e)
    {
        /*INpcEntity npcEntity = session.CurrentMapInstance.GetNpcById(e.NpcId);
        string sliver =  _gameLanguage.GetLanguage(GameDataType.Item, _itemsManager.GetItem((short)ItemVnums.SOUL_SLIVER).Name, session.UserLanguage);
        
        if (npcEntity == null)
        {
            return;
        }

        if (session.CurrentMapInstance.MapInstanceType != MapInstanceType.BaseMapInstance)
        {
            return;
        }

        if (!session.PlayerEntity.HasItem((short)ItemVnums.SOUL_SLIVER, 3))
        {
            int amount = session.CountMissingItems((short)ItemVnums.SOUL_SLIVER, 3);
            session.SendChatMessage( _gameLanguage.GetLanguageFormat(GameDialogKey.INVENTORY_SHOUTMESSAGE_NOT_ENOUGH_ITEMS, session.UserLanguage, amount, sliver), ChatMessageColorType.Yellow);
            return;
        }

        GameItemInstance powder = _gameItemInstanceFactory.CreateItem((short)ItemVnums.CLEANSING_POWDER);
        await session.AddNewItemToInventory(powder, true, ChatMessageColorType.Yellow, true);
        await session.RemoveItemFromInventory((short)ItemVnums.SOUL_SLIVER, 3);*/
    }
}