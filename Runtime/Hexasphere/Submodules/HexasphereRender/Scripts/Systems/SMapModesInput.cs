
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

using GBB.Input;
using GBB.Map.Render;

namespace HS.Hexasphere.Render
{
    public class SMapModesInput : IEcsRunSystem
    {
        readonly EcsWorldInject world = default;


        readonly EcsPoolInject<C_MapModeCore> mapModeCorePool = default;


        readonly EcsPoolInject<SR_ShowMapHoverHighlight> showMapHoverHighlightSRPool = default;

        readonly EcsFilterInject<Inc<R_MouseMapPositionCheck>> mouseMapPositionCheckFilter = default;
        readonly EcsPoolInject<R_MouseMapPositionCheck> mouseMapPositionCheckPool = default;


        readonly EcsCustomInject<MapRenderData> mapRenderData = default;
        readonly EcsCustomInject<MapModeData> mapModeData = default;

        public void Run(IEcsSystems systems)
        {
            //Обрабатываем ввод в режимах карты
            MapModesInput();
        }

        void MapModesInput()
        {
            //Обрабатываем ввод в стандартном режиме карты
            DefaultMapModeInput();
        }

        void DefaultMapModeInput()
        {
            //Если активен стандартный режим карты
            if(mapRenderData.Value.ActiveMapPE.EqualsTo(mapModeData.Value.DefaultMapModePE))
            {
                //Берём стандартный режим карты
                mapModeData.Value.DefaultMapModePE.Unpack(world.Value, out int activeMapModeEntity);
                ref C_MapModeCore activeMapMode = ref mapModeCorePool.Value.Get(activeMapModeEntity);

                //Для каждого запроса проверки положения курсора на карте
                foreach (int requestEntity in mouseMapPositionCheckFilter.Value)
                {
                    //Берём запрос
                    ref R_MouseMapPositionCheck requestComp = ref mouseMapPositionCheckPool.Value.Get(requestEntity);

                    //Обрабатываем положение курсора
                    DefaultMapModeMousePositionCheck(
                        ref activeMapMode,
                        ref requestComp);

                    //Удаляем запрос
                    mouseMapPositionCheckPool.Value.Del(requestEntity);
                }
            }
        }

        readonly EcsPoolInject<C_ProvinceRender> pRPool = default;
        void DefaultMapModeMousePositionCheck(
            ref C_MapModeCore mapMode,
            ref R_MouseMapPositionCheck requestComp)
        {
            //Берём провинцию из запроса
            requestComp.currentProvincePE.Unpack(world.Value, out int provinceEntity);
            ref C_ProvinceRender pR = ref pRPool.Value.Get(provinceEntity);

            //Берём отображаемый объект провинции
            pR.DisplayedObjectPE.Unpack(world.Value, out provinceEntity);

            //Запрашиваем для неё подсветку наведения
            MainMapModeData.ShowMapHoverHighlightRequest(
                showMapHoverHighlightSRPool.Value,
                ref mapMode,
                provinceEntity);
        }
    }
}
