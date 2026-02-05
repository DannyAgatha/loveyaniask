namespace Plugin.CoreImpl.Pathfinding
{
    public interface IPriorityQueue<T>
    {
        int Count { get; }
        int Enqueue(T item);
        T Dequeue();

        void Clear();
    }
}