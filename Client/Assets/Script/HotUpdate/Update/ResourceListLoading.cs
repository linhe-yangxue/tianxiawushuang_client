using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

/// <summary>
/// 资源文件列表元素标示数据
/// </summary>
public class ResourceListIDData
{
    public string path = "";        //本地路径
    public int size = 0;            //文件大小
    public string md5 = "";         //文件MD5

    public static bool operator ==(ResourceListIDData lhs, ResourceListIDData rhs)
    {
        bool tmpIsLhsNull = (lhs as object == null);
        bool tmpIsRhsNull = (rhs as object == null);
        if (tmpIsLhsNull && tmpIsRhsNull)
            return true;
        if (tmpIsLhsNull || tmpIsRhsNull)
            return false;

        return (
            lhs.path == rhs.path &&
            lhs.size == rhs.size &&
            lhs.md5 == rhs.md5);
    }
    public static bool operator !=(ResourceListIDData lhs, ResourceListIDData rhs)
    {
        return !(lhs == rhs);
    }

    public static bool operator ==(ResourceListIDData lhs, ResourceListData rhs)
    {
        bool tmpIsLhsNull = (lhs as object == null);
        bool tmpIsRhsNull = (rhs as object == null);
        if (tmpIsLhsNull && tmpIsRhsNull)
            return true;
        if (tmpIsLhsNull || tmpIsRhsNull)
            return false;

        return (
            lhs.path == rhs.IDData.path &&
            lhs.size == rhs.IDData.size &&
            lhs.md5 == rhs.IDData.md5);
    }
    public static bool operator !=(ResourceListIDData lhs, ResourceListData rhs)
    {
        return !(lhs == rhs);
    }
    public static bool operator ==(ResourceListData lhs, ResourceListIDData rhs)
    {
        bool tmpIsLhsNull = (lhs as object == null);
        bool tmpIsRhsNull = (rhs as object == null);
        if (tmpIsLhsNull && tmpIsRhsNull)
            return true;
        if (tmpIsLhsNull || tmpIsRhsNull)
            return false;

        return (
            lhs.IDData.path == rhs.path &&
            lhs.IDData.size == rhs.size &&
            lhs.IDData.md5 == rhs.md5);
    }
    public static bool operator !=(ResourceListData lhs, ResourceListIDData rhs)
    {
        return !(lhs == rhs);
    }
}
/// <summary>
/// 资源文件列表元素数据
/// </summary>
public class ResourceListData
{
    private string mSrcData = "";           //源数据
    private ResourceListIDData mIDData;     //标示数据

    public string SrcData
    {
        set { mSrcData = value; }
        get { return mSrcData; }
    }
    public ResourceListIDData IDData
    {
        set { mIDData = value; }
        get { return mIDData; }
    }
}
/// <summary>
/// 资源文件列表
/// </summary>
public class ResourceList
{
    public List<ResourceListIDData> files = new List<ResourceListIDData>();         //资源文件列表
    private List<ResourceListData> mListFileData = new List<ResourceListData>();    //资源文件完整数据
    private Dictionary<string, ResourceListIDData> mDicFileData = new Dictionary<string, ResourceListIDData>();     //资源文件完整数据

    public List<ResourceListData> GetListFileData()
    {
        return mListFileData;
    }
    public Dictionary<string, ResourceListIDData> GetDicFileData()
    {
        return mDicFileData;
    }

    public void Init()
    {
        if (files != null)
            files.Clear();
        if (mListFileData != null)
            mListFileData.Clear();
    }

    /// <summary>
    /// 加入数据
    /// </summary>
    /// <param name="IDData"></param>
    public void AddIDData(ResourceListIDData IDData)
    {
        if (files == null)
            return;

        ResourceListIDData tmpIDData = files.Find((ResourceListIDData tmp) =>
        {
            return (IDData.path == tmp.path);
        });
        if (tmpIDData == null as ResourceListIDData)
        {
            files.Add(IDData);
            if (mDicFileData != null)
                mDicFileData[IDData.path] = IDData;
        }
        else
        {
            tmpIDData.md5 = IDData.md5;
            tmpIDData.size = IDData.size;
        }
    }

    public void Save(string filePath)
    {
        string tmpStr = JCode.Encode(this);
        byte[] tmpBytes = Encoding.UTF8.GetBytes(tmpStr.ToCharArray());
        GameCommon.SaveFile(filePath, tmpBytes);
    }

