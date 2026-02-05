using WingsEmu.Game.Battle.Managers;

namespace WingsEmu.Game.Battle;

public class ChargeComponent : IChargeComponent
{
    private const int MAX_CHARGE = 7000;

    private int _charge;

    public void SetCharge(int chargeValue)
    {
        if (chargeValue > MAX_CHARGE)
        {
            chargeValue = MAX_CHARGE;
        }

        _charge = chargeValue;
    }

    public int GetCharge() => _charge;

    public void ResetCharge()
    {
        _charge = 0;
    }
}