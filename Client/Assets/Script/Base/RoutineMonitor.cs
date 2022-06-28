using UnityEngine;
using System;
using System.Collections.Generic;
using Utilities.Routines;


namespace Utilities.Routines
{
    public class RoutineMonitor : MonoBehaviour
    {
        public static void MakeDependencyOn(IRoutine routine, IRoutine dependOn)
        {
            if (dependOn.Append(new DependencyMonitorRoutine(routine)) == null)
            {
                routine.Break();
            }
        }

        public static void MakeDependencyOn(IRoutine routine, GameObject dependOn)
        {
            dependOn.StartRoutine(new DependencyMonitorRoutine(routine));
        }

        public static void KeepExclusively<T>(T routine, GameObject target) where T : IRoutine
        {
            RoutineMonitor monitor = target.GetComponent<RoutineMonitor>();

            if (monitor == null)
            {
                monitor = target.AddComponent<RoutineMonitor>();
            }

            monitor.KeepExclusively<T>(routine);
        }


        private List<WeakReference> keepList = new List<WeakReference>();

        private void KeepExclusively<T>(T routine) where T : IRoutine
        {
            if (routine == null || routine.status == RoutineStatus.Broken || routine.status == RoutineStatus.Done)
            {
                return;
            }

            for (int i = keepList.Count - 1; i >= 0; --i)
            {
                if (keepList[i].Target == null)
                {
                    keepList.RemoveAt(i);
                }
                else if (keepList[i].Target is T)
                {
                    IRoutine r = (IRoutine)keepList[i].Target;

                    if (r.status != RoutineStatus.Unstart)
                    {
                        r.Break();
                        keepList.RemoveAt(i);
                    }
                }
            }

            keepList.Add(new WeakReference(routine));
        }

        private class DependencyMonitorRoutine : RoutineBlock
        {
            private WeakReference routineRef;

            public DependencyMonitorRoutine(IRoutine routine)
            {
                this.routineRef = new WeakReference(routine);
            }

            protected override void OnBreak()
            {
                if (routineRef.Target != null)
                {
                    ((IRoutine)routineRef.Target).Break();
                }
            }
        }
    }
}