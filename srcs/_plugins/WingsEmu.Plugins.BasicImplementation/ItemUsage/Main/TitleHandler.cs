using System.Threading.Tasks;
using WingsEmu.Game._i18n;
using WingsEmu.Game._ItemUsage;
using WingsEmu.Game._ItemUsage.Event;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;

namespace NosEmu.Plugins.BasicImplementations.ItemUsage.Main;

public class TitleHandler : IItemHandler
{
    private readonly IGameLanguageService _gameLanguage;

    public TitleHandler(IGameLanguageService gameLanguage) => _gameLanguage = gameLanguage;

    public ItemType ItemType => ItemType.Title;
    public long[] Effects => new long[] { 0 };

    public async Task HandleAsync(IClientSession session, InventoryUseItemEvent e) => session.SendQnaPacket($"guri 306 {e.Item.ItemInstance.ItemVNum} {e.Item.Slot}",
        _gameLanguage.GetLanguage(GameDialogKey.TITLE_DIALOG_ASK_ADD, session.UserLanguage));
}