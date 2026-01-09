
using System.Collections.Generic;

using UnityEngine;

using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

using GBB.Map;
using GBB.Map.Render;

namespace HS.Hexasphere.Render
{
    public class S_Hexasphere_Render : IEcsRunSystem
    {
        readonly EcsWorldInject world = default;

        readonly EcsPoolInject<C_ProvinceCore> pC_P = default;
        readonly EcsPoolInject<C_ProvinceRender> pR_P = default;
        readonly EcsFilterInject<Inc<C_ProvinceRender, C_ProvinceHexasphere>> pHS_F = default;
        readonly EcsFilterInject<Inc<C_ProvinceRender, C_ProvinceHexasphere, C_ProvinceHexasphereRender>> pHSR_F = default;
        readonly EcsPoolInject<C_ProvinceHexasphere> pHS_P = default;
        readonly EcsPoolInject<C_ProvinceHexasphereRender> pHSR_P = default;
        readonly EcsPoolInject<C_ProvinceMapPanels> pMP_P = default;

        readonly EcsPoolInject<C_MapModeCore> mMC_P = default;


        readonly EcsCustomInject<MapRender_Data> mapRender_Data = default;
        readonly EcsCustomInject<MainMapMode_Data> mainMapMode_Data = default;
        readonly EcsCustomInject<HexasphereRender_Data> hexasphereRender_Data = default;

        public void Run(IEcsSystems systems)
        {
            //Инициализация карты
            Map_Initialization();

            //Обновление граней
            Map_UpdateEdges();

            //Обновление провинций
            Map_UpdateProvinces();

            //Обновление материалов подсветки
            MaterialHighlight_Update();

            //Подсветка наведения
            ProvinceHoverHighlight();

            //Проверяем запросы изменения положения панелей карты
            ProvinceMapPanels();

            //Проверяем, нет ли пустых GO провинций для удаления
            ProvinceGO_CheckEmpty();
        }

        readonly EcsFilterInject<Inc<R_Map_RenderInitialization>> map_RenderInitialization_R_F = default;
        readonly EcsPoolInject<R_Map_RenderInitialization> map_RenderInitialization_R_P = default;
        void Map_Initialization()
        {
            //Для каждого запроса инициализации карты
            foreach(int requestEntity in map_RenderInitialization_R_F.Value)
            {
                //Берём запрос
                ref R_Map_RenderInitialization requestComp = ref map_RenderInitialization_R_P.Value.Get(requestEntity);

                //Берём активный режим карты
                mainMapMode_Data.Value.ActiveMapModePE.Unpack(world.Value, out int activeMapModeEntity);
                ref C_MapModeCore activeMapMode = ref mMC_P.Value.Get(activeMapModeEntity);

                //Инициализируем гексасферу
                HS_Initialization();

                //Создаём провинции гексасферы
                HS_CreateProvinces();

                //Обновляем материалы
                HS_UpdateMaterials();

                //Удаляем запрос
                map_RenderInitialization_R_P.Value.Del(requestEntity);
            }
        }

        readonly EcsFilterInject<Inc<R_Map_UpdateEdges>> map_UpdateEdges_R_F = default;
        readonly EcsPoolInject<R_Map_UpdateEdges> map_UpdateEdges_R_P = default;
        void Map_UpdateEdges()
        {
            //Для каждого запроса обновления граней
            foreach(int requestEntity in map_UpdateEdges_R_F.Value)
            {
                //Берём запрос
                ref R_Map_UpdateEdges requestComp = ref map_UpdateEdges_R_P.Value.Get(requestEntity);

                //Если требуется обновление тонких граней
                if(requestComp.isThinUpdated == true)
                {
                    //Обновляем тонкие грани
                    PR_UpdateThinEdges();
                }

                //Если требуется обновление толстых граней
                if(requestComp.isThickUpdated == true)
                {
                    //Обновляем толстые грани
                    PR_UpdateThickEdges();
                }

                //Удаляем запрос
                map_UpdateEdges_R_P.Value.Del(requestEntity);
            }
        }

        readonly EcsFilterInject<Inc<R_Map_UpdateProvincesRender>> map_UpdatePR_R_F = default;
        readonly EcsPoolInject<R_Map_UpdateProvincesRender> map_UpdatePR_R_P = default;
        void Map_UpdateProvinces()
        {
            //Для каждого запроса обновления провинций
            foreach(int requestEntity in map_UpdatePR_R_F.Value)
            {
                //Берём запрос
                ref R_Map_UpdateProvincesRender requestComp = ref map_UpdatePR_R_P.Value.Get(requestEntity);

                //Берём активный режим карты
                mainMapMode_Data.Value.ActiveMapModePE.Unpack(world.Value, out int activeMapModeEntity);
                ref C_MapModeCore activeMapMode = ref mMC_P.Value.Get(activeMapModeEntity);

                //Если требуется обновление материалов
                if (requestComp.isMaterialUpdated == true)
                {
                    //Обновляем материалы
                    HS_UpdateMaterials();
                }

                //Если требуется обновление высот
                if (requestComp.isHeightUpdated == true)
                {
                    //Обновляем высоты провинций
                    PR_UpdateHeights();

                    //Обновляем меши подсветки провинций
                    ProvinceHoverHighlight_UpdateMeshes();

                    //Обновляем панели карты провинций
                    PMP_UpdateAltitude();
                }

                //Если требуется обновление цветов
                if (requestComp.isColorUpdated == true)
                {
                    //Обновляем цвета провинций
                    PR_UpdateColors(ref activeMapMode);
                }

                //Удаляем запрос
                map_UpdatePR_R_P.Value.Del(requestEntity);
            } 
        }

        readonly EcsFilterInject<Inc<C_ProvinceHexasphereRender>, Exc<C_ProvinceRender>> pHSR_WithoutPR_F = default;
        void HS_Initialization()
        {
            //Удаляем родительский объект чанков, если он не пуст
            if (hexasphereRender_Data.Value.chunksRootGO != null)
            {
                Object.DestroyImmediate(hexasphereRender_Data.Value.chunksRootGO);
            }

            //Удаляем родительский объект провинций, если он не пуст
            if (hexasphereRender_Data.Value.provincesRootGO != null)
            {
                Object.DestroyImmediate(hexasphereRender_Data.Value.provincesRootGO);
            }

            //Для каждой провинции с PHSR, но без PR
            foreach(int provinceEntity in pHSR_WithoutPR_F.Value)
            {
                //Удаляем компонент PHSR
                pHSR_P.Value.Del(provinceEntity);
            }

            //Инициализируем чанки
            Chunks_Initialization();
        }

