
using UnityEngine;

using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

using GBB.Map;
using GBB.Map.Render;
using HS.Hexasphere.Render;

namespace HS.Hexasphere.Input
{
    public class S_Mouse_Input : IEcsRunSystem
    {
        readonly EcsWorldInject world = default;


        readonly EcsPoolInject<C_ProvinceCore> pC_P = default;
        readonly EcsPoolInject<C_ProvinceRender> pR_P = default;
        readonly EcsPoolInject<C_ProvinceHexasphere> pHS_P = default;


        readonly EcsCustomInject<MapRender_Data> mapRender_Data = default;
        readonly EcsCustomInject<HexasphereRender_Data> hexasphereRender_Data = default;
        readonly EcsCustomInject<HexasphereInput_Data> hexasphereInput_Data = default;

        public void Run(IEcsSystems systems)
        {
            //Проверяем ввод с карт
            Maps_InputCheck();
        }

        readonly EcsPoolInject<C_Map> map_P = default;
        void Maps_InputCheck()
        {
            //Если активная карта не пуста
            if(mapRender_Data.Value.ActiveMapPE.Unpack(world.Value, out int activeMapEntity))
            {
                //Берём активную карту
                ref C_Map activeMap = ref map_P.Value.Get(activeMapEntity);

                //Проверяем положение курсора
                Mouse_PositionCheck(ref activeMap);

                //Сообщаем модулю ввода о положении курсора
                Mouse_PositionChange_Request();
            }
        }

        void Mouse_PositionCheck(
            ref C_Map activeMap)
        {
            //Определяем, находится ли курсор над гексасферой
            hexasphereInput_Data.Value.isMouseOverMap = Mouse_GetHitPoint(out Vector3 position, out Ray ray);

            //Если курсор не находится над объектом интерфейса и находится над сферой
            if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject() == false
                && hexasphereInput_Data.Value.isMouseOverMap == true)
            {
                //Если провинция, над которой находится курсор, существует
                if (Province_GetInRayDirection(
                    ref activeMap,
                    ray, position,
                    out Vector3 hitPosition).Unpack(world.Value, out int provinceEntity))
                {
                    //Берём провинцию
                    ref C_ProvinceHexasphere pHS = ref pHS_P.Value.Get(provinceEntity);

                }
            }
            //Иначе
            else
            {
                //Курсор точно не находится над гексасферой
                hexasphereInput_Data.Value.isMouseOverMap = false;

                //Очищаем последнюю провинцию
                hexasphereInput_Data.Value.lastHitProvincePE = new();
            }
        }

        bool Mouse_GetHitPoint(
            out Vector3 position,
            out Ray ray)
        {
            //Берём луч из положения курсора
            ray = Camera.main.ScreenPointToRay(UnityEngine.Input.mousePosition);

            //Если луч касается объекта
            if (Physics.Raycast(ray, out RaycastHit hit)
                //И этим объектом является гексасфера
                && hit.collider.gameObject == hexasphereRender_Data.Value.HexasphereGO)
            {
                //Определяем точку касания
                position = hit.point;

                //И возвращаем true
                return true;
            }
            //Иначе
            else
            {
                //Приравниваем точку касания к нулю
                position = Vector3.zero;

                //И возвращаем false
                return false;
            }
        }

