
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
            D_HexaspherePoint centerPoint)
        {
            this.centerPoint = centerPoint;
            center = this.centerPoint.ProjectedVector3;
            this.centerPoint.provinceEntity = selfEntity;

            int facesCount = centerPoint.GetOrderedTriangles(tempTriangles);
            vertexPoints = new D_HexaspherePoint[facesCount];
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
                    D_HexaspherePoint aux;
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
                    D_HexaspherePoint aux;
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
        }

        static readonly D_HexasphereTriangle[] tempTriangles = new D_HexasphereTriangle[20];

        public D_HexaspherePoint centerPoint;
        public Vector3 center;
        public D_HexaspherePoint[] vertexPoints;
        public Vector3[] vertices;
    }
}