        /// <summary>
        /// Данная функция только создаёт чанки соответствующего размера, но не заполняет их реальными данными и не отображает
        /// </summary>
        void Chunks_Initialization()
        {
            //Создаём списки массивов для данных чанков
            List<Vector3[]> chunksVerticesList = new();
            List<int[]> chunksIndicesList = new();
            List<Vector4[]> chunksUV2List = new();

            List<Vector4[]> chunksUVList = new();
            List<Color32[]> chunksColorsList = new();

            //Создаём счётчик чанков
            int chunkIndex = 0;

            //Создаём списки для текущего чанка
            List<Vector3> chunkVertices = new();
            List<int> chunkIndices = new();
            List<Vector4> chunkUV2 = new();

            List<Vector4> chunkUV = new();
            List<Color32> chunkColors = new();

            //Создаём счётчик вершин в чанке
            int chunkVerticesCount = 0;

            //Для каждой отображаемой провинции
            foreach (int provinceEntity in pHS_F.Value)
            {
                //Берём компоненты провинции
                ref C_ProvinceRender pR = ref pR_P.Value.Get(provinceEntity);
                ref C_ProvinceHexasphere pHS = ref pHS_P.Value.Get(provinceEntity);

                //Назначаем провинции компонент PHSR и заполняем его данные
                ref C_ProvinceHexasphereRender pHSR = ref pHSR_P.Value.Add(provinceEntity);
                pHSR = new(0);

                //Если текущий чанк заполнен, сохраняем его данные и создаём новый
                //Если количество вершин больше максимального количества в чанке
                if (chunkVerticesCount > HexasphereRender_Data.maxVertexCountPerChunk)
                {
                    //Заносим данные предыдущего чанка как массивы в списки массивов, а затем очищаем
                    chunksVerticesList.Add(chunkVertices.ToArray());
                    chunkVertices.Clear();
                    chunksIndicesList.Add(chunkIndices.ToArray());
                    chunkIndices.Clear();
                    chunksUV2List.Add(chunkUV2.ToArray());
                    chunkUV2.Clear();

                    chunksUVList.Add(chunkUV.ToArray());
                    chunkUV.Clear();
                    chunksColorsList.Add(chunkColors.ToArray());
                    chunkColors.Clear();

                    //Увеличиваем счётчик чанков
                    chunkIndex++;

                    //Обнуляем счётчик вершин в чанке
                    chunkVerticesCount = 0;
                }

                //Определяем количество вершин провинции
                int provinceVerticesCount = pHS.vertexPoints.Length;

                #region Vertices
                //Для каждой вершины провинции
                for (int a = 0; a < pHS.vertexPoints.Length; a++)
                {
                    //Заносим в список вершин пустую вершину
                    chunkVertices.Add(Vector3.zero);
                }

                //Заносим две дополнительные вершины, потому что сфера объёмна
                chunkVertices.Add(Vector3.zero);
                chunkVertices.Add(Vector3.zero);
                #endregion

                //Увеличиваем количество вершин
                provinceVerticesCount += 2;

                #region UV2
                //Для каждой вершины провинции, учитывая дополнительные
                for (int a = 0; a < provinceVerticesCount; a++)
                {
                    //Заносим в список пустую координату
                    chunkUV2.Add(Vector4.zero);
                }
                #endregion

                #region Indices
                //Определяем, какой массив индексов использует провинция
                int[] indicesArray;
                //Если провинция - гексагон
                if (pHS.vertexPoints.Length == 6)
                {
                    //Берём массив индексов гексагона
                    indicesArray = hexasphereRender_Data.Value.hexagonIndicesExtruded;
                }
                //Иначе провинция - пентагон
                else
                {
                    //Берём массив индексов пентагона
                    indicesArray = hexasphereRender_Data.Value.pentagonIndicesExtruded;
                }

                pHSR.parentChunkTriangleStart = chunkIndices.Count;
                //Для каждого индекса провинции
                for (int a = 0; a < indicesArray.Length; a++)
                {
                    //Заносим в список индексы
                    chunkIndices.Add(0);
                }
                #endregion

                //Определяем положение данных провинции в чанках
                pHSR.parentChunkStart = chunkVerticesCount;
                pHSR.parentChunkIndex = chunkIndex;
                pHSR.parentChunkLength = provinceVerticesCount;

                //Определяем, какой массив UV использует провинция
                Vector2[] uVArray;
                //Если провинция - гексагон
                if (pHS.vertexPoints.Length == 6)
                {
                    //Берём массив UV гексагона
                    uVArray = hexasphereRender_Data.Value.hexagonUVsExtruded;
                }
                //Иначе провинция - пентагон
                else
                {
                    //Берём массив UV пентагона
                    uVArray = hexasphereRender_Data.Value.pentagonUVsExtruded;
                }

                #region UV
                //Для каждого UV провинции
                for(int a = 0; a < uVArray.Length; a++)
                {
                    //Заносим в список пустую координату
                    chunkUV.Add(Vector4.zero);
                }
                #endregion

                #region Colors
                //Для каждого UV провинции
                for (int a = 0; a < uVArray.Length; a++)
                {
                    //Заносим в список белый цвет
                    chunkColors.Add(Color.white);
                }
                #endregion

                //Увеличиваем количество вершин в чанке на количество вершин провинции
                chunkVerticesCount += provinceVerticesCount;
            }

            //Заносим данные последнего чанка как массивы в списки массивов
            chunksVerticesList.Add(chunkVertices.ToArray());
            chunksIndicesList.Add(chunkIndices.ToArray());
            chunksUV2List.Add(chunkUV2.ToArray());

            chunksUVList.Add(chunkUV.ToArray());
            chunksColorsList.Add(chunkColors.ToArray());

            //Сохраняем списки массивов как массивы
            hexasphereRender_Data.Value.chunksVertices = chunksVerticesList.ToArray();
            hexasphereRender_Data.Value.chunksIndices = chunksIndicesList.ToArray();
            hexasphereRender_Data.Value.chunksUV2 = chunksUV2List.ToArray();

            hexasphereRender_Data.Value.chunksUV = chunksUVList.ToArray();
            hexasphereRender_Data.Value.chunksColors = chunksColorsList.ToArray();

            #region GO and Meshes
            //Создаём родительский GO для чанков и сохраняем его
            GameObject chunksRootGO = Map_CreateGOAndParent(
                hexasphereRender_Data.Value.HexasphereGO.transform,
                hexasphereRender_Data.Value.ChunksRootGOName);
            hexasphereRender_Data.Value.chunksRootGO = chunksRootGO;

            //Создаём списки мешфильтров, мешей и мешрендереров
            List<MeshFilter> chunkMeshFilters = new();
            List<Mesh> chunkMeshes = new();
            List<MeshRenderer> chunkMeshRenderers = new();

            //Для каждого чанка
            for(int a = 0; a <= chunkIndex; a++)
            {
                //Создаём GO чанка
                GameObject chunkGO = Map_CreateGOAndParent(
                    chunksRootGO.transform,
                    hexasphereRender_Data.Value.ChunkGOName);

                //Назначаем чанку компонент мешфильтра и заносим его в список
                MeshFilter meshFilter = chunkGO.AddComponent<MeshFilter>();
                chunkMeshFilters.Add(meshFilter);

                //Создаём меш, заносим его в список и назначаем мешфильтру
                Mesh mesh = new();
                chunkMeshes.Add(mesh);
                meshFilter.sharedMesh = mesh;

                //Назначаем чанку компонент мешрендерера и заносим его в список
                MeshRenderer meshRenderer = chunkGO.AddComponent<MeshRenderer>();
                chunkMeshRenderers.Add(meshRenderer);
            }

            //Сохраняем списки мешей как массивы
            hexasphereRender_Data.Value.chunkMeshFilters = chunkMeshFilters.ToArray();
            hexasphereRender_Data.Value.chunkMeshes = chunkMeshes.ToArray();
            hexasphereRender_Data.Value.chunkMeshRenderers = chunkMeshRenderers.ToArray();

            //Создаём массивы списков для граней
            hexasphereRender_Data.Value.thinEdgesChunkVertices = new List<Vector3>[chunkIndex + 1];
            hexasphereRender_Data.Value.thinEdgesChunkIndices = new List<int>[chunkIndex + 1];
            hexasphereRender_Data.Value.thinEdgesChunkUVs = new List<Vector2>[chunkIndex + 1];
            hexasphereRender_Data.Value.thinEdgesChunkColors = new List<Color32>[chunkIndex + 1];

            hexasphereRender_Data.Value.thickEdgesChunkVertices = new List<Vector3>[chunkIndex + 1];
            hexasphereRender_Data.Value.thickEdgesChunkIndices = new List<int>[chunkIndex + 1];
            hexasphereRender_Data.Value.thickEdgesChunkUVs = new List<Vector2>[chunkIndex + 1];
            hexasphereRender_Data.Value.thickEdgesChunkColors = new List<Color32>[chunkIndex + 1];

            //Создаём родительский GO для провинций и сохраняем его
            GameObject provincesRootGO = Map_CreateGOAndParent(
                hexasphereRender_Data.Value.HexasphereGO.transform,
                hexasphereRender_Data.Value.ProvincesRootGOName);
            hexasphereRender_Data.Value.provincesRootGO = provincesRootGO;
            #endregion
        }

