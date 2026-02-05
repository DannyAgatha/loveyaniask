using System.Collections.Generic;
using WingsEmu.Packets.Enums.Families;

namespace WingsEmu.Game.Families;

public interface IFamilyComponent
{
    public IFamily Family { get; }
    public FamilyMembership FamilyMembership { get; }
    public bool IsFamilyWarehouseOpen { get; set; }
    public bool IsFamilyWarehouseLogsOpen { get; set; }

    public void SetFamilyMembership(FamilyMembership membership);
    public bool IsHeadOfFamily();
    public bool IsInFamily();
    public List<FamilyMembership> GetFamilyMembers();
    public FamilyAuthority GetFamilyAuthority();
    public FamilyMembership GetMembershipByAuthority(FamilyAuthority familyAuthority);
    public FamilyMembership GetMembershipById(long id);
    public byte GetAmountOfMembersByType(FamilyAuthority type);
}