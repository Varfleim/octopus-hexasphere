
using System.Collections.Generic;

namespace HS.Pathfinding
{
    public class PathfindingQueueInt
    {
        public PathfindingQueueInt(
            IComparer<int> comparer,
            int totalCellsCount)
        {
            this.comparer = comparer;

            cells = new int[totalCellsCount];
            cellsCount = 0;
        }

        int[] cells;
        public int cellsCount;

        public IComparer<int> Comparer
        {
            get
            {
                return comparer;
            }
        }
        IComparer<int> comparer;

        void Swap(
            int i, int j)
        {
            int h = cells[i];

            cells[i] = cells[j];

            cells[j] = h;
        }

        int Compare(
            int i, int j)
        {
            return comparer.Compare(cells[i], cells[j]);
        }

        public int Pop()
        {
            //Берём первую ячейку в очереди
            int result = cells[0];

            //Создаём временные переменные
            int p = 0, p1, p2, pn;

            //Переносим последнюю ячейку в очереди на место первой
            int count = cellsCount - 1;
            cells[0] = cells[count];
            cellsCount--;

            //Сортировка
            //Делаем
            do
            {
                //Берём в PN текущую ячейку
                pn = p;

                //Берём в P1 ячейку 2P + 1
                p1 = 2 * p + 1;

                //Берём в P2 ячейку 2P + 2
                p2 = p1 + 1;

                //Если счётчик больше P1 и P больше P1
                if (count > p1 && Compare(p, p1) > 0)
                    //То берём в P ячейку P1
                    p = p1;

                //Если счётчик больше P2 и P больше P2
                if (count > p2 && Compare(p, p2) > 0)
                    //То берём в P ячейку P2
                    p = p2;

                //Если P равна изначальной ячейке
                if (p == pn)
                    //Выходим из цикла
                    break;

                //Иначе меняем местами P и PN
                Swap(p, pn);
            }
            //Пока истинно 
            while (true);

            return result;
        }

        public int Push(int item)
        {
            //Создаём временные переменные
            int p = cellsCount, p2;

            //Заносим переданную ячейку на последнее место
            cells[cellsCount] = item;
            cellsCount++;

            //Сортировка
            //Делаем
            do
            {
                //Если количество ячеек в очереди было равно 0, то теперь оно равно 1 и сортировка не требуется
                if (p == 0)
                {
                    break;
                }

                //Берём ячейку (P - 1) / 2
                p2 = (p - 1) / 2;

                //Если приоритет ячейки P меньше приоритета P2
                if (Compare(p, p2) < 0)
                {
                    //Меняем их местами
                    Swap(p, p2);

                    //Берём ячейку P2
                    p = p2;
                }
                //Иначе выходим из цикла
                else
                {
                    break;
                }
            }
            //Пока истинно
            while (true);

            //Возвращаем последнюю ячейку
            return p;
        }

        public void Clear()
        {
            cellsCount = 0;
        }
    }
}
