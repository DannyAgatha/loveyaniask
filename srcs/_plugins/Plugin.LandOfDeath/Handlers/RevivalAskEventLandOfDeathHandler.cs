using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Revival;
using WingsEmu.Packets.Enums.Chat;

namespace Plugin.LandOfDeath.Handlers;

public class RevivalAskEventLandOfDeathHandler : IAsyncEventProcessor<RevivalAskEvent>
{
    public async Task HandleAsync(RevivalAskEvent e, CancellationToken cancellation)
    {
        if (e.Sender.PlayerEntity.IsAlive() || e.AskRevivalType != AskRevivalType.LandOfDeathRevival)
        {
            return;
        }

        string message = e.Sender.GetLanguage(GameDialogKey.LAND_OF_DEATH_ASK_RESPAWN);
        e.Sender.SendDialog(CharacterPacketExtension.GenerateRevivalPacket(RevivalType.TryPayRevival), CharacterPacketExtension.GenerateRevivalPacket(RevivalType.DontPayRevival), message);
    }
}