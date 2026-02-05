using WingsEmu.Game._packetHandling;

namespace WingsEmu.Game.Characters.Events;

public class GenerateGoldEvent : PlayerEvent
{
    public GenerateGoldEvent(long amount, bool applyGoldMultiplier = true, bool isQuest = false, bool sendMessage = true, bool fallBackToBank = false)
    {
        Amount = amount;
        ApplyGoldMultiplier = applyGoldMultiplier;
        IsQuest = isQuest;
        SendMessage = sendMessage;
        FallBackToBank = fallBackToBank;
    }

    public long Amount { get; }
    public bool ApplyGoldMultiplier { get; }
    public bool IsQuest { get; }
    public bool SendMessage { get; }
    public bool FallBackToBank { get; }
}