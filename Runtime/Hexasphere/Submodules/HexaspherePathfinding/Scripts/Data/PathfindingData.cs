
using System;
using System.Collections.Generic;

using UnityEngine;

using Leopotam.EcsLite;

using GBB;

namespace HS.Pathfinding
{
    public class PathfindingData : MonoBehaviour
    {
        public static List<int> GetCellIndicesWithinSteps(
            EcsWorld world,
            ref CMapPathfinding mapPF, int modulePathfindingIndex,
            EcsPool<CCellPathfinding> cPFPool, ref EcsPackedEntity[] cellPEs,
            ref CCellPathfinding startCell,
            int maxSteps)
        {
            //������ ������ ����������
            List<int> candidates = ListPool<int>.Get();

            //��� ������� ������ ������
            for(int a = 0; a < startCell.neighbourCellPEs.Length; a++)
            {
                //���� ������
                startCell.neighbourCellPEs[a].Unpack(world, out int neighbourCellEntity);
                ref CCellPathfinding neighbourCell = ref cPFPool.Get(neighbourCellEntity);

                //������� ��� ������ � ������ ����������
                candidates.Add(neighbourCell.index);
            }

            //������ ������ ��� ������������ �����
            HashSet<int> processed = new(startCell.index);
            //������� ��������� ������ � ������
            processed.Add(startCell.index);

            //������ �������� ������
            List<int> results = ListPool<int>.Get();

            //���������� ������ ���������� ���������
            int candidatesLast = candidates.Count - 1;

            //���� �� ���������� ��������� ������ � ������
            while(candidatesLast >= 0)
            {
                //���� ���������� ���������
                int candidateIndex = candidates[candidatesLast];
                candidates.RemoveAt(candidatesLast);
                candidatesLast--;
                cellPEs[candidateIndex].Unpack(world, out int candidateCellEntity);
                ref CCellPathfinding candidateCell = ref cPFPool.Get(candidateCellEntity);

                //���� ������� ��� �� �������� ���
                if(processed.Contains(candidateIndex) == false)
                {
                    //������� ���� �� ����
                    List<int> path = PathFind(
                        world,
                        mapPF.modulesPathfindingData[modulePathfindingIndex],
                        cPFPool, ref cellPEs,
                        ref startCell, ref candidateCell,
                        maxSteps);

                    //���� ���������� ����
                    if(path != null)
                    {
                        //������� ��������� � �������� ������ � ������
                        results.Add(candidateCell.index);
                        processed.Add(candidateCell.index);

                        //��� ������� ������ ���������
                        for(int a = 0; a < candidateCell.neighbourCellPEs.Length; a++)
                        {
                            //���� ������
                            candidateCell.neighbourCellPEs[a].Unpack(world, out int neighbourCellEntity);
                            ref CCellPathfinding neighbourCell = ref cPFPool.Get(neighbourCellEntity);

                            //���� ������ �� �������� ���
                            if(processed.Contains(neighbourCell.index) == false)
                            {
                                //������� ��� � ������ ���������� � ����������� �������
                                candidates.Add(neighbourCell.index);
                                candidatesLast++;
                            }
                        }

                        //���������� ������ � ���
                        ListPool<int>.Add(path);
                    }
                }
            }

            //���������� ������ ���������� � ���
            ListPool<int>.Add(candidates);

            return results;
        }

