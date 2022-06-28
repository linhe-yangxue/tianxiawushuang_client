using UnityEngine;
using System.Collections.Generic;


public class ObjectMonitor : MonoBehaviour
{
    /// <summary>
    /// 建立Unity对象间的逻辑依赖关系，当被依赖的对象隐藏或销毁时，自动销毁依赖对象
    /// </summary>
    /// <param name="target"> 依赖的对象 </param>
    /// <param name="dependOn"> 被依赖的对象 </param>
    /// <param name="keepOnDisable"> 是否在被依赖对象隐藏时依然持有依赖对象 </param>
    public static void MakeDependency(UnityEngine.Object target, GameObject dependOn, bool keepOnDisable = false)
    {
        if (target == null)
        {
            return;
        }

        if (dependOn == null || (!keepOnDisable && !dependOn.activeInHierarchy))
        {
            Destroy(target);
            return;
        }

        ObjectMonitor monitor = dependOn.GetComponent<ObjectMonitor>();

        if (monitor == null)
        {
            monitor = dependOn.AddComponent<ObjectMonitor>();
        }

        monitor._MakeDependency(target, keepOnDisable);
    }

    /// <summary>
    /// 移除Unity对象间的逻辑依赖关系
    /// </summary>
    /// <param name="target"> 依赖的对象 </param>
    /// <param name="dependOn"> 被依赖的对象 </param>
    public static void RemoveDependency(UnityEngine.Object target, GameObject dependOn)
    {
        if (target == null || dependOn == null)
        {
            return;
        }

        ObjectMonitor monitor = dependOn.GetComponent<ObjectMonitor>();

        if (monitor != null)
        {
            monitor._RemoveDependency(target);
        }
    }

    private List<UnityEngine.Object> keepListOnExist = null;
    private List<UnityEngine.Object> keepListOnEnable = null;

    private void _MakeDependency(UnityEngine.Object target, bool keepOnDisable)
    {
        if (keepOnDisable)
        {
            if (keepListOnEnable != null)
            {
                RemoveFromKeepList(keepListOnEnable, target);
            }
            if (keepListOnExist == null)
            {
                keepListOnExist = new List<UnityEngine.Object>();
            }
            AddToKeepList(keepListOnExist, target);
        }
        else 
        {
            if (keepListOnExist != null)
            {
                RemoveFromKeepList(keepListOnExist, target);
            }
            if (keepListOnEnable == null)
            {
                keepListOnEnable = new List<UnityEngine.Object>();
            }
            AddToKeepList(keepListOnEnable, target);
        }
    }

    private void _RemoveDependency(UnityEngine.Object target)
    {
        if (keepListOnEnable != null)
        {
            RemoveFromKeepList(keepListOnEnable, target);
        }
        if (keepListOnExist != null)
        {
            RemoveFromKeepList(keepListOnExist, target);
        }
    }

    private void OnDisable()
    {
        if (keepListOnEnable != null)
        {
            DestroyKeepList(keepListOnEnable);
        }
    }

    private void OnDestroy()
    {
        if (keepListOnExist != null)
        {
            DestroyKeepList(keepListOnExist);
        }
    }

    private void RemoveFromKeepList(List<UnityEngine.Object> keepList, UnityEngine.Object target)
    {
        for (int i = keepList.Count - 1; i >= 0; --i)
        {
            if (keepList[i] == target)
            {
                keepList[i] = null;
                return;
            }
        }
    }

    private void AddToKeepList(List<UnityEngine.Object> keepList, UnityEngine.Object target)
    {
        int nullIndex = -1;

        for (int i = keepList.Count - 1; i >= 0; --i)
        {
            if (keepList[i] == target)
            {
                return;
            }

            if (keepList[i] == null)
            {
                nullIndex = i;
            }        
        }

        if (nullIndex >= 0)
        {
            keepList[nullIndex] = target;
        }
        else 
        {
            keepList.Add(target);
        }
    }

    private void DestroyKeepList(List<UnityEngine.Object> keepList)
    {
        for (int i = keepList.Count - 1; i >= 0; --i)
        {
            if (keepList[i] != null)
            {
                Destroy(keepList[i]);
            }
        }

        keepList.Clear();
    }
}