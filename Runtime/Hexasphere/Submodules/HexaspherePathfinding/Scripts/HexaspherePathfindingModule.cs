
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
            //������ ��������� ������ ������ ����
            PathfindingData pathfindingData = startup.AddDataObject().AddComponent<PathfindingData>();

            //������ ������
            startup.InjectData(pathfindingData);
        }
    }
}
