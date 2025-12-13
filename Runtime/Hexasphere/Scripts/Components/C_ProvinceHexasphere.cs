
using UnityEngine;

using Leopotam.EcsLite;

namespace HS.Hexasphere
{
    /// <summary>
    /// Компонент, хранящий данные провинции, непосредственно связанные
    /// </summary>
    public struct C_ProvinceHexasphere
    {
        public C_ProvinceHexasphere(
            int selfEntity,
            DHexaspherePoint centerPoint)
        {
            this.centerPoint = centerPoint;
            center = this.centerPoint.ProjectedVector3;
            this.centerPoint.provinceEntity = selfEntity;

            int facesCount = centerPoint.GetOrderedTriangles(tempTriangles);
            vertexPoints = new DHexaspherePoint[facesCount];
            //Для каждой вершины
            for (int a = 0; a < facesCount; a++)
            {
                vertexPoints[a] = tempTriangles[a].GetCentroid();
            }
            //Переупорядочиваем, если неверный порядок
            if (facesCount == 6)
            {
                Vector3 p0 = (Vector3)vertexPoints[0];
                Vector3 p1 = (Vector3)vertexPoints[1];
                Vector3 p5 = (Vector3)vertexPoints[5];
                Vector3 v0 = p1 - p0;
                Vector3 v1 = p5 - p0;
                Vector3 cp = Vector3.Cross(v0, v1);
                float dp = Vector3.Dot(cp, p1);
                if (dp < 0)
                {
                    DHexaspherePoint aux;
                    aux = vertexPoints[0];
                    vertexPoints[0] = vertexPoints[5];
                    vertexPoints[5] = aux;
                    aux = vertexPoints[1];
                    vertexPoints[1] = vertexPoints[4];
                    vertexPoints[4] = aux;
                    aux = vertexPoints[2];
                    vertexPoints[2] = vertexPoints[3];
                    vertexPoints[3] = aux;
                }
            }
            else if (facesCount == 5)
            {
                Vector3 p0 = (Vector3)vertexPoints[0];
                Vector3 p1 = (Vector3)vertexPoints[1];
                Vector3 p4 = (Vector3)vertexPoints[4];
                Vector3 v0 = p1 - p0;
                Vector3 v1 = p4 - p0;
                Vector3 cp = Vector3.Cross(v0, v1);
                float dp = Vector3.Dot(cp, p1);
                if (dp < 0)
                {
                    DHexaspherePoint aux;
                    aux = vertexPoints[0];
                    vertexPoints[0] = vertexPoints[4];
                    vertexPoints[4] = aux;
                    aux = vertexPoints[1];
                    vertexPoints[1] = vertexPoints[3];
                    vertexPoints[3] = aux;
                }
            }

            vertices = new Vector3[vertexPoints.Length];
            //Для каждой вершины
            for (int a = 0; a < vertexPoints.Length; a++)
            {
                vertices[a] = vertexPoints[a].ProjectedVector3;
            }

            parentChunkIndex = 0;
            parentChunkStart = 0;
            parentChunkTriangleStart = 0;
            parentChunkLength = 0;

            thinEdges = 63;
            parentThinEdgesChunkIndex = 0;
            parentThinEdgesChunkStart = 0;
            parentThinEdgesChunkLength = 0;

            thickEdges = 63;
            parentThickEdgesChunkIndex = 0;
            parentThickEdgesChunkStart = 0;
            parentThickEdgesChunkLength = 0;
        }

        static readonly DHexasphereTriangle[] tempTriangles = new DHexasphereTriangle[20];

        public DHexaspherePoint centerPoint;
        public Vector3 center;
        public DHexaspherePoint[] vertexPoints;
        public Vector3[] vertices;

        public int parentChunkIndex;
        public int parentChunkStart;
        public int parentChunkTriangleStart;
        public int parentChunkLength;

        public int thinEdges;
        public int parentThinEdgesChunkIndex;
        public int parentThinEdgesChunkStart;
        public int parentThinEdgesChunkLength;

        public int thickEdges;
        public int parentThickEdgesChunkIndex;
        public int parentThickEdgesChunkStart;
        public int parentThickEdgesChunkLength;
    }
}