        public static List<int> GetCellIndicesWithinSteps(
            EcsWorld world,
            ref CMapPathfinding mapPF, int modulePathfindingIndex,
            EcsPool<CCellPathfinding> cPFPool, ref EcsPackedEntity[] cellPEs,
            ref CCellPathfinding startCell,
            int minSteps, int maxSteps)
        {
            //������ ������������� ������
            List<int> candidates = ListPool<int>.Get();

            //��� ������� ������ ������
            for (int a = 0; a < startCell.neighbourCellPEs.Length; a++)
            {
                //���� ������
                startCell.neighbourCellPEs[a].Unpack(world, out int neighbourCellEntity);
                ref CCellPathfinding neighbourCell = ref cPFPool.Get(neighbourCellEntity);

                //������� ������ ������ � ������ ����������
                candidates.Add(neighbourCell.index);
            }

            //������ ������ ��� ������������ �����
            HashSet<int> processed = new(startCell.index);
            //������� ��������� ������ � ������
            processed.Add(startCell.index);

            //������ �������� ������
            List<int> results = ListPool<int>.Get();

            //������ �������� ������� ��� �������������� �����
            int candidatesLast = candidates.Count - 1;

            //���� �� ���������� ��������� ������ � ������
            while (candidatesLast >= 0)
            {
                //���� ���������� ���������
                int candidateIndex = candidates[candidatesLast];
                candidates.RemoveAt(candidatesLast);
                candidatesLast--;
                cellPEs[candidateIndex].Unpack(world, out int candidateCellEntity);
                ref CCellPathfinding candidateCell = ref cPFPool.Get(candidateCellEntity);

                //���� ������ ��� �� �������� ���
                if (processed.Contains(candidateIndex) == false)
                {
                    //������� ���� �� ����
                    List<int> path = PathFind(
                        world,
                        mapPF.modulesPathfindingData[modulePathfindingIndex],
                        cPFPool, ref cellPEs,
                        ref startCell, ref candidateCell,
                        maxSteps);

                    //���� ���������� ���� 
                    if (path != null)
                    {
                        //������� ��������� � ������
                        processed.Add(candidateCell.index);

                        //���� ����� ���� ������ ��� ����� ����������� � ������ ��� ����� ������������
                        if (path.Count >= minSteps && path.Count <= maxSteps)
                        {
                            //������� ��������� � �������� ������
                            results.Add(candidateCell.index);
                        }

                        //��� ������� ������ ���������
                        for (int a = 0; a < candidateCell.neighbourCellPEs.Length; a++)
                        {
                            //���� ������
                            candidateCell.neighbourCellPEs[a].Unpack(world, out int neighbourCellEntity);
                            ref CCellPathfinding neighbourCell = ref cPFPool.Get(neighbourCellEntity);

                            //���� ������ �� �������� ���
                            if (processed.Contains(neighbourCell.index) == false)
                            {
                                //������� ��� � ������ ���������� � ����������� �������
                                candidates.Add(neighbourCell.index);
                                candidatesLast++;
                            }
                        }

                        //���������� ������ � ���
                        ListPool<int>.Add(path);
                    }
                }
            }

            //���������� ������ � ���
            ListPool<int>.Add(candidates);

            return results;
        }

        public static List<int> PathFind(
            EcsWorld world,
            ref CMapPathfinding mapPF, int modulePathfindingIndex,
            EcsPool<CCellPathfinding> cPFPool, ref EcsPackedEntity[] cellPEs,
            ref CCellPathfinding startCell, ref CCellPathfinding endCell,
            int maxSteps = 0)
        {
            //������ ������ ��� �������� ����� ����
            List<int> results = ListPool<int>.Get();

            //������� ���� � ���������� ���������� ����� � ����
            int cellsCount = PathFind(
                world,
                mapPF.modulesPathfindingData[modulePathfindingIndex],
                cPFPool, ref cellPEs,
                ref startCell, ref endCell,
                results,
                maxSteps);

            //���� ���������� ����� ����, �� ���������� ������ ������
            return cellsCount == 0 ? null : results;
        }

        public static List<int> PathFind(
            EcsWorld world,
            DModulePathfinding moduleData,
            EcsPool<CCellPathfinding> cPFPool, ref EcsPackedEntity[] cellPEs,
            ref CCellPathfinding startCell, ref CCellPathfinding endCell,
            int maxSteps = 0)
        {
            //������ ������ ��� �������� ����� ����
            List<int> results = ListPool<int>.Get();

            //������� ���� � ���������� ���������� ����� � ����
            int cellsCount = PathFind(
                world,
                moduleData,
                cPFPool, ref cellPEs,
                ref startCell, ref endCell,
                results,
                maxSteps);

            //���� ���������� ����� ����, �� ���������� ������ ������
            return cellsCount == 0 ? null : results;
        }

