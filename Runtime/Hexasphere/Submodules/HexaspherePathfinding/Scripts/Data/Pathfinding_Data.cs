
using System;
using System.Collections.Generic;

using UnityEngine;

using Leopotam.EcsLite;

using GBB;

namespace HS.Hexasphere.Pathfinding
{
    public class Pathfinding_Data : MonoBehaviour
    {
        public static List<int> GetCellIndicesWithinSteps(
            EcsWorld world,
            ref C_MapPathfinding mapPF, int modulePathfindingIndex,
            EcsPool<C_CellPathfinding> cPF_P, ref EcsPackedEntity[] cellPEs,
            ref C_CellPathfinding startCell,
            int maxSteps)
        {
            //Создаём список кандидатов
            List<int> candidates = ListPool<int>.Get();

            //Для каждого соседа ячейки
            for(int a = 0; a < startCell.neighbourCellPEs.Length; a++)
            {
                //Берём соседа
                startCell.neighbourCellPEs[a].Unpack(world, out int neighbourCellEntity);
                ref C_CellPathfinding neighbourCell = ref cPF_P.Get(neighbourCellEntity);

                //Заносим его индекс в список кандидатов
                candidates.Add(neighbourCell.index);
            }

            //Создаём хэшсет для обработанных ячеек
            HashSet<int> processed = new(startCell.index);
            //Заносим стартовую ячейку в хэшсет
            processed.Add(startCell.index);

            //Создаём итоговый список
            List<int> results = ListPool<int>.Get();

            //Определяем индекс последнего кандидата
            int candidatesLast = candidates.Count - 1;

            //Пока не достигнута последняя ячейка в списке
            while(candidatesLast >= 0)
            {
                //Берём последнего кандидата
                int candidateIndex = candidates[candidatesLast];
                candidates.RemoveAt(candidatesLast);
                candidatesLast--;
                cellPEs[candidateIndex].Unpack(world, out int candidateCellEntity);
                ref C_CellPathfinding candidateCell = ref cPF_P.Get(candidateCellEntity);

                //Если словарь ещё не содержит его
                if(processed.Contains(candidateIndex) == false)
                {
                    //Находим путь до него
                    List<int> path = PathFind(
                        world,
                        mapPF.modulesPathfindingData[modulePathfindingIndex],
                        cPF_P, ref cellPEs,
                        ref startCell, ref candidateCell,
                        maxSteps);

                    //Если существует путь
                    if(path != null)
                    {
                        //Заносим кандидата в итоговый список и хэшсет
                        results.Add(candidateCell.index);
                        processed.Add(candidateCell.index);

                        //Для каждого соседа кандидата
                        for(int a = 0; a < candidateCell.neighbourCellPEs.Length; a++)
                        {
                            //Берём соседа
                            candidateCell.neighbourCellPEs[a].Unpack(world, out int neighbourCellEntity);
                            ref C_CellPathfinding neighbourCell = ref cPF_P.Get(neighbourCellEntity);

                            //Если хэшсет не содержит его
                            if(processed.Contains(neighbourCell.index) == false)
                            {
                                //Заносим его в список кандидатов и увеличиваем счётчик
                                candidates.Add(neighbourCell.index);
                                candidatesLast++;
                            }
                        }

                        //Возвращаем список в пул
                        ListPool<int>.Add(path);
                    }
                }
            }

            //Возвращаем список кандидатов в пул
            ListPool<int>.Add(candidates);

            return results;
        }

