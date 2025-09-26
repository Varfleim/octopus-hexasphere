
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
            //���� ������ ������ � �������
            int result = cells[0];

            //������ ��������� ����������
            int p = 0, p1, p2, pn;

            //��������� ��������� ������ � ������� �� ����� ������
            int count = cellsCount - 1;
            cells[0] = cells[count];
            cellsCount--;

            //����������
            //������
            do
            {
                //���� � PN ������� ������
                pn = p;

                //���� � P1 ������ 2P + 1
                p1 = 2 * p + 1;

                //���� � P2 ������ 2P + 2
                p2 = p1 + 1;

                //���� ������� ������ P1 � P ������ P1
                if (count > p1 && Compare(p, p1) > 0)
                    //�� ���� � P ������ P1
                    p = p1;

                //���� ������� ������ P2 � P ������ P2
                if (count > p2 && Compare(p, p2) > 0)
                    //�� ���� � P ������ P2
                    p = p2;

                //���� P ����� ����������� ������
                if (p == pn)
                    //������� �� �����
                    break;

                //����� ������ ������� P � PN
                Swap(p, pn);
            }
            //���� ������� 
            while (true);

            return result;
        }

        public int Push(int item)
        {
            //������ ��������� ����������
            int p = cellsCount, p2;

            //������� ���������� ������ �� ��������� �����
            cells[cellsCount] = item;
            cellsCount++;

            //����������
            //������
            do
            {
                //���� ���������� ����� � ������� ���� ����� 0, �� ������ ��� ����� 1 � ���������� �� ���������
                if (p == 0)
                {
                    break;
                }

                //���� ������ (P - 1) / 2
                p2 = (p - 1) / 2;

                //���� ��������� ������ P ������ ���������� P2
                if (Compare(p, p2) < 0)
                {
                    //������ �� �������
                    Swap(p, p2);

                    //���� ������ P2
                    p = p2;
                }
                //����� ������� �� �����
                else
                {
                    break;
                }
            }
            //���� �������
            while (true);

            //���������� ��������� ������
            return p;
        }

        public void Clear()
        {
            cellsCount = 0;
        }
    }
}
