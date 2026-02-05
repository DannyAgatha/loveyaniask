namespace WingsEmu.Game.Managers;

public interface IBubbleComponent
{
    public void SaveBubble(string message);
    public bool IsUsingBubble();
    public string GetMessage();
    public void RemoveBubble();
}