namespace HS.Hexasphere
{
    public class D_HexasphereTriangle
    {
        public D_HexasphereTriangle(
            D_HexaspherePoint point1, D_HexaspherePoint point2, D_HexaspherePoint point3,
            bool register = true)
        {
            points = new D_HexaspherePoint[]
            {
                point1,
                point2,
                point3
            };

            if (register)
            {
                point1.RegisterTriangle(this);
                point2.RegisterTriangle(this);
                point3.RegisterTriangle(this);
            }
        }

        public D_HexaspherePoint[] points;
        public int orderedFlag;

        D_HexaspherePoint centroid;
        bool isCentroid;

        public bool IsAdjacentTo(
            D_HexasphereTriangle triangle2)
        {
            bool match = false;

            //Для каждой точки
            for (int a = 0; a < 3; a++)
            {
                //Берём точку первого треугольника
                D_HexaspherePoint point1 = points[a];

                //Для каждой точки
                for (var b = 0; b < 3; b++)
                {
                    //Берём точку второго треугольника
                    D_HexaspherePoint point2 = triangle2.points[b];

                    //Если координаты совпадают
                    if (point1.x == point2.x && point1.y == point2.y && point1.z == point2.z)
                    {
                        //Если уже есть совпадение
                        if (match)
                        {
                            //То треугольники соседни
                            return true;
                        }

                        //Указываем, что есть совпадение
                        match = true;
                    }
                }
            }

            //Треугольники не соседни
            return false;
        }

        public D_HexaspherePoint GetCentroid()
        {
            if (isCentroid == true)
            {
                return centroid;
            }

            isCentroid = true;

            float x = (points[0].x + points[1].x + points[2].x) / 3;
            float y = (points[0].y + points[1].y + points[2].y) / 3;
            float z = (points[0].z + points[1].z + points[2].z) / 3;

            centroid = new D_HexaspherePoint(x, y, z);

            return centroid;
        }
    }
}
