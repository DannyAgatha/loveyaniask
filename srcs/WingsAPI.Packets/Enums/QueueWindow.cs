namespace WingsEmu.Packets.Enums
{
    public enum QueueWindow : byte
    {
        WaitForEntry = 0,
        SearchOpposingTeam = 1,
        OpposingTeamFound = 2,
        NoOpposingsTeamFound = 3,
        RejectRegistrationRequest = 4,
        SearchRegisteredTeam = 5,
        FoundRegisteredTeam = 6,
        NoRegisteredTeamFound = 7,
        SearchRegisteredTeamImmediately = 8
    }
}