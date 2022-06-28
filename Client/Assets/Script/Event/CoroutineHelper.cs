using UnityEngine;
using System;
using System.Collections;
using Logic;


public class CoroutineHelper : MonoBehaviour 
{ }


namespace Utilities
{
    public static partial class Extensions
    {
        public static T GetOrAddComponent<T>(this GameObject owner) where T : Component
        {
            T component = owner.GetComponent<T>();

            if (component == null)
                component = owner.AddComponent<T>();

            return component;
        }

        public static T FindComponentUpwards<T>(this GameObject owner) where T : Component
        {
            Transform trans = owner.transform;
            T comp = null;

            do 
            {
                comp = trans.GetComponent<T>();
                trans = trans.parent;
            } while (trans != null && comp == null);

            return comp;
        }

        public static void RemoveComponent<T>(this GameObject owner) where T : Component
        {
            T component = owner.GetComponent<T>();

            if (component != null)
                Component.Destroy(component);
        }

        public static Coroutine StartCoroutine(this GameObject owner, IEnumerator enumerator)
        {
            CoroutineHelper helper = owner.GetOrAddComponent<CoroutineHelper>();
            return helper.StartCoroutine(enumerator);
        }

        public static void StopAllCoroutines(this GameObject owner)
        {
            CoroutineHelper helper = owner.GetOrAddComponent<CoroutineHelper>();
            helper.StopAllCoroutines();
        }

        public static void ExecuteDelayed(this GameObject owner, Action action, float delay)
        {
            owner.StartCoroutine(_ExecuteDelayed(action, delay));
        }

        public static void ExecuteQueued(this MonoBehaviour script, params IEnumerator[] tasks)
        {
            script.StartCoroutine(script.QueueCoroutine(tasks));
        }

        public static void ExecuteQueued(this GameObject owner, params IEnumerator[] tasks)
        {
            CoroutineHelper helper = owner.GetOrAddComponent<CoroutineHelper>();
            helper.StartCoroutine(helper.QueueCoroutine(tasks));
        }

        public static IEnumerator QueueCoroutine(this MonoBehaviour script, params IEnumerator[] tasks)
        {
            foreach (var task in tasks)
            {
                if (task != null)
                {
                    yield return script.StartCoroutine(task);
                }
            }
        }

        public static IEnumerator GroupCoroutine(this MonoBehaviour script, params IEnumerator[] tasks)
        {
            int count = 0;

            foreach (var task in tasks)
            {
                if (task != null)
                {
                    ++count;
                    script.StartCoroutine(_WaitForCoroutine(script, task, () => --count));
                }
            }

            while (count > 0)
            {
                yield return null;
            }
        }

        public static IEnumerator ActCoroutine(this MonoBehaviour script, Action action)
        {
            if (action != null)
            {
                action();
            }

            yield return null;
        }

        public static IEnumerator WaitCoroutine(this MonoBehaviour script, float time)
        {
            yield return new WaitForSeconds(time);
        }

        public static IEnumerator QueueCoroutine(this GameObject owner, params IEnumerator[] tasks)
        {
            return owner.GetOrAddComponent<CoroutineHelper>().QueueCoroutine(tasks);
        }

        public static IEnumerator GroupCoroutine(this GameObject owner, params IEnumerator[] tasks)
        {
            return owner.GetOrAddComponent<CoroutineHelper>().GroupCoroutine(tasks);
        }

        public static IEnumerator ActCoroutine(this GameObject owner, Action action)
        {
            return owner.GetOrAddComponent<CoroutineHelper>().ActCoroutine(action);
        }

        public static IEnumerator WaitCoroutine(this GameObject owner, float time)
        {
            return owner.GetOrAddComponent<CoroutineHelper>().WaitCoroutine(time);
        }

        private static IEnumerator _WaitForCoroutine(MonoBehaviour script, IEnumerator task, Action onFinish)
        {
            if (task != null)
            {
                yield return script.StartCoroutine(task);
            }

            if (onFinish != null)
            {
                onFinish();
            }
        }

        private static IEnumerator _ExecuteDelayed(Action action, float delay)
        {
            yield return new WaitForSeconds(delay);
            action();
        }
    }


    public static partial class WindowExtensions
    {
        public static Coroutine StartCoroutine(this tWindow owner, IEnumerator enumerator)
        {
            if (owner.mGameObjUI != null)
                return owner.mGameObjUI.StartCoroutine(enumerator);

            return null;
        }

        public static void StopAllCoroutines(this tWindow owner)
        {
            if (owner.mGameObjUI != null)
                owner.mGameObjUI.StopAllCoroutines();
        }

        public static void ExecuteDelayed(this tWindow owner, Action action, float delay)
        {
            if (owner.mGameObjUI != null)
                owner.mGameObjUI.ExecuteDelayed(action, delay);
        }

        public static void ExecuteQueued(this tWindow owner, params IEnumerator[] tasks)
        {
            if (owner.mGameObjUI != null)
                owner.mGameObjUI.StartCoroutine(owner.mGameObjUI.QueueCoroutine(tasks));
        }

        public static IEnumerator QueueCoroutine(this tWindow owner, params IEnumerator[] tasks)
        {
            if (owner.mGameObjUI != null)
                return owner.mGameObjUI.QueueCoroutine(tasks);
            else
                return null;
        }

        public static IEnumerator GroupCoroutine(this tWindow owner, params IEnumerator[] tasks)
        {
            if (owner.mGameObjUI != null)
                return owner.mGameObjUI.GroupCoroutine(tasks);
            else
                return null;
        }

        public static IEnumerator ActCoroutine(this tWindow owner, Action action)
        {
            if (owner.mGameObjUI != null)
                return owner.mGameObjUI.ActCoroutine(action);
            else
                return null;
        }

        public static IEnumerator WaitCoroutine(this tWindow owner, float time)
        {
            if (owner.mGameObjUI != null)
                return owner.mGameObjUI.WaitCoroutine(time);
            else
                return null;
        }
    }


    public static partial class CEventExtensions
    {
        public static bool CloseOwnerWindow(this CEvent evt)
        {
            string ownerWindow = evt.get("OWNER_WINDOW");

            if (!string.IsNullOrEmpty(ownerWindow))
                return DataCenter.CloseWindow(ownerWindow);

            return false;
        }
    }
}