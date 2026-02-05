using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Revival;

namespace Plugin.TimeSpaces.Handlers;

public class RevivalAskEventTimeSpaceHandler : IAsyncEventProcessor<RevivalAskEvent>
{
    public async Task HandleAsync(RevivalAskEvent e, CancellationToken cancellation)
    {
        if (e.Sender.PlayerEntity.IsAlive())
        {
            return;
        }

        if (e.AskRevivalType != AskRevivalType.TimeSpaceRevival)
        {
            return;
        }

        if (!e.Sender.PlayerEntity.TimeSpaceComponent.IsInTimeSpaceParty)
        {
            return;
        }

        if (e.Sender.PlayerEntity.TimeSpaceComponent.TimeSpace.Finished)
        {
            return;
        }

        e.Sender.SendDialog(CharacterPacketExtension.GenerateRevivalPacket(RevivalType.DontPayRevival), CharacterPacketExtension.GenerateRevivalPacket(RevivalType.DontPayRevival),
            e.Sender.GetLanguageFormat(GameDialogKey.TIMESPACE_DIALOG_ASK_REVIVAL));
    }
}