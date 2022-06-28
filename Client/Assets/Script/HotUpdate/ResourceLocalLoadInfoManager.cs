using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 资源本地加载信息管理
/// </summary>
public class ResourceLocalLoadInfoManager
{
    private static ResourceLocalLoadInfoManager msInstance = new ResourceLocalLoadInfoManager();

    private Dictionary<string, bool> mDicLoadInfo = new Dictionary<string, bool>();        //Key：路径；Value：是否从Apk中加载

    private ResourceLocalLoadInfoManager()
    {
    }

    public static ResourceLocalLoadInfoManager Instance
    {
        get { return msInstance; }
    }

    /// <summary>
    /// 添加资源本地加载信息
    /// </summary>
    /// <param name="path"></param>
    /// <param name="isFromApk"></param>
    public void AddLoadInfo(string path, bool isFromApk, bool ignorePostfix)
    {
        if (mDicLoadInfo == null)
            return;

        string tmpPath = path;
        if (ignorePostfix)
        {
            //去后缀
            int tmpIdx = tmpPath.LastIndexOf(".");
            if (tmpIdx >= 0)
                tmpPath = tmpPath.Remove(tmpIdx);
        }
        mDicLoadInfo[tmpPath] = isFromApk;
        DEBUG.Log("Resource local load info - " + tmpPath + "(realPath : " + path + ") : " + isFromApk);
    }
    /// <summary>
    /// 移除资源本地加载信息
    /// </summary>
    /// <param name="path"></param>
    public void RemoveLoadInfo(string path, bool ignorePostfix)
    {
        if (mDicLoadInfo == null)
            return;

        string tmpPath = path;
        if (ignorePostfix)
        {
            //去后缀
            int tmpIdx = tmpPath.LastIndexOf(".");
            if (tmpIdx >= 0)
                tmpPath = tmpPath.Remove(tmpIdx);
        }
        if (!mDicLoadInfo.ContainsKey(tmpPath))
            return;
        mDicLoadInfo.Remove(tmpPath);
    }
    /// <summary>
    /// 指定路径是否从Apk加载
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public bool IsLoadFromApk(string path, bool ignorePostfix)
    {
        if (mDicLoadInfo == null)
            return false;

        bool tmpIsFromApk = false;
        string tmpPath = path;
        if (ignorePostfix)
        {
            //去后缀
            int tmpIdx = tmpPath.LastIndexOf(".");
            if (tmpIdx >= 0)
                tmpPath = tmpPath.Remove(tmpIdx);
        }
        if (!mDicLoadInfo.TryGetValue(tmpPath, out tmpIsFromApk))
            return false;
        return tmpIsFromApk;
    }
}
