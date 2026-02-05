using System.Collections.Generic;
using WingsEmu.Game.Helpers.Damages;

namespace Plugin.CoreImpl.Pathfinding
{
    internal class ComparePfNodeMatrix : IComparer<Position>
    {
        private readonly PathFinderNodeFast[,] _matrix;

        public ComparePfNodeMatrix(PathFinderNodeFast[,] matrix) => _matrix = matrix;

        public int Compare(Position a, Position b)
        {
            if (_matrix[a.X, a.Y].F_Gone_Plus_Heuristic > _matrix[b.X, b.Y].F_Gone_Plus_Heuristic)
            {
                return 1;
            }

            if (_matrix[a.X, a.Y].F_Gone_Plus_Heuristic < _matrix[b.X, b.Y].F_Gone_Plus_Heuristic)
            {
                return -1;
            }

            return 0;
        }
    }
}