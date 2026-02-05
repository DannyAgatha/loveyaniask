using System.Threading.Tasks;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsEmu.Game._enum;
using WingsEmu.Game._i18n;
using WingsEmu.Game._NpcDialog;
using WingsEmu.Game._NpcDialog.Event;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Items;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;

namespace NosEmu.Plugins.BasicImplementations.NpcDialogs;

public class BankSavingBookHandler : INpcDialogAsyncHandler
{
    private readonly IGameItemInstanceFactory _gameItemInstanceFactory;
    private readonly IGameLanguageService _langService;

    public BankSavingBookHandler(IGameLanguageService langService, IGameItemInstanceFactory gameItemInstanceFactory)
    {
        _langService = langService;
        _gameItemInstanceFactory = gameItemInstanceFactory;
    }

    public NpcRunType[] NpcRunTypes => new[] { NpcRunType.GET_BANK_BOOK };

    public async Task Execute(IClientSession session, NpcDialogEvent e)
    {
        INpcEntity npcEntity = session.CurrentMapInstance.GetNpcById(e.NpcId);
        if (npcEntity == null)
        {
            return;
        }

        if (session.CantPerformActionOnAct4())
        {
            return;
        }

        bool hasItem = session.PlayerEntity.HasItem((short)ItemVnums.CUARRY_BANK_SAVINGS_BOOK);

        if (hasItem)
        {
            session.SendSayi(ChatMessageColorType.Yellow, Game18NConstString.YouHaveAlreadyReceivedIt);
            return;
        }

        GameItemInstance item = _gameItemInstanceFactory.CreateItem((short)ItemVnums.CUARRY_BANK_SAVINGS_BOOK);
        await session.AddNewItemToInventory(item, true, ChatMessageColorType.Yellow, true);
    }
}