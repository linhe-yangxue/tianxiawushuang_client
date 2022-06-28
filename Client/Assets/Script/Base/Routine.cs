using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;


namespace Utilities.Routines
{
    public interface IRoutine : IEnumerator/*, IDisposable*/
    {
        //bool hasStarted { get; }
        //bool isBroken { get; }
        //bool isDone { get; }
        RoutineStatus status { get; }
        void Break();
        IRoutine Append(IEnumerator appendRoutine);
        IEnumerable<IRoutine> subRoutines { get; }
    }


    public enum RoutineStatus
    {
        Unstart = 0,
        Active = 1,
        Broken = 2,
        Done = 3
    }


    public class Routine : IRoutine
    {
        private static GameObject globalTarget = null;

        public static IRoutine Start(IEnumerator routine)
        {
            if (globalTarget == null)
            {
                globalTarget = new GameObject("Global Routines");
                GameObject.DontDestroyOnLoad(globalTarget);
            }

            return globalTarget.StartRoutine(routine);
        }

        public static IRoutine Wrap(IEnumerator routine)
        {
            IRoutine r = routine as IRoutine;
            return r == null ? new Routine(routine) : r;
        }

        public static void BreakAll()
        {
            if (globalTarget != null)
            {
                globalTarget.BreakAllRoutines();
            }
        }

        public static bool IsActive(IRoutine r)
        {
            return r != null && r.status == RoutineStatus.Active/*r.hasStarted && !r.isBroken && !r.isDone*/;
        }

        private IEnumerator main;
        private IRoutine sub;
        //private bool isDisposed = false;
        private GameObject aidObject = null;
        private List<IRoutine> prepareAidRoutines = null;

        //public bool hasStarted { get; private set; }
        //public bool isBroken { get; private set; }
        //public bool isDone { get; private set; }
        public RoutineStatus status { get; private set; }

        public Routine(IEnumerator main)
        {
            this.main = main;
            this.sub = null;
            this.status = RoutineStatus.Unstart;
            //this.hasStarted = false;
            //this.isBroken = false;
            //this.isDone = false;
        }

        public Routine()
            : this(null)
        { }

        public void Bind(IEnumerator main)
        {
            if (status == RoutineStatus.Unstart)
            {
                this.main = main;
            }
            else
            {
                throw new InvalidOperationException("Can't bind routine if already start.");
            }
        }

        public void Reset()
        {
            throw new NotSupportedException();
        }

        public object Current
        {
            get { return sub == null ? (main == null ? null : main.Current) : sub.Current; }
        }

        public bool MoveNext()
        {
            if (main == null || status == RoutineStatus.Broken || status == RoutineStatus.Done)
            {
                return false;
            }

            try
            {
                if (status == RoutineStatus.Unstart)
                {
                    status = RoutineStatus.Active;

                    if (prepareAidRoutines != null)
                    {
                        if (aidObject == null)
                        {
                            aidObject = AidRoutineObjectPool.New();
                        }

                        for (int i = 0; i < prepareAidRoutines.Count; ++i)
                        {
                            aidObject.StartRoutine(prepareAidRoutines[i]);
                        }

                        prepareAidRoutines.Clear();
                    }
                }

                while (true)
                {
                    if (sub == null)
                    {
                        if (!main.MoveNext())
                        {
                            if (aidObject != null)
                            {
                                AidRoutineObjectPool.Discard(aidObject);
                                aidObject = null;
                            }

                            status = RoutineStatus.Done;
                            return false;
                        }

                        if (main.Current == null || !(main.Current is IEnumerator))
                        {
                            return true;
                        }

                        sub = Routine.Wrap((IEnumerator)main.Current);

                        if (sub.status != RoutineStatus.Unstart)
                        {
                            DEBUG.LogWarning("Yielded subroutine has already started.");
                        }
                    }

                    if (sub.MoveNext())
                    {
                        return true;
                    }
                    else
                    {
                        sub = null;
                    }
                }
            }
            catch (Exception e)
            {
                Break();
                OnException(e);
                DEBUG.LogError(e.Message + "\n" + e.StackTrace);
                return false;
            }
        }

        public void Break()
        {
            if (status == RoutineStatus.Active)
            {
                status = RoutineStatus.Broken;

                if (aidObject != null)
                {
                    AidRoutineObjectPool.Discard(aidObject);
                    aidObject = null;
                }

                if (sub != null)
                {
                    sub.Break();
                }

                try
                {
                    OnBreak();
                }
                catch (Exception e)
                {
                    DEBUG.LogError(e.Message + "\n" + e.StackTrace);
                }
            }
        }