        EcsPackedEntity Province_GetInRayDirection(
            ref C_Map activeMap,
            Ray ray,
            Vector3 worldPosition,
            out Vector3 hitPosition)
        {
            hitPosition = worldPosition;

            //Определяем итоговую точку касания
            Vector3 minPoint = worldPosition;
            Vector3 maxPoint = worldPosition + 0.5f * hexasphereRender_Data.Value.HexasphereScale * ray.direction;

            float rangeMin = hexasphereRender_Data.Value.HexasphereScale * 0.5f;
            rangeMin *= rangeMin;

            float rangeMax = worldPosition.sqrMagnitude;

            float distance;
            Vector3 bestPoint = maxPoint;

            //Уточняем точку
            for (int a = 0; a < 10; a++)
            {
                Vector3 midPoint = (minPoint + maxPoint) * 0.5f;

                distance = midPoint.sqrMagnitude;

                if (distance < rangeMin)
                {
                    maxPoint = midPoint;
                    bestPoint = midPoint;
                }
                else if (distance > rangeMax)
                {
                    maxPoint = midPoint;
                }
                else
                {
                    minPoint = midPoint;
                }
            }

            //Берём PE провинции
            EcsPackedEntity nearestProvincePE = Province_GetAtLocalPosition(
                ref activeMap,
                hexasphereRender_Data.Value.HexasphereGO.transform.InverseTransformPoint(worldPosition));

            //Определяем индекс провинции
            Vector3 currentPoint = worldPosition;

            //Берём ближайшую провинцию
            nearestProvincePE.Unpack(world.Value, out int nearestProvinceEntity);
            ref C_ProvinceRender nearestPR = ref pR_P.Value.Get(nearestProvinceEntity);
            ref C_ProvinceHexasphere nearestPHS = ref pHS_P.Value.Get(nearestProvinceEntity);

            //Определяем верхнюю точку провинции
            Vector3 provinceTop = hexasphereRender_Data.Value.HexasphereGO.transform.TransformPoint(
                nearestPHS.center * (1.0f + nearestPR.ProvinceHeight * hexasphereRender_Data.Value.ExtrudeMultiplier));

            //Определяем высоту провинции и высоту луча
            float provinceHeight = provinceTop.sqrMagnitude;
            float rayHeight = currentPoint.sqrMagnitude;

            float minDistance = 1e6f;
            distance = minDistance;

            //Определяем PE провинции-кандидата
            EcsPackedEntity candidateProvincePE = new();

            const int NUM_STEPS = 10;
            //Уточняем точку
            for (int a = 1; a <= NUM_STEPS; a++)
            {
                distance = Mathf.Abs(rayHeight - provinceHeight);

                //Если расстояние меньше минимального
                if (distance < minDistance)
                {
                    //Обновляем минимальное расстояние и кандидата
                    minDistance = distance;
                    candidateProvincePE = nearestProvincePE;
                    hitPosition = currentPoint;
                }

                if (rayHeight < provinceHeight)
                {
                    return candidateProvincePE;
                }

                float t = a / (float)NUM_STEPS;

                currentPoint = worldPosition * (1f - t) + bestPoint * t;

                nearestProvincePE = Province_GetAtLocalPosition(
                    ref activeMap,
                    hexasphereRender_Data.Value.HexasphereGO.transform.InverseTransformPoint(currentPoint));


                //Обновляем ближайшую провинцию
                nearestProvincePE.Unpack(world.Value, out nearestProvinceEntity);
                nearestPR = ref pR_P.Value.Get(nearestProvinceEntity);
                nearestPHS = ref pHS_P.Value.Get(nearestProvinceEntity);

                provinceTop = hexasphereRender_Data.Value.HexasphereGO.transform.TransformPoint(
                    nearestPHS.center * (1.0f + nearestPR.ProvinceHeight * hexasphereRender_Data.Value.ExtrudeMultiplier));

                //Определяем высоту провинции и высоту луча
                provinceHeight = provinceTop.sqrMagnitude;
                rayHeight = currentPoint.sqrMagnitude;
            }

            //Если расстояние меньше минимального
            if (distance < minDistance)
            {
                //Обновляем минимальное расстояние и кандидата
                minDistance = distance;
                candidateProvincePE = nearestProvincePE;
                hitPosition = currentPoint;
            }

            if (rayHeight < provinceHeight)
            {
                return candidateProvincePE;
            }
            else
            {
                return new();
            }
        }

