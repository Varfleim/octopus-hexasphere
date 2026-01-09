
using System.Collections.Generic;

namespace HS.Hexasphere.Pathfinding
{
    public class PathfindingNodesComparer : IComparer<int>
    {
        public PathfindingNodesComparer(
            D_PathfindingNodeFast[] cells)
        {
            this.cells = cells;
        }

        D_PathfindingNodeFast[] cells;

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
            D_PathfindingNodeFast[] cells)
        {
            this.cells = cells;
        }
    }
}
