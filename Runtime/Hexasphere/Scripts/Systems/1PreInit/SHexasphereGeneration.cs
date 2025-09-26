
using System.Collections.Generic;

using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

using GBB;
using GBB.Map;

namespace HS
{
    public class SHexasphereGeneration : IEcsInitSystem
    {
        readonly EcsWorldInject world = default;


        readonly EcsFilterInject<Inc<CProvinceHexasphere>, Exc<SRProvinceCoreCreation>> provinceWithoutPCCreationSRFilter = default;
        readonly EcsPoolInject<CProvinceHexasphere> pHSPool = default;
        readonly EcsFilterInject<Inc<SRProvinceCoreCreation>> pCCreationSRFilter = default;
        readonly EcsPoolInject<SRProvinceCoreCreation> pCCreationSRPool = default;


        readonly EcsCustomInject<HexasphereData> hexasphereData = default;

        public void Init(IEcsSystems systems)
        {
            //���������� ����������
            HexasphereGeneration();
        }

        readonly EcsFilterInject<Inc<SRHexasphereGeneration>> mapHexasphereGenerationSRFilter = default;
        readonly EcsPoolInject<SRHexasphereGeneration> hexasphereGenerationSRPool = default;
        void HexasphereGeneration()
        {
            //��� ������ ����� � �������� ��������� ����������
            foreach (int mapEntity in mapHexasphereGenerationSRFilter.Value)
            {
                //���� ������
                ref SRHexasphereGeneration requestComp = ref hexasphereGenerationSRPool.Value.Get(mapEntity);

                //�������������� ������ ����������
                HexasphereInitialize();

                //���������� ������� ����������
                HexasphereGenerate(ref requestComp);

                //������������ ������� PHS � ����������� �������� PC
                ProvinceHexashpereCalculateNeighbours(ref requestComp);

                //������ PC �� �������
                ProvincesCoreCreation(
                    mapEntity);

                //������� ������
                hexasphereGenerationSRPool.Value.Del(mapEntity);
            }
        }

        void HexasphereInitialize()
        {

        }

        void HexasphereGenerate(
            ref SRHexasphereGeneration requestComp)
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

            //���������� ������������ ������������ ���������
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

            //������� ������� �����
            hexasphereData.Value.points.Clear();

            //������� ������� ��������� � �������
            for (int a = 0; a < corners.Length; a++)
            {
                hexasphereData.Value.points[corners[a]] = corners[a];
            }

            //������ ������ ����� ������� ����� ������������
            List<DHexaspherePoint> bottom = ListPool<DHexaspherePoint>.Get();
            //���������� ���������� �������������
            int triangleCount = triangles.Length;
            //��� ������� ������������
            for (int a = 0; a < triangleCount; a++)
            {
                //������ ������ ������ �����
                List<DHexaspherePoint> previous;

                //���� ������ ������� ������������
                DHexaspherePoint point0 = triangles[a].points[0];

                //������� ��������� ������ �����
                bottom.Clear();

                //������� � ������ ������ ������� ������������
                bottom.Add(point0);

                //������ ������ ����� ������ ����� ������������
                List<DHexaspherePoint> left = PointSubdivide(
                    point0, triangles[a].points[1],
                    requestComp.subdivisions);
                //������ ������ ����� ������� ����� ������������
                List<DHexaspherePoint> right = PointSubdivide(
                    point0, triangles[a].points[2],
                    requestComp.subdivisions);

                //��� ������� �������������
                for (int b = 1; b <= requestComp.subdivisions; b++)
                {
                    //��������� ������ ����� ������� �����
                    previous = bottom;

                    //������������ ��������� � ������ ����� �� �������
                    bottom = PointSubdivide(
                        left[b], right[b],
                        b);

                    //������ ����� �����������
                    new DHexasphereTriangle(previous[0], bottom[0], bottom[1]);

                    //��� ������� ...
                    for (int c = 1; c < b; c++)
                    {
                        //������ ��� ����� ������������
                        new DHexasphereTriangle(previous[c], bottom[c], bottom[c + 1]);
                        new DHexasphereTriangle(previous[c - 1], previous[c], bottom[c]);
                    }
                }

                //���������� ������ � ���
                ListPool<DHexaspherePoint>.Add(left);
                ListPool<DHexaspherePoint>.Add(right);
            }

            //���������� ������ � ���
            ListPool<DHexaspherePoint>.Add(bottom);

            //������ ���������
            //�������� ���� �����
            DHexaspherePoint.flag = 0;

            //��� ������ ������� � �������
            foreach (DHexaspherePoint point in hexasphereData.Value.points.Values)
            {
                //������ ��� ��������� ���������� ����������
                ProvinceHexashpereCreation(point);
            }
        }

        List<DHexaspherePoint> PointSubdivide(
            DHexaspherePoint startPoint, DHexaspherePoint endPoint,
            int count)
        {
            //������ ������ �����, ������������ �������� �����, � ������� � ���� ������� �������
            List<DHexaspherePoint> segments = ListPool<DHexaspherePoint>.Get();
            segments.Add(startPoint);

            //������������ ���������� �����
            double dx = endPoint.x - startPoint.x;
            double dy = endPoint.y - startPoint.y;
            double dz = endPoint.z - startPoint.z;
            double doublex = startPoint.x;
            double doubley = startPoint.y;
            double doublez = startPoint.z;
            double doubleCount = count;

            //��� ������� �������������
            for (int a = 1; a < count; a++)
            {
                //������ ����� �������
                DHexaspherePoint newPoint = new(
                    (float)(doublex + dx * a / doubleCount),
                    (float)(doubley + dy * a / doubleCount),
                    (float)(doublez + dz * a / doubleCount));

                //��������� �������
                newPoint = PointGetCached(newPoint);

                //������� ������� � ������
                segments.Add(newPoint);
            }

            //������� � ������ �������� �������
            segments.Add(endPoint);

            //���������� ������ ������
            return segments;
        }