        void HS_CreateProvinces()
        {
            //Для каждой отображаемой провинции
            foreach (int provinceEntity in pHSR_F.Value)
            {
                //Берём компоненты провинции
                ref C_ProvinceRender pR = ref pR_P.Value.Get(provinceEntity);
                ref C_ProvinceHexasphere pHS = ref pHS_P.Value.Get(provinceEntity);
                ref C_ProvinceHexasphereRender pHSR = ref pHSR_P.Value.Get(provinceEntity);

                //Берём массивы чанка, в котором располагается провинция
                ref Vector3[] chunkVertices = ref hexasphereRender_Data.Value.chunksVertices[pHSR.parentChunkIndex];
                ref int[] chunkIndices = ref hexasphereRender_Data.Value.chunksIndices[pHSR.parentChunkIndex];
                ref Vector4[] chunkUV2 = ref hexasphereRender_Data.Value.chunksUV2[pHSR.parentChunkIndex];

                //Определяем количество вершин провинции
                int provinceVerticesCount = pHS.vertexPoints.Length;

                //Создаём структуру для UV-координат провинции
                Vector4 gpos = Vector4.zero;

                //Для каждой вершины провинции
                for(int a = 0; a < pHS.vertexPoints.Length; a++)
                {
                    //Берём вершину
                    D_HexaspherePoint point = pHS.vertexPoints[a];

                    //Берём координату вершины
                    Vector3 vertex = point.ProjectedVector3;

                    //Заносим её в массив вершин
                    chunkVertices[pHSR.parentChunkStart + a] = vertex;

                    //Рассчитываем компоненты UV
                    gpos.x += vertex.x;
                    gpos.y += vertex.y;
                    gpos.z += vertex.z;
                }

                //Корректируем gpos
                gpos.x /= pHS.vertexPoints.Length;
                gpos.y /= pHS.vertexPoints.Length;
                gpos.z /= pHS.vertexPoints.Length;

                //Определяем, какой массив индексов использует провинция
                int[] indicesArray;

                //Если провинция - гексагон
                if(pHS.vertexPoints.Length == 6)
                {
                    //Заносим вершины основания гексагона в массив
                    chunkVertices[pHSR.parentChunkStart + 6]
                        = (pHS.vertexPoints[1].ProjectedVector3 + pHS.vertexPoints[5].ProjectedVector3) * 0.5f;
                    chunkVertices[pHSR.parentChunkStart + 7]
                        = (pHS.vertexPoints[2].ProjectedVector3 + pHS.vertexPoints[4].ProjectedVector3) * 0.5f;

                    //Обновляем количество вершин
                    provinceVerticesCount += 2;

                    //Назначаем провинции массив индексов гексагона
                    indicesArray = hexasphereRender_Data.Value.hexagonIndicesExtruded;
                }
                //Иначе провинция - пентагон
                else
                {
                    //Заносим вершины основания пентагона в массив
                    chunkVertices[pHSR.parentChunkStart + 5]
                        = (pHS.vertexPoints[1].ProjectedVector3 + pHS.vertexPoints[4].ProjectedVector3) * 0.5f;
                    chunkVertices[pHSR.parentChunkStart + 6]
                        = (pHS.vertexPoints[2].ProjectedVector3 + pHS.vertexPoints[4].ProjectedVector3) * 0.5f;

                    //Обновляем количество вершин
                    provinceVerticesCount += 2;

                    //Обнуляем bevel для пентагонов
                    gpos.w = 1.0f;

                    //Назначаем провинции массив индексов пентагона
                    indicesArray = hexasphereRender_Data.Value.pentagonIndicesExtruded;
                }

                //Для каждой вершины
                for (int a = 0; a < provinceVerticesCount; a++)
                {
                    //Заносим gpos в массив UV-координат
                    chunkUV2[pHSR.parentChunkStart + a] = gpos;
                }

                //Для каждого индекса
                for (int a = 0; a < indicesArray.Length; a++)
                {
                    //Заносим индекс в массив индексов
                    chunkIndices[pHSR.parentChunkTriangleStart + a] = pHSR.parentChunkStart + indicesArray[a];
                }
            }

            //Для каждого мешфильтра чанка
            for(int a = 0; a < hexasphereRender_Data.Value.chunkMeshFilters.Length; a++)
            {
                //Заполняем меш вершинами, индексами и UV
                hexasphereRender_Data.Value.chunkMeshes[a].SetVertices(hexasphereRender_Data.Value.chunksVertices[a]);
                hexasphereRender_Data.Value.chunkMeshes[a].SetTriangles(hexasphereRender_Data.Value.chunksIndices[a], 0);
                hexasphereRender_Data.Value.chunkMeshes[a].SetUVs(1, hexasphereRender_Data.Value.chunksUV2[a]);
            }
        }

        void HS_UpdateMaterials()
        {
            //Обновляем тени
            MeshRenderers_UpdateShadowSupport();

            //Обновляем материал провинций
            hexasphereRender_Data.Value.provinceMaterial.SetFloat("_GradientIntensity", 1f - hexasphereRender_Data.Value.gradientIntensity);
            hexasphereRender_Data.Value.provinceMaterial.SetFloat("_ExtrusionMultiplier", hexasphereRender_Data.Value.ExtrudeMultiplier);
            hexasphereRender_Data.Value.provinceMaterial.SetColor("_Color", hexasphereRender_Data.Value.tileTintColor);
            hexasphereRender_Data.Value.provinceMaterial.SetColor("_AmbientColor", hexasphereRender_Data.Value.ambientColor);
            hexasphereRender_Data.Value.provinceMaterial.SetFloat("_MinimumLight", hexasphereRender_Data.Value.minimumLight);

            //Обновляем материалы граней
            hexasphereRender_Data.Value.thinEdgesMaterial.SetFloat("_GradientIntensity", 1f - hexasphereRender_Data.Value.gradientIntensity);
            hexasphereRender_Data.Value.thinEdgesMaterial.SetFloat("_ExtrusionMultiplier", hexasphereRender_Data.Value.ExtrudeMultiplier);
            Color thinEdgesColor = hexasphereRender_Data.Value.thinEdgesColor;
            thinEdgesColor.r *= hexasphereRender_Data.Value.thinEdgesColorIntensity;
            thinEdgesColor.g *= hexasphereRender_Data.Value.thinEdgesColorIntensity;
            thinEdgesColor.b *= hexasphereRender_Data.Value.thinEdgesColorIntensity;
            hexasphereRender_Data.Value.thinEdgesMaterial.SetColor("_Color", thinEdgesColor);

            hexasphereRender_Data.Value.thickEdgesMaterial.SetFloat("_GradientIntensity", 1f - hexasphereRender_Data.Value.gradientIntensity);
            hexasphereRender_Data.Value.thickEdgesMaterial.SetFloat("_ExtrusionMultiplier", hexasphereRender_Data.Value.ExtrudeMultiplier);
            Color thickEdgesColor = hexasphereRender_Data.Value.thickEdgesColor;
            thickEdgesColor.r *= hexasphereRender_Data.Value.thickEdgesColorIntensity;
            thickEdgesColor.g *= hexasphereRender_Data.Value.thickEdgesColorIntensity;
            thickEdgesColor.b *= hexasphereRender_Data.Value.thickEdgesColorIntensity;
            hexasphereRender_Data.Value.thickEdgesMaterial.SetColor("_Color", thickEdgesColor);

            //Обновляем размер коллайдера
            hexasphereRender_Data.Value.HexasphereCollider.radius = 0.5f * (1.0f + hexasphereRender_Data.Value.ExtrudeMultiplier);

            //Обновляем свет
            HS_UpdateLightingMode();

            //Обновляем скос
            HS_UpdateBevel();
        }

