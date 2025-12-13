
using UnityEngine;

using Leopotam.EcsLite;

namespace HS.Hexasphere.Pathfinding
{
    public readonly struct C_CellPathfinding
    {
        public C_CellPathfinding(
            int index, 
            Vector3 center,
            EcsPackedEntity[] neighbourCellPEs)
        {
            this.index = index;
            
            this.center = center;

            this.crossCost = 1;

            this.neighbourCellPEs = neighbourCellPEs;
        }

        public readonly int index;

        public readonly Vector3 center;

        public readonly int crossCost;

        public readonly EcsPackedEntity[] neighbourCellPEs;
    }
}
