
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
            //Для каждой активной карты
            foreach (int activeMapEntity in activeMapFilter.Value)
            {
                //Берём карту
                ref CMap activeMap = ref mapPool.Value.Get(activeMapEntity);

                //Создаём запросы изменения визуализации для каждой провинции
                SetMapRenderValuesRequests(ref activeMap);
            }
        }

        readonly EcsPoolInject<SRUpdateProvinceRender> setMapRenderValuesSRPool = default;
        void SetMapRenderValuesRequests(
            ref CMap map)
        {
            //Для каждой провинции карты
            for(int a = 0; a < map.provincePEs.Length; a++)
            {
                //Берём сущность провинции
                map.provincePEs[a].Unpack(world.Value, out int provinceEntity);

                //Создаём запрос обновления визуализации для неё
                GBB.Map.MapModeData.UpdateProvinceRenderRequestCreation(
                    setMapRenderValuesSRPool.Value,
                    provinceEntity);
            }
        }
    }
}