        static int PathFind(
            EcsWorld world,
            DModulePathfinding moduleData,
            EcsPool<CCellPathfinding> cellPathfindingPool, ref EcsPackedEntity[] cellPEs,
            ref CCellPathfinding startCell, ref CCellPathfinding endCell,
            List<int> results,
            int maxSteps = 0)
        {
            //������� ������
            results.Clear();

            //���� ��������� ������ �� ����� ��������
            if(startCell.index != endCell.index)
            {
                //������������ ������� ����
                PathMatrixRefresh(
                    moduleData, 
                    cellPEs.Length);

                //���������� ������������ ���������� ����� ��� ������
                moduleData.pathfindingSearchLimit = maxSteps == 0 ? DModulePathfinding.pathfindingSearchLimitBase : maxSteps;

                //������� ����
                List<DPathfindingClosedNode> path = PathFindFast(
                    world,
                    moduleData,
                    cellPathfindingPool, ref cellPEs,
                    ref startCell, ref endCell);

                //���� ���� �� ����
                if(path != null)
                {
                    //��� ������ ������ � ����, ����� ���� ���������, � �������� �������
                    for(int a = path.Count - 2; a > 0; a--)
                    {
                        //������� ��� � ������ ��������
                        results.Add(path[a].index);
                    }
                    //������� � ������ �������� ������ ��������� ������
                    results.Add(endCell.index);
                }
                //�����
                else
                {
                    //���������� 0, ���������, ��� ���� ����
                    return 0;
                }
            }

            //���������� ���������� ����� � ����
            return results.Count;
        }

        static void PathMatrixRefresh(
            DModulePathfinding moduleData,
            int cellsCount)
        {
            //���� ������� ���� �� ������� ����������, �� ������� �� �������
            if(moduleData.pathMatrixUpdated == false)
            {
                return;
            }

            //��������, ��� ������� ���� �� ������� ����������
            moduleData.pathMatrixUpdated = false;

            //���� ������ ��� ������ ����
            if(moduleData.pfCalc == null)
            {
                //������ ������
                moduleData.pfCalc = new DPathfindingNodeFast[cellsCount];

                //������ �������
                moduleData.open = new(
                    new PathfindingNodesComparer(moduleData.pfCalc),
                    cellsCount);
            }
            //�����
            else
            {
                //������� ������� � ������
                moduleData.open.Clear();
                Array.Clear(moduleData.pfCalc, 0, moduleData.pfCalc.Length);

                //��������� ���������� ����� � �������
                PathfindingNodesComparer comparer = (PathfindingNodesComparer)moduleData.open.Comparer;
                comparer.SetMatrix(moduleData.pfCalc);
            }
        }

