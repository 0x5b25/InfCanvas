using System;
using System.Collections.Generic;
using System.Text;

namespace CanvasApp.Utilities
{
    class Queue<T> where T:class
    {
        LinkedList<T> _queue = new LinkedList<T>();

        public T Pop()
        {
            lock (_queue)
            {
                if (_queue.Count <= 0)
                    return null;
                T val = _queue.First.Value;
                _queue.RemoveFirst();
                return val;
            }
        }

        public void Push(T obj)
        {
            lock (_queue)
                _queue.AddLast(obj);
        }

        public void Reset()
        {
            lock (_queue)
                _queue.Clear();
        }

        public int GetDepth() { lock (_queue) return _queue.Count; }
    }
}
