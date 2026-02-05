// NosEmu
// 


namespace WingsEmu.Game.Battle.Managers;

public interface IChargeComponent
{
    public void SetCharge(int chargeValue);
    public int GetCharge();
    public void ResetCharge();
}