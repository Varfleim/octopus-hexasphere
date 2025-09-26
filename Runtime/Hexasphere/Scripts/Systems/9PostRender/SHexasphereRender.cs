
using System.Collections.Generic;

using UnityEngine;

using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

using GBB.Map;

namespace HS
{
    public class SHexasphereRender : IEcsRunSystem
    {
        readonly EcsWorldInject world = default;

        readonly EcsFilterInject<Inc<CProvinceRender, CProvinceHexasphere>> pRFilter = default;
        readonly EcsPoolInject<CProvinceCore> pCPool = default;
        readonly EcsPoolInject<CProvinceRender> pRPool = default;
        readonly EcsPoolInject<CProvinceHexasphere> pHSPool = default;
        readonly EcsPoolInject<CProvinceMapPanels> pMPPool = default;

        readonly EcsFilterInject<Inc<CMapModeCore, CActiveMapMode>> activeMapModeFilter = default;
        readonly EcsPoolInject<CMapModeCore> mapModeCorePool = default;


        readonly EcsCustomInject<ProvinceData> provinceData = default;
        readonly EcsCustomInject<HexasphereData> hexasphereData = default;

        public void Run(IEcsSystems systems)
        {
            //Инициализация карты
            MapInitialization();

            //Обновление граней
            MapEdgesUpdate();

            //Обновление провинций
            MapProvincesUpdate();

            //Обновление материалов подсветки
            HighlightMaterialUpdate();

            //Подсветка наведения
            HoverHighlight();

            //Проверяем запросы изменения положения панелей карты
            MapPanels();

            //Проверяем, нет ли пустых GO провинций для удаления
            ProvinceGOEmptyCheck();
        }

        readonly EcsFilterInject<Inc<RMapRenderInitialization>> mapRenderInitializationRFilter = default;
        readonly EcsPoolInject<RMapRenderInitialization> mapRenderInitializationRPool = default;
        void MapInitialization()
        {
            //Для каждого запроса инициализации карты
            foreach(int requestEntity in mapRenderInitializationRFilter.Value)
            {
                //Берём запрос
                ref RMapRenderInitialization requestComp = ref mapRenderInitializationRPool.Value.Get(requestEntity);

                //Находим сущность активного режима карты
                int activeMapModeEntity = -1;

                //Для каждого активного режима карты
                foreach (int mapModeEntity in activeMapModeFilter.Value)
                {
                    //Сохраняем сущность
                    activeMapModeEntity = mapModeEntity;
                }

                //Берём активный режим карты
                ref CMapModeCore activeMapMode = ref mapModeCorePool.Value.Get(activeMapModeEntity);

                //Инициализируем гексасферу
                HexasphereInitialization();

                //Создаём провинции гексасферы
                HexasphereProvincesCreation();

                //Обновляем материалы
                MaterialsUpdate();

                //Удаляем запрос
                mapRenderInitializationRPool.Value.Del(requestEntity);
            }
        }

        readonly EcsFilterInject<Inc<RMapEdgesUpdate>> mapEdgesUpdateRFilter = default;
        readonly EcsPoolInject<RMapEdgesUpdate> mapEdgesUpdateRPool = default;
        void MapEdgesUpdate()
        {
            //Для каждого запроса обновления граней
            foreach(int requestEntity in mapEdgesUpdateRFilter.Value)
            {
                //Берём запрос
                ref RMapEdgesUpdate requestComp = ref mapEdgesUpdateRPool.Value.Get(requestEntity);

                //Если требуется обновление тонких граней
                if(requestComp.isThinUpdated == true)
                {
                    //Обновляем тонкие грани
                    EdgesThinUpdate();
                }

                //Если требуется обновление толстых граней
                if(requestComp.isThickUpdated == true)
                {
                    //Обновляем толстые грани
                    EdgesThickUpdate();
                }

                //Удаляем запрос
                mapEdgesUpdateRPool.Value.Del(requestEntity);
            }
        }

        readonly EcsFilterInject<Inc<RMapProvincesUpdate>> mapProvincesUpdateRFilter = default;
        readonly EcsPoolInject<RMapProvincesUpdate> mapProvincesUpdateRPool = default;
        void MapProvincesUpdate()
        {
            //Для каждого запроса обновления провинций
            foreach(int requestEntity in mapProvincesUpdateRFilter.Value)
            {
                //Берём запрос
                ref RMapProvincesUpdate requestComp = ref mapProvincesUpdateRPool.Value.Get(requestEntity);

                //Находим сущность активного режима карты
                int activeMapModeEntity = -1;

                //Для каждого активного режима карты
                foreach (int mapModeEntity in activeMapModeFilter.Value)
                {
                    //Сохраняем сущность
                    activeMapModeEntity = mapModeEntity;
                }

                //Берём активный режим карты
                ref CMapModeCore activeMapMode = ref mapModeCorePool.Value.Get(activeMapModeEntity);

                //Если требуется обновление материалов
                if (requestComp.isMaterialUpdated == true)
                {
                    //Обновляем материалы
                    MaterialsUpdate();
                }

                //Если требуется обновление высот
                if (requestComp.isHeightUpdated == true)
                {
                    //Обновляем высоты провинций
                    ProvinceHeightsUpdate();

                    //Обновляем меши подсветки провинций
                    ProvinceHighlightMeshesUpdate();

                    //Обновляем панели карты провинций
                    ProvinceMapPanelsAltitudeUpdate();
                }

                //Если требуется обновление цветов
                if (requestComp.isColorUpdated == true)
                {
                    //Обновляем цвета провинций
                    ProvinceColorsUpdate(ref activeMapMode);
                }

                //Удаляем запрос
                mapProvincesUpdateRPool.Value.Del(requestEntity);
            } 
        }

        void HexasphereInitialization()
        {
            //Удаляем родительский объект чанков, если он не пуст
            if (HexasphereData.chunksRootGO != null)
            {
                Object.DestroyImmediate(HexasphereData.chunksRootGO);
            }

            //Удаляем родительский объект провинций, если он не пуст
            if (HexasphereData.provincesRootGO != null)
            {
                Object.DestroyImmediate(HexasphereData.provincesRootGO);
            }
                 
            //Инициализируем чанки
            ChunksInitialization();
        }

        /// <summary>
        /// Данная функция только создаёт чанки соответствующего размера, но не заполняет их реальными данными и не отображает
        /// </summary>
        void ChunksInitialization()
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

