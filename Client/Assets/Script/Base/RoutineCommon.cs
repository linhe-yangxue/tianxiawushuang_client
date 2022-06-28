using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;


namespace Utilities.Routines
{
    public sealed class Delay : RoutineBlock
    {
        public float duration { get; private set; }
        public float destTime { get; private set; }

        public Delay(float duration)
        {
            this.duration = Mathf.Max(0f, duration);
        }

        protected override void OnStart()
        {
            destTime = Time.time + duration + 0.001f;
        }

        protected override bool OnBlock()
        {
            return Time.time < destTime;
        }

        public static void Start(float delay, Action action)
        {
            Routine.Start(_Start(delay, action));
        }

        public static void Start(GameObject target, float delay, Action action)
        {
            target.StartRoutine(_Start(delay, action));
        }

        private static IEnumerator _Start(float delay, Action action)
        {
            yield return new Delay(delay);
            action();
        }
    }


    public sealed class DelayFrames : RoutineBlock
    {
        public float count { get; private set; }
        public float destFrameCount { get; private set; }

        public DelayFrames(int count)
        {
            this.count = count;
        }

        protected override void OnStart()
        {
            destFrameCount = Time.frameCount + count;
        }

        protected override bool OnBlock()
        {
            return Time.frameCount < destFrameCount;
        }

        public static void Start(int framesDelay, Action action)
        {
            Routine.Start(_Start(framesDelay, action));
        }

        public static void Start(GameObject target, int framesDelay, Action action)
        {
            target.StartRoutine(_Start(framesDelay, action));
        }

        private static IEnumerator _Start(int framesDelay, Action action)
        {
            yield return new DelayFrames(framesDelay);
            action();
        }
    }


    public sealed class RealTimeDelay : RoutineBlock
    {
        public float duration { get; private set; }
        public float destTime { get; private set; }

        public RealTimeDelay(float duration)
        {
            this.duration = Mathf.Max(0f, duration);
        }

        protected override void OnStart()
        {
            destTime = Time.realtimeSinceStartup + duration + 0.001f;
        }

        protected override bool OnBlock()
        {
            return Time.realtimeSinceStartup < destTime;
        }

        public static void Start(float delay, Action action)
        {
            Routine.Start(_Start(delay, action));
        }

        public static void Start(GameObject target, float delay, Action action)
        {
            target.StartRoutine(_Start(delay, action));
        }

        private static IEnumerator _Start(float delay, Action action)
        {
            yield return new RealTimeDelay(delay);
            action();
        }
    }


    public sealed class WaitUntil : RoutineBlock
    {
        private Func<bool> condition;

        public WaitUntil(Func<bool> condition)
        {
            this.condition = condition;
        }

        protected override bool OnBlock()
        {
            return condition != null && !condition();
        }

        public static void Start(Func<bool> condition, Action action)
        {
            Routine.Start(_Start(condition, action));
        }

        public static void Start(GameObject target, Func<bool> condition, Action action)
        {
            target.StartRoutine(_Start(condition, action));
        }

        private static IEnumerator _Start(Func<bool> condition, Action action)
        {
            yield return new WaitUntil(condition);
            action();
        }
    }


    //public sealed class Protecter : Routine
    //{
    //    private int frame;

    //    public Protecter()
    //    {
    //        this.frame = Time.frameCount;
    //        Bind(DoProtecter());
    //    }

    //    private IEnumerator DoProtecter()
    //    {
    //        if (Time.frameCount == frame)
    //        {
    //            yield return null;
    //        }
    //    }
    //}

    public sealed class Act : RoutineBlock
    {
        public Action action { get; private set; }

        public Act(Action action)
        {
            this.action = action;
        }

        protected override void OnStart()
        {
            if (action != null)
            {
                action();
            }
        }

        protected override bool OnBlock()
        {
            return false;
        }
    }


    //public sealed class Act : Routine
    //{
    //    public Act(Action action)
    //    {
    //        Bind(DoAct(action));
    //    }

    //    private IEnumerator DoAct(Action action)
    //    {
    //        if (action != null)
    //        {
    //            action();
    //        }

    //        yield break;
    //    }
    //}


    public sealed class Series : Routine
    {
        private List<IRoutine> routines;

        public Series(params IEnumerator[] rs)
        {
            int length = rs.Length;
            this.routines = new List<IRoutine>(length > 2 ? length : 2);

            for (int i = 0; i < length; ++i)
            {
                this.routines.Add(Routine.Wrap(rs[i]));
            }

            Bind(DoSeries());
        }

