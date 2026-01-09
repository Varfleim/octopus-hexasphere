
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

using GBB.Map.Render;

namespace HS.Hexasphere.Render
{
    public class S_MapMode_Creation : IEcsInitSystem
    {
        readonly EcsWorldInject world = default;


        readonly EcsCustomInject<MapMode_Data> mapMode_Data = default;

        public void Init(IEcsSystems systems)
        {
            //Создаём стандартный режим карты
            DefaultMapMode_Creation();
        }

        readonly EcsPoolInject<SR_MapModeCore_Creation> mMC_Creation_SR_P = default;
        readonly EcsPoolInject<R_MapMode_UpdateColorsListSecond> mM_UpdateColorsListSecond_R_P = default;
        readonly EcsPoolInject<C_DefaultMapMode> defaultMapMode_P = default;
        void DefaultMapMode_Creation()
        {
            //Создаём новую сущность и назначаем ей компонент стандартного режима карты
            int mapModeEntity = world.Value.NewEntity();
            ref C_DefaultMapMode defaultMapMode = ref defaultMapMode_P.Value.Add(mapModeEntity);

            //Сохраняем PE стандартного режима карты
            mapMode_Data.Value.DefaultMapModePE = world.Value.PackEntity(mapModeEntity);

            //Запрашиваем назначение главного компонента режима карты
            MainMapMode_Data.MapModeCore_Creation_Request(
                mMC_Creation_SR_P.Value,
                mapModeEntity, mapMode_Data.Value.DefaultMapModeName,
                mapMode_Data.Value.DefaultMapModeDefaultState);

            //Запрашиваем вторичное обновление списка цветов режима карты
            MainMapMode_Data.MapMode_UpdateColorsListSecond_Request(
                world.Value,
                mM_UpdateColorsListSecond_R_P.Value,
                mapMode_Data.Value.DefaultMapModePE,
                mapMode_Data.Value.DefaultMapModeColors, mapMode_Data.Value.DefaultMapModeDefaultColor);
        }
    }
}
