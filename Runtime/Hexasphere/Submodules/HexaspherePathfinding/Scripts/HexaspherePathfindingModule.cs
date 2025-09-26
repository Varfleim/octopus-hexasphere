
using UnityEngine;

using GBB;

namespace HS.Pathfinding
{
    [CreateAssetMenu]
    public class HexaspherePathfindingModule : GameModule
    {
        public override void AddSystems(GameStartup startup)
        {
            
        }

        public override void InjectData(GameStartup startup)
        {
            //—оздаЄм компонент данных поиска пути
            PathfindingData pathfindingData = startup.AddDataObject().AddComponent<PathfindingData>();

            //¬водим данные
            startup.InjectData(pathfindingData);
        }
    }
}
