
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using Leopotam.EcsLite.Threads;

using GBB.Map;

namespace HS
{
    public class SMTDefaultMapModeRender : EcsThreadSystem<TDefaultMapModeRender,
        CProvinceRender, CProvinceHexasphere, SRUpdateProvinceRender,
        CMapModeCore>
    {
        readonly EcsWorldInject world = default;

        readonly EcsCustomInject<GBB.Map.MapModeData> mainMapModeData = default;

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
            return world.Filter<CProvinceRender>().Inc<CProvinceHexasphere>().Inc<SRUpdateProvinceRender>().End();
        }

        protected override void SetData(IEcsSystems systems, ref TDefaultMapModeRender thread)
        {
            thread.world = world.Value;

            thread.mainMapModeData = mainMapModeData.Value;
        }
    }
}
