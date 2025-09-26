
using UnityEngine;

using GBB;

namespace HS.Input
{
    [CreateAssetMenu]
    public class HexasphereInputModule : GameModule
    {
        public override void AddSystems(GameStartup startup)
        {
            //��������� ������� �������������

            //��������� ���������� �������
            #region PreFrame
            //���� � ����
            startup.AddPreFrameSystem(new SMouseInput());
            #endregion

            //��������� ������� ����������

            //��������� ��������� �������

        }

        public override void InjectData(GameStartup startup)
        {
            //������ ����� ������ ��� ������ ����� ���������� � ��������� ��� �� ���������
            HexasphereInputData hexasphereInputData = startup.AddDataObject().AddComponent<HexasphereInputData>();

            //������ ������
            startup.InjectData(hexasphereInputData);
        }
    }
}