        DHexaspherePoint PointGetCached(
            DHexaspherePoint point)
        {
            //���� ����������� ������� ���������� � �������
            if (hexasphereData.Value.points.TryGetValue(point, out DHexaspherePoint thePoint))
            {
                //���������� �������
                return thePoint;
            }
            //�����
            else
            {
                //��������� ������� � �������
                hexasphereData.Value.points[point] = point;

                //���������� �������
                return point;
            }
        }

        void ProvinceHexashpereCreation(
            DHexaspherePoint centerPoint)
        {
            //������ ����� �������� � ��������� �� ��������� PHS
            int provinceEntity = world.Value.NewEntity();
            ref CProvinceHexasphere currentPHS = ref pHSPool.Value.Add(provinceEntity);

            //��������� �������� ������ PHS
            currentPHS = new(
                world.Value.PackEntity(provinceEntity),
                centerPoint);
        }

        void ProvinceHexashpereCalculateNeighbours(
            ref SRHexasphereGeneration hexasphereGenerationRequestComp)
        {
            //������ ��������� ������ ��� �������
            List<EcsPackedEntity> tempNeighbours = ListPool<EcsPackedEntity>.Get();

            //��� ������ ��������� ��� ������� �������� PC
            foreach (int provinceEntity in provinceWithoutPCCreationSRFilter.Value)
            {
                //���� PHS
                ref CProvinceHexasphere pHS = ref pHSPool.Value.Get(provinceEntity);

                //������������ �������

                //������� ��������� ������
                tempNeighbours.Clear();

                //���� ������ ����������� � ������ ���������
                DHexasphereTriangle firstTriangle = pHS.centerPoint.triangles[0];

                //��� ������ ������� ������������
                for (int a = 0; a < firstTriangle.points.Length; a++)
                {
                    //���� ��� �� ������� ���������
                    if (firstTriangle.points[a].provincePE.EqualsTo(pHS.selfPE) == false)
                    {
                        //������� ��������� � ������
                        tempNeighbours.Add(firstTriangle.points[a].provincePE);
                    }
                }

                //���� ���������� ������� �� ��������� ������ ������ ���������� �������������
                while (tempNeighbours.Count < pHS.centerPoint.triangleCount)
                {
                    //������ ���������� ��� ����� ���������
                    EcsPackedEntity newProvincePE = new();

                    //��� ������� ������������ ���������
                    for (int a = 0; a < pHS.centerPoint.triangleCount; a++)
                    {
                        //���� �����������
                        DHexasphereTriangle triangle = pHS.centerPoint.triangles[a];

                        //���������, �������� �� ���� ����������� ��������� �� �������
                        bool isContainPreviousProvince = false;
                        bool isContainNewProvince = false;

                        //��� ������ ������� ������������
                        for (int b = 0; b < triangle.points.Length; b++)
                        {
                            //���� ��� �� ������� ���������
                            if (triangle.points[b].provincePE.EqualsTo(pHS.selfPE) == false)
                            {
                                //���� PE ���������
                                EcsPackedEntity trianglePointProvincePE = triangle.points[b].provincePE;

                                //���� ��� ��������� - ��������� �� ��������� ������
                                if (tempNeighbours[tempNeighbours.Count - 1].EqualsTo(trianglePointProvincePE) == true)
                                {
                                    //��������, ��� ����������� �������� ���������� ���������
                                    isContainPreviousProvince = true;
                                }
                                //�����, ���� ���� ��������� ��� �� ��������� ������
                                else if (tempNeighbours.Contains(trianglePointProvincePE) == false)
                                {
                                    //��������, ��� ����������� �������� ����� ���������
                                    isContainNewProvince = true;

                                    //��������� PE ����� ���������
                                    newProvincePE = trianglePointProvincePE;
                                }
                            }
                        }

                        //���� ���� ����������� ������������� ����� ��������, �� �� �������� ���������
                        if (isContainPreviousProvince == true
                            && isContainNewProvince == true)
                        {
                            //������� �� �����
                            break;
                        }
                    }

                    //������� ����� ��������� �� ��������� ������
                    tempNeighbours.Add(newProvincePE);
                }

                //����������� �������� PC �� PHS
                ProvinceData.ProvinceCoreCreationRequest(
                    pCCreationSRPool.Value,
                    provinceEntity,
                    hexasphereGenerationRequestComp.mapPE,
                    tempNeighbours);
            }

            //���������� ������ � ���
            ListPool<EcsPackedEntity>.Add(tempNeighbours);
        }

        readonly EcsPoolInject<CMap> mapPool = default;
        readonly EcsPoolInject<CProvinceCore> pCPool = default;
        void ProvincesCoreCreation(
            int mapEntity)
        {
            //������ ��������� ������ ���������
            List<EcsPackedEntity> tempProvinces = new();

            //���� �����
            ref CMap map = ref mapPool.Value.Get(mapEntity);

            //��� ������ ��������� � �������� �������� PC
            foreach(int provinceEntity in pCCreationSRFilter.Value)
            {
                //���� ������
                ref SRProvinceCoreCreation requestComp = ref pCCreationSRPool.Value.Get(provinceEntity);

                //������ PC �� �������
                ProvinceData.ProvinceCoreCreation(
                    world.Value,
                    ref requestComp,
                    provinceEntity,
                    pCPool.Value,
                    tempProvinces);

                //������� ������
                pCCreationSRPool.Value.Del(provinceEntity);
            }

            //��������� ������ ��� ������ ��������� �����
            map.provincePEs = tempProvinces.ToArray();
        }
    }
}
