using System.Collections.Generic;
using WingsEmu.Game.Helpers.Damages;

namespace Plugin.CoreImpl.Pathfinding
{
    public interface IPathFinder
    {
        Position FindPath(Position start, Position end, float speedIndex, IReadOnlyList<byte> grid, int width, int height, bool useBresenhamFirst = false);
    }
}