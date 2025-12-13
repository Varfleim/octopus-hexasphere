namespace HS.Hexasphere
{
    public class DHexasphereTriangle
    {
        public DHexasphereTriangle(
            DHexaspherePoint point1, DHexaspherePoint point2, DHexaspherePoint point3,
            bool register = true)
        {
            points = new DHexaspherePoint[]
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

        public DHexaspherePoint[] points;
        public int orderedFlag;

        DHexaspherePoint centroid;
        bool isCentroid;

        public bool IsAdjacentTo(
            DHexasphereTriangle triangle2)
        {
            bool match = false;

            //Для каждой точки
            for (int a = 0; a < 3; a++)
            {
                //Берём точку первого треугольника
                DHexaspherePoint point1 = points[a];

                //Для каждой точки
                for (var b = 0; b < 3; b++)
                {
                    //Берём точку второго треугольника
                    DHexaspherePoint point2 = triangle2.points[b];

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

        public DHexaspherePoint GetCentroid()
        {
            if (isCentroid == true)
            {
                return centroid;
            }

            isCentroid = true;

            float x = (points[0].x + points[1].x + points[2].x) / 3;
            float y = (points[0].y + points[1].y + points[2].y) / 3;
            float z = (points[0].z + points[1].z + points[2].z) / 3;

            centroid = new DHexaspherePoint(x, y, z);

            return centroid;
        }
    }
}
