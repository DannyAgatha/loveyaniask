// NosEmu
// 


using System.Collections.Generic;
using WingsEmu.Game.Families;
using WingsEmu.Packets.Enums.Families;

namespace WingsEmu.Game.Characters;

public partial class PlayerEntity
{
    private readonly IFamilyComponent _familyComponent;

    public bool IsFamilyWarehouseOpen
    {
        get => _familyComponent.IsFamilyWarehouseOpen;
        set => _familyComponent.IsFamilyWarehouseOpen = value;
    }

    public bool IsFamilyWarehouseLogsOpen
    {
        get => _familyComponent.IsFamilyWarehouseLogsOpen;
        set => _familyComponent.IsFamilyWarehouseLogsOpen = value;
    }

    public IFamily Family => _familyComponent.Family;
    public FamilyMembership FamilyMembership => _familyComponent.FamilyMembership;

    public void SetFamilyMembership(FamilyMembership membership) => _familyComponent.SetFamilyMembership(membership);

    public bool IsHeadOfFamily() => _familyComponent.IsHeadOfFamily();
    public bool IsInFamily() => _familyComponent.IsInFamily();
    public List<FamilyMembership> GetFamilyMembers() => _familyComponent.GetFamilyMembers();
    public FamilyAuthority GetFamilyAuthority() => _familyComponent.GetFamilyAuthority();
    public FamilyMembership GetMembershipByAuthority(FamilyAuthority familyAuthority) => _familyComponent.GetMembershipByAuthority(familyAuthority);
    public FamilyMembership GetMembershipById(long id) => _familyComponent.GetMembershipById(id);
    public byte GetAmountOfMembersByType(FamilyAuthority type) => _familyComponent.GetAmountOfMembersByType(type);
}