
using System.Collections.Generic;

namespace HS.Hexasphere.Pathfinding
{
    public class PathfindingNodesComparer : IComparer<int>
    {
        public PathfindingNodesComparer(
            DPathfindingNodeFast[] cells)
        {
            this.cells = cells;
        }

        DPathfindingNodeFast[] cells;

        public int Compare(
            int a, int b)
        {
            if (cells[a].priority > cells[b].priority)
            {
                return 1;
            }
            else if (cells[a].priority < cells[b].priority)
            {
                return -1;
            }
            return 0;
        }

        public void SetMatrix(
            DPathfindingNodeFast[] cells)
        {
            this.cells = cells;
        }
    }
}
