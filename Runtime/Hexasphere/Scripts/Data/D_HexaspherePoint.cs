
using System;
using System.Collections.Generic;

using UnityEngine;

namespace HS.Hexasphere
{
    public class D_HexaspherePoint : IEqualityComparer<D_HexaspherePoint>, IEquatable<D_HexaspherePoint>
    {
        public D_HexaspherePoint(
            float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public float x;
        public float y;
        public float z;

        public float Elevation
        {
            get
            {
                return elevation;
            }
            set
            {
                if (elevation != value)
                {
                    elevation = value;

                    isProjectedVector3 = false;
                }
            }
        }
        float elevation;

        public Vector3 ProjectedVector3
        {
            get
            {
                //Если вектор рассчитан, то возвращаем его
                if (isProjectedVector3 == true)
                {
                    return projectedVector3;
                }
                //Иначе рассчитываем его и возвращаем
                else
                {
                    CalculateProjectedVertex();

                    return projectedVector3;
                }
            }
        }
        Vector3 projectedVector3;
        bool isProjectedVector3 = false;

        public D_HexasphereTriangle[] triangles;
        public int triangleCount;
        public int provinceEntity;

        public static int flag = 0;

        int hashCode;

        void CalculateProjectedVertex()
        {
            double len = 2.0 * Math.Sqrt(x * (double)x + y * (double)y + z * (double)z);
            len /= 1.0 + elevation;

            double xx = x / len;
            double yy = y / len;
            double zz = z / len;

            projectedVector3 = new Vector3((float)xx, (float)yy, (float)zz);

            isProjectedVector3 = true;
        }

        public void RegisterTriangle(
            D_HexasphereTriangle triangle)
        {
            //Если массив треугольников пуст, создаём новый на шесть треугольников
            if (triangles == null)
            {
                triangles = new D_HexasphereTriangle[6];
            }

            //Заносим треугольник в массив на последнее свободное место
            triangles[triangleCount++] = triangle;
        }

        public int GetOrderedTriangles(D_HexasphereTriangle[] tempTriangles)
        {
            //Если количество треугольников равно нулю
            if (triangleCount == 0)
            {
                return 0;
            }

            tempTriangles[0] = triangles[0];

            //Создаём счётчик треугольников
            int count = 1;

            flag++;

            //Для каждого треугольника, кроме последнего
            for (int a = 0; a < triangleCount - 1; a++)
            {
                //Для каждого треугольника
                for (int b = 1; b < triangleCount; b++)
                {
                    //Если флаг треугольника не равен текущему флагу точки
                    if (triangles[b].orderedFlag != flag
                        //Если текущий временный треугольник не пуст
                        && tempTriangles[a] != null
                        //Если треугольники соседний
                        && triangles[b].IsAdjacentTo(tempTriangles[a]))
                    {
                        //Заносим треугольник в список временных и увеличиваем счётчик
                        tempTriangles[count++] = triangles[b];

                        //Обновляем флаг треугольника
                        triangles[b].orderedFlag = flag;

                        //Выходим из цикла
                        break;
                    }
                }
            }

            //Возвращаем количество треугольников
            return count;
        }

        public override string ToString()
        {
            return (int)(x * 100f) / 100f + "," + (int)(y * 100f) / 100f + "," + (int)(z * 100f) / 100f;

        }

        public override bool Equals(object obj)
        {
            if (obj is D_HexaspherePoint)
            {
                D_HexaspherePoint other = (D_HexaspherePoint)obj;
                return x == other.x && y == other.y && z == other.z;
            }
            return false;
        }

        public bool Equals(D_HexaspherePoint p2)
        {
            return x == p2.x && y == p2.y && z == p2.z;
        }

        public bool Equals(D_HexaspherePoint p1, D_HexaspherePoint p2)
        {
            return p1.x == p2.x && p1.y == p2.y && p1.z == p2.z;
        }

        public override int GetHashCode()
        {
            if (hashCode == 0)
            {
                hashCode = x.GetHashCode() ^ y.GetHashCode() << 2 ^ z.GetHashCode() >> 2;
            }
            return hashCode;
        }

        public int GetHashCode(D_HexaspherePoint p)
        {
            if (hashCode == 0)
            {
                hashCode = p.x.GetHashCode() ^ p.y.GetHashCode() << 2 ^ p.z.GetHashCode() >> 2;
            }
            return hashCode;
        }

        public static explicit operator Vector3(D_HexaspherePoint point)
        {
            return new Vector3(point.x, point.y, point.z);
        }
    }
}
