using System.Threading.Tasks;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsEmu.Game._i18n;
using WingsEmu.Game._ItemUsage;
using WingsEmu.Game._ItemUsage.Event;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Mates;
using WingsEmu.Game.Mates.Events;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;

namespace NosEmu.Plugins.BasicImplementations.ItemUsage.Etc.Teacher;

public class MateReleaseHandler : IItemHandler
{
    private readonly IGameLanguageService _gameLanguage;

    public MateReleaseHandler(IGameLanguageService gameLanguageService) => _gameLanguage = gameLanguageService;

    public ItemType ItemType => ItemType.PetPartnerItem;
    public long[] Effects => new long[] { 1000 };

    public async Task HandleAsync(IClientSession session, InventoryUseItemEvent e)
    {
        if (!int.TryParse(e.Packet[3], out int x1))
        {
            return;
        }

        IMateEntity mateEntity = session.PlayerEntity.MateComponent.GetMate(s => s.Id == x1 && s.MateType == MateType.Pet);
        if (mateEntity == null)
        {
            return;
        }

        if (mateEntity.IsTeamMember)
        {
            session.SendMsg(_gameLanguage.GetLanguage(GameDialogKey.INFORMATION_SHOUTMESSAGE_PET_IN_TEAM_UNRELEASABLE, e.Sender.UserLanguage), MsgMessageType.Middle);
            return;
        }

        await session.EmitEventAsync(new MateRemoveEvent
        {
            MateEntity = mateEntity
        });

        session.SendInfo(_gameLanguage.GetLanguage(GameDialogKey.PET_INFO_RELEASED, e.Sender.UserLanguage));
        await session.RemoveItemFromInventory(item: e.Item);
    }
}