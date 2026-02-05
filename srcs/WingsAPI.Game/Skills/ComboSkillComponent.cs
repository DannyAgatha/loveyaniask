namespace WingsEmu.Game.Skills;

public class ComboSkillComponent : IComboSkillComponent
{
    private ComboSkillState _comboSkillState;

    public void SaveComboSkill(ComboSkillState comboSkillState) => _comboSkillState = comboSkillState;

    public ComboSkillState GetComboState() => _comboSkillState;

    public void IncreaseComboState(byte castId)
    {
        if (_comboSkillState == null)
        {
            return;
        }

        _comboSkillState.State++;
        _comboSkillState.LastSkillByCastId = castId;
    }

    public void CleanComboState() => _comboSkillState = null;
}

public class ComboSkillState
{
    public byte State { get; set; }

    public byte OriginalSkillCastId { get; set; }

    public byte LastSkillByCastId { get; set; }

    public short AngelSkillVnumId { get; set; }
}