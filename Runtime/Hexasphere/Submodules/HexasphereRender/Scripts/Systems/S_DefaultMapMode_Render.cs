
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

using GBB.Map.Render;

namespace HS.Hexasphere.Render
{
    public class S_DefaultMapMode_Render : IEcsRunSystem
    {
        public void Run(IEcsSystems systems)
        {
            //Создаём запросы изменения визуализации для каждой провинции
            PR_Update_Requests();
        }

        readonly EcsFilterInject<Inc<C_ProvinceRender>> pR_F = default;
        readonly EcsPoolInject<SR_ProvinceRender_Update> pR_Update_SR_P = default;
        void PR_Update_Requests()
        {
            //Для каждой провинции с PR
            foreach(int provinceEntity in pR_F.Value)
            {
                //Создаём запрос обновления визуализации для неё
                MainMapMode_Data.ProvinceRender_Update_Request_Creation(
                    pR_Update_SR_P.Value,
                    provinceEntity);
            }
        }
    }
}