        private IEnumerator DoSeries()
        {
            for (int i = 0; i < routines.Count; ++i)
            {
                yield return routines[i];
            }
        }

        public void Extends(IEnumerator r)
        {
            if (r is Series)
                routines.AddRange(((Series)r).routines);
            else
                routines.Add(Routine.Wrap(r));
        }
    }


    public sealed class All : RoutineBlock
    {
        private List<IRoutine> routines;

        public All(params IEnumerator[] rs)
        {
            int length = rs.Length;
            this.routines = new List<IRoutine>(length > 2 ? length : 2);

            for (int i = 0; i < length; ++i)
            {
                this.routines.Add(Routine.Wrap(rs[i]));
            }
        }

        protected override void OnStart()
        {
            for (int i = 0; i < routines.Count; ++i)
            {
                RawAttach(routines[i]);
            }
        }

        protected override bool OnBlock()
        {
            bool finished = true;

            for (int i = 0; i < routines.Count; ++i)
            {
                finished &= !routines[i].MoveNext();
            }

            if (finished)
                BreakAll();

            return !finished;
        }

        protected override void OnBreak()
        {
            BreakAll();
        }

        private void BreakAll()
        {
            for (int i = 0; i < routines.Count; ++i)
            {
                routines[i].Break();
            }
        }

        public void Extends(IEnumerator r)
        {
            if (r is All)
                routines.AddRange(((All)r).routines);
            else
                routines.Add(Routine.Wrap(r));
        }
    }


    public sealed class Any : RoutineBlock
    {
        private List<IRoutine> routines;

        public Any(params IEnumerator[] rs)
        {
            int length = rs.Length;
            this.routines = new List<IRoutine>(length > 2 ? length : 2);

            for (int i = 0; i < length; ++i)
            {
                this.routines.Add(Routine.Wrap(rs[i]));
            }
        }

        protected override void OnStart()
        {
            for (int i = 0; i < routines.Count; ++i)
            {
                RawAttach(routines[i]);
            }
        }

        protected override bool OnBlock()
        {
            bool finished = routines.Count == 0;

            for (int i = 0; i < routines.Count; ++i)
            {
                finished |= !routines[i].MoveNext();
            }

            if (finished)
                BreakAll();

            return !finished;
        }

        protected override void OnBreak()
        {
            BreakAll();
        }

        private void BreakAll()
        {
            for (int i = 0; i < routines.Count; ++i)
            {
                routines[i].Break();
            }
        }

        public void Extends(IEnumerator r)
        {
            if (r is Any)
                routines.AddRange(((Any)r).routines);
            else
                routines.Add(Routine.Wrap(r));
        }
    }


    public sealed class Conditional : RoutineBlock
    {
        private IRoutine main;
        private Func<bool> predicate;

        public Conditional(IEnumerator main, Func<bool> predicate)
        {
            this.main = Routine.Wrap(main);
            this.predicate = predicate;
        }

        protected override void OnStart()
        {
            RawAttach(main);
        }

        protected override bool OnBlock()
        {
            if (predicate == null || predicate())
            {
                return main.MoveNext();
            }
            else 
            {
                main.Break();
                return false;
            }
        }

        protected override void OnBreak()
        {
            main.Break();
        }
    }


    public sealed class WaitUntilRoutineFinish : RoutineBlock
    {
        public IRoutine target { get; private set; }

        public WaitUntilRoutineFinish(IRoutine r)
        {
            target = r;
        }

        protected override bool OnBlock()
        {
            return target.status != RoutineStatus.Done && target.status != RoutineStatus.Broken;
        }
    }
    
    //public sealed class WaitUntilRoutineFinish : Routine
    //{
    //    public WaitUntilRoutineFinish(IRoutine r)
    //    {
    //        Bind(DoWaitUntilRoutineFinish(r));
    //    }

    //    private IEnumerator DoWaitUntilRoutineFinish(IRoutine r)
    //    {
    //        yield return new WaitUntil(() => r.status != RoutineStatus.Active);
    //    }
    //}


    public sealed class ErrorCompensationTimer
    {
        private float error;

        public void ResetError()
        {
            error = 0f;
        }

        public IEnumerator DoDelay(float time)
        {
            if (time <= error)
            {
                error -= time;
            }
            else
            {
                float startTime = Time.time;
                float realTime = time - error;
                error = 0f;
                yield return new Delay(realTime);
                error += Time.time - startTime - realTime;
            }
        }
    }
}