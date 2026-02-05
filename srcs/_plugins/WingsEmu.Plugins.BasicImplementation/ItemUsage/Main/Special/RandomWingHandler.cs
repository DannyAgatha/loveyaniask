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

public class RandomWingHandler : IItemUsageByVnumHandler
{
    private readonly IGameLanguageService _languageService;

    public RandomWingHandler(IGameLanguageService languageService)
    {
        _languageService = languageService;
    }

    public long[] Vnums => new long[] { 9591 };

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

        int rnd = StaticRandomGenerator.Instance.RandomNumber(1, 21);

        if (session.PlayerEntity.MorphUpgrade2 == rnd) // Preventing the rnd number from falling on the current wings
        {
            rnd++;
        }

        session.PlayerEntity.Specialist.Design = (short)rnd;
        session.PlayerEntity.MorphUpgrade2 = rnd;

        switch (session.PlayerEntity.MorphUpgrade2)
        {
            case 1: // Angel Wings
                session.PlayerEntity.Specialist.AppearanceWingsVNum = 1685;
                break;
            case 2: // Devil Wings
                session.PlayerEntity.Specialist.AppearanceWingsVNum = 1686;
                break;
            case 3: // Fire Wings
                session.PlayerEntity.Specialist.AppearanceWingsVNum = 5087;
                break;
            case 4: // Ice Wings
                session.PlayerEntity.Specialist.AppearanceWingsVNum = 5203;
                break;
            case 5: // Golden Eagle Wings
                session.PlayerEntity.Specialist.AppearanceWingsVNum = 9453;
                break;
            case 6: // Titan Wings
                session.PlayerEntity.Specialist.AppearanceWingsVNum = 5372;
                break;
            case 7: // Archangel Wings
                session.PlayerEntity.Specialist.AppearanceWingsVNum = 5431;
                break;
            case 8: // Archdaemon Wings
                session.PlayerEntity.Specialist.AppearanceWingsVNum = 5432;
                break;
            case 9: // Blazing Fire Wings
                session.PlayerEntity.Specialist.AppearanceWingsVNum = 5498;
                break;
            case 10: // Frosty Ice Wings
                session.PlayerEntity.Specialist.AppearanceWingsVNum = 5499;
                break;
            case 11: // Golden Wings
                session.PlayerEntity.Specialist.AppearanceWingsVNum = 5553;
                break;
            case 12: // Onyx Wings
                session.PlayerEntity.Specialist.AppearanceWingsVNum = 5560;
                break;
            case 13: // Fairy Wings
                session.PlayerEntity.Specialist.AppearanceWingsVNum = 5591;
                break;
            case 14: // Mega Titan Wings
                session.PlayerEntity.Specialist.AppearanceWingsVNum = 5837;
                break;
            case 15: // Zephyr Wings
                session.PlayerEntity.Specialist.AppearanceWingsVNum = 5702;
                break;
            case 16: // Lightning Wings
                session.PlayerEntity.Specialist.AppearanceWingsVNum = 5800;
                break;
            case 17: // Blade Wings
                session.PlayerEntity.Specialist.AppearanceWingsVNum = 9176;
                break;
            case 18: // Crystal Wings
                session.PlayerEntity.Specialist.AppearanceWingsVNum = 9212;
                break;
            case 19: // Petal Wings
                session.PlayerEntity.Specialist.AppearanceWingsVNum = 9242;
                break;
            case 20: // Lunar Wings
                session.PlayerEntity.Specialist.AppearanceWingsVNum = 9546;
                break;
        }

        session.PlayerEntity.Specialist.CanResetWingsAppearance = true;

        session.BroadcastCMode();
        session.RefreshStat();
        session.RefreshStatChar();

        await session.RemoveItemFromInventory(item: e.Item);
    }
}