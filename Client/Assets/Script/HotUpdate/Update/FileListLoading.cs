using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using System.Text;

/// <summary>
/// 文件列表数据
/// </summary>
public class FileListData
{
    private string mKey = "";               //文件标示
    private string mRelativePath = "";      //相对路径
    private byte[] mData;                   //文件数据

    private int mFileTotalOPCount = 0;      //需要操作的文件总个数
    private int mFileTotalOPSize = 0;       //需要操作的文件总大小
    private ResourceList mLocalResourceList;        //本地资源列表
    private List<HotUpdateFileData> mUpdateData = new List<HotUpdateFileData>();    //文件操作列表

    public string Key
    {
        set { mKey = value; }
        get { return mKey; }
    }
    public string RelativePath
    {
        set { mRelativePath = value; }
        get { return mRelativePath; }
    }
    public byte[] Data
    {
        set { mData = value; }
        get { return mData; }
    }

    public int FileTotalOPCount
    {
        set { mFileTotalOPCount = value; }
        get { return mFileTotalOPCount; }
    }
    public int FileTotalOPSize
    {
        set { mFileTotalOPSize = value; }
        get { return mFileTotalOPSize; }
    }
    public ResourceList LocalResourceList
    {
        set { mLocalResourceList = value; }
        get { return mLocalResourceList; }
    }
    public List<HotUpdateFileData> UpdateData
    {
        get { return mUpdateData; }
    }
}

/// <summary>
/// 所有更新列表更新
/// </summary>
public class FileListLoading : DefaultLoading
{
    private static FileListLoading msInstance;

    private Dictionary<string, FileListData> mDicFileListData = new Dictionary<string, FileListData>();     //文件更新列表数据，Key：相对路径
    private int mFileTotalCount = 0;
    private int mFileTotalSize = 0;

    public FileListLoading()
        : base()
    {
        InitData();
    }

    public int FileTotalCount
    {
        get { return mFileTotalCount; }
    }
    public int FileTotalSize
    {
        get { return mFileTotalSize; }
    }

    /// <summary>
    /// 初始化数据
    /// </summary>
    public void InitData()
    {
        //初始化列表数据
        mDicFileListData.Clear();

        string tmpPath = "";
        //配置项列表
        tmpPath = HotUpdatePath.RelativeConfig + ConfigUpdate.ConfigListRelativePath;
        mDicFileListData[tmpPath] = new FileListData() { Key = ConfigUpdate.ConfigListRelativePath, RelativePath = tmpPath };
    }

    /// <summary>
    /// 获取文件列表数据
    /// </summary>
    /// <param name="listName"></param>
    /// <returns></returns>
    public FileListData GetFileListData(string listName)
    {
        FileListData tmpFileListData = null;
        if (!mDicFileListData.TryGetValue(listName, out tmpFileListData))
            return null;
        return tmpFileListData;
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
            HotUpdateLog.Log("FileListLoading CoroutineOperation is null.");
            yield break;
        }

        HotUpdateParam tmpHotUpdateParam = param as HotUpdateParam;

        mFileTotalCount = 0;
        mFileTotalSize = 0;

