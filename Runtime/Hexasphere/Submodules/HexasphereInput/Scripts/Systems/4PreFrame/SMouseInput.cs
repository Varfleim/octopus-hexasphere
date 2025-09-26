
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
            //��� ������ �������� �����
            foreach(int activeMapEntity in activeMapFilter.Value)
            {
                //���� �����
                ref CMap activeMap = ref mapPool.Value.Get(activeMapEntity);

                //��������� ��������� �������
                MousePositionCheck(ref activeMap);

                //�������� ������ ����� � ��������� �������
                MousePositionRequest();
            }
        }

        void MousePositionCheck(
            ref CMap activeMap)
        {
            //����������, ��������� �� ������ ��� �����������
            hexasphereInputData.Value.isMouseOverMap = MouseGetHitPoint(out Vector3 position, out Ray ray);

            //���� ������ �� ��������� ��� �������� ���������� � ��������� ��� ������
            if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject() == false
                && hexasphereInputData.Value.isMouseOverMap == true)
            {
                //���� ���������, ��� ������� ��������� ������, ����������
                if (ProvinceGetInRayDirection(
                    ref activeMap,
                    ray, position,
                    out Vector3 hitPosition).Unpack(world.Value, out int provinceEntity))
                {
                    //���� ���������
                    ref CProvinceHexasphere pHS = ref pHSPool.Value.Get(provinceEntity);

                }
            }
            //�����
            else
            {
                //������ ����� �� ��������� ��� �����������
                hexasphereInputData.Value.isMouseOverMap = false;

                //������� ��������� ���������
                hexasphereInputData.Value.lastHitProvincePE = new();
            }
        }

        bool MouseGetHitPoint(
            out Vector3 position,
            out Ray ray)
        {
            //���� ��� �� ��������� �������
            ray = Camera.main.ScreenPointToRay(UnityEngine.Input.mousePosition);

            //���� ��� �������� �������
            if (Physics.Raycast(ray, out RaycastHit hit)
                //� ���� �������� �������� ����������
                && hit.collider.gameObject == HexasphereData.HexasphereGO)
            {
                //���������� ����� �������
                position = hit.point;

                //� ���������� true
                return true;
            }
            //�����
            else
            {
                //������������ ����� ������� � ����
                position = Vector3.zero;

                //� ���������� false
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

            //���������� �������� ����� �������
            Vector3 minPoint = worldPosition;
            Vector3 maxPoint = worldPosition + 0.5f * hexasphereData.Value.hexasphereScale * ray.direction;

            float rangeMin = hexasphereData.Value.hexasphereScale * 0.5f;
            rangeMin *= rangeMin;

            float rangeMax = worldPosition.sqrMagnitude;

            float distance;
            Vector3 bestPoint = maxPoint;

            //�������� �����
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

            //���� PE ���������
            EcsPackedEntity nearestProvincePE = ProvinceGetAtLocalPosition(
                ref activeMap,
                HexasphereData.HexasphereGO.transform.InverseTransformPoint(worldPosition));

            //���������� ������ ���������
            Vector3 currentPoint = worldPosition;

            //���� ��������� ���������
            nearestProvincePE.Unpack(world.Value, out int nearestProvinceEntity);
            ref CProvinceRender nearestPR = ref pRPool.Value.Get(nearestProvinceEntity);
            ref CProvinceHexasphere nearestPHS = ref pHSPool.Value.Get(nearestProvinceEntity);

            //���������� ������� ����� ���������
            Vector3 provinceTop = HexasphereData.HexasphereGO.transform.TransformPoint(
                nearestPHS.center * (1.0f + nearestPR.ProvinceHeight * HexasphereData.ExtrudeMultiplier));

            //���������� ������ ��������� � ������ ����
            float provinceHeight = provinceTop.sqrMagnitude;
            float rayHeight = currentPoint.sqrMagnitude;

            float minDistance = 1e6f;
            distance = minDistance;

            //���������� PE ���������-���������
            EcsPackedEntity candidateProvincePE = new();

            const int NUM_STEPS = 10;
            //�������� �����
            for (int a = 1; a <= NUM_STEPS; a++)
            {
                distance = Mathf.Abs(rayHeight - provinceHeight);

                //���� ���������� ������ ������������
                if (distance < minDistance)
                {
                    //��������� ����������� ���������� � ���������
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


                //��������� ��������� ���������
                nearestProvincePE.Unpack(world.Value, out nearestProvinceEntity);
                nearestPR = ref pRPool.Value.Get(nearestProvinceEntity);
                nearestPHS = ref pHSPool.Value.Get(nearestProvinceEntity);

                provinceTop = HexasphereData.HexasphereGO.transform.TransformPoint(
                    nearestPHS.center * (1.0f + nearestPR.ProvinceHeight * HexasphereData.ExtrudeMultiplier));

                //���������� ������ ��������� � ������ ����
                provinceHeight = provinceTop.sqrMagnitude;
                rayHeight = currentPoint.sqrMagnitude;
            }

            //���� ���������� ������ ������������
            if (distance < minDistance)
            {
                //��������� ����������� ���������� � ���������
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
        /// ����������� ���������, ��������� � ����������� �����
        /// </summary>
        /// <param name="activeMap"></param>
        /// <param name="localPosition"></param>
        /// <returns></returns>
        EcsPackedEntity ProvinceGetAtLocalPosition(
            ref CMap activeMap,
            Vector3 localPosition)
        {
            //��������� ���������, �� �������� �� ��������� ��������� ��� �������� �������
            //���� ��������� ��������� ����������
            if(hexasphereInputData.Value.lastHitProvincePE.Unpack(world.Value, out int lastHitProvinceEntity))
            {
                //���� �
                ref CProvinceCore lastHitPC = ref pCPool.Value.Get(lastHitProvinceEntity);
                ref CProvinceHexasphere lastHitPHS = ref pHSPool.Value.Get(lastHitProvinceEntity);

                //���������� ���������� �� ������ ���������
                float distance = Vector3.SqrMagnitude(lastHitPHS.center - localPosition);

                //����������, ��������� �� � ������ ������, ��� ���
                bool isValid = true;

                //��� ������ �������� ���������
                for (int a = 0; a < lastHitPC.neighbourProvincePEs.Length; a++)
                {
                    //���� �������� ���������
                    lastHitPC.neighbourProvincePEs[a].Unpack(world.Value, out int neighbourProvinceEntity);
                    ref CProvinceHexasphere neighbourPHS = ref pHSPool.Value.Get(neighbourProvinceEntity);

                    //���������� ���������� �� ������ ���������
                    float otherDistance = Vector3.SqrMagnitude(neighbourPHS.center - localPosition);

                    //���� ��� ������ ���������� �� ��������� ���������
                    if (otherDistance < distance)
                    {
                        //�������� ��� � ������� �� �����
                        isValid = false;
                        break;
                    }
                }

                //���� ��� ��������� ���������
                if (isValid == true)
                {
                    //���������� � PE
                    return hexasphereInputData.Value.lastHitProvincePE;
                }
            }

            //���� ������� ��������� ��� �������� ����� ����
            //���� ������ ��������� �����
            activeMap.provincePEs[0].Unpack(world.Value, out int nearestProvinceEntity);
            ref CProvinceCore nearestPC = ref pCPool.Value.Get(nearestProvinceEntity);

            //���������� ����������� ���������� ��� ����������� ���������
            float minDistance = float.MaxValue;

            //��� ������ ��������� �����
            for(int a = 0; a < activeMap.provincePEs.Length; a++)
            {
                //���� ��������� ��������� ����� �������
                ProvinceGetNearestNeighbourToPosition(
                    ref nearestPC,
                    localPosition,
                    out float provinceDistance).Unpack(world.Value, out int newNearestProvinceEntity);

                //���� ���������� ������ ������������
                if (provinceDistance < minDistance)
                {
                    //��������� ��������� � ����������� ���������� 
                    nearestPC = ref pCPool.Value.Get(newNearestProvinceEntity);
                    minDistance = provinceDistance;
                }
                //����� ������� �� �����
                else
                {
                    break;
                }
            }

            //PE ��������� ��������� - ��� PE ����������
            hexasphereInputData.Value.lastHitProvincePE = nearestPC.selfPE;

            return hexasphereInputData.Value.lastHitProvincePE;
        }

        /// <summary>
        /// ����������� ���������, ��������� � �����, �� ������� ���������� ���������
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
            //���������� ����������� ���������� ��� ����������� ���������
            minDistance = float.MaxValue;

            //������ ���������� ��� PE �������� ���������
            EcsPackedEntity nearestProvincePE = new();

            //��� ������ �������� ���������
            for (int a = 0; a < pC.neighbourProvincePEs.Length; a++)
            {
                //���� �������� ���������
                pC.neighbourProvincePEs[a].Unpack(world.Value, out int neighbourProvinceEntity);
                ref CProvinceHexasphere neighbourPHS = ref pHSPool.Value.Get(neighbourProvinceEntity);

                //���� ����� ���������
                Vector3 center = neighbourPHS.center;

                //������������ ����������
                float distance
                    = (center.x - localPosition.x) * (center.x - localPosition.x)
                    + (center.y - localPosition.y) * (center.y - localPosition.y)
                    + (center.z - localPosition.z) * (center.z - localPosition.z);

                //���� ���������� ������ ������������
                if (distance < minDistance)
                {
                    //��������� ��������� � ����������� ����������
                    nearestProvincePE = neighbourPHS.selfPE;
                    minDistance = distance;
                }
            }

            //���������� ���������� � ���������� ����� ������
            return nearestProvincePE;
        }

        readonly EcsPoolInject<GBB.Input.RMousePositionChange> mousePositionChangeRPool = default;
        void MousePositionRequest()
        {
            //������ ����� �������� � ��������� �� ������ ����� ��������� �������
            int requestEntity = world.Value.NewEntity();
            ref GBB.Input.RMousePositionChange requestComp = ref mousePositionChangeRPool.Value.Add(requestEntity);

            //��������� ������ �������
            requestComp = new GBB.Input.RMousePositionChange(
                hexasphereInputData.Value.isMouseOverMap,
                hexasphereInputData.Value.lastHitProvincePE);
        }
    }
}
