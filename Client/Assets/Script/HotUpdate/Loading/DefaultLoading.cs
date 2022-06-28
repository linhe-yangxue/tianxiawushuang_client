using UnityEngine;
using System.Collections;
using System;

/// <summary>
/// 默认下载实现
/// </summary>
public class DefaultLoading : ILoading
{
    protected LoadingProgressParam mProgress = new LoadingProgressParam();    //加载进度
    protected bool mIsLoading = false;            //是否在下载
    protected bool mHasLoadError = false;         //是否有下载错误
    protected string mLoadError = "";             //下载错误信息

    /// <summary>
    /// 协同操作
    /// </summary>
    public virtual ICoroutineOperation CoroutineOperation { set; get; }

    /// <summary>
    /// 开始加载
    /// </summary>
    /// <param name="path">加载路径</param>
    /// <param name="progressCallback">加载进度回调，参数：已加载大小，总大小，已加载进度</param>
    /// <param name="param">加载参数</param>
    /// <returns></returns>
    public IEnumerator StartLoad(string path, Action<LoadingProgressParam> progressCallback, object param)
    {
        if (mIsLoading)
            yield break;

        mIsLoading = true;
        mHasLoadError = false;
        mLoadError = "";

        //初始化加载
        mProgress.Init();
        if (progressCallback != null)
            progressCallback(mProgress);

        //开始加载
        if (CoroutineOperation != null)
            yield return CoroutineOperation.StartCoroutine(_OnStartLoading(path, progressCallback, param));

        mIsLoading = false;
        if (mHasLoadError)
            yield break;
        //加载完成
        mProgress.Progress = 1.0f;
        if (progressCallback != null)
            progressCallback(mProgress);
    }
    /// <summary>
    /// 停止加载
    /// </summary>
    public void StopLoad()
    {
        if (!mIsLoading)
            return;

        _OnStopLoading();

        mIsLoading = false;
    }
    /// <summary>
    /// 当前加载进度
    /// </summary>
    /// <returns></returns>
    public LoadingProgressParam LoadProgress()
    {
        return mProgress;
    }
    /// <summary>
    /// 是否在加载中
    /// </summary>
    /// <returns></returns>
    public bool IsLoading()
    {
        return mIsLoading;
    }
    /// <summary>
    /// 是否加载完成
    /// </summary>
    /// <returns></returns>
    public virtual bool IsComplete()
    {
        return (LoadProgress().Progress >= 1.0f);
    }
    /// <summary>
    /// 是否加载成功
    /// </summary>
    /// <returns></returns>
    public virtual bool IsSuccess()
    {
        return (IsComplete() && !mHasLoadError);
    }
    /// <summary>
    /// 错误信息
    /// </summary>
    /// <returns></returns>
    public virtual string ErrorInfo()
    {
        return mLoadError;
    }

    /// <summary>
    /// 开始加载时被调用
    /// </summary>
    /// <param name="path"></param>
    /// <param name="progressCallback"></param>
    /// <param name="param"></param>
    /// <returns></returns>
    protected virtual IEnumerator _OnStartLoading(string path, Action<LoadingProgressParam> progressCallback, object param)
    {
        yield break;
    }
    /// <summary>
    /// 停止加载时被调用
    /// </summary>
    protected virtual void _OnStopLoading()
    {
    }
}
