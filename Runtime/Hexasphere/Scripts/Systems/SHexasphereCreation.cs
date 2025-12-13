
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
    public class SHexasphereCreation : IEcsInitSystem, IEcsRunSystem
    {
        readonly EcsWorldInject world = default;


        readonly EcsPoolInject<C_ProvinceHexasphere> pHSPool = default;
        readonly EcsPoolInject<SR_ProvinceCoreCreation> pCCreationSRPool = default;


        readonly EcsCustomInject<HexasphereData> hexasphereData = default;

        public void Init(IEcsSystems systems)
        {
            //Создаём гексасферы
            HexaspheresCreation();
        }

        public void Run(IEcsSystems systems)
        {
            //Создаём гексасферы
            HexaspheresCreation();
        }

        readonly EcsFilterInject<Inc<C_Map, SR_HexasphereCreation>> mapHexasphereCreationSRFilter = default;
        readonly EcsPoolInject<C_Map> mapPool = default;
        readonly EcsPoolInject<SR_HexasphereCreation> hexasphereCreationSRPool = default;
        readonly EcsFilterInject<Inc<SR_ProvinceCoreCreation>> pCCreationSRFilter = default;
        readonly EcsPoolInject<C_ProvinceCore> pCPool = default;
        void HexaspheresCreation()
        {
            //Для каждой карты с запросом создания гексасферы
            foreach (int mapEntity in mapHexasphereCreationSRFilter.Value)
            {
                //Берём карту и запрос создания гексасферы
                ref C_Map map = ref mapPool.Value.Get(mapEntity);
                ref SR_HexasphereCreation requestComp = ref hexasphereCreationSRPool.Value.Get(mapEntity);

                //Инициализируем данные гексасферы
                HexasphereInitialize();

                //Создаём простую гексасферу
                HexasphereCreation(ref requestComp);

                //Рассчитываем соседей PHS и запрашиваем создание PC
                ProvinceHexashpereCalculateNeighbours(mapEntity, ref requestComp);

                //ТЕСТ
                //Создаём PC по запросу
                MapData.ProvincesCoreCreation(
                    ref map,
                    pCCreationSRFilter.Value, pCCreationSRPool.Value,
                    pCPool.Value);
                //ТЕСТ

                //Удаляем запрос
                hexasphereCreationSRPool.Value.Del(mapEntity);
            }
        }

        void HexasphereInitialize()
        {

        }

        void HexasphereCreation(
            ref SR_HexasphereCreation requestComp)
        {
            DHexaspherePoint[] corners = new DHexaspherePoint[]
            {
                new DHexaspherePoint(1, HexasphereData.PHI, 0),
                new DHexaspherePoint(-1, HexasphereData.PHI, 0),
                new DHexaspherePoint(1, -HexasphereData.PHI, 0),
                new DHexaspherePoint(-1, -HexasphereData.PHI, 0),
                new DHexaspherePoint(0, 1, HexasphereData.PHI),
                new DHexaspherePoint(0, -1, HexasphereData.PHI),
                new DHexaspherePoint(0, 1, -HexasphereData.PHI),
                new DHexaspherePoint(0, -1, -HexasphereData.PHI),
                new DHexaspherePoint(HexasphereData.PHI, 0, 1),
                new DHexaspherePoint(-HexasphereData.PHI, 0, 1),
                new DHexaspherePoint(HexasphereData.PHI, 0, -1),
                new DHexaspherePoint(-HexasphereData.PHI, 0, -1)
            };

            //Определяем треугольники изначального икосаэдра
            DHexasphereTriangle[] triangles = new DHexasphereTriangle[]
            {
                new DHexasphereTriangle(corners [0], corners [1], corners [4], false),
                new DHexasphereTriangle(corners [1], corners [9], corners [4], false),
                new DHexasphereTriangle(corners [4], corners [9], corners [5], false),
                new DHexasphereTriangle(corners [5], corners [9], corners [3], false),
                new DHexasphereTriangle(corners [2], corners [3], corners [7], false),
                new DHexasphereTriangle(corners [3], corners [2], corners [5], false),
                new DHexasphereTriangle(corners [7], corners [10], corners [2], false),
                new DHexasphereTriangle(corners [0], corners [8], corners [10], false),
                new DHexasphereTriangle(corners [0], corners [4], corners [8], false),
                new DHexasphereTriangle(corners [8], corners [2], corners [10], false),
                new DHexasphereTriangle(corners [8], corners [4], corners [5], false),
                new DHexasphereTriangle(corners [8], corners [5], corners [2], false),
                new DHexasphereTriangle(corners [1], corners [0], corners [6], false),
                new DHexasphereTriangle(corners [11], corners [1], corners [6], false),
                new DHexasphereTriangle(corners [3], corners [9], corners [11], false),
                new DHexasphereTriangle(corners [6], corners [10], corners [7], false),
                new DHexasphereTriangle(corners [3], corners [11], corners [7], false),
                new DHexasphereTriangle(corners [11], corners [6], corners [7], false),
                new DHexasphereTriangle(corners [6], corners [0], corners [10], false),
                new DHexasphereTriangle(corners [9], corners [1], corners [11], false)
            };

            //Очищаем словарь точек
            hexasphereData.Value.tempPoints.Clear();

            //Заносим вершины икосаэдра в словарь
            for (int a = 0; a < corners.Length; a++)
            {
                hexasphereData.Value.tempPoints[corners[a]] = corners[a];
            }

            //Создаём список точек нижнего ребра треугольника
            List<DHexaspherePoint> bottom = ListPool<DHexaspherePoint>.Get();
            //Определяем количество треугольников
            int triangleCount = triangles.Length;
            //Для каждого треугольника
            for (int a = 0; a < triangleCount; a++)
            {
                //Создаём пустой список точек
                List<DHexaspherePoint> previous;

                //Берём первую вершину треугольника
                DHexaspherePoint point0 = triangles[a].points[0];

                //Очищаем временный список точек
                bottom.Clear();

                //Заносим в список первую вершину треугольника
                bottom.Add(point0);

                //Создаём список точек левого ребра треугольника
                List<DHexaspherePoint> left = PointSubdivide(
                    point0, triangles[a].points[1],
                    requestComp.subdivisions);
                //Создаём список точек правого ребра треугольника
                List<DHexaspherePoint> right = PointSubdivide(
                    point0, triangles[a].points[2],
                    requestComp.subdivisions);

                //Для каждого подразделения
                for (int b = 1; b <= requestComp.subdivisions; b++)
                {
                    //Переносим список точек нижнего ребра
                    previous = bottom;

                    //Подразделяем перемычку с левого ребра до правого
                    bottom = PointSubdivide(
                        left[b], right[b],
                        b);

                    //Создаём новый треугольник
                    new DHexasphereTriangle(previous[0], bottom[0], bottom[1]);

                    //Для каждого ...
                    for (int c = 1; c < b; c++)
                    {
                        //Создаём два новых треугольника
                        new DHexasphereTriangle(previous[c], bottom[c], bottom[c + 1]);
                        new DHexasphereTriangle(previous[c - 1], previous[c], bottom[c]);
                    }
                }

                //Возвращаем списки в пул
                ListPool<DHexaspherePoint>.Add(left);
                ListPool<DHexaspherePoint>.Add(right);
            }

            //Возвращаем список в пул
            ListPool<DHexaspherePoint>.Add(bottom);

            //Создаём провинции
            //Обнуляем флаг точек
            DHexaspherePoint.flag = 0;

            //Для каждой вершины в словаре
            foreach (DHexaspherePoint point in hexasphereData.Value.tempPoints.Values)
            {
                //Создаём для провинций компоненты гексасферы
                ProvinceHexashpereCreation(point);
            }

            //Очищаем словарь точек
            hexasphereData.Value.tempPoints.Clear();
        }

        /// <summary>
        /// Функция подразделяет рёбра икосферы соответственно числу подразделений.
        /// List для точек берёт из пула, поэтому его нужно туда возвращать
        /// </summary>
        /// <param name="startPoint"></param>
        /// <param name="endPoint"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        List<DHexaspherePoint> PointSubdivide(
            DHexaspherePoint startPoint, DHexaspherePoint endPoint,
            int count)
        {
            //Создаём список точек, определяющих сегменты грани, и заносим в него текущую вершину
            List<DHexaspherePoint> segments = ListPool<DHexaspherePoint>.Get();
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
                DHexaspherePoint newPoint = new(
                    (float)(doublex + dx * a / doubleCount),
                    (float)(doubley + dy * a / doubleCount),
                    (float)(doublez + dz * a / doubleCount));

                //Проверяем вершину
                newPoint = PointGetCached(newPoint);

                //Заносим вершину в список
                segments.Add(newPoint);
            }

            //Заносим в список конечную вершину
            segments.Add(endPoint);

            //Возвращаем список точеку
            return segments;
        }

        DHexaspherePoint PointGetCached(
            DHexaspherePoint point)
        {
            //Если запрошенная вершина существует в словаре
            if (hexasphereData.Value.tempPoints.TryGetValue(point, out DHexaspherePoint thePoint))
            {
                //Возвращаем вершину
                return thePoint;
            }
            //Иначе
            else
            {
                //Обновляем вершину в словаре
                hexasphereData.Value.tempPoints[point] = point;

                //Возвращаем вершину
                return point;
            }
        }

        void ProvinceHexashpereCreation(
            DHexaspherePoint centerPoint)
        {
            //Создаём новую сущность и назначаем ей компонент PHS
            int provinceEntity = world.Value.NewEntity();
            ref C_ProvinceHexasphere currentPHS = ref pHSPool.Value.Add(provinceEntity);

            //Заполняем основные данные PHS
            currentPHS = new(
                provinceEntity,
                centerPoint);
        }

        readonly EcsFilterInject<Inc<C_ProvinceHexasphere>, Exc<SR_ProvinceCoreCreation>> provinceWithoutPCCreationSRFilter = default;
        void ProvinceHexashpereCalculateNeighbours(
            int mapEntity, ref SR_HexasphereCreation hexasphereCreationRequestComp)
        {
            //Создаём временный список для сущностей соседей
            List<int> tempNeighbourEntities = ListPool<int>.Get();

            //Для каждой провинции без запроса создания PC
            foreach (int provinceEntity in provinceWithoutPCCreationSRFilter.Value)
            {
                //Берём PHS
                ref C_ProvinceHexasphere pHS = ref pHSPool.Value.Get(provinceEntity);

                //Рассчитываем соседей

                //Очищаем временный список
                tempNeighbourEntities.Clear();

                //Берём первый треугольник в данных провинции
                DHexasphereTriangle firstTriangle = pHS.centerPoint.triangles[0];

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
                        DHexasphereTriangle triangle = pHS.centerPoint.triangles[a];

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

                //Создаём временный список PE и заносим в него PE соседей
                List<EcsPackedEntity> tempNeighbourPEs = ListPool<EcsPackedEntity>.Get();
                for(int a = 0; a < tempNeighbourEntities.Count; a++)
                {
                    tempNeighbourPEs.Add(world.Value.PackEntity(tempNeighbourEntities[a]));
                }

                //Запрашиваем создание PC по PHS
                MapData.ProvinceCoreCreationRequest(
                    pCCreationSRPool.Value,
                    provinceEntity,
                    mapEntity,
                    tempNeighbourEntities);

                ListPool<EcsPackedEntity>.Add(tempNeighbourPEs);
            }

            //Возвращаем список в пул
            ListPool<int>.Add(tempNeighbourEntities);
        }
    }
}
