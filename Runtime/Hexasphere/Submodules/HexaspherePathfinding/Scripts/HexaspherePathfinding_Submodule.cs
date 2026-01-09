
using UnityEngine;

using GBB;

namespace HS.Hexasphere.Pathfinding
{
    public class HexaspherePathfinding_Submodule : GameSubmodule
    {
        [SerializeField]
        private Pathfinding_Data pathfinding_Data;

        public override void Systems_Add(GameStartup startup)
        {
            
        }

        public override void Data_Inject(GameStartup startup)
        {
            //¬водим данные
            startup.Data_Inject(pathfinding_Data);
        }
    }
}
