using UnityEngine;
using System.Collections;
using System;

public class ResourceUpdate : ILoading
{
    private LoadingProgressParam mProgress = new LoadingProgressParam();

    /// <summary>
    /// 协同操作
    /// </summary>
    public ICoroutineOperation CoroutineOperation { set; get; }

    /// <summary>
    /// 开始加载
    /// </summary>
    /// <param name="path">加载路径</param>
    /// <param name="progressCallback">加载进度回调</param>
    /// <param name="param">加载参数</param>
    /// <returns></returns>
    public virtual IEnumerator StartLoad(string path, Action<LoadingProgressParam> progressCallback, object param)
    {
        yield break;

        //TODO
    }
    /// <summary>
    /// 停止加载
    /// </summary>
    public virtual void StopLoad()
    {
        //TODO
    }
    /// <summary>
    /// 当前加载进度
    /// </summary>
    /// <returns></returns>
    public virtual LoadingProgressParam LoadProgress()
    {
        //TODO
        return mProgress;
    }
    /// <summary>
    /// 是否在加载中
    /// </summary>
    /// <returns></returns>
    public virtual bool IsLoading()
    {
        //TODO
        return false;
    }
    /// <summary>
    /// 是否加载完成
    /// </summary>
    /// <returns></returns>
    public virtual bool IsComplete()
    {
        //TODO
        return false;
    }
    /// <summary>
    /// 是否加载成功
    /// </summary>
    /// <returns></returns>
    public virtual bool IsSuccess()
    {
        //TODO
        return true;
    }
    /// <summary>
    /// 错误信息
    /// </summary>
    /// <returns></returns>
    public string ErrorInfo()
    {
        //TODO
        return "";
    }
}
