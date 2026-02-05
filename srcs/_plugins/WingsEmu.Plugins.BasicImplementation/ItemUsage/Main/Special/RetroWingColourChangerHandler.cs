using System.Threading.Tasks;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsEmu.Game;
using WingsEmu.Game._i18n;
using WingsEmu.Game._ItemUsage;
using WingsEmu.Game._ItemUsage.Event;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums.Chat;

namespace NosEmu.Plugins.BasicImplementations.ItemUsage.Main.Special;

public class RetroWingColourChangerHandler : IItemUsageByVnumHandler
{
    private readonly IGameLanguageService _languageService;

    public RetroWingColourChangerHandler(IGameLanguageService languageService)
    {
        _languageService = languageService;
    }

    public long[] Vnums => new long[] { 9781 };

    public async Task HandleAsync(IClientSession session, InventoryUseItemEvent e)
    {
        if (!session.HasCurrentMapInstance)
        {
            return;
        }

        if (session.PlayerEntity.HasShopOpened)
        {
            return;
        }

        if (session.PlayerEntity.IsOnVehicle)
        {
            return;
        }

        if (!session.PlayerEntity.UseSp || session.PlayerEntity.Specialist == null)
        {
            session.SendMsg(_languageService.GetLanguage(GameDialogKey.INFORMATION_SHOUTMESSAGE_NO_SPECIALIST_CARD, session.UserLanguage), MsgMessageType.Middle);
            return;
        }

        if (session.PlayerEntity.Specialist.Design == 0)
        {
            session.SendMsg(_languageService.GetLanguage(GameDialogKey.NEED_WINGS, session.UserLanguage), MsgMessageType.Middle);
            return;
        }

        if (session.PlayerEntity.Specialist.Upgrade == 0)
        {
            session.SendMsg(_languageService.GetLanguage(GameDialogKey.INTERACTION_SHOUTMESSAGE_NEED_SP_UPGRADE_FOR_WINGS, session.UserLanguage), MsgMessageType.Middle);
            return;
        }

        if (e.Option == 0)
        {
            session.SendQnaPacket($"u_i 1 {session.PlayerEntity.Id} {(byte)e.Item.ItemInstance.GameItem.Type} {e.Item.Slot} 3",
                _languageService.GetLanguage(GameDialogKey.ITEM_DIALOG_ASK_CHANGE_SP_WINGS, session.UserLanguage));
            return;
        }

        if (session.PlayerEntity.MorphUpgrade2 < 21)
        {
            session.SendChatMessage(session.GetLanguageFormat(GameDialogKey.EXCEPTION_RETRO_WINGS), ChatMessageColorType.Yellow);
            return;
        }

        int rnd = StaticRandomGenerator.Instance.RandomNumber(21, 28);

        if (session.PlayerEntity.MorphUpgrade2 == rnd || session.PlayerEntity.MorphUpgrade2 == 25) // Preventing the rnd number from falling on the current wings
        {
            rnd++;
        }

        session.PlayerEntity.Specialist.Design = (short)rnd;
        session.PlayerEntity.MorphUpgrade2 = rnd;

        switch (session.PlayerEntity.MorphUpgrade2)
        {
            case 21: // Green Retro
                session.PlayerEntity.Specialist.AppearanceWingsVNum = 9445;
                break;
            case 22: // Pink Retro
                session.PlayerEntity.Specialist.AppearanceWingsVNum = 9446;
                break;
            case 23: // Yellow Retro
                session.PlayerEntity.Specialist.AppearanceWingsVNum = 9447;
                break;
            case 24: // Purple Retro
                session.PlayerEntity.Specialist.AppearanceWingsVNum = 9448;
                break;
            case 26: // Magenta Retro
                session.PlayerEntity.Specialist.AppearanceWingsVNum = 9450;
                break;
            case 27: // Cyan Retro 
                session.PlayerEntity.Specialist.AppearanceWingsVNum = 9451;
                break;
            case 28: // Red Retro
                session.PlayerEntity.Specialist.AppearanceWingsVNum = 9449;
                break;
        }

        session.PlayerEntity.Specialist.CanResetWingsAppearance = true;

        session.BroadcastCMode();
        session.RefreshStat();
        session.RefreshStatChar();

        await session.RemoveItemFromInventory(item: e.Item);
    }
}