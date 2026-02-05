namespace WingsEmu.Game.Managers;

public class BubbleComponent : IBubbleComponent
{
    private string _bubbleMessage;

    public void SaveBubble(string message) => _bubbleMessage = message;

    public bool IsUsingBubble() => !string.IsNullOrEmpty(_bubbleMessage);

    public string GetMessage() => _bubbleMessage;

    public void RemoveBubble() => _bubbleMessage = null;
}