using System.Linq;
using System.Threading.Tasks;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsEmu.Game._i18n;
using WingsEmu.Game._ItemUsage;
using WingsEmu.Game._ItemUsage.Event;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Relations;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Relations;

namespace NosEmu.Plugins.BasicImplementations.ItemUsage.Main.Special;

public class CupidArrowHandler : IItemHandler
{
    private readonly IInvitationManager _invitation;
    private readonly IGameLanguageService _languageService;

    private readonly ISessionManager _sessionManager;

    public CupidArrowHandler(ISessionManager sessionManager, IGameLanguageService languageService, IInvitationManager invitation)
    {
        _sessionManager = sessionManager;
        _languageService = languageService;
        _invitation = invitation;
    }

    public ItemType ItemType => ItemType.Special;
    public long[] Effects => new long[] { 34 };

    public async Task HandleAsync(IClientSession session, InventoryUseItemEvent e)
    {
        if (e.Packet.Length < 4)
        {
            return;
        }

        if (!long.TryParse(e.Packet[3], out long characterId))
        {
            return;
        }

        IClientSession otherSession = _sessionManager.GetSessionByCharacterId(characterId);
        if (otherSession == null || otherSession.PlayerEntity.Id == session.PlayerEntity.Id)
        {
            return;
        }

        if (session.PlayerEntity.GetRelations().Any(x => x.RelationType == CharacterRelationType.Spouse))
        {
            return;
        }

        if (otherSession.PlayerEntity.GetRelations().Any(x => x.RelationType == CharacterRelationType.Spouse))
        {
            return;
        }

        if (!session.PlayerEntity.IsFriend(characterId))
        {
            session.SendInfo(_languageService.GetLanguage(GameDialogKey.FRIEND_MESSAGE_NOT_FRIEND, session.UserLanguage));
            return;
        }

        if (e.Option == 0)
        {
            session.SendQnaPacket($"u_i 1 {otherSession.PlayerEntity.Id} {(byte)e.Item.ItemInstance.GameItem.Type} {e.Item.Slot} 2",
                _languageService.GetLanguageFormat(GameDialogKey.WEDDING_DIALOG_REQUEST_VERIFICATION, session.UserLanguage, otherSession.PlayerEntity.Name));
            return;
        }

        await session.EmitEventAsync(new InvitationEvent(characterId, InvitationType.Spouse));
        await session.RemoveItemFromInventory(item: e.Item);
    }
}