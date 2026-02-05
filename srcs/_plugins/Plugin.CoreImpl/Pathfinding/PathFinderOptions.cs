namespace Plugin.CoreImpl.Pathfinding
{
    public class PathFinderOptions
    {
        public PathFinderOptions()
        {
            Formula = HeuristicFormula.Chebyshev;
            HeuristicEstimate = 2;
            SearchLimit = 100;
            Diagonals = false;
        }

        public HeuristicFormula Formula { get; set; }

        public bool Diagonals { get; set; }

        public bool HeavyDiagonals { get; set; }

        public int HeuristicEstimate { get; set; }

        public bool PunishChangeDirection { get; set; }

        public bool TieBreaker { get; set; }

        public int SearchLimit { get; set; }
    }
}