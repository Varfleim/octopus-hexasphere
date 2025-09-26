
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace HS
{
    public class SMapModesCreation : IEcsInitSystem
    {
        readonly EcsWorldInject world = default;


        readonly EcsPoolInject<GBB.Map.SRMapModeCreation> mapModeCreationSRPool = default;

        readonly EcsPoolInject<GBB.Map.RMapModeUpdateColorsListSecond> mapModeUpdateColorsListSecondRPool = default;


        readonly EcsCustomInject<MapModeData> mapModeData = default;

        public void Init(IEcsSystems systems)
        {
            //Создаём стандартный режим карты
            DefaultMapModeCreation();
        }

        readonly EcsPoolInject<CDefaultMapMode> defaultMapModePool = default;
        void DefaultMapModeCreation()
        {
            //Создаём новую сущность и назначаем ей компонент стандартного режима карты
            int mapModeEntity = world.Value.NewEntity();
            ref CDefaultMapMode defaultMapMode = ref defaultMapModePool.Value.Add(mapModeEntity);

            //Сохраняем PE стандартного режима карты
            mapModeData.Value.defaultMapModePE = world.Value.PackEntity(mapModeEntity);

            //Запрашиваем назначение главного компонента режима карты
            GBB.Map.MapModeData.MapModeCreationRequest(
                mapModeCreationSRPool.Value,
                mapModeEntity, mapModeData.Value.defaultMapModeName,
                false);

            //Запрашиваем вторичное обновление списка цветов режима карты
            GBB.Map.MapModeData.MapModeUpdateColorsListSecondRequest(
                world.Value,
                mapModeUpdateColorsListSecondRPool.Value,
                mapModeData.Value.defaultMapModePE,
                mapModeData.Value.defaultMapModeColors, mapModeData.Value.defaultMapModeDefaultColor);
        }
    }
}
