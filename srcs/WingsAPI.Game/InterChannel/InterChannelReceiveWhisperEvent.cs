using WingsEmu.DTOs.Account;
using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.InterChannel;

public class InterChannelReceiveWhisperEvent : PlayerEvent
{
    public InterChannelReceiveWhisperEvent(long senderCharacterId, string senderNickname, int senderChannelId, AuthorityType authorityType, string message)
    {
        SenderNickname = senderNickname;
        SenderChannelId = senderChannelId;
        AuthorityType = authorityType;
        Message = message;
        SenderCharacterId = senderCharacterId;
    }

    public long SenderCharacterId { get; }

    public string SenderNickname { get; }

    public int SenderChannelId { get; }

    public AuthorityType AuthorityType { get; }

    public string Message { get; }
}