        static List<DPathfindingClosedNode> PathFindFast(
            EcsWorld world,
            DModulePathfinding moduleData,
            EcsPool<CCellPathfinding> cellPathfindingPool, ref EcsPackedEntity[] cellPEs,
            ref CCellPathfinding startCell, ref CCellPathfinding endCell)
        {
            //������ ���������� ��� �������� ���������� ����
            bool isPathFound = false;

            //���� ���� ������ ������ 250
            if (moduleData.openCellValue > 250)
            {
                //�������� ����
                moduleData.openCellValue = 1;
                moduleData.closeCellValue = 2;
            }
            //�����
            else
            {
                //��������� ����
                moduleData.openCellValue += 2;
                moduleData.closeCellValue += 2;
            }
            //������� ������� � ����
            moduleData.open.Clear();
            moduleData.close.Clear();

            //���� ����� �������� ������
            Vector3 destinationCenter = endCell.center;

            //�������� ������ ��������� ������ � �������
            moduleData.pfCalc[startCell.index].distance = 0;
            moduleData.pfCalc[startCell.index].priority = 2;
            moduleData.pfCalc[startCell.index].prevIndex = startCell.index;
            moduleData.pfCalc[startCell.index].status = moduleData.openCellValue;
            moduleData.pfCalc[startCell.index].steps = 0;

            //������� ��������� ������ � �������
            moduleData.open.Push(startCell.index);

            //���� � ������� ���� ������
            while (moduleData.open.cellsCount > 0)
            {
                //���� ������ ������ � ������� ��� �������
                int currentCellIndex = moduleData.open.Pop();

                //���� ������ ������ ��� ����� �� ������� ������, �� ��������� � ���������
                if (moduleData.pfCalc[currentCellIndex].status == moduleData.closeCellValue)
                {
                    continue;
                }

                //���� ������ ������ ����� ������� �������� ������
                if (currentCellIndex == endCell.index)
                {
                    //������� ������ �� ������� ������
                    moduleData.pfCalc[currentCellIndex].status = moduleData.closeCellValue;

                    //��������, ��� ���� ������, � ������� �� �����
                    isPathFound = true;

                    break;
                }

                //���� ������� ����� ������ �������
                if (moduleData.pfCalc[currentCellIndex].steps >= moduleData.pathfindingSearchLimit)
                {
                    continue;
                }

                //���� ������� ������
                cellPEs[currentCellIndex].Unpack(world, out int currentCellEntity);
                ref CCellPathfinding currentCell = ref cellPathfindingPool.Get(currentCellEntity);

                //��� ������� ������ ������� ������
                for (int a = 0; a < currentCell.neighbourCellPEs.Length; a++)
                {
                    //���� ������
                    currentCell.neighbourCellPEs[a].Unpack(world, out int neighbourCellEntity);
                    ref CCellPathfinding neighbourCell = ref cellPathfindingPool.Get(neighbourCellEntity);

                    //������������ ���������� �� ������
                    float newDistance = moduleData.pfCalc[currentCellIndex].distance + neighbourCell.crossCost;

                    //���� ������ ��������� � ������� ������ ��� ��� �������� �� �������
                    if (moduleData.pfCalc[neighbourCell.index].status == moduleData.openCellValue
                        || moduleData.pfCalc[neighbourCell.index].status == moduleData.closeCellValue)
                    {
                        //���� ���������� �� ������ ������ ��� ����� ������, �� ��������� � ���������� ������
                        if (moduleData.pfCalc[neighbourCell.index].distance <= newDistance)
                        {
                            continue;
                        }
                    }
                    //����� ��������� ����������

                    //��������� ������ ���������� ������ � ����������
                    moduleData.pfCalc[neighbourCell.index].prevIndex = currentCellIndex;
                    moduleData.pfCalc[neighbourCell.index].distance = newDistance;
                    moduleData.pfCalc[neighbourCell.index].steps = moduleData.pfCalc[currentCellIndex].steps + 1;

                    //������������ ��������� ������
                    //������������ ����, ������������ � �������� ���������
                    float angle = Vector3.Angle(destinationCenter, neighbourCell.center);

                    //��������� ��������� ������ � ������
                    moduleData.pfCalc[neighbourCell.index].priority = newDistance + 2f * angle;
                    moduleData.pfCalc[neighbourCell.index].status = moduleData.openCellValue;
                    
                    //������� ������ � �������
                    moduleData.open.Push(neighbourCell.index);
                }

                //������� ������� ������ �� ������� ������
                moduleData.pfCalc[currentCellIndex].status = moduleData.closeCellValue;
            }

            //���� ���� ������
            if (isPathFound == true)
            {
                //������� ������ ����
                moduleData.close.Clear();

                //���� ������ �������� ������ �� ��������� ���������-������ ��� ������� ��������� ������
                ref DPathfindingNodeFast currentFindedCell = ref moduleData.pfCalc[endCell.index];

                //������ ��������� ��� �������� ������ ������� ������
                DPathfindingClosedNode currentResultCell;

                //��������� ������ �� ��������� � ��������
                currentResultCell.priority = currentFindedCell.priority;
                currentResultCell.distance = currentFindedCell.distance;
                currentResultCell.prevIndex = currentFindedCell.prevIndex;
                currentResultCell.index = endCell.index;

                //���� ������ �������� ������ �� ����� ������� ����������,
                //�� ���� ���� �� ���������� ��������� ������
                while (currentResultCell.index != currentResultCell.prevIndex)
                {
                    //������� �������� ������ � ������ ����
                    moduleData.close.Add(currentResultCell);

                    //���� ������ ���������� ��������� ������
                    int prevCellIndex = currentResultCell.prevIndex;
                    currentFindedCell = ref moduleData.pfCalc[prevCellIndex];

                    //��������� ������ �� ��������� � ��������
                    currentResultCell.priority = currentFindedCell.priority;
                    currentResultCell.distance = currentFindedCell.distance;
                    currentResultCell.prevIndex = currentFindedCell.prevIndex;
                    currentResultCell.index = prevCellIndex;
                }
                //������� ��������� �������� ������ � ������ ����
                moduleData.close.Add(currentResultCell);

                //���������� ������ ����
                return moduleData.close;
            }

            return null;
        }
    }
}