            //Для каждой провинции
            foreach (int provinceEntity in pRFilter.Value)
            {
                //Берём компоненты провинции
                ref CProvinceRender pR = ref pRPool.Value.Get(provinceEntity);
                ref CProvinceHexasphere pHS = ref pHSPool.Value.Get(provinceEntity);

                //Если текущий чанк заполнен, сохраняем его данные и создаём новый
                //Если количество вершин больше максимального количества в чанке
                if (chunkVerticesCount > HexasphereData.maxVertexCountPerChunk)
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
                    indicesArray = hexasphereData.Value.hexagonIndicesExtruded;
                }
                //Иначе провинция - пентагон
                else
                {
                    //Берём массив индексов пентагона
                    indicesArray = hexasphereData.Value.pentagonIndicesExtruded;
                }

                pHS.parentChunkTriangleStart = chunkIndices.Count;
                //Для каждого индекса провинции
                for (int a = 0; a < indicesArray.Length; a++)
                {
                    //Заносим в список индексы
                    chunkIndices.Add(0);
                }
                #endregion

                //Определяем положение данных провинции в чанках
                pHS.parentChunkStart = chunkVerticesCount;
                pHS.parentChunkIndex = chunkIndex;
                pHS.parentChunkLength = provinceVerticesCount;

                //Определяем, какой массив UV использует провинция
                Vector2[] uVArray;
                //Если провинция - гексагон
                if (pHS.vertexPoints.Length == 6)
                {
                    //Берём массив UV гексагона
                    uVArray = hexasphereData.Value.hexagonUVsExtruded;
                }
                //Иначе провинция - пентагон
                else
                {
                    //Берём массив UV пентагона
                    uVArray = hexasphereData.Value.pentagonUVsExtruded;
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
            hexasphereData.Value.chunksVertices = chunksVerticesList.ToArray();
            hexasphereData.Value.chunksIndices = chunksIndicesList.ToArray();
            hexasphereData.Value.chunksUV2 = chunksUV2List.ToArray();

            hexasphereData.Value.chunksUV = chunksUVList.ToArray();
            hexasphereData.Value.chunksColors = chunksColorsList.ToArray();

            #region GO and Meshes
            //Создаём родительский GO для чанков и сохраняем его
            GameObject chunksRootGO = MapCreateGOAndParent(
                HexasphereData.HexasphereGO.transform,
                HexasphereData.chunksRootGOName);
            HexasphereData.chunksRootGO = chunksRootGO;

            //Создаём списки мешфильтров, мешей и мешрендереров
            List<MeshFilter> chunkMeshFilters = new();
            List<Mesh> chunkMeshes = new();
            List<MeshRenderer> chunkMeshRenderers = new();

            //Для каждого чанка
            for(int a = 0; a <= chunkIndex; a++)
            {
                //Создаём GO чанка
                GameObject chunkGO = MapCreateGOAndParent(
                    chunksRootGO.transform,
                    HexasphereData.chunkGOName);

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
            hexasphereData.Value.chunkMeshFilters = chunkMeshFilters.ToArray();
            hexasphereData.Value.chunkMeshes = chunkMeshes.ToArray();
            hexasphereData.Value.chunkMeshRenderers = chunkMeshRenderers.ToArray();

            //Создаём массивы списков для граней
            hexasphereData.Value.thinEdgesChunkVertices = new List<Vector3>[chunkIndex + 1];
            hexasphereData.Value.thinEdgesChunkIndices = new List<int>[chunkIndex + 1];
            hexasphereData.Value.thinEdgesChunkUVs = new List<Vector2>[chunkIndex + 1];
            hexasphereData.Value.thinEdgesChunkColors = new List<Color32>[chunkIndex + 1];

            hexasphereData.Value.thickEdgesChunkVertices = new List<Vector3>[chunkIndex + 1];
            hexasphereData.Value.thickEdgesChunkIndices = new List<int>[chunkIndex + 1];
            hexasphereData.Value.thickEdgesChunkUVs = new List<Vector2>[chunkIndex + 1];
            hexasphereData.Value.thickEdgesChunkColors = new List<Color32>[chunkIndex + 1];

            //Создаём родительский GO для провинций и сохраняем его
            GameObject provincesRootGO = MapCreateGOAndParent(
                HexasphereData.HexasphereGO.transform,
                HexasphereData.provincesRootGOName);
            HexasphereData.provincesRootGO = provincesRootGO;
            #endregion
        }

        void HexasphereProvincesCreation()
        {
            //Для каждой провинции
            foreach(int provinceEntity in pRFilter.Value)
            {
                //Берём компоненты провинции
                ref CProvinceRender pR = ref pRPool.Value.Get(provinceEntity);
                ref CProvinceHexasphere pHS = ref pHSPool.Value.Get(provinceEntity);

                //Берём массивы чанка, в котором располагается провинция
                ref Vector3[] chunkVertices = ref hexasphereData.Value.chunksVertices[pHS.parentChunkIndex];
                ref int[] chunkIndices = ref hexasphereData.Value.chunksIndices[pHS.parentChunkIndex];
                ref Vector4[] chunkUV2 = ref hexasphereData.Value.chunksUV2[pHS.parentChunkIndex];

                //Определяем количество вершин провинции
                int provinceVerticesCount = pHS.vertexPoints.Length;

                //Создаём структуру для UV-координат провинции
                Vector4 gpos = Vector4.zero;

                //Для каждой вершины провинции
                for(int a = 0; a < pHS.vertexPoints.Length; a++)
                {
                    //Берём вершину
                    DHexaspherePoint point = pHS.vertexPoints[a];

                    //Берём координату вершины
                    Vector3 vertex = point.ProjectedVector3;

                    //Заносим её в массив вершин
                    chunkVertices[pHS.parentChunkStart + a] = vertex;

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
                    chunkVertices[pHS.parentChunkStart + 6]
                        = (pHS.vertexPoints[1].ProjectedVector3 + pHS.vertexPoints[5].ProjectedVector3) * 0.5f;
                    chunkVertices[pHS.parentChunkStart + 7]
                        = (pHS.vertexPoints[2].ProjectedVector3 + pHS.vertexPoints[4].ProjectedVector3) * 0.5f;

                    //Обновляем количество вершин
                    provinceVerticesCount += 2;

                    //Назначаем провинции массив индексов гексагона
                    indicesArray = hexasphereData.Value.hexagonIndicesExtruded;
                }
                //Иначе провинция - пентагон
                else
                {
                    //Заносим вершины основания пентагона в массив
                    chunkVertices[pHS.parentChunkStart + 5]
                        = (pHS.vertexPoints[1].ProjectedVector3 + pHS.vertexPoints[4].ProjectedVector3) * 0.5f;
                    chunkVertices[pHS.parentChunkStart + 6]
                        = (pHS.vertexPoints[2].ProjectedVector3 + pHS.vertexPoints[4].ProjectedVector3) * 0.5f;

                    //Обновляем количество вершин
                    provinceVerticesCount += 2;

                    //Обнуляем bevel для пентагонов
                    gpos.w = 1.0f;

                    //Назначаем провинции массив индексов пентагона
                    indicesArray = hexasphereData.Value.pentagonIndicesExtruded;
                }

                //Для каждой вершины
                for (int a = 0; a < provinceVerticesCount; a++)
                {
                    //Заносим gpos в массив UV-координат
                    chunkUV2[pHS.parentChunkStart + a] = gpos;
                }

                //Для каждого индекса
                for (int a = 0; a < indicesArray.Length; a++)
                {
                    //Заносим индекс в массив индексов
                    chunkIndices[pHS.parentChunkTriangleStart + a] = pHS.parentChunkStart + indicesArray[a];
                }
            }

            //Для каждого мешфильтра чанка
            for(int a = 0; a < hexasphereData.Value.chunkMeshFilters.Length; a++)
            {
                //Заполняем меш вершинами, индексами и UV
                hexasphereData.Value.chunkMeshes[a].SetVertices(hexasphereData.Value.chunksVertices[a]);
                hexasphereData.Value.chunkMeshes[a].SetTriangles(hexasphereData.Value.chunksIndices[a], 0);
                hexasphereData.Value.chunkMeshes[a].SetUVs(1, hexasphereData.Value.chunksUV2[a]);
            }
        }

        void MaterialsUpdate()
        {
            //Обновляем тени
            MeshRenderersShadowSupportUpdate();

            //Обновляем материал провинций
            hexasphereData.Value.provinceMaterial.SetFloat("_GradientIntensity", 1f - hexasphereData.Value.gradientIntensity);
            hexasphereData.Value.provinceMaterial.SetFloat("_ExtrusionMultiplier", HexasphereData.ExtrudeMultiplier);
            hexasphereData.Value.provinceMaterial.SetColor("_Color", hexasphereData.Value.tileTintColor);
            hexasphereData.Value.provinceMaterial.SetColor("_AmbientColor", hexasphereData.Value.ambientColor);
            hexasphereData.Value.provinceMaterial.SetFloat("_MinimumLight", hexasphereData.Value.minimumLight);

            //Обновляем материалы граней
            hexasphereData.Value.thinEdgesMaterial.SetFloat("_GradientIntensity", 1f - hexasphereData.Value.gradientIntensity);
            hexasphereData.Value.thinEdgesMaterial.SetFloat("_ExtrusionMultiplier", HexasphereData.ExtrudeMultiplier);
            Color thinEdgesColor = hexasphereData.Value.thinEdgesColor;
            thinEdgesColor.r *= hexasphereData.Value.thinEdgesColorIntensity;
            thinEdgesColor.g *= hexasphereData.Value.thinEdgesColorIntensity;
            thinEdgesColor.b *= hexasphereData.Value.thinEdgesColorIntensity;
            hexasphereData.Value.thinEdgesMaterial.SetColor("_Color", thinEdgesColor);

            hexasphereData.Value.thickEdgesMaterial.SetFloat("_GradientIntensity", 1f - hexasphereData.Value.gradientIntensity);
            hexasphereData.Value.thickEdgesMaterial.SetFloat("_ExtrusionMultiplier", HexasphereData.ExtrudeMultiplier);
            Color thickEdgesColor = hexasphereData.Value.thickEdgesColor;
            thickEdgesColor.r *= hexasphereData.Value.thickEdgesColorIntensity;
            thickEdgesColor.g *= hexasphereData.Value.thickEdgesColorIntensity;
            thickEdgesColor.b *= hexasphereData.Value.thickEdgesColorIntensity;
            hexasphereData.Value.thickEdgesMaterial.SetColor("_Color", thickEdgesColor);

            //Обновляем размер коллайдера
            HexasphereData.HexasphereCollider.radius = 0.5f * (1.0f + HexasphereData.ExtrudeMultiplier);

            //Обновляем свет
            MapUpdateLightingMode();

            //Обновляем скос
            MapUpdateBevel();
        }

        void EdgesThinUpdate()
        {
            //Проверяем, какие грани требуется отобразить
            //Для каждой провинции
            foreach(int provinceEntity in pRFilter.Value)
            {
                //Берём провинцию
                ref CProvinceCore pC = ref pCPool.Value.Get(provinceEntity);
                ref CProvinceRender pR = ref pRPool.Value.Get(provinceEntity);
                ref CProvinceHexasphere pHS = ref pHSPool.Value.Get(provinceEntity);

                //Обновляем маску граней
                pHS.thinEdges = 63;

                //Для каждой вершины
                for (int a = 0; a < pHS.vertices.Length; a++)
                {
                    //Берём текущую вершину и следующую
                    Vector3 p0 = pHS.vertices[a];
                    Vector3 p1 = a < pHS.vertices.Length - 1 ? pHS.vertices[a + 1] : pHS.vertices[0];

                    //Для каждого соседа
                    for(int b = 0; b < pC.neighbourProvincePEs.Length; b++)
                    {
                        //Берём соседа
                        pC.neighbourProvincePEs[b].Unpack(world.Value, out int neighbourProvinceEntity);
                        ref CProvinceCore neighbourPC = ref pCPool.Value.Get(neighbourProvinceEntity);
                        ref CProvinceRender neighbourPR = ref pRPool.Value.Get(neighbourProvinceEntity);
                        ref CProvinceHexasphere neighbourPHS = ref pHSPool.Value.Get(neighbourProvinceEntity);

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
                                    pHS.thinEdges &= 63 - (1 << a);

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
            List<Vector3> chunkVertices = HexasphereData.CheckList<Vector3>(ref hexasphereData.Value.thinEdgesChunkVertices[chunkIndex]);
            List<int> chunkIndices = HexasphereData.CheckList<int>(ref hexasphereData.Value.thinEdgesChunkIndices[chunkIndex]);
            List<Vector2> chunkUVs = HexasphereData.CheckList<Vector2>(ref hexasphereData.Value.thinEdgesChunkUVs[chunkIndex]);
            List<Color32> chunkColors = HexasphereData.CheckList<Color32>(ref hexasphereData.Value.thinEdgesChunkColors[chunkIndex]);

            //Для каждой провинции
            foreach (int provinceEntity in pRFilter.Value)
            {
                //Берём провинцию
                ref CProvinceRender pR = ref pRPool.Value.Get(provinceEntity);
                ref CProvinceHexasphere pHS = ref pHSPool.Value.Get(provinceEntity);

                //Если число вершин больше максимального в списке
                if(verticesCount > HexasphereData.maxVertexArraySize)
                {
                    //Увеличиваем индекс чанка
                    chunkIndex++;

                    //Берём списки нового чанка
                    chunkVertices = HexasphereData.CheckList<Vector3>(ref hexasphereData.Value.thinEdgesChunkVertices[chunkIndex]);
                    chunkIndices = HexasphereData.CheckList<int>(ref hexasphereData.Value.thinEdgesChunkIndices[chunkIndex]);
                    chunkUVs = HexasphereData.CheckList<Vector2>(ref hexasphereData.Value.thinEdgesChunkUVs[chunkIndex]);
                    chunkColors = HexasphereData.CheckList<Color32>(ref hexasphereData.Value.thinEdgesChunkColors[chunkIndex]);

                    //Обнуляем счётчик вершин
                    verticesCount = 0;
                }

                //Создаём структуру для UV-координат
                Vector2 uVExtruded = new(provinceCount, pR.ProvinceHeight);

                //Определяем положение данных провинции в чанках
                pHS.parentThinEdgesChunkIndex = chunkIndex;
                pHS.parentThinEdgesChunkStart = verticesCount;
                pHS.parentThinEdgesChunkLength = 0;

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
                    bool segmentVisible = (pHS.thinEdges & (1 << a)) != 0;

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
                        pHS.parentThinEdgesChunkLength++;
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
            if (HexasphereData.thinEdgesRootGO != null)
            {
                Object.DestroyImmediate(HexasphereData.thinEdgesRootGO);
            }

            //Создаём родительский GO для граней и сохраняем его
            GameObject edgesRootGO = MapCreateGOAndParent(
                HexasphereData.HexasphereGO.transform,
                HexasphereData.thinEdgesRootGOName);
            HexasphereData.thinEdgesRootGO = edgesRootGO;

            //Создаём списки мешфильтров, мешей и мешрендереров для граней
            List<MeshFilter> chunksMeshFilters = new();
            List<Mesh> chunksMeshes = new();
            List<MeshRenderer> chunksMeshRenderers = new();

            //Для каждого чанка
            for (int a = 0; a <= chunkIndex; a++)
            {
                //Создаём GO чанка граней
                GameObject chunkGO = MapCreateGOAndParent(
                    edgesRootGO.transform,
                    HexasphereData.thinEdgeChunkGOName);

                //Назначаем чанку компонент мешфильтра и заносим его в список
                MeshFilter meshFilter = chunkGO.AddComponent<MeshFilter>();
                chunksMeshFilters.Add(meshFilter);

                //Создаём меш и заносим его в список
                Mesh mesh = new();
                chunksMeshes.Add(mesh);

                //Заполняем меш вершинами, UV, цветами и индексами
                mesh.SetVertices(hexasphereData.Value.thinEdgesChunkVertices[a]);
                mesh.SetUVs(0, hexasphereData.Value.thinEdgesChunkUVs[a]);
                mesh.SetColors(hexasphereData.Value.thinEdgesChunkColors[a]);
                mesh.SetIndices(hexasphereData.Value.thinEdgesChunkIndices[a], MeshTopology.Lines, 0, false);

                //Назначаем меш мешфильтру
                meshFilter.sharedMesh = mesh;

                //Назначаем чанку компонент мешрендерера и заносим его в список
                MeshRenderer meshRenderer = chunkGO.AddComponent<MeshRenderer>();
                chunksMeshRenderers.Add(meshRenderer);

                //Устанавливаем параметры мешрендерера
                meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                meshRenderer.sharedMaterial = hexasphereData.Value.thinEdgesMaterial;
            }

            //Сохраняем списки мешей как массивы
            hexasphereData.Value.thinEdgesChunkMeshFilters = chunksMeshFilters.ToArray();
            hexasphereData.Value.thinEdgesChunkMeshes = chunksMeshes.ToArray();
            hexasphereData.Value.thinEdgesChunkMeshRenderers = chunksMeshRenderers.ToArray();
        }

        void EdgesThickUpdate()
        {
            //Проверяем, какие грани требуется отобразить
            //Для каждой провинции
            foreach (int provinceEntity in pRFilter.Value)
            {
                //Берём провинцию
                ref CProvinceCore pC = ref pCPool.Value.Get(provinceEntity);
                ref CProvinceRender pR = ref pRPool.Value.Get(provinceEntity);
                ref CProvinceHexasphere pHS = ref pHSPool.Value.Get(provinceEntity);

                //Обновляем маску граней
                pHS.thickEdges = 63;

                //Для каждой вершины
                for (int a = 0; a < pHS.vertices.Length; a++)
                {
                    //Берём текущую вершину и следующую
                    Vector3 p0 = pHS.vertices[a];
                    Vector3 p1 = a < pHS.vertices.Length - 1 ? pHS.vertices[a + 1] : pHS.vertices[0];

                    //Для каждого соседа
                    for (int b = 0; b < pC.neighbourProvincePEs.Length; b++)
                    {
                        //Берём соседа
                        pC.neighbourProvincePEs[b].Unpack(world.Value, out int neighbourProvinceEntity);
                        ref CProvinceCore neighbourPC = ref pCPool.Value.Get(neighbourProvinceEntity);
                        ref CProvinceRender neighbourPR = ref pRPool.Value.Get(neighbourProvinceEntity);
                        ref CProvinceHexasphere neighbourPHS = ref pHSPool.Value.Get(neighbourProvinceEntity);

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
                                    pHS.thickEdges &= 63 - (1 << a);

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
            List<Vector3> chunkVertices = HexasphereData.CheckList<Vector3>(ref hexasphereData.Value.thickEdgesChunkVertices[chunkIndex]);
            List<int> chunkIndices = HexasphereData.CheckList<int>(ref hexasphereData.Value.thickEdgesChunkIndices[chunkIndex]);
            List<Vector2> chunkUVs = HexasphereData.CheckList<Vector2>(ref hexasphereData.Value.thickEdgesChunkUVs[chunkIndex]);
            List<Color32> chunkColors = HexasphereData.CheckList<Color32>(ref hexasphereData.Value.thickEdgesChunkColors[chunkIndex]);

            //Для каждой провинции
            foreach (int provinceEntity in pRFilter.Value)
            {
                //Берём провинцию
                ref CProvinceRender pR = ref pRPool.Value.Get(provinceEntity);
                ref CProvinceHexasphere pHS = ref pHSPool.Value.Get(provinceEntity);

                //Если число вершин больше максимального в списке
                if (verticesCount > HexasphereData.maxVertexArraySize)
                {
                    //Увеличиваем индекс чанка
                    chunkIndex++;

                    //Берём списки нового чанка
                    chunkVertices = HexasphereData.CheckList<Vector3>(ref hexasphereData.Value.thickEdgesChunkVertices[chunkIndex]);
                    chunkIndices = HexasphereData.CheckList<int>(ref hexasphereData.Value.thickEdgesChunkIndices[chunkIndex]);
                    chunkUVs = HexasphereData.CheckList<Vector2>(ref hexasphereData.Value.thickEdgesChunkUVs[chunkIndex]);
                    chunkColors = HexasphereData.CheckList<Color32>(ref hexasphereData.Value.thickEdgesChunkColors[chunkIndex]);

                    //Обнуляем счётчик вершин
                    verticesCount = 0;
                }

                //Создаём структуру для UV-координат
                Vector2 uVExtruded = new(provinceCount, pR.ProvinceHeight);

                //Определяем положение данных провинции в чанках
                pHS.parentThickEdgesChunkIndex = chunkIndex;
                pHS.parentThickEdgesChunkStart = verticesCount;
                pHS.parentThickEdgesChunkLength = 0;

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
                    bool segmentVisible = (pHS.thickEdges & (1 << a)) != 0;

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
                        pHS.parentThickEdgesChunkLength++;
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
            if (HexasphereData.thickEdgesRootGO != null)
            {
                Object.DestroyImmediate(HexasphereData.thickEdgesRootGO);
            }

            //Создаём родительский GO для граней и сохраняем его
            GameObject edgesRootGO = MapCreateGOAndParent(
                HexasphereData.HexasphereGO.transform,
                HexasphereData.thickEdgesRootGOName);
            HexasphereData.thickEdgesRootGO = edgesRootGO;

            //Создаём списки мешфильтров, мешей и мешрендереров для граней
            List<MeshFilter> chunksMeshFilters = new();
            List<Mesh> chunksMeshes = new();
            List<MeshRenderer> chunksMeshRenderers = new();

            //Для каждого чанка
            for (int a = 0; a <= chunkIndex; a++)
            {
                //Создаём GO чанка граней
                GameObject chunkGO = MapCreateGOAndParent(
                    edgesRootGO.transform,
                    HexasphereData.thickEdgeChunkGOName);

                //Назначаем чанку компонент мешфильтра и заносим его в список
                MeshFilter meshFilter = chunkGO.AddComponent<MeshFilter>();
                chunksMeshFilters.Add(meshFilter);

                //Создаём меш и заносим его в список
                Mesh mesh = new();
                chunksMeshes.Add(mesh);

                //Заполняем меш вершинами, UV, цветами и индексами
                mesh.SetVertices(hexasphereData.Value.thickEdgesChunkVertices[a]);
                mesh.SetUVs(0, hexasphereData.Value.thickEdgesChunkUVs[a]);
                mesh.SetColors(hexasphereData.Value.thickEdgesChunkColors[a]);
                mesh.SetIndices(hexasphereData.Value.thickEdgesChunkIndices[a], MeshTopology.Lines, 0, false);

                //Назначаем меш мешфильтру
                meshFilter.sharedMesh = mesh;

                //Назначаем чанку компонент мешрендерера и заносим его в список
                MeshRenderer meshRenderer = chunkGO.AddComponent<MeshRenderer>();
                chunksMeshRenderers.Add(meshRenderer);

                //Устанавливаем параметры мешрендерера
                meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                meshRenderer.sharedMaterial = hexasphereData.Value.thickEdgesMaterial;
            }

            //Сохраняем списки мешей как массивы
            hexasphereData.Value.thickEdgesChunkMeshFilters = chunksMeshFilters.ToArray();
            hexasphereData.Value.thickEdgesChunkMeshes = chunksMeshes.ToArray();
            hexasphereData.Value.thickEdgesChunkMeshRenderers = chunksMeshRenderers.ToArray();
        }

        void ProvinceHeightsUpdate()
        {
            //Для каждой провинции
            foreach (int provinceEntity in pRFilter.Value)
            {
                //Берём компоненты провинции
                ref CProvinceRender pR = ref pRPool.Value.Get(provinceEntity);
                ref CProvinceHexasphere pHS = ref pHSPool.Value.Get(provinceEntity);

                #region ProvinceMesh
                //Берём массив чанка, в котором располагается провинция
                ref Vector4[] chunkUV = ref hexasphereData.Value.chunksUV[pHS.parentChunkIndex];

                //Определяем, какой массив UV использует провинция
                Vector2[] uVArray;

                //Если провинция - гексагон
                if (pHS.vertexPoints.Length == 6)
                {
                    //Назначаем провинции массив UV гексагона
                    uVArray = hexasphereData.Value.hexagonUVsExtruded;
                }
                //Иначе это пентагон
                else
                {
                    //Назначаем провинции массив UV пентагона
                    uVArray = hexasphereData.Value.pentagonUVsExtruded;
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
                    chunkUV[pHS.parentChunkStart + a] = uV4;
                }
                #endregion

                #region EdgesMesh
                //Берём список чанка тонких граней, в котором располагается провинция
                List<Vector2> thinEdgesChunkUVs = hexasphereData.Value.thinEdgesChunkUVs[pHS.parentThinEdgesChunkIndex];

                //Для каждой вершины провинции в чанке тонких граней
                for(int a = 0; a < pHS.parentThinEdgesChunkLength; a++)
                {
                    //Берём UV2-координаты вершины
                    Vector2 uv = thinEdgesChunkUVs[pHS.parentThinEdgesChunkStart + a];

                    //Обновляем высоту провинции
                    uv.y = pR.ProvinceHeight;

                    //Заносим координаты в список
                    thinEdgesChunkUVs[pHS.parentThinEdgesChunkStart + a] = uv;
                }

                //Берём список чанка толстых граней, в котором располагается провинция
                List<Vector2> thickEdgesChunkUVs = hexasphereData.Value.thickEdgesChunkUVs[pHS.parentThickEdgesChunkIndex];

                //Для каждой вершины провинции в чанке тонких граней
                for (int a = 0; a < pHS.parentThickEdgesChunkLength; a++)
                {
                    //Берём UV2-координаты вершины
                    Vector2 uv = thickEdgesChunkUVs[pHS.parentThickEdgesChunkStart + a];

                    //Обновляем высоту провинции
                    uv.y = pR.ProvinceHeight;

                    //Заносим координаты в список
                    thickEdgesChunkUVs[pHS.parentThickEdgesChunkStart + a] = uv;
                }
                #endregion
            }

            //Для каждого мешфильтра чанка
            for (int a = 0; a < hexasphereData.Value.chunkMeshFilters.Length; a++)
            {
                //Заполняем меш UV
                hexasphereData.Value.chunkMeshes[a].SetUVs(0, hexasphereData.Value.chunksUV[a]);

                hexasphereData.Value.chunkMeshFilters[a].sharedMesh = hexasphereData.Value.chunkMeshes[a];

                //Назначаем мешрендереру материал
                hexasphereData.Value.chunkMeshRenderers[a].sharedMaterial = hexasphereData.Value.provinceMaterial;
            }

            //Для каждого мешфильтра тонких граней
            for(int a = 0;a < hexasphereData.Value.thinEdgesChunkMeshFilters.Length; a++)
            {
                //Заполняем меш UV
                hexasphereData.Value.thinEdgesChunkMeshes[a].SetUVs(0, hexasphereData.Value.thinEdgesChunkUVs[a]);

                hexasphereData.Value.thinEdgesChunkMeshFilters[a].sharedMesh = hexasphereData.Value.thinEdgesChunkMeshes[a];

                //Назначаем мешрендереру материал
                hexasphereData.Value.thinEdgesChunkMeshRenderers[a].sharedMaterial = hexasphereData.Value.thinEdgesMaterial;
            }

            //Для каждого мешфильтра толстых граней
            for (int a = 0; a < hexasphereData.Value.thickEdgesChunkMeshFilters.Length; a++)
            {
                //Заполняем меш UV
                hexasphereData.Value.thickEdgesChunkMeshes[a].SetUVs(0, hexasphereData.Value.thickEdgesChunkUVs[a]);

                hexasphereData.Value.thickEdgesChunkMeshFilters[a].sharedMesh = hexasphereData.Value.thickEdgesChunkMeshes[a];

                //Назначаем мешрендереру материал
                hexasphereData.Value.thickEdgesChunkMeshRenderers[a].sharedMaterial = hexasphereData.Value.thickEdgesMaterial;
            }
        }

        void ProvinceColorsUpdate(
            ref CMapModeCore mapMode)
        {
            //Для каждой провинции
            foreach (int provinceEntity in pRFilter.Value)
            {
                //Берём компоненты провинции
                ref CProvinceRender pR = ref pRPool.Value.Get(provinceEntity);
                ref CProvinceHexasphere pHS = ref pHSPool.Value.Get(provinceEntity);

                //Берём массивы чанка, в котором располагается провинция
                ref Color32[] chunkColors = ref hexasphereData.Value.chunksColors[pHS.parentChunkIndex];

                //Определяем количество вершин провинции
                int provinceVertexCount = pHS.vertexPoints.Length + 2;

                //Для каждых UV2-координат в списке
                for (int a = 0; a < provinceVertexCount; a++)
                {
                    //Заносим цвет в массив цветов
                    chunkColors[pHS.parentChunkStart + a] = mapMode.GetProvinceColor(ref pR);
                }
            }

            //Для каждого мешфильтра чанка
            for (int a = 0; a < hexasphereData.Value.chunkMeshFilters.Length; a++)
            {
                //Заполняем меш цветами
                hexasphereData.Value.chunkMeshes[a].SetColors(hexasphereData.Value.chunksColors[a]);

                hexasphereData.Value.chunkMeshFilters[a].sharedMesh = hexasphereData.Value.chunkMeshes[a];

                //Назначаем мешрендереру материал
                hexasphereData.Value.chunkMeshRenderers[a].sharedMaterial = hexasphereData.Value.provinceMaterial;
            }
        }

        GameObject MapCreateGOAndParent(
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

        void MeshRenderersShadowSupportUpdate() 
        {
            //Для каждого мешрендерера провинции
            for (int a = 0; a < hexasphereData.Value.chunkMeshRenderers.Length; a++)
            {
                //Если рендерер не пуст и его имя верно
                if (hexasphereData.Value.chunkMeshRenderers[a] != null
                    && hexasphereData.Value.chunkMeshRenderers[a].name.Equals(HexasphereData.chunkGOName))
                {
                    //Настраиваем отбрасывание принятие и отбрасывание теней
                    hexasphereData.Value.chunkMeshRenderers[a].receiveShadows = true;
                    hexasphereData.Value.chunkMeshRenderers[a].shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
                }
            }
        }

        void MapUpdateLightingMode()
        {
            //Обновляем материалы
            MapUpdateLightingMaterial(hexasphereData.Value.provinceMaterial);
            MapUpdateLightingMaterial(hexasphereData.Value.provinceColoredMaterial);

            hexasphereData.Value.provinceMaterial.EnableKeyword("HEXA_ALPHA");
        }

        void MapUpdateLightingMaterial(
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

        void MapUpdateBevel()
        {
            //Определяем размер текстуры
            const int textureSize = 256;

            //Если текстура отсутствует или её ширина неверна
            if (hexasphereData.Value.bevelNormals == null || hexasphereData.Value.bevelNormals.width != textureSize)
            {
                //Создаём новую текстуру
                hexasphereData.Value.bevelNormals = new(
                    textureSize, textureSize,
                    TextureFormat.ARGB32,
                    false);
            }

            //Определяем размер текстуры
            int textureHeight = hexasphereData.Value.bevelNormals.height;
            int textureWidth = hexasphereData.Value.bevelNormals.width;

            //Если массив цветов текстуры отсутствует или его длина не соответствует текстуре
            if (hexasphereData.Value.bevelNormalsColors == null || hexasphereData.Value.bevelNormalsColors.Length != textureHeight * textureWidth)
            {
                //Создаём новый массив
                hexasphereData.Value.bevelNormalsColors = new Color[textureHeight * textureWidth];
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
                    hexasphereData.Value.bevelNormalsColors[index].r = 0f;

                    //Определяем расстояние до данного пикселя
                    float minDistSqr = float.MaxValue;

                    //Для каждого ребра шестиугольника
                    for (int a = 0; a < 6; a++)
                    {
                        //Берём индексы вершин
                        Vector2 t0 = hexasphereData.Value.hexagonUVsExtruded[a];
                        Vector2 t1 = a < 5 ? hexasphereData.Value.hexagonUVsExtruded[a + 1] : hexasphereData.Value.hexagonUVsExtruded[0];

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
                    hexasphereData.Value.bevelNormalsColors[index].r = f;

                    //Увеличиваем индекс пикселя
                    index++;
                }
            }

            //Задаём массив пикселей текстуре
            hexasphereData.Value.bevelNormals.SetPixels(hexasphereData.Value.bevelNormalsColors);

            //Применяем текстуру
            hexasphereData.Value.bevelNormals.Apply();

            //Задаём текстуру материалу
            hexasphereData.Value.provinceMaterial.SetTexture("_BumpMask", hexasphereData.Value.bevelNormals);
        }

        void HighlightMaterialUpdate()
        {
            //Обновляем яркость материалов подсветки
            hexasphereData.Value.hoverProvinceHighlightMaterial.SetFloat(
                ShaderParameters.ColorShift, Mathf.PingPong(Time.time * 0.25f, 1f));
            hexasphereData.Value.currentProvinceHighlightMaterial.SetFloat(
                ShaderParameters.ColorShift, Mathf.PingPong(Time.time * 0.25f, 1f));
        }

        readonly EcsPoolInject<CProvinceHoverHighlight> provinceHoverHighlightPool = default;
        readonly EcsPoolInject<SRShowMapHoverHighlight> showMapHoverHighlightPool = default;
        void HoverHighlight()
        {
            //Отключаем подсветку, где уже не требуется
            HoverHighlightDeactivation();

            //Обновляем подсветку, если она уже включена там, где требуется
            HoverHighlightUpdate();

            //Активируем подсветку, где требуется
            HoverHighlightActivation();
        }

        readonly EcsFilterInject<Inc<CProvinceHexasphere, CProvinceHoverHighlight>, Exc<SRShowMapHoverHighlight>> provinceHoverHighlightDeactivationFilter = default;
        void HoverHighlightDeactivation()
        {
            //Для каждой провинции, имеющей подсветку наведения и не имеющей запроса
            foreach (int provinceEntity in provinceHoverHighlightDeactivationFilter.Value)
            {
                //Берём компонент подсветки
                ref CProvinceHoverHighlight pHoverHighlight = ref provinceHoverHighlightPool.Value.Get(provinceEntity);

                //Кэшируем подсветку
                GOProvinceHighlight.CacheProvinceHighlight(ref pHoverHighlight);

                //Удаляем компонент подсветки
                provinceHoverHighlightPool.Value.Del(provinceEntity);
            }
        }

        readonly EcsFilterInject<Inc<CProvinceHexasphere, CProvinceHoverHighlight, SRShowMapHoverHighlight>> provinceHoverHighlightUpdateFilter = default;
        void HoverHighlightUpdate()
        {
            //Для каждой провинции, имеющей подсветку наведения и запрос
            foreach (int provinceEntity in provinceHoverHighlightUpdateFilter.Value)
            {
                //Берём провинцию 
                ref CProvinceRender pR = ref pRPool.Value.Get(provinceEntity);
                ref CProvinceHexasphere pHS = ref pHSPool.Value.Get(provinceEntity);
                ref CProvinceHoverHighlight pHoverHighlight = ref provinceHoverHighlightPool.Value.Get(provinceEntity);

                //Обновляем подсветку
                GOProvinceHighlight.UpdateProvinceHighlight(
                    ref pR,
                    pHoverHighlight.highlight,
                    hexasphereData.Value.hoverProvinceHighlightMaterial);

                //Удаляем запрос
                showMapHoverHighlightPool.Value.Del(provinceEntity);
            }
        }

        readonly EcsFilterInject<Inc<CProvinceHexasphere, SRShowMapHoverHighlight>> provinceHoverHighlightActivationFilter = default;
        void HoverHighlightActivation()
        {
            //Для каждой провинции, имеющей запрос
            foreach (int provinceEntity in provinceHoverHighlightActivationFilter.Value)
            {
                //Берём провинцию
                ref CProvinceRender pR = ref pRPool.Value.Get(provinceEntity);
                ref CProvinceHexasphere pHS = ref pHSPool.Value.Get(provinceEntity);

                //Если провинция не имеет GO
                if (pR.ProvinceGO == null)
                {
                    //Создаём его
                    GOProvince.InstantiateProvinceGO(
                        HexasphereData.provincesRootGO,
                        ref pR);
                }

                //Назначаем ей компонент подсветки
                ref CProvinceHoverHighlight pHoverHighlight = ref provinceHoverHighlightPool.Value.Add(provinceEntity);

                //Создаём подсветку
                GOProvinceHighlight.InstantiateProvinceHighlight(
                    ref pR, ref pHoverHighlight,
                    hexasphereData.Value.hoverProvinceHighlightMaterial);

                //Создаём меш подсветки
                ProvinceHighlightMeshCreation(
                    ref pR, ref pHS,
                    pHoverHighlight.highlight);

                //Удаляем запрос
                showMapHoverHighlightPool.Value.Del(provinceEntity);
            }
        }

        void ProvinceHighlightMeshCreation(
            ref CProvinceRender pR, ref CProvinceHexasphere pHS,
            GOProvinceHighlight provinceHighlight)
        {
            //Создаём новый меш
            Mesh mesh = new();
            mesh.hideFlags = HideFlags.DontSave;

            //Определяем высоту подсветки
            float extrusion = pR.ProvinceHeight * HexasphereData.ExtrudeMultiplier;

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
                    HexasphereData.hexagonIndices,
                    MeshTopology.Triangles,
                    0,
                    false);
                mesh.uv = HexasphereData.hexagonUVs;
            }
            //Иначе
            else
            {
                mesh.SetIndices(
                    HexasphereData.pentagonIndices,
                    MeshTopology.Triangles,
                    0,
                    false);
                mesh.uv = HexasphereData.pentagonUVs;
            }

            //Рассчитываем нормали меша
            mesh.SetNormals(pHS.vertices);
            mesh.RecalculateNormals();

            //Назначаем меш подсветке и включаем визуализацию
            provinceHighlight.meshFilter.sharedMesh = mesh;
            provinceHighlight.meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            provinceHighlight.meshRenderer.enabled = true;
        }

        readonly EcsFilterInject<Inc<CProvinceRender, CProvinceHexasphere, CProvinceHoverHighlight>> provinceHoverHighlightMeshUpdateFilter = default;
        void ProvinceHighlightMeshesUpdate()
        {
            //Для каждой провинции с компонентом подсветки наведения
            foreach (int provinceEntity in provinceHoverHighlightMeshUpdateFilter.Value)
            {
                //Берём провинцию
                ref CProvinceRender pR = ref pRPool.Value.Get(provinceEntity);
                ref CProvinceHexasphere pHS = ref pHSPool.Value.Get(provinceEntity);
                ref CProvinceHoverHighlight pHoverHighlight = ref provinceHoverHighlightPool.Value.Get(provinceEntity);

                //Обновляем меш подсветки
                ProvinceHighlightMeshUpdate(
                    ref pR, ref pHS,
                    pHoverHighlight.highlight);
            }
        }

        void ProvinceHighlightMeshUpdate(
            ref CProvinceRender pR, ref CProvinceHexasphere pHS,
            GOProvinceHighlight provinceHighlight)
        {
            //Берём меш подсветки
            Mesh mesh = provinceHighlight.meshFilter.sharedMesh;

            //Определяем высоту подсветки
            float extrusion = pR.ProvinceHeight * HexasphereData.ExtrudeMultiplier;

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

        void MapPanels()
        {
            //Изменяем родительские объекты панелей карты
            SetParentProvinceMapPanels();

            //Проверяем, нет ли провинций без панелей карты, но с компонентом
            ProvinceMapPanelsEmptyCheck();

            //Поворачиваем панели карты
            ProvinceMapPanelsUpdate();
        }

        readonly EcsFilterInject<Inc<RProvinceMapPanelSetParent>> provinceMapPanelSetParentRFilter = default;
        readonly EcsPoolInject<RProvinceMapPanelSetParent> provinceMapPanelSetParentRPool = default;
        void SetParentProvinceMapPanels()
        {
            //Для каждого запроса изменения родителя панели карты
            foreach (int requestEntity in provinceMapPanelSetParentRFilter.Value)
            {
                //Берём запрос
                ref RProvinceMapPanelSetParent requestComp = ref provinceMapPanelSetParentRPool.Value.Get(requestEntity);

                //Изменяем родителя панели карты
                ProvinceMapPanelSetParent(ref requestComp);

                //Удаляем запрос
                provinceMapPanelSetParentRPool.Value.Del(requestEntity);
            }
        }

        void ProvinceMapPanelSetParent(
            ref RProvinceMapPanelSetParent requestComp)
        {
            //Берём целевую провинцию
            requestComp.parentProvincePE.Unpack(world.Value, out int provinceEntity);
            ref CProvinceRender pR = ref pRPool.Value.Get(provinceEntity);
            ref CProvinceHexasphere pHS = ref pHSPool.Value.Get(provinceEntity);

            //Если провинция не имеет GO
            if (pR.ProvinceGO == null)
            {
                //Создаём его
                GOProvince.InstantiateProvinceGO(
                    HexasphereData.provincesRootGO,
                    ref pR);
            }

            //Если провинция не имеет компонента панелей карты
            if(pMPPool.Value.Has(provinceEntity) == false)
            {
                //Назначаем компонент
                ProvinceMapPanelsCreate(
                    provinceEntity,
                    ref pR, ref pHS);
            }

            //Берём компонент панелей карты
            ref CProvinceMapPanels pMP = ref pMPPool.Value.Get(provinceEntity);

            //Присоединяем переданную панель к группе панелей провинции
            requestComp.mapPanelGO.transform.SetParent(pMP.mapPanelGroup.transform);
            requestComp.mapPanelGO.transform.localPosition = Vector3.zero;
        }

        void ProvinceMapPanelsCreate(
            int provinceEntity,
            ref CProvinceRender pR, ref CProvinceHexasphere pHS)
        {
            //Назначаем сущности провинции компонент панелей карты
            ref CProvinceMapPanels pMP = ref pMPPool.Value.Add(provinceEntity);

            //Заполняем данные компонента
            pMP = new(0);

            //Определяем центр провинции
            Vector3 provinceCenter = pHS.center;
            provinceCenter *= 1.0f + pR.ProvinceHeight * HexasphereData.ExtrudeMultiplier;
            provinceCenter *= hexasphereData.Value.hexasphereScale;

            //Создаём группу панелей карты
            CProvinceMapPanels.InstantiateMapPanelGroup(
                ref pR, ref pMP,
                provinceCenter,
                provinceData.Value.mapPanelAltitude);
        }

        readonly EcsFilterInject<Inc<CProvinceRender, CProvinceMapPanels>> pMPFilter = default;
        void ProvinceMapPanelsEmptyCheck()
        {
            //Для каждой провинции с компонентом панелей карты
            foreach(int provinceEntity in pMPFilter.Value)
            {
                //Берём компонент панелей карты
                ref CProvinceMapPanels pMP = ref pMPPool.Value.Get(provinceEntity);

                //Если провинция не имеет панелей карты
                if(pMP.mapPanelGroup.transform.childCount == 0)
                {
                    //Кэшируем группу панелей
                    CProvinceMapPanels.CacheMapPanelGroup(ref pMP);

                    //Удаляем с сущности компонент панелей карты
                    pMPPool.Value.Del(provinceEntity);
                }
            }
        }

        void ProvinceMapPanelsAltitudeUpdate()
        {
            //Для каждой провинции с компонентом панелей карты
            foreach (int provinceEntity in pMPFilter.Value)
            {
                //Берём компоненты провинции
                ref CProvinceRender pR = ref pRPool.Value.Get(provinceEntity);
                ref CProvinceHexasphere pHS = ref pHSPool.Value.Get(provinceEntity);
                ref CProvinceMapPanels pMP = ref pMPPool.Value.Get(provinceEntity);

                //Определяем положение центра провинции
                Vector3 provinceCenter = pHS.center;
                provinceCenter *= 1.0f + pR.ProvinceHeight * HexasphereData.ExtrudeMultiplier;
                provinceCenter *= hexasphereData.Value.hexasphereScale;

                //Задаём положение группы панелей
                CProvinceMapPanels.CalculateMapPanelGroupPosition(
                    ref pMP,
                    provinceCenter,
                    provinceData.Value.mapPanelAltitude);
            }
        }

        void ProvinceMapPanelsUpdate()
        {
            //Для каждой провинции с компонентом панелей карты
            foreach (int provinceEntity in pMPFilter.Value)
            {
                //Берём компонент панелей карты
                ref CProvinceMapPanels pMP = ref pMPPool.Value.Get(provinceEntity);

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

        void ProvinceGOEmptyCheck()
        {
            //Для каждой провинции с PR
            foreach (int provinceEntity in pRFilter.Value)
            {
                //Берём PR
                ref CProvinceRender pR = ref pRPool.Value.Get(provinceEntity);

                //Если GO провинции не пуст, но не имеет дочерних объектов
                if (pR.ProvinceGO != null
                    && pR.ProvinceGO.transform.childCount == 0)
                {
                    //Кэшируем его
                    GOProvince.CacheProvinceGO(ref pR);
                }
            }
        }
    }
}
