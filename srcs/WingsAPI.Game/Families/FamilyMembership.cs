// NosEmu
// 


using WingsAPI.Communication.Player;
using WingsAPI.Data.Families;
using WingsEmu.Game.Managers;

namespace WingsEmu.Game.Families;

public class FamilyMembership : FamilyMembershipDto
{
    private readonly ISessionManager _sessionManager;

    public FamilyMembership()
    {
    }

    public FamilyMembership(FamilyMembershipDto input, ISessionManager sessionManager)
    {
        _sessionManager = sessionManager;
        FamilyId = input.FamilyId;
        CharacterId = input.CharacterId;
        Authority = input.Authority;
        DailyMessage = input.DailyMessage;
        Experience = input.Experience;
        Title = input.Title;
        JoinDate = input.JoinDate;
        LastOnlineDate = input.LastOnlineDate;
    }

    public ClusterCharacterInfo Character => _sessionManager.GetOnlineCharacterById(CharacterId).ConfigureAwait(false).GetAwaiter().GetResult();
}