using WingsEmu.Game._i18n;
using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.InterChannel;

public class InterChannelSendInfoByCharIdEvent : PlayerEvent
{
    public InterChannelSendInfoByCharIdEvent(long characterId, GameDialogKey dialogKey)
    {
        CharacterId = characterId;
        DialogKey = dialogKey;
    }

    public long CharacterId { get; }

    public GameDialogKey DialogKey { get; }
}