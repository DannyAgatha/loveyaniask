using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Skills;
using WingsEmu.Packets.Enums;

namespace WingsEmu.Game.Characters;

public partial class PlayerEntity
{
    private readonly IAngelElementBuffComponent _angelElementBuffComponent;
    private readonly ICastingComponent _castingComponent;
    private readonly IComboSkillComponent _comboSkillComponent;
    private readonly IEndBuffDamageComponent _endBuffDamageComponent;
    private readonly IScoutComponent _scoutComponent;
    private readonly ISkillCooldownComponent _skillCooldownComponent;
    public IWildKeeperComponent WildKeeperComponent { get; }
    public ISkillComponent SkillComponent { get; }

    public void SaveComboSkill(ComboSkillState comboSkillState) => _comboSkillComponent.SaveComboSkill(comboSkillState);

    public ComboSkillState GetComboState() => _comboSkillComponent.GetComboState();

    public void IncreaseComboState(byte castId) => _comboSkillComponent.IncreaseComboState(castId);

    public void CleanComboState() => _comboSkillComponent.CleanComboState();

    public SkillCast SkillCast => _castingComponent.SkillCast;

    public bool IsCastingSkill => _castingComponent.IsCastingSkill;

    public void SetCastingSkill(SkillInfo skill, DateTime time)
    {
        _castingComponent.SetCastingSkill(skill, time);
    }

    public void RemoveCastingSkill()
    {
        _castingComponent.RemoveCastingSkill();
    }

    public ConcurrentQueue<(DateTime time, short castId)> SkillCooldowns => _skillCooldownComponent.SkillCooldowns;

    public ConcurrentQueue<(DateTime time, short castId, MateType mateType)> MatesSkillCooldowns => _skillCooldownComponent.MatesSkillCooldowns;

    public void AddSkillCooldown(DateTime time, short castId)
    {
        _skillCooldownComponent.AddSkillCooldown(time, castId);
    }

    public void ClearSkillCooldowns()
    {
        _skillCooldownComponent.ClearSkillCooldowns();
    }
    
    public void ClearSkillCooldownsById(short id)
    {
        _skillCooldownComponent.ClearSkillCooldownsById(id);
    }

    public void AddMateSkillCooldown(DateTime time, short castId, MateType mateType)
    {
        _skillCooldownComponent.AddMateSkillCooldown(time, castId, mateType);
    }

    public void ClearMateSkillCooldowns()
    {
        _skillCooldownComponent.ClearMateSkillCooldowns();
    }

    public ElementType? AngelElement => _angelElementBuffComponent.AngelElement;

    public void AddAngelElement(ElementType elementType)
    {
        _angelElementBuffComponent.AddAngelElement(elementType);
    }

    public void RemoveAngelElement()
    {
        _angelElementBuffComponent.RemoveAngelElement();
    }

    public IReadOnlyDictionary<short, int> EndBuffDamages => _endBuffDamageComponent.EndBuffDamages;

    public void AddEndBuff(short buffVnum, int damage)
    {
        _endBuffDamageComponent.AddEndBuff(buffVnum, damage);
    }

    public int DecreaseDamageEndBuff(short buffVnum, int damage) => _endBuffDamageComponent.DecreaseDamageEndBuff(buffVnum, damage);

    public void RemoveEndBuffDamage(short buffVnum)
    {
        _endBuffDamageComponent.RemoveEndBuffDamage(buffVnum);
    }

    public ScoutStateType ScoutStateType => _scoutComponent.ScoutStateType;

    public void ChangeScoutState(ScoutStateType stateType)
    {
        _scoutComponent.ChangeScoutState(stateType);
    }

    public void SetCharge(int chargeValue)
    {
        _chargeComponent.SetCharge(chargeValue);
    }

    public int GetCharge() => _chargeComponent.GetCharge();

    public void ResetCharge() => _chargeComponent.ResetCharge();
}