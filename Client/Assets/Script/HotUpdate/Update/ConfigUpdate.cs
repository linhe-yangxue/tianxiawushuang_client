using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

/// <summary>
/// 配置文件更新
/// </summary>
public class ConfigUpdate : ILoading
{
    private static string msConfigListRelativePath = "/ConfigList.txt";         //版本文件列表本地现对路径

    private HotUpdateParam mHotUpdateParam;
    private FileListData mFileListData;
    private LoadingProgressParam mProgress = new LoadingProgressParam();
    private ResourceList mLoadedFileList = new ResourceList();      //已经加载的文件列表
    private bool mLoadSuccess = true;       //是否下载成功

    /// <summary>
    /// 协同操作
    /// </summary>
    public ICoroutineOperation CoroutineOperation { set; get; }

    public static string ConfigListRelativePath
    {
        get { return msConfigListRelativePath; }
    }

    /// <summary>
    /// 开始加载
    /// </summary>
    /// <param name="path">加载路径</param>
    /// <param name="progressCallback">加载进度回调</param>
    /// <param name="param">加载参数</param>
    /// <returns></returns>
    public virtual IEnumerator StartLoad(string path, Action<LoadingProgressParam> progressCallback, object param)
    {
        mHotUpdateParam = param as HotUpdateParam;
        CoroutineOperation = mHotUpdateParam.CoroutineOperation;

        mProgress.Init();
        if (progressCallback != null)
            progressCallback(mProgress);

        if (CoroutineOperation == null)
        {
            mLoadSuccess = false;
            HotUpdateLog.Log("ConfigUpdate CoroutineOperation is null.");
            yield break;
        }

        //判断是否需要更新，只有过高才需要更新
        if (mHotUpdateParam.ListVersionDiff[(int)GAME_VERSION_FIELD.PATCH] == GAME_VERSION_FIELD_DIFF.EQUAL)
        {
            mProgress.Progress = 1.0f;
            if (progressCallback != null)
                progressCallback(mProgress);
            yield break;
        }

        string tmpConfigListLocalPath = GameCommon.DynamicAbsoluteGameDataTempPath + HotUpdatePath.RelativeConfig + msConfigListRelativePath;
        mFileListData = mHotUpdateParam.Param as FileListData;
        mHotUpdateParam.Param = null;

//         yield return CoroutineOperation.StartCoroutine(__LoadConfigListFile(null));
//         if (!mLoadSuccess)
//             yield break;
        //保存最新的本地资源列表
        mFileListData.LocalResourceList.Save(tmpConfigListLocalPath);

        yield return CoroutineOperation.StartCoroutine(__UpdateFiles((LoadingProgressParam tmpProgress) =>
        {
            //进度
            mProgress.CopyFrom(tmpProgress);
            if (progressCallback != null)
                progressCallback(mProgress);
        }));
        if (!mLoadSuccess)
            yield break;

        //将新的配置列表文件覆盖到旧的路径
        string tmpNewConfigListLocalPath = GameCommon.DynamicAbsoluteGameDataTempPath + HotUpdatePath.RelativeConfig + msConfigListRelativePath;
        string tmpOldConfigListPath = GameCommon.DynamicAbsoluteGameDataPath + HotUpdatePath.RelativeConfig + msConfigListRelativePath;
        try
        {
            GameCommon.MoveFile(tmpNewConfigListLocalPath, tmpOldConfigListPath, true);
        }
        catch (Exception excep)
        {
            HotUpdateLog.Log("Move file error = " + excep.ToString());
        }

        mProgress.Progress = 1.0f;
        if (progressCallback != null)
            progressCallback(mProgress);
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
        return mProgress;
    }
    /// <summary>
    /// 是否在加载中
    /// </summary>
    /// <returns></returns>
    public virtual bool IsLoading()
    {
        //TODO
        return true;
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
        return (IsComplete() && mLoadSuccess);
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

    /// <summary>
    /// 下载文件列表
    /// </summary>
    /// <returns></returns>
    private IEnumerator __LoadConfigListFile(Action<LoadingProgressParam> progressCallback)
    {
        if (CoroutineOperation == null)
        {
            HotUpdateLog.Log("ConfigUpdate CoroutineOperation is null.");
            yield break;
        }

//        string tmpConfigListFile = mHotUpdateParam.Address + HotUpdatePath.RelativeConfig + msConfigListRelativePath;
        string tmpConfigListFile = HotUpdatePath.RelativeConfig + msConfigListRelativePath;
        ResourceLoading tmpResourceLoading = new ResourceLoading();
        tmpResourceLoading.CoroutineOperation = CoroutineOperation;
        yield return CoroutineOperation.StartCoroutine(tmpResourceLoading.StartLoad(tmpConfigListFile, (LoadingProgressParam tmpProgress) =>
        {
            if (progressCallback != null)
                progressCallback(tmpProgress);
        }, mHotUpdateParam));
        string tmpConfigListLocalPath = GameCommon.DynamicAbsoluteGameDataTempPath + HotUpdatePath.RelativeConfig + msConfigListRelativePath;
        GameCommon.SaveFile(tmpConfigListLocalPath, tmpResourceLoading.www.bytes);
    }
    /// <summary>
    /// 更新配置文件
    /// </summary>
    /// <returns></returns>
    private IEnumerator __UpdateFiles(Action<LoadingProgressParam> progressCallback)
    {
        if (CoroutineOperation == null)
        {
            HotUpdateLog.Log("ConfigUpdate CoroutineOperation is null.");
            yield break;
        }

        List<HotUpdateFileData> tmpUpdateData = new List<HotUpdateFileData>();
        long tmpBytesTotal = 0;
//         yield return CoroutineOperation.StartCoroutine(__CheckUpdateFiles(tmpUpdateData,
//             (long bytesTotal) =>
//             {
//                 tmpBytesTotal = bytesTotal;
//             }, null));

        tmpUpdateData = mFileListData.UpdateData;
        tmpBytesTotal = mFileListData.FileTotalOPSize;
        yield return CoroutineOperation.StartCoroutine(__HandleUpdateFiles(tmpUpdateData, tmpBytesTotal, (LoadingProgressParam tmpProgress) =>
        {
            //进度
            if (progressCallback != null)
                progressCallback(tmpProgress);
        }));
    }
    /// <summary>
    /// 检查更新文件数据
    /// </summary>
    /// <param name="updateData"></param>
    private IEnumerator __CheckUpdateFiles(List<HotUpdateFileData> updateData, Action<long> funcBytesTotal, Action<LoadingProgressParam> progressCallback)
    {
        string tmpNewConfigListLocalPath = GameCommon.DynamicAbsoluteGameDataTempPath + HotUpdatePath.RelativeConfig + msConfigListRelativePath;
        string tmpOldConfigListLocalPath = GameCommon.DynamicAbsoluteGameDataPath + HotUpdatePath.RelativeConfig + msConfigListRelativePath;
        LoadingProgressParam tmpProgress = new LoadingProgressParam();

        ResourceList tmpNewConfigList = ResourceList.GetResourceList(tmpNewConfigListLocalPath);
        if (tmpNewConfigList == null)
        {
            //无新配置文件列表
            HotUpdateLog.Log("未找到配置文件" + tmpNewConfigListLocalPath);
            yield break;
        }
        ResourceList tmpOldConfigList = ResourceList.GetResourceList(tmpOldConfigListLocalPath);
        long tmpBytesTotal = 0;
        for (int i = 0, count = tmpNewConfigList.files.Count; i < count; i++)
        {
            ResourceListIDData tmpNewConfigListData = tmpNewConfigList.files[i];
            if (tmpOldConfigList == null)
            {
                updateData.Add(new HotUpdateFileData()
                {
                    Path = tmpNewConfigListData.path,
                    FileOP = HOT_UPDATE_FILE_OP.OVERRIDE
                });
                tmpBytesTotal += tmpNewConfigListData.size;
            }
            else
            {
                bool tmpIsLoad = true;
                for (int j = 0, jCount = tmpOldConfigList.files.Count; j < jCount; j++)
                {
                    ResourceListIDData tmpOldConfigListData = tmpOldConfigList.files[j];
                    if (tmpNewConfigListData.path == tmpOldConfigListData.path)
                    {
                        if (tmpNewConfigListData == tmpOldConfigListData)
                            tmpIsLoad = false;
                        break;
                    }
                }
                if (tmpIsLoad)
                {
                    updateData.Add(new HotUpdateFileData()
                    {
                        Path = tmpNewConfigListData.path,
                        FileOP = HOT_UPDATE_FILE_OP.OVERRIDE
                    });
                    tmpBytesTotal += tmpNewConfigListData.size;
                }
            }
            tmpProgress.Progress = (float)(i + 1) / (float)count;
            if (progressCallback != null)
                progressCallback(tmpProgress);

            yield return null;
        }
        if (funcBytesTotal != null)
            funcBytesTotal(tmpBytesTotal);
    }
    /// <summary>
    /// 处理更新文件
    /// </summary>
    /// <param name="updateData"></param>
    private IEnumerator __HandleUpdateFiles(List<HotUpdateFileData> updateData, long bytesTotal, Action<LoadingProgressParam> progressCallback)
    {
        if (CoroutineOperation == null)
        {
            HotUpdateLog.Log("ConfigUpdate CoroutineOperation is null.");
            yield break;
        }

#if UNITY_ANDROID || UNITY_IOS
        int tmpActionRet = -1;
        if (NetManager.HasCarrierDataNetConnectable() && bytesTotal > 0)
        {
            //询问是否下载数据包
            if (mHotUpdateParam.StateActionHandle == null)
            {
                mLoadSuccess = false;
                yield break;
            }
            else
            {
                HotUpdateCallbackParam tmpCBParam = new HotUpdateCallbackParam();
                tmpCBParam.Param = LoadingHelper.Instance.ConvertBToMB(bytesTotal);
                tmpCBParam.Callback = (object actionRet) =>
                {
                    tmpActionRet = (int)actionRet;
                };
                mHotUpdateParam.StateActionHandle(HOT_UPDATE_STATE.REQUEST_LOAD_DATAPACKAGE, tmpCBParam);
            }
            while (true)
            {
                if (tmpActionRet == 1)
                    break;
                else if (tmpActionRet == 0)
                {
                    mLoadSuccess = false;
                    HotUpdateLoading.ApplicationQuit();
                    while (true)
                        yield return null;
                }
                yield return null;
            }
        }
#endif

        string tmpOldConfigListLocalPath = GameCommon.DynamicAbsoluteGameDataPath + HotUpdatePath.RelativeConfig + msConfigListRelativePath;

        LoadingProgressParam tmpProgress = new LoadingProgressParam();
        int tmpBytesLoaded = 0;
        tmpProgress.BytesTotal = bytesTotal;
        for (int i = 0, count = updateData.Count; i < count; i++)
        {
            HotUpdateFileData tmpUpdateData = updateData[i];
//            string tmpServerPath = mHotUpdateParam.Address + HotUpdatePath.RelativeConfig + updateData[i].Path;
            string tmpServerPath = HotUpdatePath.RelativeConfig + updateData[i].Path;
            ResourceLoading tmpResourceLoading = new ResourceLoading();
            tmpResourceLoading.CoroutineOperation = CoroutineOperation;
            tmpResourceLoading.ActionCallback = (ResourceLoadingAction loadingAction) =>
            {
                switch (loadingAction)
                {
                    case ResourceLoadingAction.CanotLoad:
                        {
                            //保存到本地列表
                            mLoadedFileList.Save(tmpOldConfigListLocalPath);
                        } break;
                }
            };
            tmpResourceLoading.VerifyIntegrityCallback = (byte[] loadedData) =>
            {
                //对下载好的本地文件进行MD5校验
                return __CheckMD5(loadedData, tmpUpdateData.ServerData.md5);
            };
            yield return CoroutineOperation.StartCoroutine(tmpResourceLoading.StartLoad(tmpServerPath, (LoadingProgressParam progress) =>
            {
                if (tmpResourceLoading.www == null)
                    return;
                tmpProgress.BytesLoaded = tmpBytesLoaded + tmpResourceLoading.www.bytesDownloaded;
                tmpProgress.Progress = (float)tmpProgress.BytesLoaded / (float)tmpProgress.BytesTotal;
                if (progressCallback != null)
                    progressCallback(tmpProgress);
            }, mHotUpdateParam));
            string tmpLocalFilePath = GameCommon.DynamicAbsoluteGameDataPath + HotUpdatePath.RelativeConfig + tmpUpdateData.Path;
            GameCommon.SaveFile(tmpLocalFilePath, tmpResourceLoading.www.bytes);
            mLoadedFileList.AddIDData(tmpUpdateData.ServerData);
            tmpBytesLoaded += tmpResourceLoading.www.size;
        }
        tmpProgress.Progress = 1.0f;
        if (progressCallback != null)
            progressCallback(tmpProgress);
    }

    /// <summary>
    /// 验证指定数据是否和指定MD5值相同
    /// </summary>
    /// <param name="bytes"></param>
    /// <param name="md5"></param>
    /// <returns></returns>
    private bool __CheckMD5(byte[] bytes, string md5)
    {
        MemoryStream tmpMS = new MemoryStream(bytes);
        string tmpMD5 = MD5.CalculateMD5(tmpMS).ToLower();
        string tmpSrcMD5 = md5.ToLower();
        return (tmpMD5 == tmpSrcMD5);
    }
}
