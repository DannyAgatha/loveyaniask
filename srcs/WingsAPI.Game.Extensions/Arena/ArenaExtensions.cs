namespace WingsAPI.Game.Extensions.Arena
{
    public static class ArenaExtensions
    {
        public static long GetArenaEntryPrice(bool familyArena) => familyArena ? 1000 : 500;
    }
}