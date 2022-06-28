using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.IO;

public enum ResourceLoadingAction
{
    CanotLoad,      //不能下载
    ContinueLoad    //继续下载
}

/// <summary>
/// 游戏资源基类
/// </summary>
public class ResourceLoading : DefaultLoading
{
    protected const int MAX_VERIFY_INTEGRITY_COUNT = 5;          //最大下载文件验证次数

    protected HotUpdateParam mHotUpdateParam;
    protected FileLoading mFileLoading;

    protected float mSpeedLimit = 1024.0f;          //字节单位
    protected float mCurrSpeed = 0.0f;              //字节单位
    protected float mCurrLoadBytes = 0.0f;
    protected float mCurrCalcSpeedTime = 0.0f;
    protected float mCurrCalcSpeedDelta = 0.03f;
    protected float mLimitSpeedTime = -1.0f;
    protected float mLimitSpeedDelta = 5.0f;

    protected Action<ResourceLoadingAction> mActionCallback;            //触发动作时回调
    protected Func<byte[], bool> mVerifyIntegrityCallback;              //校验资源完整性回调

    public WWW www
    {
        get
        {
            if (mFileLoading == null)
                return null;
            return mFileLoading.www;
        }
    }

    /// <summary>
    /// 触发动作时回调
    /// 回调函数参数：触发时回调函数
    /// </summary>
    public Action<ResourceLoadingAction> ActionCallback
    {
        set { mActionCallback = value; }
        get { return mActionCallback; }
    }
    /// <summary>
    /// 校验资源完整性回调，如果不完整将重新下载该文件
    /// 回调函数参数：资源已下载完整字节数据
    /// 回调函数返回值：是否已下载完整。true下载文件成功；false下载文件失败，重新下载文件。
    /// </summary>
    public Func<byte[], bool> VerifyIntegrityCallback
    {
        set { mVerifyIntegrityCallback = value; }
        get { return mVerifyIntegrityCallback; }
    }

    /// <summary>
    /// 开始加载时被调用
    /// </summary>
    /// <param name="path"></param>
    /// <param name="progressCallback"></param>
    /// <param name="param"></param>
    /// <returns></returns>
    protected override IEnumerator _OnStartLoading(string path, Action<LoadingProgressParam> progressCallback, object param)
    {
        if (CoroutineOperation == null)
        {
            mHasLoadError = true;
            mLoadError = "ResourceLoading CoroutineOperation is null.";
            HotUpdateLog.Log("ResourceLoading CoroutineOperation is null.");
            yield break;
        }

        mHotUpdateParam = param as HotUpdateParam;

        mFileLoading = new FileLoading();
        mFileLoading.CoroutineOperation = CoroutineOperation;
        FileLoadingParam tmpFileLoadingParam = new FileLoadingParam()
        {
            OverTime = 15.0f
        };
        int tmpVerifyIntegrityCount = 0;        //下载文件验证次数
        do
        {
            mHasLoadError = false;
            mLoadError = "";
            string tmpCurrServer = mHotUpdateParam.CurrentServer();
            string tmpPath = tmpCurrServer + path;
            float tmpLastBytes = 0.0f;
            mCurrSpeed = 0.0f;
            mCurrLoadBytes = 0.0f;
            mCurrCalcSpeedTime = 0.0f;
            mLimitSpeedTime = -1.0f;
            HotUpdateLog.TempLog("Start resource load " + tmpPath);
            yield return CoroutineOperation.StartCoroutine(mFileLoading.StartLoad(tmpPath, (LoadingProgressParam progress) =>
            {
                mCurrLoadBytes += (progress.BytesLoaded - tmpLastBytes);
                tmpLastBytes = progress.BytesLoaded;
                mCurrCalcSpeedTime += Time.deltaTime;
                if (mCurrCalcSpeedTime >= mCurrCalcSpeedDelta)
                {
                    mCurrSpeed = mCurrLoadBytes / mCurrCalcSpeedTime;
                    mCurrLoadBytes = 0.0f;
                    mCurrCalcSpeedTime = 0.0f;
                }
                if (mCurrSpeed <= mSpeedLimit)
                {
                    if (mLimitSpeedTime == -1.0f)
                        mLimitSpeedTime = 0.0f;
                    else
                        mLimitSpeedTime += Time.deltaTime;
                }
                else
                    mLimitSpeedTime = -1.0f;
                HotUpdateLog.Log(tmpPath + " loads speed info : Current Bytes = " + mCurrLoadBytes + ", Current Speed = " + mCurrSpeed);
                if (mLimitSpeedTime > mLimitSpeedDelta)
                {
                    mFileLoading.StopLoad();
                    mHasLoadError = true;
                    mLoadError = "load out of time";
                    return;
                }
                if (progressCallback != null)
                    progressCallback(progress);
            }, tmpFileLoadingParam));
            if (mFileLoading.IsSuccess() && !mHasLoadError)
            {
                //校验文件完整性
                if (mVerifyIntegrityCallback != null && !mVerifyIntegrityCallback(mFileLoading.www.bytes))
                {
                    tmpVerifyIntegrityCount += 1;
                    if (tmpVerifyIntegrityCount >= MAX_VERIFY_INTEGRITY_COUNT)
                    {
                        bool tmpNeedBreak = false;
                        yield return CoroutineOperation.StartCoroutine(HotUpdateLoading.PauseForAction(mHotUpdateParam, HOT_UPDATE_STATE.NEED_RESTART_HOTUPDATE, 0.3f, () =>
                        {
                            tmpNeedBreak = true;
                        }));
                        if (tmpNeedBreak)
                            yield break;
                    }
                    //重新下载该文件
                    continue;
                }

                break;
            }
            tmpVerifyIntegrityCount = 0;
            if (!mHasLoadError)
            {
                mHasLoadError = true;
                mLoadError = mFileLoading.ErrorInfo();
            }
            HotUpdateLog.Log("无法下载资源 - " + tmpPath + ". RelativePath = " + path + ". Error - " + mLoadError + ".");

            if (!mHotUpdateParam.HasValidServer())
            {
                __ExcCallbaclAction(ResourceLoadingAction.CanotLoad);
                bool tmpNeedBreak = false;
                yield return CoroutineOperation.StartCoroutine(HotUpdateLoading.PauseForAction(mHotUpdateParam, HOT_UPDATE_STATE.NO_INTERNET, 0.3f, () =>
                {
                    tmpNeedBreak = true;
                }));
                if (tmpNeedBreak)
                    yield break;
                __ExcCallbaclAction(ResourceLoadingAction.ContinueLoad);
                mHotUpdateParam.CurrentListAddressIndex = 0;
            }
            else
                mHotUpdateParam.NextServer();
        } while (true);
    }
    /// <summary>
    /// 停止加载时被调用
    /// </summary>
    protected override void _OnStopLoading()
    {
    }

    protected void __ExcCallbaclAction(ResourceLoadingAction action)
    {
        if (mActionCallback != null)
            mActionCallback(action);
    }
}
