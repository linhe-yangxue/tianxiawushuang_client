using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;


namespace Utilities.Routines
{
    public class RoutineComponent : MonoBehaviour
    {
        private List<IRoutine> main = new List<IRoutine>();
        private List<IRoutine> buffer = new List<IRoutine>();
        private List<IRoutine> raw = new List<IRoutine>();
        private bool hasQuit = false;

        public IEnumerable<IRoutine> rootRoutines
        {
            get
            {
                for (int i = 0; i < main.Count; ++i)
                {
                    if (main[i].status == RoutineStatus.Active)
                    {
                        yield return main[i];
                    }
                }

                for (int i = 0; i < buffer.Count; ++i)
                {
                    if (buffer[i].status == RoutineStatus.Active)
                    {
                        yield return buffer[i];
                    }
                }

                for (int i = 0; i < raw.Count; ++i)
                {
                    if (raw[i].status == RoutineStatus.Unstart || raw[i].status == RoutineStatus.Active)
                    {
                        yield return raw[i];
                    }
                }
            }
        }

        public void StartRoutine(IRoutine r)
        {
            if (!this.gameObject.activeInHierarchy || !this.enabled)
            {
                DEBUG.LogError("Can't start routine in an inactive gameObject in hierarchy or disabled RoutineComponent.");
                return;
            }

            if (r.status != RoutineStatus.Unstart)
            {
                DEBUG.LogError("Can't start routine because it has already started.");
                return;
            }

            if (r.MoveNext())
            {
                buffer.Add(r);
            }
        }

        public void RawAttach(IRoutine r)
        {
            if (!this.gameObject.activeInHierarchy || !this.enabled)
            {
                DEBUG.LogError("Can't start routine in an inactive gameObject in hierarchy or disabled RoutineComponent.");
                return;
            }

            if (r.status != RoutineStatus.Unstart)
            {
                DEBUG.LogError("Can't start routine because it has already started.");
                return;
            }

            raw.Add(r);
        }

        private void LateUpdate()
        {
            for (int i = 0; i < buffer.Count; ++i)
            {
                main.Add(buffer[i]);
            }

            buffer.Clear();

            for (int i = 0; i < main.Count; ++i)
            {
                main[i].MoveNext();
            }

            main.RemoveAll(r => r.status != RoutineStatus.Active);
            raw.RemoveAll(r => r.status == RoutineStatus.Broken || r.status == RoutineStatus.Done);
        }

        private void OnDisable()
        {
            if (!hasQuit)
            {
                using (var e = rootRoutines.GetEnumerator())
                {
                    while (e.MoveNext())
                    {
                        e.Current.Break();
                    }
                }

                main = new List<IRoutine>();
                buffer = new List<IRoutine>();
                raw = new List<IRoutine>();
            }         
        }

        private void OnApplicationQuit()
        {
            hasQuit = true;
        }
    }
}