        public static List<int> GetCellIndicesWithinSteps(
            EcsWorld world,
            ref C_MapPathfinding mapPF, int modulePathfindingIndex,
            EcsPool<C_CellPathfinding> cPF_P, ref EcsPackedEntity[] cellPEs,
            ref C_CellPathfinding startCell,
            int minSteps, int maxSteps)
        {
            //Создаём промежуточный список
            List<int> candidates = ListPool<int>.Get();

            //Для каждого соседа ячейки
            for (int a = 0; a < startCell.neighbourCellPEs.Length; a++)
            {
                //Берём соседа
                startCell.neighbourCellPEs[a].Unpack(world, out int neighbourCellEntity);
                ref C_CellPathfinding neighbourCell = ref cPF_P.Get(neighbourCellEntity);

                //Заносим индекс соседа в список кандидатов
                candidates.Add(neighbourCell.index);
            }

            //Создаём хэшсет для обработанных ячеек
            HashSet<int> processed = new(startCell.index);
            //Заносим стартовую ячейку в хэшсет
            processed.Add(startCell.index);

            //Создаём итоговый список
            List<int> results = ListPool<int>.Get();

            //Создаём обратный счётчик для обрабатываемых ячеек
            int candidatesLast = candidates.Count - 1;

            //Пока не достигнута последняя ячейка в списке
            while (candidatesLast >= 0)
            {
                //Берём последнего кандидата
                int candidateIndex = candidates[candidatesLast];
                candidates.RemoveAt(candidatesLast);
                candidatesLast--;
                cellPEs[candidateIndex].Unpack(world, out int candidateCellEntity);
                ref C_CellPathfinding candidateCell = ref cPF_P.Get(candidateCellEntity);

                //Если хэшсет ещё не содержит его
                if (processed.Contains(candidateIndex) == false)
                {
                    //Находим путь до него
                    List<int> path = PathFind(
                        world,
                        mapPF.modulesPathfindingData[modulePathfindingIndex],
                        cPF_P, ref cellPEs,
                        ref startCell, ref candidateCell,
                        maxSteps);

                    //Если существует путь 
                    if (path != null)
                    {
                        //Заносим кандидата в хэшсет
                        processed.Add(candidateCell.index);

                        //Если длина пути больше или равна минимальной и меньше или равна максимальной
                        if (path.Count >= minSteps && path.Count <= maxSteps)
                        {
                            //Заносим кандидата в итоговый список
                            results.Add(candidateCell.index);
                        }

                        //Для каждого соседа кандидата
                        for (int a = 0; a < candidateCell.neighbourCellPEs.Length; a++)
                        {
                            //Берём соседа
                            candidateCell.neighbourCellPEs[a].Unpack(world, out int neighbourCellEntity);
                            ref C_CellPathfinding neighbourCell = ref cPF_P.Get(neighbourCellEntity);

                            //Если хэшсет не содержит его
                            if (processed.Contains(neighbourCell.index) == false)
                            {
                                //Заносим его в список кандидатов и увеличиваем счётчик
                                candidates.Add(neighbourCell.index);
                                candidatesLast++;
                            }
                        }

                        //Возвращаем список в пул
                        ListPool<int>.Add(path);
                    }
                }
            }

            //Возвращаем список в пул
            ListPool<int>.Add(candidates);

            return results;
        }

        public static List<int> PathFind(
            EcsWorld world,
            ref C_MapPathfinding mapPF, int modulePathfindingIndex,
            EcsPool<C_CellPathfinding> cPF_P, ref EcsPackedEntity[] cellPEs,
            ref C_CellPathfinding startCell, ref C_CellPathfinding endCell,
            int maxSteps = 0)
        {
            //Создаём список для индексов ячеек пути
            List<int> results = ListPool<int>.Get();

            //Находим путь и определяем количество ячеек в пути
            int cellsCount = PathFind(
                world,
                mapPF.modulesPathfindingData[modulePathfindingIndex],
                cPF_P, ref cellPEs,
                ref startCell, ref endCell,
                results,
                maxSteps);

            //Если количество равно нулю, то возвращаем пустой список
            return cellsCount == 0 ? null : results;
        }

        public static List<int> PathFind(
            EcsWorld world,
            D_ModulePathfinding moduleData,
            EcsPool<C_CellPathfinding> cPF_P, ref EcsPackedEntity[] cellPEs,
            ref C_CellPathfinding startCell, ref C_CellPathfinding endCell,
            int maxSteps = 0)
        {
            //Создаём список для индексов ячеек пути
            List<int> results = ListPool<int>.Get();

            //Находим путь и определяем количество ячеек в пути
            int cellsCount = PathFind(
                world,
                moduleData,
                cPF_P, ref cellPEs,
                ref startCell, ref endCell,
                results,
                maxSteps);

            //Если количество равно нулю, то возвращаем пустой список
            return cellsCount == 0 ? null : results;
        }

