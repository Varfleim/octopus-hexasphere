
using System.Collections.Generic;

using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

using GBB;
using GBB.Map;

namespace HS.Hexasphere
{
    /// <summary>
    /// Система, создающая гексасферы по запросу.
    /// Отрабатывает в Init и PreTick
    /// </summary>
    public class S_Hexasphere_Creation : IEcsInitSystem, IEcsRunSystem
    {
        readonly EcsWorldInject world = default;


        readonly EcsPoolInject<C_ProvinceHexasphere> pHS_P = default;
        readonly EcsPoolInject<SR_ProvinceCore_Creation> pC_Creation_SR_P = default;


        readonly EcsCustomInject<Hexasphere_Data> hexasphere_Data = default;

        public void Init(IEcsSystems systems)
        {
            //Создаём гексасферы
            HSs_Creation();
        }

        public void Run(IEcsSystems systems)
        {
            //Создаём гексасферы
            HSs_Creation();
        }

        readonly EcsFilterInject<Inc<C_Map, SR_Hexasphere_Creation>> hS_Creation_F = default;
        readonly EcsPoolInject<C_Map> map_P = default;
        readonly EcsPoolInject<SR_Hexasphere_Creation> hS_Creation_SR_P = default;
        readonly EcsFilterInject<Inc<SR_ProvinceCore_Creation>> pC_Creation_SR_F = default;
        readonly EcsPoolInject<C_ProvinceCore> pC_P = default;
        void HSs_Creation()
        {
            //Для каждой карты с запросом создания гексасферы
            foreach (int mapRequestEntity in hS_Creation_F.Value)
            {
                //Берём карту и запрос создания гексасферы
                ref C_Map map = ref map_P.Value.Get(mapRequestEntity);
                ref SR_Hexasphere_Creation requestComp = ref hS_Creation_SR_P.Value.Get(mapRequestEntity);

                //Инициализируем данные гексасферы
                HS_Initialize();

                //Создаём простую гексасферу
                HS_Creation(ref requestComp);

                //Рассчитываем соседей PHS и запрашиваем создание PC
                PHS_CalculateNeighbours(mapRequestEntity, ref requestComp);

                //ТЕСТ
                //Создаём PC по запросу
                Map_Data.ProvincesCore_Creation(
                    ref map,
                    pC_Creation_SR_F.Value, pC_Creation_SR_P.Value,
                    pC_P.Value);
                //ТЕСТ

                //Удаляем запрос
                hS_Creation_SR_P.Value.Del(mapRequestEntity);
            }
        }

        void HS_Initialize()
        {

        }

