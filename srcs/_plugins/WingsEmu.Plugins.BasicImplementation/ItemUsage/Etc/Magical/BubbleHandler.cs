using System.Threading.Tasks;
using WingsEmu.Game._i18n;
using WingsEmu.Game._ItemUsage;
using WingsEmu.Game._ItemUsage.Event;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;

namespace NosEmu.Plugins.BasicImplementations.ItemUsage.Etc.Magical;

public class BubbleHandler : IItemHandler
{
    private readonly IGameLanguageService _languageService;

    public BubbleHandler(IGameLanguageService languageService) => _languageService = languageService;

    public ItemType ItemType => ItemType.Magical;
    public long[] Effects => new long[] { 16 };

    public async Task HandleAsync(IClientSession session, InventoryUseItemEvent e)
    {
        IPlayerEntity character = session.PlayerEntity;

        if (character.IsOnVehicle)
        {
            string message = _languageService.GetLanguage(GameDialogKey.ITEM_CHATMESSAGE_CANT_USE_THAT, session.UserLanguage);
            session.SendChatMessage(message, ChatMessageColorType.Yellow);
            return;
        }

        if (session.IsMuted())
        {
            session.SendMuteMessage();
            return;
        }

        if (e.Option != 0)
        {
            return;
        }

        session.SendGuriPacket(10, 4, 1);
    }
}