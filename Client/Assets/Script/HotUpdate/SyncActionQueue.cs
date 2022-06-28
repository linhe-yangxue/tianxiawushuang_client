using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

/// <summary>
/// 队列中元素
/// </summary>
public class SyncActionQueueElem
{
    public Action<Action<bool>> Operation;        //操作函数，参数：操作完成时回调（参数：是否继续执行）
}

/// <summary>
/// 同步队列，队列里操作依次执行
/// </summary>
public class SyncActionQueue
{
    private List<SyncActionQueueElem> m_ListQueue = new List<SyncActionQueueElem>();

    /// <summary>
    /// 增加一个操作到队列尾
    /// </summary>
    /// <param name="elem"></param>
    public void Push(SyncActionQueueElem elem)
    {
        if (m_ListQueue == null)
            return;
        m_ListQueue.Add(elem);
    }
    /// <summary>
    /// 清除所有操作
    /// </summary>
    public void Clear()
    {
        if (m_ListQueue == null || m_ListQueue.Count <= 0)
            return;
        m_ListQueue.Clear();
    }
    /// <summary>
    /// 执行下一个操作
    /// </summary>
    public void Next()
    {
        if (m_ListQueue == null || m_ListQueue.Count <= 0)
            return;

        SyncActionQueueElem tmpElem = m_ListQueue[0];
        m_ListQueue.RemoveAt(0);
        if (tmpElem.Operation == null)
            Next();
        else
            tmpElem.Operation(__ElemComplete);
    }
    /// <summary>
    /// 跳过挑一个操作
    /// </summary>
    public void Skip()
    {
        if (m_ListQueue == null || m_ListQueue.Count <= 0)
            return;

        m_ListQueue.RemoveAt(0);
    }
    /// <summary>
    /// 跳过所有操作
    /// </summary>
    /// <param name="isDoFinalAction">是否执行最后一个操作</param>
    public void SkipAll(bool isDoFinalAction)
    {
        if (m_ListQueue == null || m_ListQueue.Count <= 0)
            return;

        SyncActionQueueElem tmpElem = m_ListQueue[m_ListQueue.Count - 1];
        m_ListQueue.Clear();
        if (isDoFinalAction && tmpElem.Operation != null)
            tmpElem.Operation(__ElemComplete);
    }
    /// <summary>
    /// 操作执行结束后回调函数
    /// </summary>
    /// <param name="doNext"></param>
    private void __ElemComplete(bool doNext)
    {
        if (m_ListQueue == null)
            return;

        if (!doNext)
        {
            Clear();
            return;
        }
        Next();
    }
}
