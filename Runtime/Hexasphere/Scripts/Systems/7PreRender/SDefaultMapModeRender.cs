
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

using GBB.Map;

namespace HS
{
    public class SDefaultMapModeRender : IEcsRunSystem
    {
        readonly EcsWorldInject world = default;

        readonly EcsFilterInject<Inc<CMap, CActiveMap>> activeMapFilter = default;
        readonly EcsPoolInject<CMap> mapPool = default;

        public void Run(IEcsSystems systems)
        {
            //��� ������ �������� �����
            foreach (int activeMapEntity in activeMapFilter.Value)
            {
                //���� �����
                ref CMap activeMap = ref mapPool.Value.Get(activeMapEntity);

                //������ ������� ��������� ������������ ��� ������ ���������
                SetMapRenderValuesRequests(ref activeMap);
            }
        }

        readonly EcsPoolInject<SRUpdateProvinceRender> setMapRenderValuesSRPool = default;
        void SetMapRenderValuesRequests(
            ref CMap map)
        {
            //��� ������ ��������� �����
            for(int a = 0; a < map.provincePEs.Length; a++)
            {
                //���� �������� ���������
                map.provincePEs[a].Unpack(world.Value, out int provinceEntity);

                //������ ������ ���������� ������������ ��� ��
                GBB.Map.MapModeData.UpdateProvinceRenderRequestCreation(
                    setMapRenderValuesSRPool.Value,
                    provinceEntity);
            }
        }
    }
}
