using System;
using System.Collections.Generic;
using System.Linq;
using WingsEmu.Game.Helpers;
using WingsEmu.Game.Helpers.Damages;

namespace Plugin.CoreImpl.Pathfinding
{
    public class PathFinder : IPathFinder
    {
        private readonly List<PathFinderNode> _closed = new();
        private readonly sbyte[,] _direction;
        private readonly IReadOnlyList<byte> _grid;
        private readonly PathFinderOptions _options;
        private byte _closeNodeValue = 2;
        private int _horiz;
        private PathFinderNodeFast[,] _mCalcGrid;
        private IPriorityQueue<Position> _open;
        private byte _openNodeValue = 1;

        public PathFinder(IReadOnlyList<byte> grid, int width, int height, PathFinderOptions pathFinderOptions = null)
        {
            _grid = grid ?? throw new Exception("Grid cannot be null");
            Width = (ushort)width;
            Height = (ushort)height;


            _options = pathFinderOptions ?? new PathFinderOptions();

            _direction = _options.Diagonals
                ? new sbyte[,] { { 0, -1 }, { 1, 0 }, { 0, 1 }, { -1, 0 }, { 1, -1 }, { 1, 1 }, { -1, 1 }, { -1, -1 } }
                : new sbyte[,] { { 0, -1 }, { 1, 0 }, { 0, 1 }, { -1, 0 } };
        }

        private ushort Width { get; }
        private ushort Height { get; }

        public Position FindPath(Position start, Position end, float speedIndex, IReadOnlyList<byte> grid, int width, int height, bool useBresenhamFirst)
        {
            try
            {
                Position position;
                if (useBresenhamFirst)
                {
                    position = PathfindingAlgorithm.Bresenham(start, end, 10, grid, width, height, true);
                    position = AStar(start, position, (byte)speedIndex);
                    if (position == start)
                    {
                        position = PathfindingAlgorithm.Bresenham(start, end, speedIndex, grid, width, height, false);
                    }
                }
                else
                {
                    position = AStar(start, end, (byte)speedIndex);
                }

                return position;
            }
            catch
            {
                return start;
            }
        }

