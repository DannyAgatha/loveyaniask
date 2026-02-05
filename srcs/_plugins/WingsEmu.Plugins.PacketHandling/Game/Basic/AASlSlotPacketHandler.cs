using System;
using System.Threading.Tasks;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.ClientPackets;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;

namespace WingsEmu.Plugins.PacketHandling.Game.Basic;

public class AASlSlotPaccketHandler : GenericGamePacketHandlerBase<AASlSlotPacket>
{
    private readonly IDelayManager _delayManager;

    public AASlSlotPaccketHandler(IDelayManager delayManager)
    {
        _delayManager = delayManager;
    }

    protected override async Task HandlePacketAsync(IClientSession session, AASlSlotPacket packet)
    {
        if (!session.PlayerEntity.UseSp) return;

        if (packet.Slot == 1 && !session.PlayerEntity.Specialist.IsSecondSpecialistSlotActivated) return; // this slot isn't avalaible

        if (packet.Slot == 2 && !session.PlayerEntity.Specialist.IsThirdSpecialistSlotActivated) return; // this slot isn't avalaible

        if (packet.CardId > 1073741823 || packet.Slot > 2) return;

        if (packet.CardId != session.PlayerEntity.Specialist.TransportId) return;

        if (session.PlayerEntity.LastSlotChange.AddMinutes(1) > DateTime.Now)
        {
            session.SendChatMessage(session.GetLanguageFormat(GameDialogKey.ON_COOLDOWN_SLOT), ChatMessageColorType.Yellow);
            return;
        }

        DateTime waitUntil = await _delayManager.RegisterAction(session.PlayerEntity, DelayedActionType.ChangeSpecialistSlot);
        session.SendDelay((int)(waitUntil - DateTime.UtcNow).TotalMilliseconds, GuriType.UsingItem, $"guri 99999 {packet.Slot}");
    }
}