    /// <summary>
    /// 获取资源列表数据
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public static ResourceList GetResourceList(string filePath)
    {
        if (!File.Exists(filePath))
            return null;

        FileStream tmpFile = new FileStream(filePath, FileMode.Open);
        byte[] tmpFileData = new byte[tmpFile.Length];
        tmpFile.Read(tmpFileData, 0, tmpFileData.Length);
        tmpFile.Close();
        string tmpStrFileData = Encoding.UTF8.GetString(tmpFileData);
        ResourceList tmpResourceList = JCode.Decode<ResourceList>(tmpStrFileData);
        for (int i = 0, count = tmpResourceList.files.Count; i < count; i++)
            tmpResourceList.GetDicFileData()[tmpResourceList.files[i].path] = tmpResourceList.files[i];
        return tmpResourceList;
    }
    /// <summary>
    /// 获取包内资源列表数据
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public static ResourceList GetInnerResourceList(string filePath)
    {
        //去后缀
        int tmpIdx = filePath.LastIndexOf(".");
        string tmpLocalVersionFilePath = ((tmpIdx >= 0) ? filePath.Remove(tmpIdx) : filePath);
        //去除前面'/'
        while (tmpLocalVersionFilePath.IndexOf("/") == 0)
            tmpLocalVersionFilePath = tmpLocalVersionFilePath.Substring(1);
        UnityEngine.Object tmpObj = Resources.Load<TextAsset>(tmpLocalVersionFilePath);
        TextAsset tmpTAObj = tmpObj as TextAsset;
        if (tmpTAObj == null || tmpTAObj.bytes == null)
            return null;

        byte[] tmpFileData = tmpTAObj.bytes;
        string tmpStrFileData = Encoding.UTF8.GetString(tmpFileData);
        ResourceList tmpResourceList = JCode.Decode<ResourceList>(tmpStrFileData);
        for (int i = 0, count = tmpResourceList.files.Count; i < count; i++)
            tmpResourceList.GetDicFileData()[tmpResourceList.files[i].path] = tmpResourceList.files[i];
        return tmpResourceList;
    }

    /// <summary>
    /// 合并apk内、外的资源列表
    /// </summary>
    /// <param name="apkInnerList"></param>
    /// <param name="apkExternalList"></param>
    /// <returns></returns>
    public static ResourceList MergeResourceList(ResourceList apkInnerList, ResourceList apkExternalList)
    {
        if (apkInnerList == null)
            return apkExternalList;
        if (apkExternalList == null)
            return apkInnerList;
        ResourceList tmpList = new ResourceList();
        //加入apk内部数据
        foreach (KeyValuePair<string, ResourceListIDData> tmpPair in apkInnerList.GetDicFileData())
            tmpList.AddIDData(tmpPair.Value);
        //加入apk外部数据
        foreach (KeyValuePair<string, ResourceListIDData> tmpPair in apkExternalList.GetDicFileData())
            tmpList.AddIDData(tmpPair.Value);
        return tmpList;
    }
}

/// <summary>
/// 游戏资源文件列表更新基类
/// </summary>
public class ResourceListLoading : DefaultLoading
{
    protected List<LoadingStepData> mListLoadingStep = new List<LoadingStepData>();
    protected byte[] mFileListBytes;
    protected ResourceList mResourceList = new ResourceList();
    protected int mFileTotalCount = 0;      //文件个数
    protected long mFileTotalSize = 0;      //文件总大小

    protected string mFilePath = "";        //文件路径
    protected HotUpdateParam mHotUpdateParam;

    public ResourceListLoading()
    {
        mListLoadingStep.Add(new LoadingStepData()
        {
            RangeProgress = new Vector2(0.0f, 0.5f),
            LoadingFunction = __LoadFile
        });
        mListLoadingStep.Add(new LoadingStepData()
        {
            RangeProgress = new Vector2(0.5f, 1.0f),
            LoadingFunction = __FileListHandle
        });
    }

    public byte[] FileListBytes
    {
        get { return mFileListBytes; }
    }
    public ResourceList resourceList
    {
        get { return mResourceList; }
    }
    public int FileTotalCount
    {
        get { return mFileTotalCount; }
    }
    public long FileTotalSize
    {
        get { return mFileTotalSize; }
    }