        private List<PathFinderNode> FindPath(Position start, Position end)
        {
            if (_mCalcGrid == null || _mCalcGrid.GetLength(0) != Width || _mCalcGrid.GetLength(1) != Height)
            {
                _mCalcGrid = new PathFinderNodeFast[Width, Height];
                _open = new PriorityQueueB<Position>(new ComparePfNodeMatrix(_mCalcGrid));
            }

            bool found = false;
            int closedNodeCounter = 0;
            _openNodeValue += 2; //increment for subsequent runs
            _closeNodeValue += 2;
            _open.Clear();
            _closed.Clear();

            _mCalcGrid[start.X, start.Y].Gone = 0;
            _mCalcGrid[start.X, start.Y].F_Gone_Plus_Heuristic = _options.HeuristicEstimate;
            _mCalcGrid[start.X, start.Y].ParentX = start.X;
            _mCalcGrid[start.X, start.Y].ParentY = start.Y;
            _mCalcGrid[start.X, start.Y].Status = _openNodeValue;

            _open.Enqueue(start);

            while (_open.Count > 0)
            {
                Position location = _open.Dequeue();

                //Is it in closed list? means this node was already processed
                if (_mCalcGrid[location.X, location.Y].Status == _closeNodeValue)
                {
                    continue;
                }

                short locationX = location.X;
                short locationY = location.Y;

                if (location == end)
                {
                    _mCalcGrid[location.X, location.Y].Status = _closeNodeValue;
                    found = true;
                    break;
                }

                if (closedNodeCounter > _options.SearchLimit)
                {
                    return null;
                }

                if (_options.PunishChangeDirection)
                {
                    _horiz = locationX - _mCalcGrid[location.X, location.Y].ParentX;
                }

                //Lets calculate each successors
                for (int i = 0; i < _direction.GetLength(0); i++)
                {
                    //unsign incase we went out of bounds
                    short newLocationX = (short)(locationX + _direction[i, 0]);
                    short newLocationY = (short)(locationY + _direction[i, 1]);

                    if (newLocationX >= Width || newLocationY >= Height)
                    {
                        continue;
                    }

                    // Unbreakeable?
                    if ((_grid[newLocationX + newLocationY * Width] & (int)PathfindingAlgorithm.MapCellFlags.IsWalkingDisabled) != 0)
                    {
                        continue;
                    }

                    int newG;
                    if (_options.HeavyDiagonals && i > 3)
                    {
                        newG = _mCalcGrid[location.X, location.Y].Gone + (int)(_grid[newLocationX + newLocationY * Width] * 2.41);
                    }
                    else
                    {
                        newG = _mCalcGrid[location.X, location.Y].Gone + _grid[newLocationX + newLocationY * Width];
                    }

                    if (_options.PunishChangeDirection)
                    {
                        if ((newLocationX - locationX) != 0)
                        {
                            if (_horiz == 0)
                            {
                                newG += Math.Abs(newLocationX - end.X) + Math.Abs(newLocationY - end.Y);
                            }
                        }

                        if ((newLocationY - locationY) != 0)
                        {
                            if (_horiz != 0)
                            {
                                newG += Math.Abs(newLocationX - end.X) + Math.Abs(newLocationY - end.Y);
                            }
                        }
                    }

                    //Is it open or closed?
                    if (_mCalcGrid[newLocationX, newLocationY].Status == _openNodeValue || _mCalcGrid[newLocationX, newLocationY].Status == _closeNodeValue)
                    {
                        // The current node has less code than the previous? then skip this node
                        if (_mCalcGrid[newLocationX, newLocationY].Gone <= newG)
                        {
                            continue;
                        }
                    }

                    _mCalcGrid[newLocationX, newLocationY].ParentX = locationX;
                    _mCalcGrid[newLocationX, newLocationY].ParentY = locationY;
                    _mCalcGrid[newLocationX, newLocationY].Gone = newG;

                    int h = Heuristic.DetermineH(HeuristicFormula.Chebyshev, end, _options.HeuristicEstimate, newLocationY, newLocationX);

                    if (_options.TieBreaker)
                    {
                        int dx1 = locationX - end.X;
                        int dy1 = locationY - end.Y;
                        int dx2 = start.X - end.X;
                        int dy2 = start.Y - end.Y;
                        int cross = Math.Abs(dx1 * dy2 - dx2 * dy1);
                        h = (int)(h + cross * 0.001);
                    }

                    _mCalcGrid[newLocationX, newLocationY].F_Gone_Plus_Heuristic = newG + h;

                    _open.Enqueue(new Position(newLocationX, newLocationY));

                    _mCalcGrid[newLocationX, newLocationY].Status = _openNodeValue;
                }

                closedNodeCounter++;
                _mCalcGrid[location.X, location.Y].Status = _closeNodeValue;
            }

            return !found ? null : OrderClosedListAsPath(end);
        }

        private Position AStar(Position start, Position end, byte speedIndex)
        {
            IReadOnlyList<PathFinderNode> positions = FindPath(start, end);
            if (positions == null || positions.Count < 1)
            {
                return start;
            }

            PathFinderNode[] array = positions.Reverse().ToArray();
            int max = array.Length > speedIndex + 1 ? speedIndex : array.Length - 1;

            PathFinderNode pos = array[max];
            return new Position(pos.X, pos.Y);
        }

        private List<PathFinderNode> OrderClosedListAsPath(Position end)
        {
            _closed.Clear();

            PathFinderNodeFast fNodeTmp = _mCalcGrid[end.X, end.Y];

            var fNode = new PathFinderNode
            {
                ParentX = fNodeTmp.ParentX,
                ParentY = fNodeTmp.ParentY,
                X = end.X,
                Y = end.Y
            };

            while (fNode.X != fNode.ParentX || fNode.Y != fNode.ParentY)
            {
                _closed.Add(fNode);

                short posX = fNode.ParentX;
                short posY = fNode.ParentY;

                fNodeTmp = _mCalcGrid[posX, posY];
                fNode.ParentX = fNodeTmp.ParentX;
                fNode.ParentY = fNodeTmp.ParentY;
                fNode.X = posX;
                fNode.Y = posY;
            }

            _closed.Add(fNode);

            return _closed;
        }
    }
}