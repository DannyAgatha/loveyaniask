using System.Collections.Generic;
using System.Linq;
using WingsEmu.Packets.Enums.Families;

namespace WingsEmu.Game.Families;

public class FamilyComponent : IFamilyComponent
{
    private readonly IFamilyManager _familyManager;

    public FamilyComponent(IFamilyManager familyManager)
    {
        _familyManager = familyManager;
        FamilyMembership = null;
    }

    public IFamily Family => FamilyMembership == null ? null : _familyManager.GetFamilyByFamilyId(FamilyMembership.FamilyId);
    public FamilyMembership FamilyMembership { get; private set; }
    public bool IsFamilyWarehouseOpen { get; set; }
    public bool IsFamilyWarehouseLogsOpen { get; set; }

    public void SetFamilyMembership(FamilyMembership membership) => FamilyMembership = membership;

    public bool IsHeadOfFamily() => FamilyMembership != null && GetFamilyAuthority() == FamilyAuthority.Head;
    public bool IsInFamily() => FamilyMembership != null && Family != null;

    public List<FamilyMembership> GetFamilyMembers()
    {
        IFamily family = Family;
        return family == null ? new List<FamilyMembership>() : family.Members;
    }

    public FamilyAuthority GetFamilyAuthority() => FamilyMembership.Authority;
    public FamilyMembership GetMembershipByAuthority(FamilyAuthority familyAuthority) => Family?.Members.FirstOrDefault(x => x.Authority == familyAuthority);
    public FamilyMembership GetMembershipById(long id) => Family?.Members.FirstOrDefault(x => x.CharacterId == id);

    public byte GetAmountOfMembersByType(FamilyAuthority type)
    {
        byte amount = 0;
        foreach (FamilyMembership member in Family.Members.Where(member => member.Authority == type))
        {
            amount++;
        }

        return amount;
    }
}