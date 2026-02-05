namespace WingsEmu.Game.Networking.Broadcasting
{
    public class InMapVnumBroadcast : IBroadcastRule
    {
        private readonly int _mapVnum;
        public InMapVnumBroadcast(int mapVnum)
        {
            _mapVnum = mapVnum;
        }

        public bool Match(IClientSession session)
        {
            return session.CurrentMapInstance != null && session.CurrentMapInstance.MapVnum == _mapVnum;
        }
    }
}