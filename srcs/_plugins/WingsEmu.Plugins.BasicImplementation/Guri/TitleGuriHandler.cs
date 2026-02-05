using System.Linq;
using System.Threading.Tasks;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsEmu.DTOs.Titles;
using WingsEmu.Game._Guri;
using WingsEmu.Game._Guri.Event;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Items;
using WingsEmu.Game.Managers.StaticData;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;

namespace NosEmu.Plugins.BasicImplementations.Guri;

public class TitleGuriHandler : IGuriHandler
{
    private readonly IItemsManager _itemManager;

    public TitleGuriHandler(IItemsManager itemManager) => _itemManager = itemManager;

    public long GuriEffectId => 306;

    public async Task ExecuteAsync(IClientSession session, GuriEvent e)
    {
        if (e.User == null)
        {
            return;
        }

        IGameItem title = _itemManager.GetItem(e.Data);

        if (title == null)
        {
            return;
        }

        if (title.ItemType != ItemType.Title)
        {
            return;
        }

        if (session.PlayerEntity.GetFirstItemByVnum(title.Id) == null)
        {
            return;
        }

        if (session.PlayerEntity.Titles.Any(s => s.ItemVnum == title.Id))
        {
            return;
        }

        session.PlayerEntity.Titles.Add(new CharacterTitleDto
        {
            TitleId = _itemManager.GetTitleId(title.Id),
            ItemVnum = title.Id
        });

        await session.RemoveItemFromInventory(title.Id);
        session.SendInfo(session.GetLanguage(GameDialogKey.TITLE_INFO_UNLOCKED));
        session.SendTitlePacket();
    }
}