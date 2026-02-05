using System.Threading.Tasks;
using WingsEmu.Game._i18n;
using WingsEmu.Game._ItemUsage;
using WingsEmu.Game._ItemUsage.Event;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;
using WingsEmu.Packets.Enums.Families;

namespace NosEmu.Plugins.BasicImplementations.ItemUsage.Main.Special;

public class FactionEggHandler : IItemHandler
{
    private const int IndividualAngelEgg = 1;
    private const int IndividualDemonEgg = 2;
    private const int FamilyAngelEgg = 3;
    private const int FamilyDemonEgg = 4;

    private readonly IGameLanguageService _languageService;

    public FactionEggHandler(IGameLanguageService languageService) => _languageService = languageService;

    public ItemType ItemType => ItemType.Special;
    public long[] Effects => new long[] { 570 };

    public async Task HandleAsync(IClientSession session, InventoryUseItemEvent e)
    {
        if (session.PlayerEntity.IsOnVehicle)
        {
            session.SendMsg(_languageService.GetLanguage(GameDialogKey.INFORMATION_SHOUTMESSAGE_REMOVE_VEHICLE, session.UserLanguage), MsgMessageType.Middle);
            return;
        }

        if (session.CantPerformActionOnAct4())
        {
            return;
        }

        int eggType = e.Item.ItemInstance.GameItem.EffectValue;
        FactionType targetFaction = eggType == IndividualAngelEgg || eggType == FamilyAngelEgg ? FactionType.Angel : FactionType.Demon;

        if (eggType == IndividualAngelEgg || eggType == IndividualDemonEgg)
        {
            if (session.PlayerEntity.Faction == targetFaction)
            {
                session.SendMsg(_languageService.GetLanguage(GameDialogKey.ITEM_SHOUTMESSAGE_SAME_FACTION, session.UserLanguage), MsgMessageType.Middle);
                return;
            }

            if (session.PlayerEntity.Family != null)
            {
                session.SendMsg(_languageService.GetLanguage(GameDialogKey.ITEM_SHOUTMESSAGE_FACTION_CANT_IN_FAMILY, session.UserLanguage), MsgMessageType.Middle);
                return;
            }

            session.SendQnaPacket($"guri 750 {eggType}", _languageService.GetLanguage(GameDialogKey.ITEM_DIALOG_ASK_CHANGE_FACTION, session.UserLanguage));
        }
        else if (eggType == FamilyAngelEgg || eggType == FamilyDemonEgg)
        {
            if (!session.PlayerEntity.IsInFamily())
            {
                session.SendMsg(_languageService.GetLanguage(GameDialogKey.FAMILY_INFO_NO_FAMILY, session.UserLanguage), MsgMessageType.Middle);
                return;
            }

            if (session.PlayerEntity.GetFamilyAuthority() != FamilyAuthority.Head)
            {
                session.SendMsg(_languageService.GetLanguage(GameDialogKey.FAMILY_INFO_NOT_FAMILY_HEAD, session.UserLanguage), MsgMessageType.Middle);
                return;
            }

            if ((session.PlayerEntity.Family.Faction / 2) == eggType)
            {
                session.SendMsg(_languageService.GetLanguage(GameDialogKey.ITEM_SHOUTMESSAGE_SAME_FACTION, session.UserLanguage), MsgMessageType.Middle);
                return;
            }

            session.SendQnaPacket($"guri 750 {eggType}", _languageService.GetLanguage(GameDialogKey.ITEM_DIALOG_ASK_CHANGE_FAMILY_FACTION, session.UserLanguage));
        }
    }
}