        //public void Dispose()
        //{
        //    if (!isDisposed)
        //    {
        //        Break();
        //        isDisposed = true;
        //        GC.SuppressFinalize(this);
        //    }
        //}

        public IRoutine Append(IEnumerator appendRoutine)
        {
            if (status == RoutineStatus.Broken || status == RoutineStatus.Done)
            {
                return null;
            }

            if (status == RoutineStatus.Active)
            {
                if (aidObject == null)
                {
                    aidObject = AidRoutineObjectPool.New();
                }

                return aidObject.StartRoutine(appendRoutine);
            }
            else
            {
                if (prepareAidRoutines == null)
                {
                    prepareAidRoutines = new List<IRoutine>();
                }

                IRoutine r = Routine.Wrap(appendRoutine);
                prepareAidRoutines.Add(r);
                return r;
            }
        }

        protected virtual void OnBreak() { }
        protected virtual void OnException(Exception e) { }

        protected void RawAttach(IRoutine r)
        {
            if (status == RoutineStatus.Active)
            {
                if (aidObject == null)
                {
                    aidObject = AidRoutineObjectPool.New();
                }

                RoutineComponent c = aidObject.GetComponent<RoutineComponent>();

                if (c == null)
                {
                    c = aidObject.AddComponent<RoutineComponent>();
                }

                c.RawAttach(r);
            }
        }

        public IEnumerable<IRoutine> subRoutines
        {
            get
            {
                if (Routine.IsActive(sub))
                {
                    yield return sub;
                }

                if (aidObject != null)
                {
                    RoutineComponent c = aidObject.GetComponent<RoutineComponent>();

                    if (c != null)
                    {
                        using (var e = c.rootRoutines.GetEnumerator())
                        {
                            while (e.MoveNext())
                            {
                                yield return e.Current;
                            }
                        }
                    }
                }
            }
        }
    }


    public class RoutineBlock : IRoutine
    {
        //private bool isDisposed = false;
        private GameObject aidObject = null;
        private List<IRoutine> prepareAidRoutines = null;

        public RoutineBlock()
        {
            //hasStarted = false;
            //isBroken = false;
            //isDone = false;
            status = RoutineStatus.Unstart;
        }

        public void Reset()
        {
            throw new NotSupportedException();
        }

        //public bool hasStarted { get; private set; }
        //public bool isBroken { get; private set; }
        //public bool isDone { get; private set; }
        public RoutineStatus status { get; private set; }
        public object Current { get { return null; } }

        public bool MoveNext()
        {
            if (status == RoutineStatus.Broken || status == RoutineStatus.Done)
            {
                return false;
            }

            try
            {
                if (status == RoutineStatus.Unstart)
                {
                    status = RoutineStatus.Active;

                    if (prepareAidRoutines != null)
                    {
                        if (aidObject == null)
                        {
                            aidObject = AidRoutineObjectPool.New();
                        }

                        for (int i = 0; i < prepareAidRoutines.Count; ++i)
                        {
                            aidObject.StartRoutine(prepareAidRoutines[i]);
                        }

                        prepareAidRoutines.Clear();
                    }

                    OnStart();
                }

                if (status == RoutineStatus.Broken)
                {
                    return false;
                }
                else if (OnBlock())
                {
                    return true;
                }
                else
                {
                    if (aidObject != null)
                    {
                        AidRoutineObjectPool.Discard(aidObject);
                        aidObject = null;
                    }

                    status = RoutineStatus.Done;
                    return false;
                }
            }
            catch (Exception e)
            {
                Break();
                OnException(e);
                DEBUG.LogError(e.Message + "\n" + e.StackTrace);
                return false;
            }
        }

        public void Break()
        {
            if (status == RoutineStatus.Active)
            {
                status = RoutineStatus.Broken;

                if (aidObject != null)
                {
                    AidRoutineObjectPool.Discard(aidObject);
                    aidObject = null;
                }

                try
                {
                    OnBreak();
                }
                catch (Exception e)
                {
                    DEBUG.LogError(e.Message + "\n" + e.StackTrace);
                }
            }
        }

        //public void Dispose()
        //{
        //    if (!isDisposed)
        //    {
        //        Break();
        //        isDisposed = true;
        //        GC.SuppressFinalize(this);
        //    }
        //}

        public IRoutine Append(IEnumerator appendRoutine)
        {
            if (status == RoutineStatus.Broken || status == RoutineStatus.Done)
            {
                return null;
            }

            if (status == RoutineStatus.Active)
            {
                if (aidObject == null)
                {
                    aidObject = AidRoutineObjectPool.New();
                }

                return aidObject.StartRoutine(appendRoutine);
            }
            else 
            {
                if (prepareAidRoutines == null)
                {
                    prepareAidRoutines = new List<IRoutine>();
                }

                IRoutine r = Routine.Wrap(appendRoutine);
                prepareAidRoutines.Add(r);
                return r;
            }
        }

