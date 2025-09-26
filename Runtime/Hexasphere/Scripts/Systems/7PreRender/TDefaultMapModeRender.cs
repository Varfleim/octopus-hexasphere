
using Leopotam.EcsLite;
using Leopotam.EcsLite.Threads;

using GBB.Map;

namespace HS
{
    public struct TDefaultMapModeRender : IEcsThread<
        CProvinceRender, CProvinceHexasphere, SRUpdateProvinceRender,
        CMapModeCore>
    {
        public EcsWorld world;

        public GBB.Map.MapModeData mainMapModeData;

        int[] provinceEntities;

        CProvinceRender[] pRPool;
        int[] pRIndices;

        CProvinceHexasphere[] pHSPool;
        int[] pHSIndices;

        SRUpdateProvinceRender[] updateProvinceRenderSRPool;
        int[] updateProvinceRenderSRIndices;

        CMapModeCore[] mapModePool;
        int[] mapModeIndices;

        public void Init(
            int[] entities,
            CProvinceRender[] pool1, int[] indices1,
            CProvinceHexasphere[] pool2, int[] indices2,
            SRUpdateProvinceRender[] pool3, int[] indices3,
            CMapModeCore[] pool4, int[] indices4)
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
            mainMapModeData.activeMapModePE.Unpack(world, out int mapModeEntity);
            ref CMapModeCore mapModeCore = ref mapModePool[mapModeIndices[mapModeEntity]];

            //Для каждой провинции в потоке
            for(int a = fromIndex; a < beforeIndex; a++)
            {
                //Берём провинцию и запрос обновления визуализации
                int provinceEntity = provinceEntities[a];
                ref CProvinceRender pR = ref pRPool[pRIndices[provinceEntity]];
                ref CProvinceHexasphere pHS = ref pHSPool[pHSIndices[provinceEntity]];
                ref SRUpdateProvinceRender requestComp = ref updateProvinceRenderSRPool[updateProvinceRenderSRIndices[provinceEntity]];

                //Изменяем запрос соответственно режиму карты
                GBB.Map.MapModeData.UpdateProvinceRenderRequestUpdate(
                    ref mapModeCore,
                    ref requestComp,
                    pHS.selfPE,
                    0.01f,
                    0);
            }
        }
    }
}
