
using System.Collections.Generic;

namespace HS.Hexasphere.Pathfinding
{
    public class DModulePathfinding
    {
        public bool pathMatrixUpdated = true;
        public int pathfindingSearchLimit;
        public const int pathfindingSearchLimitBase = 100000;
        public DPathfindingNodeFast[] pfCalc;
        public PathfindingQueueInt open;
        public List<DPathfindingClosedNode> close = new();
        public byte openCellValue = 1;
        public byte closeCellValue = 2;
    }
}
