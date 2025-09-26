
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
            //Для каждой активной карты
            foreach(int activeMapEntity in activeMapFilter.Value)
            {
                //Берём карту
                ref CMap activeMap = ref mapPool.Value.Get(activeMapEntity);

                //Обрабатываем ввод в режимах карты
                MapModesInput();
            }
        }

        readonly EcsFilterInject<Inc<RMouseMapPositionCheck>> mouseMapPositionCheckFilter = default;
        readonly EcsPoolInject<RMouseMapPositionCheck> mouseMapPositionCheckPool = default;
        void MapModesInput()
        {
            //Обрабатываем ввод в стандартном режиме карты
            DefaultMapModeInput();
        }

        readonly EcsFilterInject<Inc<CMapModeCore, CDefaultMapMode, CActiveMapMode>> activeDefaultMapModeFilter = default;
        void DefaultMapModeInput()
        {
            //Для каждого активного стандартного режима карты
            foreach (int activeMapModeEntity in activeDefaultMapModeFilter.Value)
            {
                //Берём режим карты
                ref CMapModeCore mapMode = ref mapModeCorePool.Value.Get(activeMapModeEntity);

                //Для каждого запроса проверки положения курсора на карте
                foreach (int requestEntity in mouseMapPositionCheckFilter.Value)
                {
                    //Берём запрос
                    ref RMouseMapPositionCheck requestComp = ref mouseMapPositionCheckPool.Value.Get(requestEntity);

                    //Обрабатываем положение курсора
                    DefaultMapModeMousePositionCheck(
                        ref mapMode,
                        ref requestComp);
                    
                    //Удаляем запрос
                    mouseMapPositionCheckPool.Value.Del(requestEntity);
                }
            }
        }

        readonly EcsPoolInject<CProvinceRender> pRPool = default;
        void DefaultMapModeMousePositionCheck(
            ref CMapModeCore mapMode,
            ref RMouseMapPositionCheck requestComp)
        {
            //Берём провинцию из запроса
            requestComp.currentProvincePE.Unpack(world.Value, out int provinceEntity);
            ref CProvinceRender pR = ref pRPool.Value.Get(provinceEntity);

            //Берём отображаемый объект провинции
            pR.DisplayedObjectPE.Unpack(world.Value, out provinceEntity);

            //Запрашиваем для неё подсветку наведения
            GBB.Map.MapModeData.ShowMapHoverHighlightRequest(
                showMapHoverHighlightSRPool.Value,
                ref mapMode,
                provinceEntity);
        }
    }
}
