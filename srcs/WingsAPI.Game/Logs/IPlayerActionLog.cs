using System;

namespace WingsEmu.Game.Logs;

public interface IPlayerActionLog
{
    DateTime CreatedAt { get; init; }
    int ChannelId { get; init; }
    long CharacterId { get; init; }
    string CharacterName { get; init; }
    string IpAddress { get; init; }
}