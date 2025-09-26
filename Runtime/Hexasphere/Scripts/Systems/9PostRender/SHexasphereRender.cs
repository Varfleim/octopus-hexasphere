
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
            //������������� �����
            MapInitialization();

            //���������� ������
            MapEdgesUpdate();

            //���������� ���������
            MapProvincesUpdate();

            //���������� ���������� ���������
            HighlightMaterialUpdate();

            //��������� ���������
            HoverHighlight();

            //��������� ������� ��������� ��������� ������� �����
            MapPanels();

            //���������, ��� �� ������ GO ��������� ��� ��������
            ProvinceGOEmptyCheck();
        }

        readonly EcsFilterInject<Inc<RMapRenderInitialization>> mapRenderInitializationRFilter = default;
        readonly EcsPoolInject<RMapRenderInitialization> mapRenderInitializationRPool = default;
        void MapInitialization()
        {
            //��� ������� ������� ������������� �����
            foreach(int requestEntity in mapRenderInitializationRFilter.Value)
            {
                //���� ������
                ref RMapRenderInitialization requestComp = ref mapRenderInitializationRPool.Value.Get(requestEntity);

                //������� �������� ��������� ������ �����
                int activeMapModeEntity = -1;

                //��� ������� ��������� ������ �����
                foreach (int mapModeEntity in activeMapModeFilter.Value)
                {
                    //��������� ��������
                    activeMapModeEntity = mapModeEntity;
                }

                //���� �������� ����� �����
                ref CMapModeCore activeMapMode = ref mapModeCorePool.Value.Get(activeMapModeEntity);

                //�������������� ����������
                HexasphereInitialization();

                //������ ��������� ����������
                HexasphereProvincesCreation();

                //��������� ���������
                MaterialsUpdate();

                //������� ������
                mapRenderInitializationRPool.Value.Del(requestEntity);
            }
        }

        readonly EcsFilterInject<Inc<RMapEdgesUpdate>> mapEdgesUpdateRFilter = default;
        readonly EcsPoolInject<RMapEdgesUpdate> mapEdgesUpdateRPool = default;
        void MapEdgesUpdate()
        {
            //��� ������� ������� ���������� ������
            foreach(int requestEntity in mapEdgesUpdateRFilter.Value)
            {
                //���� ������
                ref RMapEdgesUpdate requestComp = ref mapEdgesUpdateRPool.Value.Get(requestEntity);

                //���� ��������� ���������� ������ ������
                if(requestComp.isThinUpdated == true)
                {
                    //��������� ������ �����
                    EdgesThinUpdate();
                }

                //���� ��������� ���������� ������� ������
                if(requestComp.isThickUpdated == true)
                {
                    //��������� ������� �����
                    EdgesThickUpdate();
                }

                //������� ������
                mapEdgesUpdateRPool.Value.Del(requestEntity);
            }
        }

        readonly EcsFilterInject<Inc<RMapProvincesUpdate>> mapProvincesUpdateRFilter = default;
        readonly EcsPoolInject<RMapProvincesUpdate> mapProvincesUpdateRPool = default;
        void MapProvincesUpdate()
        {
            //��� ������� ������� ���������� ���������
            foreach(int requestEntity in mapProvincesUpdateRFilter.Value)
            {
                //���� ������
                ref RMapProvincesUpdate requestComp = ref mapProvincesUpdateRPool.Value.Get(requestEntity);

                //������� �������� ��������� ������ �����
                int activeMapModeEntity = -1;

                //��� ������� ��������� ������ �����
                foreach (int mapModeEntity in activeMapModeFilter.Value)
                {
                    //��������� ��������
                    activeMapModeEntity = mapModeEntity;
                }

                //���� �������� ����� �����
                ref CMapModeCore activeMapMode = ref mapModeCorePool.Value.Get(activeMapModeEntity);

                //���� ��������� ���������� ����������
                if (requestComp.isMaterialUpdated == true)
                {
                    //��������� ���������
                    MaterialsUpdate();
                }

                //���� ��������� ���������� �����
                if (requestComp.isHeightUpdated == true)
                {
                    //��������� ������ ���������
                    ProvinceHeightsUpdate();

                    //��������� ���� ��������� ���������
                    ProvinceHighlightMeshesUpdate();

                    //��������� ������ ����� ���������
                    ProvinceMapPanelsAltitudeUpdate();
                }

                //���� ��������� ���������� ������
                if (requestComp.isColorUpdated == true)
                {
                    //��������� ����� ���������
                    ProvinceColorsUpdate(ref activeMapMode);
                }

                //������� ������
                mapProvincesUpdateRPool.Value.Del(requestEntity);
            } 
        }

        void HexasphereInitialization()
        {
            //������� ������������ ������ ������, ���� �� �� ����
            if (HexasphereData.chunksRootGO != null)
            {
                Object.DestroyImmediate(HexasphereData.chunksRootGO);
            }

            //������� ������������ ������ ���������, ���� �� �� ����
            if (HexasphereData.provincesRootGO != null)
            {
                Object.DestroyImmediate(HexasphereData.provincesRootGO);
            }
                 
            //�������������� �����
            ChunksInitialization();
        }

        /// <summary>
        /// ������ ������� ������ ������ ����� ���������������� �������, �� �� ��������� �� ��������� ������� � �� ����������
        /// </summary>
        void ChunksInitialization()
        {
            //������ ������ �������� ��� ������ ������
            List<Vector3[]> chunksVerticesList = new();
            List<int[]> chunksIndicesList = new();
            List<Vector4[]> chunksUV2List = new();

            List<Vector4[]> chunksUVList = new();
            List<Color32[]> chunksColorsList = new();

            //������ ������� ������
            int chunkIndex = 0;

            //������ ������ ��� �������� �����
            List<Vector3> chunkVertices = new();
            List<int> chunkIndices = new();
            List<Vector4> chunkUV2 = new();

            List<Vector4> chunkUV = new();
            List<Color32> chunkColors = new();

            //������ ������� ������ � �����
            int chunkVerticesCount = 0;

            //��� ������ ���������
            foreach (int provinceEntity in pRFilter.Value)
            {
                //���� ���������� ���������
                ref CProvinceRender pR = ref pRPool.Value.Get(provinceEntity);
                ref CProvinceHexasphere pHS = ref pHSPool.Value.Get(provinceEntity);

                //���� ������� ���� ��������, ��������� ��� ������ � ������ �����
                //���� ���������� ������ ������ ������������� ���������� � �����
                if (chunkVerticesCount > HexasphereData.maxVertexCountPerChunk)
                {
                    //������� ������ ����������� ����� ��� ������� � ������ ��������, � ����� �������
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

                    //����������� ������� ������
                    chunkIndex++;

                    //�������� ������� ������ � �����
                    chunkVerticesCount = 0;
                }

                //���������� ���������� ������ ���������
                int provinceVerticesCount = pHS.vertexPoints.Length;

                #region Vertices
                //��� ������ ������� ���������
                for (int a = 0; a < pHS.vertexPoints.Length; a++)
                {
                    //������� � ������ ������ ������ �������
                    chunkVertices.Add(Vector3.zero);
                }

                //������� ��� �������������� �������, ������ ��� ����� �������
                chunkVertices.Add(Vector3.zero);
                chunkVertices.Add(Vector3.zero);
                #endregion

                //����������� ���������� ������
                provinceVerticesCount += 2;

                #region UV2
                //��� ������ ������� ���������, �������� ��������������
                for (int a = 0; a < provinceVerticesCount; a++)
                {
                    //������� � ������ ������ ����������
                    chunkUV2.Add(Vector4.zero);
                }
                #endregion

                #region Indices
                //����������, ����� ������ �������� ���������� ���������
                int[] indicesArray;
                //���� ��������� - ��������
                if (pHS.vertexPoints.Length == 6)
                {
                    //���� ������ �������� ���������
                    indicesArray = hexasphereData.Value.hexagonIndicesExtruded;
                }
                //����� ��������� - ��������
                else
                {
                    //���� ������ �������� ���������
                    indicesArray = hexasphereData.Value.pentagonIndicesExtruded;
                }

                pHS.parentChunkTriangleStart = chunkIndices.Count;
                //��� ������� ������� ���������
                for (int a = 0; a < indicesArray.Length; a++)
                {
                    //������� � ������ �������
                    chunkIndices.Add(0);
                }
                #endregion

                //���������� ��������� ������ ��������� � ������
                pHS.parentChunkStart = chunkVerticesCount;
                pHS.parentChunkIndex = chunkIndex;
                pHS.parentChunkLength = provinceVerticesCount;

                //����������, ����� ������ UV ���������� ���������
                Vector2[] uVArray;
                //���� ��������� - ��������
                if (pHS.vertexPoints.Length == 6)
                {
                    //���� ������ UV ���������
                    uVArray = hexasphereData.Value.hexagonUVsExtruded;
                }
                //����� ��������� - ��������
                else
                {
                    //���� ������ UV ���������
                    uVArray = hexasphereData.Value.pentagonUVsExtruded;
                }

                #region UV
                //��� ������� UV ���������
                for(int a = 0; a < uVArray.Length; a++)
                {
                    //������� � ������ ������ ����������
                    chunkUV.Add(Vector4.zero);
                }
                #endregion

                #region Colors
                //��� ������� UV ���������
                for (int a = 0; a < uVArray.Length; a++)
                {
                    //������� � ������ ����� ����
                    chunkColors.Add(Color.white);
                }
                #endregion

                //����������� ���������� ������ � ����� �� ���������� ������ ���������
                chunkVerticesCount += provinceVerticesCount;
            }

            //������� ������ ���������� ����� ��� ������� � ������ ��������
            chunksVerticesList.Add(chunkVertices.ToArray());
            chunksIndicesList.Add(chunkIndices.ToArray());
            chunksUV2List.Add(chunkUV2.ToArray());

            chunksUVList.Add(chunkUV.ToArray());
            chunksColorsList.Add(chunkColors.ToArray());

            //��������� ������ �������� ��� �������
            hexasphereData.Value.chunksVertices = chunksVerticesList.ToArray();
            hexasphereData.Value.chunksIndices = chunksIndicesList.ToArray();
            hexasphereData.Value.chunksUV2 = chunksUV2List.ToArray();

            hexasphereData.Value.chunksUV = chunksUVList.ToArray();
            hexasphereData.Value.chunksColors = chunksColorsList.ToArray();

            #region GO and Meshes
            //������ ������������ GO ��� ������ � ��������� ���
            GameObject chunksRootGO = MapCreateGOAndParent(
                HexasphereData.HexasphereGO.transform,
                HexasphereData.chunksRootGOName);
            HexasphereData.chunksRootGO = chunksRootGO;

            //������ ������ �����������, ����� � �������������
            List<MeshFilter> chunkMeshFilters = new();
            List<Mesh> chunkMeshes = new();
            List<MeshRenderer> chunkMeshRenderers = new();

            //��� ������� �����
            for(int a = 0; a <= chunkIndex; a++)
            {
                //������ GO �����
                GameObject chunkGO = MapCreateGOAndParent(
                    chunksRootGO.transform,
                    HexasphereData.chunkGOName);

                //��������� ����� ��������� ���������� � ������� ��� � ������
                MeshFilter meshFilter = chunkGO.AddComponent<MeshFilter>();
                chunkMeshFilters.Add(meshFilter);

                //������ ���, ������� ��� � ������ � ��������� ����������
                Mesh mesh = new();
                chunkMeshes.Add(mesh);
                meshFilter.sharedMesh = mesh;

                //��������� ����� ��������� ������������ � ������� ��� � ������
                MeshRenderer meshRenderer = chunkGO.AddComponent<MeshRenderer>();
                chunkMeshRenderers.Add(meshRenderer);
            }

            //��������� ������ ����� ��� �������
            hexasphereData.Value.chunkMeshFilters = chunkMeshFilters.ToArray();
            hexasphereData.Value.chunkMeshes = chunkMeshes.ToArray();
            hexasphereData.Value.chunkMeshRenderers = chunkMeshRenderers.ToArray();

            //������ ������� ������� ��� ������
            hexasphereData.Value.thinEdgesChunkVertices = new List<Vector3>[chunkIndex + 1];
            hexasphereData.Value.thinEdgesChunkIndices = new List<int>[chunkIndex + 1];
            hexasphereData.Value.thinEdgesChunkUVs = new List<Vector2>[chunkIndex + 1];
            hexasphereData.Value.thinEdgesChunkColors = new List<Color32>[chunkIndex + 1];

            hexasphereData.Value.thickEdgesChunkVertices = new List<Vector3>[chunkIndex + 1];
            hexasphereData.Value.thickEdgesChunkIndices = new List<int>[chunkIndex + 1];
            hexasphereData.Value.thickEdgesChunkUVs = new List<Vector2>[chunkIndex + 1];
            hexasphereData.Value.thickEdgesChunkColors = new List<Color32>[chunkIndex + 1];

            //������ ������������ GO ��� ��������� � ��������� ���
            GameObject provincesRootGO = MapCreateGOAndParent(
                HexasphereData.HexasphereGO.transform,
                HexasphereData.provincesRootGOName);
            HexasphereData.provincesRootGO = provincesRootGO;
            #endregion
        }

        void HexasphereProvincesCreation()
        {
            //��� ������ ���������
            foreach(int provinceEntity in pRFilter.Value)
            {
                //���� ���������� ���������
                ref CProvinceRender pR = ref pRPool.Value.Get(provinceEntity);
                ref CProvinceHexasphere pHS = ref pHSPool.Value.Get(provinceEntity);

                //���� ������� �����, � ������� ������������� ���������
                ref Vector3[] chunkVertices = ref hexasphereData.Value.chunksVertices[pHS.parentChunkIndex];
                ref int[] chunkIndices = ref hexasphereData.Value.chunksIndices[pHS.parentChunkIndex];
                ref Vector4[] chunkUV2 = ref hexasphereData.Value.chunksUV2[pHS.parentChunkIndex];

                //���������� ���������� ������ ���������
                int provinceVerticesCount = pHS.vertexPoints.Length;

                //������ ��������� ��� UV-��������� ���������
                Vector4 gpos = Vector4.zero;

                //��� ������ ������� ���������
                for(int a = 0; a < pHS.vertexPoints.Length; a++)
                {
                    //���� �������
                    DHexaspherePoint point = pHS.vertexPoints[a];

                    //���� ���������� �������
                    Vector3 vertex = point.ProjectedVector3;

                    //������� � � ������ ������
                    chunkVertices[pHS.parentChunkStart + a] = vertex;

                    //������������ ���������� UV
                    gpos.x += vertex.x;
                    gpos.y += vertex.y;
                    gpos.z += vertex.z;
                }

                //������������ gpos
                gpos.x /= pHS.vertexPoints.Length;
                gpos.y /= pHS.vertexPoints.Length;
                gpos.z /= pHS.vertexPoints.Length;

                //����������, ����� ������ �������� ���������� ���������
                int[] indicesArray;

                //���� ��������� - ��������
                if(pHS.vertexPoints.Length == 6)
                {
                    //������� ������� ��������� ��������� � ������
                    chunkVertices[pHS.parentChunkStart + 6]
                        = (pHS.vertexPoints[1].ProjectedVector3 + pHS.vertexPoints[5].ProjectedVector3) * 0.5f;
                    chunkVertices[pHS.parentChunkStart + 7]
                        = (pHS.vertexPoints[2].ProjectedVector3 + pHS.vertexPoints[4].ProjectedVector3) * 0.5f;

                    //��������� ���������� ������
                    provinceVerticesCount += 2;

                    //��������� ��������� ������ �������� ���������
                    indicesArray = hexasphereData.Value.hexagonIndicesExtruded;
                }
                //����� ��������� - ��������
                else
                {
                    //������� ������� ��������� ��������� � ������
                    chunkVertices[pHS.parentChunkStart + 5]
                        = (pHS.vertexPoints[1].ProjectedVector3 + pHS.vertexPoints[4].ProjectedVector3) * 0.5f;
                    chunkVertices[pHS.parentChunkStart + 6]
                        = (pHS.vertexPoints[2].ProjectedVector3 + pHS.vertexPoints[4].ProjectedVector3) * 0.5f;

                    //��������� ���������� ������
                    provinceVerticesCount += 2;

                    //�������� bevel ��� ����������
                    gpos.w = 1.0f;

                    //��������� ��������� ������ �������� ���������
                    indicesArray = hexasphereData.Value.pentagonIndicesExtruded;
                }

                //��� ������ �������
                for (int a = 0; a < provinceVerticesCount; a++)
                {
                    //������� gpos � ������ UV-���������
                    chunkUV2[pHS.parentChunkStart + a] = gpos;
                }

                //��� ������� �������
                for (int a = 0; a < indicesArray.Length; a++)
                {
                    //������� ������ � ������ ��������
                    chunkIndices[pHS.parentChunkTriangleStart + a] = pHS.parentChunkStart + indicesArray[a];
                }
            }

            //��� ������� ���������� �����
            for(int a = 0; a < hexasphereData.Value.chunkMeshFilters.Length; a++)
            {
                //��������� ��� ���������, ��������� � UV
                hexasphereData.Value.chunkMeshes[a].SetVertices(hexasphereData.Value.chunksVertices[a]);
                hexasphereData.Value.chunkMeshes[a].SetTriangles(hexasphereData.Value.chunksIndices[a], 0);
                hexasphereData.Value.chunkMeshes[a].SetUVs(1, hexasphereData.Value.chunksUV2[a]);
            }
        }

        void MaterialsUpdate()
        {
            //��������� ����
            MeshRenderersShadowSupportUpdate();

            //��������� �������� ���������
            hexasphereData.Value.provinceMaterial.SetFloat("_GradientIntensity", 1f - hexasphereData.Value.gradientIntensity);
            hexasphereData.Value.provinceMaterial.SetFloat("_ExtrusionMultiplier", HexasphereData.ExtrudeMultiplier);
            hexasphereData.Value.provinceMaterial.SetColor("_Color", hexasphereData.Value.tileTintColor);
            hexasphereData.Value.provinceMaterial.SetColor("_AmbientColor", hexasphereData.Value.ambientColor);
            hexasphereData.Value.provinceMaterial.SetFloat("_MinimumLight", hexasphereData.Value.minimumLight);

            //��������� ��������� ������
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

            //��������� ������ ����������
            HexasphereData.HexasphereCollider.radius = 0.5f * (1.0f + HexasphereData.ExtrudeMultiplier);

            //��������� ����
            MapUpdateLightingMode();

            //��������� ����
            MapUpdateBevel();
        }

        void EdgesThinUpdate()
        {
            //���������, ����� ����� ��������� ����������
            //��� ������ ���������
            foreach(int provinceEntity in pRFilter.Value)
            {
                //���� ���������
                ref CProvinceCore pC = ref pCPool.Value.Get(provinceEntity);
                ref CProvinceRender pR = ref pRPool.Value.Get(provinceEntity);
                ref CProvinceHexasphere pHS = ref pHSPool.Value.Get(provinceEntity);

                //��������� ����� ������
                pHS.thinEdges = 63;

                //��� ������ �������
                for (int a = 0; a < pHS.vertices.Length; a++)
                {
                    //���� ������� ������� � ���������
                    Vector3 p0 = pHS.vertices[a];
                    Vector3 p1 = a < pHS.vertices.Length - 1 ? pHS.vertices[a + 1] : pHS.vertices[0];

                    //��� ������� ������
                    for(int b = 0; b < pC.neighbourProvincePEs.Length; b++)
                    {
                        //���� ������
                        pC.neighbourProvincePEs[b].Unpack(world.Value, out int neighbourProvinceEntity);
                        ref CProvinceCore neighbourPC = ref pCPool.Value.Get(neighbourProvinceEntity);
                        ref CProvinceRender neighbourPR = ref pRPool.Value.Get(neighbourProvinceEntity);
                        ref CProvinceHexasphere neighbourPHS = ref pHSPool.Value.Get(neighbourProvinceEntity);

                        //���� ������� ������ ���������
                        if(pR.ThinEdgesIndex == neighbourPR.ThinEdgesIndex)
                        {
                            //��� ������ ������� ������
                            for (int c = 0; c < neighbourPHS.vertices.Length; c++)
                            {
                                //���� ������� ������� � ���������
                                Vector3 q0 = neighbourPHS.vertices[c];
                                Vector3 q1 = c < neighbourPHS.vertices.Length - 1 ? neighbourPHS.vertices[c + 1] : neighbourPHS.vertices[0];

                                //���� ������� ���������
                                if (p0 == q0 && p1 == q1 || p0 == q1 && p1 == q0)
                                {
                                    //��������� ����� ������
                                    pHS.thinEdges &= 63 - (1 << a);

                                    //������� �� ����� ������ �� �������
                                    b = 9999;
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            //������ ���� ������

            //���������� ������ ����� � ������ �������� ��������� � ������
            int chunkIndex = 0;
            int provinceCount = 0;
            int verticesCount = 0;

            //��������� ������ ������
            List<Vector3> chunkVertices = HexasphereData.CheckList<Vector3>(ref hexasphereData.Value.thinEdgesChunkVertices[chunkIndex]);
            List<int> chunkIndices = HexasphereData.CheckList<int>(ref hexasphereData.Value.thinEdgesChunkIndices[chunkIndex]);
            List<Vector2> chunkUVs = HexasphereData.CheckList<Vector2>(ref hexasphereData.Value.thinEdgesChunkUVs[chunkIndex]);
            List<Color32> chunkColors = HexasphereData.CheckList<Color32>(ref hexasphereData.Value.thinEdgesChunkColors[chunkIndex]);

            //��� ������ ���������
            foreach (int provinceEntity in pRFilter.Value)
            {
                //���� ���������
                ref CProvinceRender pR = ref pRPool.Value.Get(provinceEntity);
                ref CProvinceHexasphere pHS = ref pHSPool.Value.Get(provinceEntity);

                //���� ����� ������ ������ ������������� � ������
                if(verticesCount > HexasphereData.maxVertexArraySize)
                {
                    //����������� ������ �����
                    chunkIndex++;

                    //���� ������ ������ �����
                    chunkVertices = HexasphereData.CheckList<Vector3>(ref hexasphereData.Value.thinEdgesChunkVertices[chunkIndex]);
                    chunkIndices = HexasphereData.CheckList<int>(ref hexasphereData.Value.thinEdgesChunkIndices[chunkIndex]);
                    chunkUVs = HexasphereData.CheckList<Vector2>(ref hexasphereData.Value.thinEdgesChunkUVs[chunkIndex]);
                    chunkColors = HexasphereData.CheckList<Color32>(ref hexasphereData.Value.thinEdgesChunkColors[chunkIndex]);

                    //�������� ������� ������
                    verticesCount = 0;
                }

                //������ ��������� ��� UV-���������
                Vector2 uVExtruded = new(provinceCount, pR.ProvinceHeight);

                //���������� ��������� ������ ��������� � ������
                pHS.parentThinEdgesChunkIndex = chunkIndex;
                pHS.parentThinEdgesChunkStart = verticesCount;
                pHS.parentThinEdgesChunkLength = 0;

                //���������� ���� ���������
                Color32 provinceColor = Color.white;

                //��������� ��������� ������� � ������
                int vertex0 = verticesCount;

                //������ ���������� ��� ������������
                bool vertexRequired = false;
                bool vertex0Missing = true;

                //��� ������ ������� ���������
                for(int a = 0; a < pHS.vertexPoints.Length; a++)
                {
                    //����������, ������ �� �����
                    bool segmentVisible = (pHS.thinEdges & (1 << a)) != 0;

                    //���� ����� ������ ��� ������� ����������
                    if (segmentVisible || vertexRequired)
                    {
                        //������� ������� � ������
                        chunkVertices.Add(pHS.vertexPoints[a].ProjectedVector3);
                        chunkUVs.Add(uVExtruded);
                        chunkColors.Add(provinceColor);

                        //���� ������� ����������
                        if (vertexRequired == true)
                        {
                            //��������� ���������� �������
                            chunkIndices.Add(verticesCount);
                        }

                        //���� ����� ������
                        if (segmentVisible == true)
                        {
                            //�������� ����� �������
                            chunkIndices.Add(verticesCount);

                            //���� ��� ������ �������
                            if(a == 0)
                            {
                                //���������, ��� ������� ������� �� �����������
                                vertex0Missing = false;
                            }
                        }

                        //����������� ������� ������
                        verticesCount++;

                        //����������� ����� ������ ��������� � �����
                        pHS.parentThinEdgesChunkLength++;
                    }

                    //�������� ������������� ������� �� ��������� �����
                    vertexRequired = segmentVisible;
                }

                //���� ������� ����������
                if(vertexRequired == true)
                {
                    //���� ������� ������� �����������
                    if (vertex0Missing == true)
                    {
                        //������� ������ ������� ��������� � ������
                        chunkVertices.Add(pHS.vertexPoints[0].ProjectedVector3);
                        chunkUVs.Add(uVExtruded);
                        chunkColors.Add(provinceColor);
                        chunkIndices.Add(verticesCount);

                        //����������� ������� ������
                        verticesCount++;
                    }
                    //�����
                    else
                    {
                        //������� ���������� ������� ������� � ������
                        chunkIndices.Add(vertex0);
                    }
                }
            }

            //������� ������������ ������ ������, ���� �� �� ����
            if (HexasphereData.thinEdgesRootGO != null)
            {
                Object.DestroyImmediate(HexasphereData.thinEdgesRootGO);
            }

            //������ ������������ GO ��� ������ � ��������� ���
            GameObject edgesRootGO = MapCreateGOAndParent(
                HexasphereData.HexasphereGO.transform,
                HexasphereData.thinEdgesRootGOName);
            HexasphereData.thinEdgesRootGO = edgesRootGO;

            //������ ������ �����������, ����� � ������������� ��� ������
            List<MeshFilter> chunksMeshFilters = new();
            List<Mesh> chunksMeshes = new();
            List<MeshRenderer> chunksMeshRenderers = new();

            //��� ������� �����
            for (int a = 0; a <= chunkIndex; a++)
            {
                //������ GO ����� ������
                GameObject chunkGO = MapCreateGOAndParent(
                    edgesRootGO.transform,
                    HexasphereData.thinEdgeChunkGOName);

                //��������� ����� ��������� ���������� � ������� ��� � ������
                MeshFilter meshFilter = chunkGO.AddComponent<MeshFilter>();
                chunksMeshFilters.Add(meshFilter);

                //������ ��� � ������� ��� � ������
                Mesh mesh = new();
                chunksMeshes.Add(mesh);

                //��������� ��� ���������, UV, ������� � ���������
                mesh.SetVertices(hexasphereData.Value.thinEdgesChunkVertices[a]);
                mesh.SetUVs(0, hexasphereData.Value.thinEdgesChunkUVs[a]);
                mesh.SetColors(hexasphereData.Value.thinEdgesChunkColors[a]);
                mesh.SetIndices(hexasphereData.Value.thinEdgesChunkIndices[a], MeshTopology.Lines, 0, false);

                //��������� ��� ����������
                meshFilter.sharedMesh = mesh;

                //��������� ����� ��������� ������������ � ������� ��� � ������
                MeshRenderer meshRenderer = chunkGO.AddComponent<MeshRenderer>();
                chunksMeshRenderers.Add(meshRenderer);

                //������������� ��������� ������������
                meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                meshRenderer.sharedMaterial = hexasphereData.Value.thinEdgesMaterial;
            }

            //��������� ������ ����� ��� �������
            hexasphereData.Value.thinEdgesChunkMeshFilters = chunksMeshFilters.ToArray();
            hexasphereData.Value.thinEdgesChunkMeshes = chunksMeshes.ToArray();
            hexasphereData.Value.thinEdgesChunkMeshRenderers = chunksMeshRenderers.ToArray();
        }

        void EdgesThickUpdate()
        {
            //���������, ����� ����� ��������� ����������
            //��� ������ ���������
            foreach (int provinceEntity in pRFilter.Value)
            {
                //���� ���������
                ref CProvinceCore pC = ref pCPool.Value.Get(provinceEntity);
                ref CProvinceRender pR = ref pRPool.Value.Get(provinceEntity);
                ref CProvinceHexasphere pHS = ref pHSPool.Value.Get(provinceEntity);

                //��������� ����� ������
                pHS.thickEdges = 63;

                //��� ������ �������
                for (int a = 0; a < pHS.vertices.Length; a++)
                {
                    //���� ������� ������� � ���������
                    Vector3 p0 = pHS.vertices[a];
                    Vector3 p1 = a < pHS.vertices.Length - 1 ? pHS.vertices[a + 1] : pHS.vertices[0];

                    //��� ������� ������
                    for (int b = 0; b < pC.neighbourProvincePEs.Length; b++)
                    {
                        //���� ������
                        pC.neighbourProvincePEs[b].Unpack(world.Value, out int neighbourProvinceEntity);
                        ref CProvinceCore neighbourPC = ref pCPool.Value.Get(neighbourProvinceEntity);
                        ref CProvinceRender neighbourPR = ref pRPool.Value.Get(neighbourProvinceEntity);
                        ref CProvinceHexasphere neighbourPHS = ref pHSPool.Value.Get(neighbourProvinceEntity);

                        //���� ������� ������ ���������
                        if (pR.ThickEdgesIndex == neighbourPR.ThickEdgesIndex)
                        {
                            //��� ������ ������� ������
                            for (int c = 0; c < neighbourPHS.vertices.Length; c++)
                            {
                                //���� ������� ������� � ���������
                                Vector3 q0 = neighbourPHS.vertices[c];
                                Vector3 q1 = c < neighbourPHS.vertices.Length - 1 ? neighbourPHS.vertices[c + 1] : neighbourPHS.vertices[0];

                                //���� ������� ���������
                                if (p0 == q0 && p1 == q1 || p0 == q1 && p1 == q0)
                                {
                                    //��������� ����� ������
                                    pHS.thickEdges &= 63 - (1 << a);

                                    //������� �� ����� ������ �� �������
                                    b = 9999;
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            //������ ���� ������

            //���������� ������ ����� � ������ �������� ��������� � ������
            int chunkIndex = 0;
            int provinceCount = 0;
            int verticesCount = 0;

            //��������� ������ ������
            List<Vector3> chunkVertices = HexasphereData.CheckList<Vector3>(ref hexasphereData.Value.thickEdgesChunkVertices[chunkIndex]);
            List<int> chunkIndices = HexasphereData.CheckList<int>(ref hexasphereData.Value.thickEdgesChunkIndices[chunkIndex]);
            List<Vector2> chunkUVs = HexasphereData.CheckList<Vector2>(ref hexasphereData.Value.thickEdgesChunkUVs[chunkIndex]);
            List<Color32> chunkColors = HexasphereData.CheckList<Color32>(ref hexasphereData.Value.thickEdgesChunkColors[chunkIndex]);

            //��� ������ ���������
            foreach (int provinceEntity in pRFilter.Value)
            {
                //���� ���������
                ref CProvinceRender pR = ref pRPool.Value.Get(provinceEntity);
                ref CProvinceHexasphere pHS = ref pHSPool.Value.Get(provinceEntity);

                //���� ����� ������ ������ ������������� � ������
                if (verticesCount > HexasphereData.maxVertexArraySize)
                {
                    //����������� ������ �����
                    chunkIndex++;

                    //���� ������ ������ �����
                    chunkVertices = HexasphereData.CheckList<Vector3>(ref hexasphereData.Value.thickEdgesChunkVertices[chunkIndex]);
                    chunkIndices = HexasphereData.CheckList<int>(ref hexasphereData.Value.thickEdgesChunkIndices[chunkIndex]);
                    chunkUVs = HexasphereData.CheckList<Vector2>(ref hexasphereData.Value.thickEdgesChunkUVs[chunkIndex]);
                    chunkColors = HexasphereData.CheckList<Color32>(ref hexasphereData.Value.thickEdgesChunkColors[chunkIndex]);

                    //�������� ������� ������
                    verticesCount = 0;
                }

                //������ ��������� ��� UV-���������
                Vector2 uVExtruded = new(provinceCount, pR.ProvinceHeight);

                //���������� ��������� ������ ��������� � ������
                pHS.parentThickEdgesChunkIndex = chunkIndex;
                pHS.parentThickEdgesChunkStart = verticesCount;
                pHS.parentThickEdgesChunkLength = 0;

                //���������� ���� ���������
                Color32 provinceColor = Color.white;

                //��������� ��������� ������� � ������
                int vertex0 = verticesCount;

                //������ ���������� ��� ������������
                bool vertexRequired = false;
                bool vertex0Missing = true;

                //��� ������ ������� ���������
                for (int a = 0; a < pHS.vertexPoints.Length; a++)
                {
                    //����������, ������ �� �����
                    bool segmentVisible = (pHS.thickEdges & (1 << a)) != 0;

                    //���� ����� ������ ��� ������� ����������
                    if (segmentVisible || vertexRequired)
                    {
                        //������� ������� � ������
                        chunkVertices.Add(pHS.vertexPoints[a].ProjectedVector3);
                        chunkUVs.Add(uVExtruded);
                        chunkColors.Add(provinceColor);

                        //���� ������� ����������
                        if (vertexRequired == true)
                        {
                            //��������� ���������� �������
                            chunkIndices.Add(verticesCount);
                        }

                        //���� ����� ������
                        if (segmentVisible == true)
                        {
                            //�������� ����� �������
                            chunkIndices.Add(verticesCount);

                            //���� ��� ������ �������
                            if (a == 0)
                            {
                                //���������, ��� ������� ������� �� �����������
                                vertex0Missing = false;
                            }
                        }

                        //����������� ������� ������
                        verticesCount++;

                        //����������� ����� ������ ��������� � �����
                        pHS.parentThickEdgesChunkLength++;
                    }

                    //�������� ������������� ������� �� ��������� �����
                    vertexRequired = segmentVisible;
                }

                //���� ������� ����������
                if (vertexRequired == true)
                {
                    //���� ������� ������� �����������
                    if (vertex0Missing == true)
                    {
                        //������� ������ ������� ��������� � ������
                        chunkVertices.Add(pHS.vertexPoints[0].ProjectedVector3);
                        chunkUVs.Add(uVExtruded);
                        chunkColors.Add(provinceColor);
                        chunkIndices.Add(verticesCount);

                        //����������� ������� ������
                        verticesCount++;
                    }
                    //�����
                    else
                    {
                        //������� ���������� ������� ������� � ������
                        chunkIndices.Add(vertex0);
                    }
                }
            }

            //������� ������������ ������ ������, ���� �� �� ����
            if (HexasphereData.thickEdgesRootGO != null)
            {
                Object.DestroyImmediate(HexasphereData.thickEdgesRootGO);
            }

            //������ ������������ GO ��� ������ � ��������� ���
            GameObject edgesRootGO = MapCreateGOAndParent(
                HexasphereData.HexasphereGO.transform,
                HexasphereData.thickEdgesRootGOName);
            HexasphereData.thickEdgesRootGO = edgesRootGO;

            //������ ������ �����������, ����� � ������������� ��� ������
            List<MeshFilter> chunksMeshFilters = new();
            List<Mesh> chunksMeshes = new();
            List<MeshRenderer> chunksMeshRenderers = new();

            //��� ������� �����
            for (int a = 0; a <= chunkIndex; a++)
            {
                //������ GO ����� ������
                GameObject chunkGO = MapCreateGOAndParent(
                    edgesRootGO.transform,
                    HexasphereData.thickEdgeChunkGOName);

                //��������� ����� ��������� ���������� � ������� ��� � ������
                MeshFilter meshFilter = chunkGO.AddComponent<MeshFilter>();
                chunksMeshFilters.Add(meshFilter);

                //������ ��� � ������� ��� � ������
                Mesh mesh = new();
                chunksMeshes.Add(mesh);

                //��������� ��� ���������, UV, ������� � ���������
                mesh.SetVertices(hexasphereData.Value.thickEdgesChunkVertices[a]);
                mesh.SetUVs(0, hexasphereData.Value.thickEdgesChunkUVs[a]);
                mesh.SetColors(hexasphereData.Value.thickEdgesChunkColors[a]);
                mesh.SetIndices(hexasphereData.Value.thickEdgesChunkIndices[a], MeshTopology.Lines, 0, false);

                //��������� ��� ����������
                meshFilter.sharedMesh = mesh;

                //��������� ����� ��������� ������������ � ������� ��� � ������
                MeshRenderer meshRenderer = chunkGO.AddComponent<MeshRenderer>();
                chunksMeshRenderers.Add(meshRenderer);

                //������������� ��������� ������������
                meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                meshRenderer.sharedMaterial = hexasphereData.Value.thickEdgesMaterial;
            }

            //��������� ������ ����� ��� �������
            hexasphereData.Value.thickEdgesChunkMeshFilters = chunksMeshFilters.ToArray();
            hexasphereData.Value.thickEdgesChunkMeshes = chunksMeshes.ToArray();
            hexasphereData.Value.thickEdgesChunkMeshRenderers = chunksMeshRenderers.ToArray();
        }

        void ProvinceHeightsUpdate()
        {
            //��� ������ ���������
            foreach (int provinceEntity in pRFilter.Value)
            {
                //���� ���������� ���������
                ref CProvinceRender pR = ref pRPool.Value.Get(provinceEntity);
                ref CProvinceHexasphere pHS = ref pHSPool.Value.Get(provinceEntity);

                #region ProvinceMesh
                //���� ������ �����, � ������� ������������� ���������
                ref Vector4[] chunkUV = ref hexasphereData.Value.chunksUV[pHS.parentChunkIndex];

                //����������, ����� ������ UV ���������� ���������
                Vector2[] uVArray;

                //���� ��������� - ��������
                if (pHS.vertexPoints.Length == 6)
                {
                    //��������� ��������� ������ UV ���������
                    uVArray = hexasphereData.Value.hexagonUVsExtruded;
                }
                //����� ��� ��������
                else
                {
                    //��������� ��������� ������ UV ���������
                    uVArray = hexasphereData.Value.pentagonUVsExtruded;
                }

                //��� ������ UV2-��������� � ������
                for (int a = 0; a < uVArray.Length; a++)
                {
                    //�������� UV4-����������
                    Vector4 uV4;
                    uV4.x = uVArray[a].x;
                    uV4.y = uVArray[a].y;
                    uV4.z = 0;
                    uV4.w = pR.ProvinceHeight;

                    //������� ���������� � ������ ���������
                    chunkUV[pHS.parentChunkStart + a] = uV4;
                }
                #endregion

                #region EdgesMesh
                //���� ������ ����� ������ ������, � ������� ������������� ���������
                List<Vector2> thinEdgesChunkUVs = hexasphereData.Value.thinEdgesChunkUVs[pHS.parentThinEdgesChunkIndex];

                //��� ������ ������� ��������� � ����� ������ ������
                for(int a = 0; a < pHS.parentThinEdgesChunkLength; a++)
                {
                    //���� UV2-���������� �������
                    Vector2 uv = thinEdgesChunkUVs[pHS.parentThinEdgesChunkStart + a];

                    //��������� ������ ���������
                    uv.y = pR.ProvinceHeight;

                    //������� ���������� � ������
                    thinEdgesChunkUVs[pHS.parentThinEdgesChunkStart + a] = uv;
                }

                //���� ������ ����� ������� ������, � ������� ������������� ���������
                List<Vector2> thickEdgesChunkUVs = hexasphereData.Value.thickEdgesChunkUVs[pHS.parentThickEdgesChunkIndex];

                //��� ������ ������� ��������� � ����� ������ ������
                for (int a = 0; a < pHS.parentThickEdgesChunkLength; a++)
                {
                    //���� UV2-���������� �������
                    Vector2 uv = thickEdgesChunkUVs[pHS.parentThickEdgesChunkStart + a];

                    //��������� ������ ���������
                    uv.y = pR.ProvinceHeight;

                    //������� ���������� � ������
                    thickEdgesChunkUVs[pHS.parentThickEdgesChunkStart + a] = uv;
                }
                #endregion
            }

            //��� ������� ���������� �����
            for (int a = 0; a < hexasphereData.Value.chunkMeshFilters.Length; a++)
            {
                //��������� ��� UV
                hexasphereData.Value.chunkMeshes[a].SetUVs(0, hexasphereData.Value.chunksUV[a]);

                hexasphereData.Value.chunkMeshFilters[a].sharedMesh = hexasphereData.Value.chunkMeshes[a];

                //��������� ������������ ��������
                hexasphereData.Value.chunkMeshRenderers[a].sharedMaterial = hexasphereData.Value.provinceMaterial;
            }

            //��� ������� ���������� ������ ������
            for(int a = 0;a < hexasphereData.Value.thinEdgesChunkMeshFilters.Length; a++)
            {
                //��������� ��� UV
                hexasphereData.Value.thinEdgesChunkMeshes[a].SetUVs(0, hexasphereData.Value.thinEdgesChunkUVs[a]);

                hexasphereData.Value.thinEdgesChunkMeshFilters[a].sharedMesh = hexasphereData.Value.thinEdgesChunkMeshes[a];

                //��������� ������������ ��������
                hexasphereData.Value.thinEdgesChunkMeshRenderers[a].sharedMaterial = hexasphereData.Value.thinEdgesMaterial;
            }

            //��� ������� ���������� ������� ������
            for (int a = 0; a < hexasphereData.Value.thickEdgesChunkMeshFilters.Length; a++)
            {
                //��������� ��� UV
                hexasphereData.Value.thickEdgesChunkMeshes[a].SetUVs(0, hexasphereData.Value.thickEdgesChunkUVs[a]);

                hexasphereData.Value.thickEdgesChunkMeshFilters[a].sharedMesh = hexasphereData.Value.thickEdgesChunkMeshes[a];

                //��������� ������������ ��������
                hexasphereData.Value.thickEdgesChunkMeshRenderers[a].sharedMaterial = hexasphereData.Value.thickEdgesMaterial;
            }
        }

        void ProvinceColorsUpdate(
            ref CMapModeCore mapMode)
        {
            //��� ������ ���������
            foreach (int provinceEntity in pRFilter.Value)
            {
                //���� ���������� ���������
                ref CProvinceRender pR = ref pRPool.Value.Get(provinceEntity);
                ref CProvinceHexasphere pHS = ref pHSPool.Value.Get(provinceEntity);

                //���� ������� �����, � ������� ������������� ���������
                ref Color32[] chunkColors = ref hexasphereData.Value.chunksColors[pHS.parentChunkIndex];

                //���������� ���������� ������ ���������
                int provinceVertexCount = pHS.vertexPoints.Length + 2;

                //��� ������ UV2-��������� � ������
                for (int a = 0; a < provinceVertexCount; a++)
                {
                    //������� ���� � ������ ������
                    chunkColors[pHS.parentChunkStart + a] = mapMode.GetProvinceColor(ref pR);
                }
            }

            //��� ������� ���������� �����
            for (int a = 0; a < hexasphereData.Value.chunkMeshFilters.Length; a++)
            {
                //��������� ��� �������
                hexasphereData.Value.chunkMeshes[a].SetColors(hexasphereData.Value.chunksColors[a]);

                hexasphereData.Value.chunkMeshFilters[a].sharedMesh = hexasphereData.Value.chunkMeshes[a];

                //��������� ������������ ��������
                hexasphereData.Value.chunkMeshRenderers[a].sharedMaterial = hexasphereData.Value.provinceMaterial;
            }
        }

        GameObject MapCreateGOAndParent(
            Transform parent,
            string name)
        {
            //������ GO
            GameObject gO = new GameObject(name);

            //��������� �������� ������ GO
            gO.layer = parent.gameObject.layer;
            gO.transform.SetParent(parent, false);
            gO.transform.localPosition = Vector3.zero;
            gO.transform.localScale = Vector3.one;
            gO.transform.localRotation = Quaternion.Euler(0, 0, 0);

            return gO;
        }

        void MeshRenderersShadowSupportUpdate() 
        {
            //��� ������� ������������ ���������
            for (int a = 0; a < hexasphereData.Value.chunkMeshRenderers.Length; a++)
            {
                //���� �������� �� ���� � ��� ��� �����
                if (hexasphereData.Value.chunkMeshRenderers[a] != null
                    && hexasphereData.Value.chunkMeshRenderers[a].name.Equals(HexasphereData.chunkGOName))
                {
                    //����������� ������������ �������� � ������������ �����
                    hexasphereData.Value.chunkMeshRenderers[a].receiveShadows = true;
                    hexasphereData.Value.chunkMeshRenderers[a].shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
                }
            }
        }

        void MapUpdateLightingMode()
        {
            //��������� ���������
            MapUpdateLightingMaterial(hexasphereData.Value.provinceMaterial);
            MapUpdateLightingMaterial(hexasphereData.Value.provinceColoredMaterial);

            hexasphereData.Value.provinceMaterial.EnableKeyword("HEXA_ALPHA");
        }

        void MapUpdateLightingMaterial(
            Material material)
        {
            //��������� ������ ���������
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
            //���������� ������ ��������
            const int textureSize = 256;

            //���� �������� ����������� ��� � ������ �������
            if (hexasphereData.Value.bevelNormals == null || hexasphereData.Value.bevelNormals.width != textureSize)
            {
                //������ ����� ��������
                hexasphereData.Value.bevelNormals = new(
                    textureSize, textureSize,
                    TextureFormat.ARGB32,
                    false);
            }

            //���������� ������ ��������
            int textureHeight = hexasphereData.Value.bevelNormals.height;
            int textureWidth = hexasphereData.Value.bevelNormals.width;

            //���� ������ ������ �������� ����������� ��� ��� ����� �� ������������� ��������
            if (hexasphereData.Value.bevelNormalsColors == null || hexasphereData.Value.bevelNormalsColors.Length != textureHeight * textureWidth)
            {
                //������ ����� ������
                hexasphereData.Value.bevelNormalsColors = new Color[textureHeight * textureWidth];
            }

            //������ ��������� ��� ��������� �������
            Vector2 texturePixel;

            //���������� ������ ����� � ������� ������
            const float bevelWidth = 0.05f;
            float bevelWidthSqr = bevelWidth * bevelWidth;

            //��� ������� ������� �� ������
            for (int y = 0, index = 0; y < textureHeight; y++)
            {
                //���������� ��� ��������� �� Y
                texturePixel.y = (float)y / textureHeight;

                //��� ������� ������� �� ������
                for (int x = 0; x < textureWidth; x++)
                {
                    //���������� ��� ��������� �� X
                    texturePixel.x = (float)x / textureWidth;

                    //�������� R-���������
                    hexasphereData.Value.bevelNormalsColors[index].r = 0f;

                    //���������� ���������� �� ������� �������
                    float minDistSqr = float.MaxValue;

                    //��� ������� ����� ��������������
                    for (int a = 0; a < 6; a++)
                    {
                        //���� ������� ������
                        Vector2 t0 = hexasphereData.Value.hexagonUVsExtruded[a];
                        Vector2 t1 = a < 5 ? hexasphereData.Value.hexagonUVsExtruded[a + 1] : hexasphereData.Value.hexagonUVsExtruded[0];

                        //���������� ����� �����
                        float l2 = Vector2.SqrMagnitude(t0 - t1);
                        //
                        float t = Mathf.Max(0, Mathf.Min(1, Vector2.Dot(texturePixel - t0, t1 - t0) / l2));
                        //
                        Vector2 projection = t0 + t * (t1 - t0);
                        //
                        float distSqr = Vector2.SqrMagnitude(texturePixel - projection);

                        //���� ���������� ������ ������������, �� ��������� �����������
                        if (distSqr < minDistSqr)
                        {
                            minDistSqr = distSqr;
                        }
                    }

                    //���������� ��������
                    float f = minDistSqr / bevelWidthSqr;
                    //���� �������� ������ �������, ������������ ���
                    if (f > 1f)
                    {
                        f = 1f;
                    }

                    //��������� R-���������
                    hexasphereData.Value.bevelNormalsColors[index].r = f;

                    //����������� ������ �������
                    index++;
                }
            }

            //����� ������ �������� ��������
            hexasphereData.Value.bevelNormals.SetPixels(hexasphereData.Value.bevelNormalsColors);

            //��������� ��������
            hexasphereData.Value.bevelNormals.Apply();

            //����� �������� ���������
            hexasphereData.Value.provinceMaterial.SetTexture("_BumpMask", hexasphereData.Value.bevelNormals);
        }

        void HighlightMaterialUpdate()
        {
            //��������� ������� ���������� ���������
            hexasphereData.Value.hoverProvinceHighlightMaterial.SetFloat(
                ShaderParameters.ColorShift, Mathf.PingPong(Time.time * 0.25f, 1f));
            hexasphereData.Value.currentProvinceHighlightMaterial.SetFloat(
                ShaderParameters.ColorShift, Mathf.PingPong(Time.time * 0.25f, 1f));
        }

        readonly EcsPoolInject<CProvinceHoverHighlight> provinceHoverHighlightPool = default;
        readonly EcsPoolInject<SRShowMapHoverHighlight> showMapHoverHighlightPool = default;
        void HoverHighlight()
        {
            //��������� ���������, ��� ��� �� ���������
            HoverHighlightDeactivation();

            //��������� ���������, ���� ��� ��� �������� ���, ��� ���������
            HoverHighlightUpdate();

            //���������� ���������, ��� ���������
            HoverHighlightActivation();
        }

        readonly EcsFilterInject<Inc<CProvinceHexasphere, CProvinceHoverHighlight>, Exc<SRShowMapHoverHighlight>> provinceHoverHighlightDeactivationFilter = default;
        void HoverHighlightDeactivation()
        {
            //��� ������ ���������, ������� ��������� ��������� � �� ������� �������
            foreach (int provinceEntity in provinceHoverHighlightDeactivationFilter.Value)
            {
                //���� ��������� ���������
                ref CProvinceHoverHighlight pHoverHighlight = ref provinceHoverHighlightPool.Value.Get(provinceEntity);

                //�������� ���������
                GOProvinceHighlight.CacheProvinceHighlight(ref pHoverHighlight);

                //������� ��������� ���������
                provinceHoverHighlightPool.Value.Del(provinceEntity);
            }
        }

        readonly EcsFilterInject<Inc<CProvinceHexasphere, CProvinceHoverHighlight, SRShowMapHoverHighlight>> provinceHoverHighlightUpdateFilter = default;
        void HoverHighlightUpdate()
        {
            //��� ������ ���������, ������� ��������� ��������� � ������
            foreach (int provinceEntity in provinceHoverHighlightUpdateFilter.Value)
            {
                //���� ��������� 
                ref CProvinceRender pR = ref pRPool.Value.Get(provinceEntity);
                ref CProvinceHexasphere pHS = ref pHSPool.Value.Get(provinceEntity);
                ref CProvinceHoverHighlight pHoverHighlight = ref provinceHoverHighlightPool.Value.Get(provinceEntity);

                //��������� ���������
                GOProvinceHighlight.UpdateProvinceHighlight(
                    ref pR,
                    pHoverHighlight.highlight,
                    hexasphereData.Value.hoverProvinceHighlightMaterial);

                //������� ������
                showMapHoverHighlightPool.Value.Del(provinceEntity);
            }
        }

        readonly EcsFilterInject<Inc<CProvinceHexasphere, SRShowMapHoverHighlight>> provinceHoverHighlightActivationFilter = default;
        void HoverHighlightActivation()
        {
            //��� ������ ���������, ������� ������
            foreach (int provinceEntity in provinceHoverHighlightActivationFilter.Value)
            {
                //���� ���������
                ref CProvinceRender pR = ref pRPool.Value.Get(provinceEntity);
                ref CProvinceHexasphere pHS = ref pHSPool.Value.Get(provinceEntity);

                //���� ��������� �� ����� GO
                if (pR.ProvinceGO == null)
                {
                    //������ ���
                    GOProvince.InstantiateProvinceGO(
                        HexasphereData.provincesRootGO,
                        ref pR);
                }

                //��������� �� ��������� ���������
                ref CProvinceHoverHighlight pHoverHighlight = ref provinceHoverHighlightPool.Value.Add(provinceEntity);

                //������ ���������
                GOProvinceHighlight.InstantiateProvinceHighlight(
                    ref pR, ref pHoverHighlight,
                    hexasphereData.Value.hoverProvinceHighlightMaterial);

                //������ ��� ���������
                ProvinceHighlightMeshCreation(
                    ref pR, ref pHS,
                    pHoverHighlight.highlight);

                //������� ������
                showMapHoverHighlightPool.Value.Del(provinceEntity);
            }
        }

        void ProvinceHighlightMeshCreation(
            ref CProvinceRender pR, ref CProvinceHexasphere pHS,
            GOProvinceHighlight provinceHighlight)
        {
            //������ ����� ���
            Mesh mesh = new();
            mesh.hideFlags = HideFlags.DontSave;

            //���������� ������ ���������
            float extrusion = pR.ProvinceHeight * HexasphereData.ExtrudeMultiplier;

            //������������ ��������� ������
            Vector3[] extrudedVertices = new Vector3[pHS.vertices.Length];
            for (int a = 0; a < pHS.vertices.Length; a++)
            {
                extrudedVertices[a] = pHS.vertices[a] * (1f + extrusion);
            }

            //��������� ������� ����
            mesh.SetVertices(extrudedVertices);

            //���� � ��������� ����� ������
            if (pHS.vertices.Length == 6)
            {
                mesh.SetIndices(
                    HexasphereData.hexagonIndices,
                    MeshTopology.Triangles,
                    0,
                    false);
                mesh.uv = HexasphereData.hexagonUVs;
            }
            //�����
            else
            {
                mesh.SetIndices(
                    HexasphereData.pentagonIndices,
                    MeshTopology.Triangles,
                    0,
                    false);
                mesh.uv = HexasphereData.pentagonUVs;
            }

            //������������ ������� ����
            mesh.SetNormals(pHS.vertices);
            mesh.RecalculateNormals();

            //��������� ��� ��������� � �������� ������������
            provinceHighlight.meshFilter.sharedMesh = mesh;
            provinceHighlight.meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            provinceHighlight.meshRenderer.enabled = true;
        }

        readonly EcsFilterInject<Inc<CProvinceRender, CProvinceHexasphere, CProvinceHoverHighlight>> provinceHoverHighlightMeshUpdateFilter = default;
        void ProvinceHighlightMeshesUpdate()
        {
            //��� ������ ��������� � ����������� ��������� ���������
            foreach (int provinceEntity in provinceHoverHighlightMeshUpdateFilter.Value)
            {
                //���� ���������
                ref CProvinceRender pR = ref pRPool.Value.Get(provinceEntity);
                ref CProvinceHexasphere pHS = ref pHSPool.Value.Get(provinceEntity);
                ref CProvinceHoverHighlight pHoverHighlight = ref provinceHoverHighlightPool.Value.Get(provinceEntity);

                //��������� ��� ���������
                ProvinceHighlightMeshUpdate(
                    ref pR, ref pHS,
                    pHoverHighlight.highlight);
            }
        }

        void ProvinceHighlightMeshUpdate(
            ref CProvinceRender pR, ref CProvinceHexasphere pHS,
            GOProvinceHighlight provinceHighlight)
        {
            //���� ��� ���������
            Mesh mesh = provinceHighlight.meshFilter.sharedMesh;

            //���������� ������ ���������
            float extrusion = pR.ProvinceHeight * HexasphereData.ExtrudeMultiplier;

            //������������ ����� ��������� ������
            Vector3[] extrudedVertices = new Vector3[pHS.vertices.Length];
            for (int a = 0; a < pHS.vertices.Length; a++)
            {
                extrudedVertices[a] = pHS.vertices[a] * (1f + extrusion);
            }

            //��������� ������� ����
            mesh.SetVertices(extrudedVertices);

            //������������ ������� ����
            mesh.SetNormals(pHS.vertices);
            mesh.RecalculateNormals();

            //��������� ��� ����������
            provinceHighlight.meshFilter.sharedMesh = null;
            provinceHighlight.meshFilter.sharedMesh = mesh;
        }

        void MapPanels()
        {
            //�������� ������������ ������� ������� �����
            SetParentProvinceMapPanels();

            //���������, ��� �� ��������� ��� ������� �����, �� � �����������
            ProvinceMapPanelsEmptyCheck();

            //������������ ������ �����
            ProvinceMapPanelsUpdate();
        }

        readonly EcsFilterInject<Inc<RProvinceMapPanelSetParent>> provinceMapPanelSetParentRFilter = default;
        readonly EcsPoolInject<RProvinceMapPanelSetParent> provinceMapPanelSetParentRPool = default;
        void SetParentProvinceMapPanels()
        {
            //��� ������� ������� ��������� �������� ������ �����
            foreach (int requestEntity in provinceMapPanelSetParentRFilter.Value)
            {
                //���� ������
                ref RProvinceMapPanelSetParent requestComp = ref provinceMapPanelSetParentRPool.Value.Get(requestEntity);

                //�������� �������� ������ �����
                ProvinceMapPanelSetParent(ref requestComp);

                //������� ������
                provinceMapPanelSetParentRPool.Value.Del(requestEntity);
            }
        }

        void ProvinceMapPanelSetParent(
            ref RProvinceMapPanelSetParent requestComp)
        {
            //���� ������� ���������
            requestComp.parentProvincePE.Unpack(world.Value, out int provinceEntity);
            ref CProvinceRender pR = ref pRPool.Value.Get(provinceEntity);
            ref CProvinceHexasphere pHS = ref pHSPool.Value.Get(provinceEntity);

            //���� ��������� �� ����� GO
            if (pR.ProvinceGO == null)
            {
                //������ ���
                GOProvince.InstantiateProvinceGO(
                    HexasphereData.provincesRootGO,
                    ref pR);
            }

            //���� ��������� �� ����� ���������� ������� �����
            if(pMPPool.Value.Has(provinceEntity) == false)
            {
                //��������� ���������
                ProvinceMapPanelsCreate(
                    provinceEntity,
                    ref pR, ref pHS);
            }

            //���� ��������� ������� �����
            ref CProvinceMapPanels pMP = ref pMPPool.Value.Get(provinceEntity);

            //������������ ���������� ������ � ������ ������� ���������
            requestComp.mapPanelGO.transform.SetParent(pMP.mapPanelGroup.transform);
            requestComp.mapPanelGO.transform.localPosition = Vector3.zero;
        }

        void ProvinceMapPanelsCreate(
            int provinceEntity,
            ref CProvinceRender pR, ref CProvinceHexasphere pHS)
        {
            //��������� �������� ��������� ��������� ������� �����
            ref CProvinceMapPanels pMP = ref pMPPool.Value.Add(provinceEntity);

            //��������� ������ ����������
            pMP = new(0);

            //���������� ����� ���������
            Vector3 provinceCenter = pHS.center;
            provinceCenter *= 1.0f + pR.ProvinceHeight * HexasphereData.ExtrudeMultiplier;
            provinceCenter *= hexasphereData.Value.hexasphereScale;

            //������ ������ ������� �����
            CProvinceMapPanels.InstantiateMapPanelGroup(
                ref pR, ref pMP,
                provinceCenter,
                provinceData.Value.mapPanelAltitude);
        }

        readonly EcsFilterInject<Inc<CProvinceRender, CProvinceMapPanels>> pMPFilter = default;
        void ProvinceMapPanelsEmptyCheck()
        {
            //��� ������ ��������� � ����������� ������� �����
            foreach(int provinceEntity in pMPFilter.Value)
            {
                //���� ��������� ������� �����
                ref CProvinceMapPanels pMP = ref pMPPool.Value.Get(provinceEntity);

                //���� ��������� �� ����� ������� �����
                if(pMP.mapPanelGroup.transform.childCount == 0)
                {
                    //�������� ������ �������
                    CProvinceMapPanels.CacheMapPanelGroup(ref pMP);

                    //������� � �������� ��������� ������� �����
                    pMPPool.Value.Del(provinceEntity);
                }
            }
        }

        void ProvinceMapPanelsAltitudeUpdate()
        {
            //��� ������ ��������� � ����������� ������� �����
            foreach (int provinceEntity in pMPFilter.Value)
            {
                //���� ���������� ���������
                ref CProvinceRender pR = ref pRPool.Value.Get(provinceEntity);
                ref CProvinceHexasphere pHS = ref pHSPool.Value.Get(provinceEntity);
                ref CProvinceMapPanels pMP = ref pMPPool.Value.Get(provinceEntity);

                //���������� ��������� ������ ���������
                Vector3 provinceCenter = pHS.center;
                provinceCenter *= 1.0f + pR.ProvinceHeight * HexasphereData.ExtrudeMultiplier;
                provinceCenter *= hexasphereData.Value.hexasphereScale;

                //����� ��������� ������ �������
                CProvinceMapPanels.CalculateMapPanelGroupPosition(
                    ref pMP,
                    provinceCenter,
                    provinceData.Value.mapPanelAltitude);
            }
        }

        void ProvinceMapPanelsUpdate()
        {
            //��� ������ ��������� � ����������� ������� �����
            foreach (int provinceEntity in pMPFilter.Value)
            {
                //���� ��������� ������� �����
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
            //��� ������ ��������� � PR
            foreach (int provinceEntity in pRFilter.Value)
            {
                //���� PR
                ref CProvinceRender pR = ref pRPool.Value.Get(provinceEntity);

                //���� GO ��������� �� ����, �� �� ����� �������� ��������
                if (pR.ProvinceGO != null
                    && pR.ProvinceGO.transform.childCount == 0)
                {
                    //�������� ���
                    GOProvince.CacheProvinceGO(ref pR);
                }
            }
        }
    }
}
