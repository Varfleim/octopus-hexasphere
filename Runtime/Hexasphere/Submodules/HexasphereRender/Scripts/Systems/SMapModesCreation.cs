
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

using GBB.Map.Render;

namespace HS.Hexasphere.Render
{
    public class SMapModesCreation : IEcsInitSystem
    {
        readonly EcsWorldInject world = default;


        readonly EcsCustomInject<MapModeData> mapModeData = default;

        public void Init(IEcsSystems systems)
        {
            //Создаём стандартный режим карты
            DefaultMapModeCreation();
        }

        readonly EcsPoolInject<SR_MapModeCreation> mapModeCreationSRPool = default;
        readonly EcsPoolInject<R_MapModeUpdateColorsListSecond> mapModeUpdateColorsListSecondRPool = default;
        readonly EcsPoolInject<C_DefaultMapMode> defaultMapModePool = default;
        void DefaultMapModeCreation()
        {
            //Создаём новую сущность и назначаем ей компонент стандартного режима карты
            int mapModeEntity = world.Value.NewEntity();
            ref C_DefaultMapMode defaultMapMode = ref defaultMapModePool.Value.Add(mapModeEntity);

            //Сохраняем PE стандартного режима карты
            mapModeData.Value.DefaultMapModePE = world.Value.PackEntity(mapModeEntity);

            //Запрашиваем назначение главного компонента режима карты
            MainMapModeData.MapModeCreationRequest(
                mapModeCreationSRPool.Value,
                mapModeEntity, mapModeData.Value.DefaultMapModeName,
                mapModeData.Value.DefaultMapModeDefaultState);

            //Запрашиваем вторичное обновление списка цветов режима карты
            MainMapModeData.MapModeUpdateColorsListSecondRequest(
                world.Value,
                mapModeUpdateColorsListSecondRPool.Value,
                mapModeData.Value.DefaultMapModePE,
                mapModeData.Value.DefaultMapModeColors, mapModeData.Value.DefaultMapModeDefaultColor);
        }
    }
}
