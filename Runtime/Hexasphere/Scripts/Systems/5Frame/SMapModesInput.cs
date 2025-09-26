
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

using GBB.Input;
using GBB.Map;

namespace HS
{
    public class SMapModesInput : IEcsRunSystem
    {
        readonly EcsWorldInject world = default;


        readonly EcsFilterInject<Inc<CMap, CActiveMap>> activeMapFilter = default;
        readonly EcsPoolInject<CMap> mapPool = default;

        readonly EcsPoolInject<CMapModeCore> mapModeCorePool = default;

        readonly EcsPoolInject<SRShowMapHoverHighlight> showMapHoverHighlightSRPool = default;

        public void Run(IEcsSystems systems)
        {
            //��� ������ �������� �����
            foreach(int activeMapEntity in activeMapFilter.Value)
            {
                //���� �����
                ref CMap activeMap = ref mapPool.Value.Get(activeMapEntity);

                //������������ ���� � ������� �����
                MapModesInput();
            }
        }

        readonly EcsFilterInject<Inc<RMouseMapPositionCheck>> mouseMapPositionCheckFilter = default;
        readonly EcsPoolInject<RMouseMapPositionCheck> mouseMapPositionCheckPool = default;
        void MapModesInput()
        {
            //������������ ���� � ����������� ������ �����
            DefaultMapModeInput();
        }

        readonly EcsFilterInject<Inc<CMapModeCore, CDefaultMapMode, CActiveMapMode>> activeDefaultMapModeFilter = default;
        void DefaultMapModeInput()
        {
            //��� ������� ��������� ������������ ������ �����
            foreach (int activeMapModeEntity in activeDefaultMapModeFilter.Value)
            {
                //���� ����� �����
                ref CMapModeCore mapMode = ref mapModeCorePool.Value.Get(activeMapModeEntity);

                //��� ������� ������� �������� ��������� ������� �� �����
                foreach (int requestEntity in mouseMapPositionCheckFilter.Value)
                {
                    //���� ������
                    ref RMouseMapPositionCheck requestComp = ref mouseMapPositionCheckPool.Value.Get(requestEntity);

                    //������������ ��������� �������
                    DefaultMapModeMousePositionCheck(
                        ref mapMode,
                        ref requestComp);
                    
                    //������� ������
                    mouseMapPositionCheckPool.Value.Del(requestEntity);
                }
            }
        }

        readonly EcsPoolInject<CProvinceRender> pRPool = default;
        void DefaultMapModeMousePositionCheck(
            ref CMapModeCore mapMode,
            ref RMouseMapPositionCheck requestComp)
        {
            //���� ��������� �� �������
            requestComp.currentProvincePE.Unpack(world.Value, out int provinceEntity);
            ref CProvinceRender pR = ref pRPool.Value.Get(provinceEntity);

            //���� ������������ ������ ���������
            pR.DisplayedObjectPE.Unpack(world.Value, out provinceEntity);

            //����������� ��� �� ��������� ���������
            GBB.Map.MapModeData.ShowMapHoverHighlightRequest(
                showMapHoverHighlightSRPool.Value,
                ref mapMode,
                provinceEntity);
        }
    }
}
