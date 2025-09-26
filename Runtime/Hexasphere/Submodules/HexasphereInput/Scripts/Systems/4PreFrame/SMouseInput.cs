
using UnityEngine;

using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

using GBB.Map;

namespace HS.Input
{
    public class SMouseInput : IEcsRunSystem
    {
        readonly EcsWorldInject world = default;

        readonly EcsFilterInject<Inc<CMap, CActiveMap>> activeMapFilter = default;
        readonly EcsPoolInject<CMap> mapPool = default;

        readonly EcsPoolInject<CProvinceCore> pCPool = default;

        readonly EcsPoolInject<CProvinceRender> pRPool = default;
        readonly EcsPoolInject<CProvinceHexasphere> pHSPool = default;


        readonly EcsCustomInject<HexasphereData> hexasphereData = default;

        readonly EcsCustomInject<HexasphereInputData> hexasphereInputData = default;

        public void Run(IEcsSystems systems)
        {
            //Для каждой активной карты
            foreach(int activeMapEntity in activeMapFilter.Value)
            {
                //Берём карту
                ref CMap activeMap = ref mapPool.Value.Get(activeMapEntity);

                //Проверяем положение курсора
                MousePositionCheck(ref activeMap);

                //Сообщаем модулю ввода о положении курсора
                MousePositionRequest();
            }
        }

        void MousePositionCheck(
            ref CMap activeMap)
        {
            //Определяем, находится ли курсор над гексасферой
            hexasphereInputData.Value.isMouseOverMap = MouseGetHitPoint(out Vector3 position, out Ray ray);

            //Если курсор не находится над объектом интерфейса и находится над сферой
            if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject() == false
                && hexasphereInputData.Value.isMouseOverMap == true)
            {
                //Если провинция, над которой находится курсор, существует
                if (ProvinceGetInRayDirection(
                    ref activeMap,
                    ray, position,
                    out Vector3 hitPosition).Unpack(world.Value, out int provinceEntity))
                {
                    //Берём провинцию
                    ref CProvinceHexasphere pHS = ref pHSPool.Value.Get(provinceEntity);

                }
            }
            //Иначе
            else
            {
                //Курсор точно не находится над гексасферой
                hexasphereInputData.Value.isMouseOverMap = false;

                //Очищаем последнюю провинцию
                hexasphereInputData.Value.lastHitProvincePE = new();
            }
        }

