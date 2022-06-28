using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


    class ConcurrentQueue<T>
    {
        private Object lk = new object();
        private Queue<T> _data = new Queue<T>();


        public void Enqueue(T item)
        {
            lock (lk)
            {
                _data.Enqueue(item);
            }
        }

        public bool TryDequeue(out T item)
        {
            lock (lk)
            {
                try
                {
                    item = _data.Dequeue();
                    return true;
                }
                catch(InvalidOperationException e)
                {
                    System.Console.WriteLine(e.Message);
                }
                item = default(T);
                return false;
            }
        }

    }