    protected override IEnumerator _OnStartLoading(string path, Action<LoadingProgressParam> progressCallback, object param)
    {
        if (mListLoadingStep == null || mListLoadingStep.Count <= 0)
            yield break;

        mFilePath = path;
        mHotUpdateParam = param as HotUpdateParam;

        mFileListBytes = null;
        mResourceList.Init();
        mFileTotalCount = 0;
        mFileTotalSize = 0;

        if (CoroutineOperation == null)
        {
            mHasLoadError = true;
            mLoadError = "ResourceListLoading CoroutineOperation is null.";
            HotUpdateLog.Log("ResourceListLoading CoroutineOperation is null.");
            yield break;
        }

        for (int i = 0, count = mListLoadingStep.Count; i < count; i++)
        {
            LoadingStepData tmpStepData = mListLoadingStep[i];
            if (tmpStepData.LoadingFunction != null)
            {
                yield return CoroutineOperation.StartCoroutine(tmpStepData.LoadingFunction(
                    tmpStepData.RangeProgress, progressCallback, null));
            }
        }
    }

    protected override void _OnStopLoading()
    {
        base._OnStopLoading();
    }

    /// <summary>
    /// 加载文件
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    private IEnumerator __LoadFile(Vector2 rangeProgress, Action<LoadingProgressParam> progressCallback, object param)
    {
        if (CoroutineOperation == null)
        {
            mHasLoadError = true;
            mLoadError = "ResourceListLoading CoroutineOperation is null.";
            HotUpdateLog.Log("ResourceListLoading CoroutineOperation is null.");
            yield break;
        }

        float tmpProgressStart = rangeProgress.x;
        float tmpProgressRange = rangeProgress.y;
        LoadingProgressParam tmpCurrProgress = new LoadingProgressParam();

        ResourceLoading tmpResourceLoading = new ResourceLoading();
        tmpResourceLoading.CoroutineOperation = CoroutineOperation;
        HotUpdateLog.TempLog("Start __LoadFile " + mFilePath);
        yield return CoroutineOperation.StartCoroutine(tmpResourceLoading.StartLoad(mFilePath, (LoadingProgressParam progress) =>
        {
            tmpCurrProgress.Progress = tmpProgressStart + progress.Progress * tmpProgressRange;
            tmpCurrProgress.BytesLoaded = progress.BytesLoaded;
            tmpCurrProgress.BytesTotal = progress.BytesTotal;
            if (progressCallback != null)
                progressCallback(tmpCurrProgress);
        }, mHotUpdateParam));
        if (tmpResourceLoading.ErrorInfo() != "")
        {
            HotUpdateLog.TempLog("Stop __LoadFile " + mFilePath + ". Error = " + tmpResourceLoading.ErrorInfo());
            mHasLoadError = true;
            mLoadError = tmpResourceLoading.ErrorInfo();
            yield break;
        }
        HotUpdateLog.TempLog("Stop __LoadFile " + mFilePath);
        mFileListBytes = tmpResourceLoading.www.bytes;
    }

    /// <summary>
    /// 处理列表文件
    /// </summary>
    /// <param name="fileListBytes"></param>
    /// <returns></returns>
    private IEnumerator __FileListHandle(Vector2 rangeProgress, Action<LoadingProgressParam> progressCallback, object param)
    {
        float tmpProgressStart = rangeProgress.x;
        float tmpProgressRange = rangeProgress.y;
        LoadingProgressParam tmpCurrProgress = new LoadingProgressParam();

        HotUpdateLog.TempLog("Start __FileListHandle");
        string tmpStrFileListBytes = Encoding.UTF8.GetString(mFileListBytes);
        mResourceList = JCode.Decode<ResourceList>(tmpStrFileListBytes);
        mFileTotalCount = mResourceList.files.Count;
        for (int i = 0; i < mFileTotalCount; i++)
        {
            ResourceListIDData tmpIDData = mResourceList.files[i];
            mResourceList.GetListFileData().Add(new ResourceListData()
            {
                SrcData = tmpStrFileListBytes,
                IDData = tmpIDData
            });
            mFileTotalSize += tmpIDData.size;
            tmpCurrProgress.Progress = tmpProgressStart + ((float)(i + 1) / (float)mFileTotalCount) * tmpProgressRange;
            if (progressCallback != null)
                progressCallback(tmpCurrProgress);
            yield return null;
        }
        HotUpdateLog.TempLog("Stop __FileListHandle");
    }
}