        void HS_Creation(
            ref SR_Hexasphere_Creation requestComp)
        {
            D_HexaspherePoint[] corners = new D_HexaspherePoint[]
            {
                new D_HexaspherePoint(1, Hexasphere_Data.PHI, 0),
                new D_HexaspherePoint(-1, Hexasphere_Data.PHI, 0),
                new D_HexaspherePoint(1, -Hexasphere_Data.PHI, 0),
                new D_HexaspherePoint(-1, -Hexasphere_Data.PHI, 0),
                new D_HexaspherePoint(0, 1, Hexasphere_Data.PHI),
                new D_HexaspherePoint(0, -1, Hexasphere_Data.PHI),
                new D_HexaspherePoint(0, 1, -Hexasphere_Data.PHI),
                new D_HexaspherePoint(0, -1, -Hexasphere_Data.PHI),
                new D_HexaspherePoint(Hexasphere_Data.PHI, 0, 1),
                new D_HexaspherePoint(-Hexasphere_Data.PHI, 0, 1),
                new D_HexaspherePoint(Hexasphere_Data.PHI, 0, -1),
                new D_HexaspherePoint(-Hexasphere_Data.PHI, 0, -1)
            };

            //Определяем треугольники изначального икосаэдра
            D_HexasphereTriangle[] triangles = new D_HexasphereTriangle[]
            {
                new D_HexasphereTriangle(corners [0], corners [1], corners [4], false),
                new D_HexasphereTriangle(corners [1], corners [9], corners [4], false),
                new D_HexasphereTriangle(corners [4], corners [9], corners [5], false),
                new D_HexasphereTriangle(corners [5], corners [9], corners [3], false),
                new D_HexasphereTriangle(corners [2], corners [3], corners [7], false),
                new D_HexasphereTriangle(corners [3], corners [2], corners [5], false),
                new D_HexasphereTriangle(corners [7], corners [10], corners [2], false),
                new D_HexasphereTriangle(corners [0], corners [8], corners [10], false),
                new D_HexasphereTriangle(corners [0], corners [4], corners [8], false),
                new D_HexasphereTriangle(corners [8], corners [2], corners [10], false),
                new D_HexasphereTriangle(corners [8], corners [4], corners [5], false),
                new D_HexasphereTriangle(corners [8], corners [5], corners [2], false),
                new D_HexasphereTriangle(corners [1], corners [0], corners [6], false),
                new D_HexasphereTriangle(corners [11], corners [1], corners [6], false),
                new D_HexasphereTriangle(corners [3], corners [9], corners [11], false),
                new D_HexasphereTriangle(corners [6], corners [10], corners [7], false),
                new D_HexasphereTriangle(corners [3], corners [11], corners [7], false),
                new D_HexasphereTriangle(corners [11], corners [6], corners [7], false),
                new D_HexasphereTriangle(corners [6], corners [0], corners [10], false),
                new D_HexasphereTriangle(corners [9], corners [1], corners [11], false)
            };

            //Очищаем словарь точек
            hexasphere_Data.Value.tempPoints.Clear();

            //Заносим вершины икосаэдра в словарь
            for (int a = 0; a < corners.Length; a++)
            {
                hexasphere_Data.Value.tempPoints[corners[a]] = corners[a];
            }

            //Создаём список точек нижнего ребра треугольника
            List<D_HexaspherePoint> bottom = ListPool<D_HexaspherePoint>.Get();
            //Определяем количество треугольников
            int triangleCount = triangles.Length;
            //Для каждого треугольника
            for (int a = 0; a < triangleCount; a++)
            {
                //Создаём пустой список точек
                List<D_HexaspherePoint> previous;

                //Берём первую вершину треугольника
                D_HexaspherePoint point0 = triangles[a].points[0];

                //Очищаем временный список точек
                bottom.Clear();

                //Заносим в список первую вершину треугольника
                bottom.Add(point0);

                //Создаём список точек левого ребра треугольника
                List<D_HexaspherePoint> left = Point_Subdivide(
                    point0, triangles[a].points[1],
                    requestComp.subdivisions);
                //Создаём список точек правого ребра треугольника
                List<D_HexaspherePoint> right = Point_Subdivide(
                    point0, triangles[a].points[2],
                    requestComp.subdivisions);

                //Для каждого подразделения
                for (int b = 1; b <= requestComp.subdivisions; b++)
                {
                    //Переносим список точек нижнего ребра
                    previous = bottom;

                    //Подразделяем перемычку с левого ребра до правого
                    bottom = Point_Subdivide(
                        left[b], right[b],
                        b);

                    //Создаём новый треугольник
                    new D_HexasphereTriangle(previous[0], bottom[0], bottom[1]);

                    //Для каждого ...
                    for (int c = 1; c < b; c++)
                    {
                        //Создаём два новых треугольника
                        new D_HexasphereTriangle(previous[c], bottom[c], bottom[c + 1]);
                        new D_HexasphereTriangle(previous[c - 1], previous[c], bottom[c]);
                    }
                }

                //Возвращаем списки в пул
                ListPool<D_HexaspherePoint>.Add(left);
                ListPool<D_HexaspherePoint>.Add(right);
            }

            //Возвращаем список в пул
            ListPool<D_HexaspherePoint>.Add(bottom);

            //Создаём провинции
            //Обнуляем флаг точек
            D_HexaspherePoint.flag = 0;

            //Для каждой вершины в словаре
            foreach (D_HexaspherePoint point in hexasphere_Data.Value.tempPoints.Values)
            {
                //Создаём для провинций компоненты гексасферы
                PHS_Creation(point);
            }

            //Очищаем словарь точек
            hexasphere_Data.Value.tempPoints.Clear();
        }

        /// <summary>
        /// Функция подразделяет рёбра икосферы соответственно числу подразделений.
        /// List для точек берёт из пула, поэтому его нужно туда возвращать
        /// </summary>
        /// <param name="startPoint"></param>
        /// <param name="endPoint"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        List<D_HexaspherePoint> Point_Subdivide(
            D_HexaspherePoint startPoint, D_HexaspherePoint endPoint,
            int count)
        {
            //Создаём список точек, определяющих сегменты грани, и заносим в него текущую вершину
            List<D_HexaspherePoint> segments = ListPool<D_HexaspherePoint>.Get();
            segments.Add(startPoint);

            //Рассчитываем координаты точек
            double dx = endPoint.x - startPoint.x;
            double dy = endPoint.y - startPoint.y;
            double dz = endPoint.z - startPoint.z;
            double doublex = startPoint.x;
            double doubley = startPoint.y;
            double doublez = startPoint.z;
            double doubleCount = count;

            //Для каждого подразделения
            for (int a = 1; a < count; a++)
            {
                //Создаём новую вершину
                D_HexaspherePoint newPoint = new(
                    (float)(doublex + dx * a / doubleCount),
                    (float)(doubley + dy * a / doubleCount),
                    (float)(doublez + dz * a / doubleCount));

                //Проверяем вершину
                newPoint = Point_GetCached(newPoint);

                //Заносим вершину в список
                segments.Add(newPoint);
            }

            //Заносим в список конечную вершину
            segments.Add(endPoint);

            //Возвращаем список точеку
            return segments;
        }

