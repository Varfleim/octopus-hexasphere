
using Leopotam.EcsLite;
using Leopotam.EcsLite.Threads;

using GBB.Map.Render;

namespace HS.Hexasphere.Render
{
    public struct TDefaultMapModeRender : IEcsThread<
        C_ProvinceRender, C_ProvinceHexasphere, SR_UpdateProvinceRender,
        C_MapModeCore>
    {
        public EcsWorld world;

        public MainMapModeData mainMapModeData;

        int[] provinceEntities;

        C_ProvinceRender[] pRPool;
        int[] pRIndices;

        C_ProvinceHexasphere[] pHSPool;
        int[] pHSIndices;

        SR_UpdateProvinceRender[] updateProvinceRenderSRPool;
        int[] updateProvinceRenderSRIndices;

        C_MapModeCore[] mapModePool;
        int[] mapModeIndices;

        public void Init(
            int[] entities,
            C_ProvinceRender[] pool1, int[] indices1,
            C_ProvinceHexasphere[] pool2, int[] indices2,
            SR_UpdateProvinceRender[] pool3, int[] indices3,
            C_MapModeCore[] pool4, int[] indices4)
        {
            provinceEntities = entities;

            pRPool = pool1;
            pRIndices = indices1;

            pHSPool = pool2;
            pHSIndices = indices2;

            updateProvinceRenderSRPool = pool3;
            updateProvinceRenderSRIndices = indices3;

            mapModePool = pool4;
            mapModeIndices = indices4;
        }

        public void Execute(int threadId, int fromIndex, int beforeIndex)
        {
            //Берём активный режим карты
            mainMapModeData.ActiveMapModePE.Unpack(world, out int mapModeEntity);
            ref C_MapModeCore mapModeCore = ref mapModePool[mapModeIndices[mapModeEntity]];

            //Для каждой провинции в потоке
            for(int a = fromIndex; a < beforeIndex; a++)
            {
                //Берём провинцию и запрос обновления визуализации
                int provinceEntity = provinceEntities[a];
                ref C_ProvinceRender pR = ref pRPool[pRIndices[provinceEntity]];
                ref C_ProvinceHexasphere pHS = ref pHSPool[pHSIndices[provinceEntity]];
                ref SR_UpdateProvinceRender requestComp = ref updateProvinceRenderSRPool[updateProvinceRenderSRIndices[provinceEntity]];

                //Изменяем запрос соответственно режиму карты
                MainMapModeData.UpdateProvinceRenderRequestUpdate(
                    ref mapModeCore,
                    ref requestComp,
                    world.PackEntity(provinceEntity),
                    0.01f,
                    0);
            }
        }
    }
}
