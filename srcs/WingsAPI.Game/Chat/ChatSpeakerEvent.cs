using WingsEmu.Game._packetHandling;
using WingsEmu.Game.Items;
using WingsEmu.Packets.Enums.Chat;

namespace WingsEmu.Game.Chat;

public class ChatSpeakerEvent : PlayerEvent
{
    public ChatSpeakerEvent(SpeakerType chatSpeakerType, string message, GameItemInstance item = null)
    {
        ChatSpeakerType = chatSpeakerType;
        Message = message;
        Item = item;
    }

    public SpeakerType ChatSpeakerType { get; set; }
    public string Message { get; set; }
    public GameItemInstance Item { get; set; }
}