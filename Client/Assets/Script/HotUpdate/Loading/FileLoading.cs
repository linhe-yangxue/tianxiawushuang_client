using UnityEngine;
using System.Collections;
using System;

public enum FILE_LOADING_WAY
{
    SYNC,       //同步加载
    ASYNC       //异步加载
}

/// <summary>
/// 文件加载参数
/// </summary>
public class FileLoadingParam
{
    private FILE_LOADING_WAY mLoadingWay = FILE_LOADING_WAY.ASYNC;
    private float mOverTime = 0.0f;

    /// <summary>
    /// 加载方式
    /// </summary>
    public FILE_LOADING_WAY LoadingWay
    {
        set { mLoadingWay = value; }
        get { return mLoadingWay; }
    }
    /// <summary>
    /// 超时时间
    /// </summary>
    public float OverTime
    {
        set { mOverTime = value; }
        get { return mOverTime; }
    }
}

/// <summary>
/// 文件加载
/// </summary>
public class FileLoading : DefaultLoading
{
    private WWW mWWW;

    private bool mIsLoadComplete;   //是否加载完成
    private bool mIsOverTime;       //是否超时

    private IEnumerator mIEnumeratorSendOverTime;
    private IEnumerator mIEnumeratorLoadAsync;

    class FileLoadingLoadParam
    {
        public string Path { set; get; }
        public Action<LoadingProgressParam> ProgressCallback { set; get; }
    }

    protected override IEnumerator _OnStartLoading(string path, Action<LoadingProgressParam> progressCallback, object param)
    {
        if (!(param is FileLoadingParam))
            yield break;
        if (CoroutineOperation == null)
        {
            mHasLoadError = true;
            mLoadError = "FileLoading CoroutineOperation is null.";
            HotUpdateLog.Log("FileLoading CoroutineOperation is null.");
            yield break;
        }

        FileLoadingParam tmpFileLoadingParam = param as FileLoadingParam;
        FILE_LOADING_WAY tmpLoadingWay = tmpFileLoadingParam.LoadingWay;
        mIsLoadComplete = false;
        mIsOverTime = false;
        mIEnumeratorSendOverTime = null;
        mIEnumeratorLoadAsync = null;
        FileLoadingLoadParam tmpLoadParam = new FileLoadingLoadParam()
        {
            Path = path,
            ProgressCallback = progressCallback
        };
//        CoroutineOperation.StartCoroutine("__OnSendOverTime", tmpFileLoadingParam.OverTime);
        mIEnumeratorSendOverTime = __OnSendOverTime(tmpFileLoadingParam.OverTime);
        CoroutineOperation.StartCoroutine(mIEnumeratorSendOverTime);
        switch (tmpLoadingWay)
        {
            case FILE_LOADING_WAY.SYNC:
                {
                    if (path.IndexOf("139.") != -1  ||
                        path.IndexOf("10.0.") != -1 ||
                        path.IndexOf("127.0.0.1") != -1 ||
                        path.IndexOf("http://res.") != -1)
                        mWWW = new WWW(path);
                    else
                    {
                        WWWForm tmpWWWForm = new WWWForm();
                        var tmpHeaders = tmpWWWForm.headers;
                        tmpHeaders["HOST"] = "res.wsxsm.gdegame.com";
                        mWWW = new WWW(path, null, tmpHeaders);
                    }
                    while (mIsLoading && mWWW != null && !mWWW.isDone)
                    {
                        if (progressCallback != null)
                        {
                            mProgress.BytesLoaded = mWWW.bytesDownloaded;
                            mProgress.BytesTotal = mWWW.size;
                            mProgress.Progress = mWWW.progress;
                            progressCallback(mProgress);
                        }
                        if (mIsOverTime)
                        {
                            if (mWWW != null)
                                mWWW.Dispose();
                            break;
                        }
                    }
//                    CoroutineOperation.StopCoroutine("__OnSendOverTime");
                    if (mIEnumeratorSendOverTime != null)
                        CoroutineOperation.StopCoroutine(mIEnumeratorSendOverTime);
                    if (mWWW != null)
                        mLoadError = mWWW.error;
                    mIsLoadComplete = true;
                } break;
            case FILE_LOADING_WAY.ASYNC:
                {
//                    CoroutineOperation.StartCoroutine("__OnLoadAsync", tmpLoadParam);
                    mIEnumeratorLoadAsync = __OnLoadAsync(tmpLoadParam);
                    CoroutineOperation.StartCoroutine(mIEnumeratorLoadAsync);
                } break;
        }
        while (!mIsLoadComplete)
            yield return null;
        mHasLoadError = (mLoadError != "" && mLoadError != null);
        mIEnumeratorSendOverTime = null;
        mIEnumeratorLoadAsync = null;
    }
    protected override void  _OnStopLoading()
    {
        if (mWWW != null)
        {
            mWWW.Dispose();
            mWWW = null;
        }
        if(mProgress.Progress < 1.0f)
        {
            mLoadError = "force stop";
            mHasLoadError = true;
        }
        mIsLoading = false;
    }

    /// <summary>
    /// 异步加载文件
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    private IEnumerator __OnLoadAsync(FileLoadingLoadParam param)
    {
        if (param == null)
            yield break;

        //异步加载
//         if (param.Path.IndexOf("139.") != -1    ||
//             param.Path.IndexOf("10.0.") != -1 ||
//             param.Path.IndexOf("127.0.0.1") != -1 ||
//             param.Path.IndexOf("http://res.") != -1)
//             mWWW = new WWW(param.Path);
//         else
//         {
//             WWWForm tmpWWWForm = new WWWForm();
//             var tmpHeaders = tmpWWWForm.headers;
//             tmpHeaders["HOST"] = "res.wsxsm.gdegame.com";
//             mWWW = new WWW(param.Path, null, tmpHeaders);
//         }
        string tmpPath = Uri.EscapeUriString(param.Path);
        mWWW = new WWW(tmpPath);
        if (mIsOverTime)
            mWWW.Dispose();
        else
        {
            while (mIsLoading && mWWW != null && !mWWW.isDone)
            {
                if (param.ProgressCallback != null)
                {
                    mProgress.BytesLoaded = mWWW.bytesDownloaded;
                    mProgress.BytesTotal = mWWW.size;
                    mProgress.Progress = mWWW.progress;
                    param.ProgressCallback(mProgress);
                }
                yield return null;
                if (mIsOverTime)
                    break;
            }
//            CoroutineOperation.StopCoroutine("__OnSendOverTime");
            if (CoroutineOperation != null && mIEnumeratorSendOverTime != null)
                CoroutineOperation.StopCoroutine(mIEnumeratorSendOverTime);
            if (mWWW != null)
                mLoadError = mWWW.error;
        }
        mIsLoadComplete = true;
    }
    private IEnumerator __OnSendOverTime(float overTime)
    {
        yield return new WaitForSeconds(overTime);
//        CoroutineOperation.StopCoroutine("__OnLoadAsync");
        if (CoroutineOperation != null && mIEnumeratorLoadAsync != null)
            CoroutineOperation.StopCoroutine(mIEnumeratorLoadAsync);
        mIsOverTime = true;
        mLoadError = "over time";
    }

    /// <summary>
    /// 加载数据
    /// </summary>
    public WWW www
    {
        get
        {
            return mWWW;
        }
    }
}
