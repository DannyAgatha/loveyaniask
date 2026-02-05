using System.Collections.Generic;

namespace Plugin.CoreImpl.Pathfinding
{
    public class PriorityQueueB<T> : IPriorityQueue<T>
    {
        private readonly IComparer<T> _comparer;
        private readonly List<T> _innerList = new();

        public PriorityQueueB() => _comparer = Comparer<T>.Default;

        public PriorityQueueB(IComparer<T> comparer) => _comparer = comparer;

        public T this[int index]
        {
            get => _innerList[index];
            set
            {
                _innerList[index] = value;
                Update(index);
            }
        }

        public void Clear()
        {
            _innerList.Clear();
        }

        public int Count => _innerList.Count;

        public int Enqueue(T item)
        {
            int p = _innerList.Count;
            _innerList.Add(item); // E[p] = O

            do
            {
                if (p == 0)
                {
                    break;
                }

                int p2 = (p - 1) / 2;

                if (OnCompare(p, p2) < 0)
                {
                    SwitchElements(p, p2);
                    p = p2;
                }
                else
                {
                    break;
                }
            } while (true);

            return p;
        }

        public T Dequeue()
        {
            T result = _innerList[0];
            int p = 0;

            _innerList[0] = _innerList[^1];
            _innerList.RemoveAt(_innerList.Count - 1);

            do
            {
                int pn = p;
                int p1 = 2 * p + 1;
                int p2 = 2 * p + 2;

                if (_innerList.Count > p1 && OnCompare(p, p1) > 0)
                {
                    p = p1;
                }

                if (_innerList.Count > p2 && OnCompare(p, p2) > 0)
                {
                    p = p2;
                }

                if (p == pn)
                {
                    break;
                }

                SwitchElements(p, pn);
            } while (true);

            return result;
        }

        public T Peek() => _innerList.Count > 0 ? _innerList[0] : default;

        private void Update(int i)
        {
            int p = i;
            int p2;

            do
            {
                if (p == 0)
                {
                    break;
                }

                p2 = (p - 1) / 2;

                if (OnCompare(p, p2) < 0)
                {
                    SwitchElements(p, p2);
                    p = p2;
                }
                else
                {
                    break;
                }
            } while (true);

            if (p < i)
            {
                return;
            }

            do
            {
                int pn = p;
                int p1 = 2 * p + 1;
                p2 = 2 * p + 2;

                if (_innerList.Count > p1 && OnCompare(p, p1) > 0)
                {
                    p = p1;
                }

                if (_innerList.Count > p2 && OnCompare(p, p2) > 0)
                {
                    p = p2;
                }

                if (p == pn)
                {
                    break;
                }

                SwitchElements(p, pn);
            } while (true);
        }

        private void SwitchElements(int i, int j)
        {
            T h = _innerList[i];
            _innerList[i] = _innerList[j];
            _innerList[j] = h;
        }

        private int OnCompare(int i, int j) => _comparer.Compare(_innerList[i], _innerList[j]);
    }
}