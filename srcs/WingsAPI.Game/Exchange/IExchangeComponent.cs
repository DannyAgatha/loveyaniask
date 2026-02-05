namespace WingsEmu.Game.Exchange;

public interface IExchangeComponent
{
    public void SetExchange(PlayerExchange exchange);
    public void RemoveExchange();
    public PlayerExchange GetExchange();
    public bool IsInExchange();
    public long GetTargetId();
}