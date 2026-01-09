
using Leopotam.EcsLite;
using Leopotam.EcsLite.Threads;

using GBB.Map.Render;

namespace HS.Hexasphere.Render
{
    public struct T_DefaultMapMode_Render : IEcsThread<
        C_ProvinceRender, C_ProvinceHexasphere, SR_ProvinceRender_Update,
        C_MapModeCore>
    {
        public EcsWorld world;

        public MainMapMode_Data mainMapMode_Data;

        int[] provinceEntities;

        C_ProvinceRender[] pR_P;
        int[] pR_I;

        C_ProvinceHexasphere[] pHS_P;
        int[] pHS_I;

        SR_ProvinceRender_Update[] pR_Update_SR_P;
        int[] pR_Update_SR_I;

        C_MapModeCore[] mMC_P;
        int[] mMC_I;

        public void Init(
            int[] entities,
            C_ProvinceRender[] pool1, int[] indices1,
            C_ProvinceHexasphere[] pool2, int[] indices2,
            SR_ProvinceRender_Update[] pool3, int[] indices3,
            C_MapModeCore[] pool4, int[] indices4)
        {
            provinceEntities = entities;

            pR_P = pool1;
            pR_I = indices1;

            pHS_P = pool2;
            pHS_I = indices2;

            pR_Update_SR_P = pool3;
            pR_Update_SR_I = indices3;

            mMC_P = pool4;
            mMC_I = indices4;
        }

        public void Execute(int threadId, int fromIndex, int beforeIndex)
        {
            //Берём активный режим карты
            mainMapMode_Data.ActiveMapModePE.Unpack(world, out int mapModeEntity);
            ref C_MapModeCore mapModeCore = ref mMC_P[mMC_I[mapModeEntity]];

            //Для каждой провинции в потоке
            for(int a = fromIndex; a < beforeIndex; a++)
            {
                //Берём провинцию и запрос обновления визуализации
                int provinceEntity = provinceEntities[a];
                ref C_ProvinceRender pR = ref pR_P[pR_I[provinceEntity]];
                ref C_ProvinceHexasphere pHS = ref pHS_P[pHS_I[provinceEntity]];
                ref SR_ProvinceRender_Update requestComp = ref pR_Update_SR_P[pR_Update_SR_I[provinceEntity]];

                //Изменяем запрос соответственно режиму карты
                MainMapMode_Data.ProvinceRender_Update_Request_Update(
                    ref mapModeCore,
                    ref requestComp,
                    world.PackEntity(provinceEntity),
                    0.01f,
                    0);
            }
        }
    }
}