        void PR_UpdateThinEdges()
        {
            //Проверяем, какие грани требуется отобразить
            //Для каждой отображаемой провинции
            foreach(int provinceEntity in pHSR_F.Value)
            {
                //Берём провинцию
                ref C_ProvinceCore pC = ref pC_P.Value.Get(provinceEntity);
                ref C_ProvinceRender pR = ref pR_P.Value.Get(provinceEntity);
                ref C_ProvinceHexasphere pHS = ref pHS_P.Value.Get(provinceEntity);
                ref C_ProvinceHexasphereRender pHSR = ref pHSR_P.Value.Get(provinceEntity);

                //Обновляем маску граней
                pHSR.thinEdges = 63;

                //Для каждой вершины
                for (int a = 0; a < pHS.vertices.Length; a++)
                {
                    //Берём текущую вершину и следующую
                    Vector3 p0 = pHS.vertices[a];
                    Vector3 p1 = a < pHS.vertices.Length - 1 ? pHS.vertices[a + 1] : pHS.vertices[0];

                    //Для каждого соседа
                    for(int b = 0; b < pC.neighbourProvinceEntities.Length; b++)
                    {
                        //Берём соседа
                        ref C_ProvinceCore neighbourPC = ref pC_P.Value.Get(pC.neighbourProvinceEntities[b]);
                        ref C_ProvinceRender neighbourPR = ref pR_P.Value.Get(pC.neighbourProvinceEntities[b]);
                        ref C_ProvinceHexasphere neighbourPHS = ref pHS_P.Value.Get(pC.neighbourProvinceEntities[b]);

                        //Если индексы граней совпадают
                        if(pR.ThinEdgesIndex == neighbourPR.ThinEdgesIndex)
                        {
                            //Для каждой вершины соседа
                            for (int c = 0; c < neighbourPHS.vertices.Length; c++)
                            {
                                //Берём текущую вершину и следующую
                                Vector3 q0 = neighbourPHS.vertices[c];
                                Vector3 q1 = c < neighbourPHS.vertices.Length - 1 ? neighbourPHS.vertices[c + 1] : neighbourPHS.vertices[0];

                                //Если вершины совпадают
                                if (p0 == q0 && p1 == q1 || p0 == q1 && p1 == q0)
                                {
                                    //Обновляем маску граней
                                    pHSR.thinEdges &= 63 - (1 << a);

                                    //Выходим из цикла вплоть до вершины
                                    b = 9999;
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            //Строим меши граней

            //Определяем индекс чанка и создаём счётчики провинций и вершин
            int chunkIndex = 0;
            int provinceCount = 0;
            int verticesCount = 0;

            //Обновляем списки граней
            List<Vector3> chunkVertices = HexasphereRender_Data.List_Check<Vector3>(ref hexasphereRender_Data.Value.thinEdgesChunkVertices[chunkIndex]);
            List<int> chunkIndices = HexasphereRender_Data.List_Check<int>(ref hexasphereRender_Data.Value.thinEdgesChunkIndices[chunkIndex]);
            List<Vector2> chunkUVs = HexasphereRender_Data.List_Check<Vector2>(ref hexasphereRender_Data.Value.thinEdgesChunkUVs[chunkIndex]);
            List<Color32> chunkColors = HexasphereRender_Data.List_Check<Color32>(ref hexasphereRender_Data.Value.thinEdgesChunkColors[chunkIndex]);

            //Для каждой отображаемой провинции
            foreach (int provinceEntity in pHSR_F.Value)
            {
                //Берём провинцию
                ref C_ProvinceRender pR = ref pR_P.Value.Get(provinceEntity);
                ref C_ProvinceHexasphere pHS = ref pHS_P.Value.Get(provinceEntity);
                ref C_ProvinceHexasphereRender pHSR = ref pHSR_P.Value.Get(provinceEntity);

                //Если число вершин больше максимального в списке
                if(verticesCount > HexasphereRender_Data.maxVertexArraySize)
                {
                    //Увеличиваем индекс чанка
                    chunkIndex++;

                    //Берём списки нового чанка
                    chunkVertices = HexasphereRender_Data.List_Check<Vector3>(ref hexasphereRender_Data.Value.thinEdgesChunkVertices[chunkIndex]);
                    chunkIndices = HexasphereRender_Data.List_Check<int>(ref hexasphereRender_Data.Value.thinEdgesChunkIndices[chunkIndex]);
                    chunkUVs = HexasphereRender_Data.List_Check<Vector2>(ref hexasphereRender_Data.Value.thinEdgesChunkUVs[chunkIndex]);
                    chunkColors = HexasphereRender_Data.List_Check<Color32>(ref hexasphereRender_Data.Value.thinEdgesChunkColors[chunkIndex]);

                    //Обнуляем счётчик вершин
                    verticesCount = 0;
                }

                //Создаём структуру для UV-координат
                Vector2 uVExtruded = new(provinceCount, pR.ProvinceHeight);

                //Определяем положение данных провинции в чанках
                pHSR.parentThinEdgesChunkIndex = chunkIndex;
                pHSR.parentThinEdgesChunkStart = verticesCount;
                pHSR.parentThinEdgesChunkLength = 0;

                //Определяем цвет провинции
                Color32 provinceColor = Color.white;

                //Сохраняем последнюю вершину в списке
                int vertex0 = verticesCount;

                //Создаём переменные для отслеживания
                bool vertexRequired = false;
                bool vertex0Missing = true;

                //Для каждой вершины провинции
                for(int a = 0; a < pHS.vertexPoints.Length; a++)
                {
                    //Определяем, видима ли грань
                    bool segmentVisible = (pHSR.thinEdges & (1 << a)) != 0;

                    //Если грань видима или вершина необходима
                    if (segmentVisible || vertexRequired)
                    {
                        //Заносим вершину в списки
                        chunkVertices.Add(pHS.vertexPoints[a].ProjectedVector3);
                        chunkUVs.Add(uVExtruded);
                        chunkColors.Add(provinceColor);

                        //Если вершина необходима
                        if (vertexRequired == true)
                        {
                            //Закрываем предыдущий сегмент
                            chunkIndices.Add(verticesCount);
                        }

                        //Если грань видима
                        if (segmentVisible == true)
                        {
                            //Начинаем новый сегмент
                            chunkIndices.Add(verticesCount);

                            //Если это первая вершина
                            if(a == 0)
                            {
                                //Указываем, что нулевая вершина не отсутствует
                                vertex0Missing = false;
                            }
                        }

                        //Увеличиваем счётчик вершин
                        verticesCount++;

                        //Увеличиваем длину данных провинции в чанке
                        pHSR.parentThinEdgesChunkLength++;
                    }

                    //Заменяем необходимость вершины на видимость грани
                    vertexRequired = segmentVisible;
                }

                //Если вершина необходима
                if(vertexRequired == true)
                {
                    //Если нулевая вершина отсутствует
                    if (vertex0Missing == true)
                    {
                        //Заносим первую вершину провинции в списки
                        chunkVertices.Add(pHS.vertexPoints[0].ProjectedVector3);
                        chunkUVs.Add(uVExtruded);
                        chunkColors.Add(provinceColor);
                        chunkIndices.Add(verticesCount);

                        //Увеличиваем счётчик вершин
                        verticesCount++;
                    }
                    //Иначе
                    else
                    {
                        //Заносим сохранённую нулевую вершину в список
                        chunkIndices.Add(vertex0);
                    }
                }
            }

            //Удаляем родительский объект граней, если он не пуст
            if (hexasphereRender_Data.Value.thinEdgesRootGO != null)
            {
                Object.DestroyImmediate(hexasphereRender_Data.Value.thinEdgesRootGO);
            }

            //Создаём родительский GO для граней и сохраняем его
            GameObject edgesRootGO = Map_CreateGOAndParent(
                hexasphereRender_Data.Value.HexasphereGO.transform,
                hexasphereRender_Data.Value.ThinEdgesRootGOName);
            hexasphereRender_Data.Value.thinEdgesRootGO = edgesRootGO;

            //Создаём списки мешфильтров, мешей и мешрендереров для граней
            List<MeshFilter> chunksMeshFilters = new();
            List<Mesh> chunksMeshes = new();
            List<MeshRenderer> chunksMeshRenderers = new();

            //Для каждого чанка
            for (int a = 0; a <= chunkIndex; a++)
            {
                //Создаём GO чанка граней
                GameObject chunkGO = Map_CreateGOAndParent(
                    edgesRootGO.transform,
                    hexasphereRender_Data.Value.ThinEdgeChunkGOName);

                //Назначаем чанку компонент мешфильтра и заносим его в список
                MeshFilter meshFilter = chunkGO.AddComponent<MeshFilter>();
                chunksMeshFilters.Add(meshFilter);

                //Создаём меш и заносим его в список
                Mesh mesh = new();
                chunksMeshes.Add(mesh);

                //Заполняем меш вершинами, UV, цветами и индексами
                mesh.SetVertices(hexasphereRender_Data.Value.thinEdgesChunkVertices[a]);
                mesh.SetUVs(0, hexasphereRender_Data.Value.thinEdgesChunkUVs[a]);
                mesh.SetColors(hexasphereRender_Data.Value.thinEdgesChunkColors[a]);
                mesh.SetIndices(hexasphereRender_Data.Value.thinEdgesChunkIndices[a], MeshTopology.Lines, 0, false);

                //Назначаем меш мешфильтру
                meshFilter.sharedMesh = mesh;

                //Назначаем чанку компонент мешрендерера и заносим его в список
                MeshRenderer meshRenderer = chunkGO.AddComponent<MeshRenderer>();
                chunksMeshRenderers.Add(meshRenderer);

                //Устанавливаем параметры мешрендерера
                meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                meshRenderer.sharedMaterial = hexasphereRender_Data.Value.thinEdgesMaterial;
            }

            //Сохраняем списки мешей как массивы
            hexasphereRender_Data.Value.thinEdgesChunkMeshFilters = chunksMeshFilters.ToArray();
            hexasphereRender_Data.Value.thinEdgesChunkMeshes = chunksMeshes.ToArray();
            hexasphereRender_Data.Value.thinEdgesChunkMeshRenderers = chunksMeshRenderers.ToArray();
        }

        void PR_UpdateThickEdges()
        {
            //Проверяем, какие грани требуется отобразить
            //Для каждой отображаемой провинции
            foreach (int provinceEntity in pHSR_F.Value)
            {
                //Берём провинцию
                ref C_ProvinceCore pC = ref pC_P.Value.Get(provinceEntity);
                ref C_ProvinceRender pR = ref pR_P.Value.Get(provinceEntity);
                ref C_ProvinceHexasphere pHS = ref pHS_P.Value.Get(provinceEntity);
                ref C_ProvinceHexasphereRender pHSR = ref pHSR_P.Value.Get(provinceEntity);

                //Обновляем маску граней
                pHSR.thickEdges = 63;

                //Для каждой вершины
                for (int a = 0; a < pHS.vertices.Length; a++)
                {
                    //Берём текущую вершину и следующую
                    Vector3 p0 = pHS.vertices[a];
                    Vector3 p1 = a < pHS.vertices.Length - 1 ? pHS.vertices[a + 1] : pHS.vertices[0];

                    //Для каждого соседа
                    for (int b = 0; b < pC.neighbourProvinceEntities.Length; b++)
                    {
                        //Берём соседа
                        ref C_ProvinceCore neighbourPC = ref pC_P.Value.Get(pC.neighbourProvinceEntities[b]);
                        ref C_ProvinceRender neighbourPR = ref pR_P.Value.Get(pC.neighbourProvinceEntities[b]);
                        ref C_ProvinceHexasphere neighbourPHS = ref pHS_P.Value.Get(pC.neighbourProvinceEntities[b]);

                        //Если индексы граней совпадают
                        if (pR.ThickEdgesIndex == neighbourPR.ThickEdgesIndex)
                        {
                            //Для каждой вершины соседа
                            for (int c = 0; c < neighbourPHS.vertices.Length; c++)
                            {
                                //Берём текущую вершину и следующую
                                Vector3 q0 = neighbourPHS.vertices[c];
                                Vector3 q1 = c < neighbourPHS.vertices.Length - 1 ? neighbourPHS.vertices[c + 1] : neighbourPHS.vertices[0];

                                //Если вершины совпадают
                                if (p0 == q0 && p1 == q1 || p0 == q1 && p1 == q0)
                                {
                                    //Обновляем маску граней
                                    pHSR.thickEdges &= 63 - (1 << a);

                                    //Выходим из цикла вплоть до вершины
                                    b = 9999;
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            //Строим меши граней

            //Определяем индекс чанка и создаём счётчики провинций и вершин
            int chunkIndex = 0;
            int provinceCount = 0;
            int verticesCount = 0;

            //Обновляем списки граней
            List<Vector3> chunkVertices = HexasphereRender_Data.List_Check<Vector3>(ref hexasphereRender_Data.Value.thickEdgesChunkVertices[chunkIndex]);
            List<int> chunkIndices = HexasphereRender_Data.List_Check<int>(ref hexasphereRender_Data.Value.thickEdgesChunkIndices[chunkIndex]);
            List<Vector2> chunkUVs = HexasphereRender_Data.List_Check<Vector2>(ref hexasphereRender_Data.Value.thickEdgesChunkUVs[chunkIndex]);
            List<Color32> chunkColors = HexasphereRender_Data.List_Check<Color32>(ref hexasphereRender_Data.Value.thickEdgesChunkColors[chunkIndex]);

            //Для каждой отображаемой провинции
            foreach (int provinceEntity in pHSR_F.Value)
            {
                //Берём провинцию
                ref C_ProvinceRender pR = ref pR_P.Value.Get(provinceEntity);
                ref C_ProvinceHexasphere pHS = ref pHS_P.Value.Get(provinceEntity);
                ref C_ProvinceHexasphereRender pHSR = ref pHSR_P.Value.Get(provinceEntity);

                //Если число вершин больше максимального в списке
                if (verticesCount > HexasphereRender_Data.maxVertexArraySize)
                {
                    //Увеличиваем индекс чанка
                    chunkIndex++;

                    //Берём списки нового чанка
                    chunkVertices = HexasphereRender_Data.List_Check<Vector3>(ref hexasphereRender_Data.Value.thickEdgesChunkVertices[chunkIndex]);
                    chunkIndices = HexasphereRender_Data.List_Check<int>(ref hexasphereRender_Data.Value.thickEdgesChunkIndices[chunkIndex]);
                    chunkUVs = HexasphereRender_Data.List_Check<Vector2>(ref hexasphereRender_Data.Value.thickEdgesChunkUVs[chunkIndex]);
                    chunkColors = HexasphereRender_Data.List_Check<Color32>(ref hexasphereRender_Data.Value.thickEdgesChunkColors[chunkIndex]);

                    //Обнуляем счётчик вершин
                    verticesCount = 0;
                }

                //Создаём структуру для UV-координат
                Vector2 uVExtruded = new(provinceCount, pR.ProvinceHeight);

                //Определяем положение данных провинции в чанках
                pHSR.parentThickEdgesChunkIndex = chunkIndex;
                pHSR.parentThickEdgesChunkStart = verticesCount;
                pHSR.parentThickEdgesChunkLength = 0;

                //Определяем цвет провинции
                Color32 provinceColor = Color.white;

                //Сохраняем последнюю вершину в списке
                int vertex0 = verticesCount;

                //Создаём переменные для отслеживания
                bool vertexRequired = false;
                bool vertex0Missing = true;

                //Для каждой вершины провинции
                for (int a = 0; a < pHS.vertexPoints.Length; a++)
                {
                    //Определяем, видима ли грань
                    bool segmentVisible = (pHSR.thickEdges & (1 << a)) != 0;

                    //Если грань видима или вершина необходима
                    if (segmentVisible || vertexRequired)
                    {
                        //Заносим вершину в списки
                        chunkVertices.Add(pHS.vertexPoints[a].ProjectedVector3);
                        chunkUVs.Add(uVExtruded);
                        chunkColors.Add(provinceColor);

                        //Если вершина необходима
                        if (vertexRequired == true)
                        {
                            //Закрываем предыдущий сегмент
                            chunkIndices.Add(verticesCount);
                        }

                        //Если грань видима
                        if (segmentVisible == true)
                        {
                            //Начинаем новый сегмент
                            chunkIndices.Add(verticesCount);

                            //Если это первая вершина
                            if (a == 0)
                            {
                                //Указываем, что нулевая вершина не отсутствует
                                vertex0Missing = false;
                            }
                        }

                        //Увеличиваем счётчик вершин
                        verticesCount++;

                        //Увеличиваем длину данных провинции в чанке
                        pHSR.parentThickEdgesChunkLength++;
                    }

                    //Заменяем необходимость вершины на видимость грани
                    vertexRequired = segmentVisible;
                }

                //Если вершина необходима
                if (vertexRequired == true)
                {
                    //Если нулевая вершина отсутствует
                    if (vertex0Missing == true)
                    {
                        //Заносим первую вершину провинции в списки
                        chunkVertices.Add(pHS.vertexPoints[0].ProjectedVector3);
                        chunkUVs.Add(uVExtruded);
                        chunkColors.Add(provinceColor);
                        chunkIndices.Add(verticesCount);

                        //Увеличиваем счётчик вершин
                        verticesCount++;
                    }
                    //Иначе
                    else
                    {
                        //Заносим сохранённую нулевую вершину в список
                        chunkIndices.Add(vertex0);
                    }
                }
            }

            //Удаляем родительский объект граней, если он не пуст
            if (hexasphereRender_Data.Value.thickEdgesRootGO != null)
            {
                Object.DestroyImmediate(hexasphereRender_Data.Value.thickEdgesRootGO);
            }

            //Создаём родительский GO для граней и сохраняем его
            GameObject edgesRootGO = Map_CreateGOAndParent(
                hexasphereRender_Data.Value.HexasphereGO.transform,
                hexasphereRender_Data.Value.ThickEdgesRootGOName);
            hexasphereRender_Data.Value.thickEdgesRootGO = edgesRootGO;

            //Создаём списки мешфильтров, мешей и мешрендереров для граней
            List<MeshFilter> chunksMeshFilters = new();
            List<Mesh> chunksMeshes = new();
            List<MeshRenderer> chunksMeshRenderers = new();

            //Для каждого чанка
            for (int a = 0; a <= chunkIndex; a++)
            {
                //Создаём GO чанка граней
                GameObject chunkGO = Map_CreateGOAndParent(
                    edgesRootGO.transform,
                    hexasphereRender_Data.Value.ThickEdgeChunkGOName);

                //Назначаем чанку компонент мешфильтра и заносим его в список
                MeshFilter meshFilter = chunkGO.AddComponent<MeshFilter>();
                chunksMeshFilters.Add(meshFilter);

                //Создаём меш и заносим его в список
                Mesh mesh = new();
                chunksMeshes.Add(mesh);

                //Заполняем меш вершинами, UV, цветами и индексами
                mesh.SetVertices(hexasphereRender_Data.Value.thickEdgesChunkVertices[a]);
                mesh.SetUVs(0, hexasphereRender_Data.Value.thickEdgesChunkUVs[a]);
                mesh.SetColors(hexasphereRender_Data.Value.thickEdgesChunkColors[a]);
                mesh.SetIndices(hexasphereRender_Data.Value.thickEdgesChunkIndices[a], MeshTopology.Lines, 0, false);

                //Назначаем меш мешфильтру
                meshFilter.sharedMesh = mesh;

                //Назначаем чанку компонент мешрендерера и заносим его в список
                MeshRenderer meshRenderer = chunkGO.AddComponent<MeshRenderer>();
                chunksMeshRenderers.Add(meshRenderer);

                //Устанавливаем параметры мешрендерера
                meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                meshRenderer.sharedMaterial = hexasphereRender_Data.Value.thickEdgesMaterial;
            }

            //Сохраняем списки мешей как массивы
            hexasphereRender_Data.Value.thickEdgesChunkMeshFilters = chunksMeshFilters.ToArray();
            hexasphereRender_Data.Value.thickEdgesChunkMeshes = chunksMeshes.ToArray();
            hexasphereRender_Data.Value.thickEdgesChunkMeshRenderers = chunksMeshRenderers.ToArray();
        }

        void PR_UpdateHeights()
        {
            //Для каждой отображаемой провинции
            foreach (int provinceEntity in pHSR_F.Value)
            {
                //Берём компоненты провинции
                ref C_ProvinceRender pR = ref pR_P.Value.Get(provinceEntity);
                ref C_ProvinceHexasphere pHS = ref pHS_P.Value.Get(provinceEntity);
                ref C_ProvinceHexasphereRender pHSR = ref pHSR_P.Value.Get(provinceEntity);

                #region ProvinceMesh
                //Берём массив чанка, в котором располагается провинция
                ref Vector4[] chunkUV = ref hexasphereRender_Data.Value.chunksUV[pHSR.parentChunkIndex];

                //Определяем, какой массив UV использует провинция
                Vector2[] uVArray;

                //Если провинция - гексагон
                if (pHS.vertexPoints.Length == 6)
                {
                    //Назначаем провинции массив UV гексагона
                    uVArray = hexasphereRender_Data.Value.hexagonUVsExtruded;
                }
                //Иначе это пентагон
                else
                {
                    //Назначаем провинции массив UV пентагона
                    uVArray = hexasphereRender_Data.Value.pentagonUVsExtruded;
                }

                //Для каждых UV2-координат в списке
                for (int a = 0; a < uVArray.Length; a++)
                {
                    //Собираем UV4-координаты
                    Vector4 uV4;
                    uV4.x = uVArray[a].x;
                    uV4.y = uVArray[a].y;
                    uV4.z = 0;
                    uV4.w = pR.ProvinceHeight;

                    //Заносим координаты в массив координат
                    chunkUV[pHSR.parentChunkStart + a] = uV4;
                }
                #endregion

                #region EdgesMesh
                //Берём список чанка тонких граней, в котором располагается провинция
                List<Vector2> thinEdgesChunkUVs = hexasphereRender_Data.Value.thinEdgesChunkUVs[pHSR.parentThinEdgesChunkIndex];

                //Для каждой вершины провинции в чанке тонких граней
                for(int a = 0; a < pHSR.parentThinEdgesChunkLength; a++)
                {
                    //Берём UV2-координаты вершины
                    Vector2 uv = thinEdgesChunkUVs[pHSR.parentThinEdgesChunkStart + a];

                    //Обновляем высоту провинции
                    uv.y = pR.ProvinceHeight;

                    //Заносим координаты в список
                    thinEdgesChunkUVs[pHSR.parentThinEdgesChunkStart + a] = uv;
                }

                //Берём список чанка толстых граней, в котором располагается провинция
                List<Vector2> thickEdgesChunkUVs = hexasphereRender_Data.Value.thickEdgesChunkUVs[pHSR.parentThickEdgesChunkIndex];

                //Для каждой вершины провинции в чанке тонких граней
                for (int a = 0; a < pHSR.parentThickEdgesChunkLength; a++)
                {
                    //Берём UV2-координаты вершины
                    Vector2 uv = thickEdgesChunkUVs[pHSR.parentThickEdgesChunkStart + a];

                    //Обновляем высоту провинции
                    uv.y = pR.ProvinceHeight;

                    //Заносим координаты в список
                    thickEdgesChunkUVs[pHSR.parentThickEdgesChunkStart + a] = uv;
                }
                #endregion
            }

            //Для каждого мешфильтра чанка
            for (int a = 0; a < hexasphereRender_Data.Value.chunkMeshFilters.Length; a++)
            {
                //Заполняем меш UV
                hexasphereRender_Data.Value.chunkMeshes[a].SetUVs(0, hexasphereRender_Data.Value.chunksUV[a]);

                hexasphereRender_Data.Value.chunkMeshFilters[a].sharedMesh = hexasphereRender_Data.Value.chunkMeshes[a];

                //Назначаем мешрендереру материал
                hexasphereRender_Data.Value.chunkMeshRenderers[a].sharedMaterial = hexasphereRender_Data.Value.provinceMaterial;
            }

            //Для каждого мешфильтра тонких граней
            for(int a = 0;a < hexasphereRender_Data.Value.thinEdgesChunkMeshFilters.Length; a++)
            {
                //Заполняем меш UV
                hexasphereRender_Data.Value.thinEdgesChunkMeshes[a].SetUVs(0, hexasphereRender_Data.Value.thinEdgesChunkUVs[a]);

                hexasphereRender_Data.Value.thinEdgesChunkMeshFilters[a].sharedMesh = hexasphereRender_Data.Value.thinEdgesChunkMeshes[a];

                //Назначаем мешрендереру материал
                hexasphereRender_Data.Value.thinEdgesChunkMeshRenderers[a].sharedMaterial = hexasphereRender_Data.Value.thinEdgesMaterial;
            }

            //Для каждого мешфильтра толстых граней
            for (int a = 0; a < hexasphereRender_Data.Value.thickEdgesChunkMeshFilters.Length; a++)
            {
                //Заполняем меш UV
                hexasphereRender_Data.Value.thickEdgesChunkMeshes[a].SetUVs(0, hexasphereRender_Data.Value.thickEdgesChunkUVs[a]);

                hexasphereRender_Data.Value.thickEdgesChunkMeshFilters[a].sharedMesh = hexasphereRender_Data.Value.thickEdgesChunkMeshes[a];

                //Назначаем мешрендереру материал
                hexasphereRender_Data.Value.thickEdgesChunkMeshRenderers[a].sharedMaterial = hexasphereRender_Data.Value.thickEdgesMaterial;
            }
        }

        void PR_UpdateColors(
            ref C_MapModeCore mMC)
        {
            //Для каждой отображаемой провинции
            foreach (int provinceEntity in pHSR_F.Value)
            {
                //Берём компоненты провинции
                ref C_ProvinceRender pR = ref pR_P.Value.Get(provinceEntity);
                ref C_ProvinceHexasphere pHS = ref pHS_P.Value.Get(provinceEntity);
                ref C_ProvinceHexasphereRender pHSR = ref pHSR_P.Value.Get(provinceEntity);

                //Берём массивы чанка, в котором располагается провинция
                ref Color32[] chunkColors = ref hexasphereRender_Data.Value.chunksColors[pHSR.parentChunkIndex];

                //Определяем количество вершин провинции
                int provinceVertexCount = pHS.vertexPoints.Length + 2;

                //Для каждых UV2-координат в списке
                for (int a = 0; a < provinceVertexCount; a++)
                {
                    //Заносим цвет в массив цветов
                    chunkColors[pHSR.parentChunkStart + a] = mMC.GetProvinceColor(ref pR);
                }
            }

            //Для каждого мешфильтра чанка
            for (int a = 0; a < hexasphereRender_Data.Value.chunkMeshFilters.Length; a++)
            {
                //Заполняем меш цветами
                hexasphereRender_Data.Value.chunkMeshes[a].SetColors(hexasphereRender_Data.Value.chunksColors[a]);

                hexasphereRender_Data.Value.chunkMeshFilters[a].sharedMesh = hexasphereRender_Data.Value.chunkMeshes[a];

                //Назначаем мешрендереру материал
                hexasphereRender_Data.Value.chunkMeshRenderers[a].sharedMaterial = hexasphereRender_Data.Value.provinceMaterial;
            }
        }

        GameObject Map_CreateGOAndParent(
            Transform parent,
            string name)
        {
            //Создаём GO
            GameObject gO = new GameObject(name);

            //Заполняем основные данные GO
            gO.layer = parent.gameObject.layer;
            gO.transform.SetParent(parent, false);
            gO.transform.localPosition = Vector3.zero;
            gO.transform.localScale = Vector3.one;
            gO.transform.localRotation = Quaternion.Euler(0, 0, 0);

            return gO;
        }

        void MeshRenderers_UpdateShadowSupport() 
        {
            //Для каждого мешрендерера провинции
            for (int a = 0; a < hexasphereRender_Data.Value.chunkMeshRenderers.Length; a++)
            {
                //Если рендерер не пуст и его имя верно
                if (hexasphereRender_Data.Value.chunkMeshRenderers[a] != null
                    && hexasphereRender_Data.Value.chunkMeshRenderers[a].name.Equals(hexasphereRender_Data.Value.ChunkGOName))
                {
                    //Настраиваем отбрасывание принятие и отбрасывание теней
                    hexasphereRender_Data.Value.chunkMeshRenderers[a].receiveShadows = true;
                    hexasphereRender_Data.Value.chunkMeshRenderers[a].shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
                }
            }
        }

        void HS_UpdateLightingMode()
        {
            //Обновляем материалы
            HS_UpdateLightingMaterial(hexasphereRender_Data.Value.provinceMaterial);
            HS_UpdateLightingMaterial(hexasphereRender_Data.Value.provinceColoredMaterial);

            hexasphereRender_Data.Value.provinceMaterial.EnableKeyword("HEXA_ALPHA");
        }

        void HS_UpdateLightingMaterial(
            Material material)
        {
            //Заполняем данные материала
            material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
            material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
            material.SetInt("_ZWrite", 1);
            if (material.renderQueue >= 3000)
            {
                material.renderQueue -= 2000;
            }
            material.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Back);

            material.EnableKeyword("HEXA_LIT");
        }

        void HS_UpdateBevel()
        {
            //Определяем размер текстуры
            const int textureSize = 256;

            //Если текстура отсутствует или её ширина неверна
            if (hexasphereRender_Data.Value.bevelNormals == null || hexasphereRender_Data.Value.bevelNormals.width != textureSize)
            {
                //Создаём новую текстуру
                hexasphereRender_Data.Value.bevelNormals = new(
                    textureSize, textureSize,
                    TextureFormat.ARGB32,
                    false);
            }

            //Определяем размер текстуры
            int textureHeight = hexasphereRender_Data.Value.bevelNormals.height;
            int textureWidth = hexasphereRender_Data.Value.bevelNormals.width;

            //Если массив цветов текстуры отсутствует или его длина не соответствует текстуре
            if (hexasphereRender_Data.Value.bevelNormalsColors == null || hexasphereRender_Data.Value.bevelNormalsColors.Length != textureHeight * textureWidth)
            {
                //Создаём новый массив
                hexasphereRender_Data.Value.bevelNormalsColors = new Color[textureHeight * textureWidth];
            }

            //Создаём структуру для координат пикселя
            Vector2 texturePixel;

            //Определяем ширину скоса и квадрат ширины
            const float bevelWidth = 0.05f;
            float bevelWidthSqr = bevelWidth * bevelWidth;

            //Для каждого пикселя по высоте
            for (int y = 0, index = 0; y < textureHeight; y++)
            {
                //Определяем его положение по Y
                texturePixel.y = (float)y / textureHeight;

                //Для каждого пикселя по ширине
                for (int x = 0; x < textureWidth; x++)
                {
                    //Определяем его положение по X
                    texturePixel.x = (float)x / textureWidth;

                    //Обнуляем R-компонент
                    hexasphereRender_Data.Value.bevelNormalsColors[index].r = 0f;

                    //Определяем расстояние до данного пикселя
                    float minDistSqr = float.MaxValue;

                    //Для каждого ребра шестиугольника
                    for (int a = 0; a < 6; a++)
                    {
                        //Берём индексы вершин
                        Vector2 t0 = hexasphereRender_Data.Value.hexagonUVsExtruded[a];
                        Vector2 t1 = a < 5 ? hexasphereRender_Data.Value.hexagonUVsExtruded[a + 1] : hexasphereRender_Data.Value.hexagonUVsExtruded[0];

                        //Определяем длину ребра
                        float l2 = Vector2.SqrMagnitude(t0 - t1);
                        //
                        float t = Mathf.Max(0, Mathf.Min(1, Vector2.Dot(texturePixel - t0, t1 - t0) / l2));
                        //
                        Vector2 projection = t0 + t * (t1 - t0);
                        //
                        float distSqr = Vector2.SqrMagnitude(texturePixel - projection);

                        //Если расстояние меньше минимального, то обновляем минимальное
                        if (distSqr < minDistSqr)
                        {
                            minDistSqr = distSqr;
                        }
                    }

                    //Определяем градиент
                    float f = minDistSqr / bevelWidthSqr;
                    //Если градиент больше единицы, ограничиваем его
                    if (f > 1f)
                    {
                        f = 1f;
                    }

                    //Обновляем R-компонент
                    hexasphereRender_Data.Value.bevelNormalsColors[index].r = f;

                    //Увеличиваем индекс пикселя
                    index++;
                }
            }

            //Задаём массив пикселей текстуре
            hexasphereRender_Data.Value.bevelNormals.SetPixels(hexasphereRender_Data.Value.bevelNormalsColors);

            //Применяем текстуру
            hexasphereRender_Data.Value.bevelNormals.Apply();

            //Задаём текстуру материалу
            hexasphereRender_Data.Value.provinceMaterial.SetTexture("_BumpMask", hexasphereRender_Data.Value.bevelNormals);
        }

        void MaterialHighlight_Update()
        {
            //Обновляем яркость материалов подсветки
            hexasphereRender_Data.Value.hoverProvinceHighlightMaterial.SetFloat(
                ShaderParameters.ColorShift, Mathf.PingPong(Time.time * 0.25f, 1f));
            hexasphereRender_Data.Value.currentProvinceHighlightMaterial.SetFloat(
                ShaderParameters.ColorShift, Mathf.PingPong(Time.time * 0.25f, 1f));
        }

        readonly EcsPoolInject<C_ProvinceHoverHighlight> provinceHoverHighlight_P = default;
        readonly EcsPoolInject<SR_ProvinceHoverHighlight_Show> provinceHoverHighlight_Show_SR_P = default;
        void ProvinceHoverHighlight()
        {
            //Отключаем подсветку, где уже не требуется
            ProvinceHoverHighlight_Deactivation();

            //Обновляем подсветку, если она уже включена там, где требуется
            ProvinceHoverHighlight_Update();

            //Активируем подсветку, где требуется
            ProvinceHoverHighlight_Activation();
        }

        readonly EcsFilterInject<Inc<C_ProvinceHexasphere, C_ProvinceHoverHighlight>, Exc<SR_ProvinceHoverHighlight_Show>> provinceHoverHighlight_Deactivation_F = default;
        void ProvinceHoverHighlight_Deactivation()
        {
            //Для каждой провинции, имеющей подсветку наведения и не имеющей запроса
            foreach (int provinceEntity in provinceHoverHighlight_Deactivation_F.Value)
            {
                //Берём компонент подсветки
                ref C_ProvinceHoverHighlight pHoverHighlight = ref provinceHoverHighlight_P.Value.Get(provinceEntity);

                //Кэшируем подсветку
                GO_ProvinceHighlight.CacheProvinceHighlight(ref pHoverHighlight);

                //Удаляем компонент подсветки
                provinceHoverHighlight_P.Value.Del(provinceEntity);
            }
        }

        readonly EcsFilterInject<Inc<C_ProvinceHexasphere, C_ProvinceHoverHighlight, SR_ProvinceHoverHighlight_Show>> provinceHoverHighlight_Update_F = default;
        void ProvinceHoverHighlight_Update()
        {
            //Для каждой провинции, имеющей подсветку наведения и запрос
            foreach (int provinceEntity in provinceHoverHighlight_Update_F.Value)
            {
                //Берём провинцию 
                ref C_ProvinceRender pR = ref pR_P.Value.Get(provinceEntity);
                ref C_ProvinceHexasphere pHS = ref pHS_P.Value.Get(provinceEntity);
                ref C_ProvinceHoverHighlight pHoverHighlight = ref provinceHoverHighlight_P.Value.Get(provinceEntity);

                //Обновляем подсветку
                GO_ProvinceHighlight.UpdateProvinceHighlight(
                    ref pR,
                    pHoverHighlight.highlight,
                    hexasphereRender_Data.Value.hoverProvinceHighlightMaterial);

                //Удаляем запрос
                provinceHoverHighlight_Show_SR_P.Value.Del(provinceEntity);
            }
        }

        readonly EcsFilterInject<Inc<C_ProvinceHexasphere, SR_ProvinceHoverHighlight_Show>> provinceHoverHighlight_Activation_F = default;
        void ProvinceHoverHighlight_Activation()
        {
            //Для каждой провинции, имеющей запрос
            foreach (int provinceEntity in provinceHoverHighlight_Activation_F.Value)
            {
                //Берём провинцию
                ref C_ProvinceRender pR = ref pR_P.Value.Get(provinceEntity);
                ref C_ProvinceHexasphere pHS = ref pHS_P.Value.Get(provinceEntity);

                //Если провинция не имеет GO
                if (pR.ProvinceGO == null)
                {
                    //Создаём его
                    GO_Province.InstantiateProvinceGO(
                        hexasphereRender_Data.Value.provincesRootGO,
                        ref pR);
                }

                //Назначаем ей компонент подсветки
                ref C_ProvinceHoverHighlight pHoverHighlight = ref provinceHoverHighlight_P.Value.Add(provinceEntity);

                //Создаём подсветку
                GO_ProvinceHighlight.InstantiateProvinceHighlight(
                    ref pR, ref pHoverHighlight,
                    hexasphereRender_Data.Value.hoverProvinceHighlightMaterial);

                //Создаём меш подсветки
                ProvinceHighlight_CreateMesh(
                    ref pR, ref pHS,
                    pHoverHighlight.highlight);

                //Удаляем запрос
                provinceHoverHighlight_Show_SR_P.Value.Del(provinceEntity);
            }
        }

        void ProvinceHighlight_CreateMesh(
            ref C_ProvinceRender pR, ref C_ProvinceHexasphere pHS,
            GO_ProvinceHighlight provinceHighlight)
        {
            //Создаём новый меш
            Mesh mesh = new();
            mesh.hideFlags = HideFlags.DontSave;

            //Определяем высоту подсветки
            float extrusion = pR.ProvinceHeight * hexasphereRender_Data.Value.ExtrudeMultiplier;

            //Рассчитываем положение вершин
            Vector3[] extrudedVertices = new Vector3[pHS.vertices.Length];
            for (int a = 0; a < pHS.vertices.Length; a++)
            {
                extrudedVertices[a] = pHS.vertices[a] * (1f + extrusion);
            }

            //Назначаем вершины мешу
            mesh.SetVertices(extrudedVertices);

            //Если у провинции шесть вершин
            if (pHS.vertices.Length == 6)
            {
                mesh.SetIndices(
                    hexasphereRender_Data.Value.hexagonIndices,
                    MeshTopology.Triangles,
                    0,
                    false);
                mesh.uv = hexasphereRender_Data.Value.hexagonUVs;
            }
            //Иначе
            else
            {
                mesh.SetIndices(
                    hexasphereRender_Data.Value.pentagonIndices,
                    MeshTopology.Triangles,
                    0,
                    false);
                mesh.uv = hexasphereRender_Data.Value.pentagonUVs;
            }

            //Рассчитываем нормали меша
            mesh.SetNormals(pHS.vertices);
            mesh.RecalculateNormals();

            //Назначаем меш подсветке и включаем визуализацию
            provinceHighlight.meshFilter.sharedMesh = mesh;
            provinceHighlight.meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            provinceHighlight.meshRenderer.enabled = true;
        }

        readonly EcsFilterInject<Inc<C_ProvinceRender, C_ProvinceHexasphere, C_ProvinceHoverHighlight>> provinceHoverHighlight_UpdateMesh_F = default;
        void ProvinceHoverHighlight_UpdateMeshes()
        {
            //Для каждой провинции с компонентом подсветки наведения
            foreach (int provinceEntity in provinceHoverHighlight_UpdateMesh_F.Value)
            {
                //Берём провинцию
                ref C_ProvinceRender pR = ref pR_P.Value.Get(provinceEntity);
                ref C_ProvinceHexasphere pHS = ref pHS_P.Value.Get(provinceEntity);
                ref C_ProvinceHoverHighlight pHoverHighlight = ref provinceHoverHighlight_P.Value.Get(provinceEntity);

                //Обновляем меш подсветки
                ProvinceHighlight_UpdateMesh(
                    ref pR, ref pHS,
                    pHoverHighlight.highlight);
            }
        }

        void ProvinceHighlight_UpdateMesh(
            ref C_ProvinceRender pR, ref C_ProvinceHexasphere pHS,
            GO_ProvinceHighlight provinceHighlight)
        {
            //Берём меш подсветки
            Mesh mesh = provinceHighlight.meshFilter.sharedMesh;

            //Определяем высоту подсветки
            float extrusion = pR.ProvinceHeight * hexasphereRender_Data.Value.ExtrudeMultiplier;

            //Рассчитываем новое положение вершин
            Vector3[] extrudedVertices = new Vector3[pHS.vertices.Length];
            for (int a = 0; a < pHS.vertices.Length; a++)
            {
                extrudedVertices[a] = pHS.vertices[a] * (1f + extrusion);
            }

            //Назначаем вершины мешу
            mesh.SetVertices(extrudedVertices);

            //Рассчитываем нормали меша
            mesh.SetNormals(pHS.vertices);
            mesh.RecalculateNormals();

            //Обновляем меш мешфильтра
            provinceHighlight.meshFilter.sharedMesh = null;
            provinceHighlight.meshFilter.sharedMesh = mesh;
        }

        void ProvinceMapPanels()
        {
            //Изменяем родительские объекты панелей карты
            PMP_SetParent();

            //Проверяем, нет ли провинций без панелей карты, но с компонентом
            PMP_CheckEmpty();

            //Поворачиваем панели карты
            PMP_Update();
        }

        readonly EcsFilterInject<Inc<R_ProvinceMapPanel_SetParent>> pMP_SetParent_R_F = default;
        readonly EcsPoolInject<R_ProvinceMapPanel_SetParent> pMP_SetParent_R_P = default;
        void PMP_SetParent()
        {
            //Для каждого запроса изменения родителя панели карты
            foreach (int requestEntity in pMP_SetParent_R_F.Value)
            {
                //Берём запрос
                ref R_ProvinceMapPanel_SetParent requestComp = ref pMP_SetParent_R_P.Value.Get(requestEntity);

                //Изменяем родителя панели карты
                PMP_SetParent(ref requestComp);

                //Удаляем запрос
                pMP_SetParent_R_P.Value.Del(requestEntity);
            }
        }

        void PMP_SetParent(
            ref R_ProvinceMapPanel_SetParent requestComp)
        {
            //Берём целевую провинцию
            requestComp.parentProvincePE.Unpack(world.Value, out int provinceEntity);
            ref C_ProvinceRender pR = ref pR_P.Value.Get(provinceEntity);
            ref C_ProvinceHexasphere pHS = ref pHS_P.Value.Get(provinceEntity);

            //Если провинция не имеет GO
            if (pR.ProvinceGO == null)
            {
                //Создаём его
                GO_Province.InstantiateProvinceGO(
                    hexasphereRender_Data.Value.provincesRootGO,
                    ref pR);
            }

            //Если провинция не имеет компонента панелей карты
            if(pMP_P.Value.Has(provinceEntity) == false)
            {
                //Назначаем компонент
                PMP_Create(
                    provinceEntity,
                    ref pR, ref pHS);
            }

            //Берём компонент панелей карты
            ref C_ProvinceMapPanels pMP = ref pMP_P.Value.Get(provinceEntity);

            //Присоединяем переданную панель к группе панелей провинции
            requestComp.mapPanelGO.transform.SetParent(pMP.mapPanelGroup.transform);
            requestComp.mapPanelGO.transform.localPosition = Vector3.zero;
        }

        void PMP_Create(
            int provinceEntity,
            ref C_ProvinceRender pR, ref C_ProvinceHexasphere pHS)
        {
            //Назначаем сущности провинции компонент панелей карты и заполняем его данные
            ref C_ProvinceMapPanels pMP = ref pMP_P.Value.Add(provinceEntity);
            pMP = new(0);

            //Определяем центр провинции
            Vector3 provinceCenter = pHS.center;
            provinceCenter *= 1.0f + pR.ProvinceHeight * hexasphereRender_Data.Value.ExtrudeMultiplier;
            provinceCenter *= hexasphereRender_Data.Value.HexasphereScale;

            //Создаём группу панелей карты
            C_ProvinceMapPanels.InstantiateMapPanelGroup(
                ref pR, ref pMP,
                provinceCenter,
                mapRender_Data.Value.MapPanelAltitude);
        }

        readonly EcsFilterInject<Inc<C_ProvinceRender, C_ProvinceMapPanels>> pMP_F = default;
        void PMP_CheckEmpty()
        {
            //Для каждой провинции с компонентом панелей карты
            foreach(int provinceEntity in pMP_F.Value)
            {
                //Берём компонент панелей карты
                ref C_ProvinceMapPanels pMP = ref pMP_P.Value.Get(provinceEntity);

                //Если провинция не имеет панелей карты
                if(pMP.mapPanelGroup.transform.childCount == 0)
                {
                    //Кэшируем группу панелей
                    C_ProvinceMapPanels.CacheMapPanelGroup(ref pMP);

                    //Удаляем с сущности компонент панелей карты
                    pMP_P.Value.Del(provinceEntity);
                }
            }
        }

        void PMP_UpdateAltitude()
        {
            //Для каждой провинции с компонентом панелей карты
            foreach (int provinceEntity in pMP_F.Value)
            {
                //Берём компоненты провинции
                ref C_ProvinceRender pR = ref pR_P.Value.Get(provinceEntity);
                ref C_ProvinceHexasphere pHS = ref pHS_P.Value.Get(provinceEntity);
                ref C_ProvinceMapPanels pMP = ref pMP_P.Value.Get(provinceEntity);

                //Определяем положение центра провинции
                Vector3 provinceCenter = pHS.center;
                provinceCenter *= 1.0f + pR.ProvinceHeight * hexasphereRender_Data.Value.ExtrudeMultiplier;
                provinceCenter *= hexasphereRender_Data.Value.HexasphereScale;

                //Задаём положение группы панелей
                C_ProvinceMapPanels.CalculateMapPanelGroupPosition(
                    ref pMP,
                    provinceCenter,
                    mapRender_Data.Value.MapPanelAltitude);
            }
        }

        void PMP_Update()
        {
            //Для каждой провинции с компонентом панелей карты
            foreach (int provinceEntity in pMP_F.Value)
            {
                //Берём компонент панелей карты
                ref C_ProvinceMapPanels pMP = ref pMP_P.Value.Get(provinceEntity);

                float d = Vector3.Dot(
                    Camera.main.transform.position.normalized,
                    pMP.mapPanelGroup.transform.position.normalized);

                pMP.mapPanelGroup.transform.LookAt(Vector3.zero, Vector3.up);
                d = Mathf.Clamp01(d);
                pMP.mapPanelGroup.transform.rotation = Quaternion.Lerp(
                    pMP.mapPanelGroup.transform.rotation,
                    Quaternion.LookRotation(
                        pMP.mapPanelGroup.transform.position - Camera.main.transform.position,
                        Camera.main.transform.up),
                    d);
            }
        }

        void ProvinceGO_CheckEmpty()
        {
            //Для каждой отображаемой провинции
            foreach (int provinceEntity in pHS_F.Value)
            {
                //Берём PR
                ref C_ProvinceRender pR = ref pR_P.Value.Get(provinceEntity);

                //Если GO провинции не пуст, но не имеет дочерних объектов
                if (pR.ProvinceGO != null
                    && pR.ProvinceGO.transform.childCount == 0)
                {
                    //Кэшируем его
                    GO_Province.CacheProvinceGO(ref pR);
                }
            }
        }
    }
}