        static int PathFind(
            EcsWorld world,
            D_ModulePathfinding moduleData,
            EcsPool<C_CellPathfinding> cPF_P, ref EcsPackedEntity[] cellPEs,
            ref C_CellPathfinding startCell, ref C_CellPathfinding endCell,
            List<int> results,
            int maxSteps = 0)
        {
            //Очищаем список
            results.Clear();

            //Если стартовая ячейка не равна конечной
            if(startCell.index != endCell.index)
            {
                //Рассчитываем матрицу пути
                PathMatrixRefresh(
                    moduleData, 
                    cellPEs.Length);

                //Определяем максимальное количество шагов при поиске
                moduleData.pathfindingSearchLimit = maxSteps == 0 ? D_ModulePathfinding.pathfindingSearchLimitBase : maxSteps;

                //Находим путь
                List<D_PathfindingClosedNode> path = PathFindFast(
                    world,
                    moduleData,
                    cPF_P, ref cellPEs,
                    ref startCell, ref endCell);

                //Если путь не пуст
                if(path != null)
                {
                    //Для каждой ячейки в пути, кроме двух последних, в обратном порядке
                    for(int a = path.Count - 2; a > 0; a--)
                    {
                        //Заносим его в список индексов
                        results.Add(path[a].index);
                    }
                    //Заносим в список индексов индекс последней ячейки
                    results.Add(endCell.index);
                }
                //Иначе
                else
                {
                    //Возвращаем 0, обозначая, что путь пуст
                    return 0;
                }
            }

            //Возвращаем количество ячеек в пути
            return results.Count;
        }

        static void PathMatrixRefresh(
            D_ModulePathfinding moduleData,
            int cellsCount)
        {
            //Если матрица пути не требует обновления, то выходим из функции
            if(moduleData.pathMatrixUpdated == false)
            {
                return;
            }

            //Отмечаем, что матрица пути не требует обновления
            moduleData.pathMatrixUpdated = false;

            //Если массив для поиска пуст
            if(moduleData.pfCalc == null)
            {
                //Создаём массив
                moduleData.pfCalc = new D_PathfindingNodeFast[cellsCount];

                //Создаём очередь
                moduleData.open = new(
                    new PathfindingNodesComparer(moduleData.pfCalc),
                    cellsCount);
            }
            //Иначе
            else
            {
                //Очищаем очередь и массив
                moduleData.open.Clear();
                Array.Clear(moduleData.pfCalc, 0, moduleData.pfCalc.Length);

                //Обновляем сравнитель ячеек в очереди
                PathfindingNodesComparer comparer = (PathfindingNodesComparer)moduleData.open.Comparer;
                comparer.SetMatrix(moduleData.pfCalc);
            }
        }

