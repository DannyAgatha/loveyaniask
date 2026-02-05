namespace WingsAPI.Communication.Sessions.Model
{
    public enum SessionState
    {
        Disconnected,
        ServerSelection,
        CrossChannelAuthentication,
        CharacterSelection,
        InGame
    }
}