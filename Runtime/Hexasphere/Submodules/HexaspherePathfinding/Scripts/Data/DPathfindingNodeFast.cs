
namespace HS.Pathfinding
{
    public struct DPathfindingNodeFast
    {
        public float priority;
        public float distance;

        public int prevIndex;
        public byte status;
        public int steps;
    }
}
