
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

using GBB.Map.Render;

namespace HS.Hexasphere.Render
{
    public class SDefaultMapModeRender : IEcsRunSystem
    {
        public void Run(IEcsSystems systems)
        {
            //Создаём запросы изменения визуализации для каждой провинции
            SetMapRenderValuesRequests();
        }

        readonly EcsFilterInject<Inc<C_ProvinceRender>> pRFilter = default;
        readonly EcsPoolInject<SR_UpdateProvinceRender> updateProvinceRenderSRPool = default;
        void SetMapRenderValuesRequests()
        {
            //Для каждой провинции с PR
            foreach(int provinceEntity in pRFilter.Value)
            {
                //Создаём запрос обновления визуализации для неё
                MainMapModeData.UpdateProvinceRenderRequestCreation(
                    updateProvinceRenderSRPool.Value,
                    provinceEntity);
            }
        }
    }
}
