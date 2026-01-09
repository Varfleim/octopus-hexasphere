
using System.Collections.Generic;

namespace HS.Hexasphere.Pathfinding
{
    public class D_ModulePathfinding
    {
        public bool pathMatrixUpdated = true;
        public int pathfindingSearchLimit;
        public const int pathfindingSearchLimitBase = 100000;
        public D_PathfindingNodeFast[] pfCalc;
        public PathfindingQueueInt open;
        public List<D_PathfindingClosedNode> close = new();
        public byte openCellValue = 1;
        public byte closeCellValue = 2;
    }
}
