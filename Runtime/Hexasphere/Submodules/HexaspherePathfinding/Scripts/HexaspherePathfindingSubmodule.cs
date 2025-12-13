
using UnityEngine;

using GBB;

namespace HS.Hexasphere.Pathfinding
{
    public class HexaspherePathfindingSubmodule : GameSubmodule
    {
        [SerializeField]
        private PathfindingData pathfindingData;

        public override void AddSystems(GameStartup startup)
        {
            
        }

        public override void InjectData(GameStartup startup)
        {
            //¬водим данные
            startup.InjectData(pathfindingData);
        }
    }
}