        bool MouseGetHitPoint(
            out Vector3 position,
            out Ray ray)
        {
            //Берём луч из положения курсора
            ray = Camera.main.ScreenPointToRay(UnityEngine.Input.mousePosition);

            //Если луч касается объекта
            if (Physics.Raycast(ray, out RaycastHit hit)
                //И этим объектом является гексасфера
                && hit.collider.gameObject == HexasphereData.HexasphereGO)
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

        EcsPackedEntity ProvinceGetInRayDirection(
            ref CMap activeMap,
            Ray ray,
            Vector3 worldPosition,
            out Vector3 hitPosition)
        {
            hitPosition = worldPosition;

            //Определяем итоговую точку касания
            Vector3 minPoint = worldPosition;
            Vector3 maxPoint = worldPosition + 0.5f * hexasphereData.Value.hexasphereScale * ray.direction;

            float rangeMin = hexasphereData.Value.hexasphereScale * 0.5f;
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
            EcsPackedEntity nearestProvincePE = ProvinceGetAtLocalPosition(
                ref activeMap,
                HexasphereData.HexasphereGO.transform.InverseTransformPoint(worldPosition));

            //Определяем индекс провинции
            Vector3 currentPoint = worldPosition;

            //Берём ближайшую провинцию
            nearestProvincePE.Unpack(world.Value, out int nearestProvinceEntity);
            ref CProvinceRender nearestPR = ref pRPool.Value.Get(nearestProvinceEntity);
            ref CProvinceHexasphere nearestPHS = ref pHSPool.Value.Get(nearestProvinceEntity);

            //Определяем верхнюю точку провинции
            Vector3 provinceTop = HexasphereData.HexasphereGO.transform.TransformPoint(
                nearestPHS.center * (1.0f + nearestPR.ProvinceHeight * HexasphereData.ExtrudeMultiplier));

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

                nearestProvincePE = ProvinceGetAtLocalPosition(
                    ref activeMap,
                    HexasphereData.HexasphereGO.transform.InverseTransformPoint(currentPoint));


                //Обновляем ближайшую провинцию
                nearestProvincePE.Unpack(world.Value, out nearestProvinceEntity);
                nearestPR = ref pRPool.Value.Get(nearestProvinceEntity);
                nearestPHS = ref pHSPool.Value.Get(nearestProvinceEntity);

                provinceTop = HexasphereData.HexasphereGO.transform.TransformPoint(
                    nearestPHS.center * (1.0f + nearestPR.ProvinceHeight * HexasphereData.ExtrudeMultiplier));

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
        EcsPackedEntity ProvinceGetAtLocalPosition(
            ref CMap activeMap,
            Vector3 localPosition)
        {
            //Упрощённо проверяем, не является ли последняя провинция под курсором текущей
            //Если последняя провинция существует
            if(hexasphereInputData.Value.lastHitProvincePE.Unpack(world.Value, out int lastHitProvinceEntity))
            {
                //Берём её
                ref CProvinceCore lastHitPC = ref pCPool.Value.Get(lastHitProvinceEntity);
                ref CProvinceHexasphere lastHitPHS = ref pHSPool.Value.Get(lastHitProvinceEntity);

                //Определяем расстояние до центра провинции
                float distance = Vector3.SqrMagnitude(lastHitPHS.center - localPosition);

                //Определяем, находятся ли её соседи дальше, чем она
                bool isValid = true;

                //Для каждой соседней провинции
                for (int a = 0; a < lastHitPC.neighbourProvincePEs.Length; a++)
                {
                    //Берём соседнюю провинцию
                    lastHitPC.neighbourProvincePEs[a].Unpack(world.Value, out int neighbourProvinceEntity);
                    ref CProvinceHexasphere neighbourPHS = ref pHSPool.Value.Get(neighbourProvinceEntity);

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
                    return hexasphereInputData.Value.lastHitProvincePE;
                }
            }

            //Ищем текущую провинцию под курсором среди всех
            //Берём первую провинцию карты
            activeMap.provincePEs[0].Unpack(world.Value, out int nearestProvinceEntity);
            ref CProvinceCore nearestPC = ref pCPool.Value.Get(nearestProvinceEntity);

            //Определяем минимальное расстояние как максимально возможное
            float minDistance = float.MaxValue;

            //Для каждой провинции карты
            for(int a = 0; a < activeMap.provincePEs.Length; a++)
            {
                //Берём ближайшую провинцию среди соседей
                ProvinceGetNearestNeighbourToPosition(
                    ref nearestPC,
                    localPosition,
                    out float provinceDistance).Unpack(world.Value, out int newNearestProvinceEntity);

                //Если расстояние меньше минимального
                if (provinceDistance < minDistance)
                {
                    //Обновляем провинцию и минимальное расстояние 
                    nearestPC = ref pCPool.Value.Get(newNearestProvinceEntity);
                    minDistance = provinceDistance;
                }
                //Иначе выходим из цикла
                else
                {
                    break;
                }
            }

            //PE последней провинции - это PE ближайшего
            hexasphereInputData.Value.lastHitProvincePE = nearestPC.selfPE;

            return hexasphereInputData.Value.lastHitProvincePE;
        }

        /// <summary>
        /// Определение провинции, ближайшей к точке, из соседей переданной провинции
        /// </summary>
        /// <param name="pC"></param>
        /// <param name="localPosition"></param>
        /// <param name="minDistance"></param>
        /// <returns></returns>
        EcsPackedEntity ProvinceGetNearestNeighbourToPosition(
            ref CProvinceCore pC,
            Vector3 localPosition,
            out float minDistance)
        {
            //Определяем минимальное расстояние как максимально возможное
            minDistance = float.MaxValue;

            //Создаём переменную для PE итоговой провинции
            EcsPackedEntity nearestProvincePE = new();

            //Для каждой соседней провинции
            for (int a = 0; a < pC.neighbourProvincePEs.Length; a++)
            {
                //Берём соседнюю провинцию
                pC.neighbourProvincePEs[a].Unpack(world.Value, out int neighbourProvinceEntity);
                ref CProvinceHexasphere neighbourPHS = ref pHSPool.Value.Get(neighbourProvinceEntity);

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
                    nearestProvincePE = neighbourPHS.selfPE;
                    minDistance = distance;
                }
            }

            //Возвращаем ближайшего к переданной точке соседа
            return nearestProvincePE;
        }

        readonly EcsPoolInject<GBB.Input.RMousePositionChange> mousePositionChangeRPool = default;
        void MousePositionRequest()
        {
            //Создаём новую сущность и назначаем ей запрос смены положения курсора
            int requestEntity = world.Value.NewEntity();
            ref GBB.Input.RMousePositionChange requestComp = ref mousePositionChangeRPool.Value.Add(requestEntity);

            //Заполняем данные запроса
            requestComp = new GBB.Input.RMousePositionChange(
                hexasphereInputData.Value.isMouseOverMap,
                hexasphereInputData.Value.lastHitProvincePE);
        }
    }
}