        /// <summary>
        /// Определение провинции, ближайшей к запрошенной точке
        /// </summary>
        /// <param name="activeMap"></param>
        /// <param name="localPosition"></param>
        /// <returns></returns>
        EcsPackedEntity Province_GetAtLocalPosition(
            ref C_Map activeMap,
            Vector3 localPosition)
        {
            //Упрощённо проверяем, не является ли последняя провинция под курсором текущей
            //Если последняя провинция существует
            if(hexasphereInput_Data.Value.lastHitProvincePE.Unpack(world.Value, out int lastHitProvinceEntity))
            {
                //Берём её
                ref C_ProvinceCore lastHitPC = ref pC_P.Value.Get(lastHitProvinceEntity);
                ref C_ProvinceHexasphere lastHitPHS = ref pHS_P.Value.Get(lastHitProvinceEntity);

                //Определяем расстояние до центра провинции
                float distance = Vector3.SqrMagnitude(lastHitPHS.center - localPosition);

                //Определяем, находятся ли её соседи дальше, чем она
                bool isValid = true;

                //Для каждой соседней провинции
                for (int a = 0; a < lastHitPC.neighbourProvinceEntities.Length; a++)
                {
                    //Берём соседнюю провинцию
                    ref C_ProvinceHexasphere neighbourPHS = ref pHS_P.Value.Get(lastHitPC.neighbourProvinceEntities[a]);

                    //Определяем расстояние до центра провинции
                    float otherDistance = Vector3.SqrMagnitude(neighbourPHS.center - localPosition);

                    //Если оно меньше расстояния до последней провинции
                    if (otherDistance < distance)
                    {
                        //Отмечаем это и выходим из цикла
                        isValid = false;
                        break;
                    }
                }

                //Если это последняя провинция
                if (isValid == true)
                {
                    //Возвращаем её PE
                    return hexasphereInput_Data.Value.lastHitProvincePE;
                }
            }

            //Ищем текущую провинцию под курсором среди всех
            //Берём первую провинцию карты
            int nearestProvinceEntity = activeMap.provinceEntities[0];
            ref C_ProvinceCore nearestPC = ref pC_P.Value.Get(nearestProvinceEntity);

            //Определяем минимальное расстояние как максимально возможное
            float minDistance = float.MaxValue;

            //Для каждой провинции карты
            for(int a = 0; a < activeMap.provinceEntities.Length; a++)
            {
                //Берём ближайшую провинцию среди соседей
                int newNearestProvinceEntity = Province_GetNearestNeighbourToPosition(
                    ref nearestPC,
                    localPosition,
                    out float provinceDistance);

                //Если расстояние меньше минимального
                if (provinceDistance < minDistance)
                {
                    //Обновляем провинцию и минимальное расстояние 
                    nearestPC = ref pC_P.Value.Get(newNearestProvinceEntity);
                    nearestProvinceEntity = newNearestProvinceEntity;
                    minDistance = provinceDistance;
                }
                //Иначе выходим из цикла
                else
                {
                    break;
                }
            }

            //PE последней провинции - это PE ближайшего
            hexasphereInput_Data.Value.lastHitProvincePE = world.Value.PackEntity(nearestProvinceEntity);

            return hexasphereInput_Data.Value.lastHitProvincePE;
        }

        /// <summary>
        /// Определение провинции, ближайшей к точке, из соседей переданной провинции
        /// </summary>
        /// <param name="pC"></param>
        /// <param name="localPosition"></param>
        /// <param name="minDistance"></param>
        /// <returns></returns>
        int Province_GetNearestNeighbourToPosition(
            ref C_ProvinceCore pC,
            Vector3 localPosition,
            out float minDistance)
        {
            //Определяем минимальное расстояние как максимально возможное
            minDistance = float.MaxValue;

            //Создаём переменную для сущности итоговой провинции
            int nearestProvinceEntity = -1;

            //Для каждой соседней провинции
            for (int a = 0; a < pC.neighbourProvinceEntities.Length; a++)
            {
                //Берём соседнюю провинцию
                ref C_ProvinceHexasphere neighbourPHS = ref pHS_P.Value.Get(pC.neighbourProvinceEntities[a]);

                //Берём центр провинции
                Vector3 center = neighbourPHS.center;

                //Рассчитываем расстояние
                float distance
                    = (center.x - localPosition.x) * (center.x - localPosition.x)
                    + (center.y - localPosition.y) * (center.y - localPosition.y)
                    + (center.z - localPosition.z) * (center.z - localPosition.z);

                //Если расстояние меньше минимального
                if (distance < minDistance)
                {
                    //Обновляем провинцию и минимальное расстояние
                    nearestProvinceEntity = pC.neighbourProvinceEntities[a];
                    minDistance = distance;
                }
            }

            //Возвращаем ближайшего к переданной точке соседа
            return nearestProvinceEntity;
        }

        readonly EcsPoolInject<GBB.Input.R_Mouse_PositionChange> mouse_PositionChange_R_P = default;
        void Mouse_PositionChange_Request()
        {
            //Создаём новую сущность и назначаем ей запрос смены положения курсора
            int requestEntity = world.Value.NewEntity();
            ref GBB.Input.R_Mouse_PositionChange requestComp = ref mouse_PositionChange_R_P.Value.Add(requestEntity);

            //Заполняем данные запроса
            requestComp = new GBB.Input.R_Mouse_PositionChange(
                hexasphereInput_Data.Value.isMouseOverMap,
                hexasphereInput_Data.Value.lastHitProvincePE);
        }
    }
}
