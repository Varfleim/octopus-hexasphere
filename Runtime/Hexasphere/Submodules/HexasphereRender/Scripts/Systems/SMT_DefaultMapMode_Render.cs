
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using Leopotam.EcsLite.Threads;

using GBB.Map.Render;

namespace HS.Hexasphere.Render
{
    public class SMT_DefaultMapMode_Render : EcsThreadSystem<T_DefaultMapMode_Render,
        C_ProvinceRender, C_ProvinceHexasphere, SR_ProvinceRender_Update,
        C_MapModeCore>
    {
        readonly EcsWorldInject world = default;

        readonly EcsCustomInject<MainMapMode_Data> mainMapMode_Data = default;

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
            return world.Filter<C_ProvinceRender>().Inc<C_ProvinceHexasphere>().Inc<SR_ProvinceRender_Update>().End();
        }

        protected override void SetData(IEcsSystems systems, ref T_DefaultMapMode_Render thread)
        {
            thread.world = world.Value;

            thread.mainMapMode_Data = mainMapMode_Data.Value;
        }
    }
}
