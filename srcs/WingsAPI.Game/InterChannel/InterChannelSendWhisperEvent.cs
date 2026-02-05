using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.InterChannel;

public class InterChannelSendWhisperEvent : PlayerEvent
{
    public InterChannelSendWhisperEvent(string nickname, string message)
    {
        Nickname = nickname;
        Message = message;
    }

    public string Nickname { get; }

    public string Message { get; }
}