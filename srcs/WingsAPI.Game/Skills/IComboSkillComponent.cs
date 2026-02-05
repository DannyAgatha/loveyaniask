namespace WingsEmu.Game.Skills;

public interface IComboSkillComponent
{
    public void SaveComboSkill(ComboSkillState comboSkillState);
    public ComboSkillState GetComboState();
    public void IncreaseComboState(byte castId);
    public void CleanComboState();
}