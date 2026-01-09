
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

using GBB.Input;
using GBB.Map.Render;

namespace HS.Hexasphere.Render
{
    public class S_MapMode_Input : IEcsRunSystem
    {
        readonly EcsWorldInject world = default;


        readonly EcsPoolInject<C_MapModeCore> mMC_P = default;


        readonly EcsPoolInject<SR_ProvinceHoverHighlight_Show> pHoverHighlight_Show_SR_P = default;

        readonly EcsFilterInject<Inc<R_Mouse_MapPositionCheck>> mouse_MapPositionCheck_F = default;
        readonly EcsPoolInject<R_Mouse_MapPositionCheck> mouse_MapPositionCheck_P = default;


        readonly EcsCustomInject<MapRender_Data> mapRender_Data = default;
        readonly EcsCustomInject<MapMode_Data> mapMode_Data = default;

        public void Run(IEcsSystems systems)
        {
            //Обрабатываем ввод в режимах карты
            MMs_Input();
        }

        void MMs_Input()
        {
            //Обрабатываем ввод в стандартном режиме карты
            DefaultMM_Input();
        }

        void DefaultMM_Input()
        {
            //Если активен стандартный режим карты
            if(mapRender_Data.Value.ActiveMapPE.EqualsTo(mapMode_Data.Value.DefaultMapModePE))
            {
                //Берём стандартный режим карты
                mapMode_Data.Value.DefaultMapModePE.Unpack(world.Value, out int activeMapModeEntity);
                ref C_MapModeCore activeMapMode = ref mMC_P.Value.Get(activeMapModeEntity);

                //Для каждого запроса проверки положения курсора на карте
                foreach (int requestEntity in mouse_MapPositionCheck_F.Value)
                {
                    //Берём запрос
                    ref R_Mouse_MapPositionCheck requestComp = ref mouse_MapPositionCheck_P.Value.Get(requestEntity);

                    //Обрабатываем положение курсора
                    DefaultMM_MousePositionCheck(
                        ref activeMapMode,
                        ref requestComp);

                    //Удаляем запрос
                    mouse_MapPositionCheck_P.Value.Del(requestEntity);
                }
            }
        }

        readonly EcsPoolInject<C_ProvinceRender> pR_P = default;
        void DefaultMM_MousePositionCheck(
            ref C_MapModeCore mapMode,
            ref R_Mouse_MapPositionCheck requestComp)
        {
            //Берём провинцию из запроса
            requestComp.currentProvincePE.Unpack(world.Value, out int provinceEntity);
            ref C_ProvinceRender pR = ref pR_P.Value.Get(provinceEntity);

            //Берём отображаемый объект провинции
            pR.DisplayedObjectPE.Unpack(world.Value, out provinceEntity);

            //Запрашиваем для неё подсветку наведения
            MainMapMode_Data.ProvinceHoverHighlight_Show_Request(
                pHoverHighlight_Show_SR_P.Value,
                ref mapMode,
                provinceEntity);
        }
    }
}
