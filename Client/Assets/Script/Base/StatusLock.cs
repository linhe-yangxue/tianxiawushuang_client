using UnityEngine;
using System;
using System.Collections.Generic;


public abstract class StatusLock
{
    private List<WeakReference> retainList = new List<WeakReference>();

    public void Retain(object owner)
    {
        if (owner == null)
            return;

        if (retainList.Find(x => x.Target == owner) == null)
        {
            retainList.Add(new WeakReference(owner));

            if (retainList.Count == 1)
                OnRetain();
        }
    }

    public void Release(object owner)
    {
        Update();

        if (owner == null || retainList.Count == 0)
            return;

        for (int i = retainList.Count - 1; i >= 0; --i)
        {
            if (retainList[i].Target == owner)
            {
                retainList.RemoveAt(i);
                break;
            }
        }

        if (retainList.Count == 0)
            OnRelease();
    }

    public void ReleaseAll()
    {
        if (retainList.Count > 0)
        {
            retainList.Clear();
            OnRelease();
        }
    }

    public void Update()
    {
        if (retainList.Count == 0)
            return;

        retainList.RemoveAll(x => x.Target == null);

        if (retainList.Count == 0)
            OnRelease();
    }

    protected abstract void OnRetain();
    protected abstract void OnRelease();
}


public class CheckClickFlagLock : StatusLock
{
    private bool checkClickFlag;

    protected override void OnRetain()
    {
        checkClickFlag = UICamera.NeedCheckClick;
        UICamera.NeedCheckClick = false;
    }

    protected override void OnRelease()
    {
        UICamera.NeedCheckClick = checkClickFlag;
    }
}