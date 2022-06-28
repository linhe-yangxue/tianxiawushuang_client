using System;
using System.Collections.Generic;


namespace Utilities.Tasks
{
    public interface ITask
    {
        int priority { get; }
        event Action onFinish;
        void Execute();
        void Terminate();
    }


    public abstract class ATask : ITask
    {
        private readonly int _priority = 0;

        public ATask()
            : this(0)
        { }

        public ATask(int priority)
        {
            this._priority = priority;
        }

        public int priority { get { return _priority; } }

        public event Action onFinish;

        public abstract void Execute();

        public virtual void Terminate() { }

        protected void Finish()
        {
            if (onFinish != null)
                onFinish();
        }
    }


    public class TaskQueue<T> where T : ITask
    {
        public readonly int limit = 100;

        private List<T> queue = new List<T>();
        private Predicate<T> filter = null;

        public bool stopped { get; private set; }

        public TaskQueue()
            : this(100)
        { }

        public TaskQueue(int limit)
            : this(limit, true)
        { }

        public TaskQueue(int limit, bool autoStart)
        {
            this.limit = limit;
            this.stopped = !autoStart;
        }

        public int count
        {
            get { return queue.Count; }
        }

        public T current
        {
            get { return queue.Count > 0 ? queue[0] : default(T); }
        }

        public bool Accept(T task)
        {
            if (filter != null && filter(task))
                return false;

            if (count >= limit && !Discard(task.priority))
                return false;

            task.onFinish += Dequeue;
            Enqueue(task);

            if (queue.Count == 1 && !stopped)
                task.Execute();

            return true;
        }

        public void Clear()
        {
            if (queue.Count > 0)
            {
                if (!stopped)
                    queue[0].Terminate();

                queue.Clear();
            }
        }

        public void Remove(Predicate<T> filter)
        {
            for (int i = (stopped ? 0 : 1); i < queue.Count; ++i)
            {
                if (filter != null && filter(queue[i]))
                {
                    queue.RemoveAt(i);
                    --i;
                }
            }
        }

        public void Refuse(Predicate<T> filter)
        {
            this.filter = filter;
        }

        public void Stop()
        {
            if (!stopped)
            {
                stopped = true;

                if (queue.Count > 0)
                    queue[0].Terminate();
            }
        }

        public void Start()
        {
            if (stopped)
            {
                stopped = false;

                if (queue.Count > 0)
                    queue[0].Execute();
            }
        }

        private void Enqueue(T task)
        {
            if (queue.Count == 0)
            {
                queue.Add(task);
                return;
            }

            for (int i = queue.Count - 1; i > 0; --i)
            {
                if (queue[i].priority >= task.priority)
                {
                    queue.Insert(i + 1, task);
                    return;
                }
            }
            queue.Insert(1, task);
        }

        private void Dequeue()
        {
            if (queue.Count > 0)
                queue.RemoveAt(0);

            if (queue.Count > 0 && !stopped)
                queue[0].Execute();
        }

        private bool Discard(int priority)
        {
            if (queue.Count > (stopped ? 0 : 1) && queue[queue.Count - 1].priority < priority)
            {
                queue.RemoveAt(queue.Count - 1);
                return true;
            }

            return false;
        }
    }
}