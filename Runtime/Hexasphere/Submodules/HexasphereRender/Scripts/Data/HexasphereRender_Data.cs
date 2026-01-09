using System.Collections.Generic;

using UnityEngine;

namespace HS.Hexasphere.Render
{
    public class HexasphereRender_Data : MonoBehaviour
    {
        #region Constants
        internal readonly int[] hexagonIndices = new int[] {
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
        internal readonly Vector2[] hexagonUVs = new Vector2[] {
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

        internal readonly int[] pentagonIndices = new int[] {
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
        internal readonly Vector2[] pentagonUVs = new Vector2[] {
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
        #endregion

        #region Sphere
        [Header("Sphere")]
        public GameObject HexasphereGO
        {
            get
            {
                return hexasphereGO;
            }
        }
        [SerializeField]
        private GameObject hexasphereGO;
        internal SphereCollider HexasphereCollider
        {
            get
            {
                return hexasphereCollider;
            }
        }
        [SerializeField]
        private SphereCollider hexasphereCollider;

        public float HexasphereScale
        {
            get
            {
                return hexasphereScale;
            }
        }
        [SerializeField]
        private float hexasphereScale;
        public float ExtrudeMultiplier
        {
            get
            {
                return extrudeMultiplier;
            }
        }
        [SerializeField]
        private float extrudeMultiplier;
        #endregion

        #region Provinces
        [Header("Province Materials")]
        [SerializeField]
        internal Material provinceMaterial;
        [SerializeField]
        internal Material provinceColoredMaterial;
        [SerializeField]
        internal Color defaultShadedColor = new Color(0.56f, 0.71f, 0.54f);

        [SerializeField]
        internal float gradientIntensity;
        [SerializeField]
        internal Color tileTintColor;
        [SerializeField]
        internal Color ambientColor;
        [SerializeField]
        internal float minimumLight;

        [SerializeField]
        internal Material hoverProvinceHighlightMaterial;
        [SerializeField]
        internal Material currentProvinceHighlightMaterial;

        internal string ChunksRootGOName
        {
            get
            {
                return chunksRootGOName;
            }
        }
        [SerializeField]
        private string chunksRootGOName;
        internal GameObject chunksRootGO;
        internal string ChunkGOName
        {
            get
            {
                return chunkGOName;
            }
        }
        [SerializeField]
        private string chunkGOName;
        internal string ProvincesRootGOName
        {
            get
            {
                return provincesRootGOName;
            }
        }
        [SerializeField]
        private string provincesRootGOName;
        internal GameObject provincesRootGO;

        internal const int maxVertexCountPerChunk = 65500;
        internal const int maxVertexArraySize = 65530;

        internal Vector3[][] chunksVertices;
        internal int[][] chunksIndices;
        internal Vector4[][] chunksUV2;
        internal Vector4[][] chunksUV;
        internal Color32[][] chunksColors;

        internal MeshFilter[] chunkMeshFilters;
        internal Mesh[] chunkMeshes;
        internal MeshRenderer[] chunkMeshRenderers;

        internal Texture2D bevelNormals;
        internal Color[] bevelNormalsColors;
        #endregion

        #region ThinEdges
        [Header("Thin Edges")]
        [SerializeField]
        internal Material thinEdgesMaterial;
        [SerializeField]
        internal Color thinEdgesColor;
        [SerializeField]
        [Range(0f, 2f)]
        internal float thinEdgesColorIntensity;

        internal string ThinEdgesRootGOName
        {
            get
            {
                return thinEdgesRootGOName;
            }
        }
        [SerializeField]
        private string thinEdgesRootGOName;
        internal GameObject thinEdgesRootGO;
        internal string ThinEdgeChunkGOName
        {
            get
            {
                return thinEdgeChunkGOName;
            }
        }
        [SerializeField]
        private string thinEdgeChunkGOName;

        internal List<Vector3>[] thinEdgesChunkVertices = new List<Vector3>[0];
        internal List<int>[] thinEdgesChunkIndices = new List<int>[0];
        internal List<Vector2>[] thinEdgesChunkUVs = new List<Vector2>[0];
        internal List<Color32>[] thinEdgesChunkColors = new List<Color32>[0];

        internal MeshFilter[] thinEdgesChunkMeshFilters = new MeshFilter[0];
        internal Mesh[] thinEdgesChunkMeshes = new Mesh[0];
        internal MeshRenderer[] thinEdgesChunkMeshRenderers = new MeshRenderer[0];
        #endregion

        #region ThickEdges
        [Header("Thick Edges")]
        [SerializeField]
        internal Material thickEdgesMaterial;
        [SerializeField]
        internal Color thickEdgesColor;
        [SerializeField]
        [Range(0f, 2f)]
        internal float thickEdgesColorIntensity;

        internal string ThickEdgesRootGOName
        {
            get
            {
                return thickEdgesRootGOName;
            }
        }
        [SerializeField]
        private string thickEdgesRootGOName;
        internal GameObject thickEdgesRootGO;
        internal string ThickEdgeChunkGOName
        {
            get
            {
                return thickEdgeChunkGOName;
            }
        }
        [SerializeField]
        private string thickEdgeChunkGOName;

        internal List<Vector3>[] thickEdgesChunkVertices = new List<Vector3>[0];
        internal List<int>[] thickEdgesChunkIndices = new List<int>[0];
        internal List<Vector2>[] thickEdgesChunkUVs = new List<Vector2>[0];
        internal List<Color32>[] thickEdgesChunkColors = new List<Color32>[0];

        internal MeshFilter[] thickEdgesChunkMeshFilters = new MeshFilter[0];
        internal Mesh[] thickEdgesChunkMeshes = new Mesh[0];
        internal MeshRenderer[] thickEdgesChunkMeshRenderers = new MeshRenderer[0];
        #endregion

        internal static List<T> List_Check<T>(ref List<T> list)
        {
            if (list == null)
            {
                list = new List<T>(maxVertexArraySize);
            }
            else
            {
                list.Clear();
            }

            return list;
        }
    }
}
