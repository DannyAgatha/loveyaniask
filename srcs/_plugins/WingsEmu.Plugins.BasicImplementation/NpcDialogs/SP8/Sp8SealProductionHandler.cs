using System.Threading.Tasks;
using WingsEmu.Game._i18n;
using WingsEmu.Game._NpcDialog;
using WingsEmu.Game._NpcDialog.Event;
using WingsEmu.Game.Items;
using WingsEmu.Game.Managers.StaticData;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;

namespace NosEmu.Plugins.BasicImplementations.NpcDialogs.SP8;

public class Sp8SealProductionHandler : INpcDialogAsyncHandler
{
    private readonly IGameItemInstanceFactory _gameItemInstanceFactory;
    private readonly IGameLanguageService _gameLanguage;
    private readonly IItemsManager _itemsManager;

    public Sp8SealProductionHandler(IGameLanguageService gameLanguage, IItemsManager itemsManager, IGameItemInstanceFactory gameItemInstanceFactory)
    {
        _gameLanguage = gameLanguage;
        _itemsManager = itemsManager;
        _gameItemInstanceFactory = gameItemInstanceFactory;
    }

    public NpcRunType[] NpcRunTypes => new[] { NpcRunType.SP8_PRODUCTION_5_SEED_DAMNATION_SEAL };

    public async Task Execute(IClientSession session, NpcDialogEvent e)
    {
        /*INpcEntity npcEntity = session.CurrentMapInstance.GetNpcById(e.NpcId);
        string seed =  _gameLanguage.GetLanguage(GameDataType.Item, _itemsManager.GetItem((short)ItemVnums.SEED_OF_DAMNATION).Name, session.UserLanguage);
        string sealName =  _gameLanguage.GetLanguage(GameDataType.Item, _itemsManager.GetItem((short)ItemVnums.LAURENA_SEAL).Name, session.UserLanguage);
        
        if (npcEntity == null)
        {
            return;
        }

        if (session.CurrentMapInstance.MapInstanceType != MapInstanceType.BaseMapInstance)
        {
            return;
        }

        if (!session.PlayerEntity.HasItem((short)ItemVnums.SEED_OF_DAMNATION, 5))
        {
            int amount = session.CountMissingItems((short)ItemVnums.SEED_OF_DAMNATION, 5);
            session.SendChatMessage( _gameLanguage.GetLanguageFormat(GameDialogKey.INVENTORY_SHOUTMESSAGE_NOT_ENOUGH_ITEMS, session.UserLanguage, amount, seed), ChatMessageColorType.Yellow);
            return;
        }
        
        GameItemInstance seal = _gameItemInstanceFactory.CreateItem((short)ItemVnums.LAURENA_SEAL, 2);
        await session.AddNewItemToInventory(seal, true, ChatMessageColorType.Yellow, true);
        await session.RemoveItemFromInventory((short)ItemVnums.SEED_OF_DAMNATION, 5);*/
    }
}