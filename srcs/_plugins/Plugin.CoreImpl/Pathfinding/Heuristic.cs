using System;
using WingsEmu.Game.Helpers.Damages;

namespace Plugin.CoreImpl.Pathfinding
{
    public class Heuristic
    {
        public static int DetermineH(HeuristicFormula heuristicFormula, Position end, int heuristicEstimate, int newLocationY, int newLocationX)
        {
            int h;

            switch (heuristicFormula)
            {
                case HeuristicFormula.Chebyshev:
                    h = heuristicEstimate * Math.Max(Math.Abs(newLocationX - end.X), Math.Abs(newLocationY - end.Y));
                    break;

                case HeuristicFormula.DiagonalShortCut:
                    int hDiagonal = Math.Min(Math.Abs(newLocationX - end.X), Math.Abs(newLocationY - end.Y));
                    int hStraight = Math.Abs(newLocationX - end.X) + Math.Abs(newLocationY - end.Y);
                    h = heuristicEstimate * 2 * hDiagonal + heuristicEstimate * (hStraight - 2 * hDiagonal);
                    break;

                case HeuristicFormula.Euclidean:
                    h = (int)(heuristicEstimate * Math.Sqrt(Math.Pow(newLocationX - end.X, 2) + Math.Pow(newLocationY - end.Y, 2)));
                    break;

                case HeuristicFormula.EuclideanNoSQR:
                    h = (int)(heuristicEstimate * (Math.Pow(newLocationX - end.X, 2) + Math.Pow(newLocationY - end.Y, 2)));
                    break;

                case HeuristicFormula.Custom1:
                    var dxy = new Position((short)Math.Abs(end.X - newLocationX), (short)Math.Abs(end.Y - newLocationY));
                    int orthogonal = Math.Abs(dxy.X - dxy.Y);
                    int diagonal = Math.Abs((dxy.X + dxy.Y - orthogonal) / 2);
                    h = heuristicEstimate * (diagonal + orthogonal + dxy.X + dxy.Y);
                    break;

                // ReSharper disable once RedundantCaseLabel
                case HeuristicFormula.Manhattan:
                default:
                    h = heuristicEstimate * (Math.Abs(newLocationX - end.X) + Math.Abs(newLocationY - end.Y));
                    break;
            }

            return h;
        }
    }
}