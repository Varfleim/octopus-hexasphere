
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using Leopotam.EcsLite.Threads;

using GBB.Map.Render;

namespace HS.Hexasphere.Render
{
    public class SMTDefaultMapModeRender : EcsThreadSystem<TDefaultMapModeRender,
        C_ProvinceRender, C_ProvinceHexasphere, SR_UpdateProvinceRender,
        C_MapModeCore>
    {
        readonly EcsWorldInject world = default;

        readonly EcsCustomInject<MainMapModeData> mainMapModeData = default;

        protected override int GetChunkSize(IEcsSystems systems)
        {
            return 4096;
        }

        protected override EcsWorld GetWorld(IEcsSystems systems)
        {
            return systems.GetWorld();
        }

        protected override EcsFilter GetFilter(EcsWorld world)
        {
            return world.Filter<C_ProvinceRender>().Inc<C_ProvinceHexasphere>().Inc<SR_UpdateProvinceRender>().End();
        }

        protected override void SetData(IEcsSystems systems, ref TDefaultMapModeRender thread)
        {
            thread.world = world.Value;

            thread.mainMapModeData = mainMapModeData.Value;
        }
    }
}
