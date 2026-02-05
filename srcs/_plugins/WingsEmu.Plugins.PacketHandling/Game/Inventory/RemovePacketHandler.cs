using PhoenixLib.Scheduler;
using System;
using System.Threading.Tasks;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Inventory.Event;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.ClientPackets;
using WingsEmu.Packets.Enums.Chat;

namespace WingsEmu.Plugins.PacketHandling.Game.Inventory;

public class RemovePacketHandler : GenericGamePacketHandlerBase<RemovePacket>
{
    private readonly IScheduler _scheduler;
    private readonly IGameLanguageService _languageService;

    public RemovePacketHandler(IScheduler scheduler, IGameLanguageService languageService)
    {
        _scheduler = scheduler;
        _languageService = languageService;
    }

    protected override async Task HandlePacketAsync(IClientSession session, RemovePacket removePacket)
    {

        if (session.PlayerEntity.IsSeal)
        {
            return;
        }

        if (removePacket.InventorySlot == 12 && removePacket.PartnerSlot == 0)
        {
            session.PlayerEntity.UntransformPressingKeyCount++;
            if (session.PlayerEntity.UntransformPressingKeyCount == 1)
            {
                session.SendChatMessage(_languageService.GetLanguageFormat(GameDialogKey.REMOVE_SP_CONFIRMATION, session.UserLanguage), ChatMessageColorType.Red);
                _scheduler.Schedule(TimeSpan.FromSeconds(1.5), () =>
                {
                    session.PlayerEntity.UntransformPressingKeyCount = 0;
                });
            }

            if (session.PlayerEntity.UntransformPressingKeyCount < 2)
            {
                return;
            }

            await session.EmitEventAsync(new InventoryTakeOffItemEvent(removePacket.InventorySlot));
            return;
        }

        if (removePacket.PartnerSlot == 0)
        {
            await session.EmitEventAsync(new InventoryTakeOffItemEvent(removePacket.InventorySlot));
            return;
        }

        await session.EmitEventAsync(new PartnerInventoryTakeOffItemEvent(removePacket.PartnerSlot, removePacket.InventorySlot));
    }
}