        static List<D_PathfindingClosedNode> PathFindFast(
            EcsWorld world,
            D_ModulePathfinding moduleData,
            EcsPool<C_CellPathfinding> cPF_P, ref EcsPackedEntity[] cellPEs,
            ref C_CellPathfinding startCell, ref C_CellPathfinding endCell)
        {
            //Создаём переменную для проверки нахождения пути
            bool isPathFound = false;

            //Если фаза поиска больше 250
            if (moduleData.openCellValue > 250)
            {
                //Обнуляем фазу
                moduleData.openCellValue = 1;
                moduleData.closeCellValue = 2;
            }
            //Иначе
            else
            {
                //Обновляем фазу
                moduleData.openCellValue += 2;
                moduleData.closeCellValue += 2;
            }
            //Очищаем очередь и путь
            moduleData.open.Clear();
            moduleData.close.Clear();

            //Берём центр конечной ячейки
            Vector3 destinationCenter = endCell.center;

            //Обнуляем данные стартовой ячейки в массиве
            moduleData.pfCalc[startCell.index].distance = 0;
            moduleData.pfCalc[startCell.index].priority = 2;
            moduleData.pfCalc[startCell.index].prevIndex = startCell.index;
            moduleData.pfCalc[startCell.index].status = moduleData.openCellValue;
            moduleData.pfCalc[startCell.index].steps = 0;

            //Заносим стартовую ячейку в очередь
            moduleData.open.Push(startCell.index);

            //Пока в очереди есть ячейки
            while (moduleData.open.cellsCount > 0)
            {
                //Берём первую ячейку в очереди как текущую
                int currentCellIndex = moduleData.open.Pop();

                //Если данная ячейка уже вышла за границу поиска, то переходим к следующей
                if (moduleData.pfCalc[currentCellIndex].status == moduleData.closeCellValue)
                {
                    continue;
                }

                //Если индекс ячейки равен индексу конечной ячейки
                if (currentCellIndex == endCell.index)
                {
                    //Выводим ячейку за границу поиска
                    moduleData.pfCalc[currentCellIndex].status = moduleData.closeCellValue;

                    //Отмечаем, что путь найден, и выходим из цикла
                    isPathFound = true;

                    break;
                }

                //Если счётчик шагов больше предела
                if (moduleData.pfCalc[currentCellIndex].steps >= moduleData.pathfindingSearchLimit)
                {
                    continue;
                }

                //Берём текущую ячейку
                cellPEs[currentCellIndex].Unpack(world, out int currentCellEntity);
                ref C_CellPathfinding currentCell = ref cPF_P.Get(currentCellEntity);

                //Для каждого соседа текущей ячейки
                for (int a = 0; a < currentCell.neighbourCellPEs.Length; a++)
                {
                    //Берём соседа
                    currentCell.neighbourCellPEs[a].Unpack(world, out int neighbourCellEntity);
                    ref C_CellPathfinding neighbourCell = ref cPF_P.Get(neighbourCellEntity);

                    //Рассчитываем расстояние до соседа
                    float newDistance = moduleData.pfCalc[currentCellIndex].distance + neighbourCell.crossCost;

                    //Если ячейка находится в границе поиска или уже выведена за границу
                    if (moduleData.pfCalc[neighbourCell.index].status == moduleData.openCellValue
                        || moduleData.pfCalc[neighbourCell.index].status == moduleData.closeCellValue)
                    {
                        //Если расстояние до ячейки меньше или равно новому, то переходим к следующему соседу
                        if (moduleData.pfCalc[neighbourCell.index].distance <= newDistance)
                        {
                            continue;
                        }
                    }
                    //Иначе обновляем расстояние

                    //Обновляем индекс предыдущей ячейки и расстояние
                    moduleData.pfCalc[neighbourCell.index].prevIndex = currentCellIndex;
                    moduleData.pfCalc[neighbourCell.index].distance = newDistance;
                    moduleData.pfCalc[neighbourCell.index].steps = moduleData.pfCalc[currentCellIndex].steps + 1;

                    //Рассчитываем приоритет поиска
                    //Рассчитываем угол, используемый в качестве эвристики
                    float angle = Vector3.Angle(destinationCenter, neighbourCell.center);

                    //Обновляем приоритет ячейки и статус
                    moduleData.pfCalc[neighbourCell.index].priority = newDistance + 2f * angle;
                    moduleData.pfCalc[neighbourCell.index].status = moduleData.openCellValue;
                    
                    //Заносим ячейку в очередь
                    moduleData.open.Push(neighbourCell.index);
                }

                //Выводим текущую ячейку за границу поиска
                moduleData.pfCalc[currentCellIndex].status = moduleData.closeCellValue;
            }

            //Если путь найден
            if (isPathFound == true)
            {
                //Очищаем список пути
                moduleData.close.Clear();

                //Берём данные конечной ячейки во временную структуру-ссылку как текущую найденную ячейку
                ref D_PathfindingNodeFast currentFindedCell = ref moduleData.pfCalc[endCell.index];

                //Создаём структуру для итоговых данных текущей ячейки
                D_PathfindingClosedNode currentResultCell;

                //Переносим данные из найденной в итоговую
                currentResultCell.priority = currentFindedCell.priority;
                currentResultCell.distance = currentFindedCell.distance;
                currentResultCell.prevIndex = currentFindedCell.prevIndex;
                currentResultCell.index = endCell.index;

                //Пока индекс итоговой ячейки не равен индексу предыдущей,
                //то есть пока не достигнута стартовая ячейка
                while (currentResultCell.index != currentResultCell.prevIndex)
                {
                    //Заносим итоговую ячейку в список пути
                    moduleData.close.Add(currentResultCell);

                    //Берём данные предыдущей найденной ячейки
                    int prevCellIndex = currentResultCell.prevIndex;
                    currentFindedCell = ref moduleData.pfCalc[prevCellIndex];

                    //Переносим данные из найденной в итоговую
                    currentResultCell.priority = currentFindedCell.priority;
                    currentResultCell.distance = currentFindedCell.distance;
                    currentResultCell.prevIndex = currentFindedCell.prevIndex;
                    currentResultCell.index = prevCellIndex;
                }
                //Заносим последнюю итоговую ячейку в список пути
                moduleData.close.Add(currentResultCell);

                //Возвращаем список пути
                return moduleData.close;
            }

            return null;
        }
    }
}
