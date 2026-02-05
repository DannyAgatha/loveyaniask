using System.Runtime.InteropServices;

namespace Plugin.CoreImpl.Pathfinding
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct PathFinderNodeFast
    {
        public int F_Gone_Plus_Heuristic; // f = gone + heuristic
        public int Gone;
        public short ParentX; // Parent
        public short ParentY;
        public byte Status;
    }
}