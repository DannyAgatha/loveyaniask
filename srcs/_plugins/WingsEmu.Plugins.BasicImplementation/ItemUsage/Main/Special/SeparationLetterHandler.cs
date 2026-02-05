using System.Linq;
using System.Threading.Tasks;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsAPI.Game.Extensions.RelationsExtensions;
using WingsEmu.DTOs.Relations;
using WingsEmu.Game._i18n;
using WingsEmu.Game._ItemUsage;
using WingsEmu.Game._ItemUsage.Event;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Groups.Events;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Relations;

namespace NosEmu.Plugins.BasicImplementations.ItemUsage.Main.Special;

public class SeparationLetterHandler : IItemHandler
{
    private readonly IGameLanguageService _languageService;
    private readonly ISessionManager _sessionManager;

    public SeparationLetterHandler(ISessionManager sessionManager, IGameLanguageService languageService)
    {
        _sessionManager = sessionManager;
        _languageService = languageService;
    }

    public ItemType ItemType => ItemType.Special;
    public long[] Effects => new long[] { 6969 };

    public async Task HandleAsync(IClientSession session, InventoryUseItemEvent e)
    {
        CharacterRelationDTO rel = session.PlayerEntity.GetRelations().FirstOrDefault(x => x.RelationType == CharacterRelationType.Spouse);
        if (rel == null)
        {
            return;
        }

        if (e.Option == 0)
        {
            session.SendQnaPacket($"u_i 1 {session.PlayerEntity.Id} {(int)e.Item.InventoryType} {e.Item.Slot} 1", session.GetLanguage(GameDialogKey.WEDDING_DIALOG_ASK_DIVORCE_CONFIRM));
            return;
        }

        await session.RemoveRelationAsync(rel.RelatedCharacterId, CharacterRelationType.Spouse);

        session.SendInfo(_languageService.GetLanguage(GameDialogKey.WEDDING_INFO_DIVORCED, session.UserLanguage));
        await session.RemoveItemFromInventory(item: e.Item);

        await session.EmitEventAsync(new GroupWeedingEvent
        {
            RemoveBuff = true,
            RelatedId = rel.RelatedCharacterId
        });
    }
}