        protected virtual bool OnBlock() { return true; }
        protected virtual void OnStart() { }
        protected virtual void OnBreak() { }
        protected virtual void OnException(Exception e) { }

        protected void RawAttach(IRoutine r)
        {
            if (status == RoutineStatus.Active)
            {
                if (aidObject == null)
                {
                    aidObject = AidRoutineObjectPool.New();
                }

                RoutineComponent c = aidObject.GetComponent<RoutineComponent>();

                if (c == null)
                {
                    c = aidObject.AddComponent<RoutineComponent>();
                }

                c.RawAttach(r);
            }
        }

        public IEnumerable<IRoutine> subRoutines
        {
            get
            {
                if (aidObject != null)
                {
                    RoutineComponent c = aidObject.GetComponent<RoutineComponent>();

                    if (c != null)
                    {
                        using (var e = c.rootRoutines.GetEnumerator())
                        {
                            while (e.MoveNext())
                            {
                                yield return e.Current;
                            }
                        }
                    }
                }
            }
        }
    }


    public interface IResponsable<T>
    {
        // ret < 0 : refuse
        // ret = 0 : depute to sub routines
        // ret > 0 : accept
        int Response(T requester);
    }


    internal class AidRoutineObjectPool
    {        
        private static List<GameObject> buffer = new List<GameObject>();      
        private static GameObject aidRoutineRootObject = null;
        private static int _maxBufferCount = 100;

        public static int maxBufferCount
        {
            get 
            {
                return _maxBufferCount;
            }
            set 
            {
                _maxBufferCount = value;

                if (buffer.Count > value)
                {
                    int startIndex = Mathf.Max(0, value);
                    buffer.RemoveRange(startIndex, buffer.Count - startIndex);
                }
            }
        }

        public static GameObject New()
        {
            for (int i = buffer.Count - 1; i >= 0; --i)
            {
                var obj = buffer[i];
                buffer.RemoveAt(i);

                if (obj != null)
                {
                    obj.SetActive(true);
                    return obj;
                }
            }

            if (aidRoutineRootObject == null)
            {
                aidRoutineRootObject = new GameObject("Aid Routines Root");
                GameObject.DontDestroyOnLoad(aidRoutineRootObject);
            }

            aidRoutineRootObject.SetActive(true);
            var newObj = new GameObject("Aid Routines");           
            newObj.transform.parent = aidRoutineRootObject.transform;
            newObj.AddComponent<RoutineComponent>();
            return newObj;
        }

        public static void Discard(GameObject obj)
        {
            if (obj != null)
            {
                if (buffer.Count < _maxBufferCount)
                {
                    obj.SetActive(false);
                    buffer.Add(obj);
                }
                else 
                {
                    GameObject.Destroy(obj);
                }
            }
        }
    }


    public static class RoutineExtensions
    {
        public static IRoutine AppendWith(this IRoutine mainRoutine, IEnumerator appendRoutine)
        {
            mainRoutine.Append(appendRoutine);
            return mainRoutine;
        }

        public static IRoutine StartRoutine(this GameObject target, IEnumerator routine)
        {
            RoutineComponent c = target.GetComponent<RoutineComponent>();

            if (c == null)
            {
                c = target.AddComponent<RoutineComponent>();
            }

            IRoutine r = Routine.Wrap(routine);
            c.StartRoutine(r);
            return r;
        }

        public static void BreakAllRoutines(this GameObject target)
        {
            RoutineComponent c = target.GetComponent<RoutineComponent>();

            if (c != null)
            {
                using (var e = c.rootRoutines.GetEnumerator())
                {
                    while (e.MoveNext())
                    {
                        e.Current.Break();
                    }
                }
            }
        }

        public static IRoutine Request<T>(this IRoutine target, T requester)
        {
            if (!Routine.IsActive(target))
            {
                return null;
            }

            var resper = target as IResponsable<T>;

            if (resper != null)
            {
                var ret = resper.Response(requester);

                if (ret > 0)
                {
                    return null;
                }
                else if (ret < 0)
                {
                    return target;
                }
            }

            using (var e = target.subRoutines.GetEnumerator())
            {
                while (e.MoveNext())
                {
                    var ret = Request<T>(e.Current, requester);

                    if (ret != null)
                    {
                        return ret;
                    }
                }
            }

            return null;
        }
    }
}