        foreach (KeyValuePair<string, FileListData> tmpPair in mDicFileListData)
        {
            FileListData tmpFileListData = tmpPair.Value;
//            string tmpAddress = tmpUpdateServer + tmpFileListData.RelativePath;
            string tmpAddress = tmpFileListData.RelativePath;
            ResourceListLoading tmpResourceListLoading = new ResourceListLoading();
            tmpResourceListLoading.CoroutineOperation = CoroutineOperation;
            yield return CoroutineOperation.StartCoroutine(tmpResourceListLoading.StartLoad(tmpAddress, null, tmpHotUpdateParam));
            if (!tmpResourceListLoading.IsSuccess())
            {
                //下载未成功
                yield break;
            }
            tmpFileListData.Data = tmpResourceListLoading.FileListBytes;
            HotUpdateLog.TempLog("Start _CheckUpdateFiles " + tmpAddress);
            _CheckUpdateFiles(tmpFileListData);
            HotUpdateLog.TempLog("Stop _CheckUpdateFiles " + tmpAddress);
            mFileTotalCount += tmpFileListData.FileTotalOPCount;
            mFileTotalSize += tmpFileListData.FileTotalOPSize;
        }
    }
    /// <summary>
    /// 检查需要下载的文件列表
    /// </summary>
    /// <param name="fileListData"></param>
    /// <returns></returns>
    protected void _CheckUpdateFiles(FileListData fileListData)
    {
        string tmpLocalConfigListPath = GameCommon.DynamicAbsoluteGameDataPath + fileListData.RelativePath;
//         fileListData.LocalResourceList = ResourceList.MergeResourceList(
//             ResourceList.GetInnerResourceList(fileListData.RelativePath), ResourceList.GetResourceList(tmpLocalConfigListPath));
        ResourceList tmpLocalResourceList = ResourceList.MergeResourceList(
            ResourceList.GetInnerResourceList(fileListData.RelativePath), ResourceList.GetResourceList(tmpLocalConfigListPath));
        HotUpdateLog.TempLog("_CheckUpdateFiles innerPath = " + fileListData.RelativePath + ", path = " + tmpLocalConfigListPath + ", localResourceList " + (fileListData.LocalResourceList != null));
//         if (fileListData.LocalResourceList == null)
//             fileListData.LocalResourceList = new ResourceList();
        if (tmpLocalResourceList == null)
            tmpLocalResourceList = new ResourceList();
        string tmpStrData = Encoding.UTF8.GetString(fileListData.Data);
        ResourceList tmpServerResourceList = JCode.Decode<ResourceList>(tmpStrData);

        fileListData.FileTotalOPCount = 0;
        fileListData.FileTotalOPSize = 0;
        fileListData.UpdateData.Clear();
        fileListData.LocalResourceList = ResourceList.GetResourceList(tmpLocalConfigListPath);
        if (fileListData.LocalResourceList == null)
            fileListData.LocalResourceList = new ResourceList();

        HotUpdateLog.TempLog("Start Check ResourceList, server file count = " + tmpServerResourceList.files.Count + ", local file count = " + fileListData.LocalResourceList.files.Count);
        long tmpBytesTotal = 0;
        for (int i = 0, count = tmpServerResourceList.files.Count; i < count; i++)
        {
            ResourceListIDData tmpServerConfigListData = tmpServerResourceList.files[i];
            HotUpdateLog.TempLog("Checking file " + tmpServerConfigListData.path);

            bool tmpIsLoad = true;
            ResourceListIDData tmpLocalConfigListData = null;
//             if (fileListData.LocalResourceList.DicFileData.TryGetValue(tmpServerConfigListData.path, out tmpLocalConfigListData))
//             {
//                 if (tmpServerConfigListData == tmpLocalConfigListData)
//                     tmpIsLoad = false;
//             }
            if (tmpLocalResourceList.GetDicFileData().TryGetValue(tmpServerConfigListData.path, out tmpLocalConfigListData))
            {
                if (tmpServerConfigListData == tmpLocalConfigListData)
                    tmpIsLoad = false;
            }
            if (tmpIsLoad)
            {
                HotUpdateLog.TempLog("Need load file " + tmpServerConfigListData.path);
                fileListData.UpdateData.Add(new HotUpdateFileData()
                {
                    Path = tmpServerConfigListData.path,
                    FileOP = HOT_UPDATE_FILE_OP.OVERRIDE,
                    LocalData = tmpLocalConfigListData,
                    ServerData = tmpServerConfigListData
                });
                tmpBytesTotal += tmpServerConfigListData.size;
                fileListData.FileTotalOPCount += 1;
                fileListData.FileTotalOPSize += tmpServerConfigListData.size;
                fileListData.LocalResourceList.AddIDData(tmpServerConfigListData);
            }
        }
        HotUpdateLog.TempLog("Stop Check ResourceList, server file count = " + tmpServerResourceList.files.Count + ", local file count = " + fileListData.LocalResourceList.files.Count);
    }
}
