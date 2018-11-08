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
            if (_queue.Count <= 0)
                return null;
            T val = _queue.First.Value;
            _queue.RemoveFirst();
            return val;
        }

        public void Push(T obj)
        {
            _queue.AddLast(obj);
        }

        public void Reset()
        {
            _queue.Clear();
        }

        public int GetDepth() { return _queue.Count; }
    }
}
