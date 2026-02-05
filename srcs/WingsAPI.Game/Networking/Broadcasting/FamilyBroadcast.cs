namespace WingsEmu.Game.Networking.Broadcasting;

public class FamilyBroadcast : IBroadcastRule
{
    private readonly long _familyId;

    public FamilyBroadcast(long familyId) => _familyId = familyId;

    public bool Match(IClientSession session) => session.PlayerEntity.Family != null && session.PlayerEntity.Family.Id == _familyId;
}