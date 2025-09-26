
using System.Collections.Generic;

using UnityEngine;

namespace HS
{
    public class HexasphereData : MonoBehaviour
    {
        //Константы
        internal const float PHI = 1.61803399f;

        internal const string chunksRootGOName = "ChunksRoot";
        internal static GameObject chunksRootGO;
        internal const string chunkGOName = "Chunk";
        internal const string provincesRootGOName = "ProvincesRoot";
        internal static GameObject provincesRootGO;

        internal const int maxVertexCountPerChunk = 65500;
        internal const int maxVertexArraySize = 65530;

        internal static readonly int[] hexagonIndices = new int[] {
            0, 1, 5,
            1, 2, 5,
            4, 5, 2,
            3, 4, 2
        };
        internal readonly int[] hexagonIndicesExtruded = new int[]
        {
            0, 1, 6,
            5, 0, 6,
            1, 2, 5,
            4, 5, 2,
            2, 3, 7,
            3, 4, 7
        };
        internal static readonly Vector2[] hexagonUVs = new Vector2[] {
            new Vector2 (0, 0.5f),
            new Vector2 (0.25f, 1f),
            new Vector2 (0.75f, 1f),
            new Vector2 (1f, 0.5f),
            new Vector2 (0.75f, 0f),
            new Vector2 (0.25f, 0f)
        };
        internal readonly Vector2[] hexagonUVsExtruded = new Vector2[]
        {
            new Vector2 (0, 0.5f),
            new Vector2 (0.25f, 1f),
            new Vector2 (0.75f, 1f),
            new Vector2 (1f, 0.5f),
            new Vector2 (0.75f, 0f),
            new Vector2 (0.25f, 0f),
            new Vector2 (0.25f, 0.5f),
            new Vector2 (0.75f, 0.5f)
        };

        internal static readonly int[] pentagonIndices = new int[] {
            0, 1, 4,
            1, 2, 4,
            3, 4, 2
        };
        internal readonly int[] pentagonIndicesExtruded = new int[]
        {
            0, 1, 5,
            4, 0, 5,
            1, 2, 4,
            2, 3, 6,
            3, 4, 6
        };
        internal static readonly Vector2[] pentagonUVs = new Vector2[] {
            new Vector2 (0, 0.33f),
            new Vector2 (0.25f, 1f),
            new Vector2 (0.75f, 1f),
            new Vector2 (1f, 0.33f),
            new Vector2 (0.5f, 0f),
        };
        internal readonly Vector2[] pentagonUVsExtruded = new Vector2[]
        {
            new Vector2 (0, 0.33f),
            new Vector2 (0.25f, 1f),
            new Vector2 (0.75f, 1f),
            new Vector2 (1f, 0.33f),
            new Vector2 (0.5f, 0f),
            new Vector2 (0.375f, 0.5f),
            new Vector2 (0.625f, 0.5f)
        };

        //Переменные для генерации
        internal int subdivisions;
        public float hexasphereScale;

        internal readonly Dictionary<DHexaspherePoint, DHexaspherePoint> points = new();

        //Переменные для визуализации
        public static float ExtrudeMultiplier;

        internal Vector3[][] chunksVertices;
        internal int[][] chunksIndices;
        internal Vector4[][] chunksUV2;
        internal Vector4[][] chunksUV;
        internal Color32[][] chunksColors;

        internal MeshFilter[] chunkMeshFilters;
        internal Mesh[] chunkMeshes;
        internal MeshRenderer[] chunkMeshRenderers;

        internal static List<T> CheckList<T>(ref List<T> list)
        {
            if(list == null)
            {
                list = new List<T>(maxVertexArraySize);
            }
            else
            {
                list.Clear();
            }

            return list;
        }

        #region ThinEdges
        internal const string thinEdgesRootGOName = "ThinEdgesRoot";
        internal static GameObject thinEdgesRootGO;
        internal const string thinEdgeChunkGOName = "ThinEdgesChunk";

        internal List<Vector3>[] thinEdgesChunkVertices = new List<Vector3>[0];
        internal List<int>[] thinEdgesChunkIndices = new List<int>[0];
        internal List<Vector2>[] thinEdgesChunkUVs = new List<Vector2>[0];
        internal List<Color32>[] thinEdgesChunkColors = new List<Color32>[0];

        internal MeshFilter[] thinEdgesChunkMeshFilters = new MeshFilter[0];
        internal Mesh[] thinEdgesChunkMeshes = new Mesh[0];
        internal MeshRenderer[] thinEdgesChunkMeshRenderers = new MeshRenderer[0];

        internal Material thinEdgesMaterial;
        internal Color thinEdgesColor;
        internal float thinEdgesColorIntensity;
        #endregion

        #region ThickEdges
        internal const string thickEdgesRootGOName = "ThickEdgesRoot";
        internal static GameObject thickEdgesRootGO;
        internal const string thickEdgeChunkGOName = "ThickEdgesChunk";

        internal List<Vector3>[] thickEdgesChunkVertices = new List<Vector3>[0];
        internal List<int>[] thickEdgesChunkIndices = new List<int>[0];
        internal List<Vector2>[] thickEdgesChunkUVs = new List<Vector2>[0];
        internal List<Color32>[] thickEdgesChunkColors = new List<Color32>[0];

        internal MeshFilter[] thickEdgesChunkMeshFilters = new MeshFilter[0];
        internal Mesh[] thickEdgesChunkMeshes = new Mesh[0];
        internal MeshRenderer[] thickEdgesChunkMeshRenderers = new MeshRenderer[0];

        internal Material thickEdgesMaterial;
        internal Color thickEdgesColor;
        internal float thickEdgesColorIntensity;
        #endregion

        //Объекты
        public static GameObject HexasphereGO;
        internal static SphereCollider HexasphereCollider;

        //Материалы
        internal Material provinceMaterial;
        internal Material provinceColoredMaterial;

        internal Color defaultShadedColor = new Color(0.56f, 0.71f, 0.54f);

        internal float gradientIntensity;
        internal Color tileTintColor;
        internal Color ambientColor;
        internal float minimumLight;

        internal Texture2D bevelNormals;
        internal Color[] bevelNormalsColors;

        internal Material hoverProvinceHighlightMaterial;
        internal Material currentProvinceHighlightMaterial;
    }
}