        D_HexaspherePoint Point_GetCached(
            D_HexaspherePoint point)
        {
            //Если запрошенная вершина существует в словаре
            if (hexasphere_Data.Value.tempPoints.TryGetValue(point, out D_HexaspherePoint thePoint))
            {
                //Возвращаем вершину
                return thePoint;
            }
            //Иначе
            else
            {
                //Обновляем вершину в словаре
                hexasphere_Data.Value.tempPoints[point] = point;

                //Возвращаем вершину
                return point;
            }
        }

        void PHS_Creation(
            D_HexaspherePoint centerPoint)
        {
            //Создаём новую сущность и назначаем ей компонент PHS
            int provinceEntity = world.Value.NewEntity();
            ref C_ProvinceHexasphere currentPHS = ref pHS_P.Value.Add(provinceEntity);

            //Заполняем основные данные PHS
            currentPHS = new(
                provinceEntity,
                centerPoint);
        }

        readonly EcsFilterInject<Inc<C_ProvinceHexasphere>, Exc<SR_ProvinceCore_Creation>> pHS_WithoutCreationSR_F = default;
        void PHS_CalculateNeighbours(
            int mapEntity, ref SR_Hexasphere_Creation hexasphereCreationRequestComp)
        {
            //Создаём временный список для сущностей соседей
            List<int> tempNeighbourEntities = ListPool<int>.Get();

            //Для каждой провинции без запроса создания PC
            foreach (int provinceEntity in pHS_WithoutCreationSR_F.Value)
            {
                //Берём PHS
                ref C_ProvinceHexasphere pHS = ref pHS_P.Value.Get(provinceEntity);

                //Рассчитываем соседей

                //Очищаем временный список
                tempNeighbourEntities.Clear();

                //Берём первый треугольник в данных провинции
                D_HexasphereTriangle firstTriangle = pHS.centerPoint.triangles[0];

                //Для каждой вершины треугольника
                for (int a = 0; a < firstTriangle.points.Length; a++)
                {
                    //Если это не текущая провинция
                    if (firstTriangle.points[a].provinceEntity != provinceEntity)
                    {
                        //Заносим провинцию в список
                        tempNeighbourEntities.Add(firstTriangle.points[a].provinceEntity);
                    }
                }

                //Пока количество соседей во временном списке меньше количества треугольников
                while (tempNeighbourEntities.Count < pHS.centerPoint.triangleCount)
                {
                    //Создаём переменную для новой провинции
                    int newProvinceEntity = new();

                    //Для каждого треугольника провинции
                    for (int a = 0; a < pHS.centerPoint.triangleCount; a++)
                    {
                        //Берём треугольник
                        D_HexasphereTriangle triangle = pHS.centerPoint.triangles[a];

                        //Проверяем, является ли этот треугольник следующим по порядку
                        bool isContainPreviousProvince = false;
                        bool isContainNewProvince = false;

                        //Для каждой вершины треугольника
                        for (int b = 0; b < triangle.points.Length; b++)
                        {
                            //Если это не текущая провинция
                            if (triangle.points[b].provinceEntity != provinceEntity)
                            {
                                //Если эта провинция - последняя во временном списке
                                if (tempNeighbourEntities[tempNeighbourEntities.Count - 1] == triangle.points[b].provinceEntity)
                                {
                                    //Отмечаем, что треугольник содержит предыдущую провинцию
                                    isContainPreviousProvince = true;
                                }
                                //Иначе, если этой провинции нет во временном списке
                                else if (tempNeighbourEntities.Contains(triangle.points[b].provinceEntity) == false)
                                {
                                    //Отмечаем, что треугольник содержит новую провинцию
                                    isContainNewProvince = true;

                                    //Сохраняем PE новой провинции
                                    newProvinceEntity = triangle.points[b].provinceEntity;
                                }
                            }
                        }

                        //Если этот треугольник удовлетворяет обоим условиям, то он является следующим
                        if (isContainPreviousProvince == true
                            && isContainNewProvince == true)
                        {
                            //Выходим из цикла
                            break;
                        }
                    }

                    //Заносим новую провинцию во временный список
                    tempNeighbourEntities.Add(newProvinceEntity);
                }

                //Запрашиваем создание PC по PHS
                Map_Data.ProvinceCore_Creation_Request(
                    pC_Creation_SR_P.Value,
                    provinceEntity,
                    mapEntity,
                    tempNeighbourEntities);
            }

            //Возвращаем список в пул
            ListPool<int>.Add(tempNeighbourEntities);
        }
    }
}
