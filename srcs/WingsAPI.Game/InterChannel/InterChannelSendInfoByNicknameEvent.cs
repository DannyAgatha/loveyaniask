using WingsEmu.Game._i18n;
using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.InterChannel;

public class InterChannelSendInfoByNicknameEvent : PlayerEvent
{
    public InterChannelSendInfoByNicknameEvent(string nickname, GameDialogKey dialogKey)
    {
        Nickname = nickname;
        DialogKey = dialogKey;
    }

    public string Nickname { get; }

    public GameDialogKey DialogKey { get; }
}