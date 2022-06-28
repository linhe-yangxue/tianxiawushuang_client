using UnityEngine;
using System.Collections;
using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

public class ClientVersion
{
    public string version = "";     //版本号
    public string channel = "";     //渠道号

    public static string FillVersion(string target)
    {
        return FillVersion(target, (int)GAME_VERSION_FIELD.FIELD_MAX);
    }
    public static string FillVersion(string target, int maxCount)
    {
        if (target == null)
            return "";

        string[] tmpTarget = target.Split(new char[] { '.' });
        List<string> tmpListTarget = new List<string>(tmpTarget);
        for (int i = tmpListTarget.Count; i < maxCount; i++)
            tmpListTarget.Add("0");
        string tmpRet = "";
        for (int i = 0, count = tmpListTarget.Count; i < count; i++)
        {
            if (i != 0)
                tmpRet += ".";
            tmpRet += tmpListTarget[i];
        }
        return tmpRet;
    }

    public static string ReplaceSubVersion(string target, int[] targetIdx, string[] source)
    {
        if (target == null || targetIdx == null || source == null)
            return "";
        if(targetIdx.Length != source.Length)
            return "";

        string[] tmpTarget = target.Split(new char[] { '.' });
        for (int i = 0, count = targetIdx.Length; i < count; i++)
        {
            if (targetIdx[i] < 0 || targetIdx[i] >= count)
                continue;
            tmpTarget[targetIdx[i]] = source[i];
        }
        string tmpRet = "";
        for (int i = 0, count = tmpTarget.Length; i < count; i++)
        {
            if (i != 0)
                tmpRet += ".";
            tmpRet += tmpTarget[i];
        }
        return tmpRet;
    }

    public static string ReplaceSubVersionFrom(string target, int[] targetIdx, string source)
    {
        if (target == null || targetIdx == null || source == null)
            return "";

        string[] tmpSource = source.Split(new char[] { '.' });
        string[] tmpTarget = target.Split(new char[] { '.' });
        int tmpSrcCount = tmpSource.Length;
        for (int i = 0, count = targetIdx.Length; i < count; i++)
        {
            if (targetIdx[i] < 0 || targetIdx[i] >= count || targetIdx[i] >= tmpSrcCount)
                continue;
            tmpTarget[targetIdx[i]] = tmpSource[targetIdx[i]];
        }
        string tmpRet = "";
        for (int i = 0, count = tmpTarget.Length; i < count; i++)
        {
            if (i != 0)
                tmpRet += ".";
            tmpRet += tmpTarget[i];
        }
        return tmpRet;
    }
}

/// <summary>
/// Group渠道数据
/// </summary>
public class VersionConfigGroupChannel
{
    public string id = "";      //渠道号
    public string updUrl = "";  //渠道更新地址
}
/// <summary>
/// Group.txt中每个数组项
/// </summary>
public class VersionConfigGroup
{
    public string id = "";              //Group配置顺序
    public string version = "";         //版本号
    public string plateform = "";       //平台标志
    public VersionConfigGroupChannel[] channel;     //渠道信息列表
    public string group = "";           //Group编号
}

/// <summary>
/// GroupItem.tx中服务器列表项数据
/// </summary>
public class VersionConfigGroupItemServer
{
    public string id = "";      //服务器ID
    public string name = "";    //服务器名称
    public string ip = "";      //服务器IP
    public string port = "";    //服务器端口
    public string state = "";   //服务器状态
}
/// <summary>
/// GroupItem.txt中数据
/// </summary>
public class VersionConfigGroupItem
{
    public string minversion = "";          //支持的最低版本
    public string maxversion = "";          //当前最新版本
    public string limitmemory = "";         //内存限制
    public string limitdevicelist = "";     //设备限制
    public string notice = "";              //静态公告文件地址
    public string[] patchpath;              //静态补丁文件地址
    public VersionConfigGroupItemServer[] serverlist;       //服务器列表
}

/// <summary>
/// 最小支持分辨率开关数据
/// </summary>
public class ConfigToggleCheckResolution
{
    public bool isCheck = true;
    public string min = "4:3";
    public string max = "0";

    private Vector2 mMin = Vector2.zero;
    private Vector2 mMax = Vector2.zero;

    public void CalcCurrentResolutionValue()
    {
        mMin = __CalcResolutionValue(min);
        mMax = __CalcResolutionValue(max);
    }
    private Vector2 __CalcResolutionValue(string str)
    {
        if (str == null)
            return Vector2.zero;
        string[] tmpStrValue = str.Split(new char[] { ':' });
        if (tmpStrValue.Length < 2)
            return Vector2.zero;
        Vector2 tmpValue = Vector2.zero;
        if (!float.TryParse(tmpStrValue[0], out tmpValue.x) || !float.TryParse(tmpStrValue[1], out tmpValue.y))
            return Vector2.zero;
        if (tmpValue.y == 0.0f)
            return Vector2.zero;
        return tmpValue;
    }

    public Vector2 Min
    {
        get { return mMin; }
    }
    public Vector2 Max
    {
        get { return mMax; }
    }
    public float MinResolution
    {
        get
        {
            if (mMin == Vector2.zero || mMin.y == 0.0f)
                return 0.0f;
            return (mMin.x * 1.0f / mMin.y);
        }
    }
    public float MaxResolution
    {
        get
        {
            if (mMax == Vector2.zero || mMax.y == 0.0f)
                return 0.0f;
            return (mMax.x * 1.0f / mMax.y);
        }
    }

    public static bool IsBigger(Vector2 src, Vector2 dst)
    {
        if (src == null || dst == null)
            return false;

        if (src.x * dst.y > dst.x * src.y)
            return true;
        return false;
    }
}
/// <summary>
/// 配置开关
/// </summary>
public class ConfigToggle
{
    public bool isCheckApkUpdate = true;        //是否检查整包更新
    public bool isUseHotUpdate = true;          //是否使用热更新
    public bool isUseSDK = true;                //是否使用SDK
    public bool isOpenHotUpdateLog = true;      //是否打开热更新Log
    public bool isOpenHotUpdateTempLog = true;  //是否打开热更新临时Log
    public bool isEnableGuide = false;          //是否开启新手引导
    public bool isEnablePrologue = false;       //是否开启序章假战斗
    public bool isUseHttps = false;             //是否在游戏中使用https
    public ConfigToggleCheckResolution isCheckResolution;       //是否检查最小分辨率
}

/// <summary>
/// Apk更新类型
/// </summary>
enum ApkUpdateType
{
    NoUpdate,       //不需要更新
    MustUpdate,     //强制更新
    NeedUpdate      //需要更新
}

public class VersionConfigUpdate : ILoading
{
    private static string msRelativeVersionPath = "/Version.txt";
    private static string msRelativeGroupPath = "/group.txt";
    private static string msRelativeGroupItemPath = "/{0}/groupitem.txt";
    private static string msRelativeConfigToggle = "/{0}/ConfigToggle.txt";

    private HotUpdateParam mHotUpdateParam;

    private ClientVersion mOldVersion;         //当前版本
    private ClientVersion mMatchVersion;        //匹配版本
    private ClientVersion mLastestVersion;      //最新版本
    private string mGroupID = "";               //匹配的区组ID
    private VersionConfigGroupItem mGroupItem;  //组数据
    private string mMatchedApkUpdateUrl = "";   //匹配到的Apk更新版本

    private LoadingProgressParam mProgress = new LoadingProgressParam();
    private bool mLoadSuccess = true;       //是否加载成功

    public static string RelativeVersionPath
    {
        get { return msRelativeVersionPath; }
    }

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
    public IEnumerator StartLoad(string path, Action<LoadingProgressParam> progressCallback, object param)
    {
        mHotUpdateParam = param as HotUpdateParam;
        CoroutineOperation = mHotUpdateParam.CoroutineOperation;

        mProgress.Init();
        if (progressCallback != null)
            progressCallback(mProgress);

        if (CoroutineOperation == null)
        {
            mLoadSuccess = false;
            HotUpdateLog.Log("VersionConfigUpdate CoroutineOperation is null.");
            yield break;
        }

        float tmpProgressStep = 1.0f / 3.0f;
        float tmpCurrProgressStep = 0.0f;

        __LoadCurrVersionFile();
        if (!mLoadSuccess)
        {
            HotUpdateLog.Log("加载当前客户端版本失败");
            yield break;
        }
        yield return CoroutineOperation.StartCoroutine(__LoadGroupFile((float tmpProgress) =>
        {
            //进度
            mProgress.Progress = tmpCurrProgressStep + tmpProgressStep * tmpProgress;
            if (progressCallback != null)
                progressCallback(mProgress);
        }));
        tmpCurrProgressStep += tmpProgressStep;
        if (!mLoadSuccess)
            yield break;
        if (mGroupID == "")
        {
            //没找到GroupID，不进行更新
            mProgress.Progress = 1.0f;
            mHotUpdateParam.ListVersionDiff = new List<GAME_VERSION_FIELD_DIFF>();
            for (int i = 0, count = (int)GAME_VERSION_FIELD.FIELD_MAX; i < count; i++)
                mHotUpdateParam.ListVersionDiff.Add(GAME_VERSION_FIELD_DIFF.EQUAL);

            //TODO
            HotUpdateLog.Log("未匹配到GroupID");
            if (mHotUpdateParam.StateActionHandle != null)
                mHotUpdateParam.StateActionHandle(HOT_UPDATE_STATE.NO_MATCH_GROUP_ID, null);
            while (true)
                yield return null;
            yield break;
        }

        yield return CoroutineOperation.StartCoroutine(__LoadGroupItemFile((float tmpProgress) =>
        {
            //进度
            mProgress.Progress = tmpCurrProgressStep + tmpProgressStep * tmpProgress;
            if (progressCallback != null)
                progressCallback(mProgress);
        }));
        tmpCurrProgressStep += tmpProgressStep;
        if (!mLoadSuccess)
            yield break;

        yield return CoroutineOperation.StartCoroutine(__LoadConfigToggle((float tmpProgress) =>
        {
            //进度
            mProgress.Progress = tmpCurrProgressStep + tmpProgressStep * tmpProgress;
            if (progressCallback != null)
                progressCallback(mProgress);
        }));
        tmpCurrProgressStep += tmpProgressStep;
        if (!mLoadSuccess)
            yield break;

        //设置最新版本
        mLastestVersion = new ClientVersion()
        {
            version = mMatchVersion.version,
            channel = mMatchVersion.channel
        };

        //检查是否整包更新
        if (HotUpdateLoading.IsCheckApkUpdate)
        {
            ApkUpdateType tmpApkUpdateType = __CheckUpdateApk();
            if (tmpApkUpdateType != ApkUpdateType.NoUpdate)
            {
                bool tmpContinueLogical = false;
                HotUpdateCallbackParam tmpCBParam = new HotUpdateCallbackParam()
                {
                    Param = mMatchedApkUpdateUrl,
                    Callback = (object cbParam) =>
                    {
                        tmpContinueLogical = true;
                    }
                };
                HOT_UPDATE_STATE tmpHotUpdateState = HOT_UPDATE_STATE.SUCCESS;
                if (tmpApkUpdateType == ApkUpdateType.MustUpdate)
                    tmpHotUpdateState = HOT_UPDATE_STATE.MUST_UPDATE_APK;
                else if (tmpApkUpdateType == ApkUpdateType.NeedUpdate)
                    tmpHotUpdateState = HOT_UPDATE_STATE.NEED_UPDATE_APK;
                if (mHotUpdateParam.StateActionHandle != null)
                    mHotUpdateParam.StateActionHandle(tmpHotUpdateState, tmpCBParam);
                while (!tmpContinueLogical)
                    yield return null;

                //除了配置号之外，将最新版本号设置为本地当前最新版本
                mLastestVersion.version = ClientVersion.ReplaceSubVersionFrom(
                    mLastestVersion.version, new int[] { 0, 1, 2, 4 }, mOldVersion.version);
            }
        }
        else
        {
            //除了配置号之外，将最新版本号设置为本地当前最新版本
            mLastestVersion.version = ClientVersion.ReplaceSubVersionFrom(
                mLastestVersion.version, new int[] { 0, 1, 2, 4 }, mOldVersion.version);
        }
        HotUpdateLog.Log("Lastest version = " + mLastestVersion.version + ", channel = " + mLastestVersion.channel);

        mHotUpdateParam.ListVersionDiff = GameVersion.GetFullDifferentInfo(
            new GameVersion(mOldVersion.version), new GameVersion(mLastestVersion.version));

        //将服务器列表存在本地数据
        for (int i = 0, count = mGroupItem.serverlist.Length; i < count; i++)
            GlobalModule.ServerList.Add(mGroupItem.serverlist[i]);

        //重新设置服务器IP列表
        mHotUpdateParam.ListAddress = new List<string>();
        for (int i = 0, count = mGroupItem.patchpath.Length; i < count; i++)
        {
            //依次附加版本号、渠道号到更新地址
            mHotUpdateParam.ListAddress.Add(mGroupItem.patchpath[i] + "/" + mMatchVersion.version + "/" + mMatchVersion.channel + "/Update");
        }
        mHotUpdateParam.CurrentListAddressIndex = 0;

        HotUpdateLoading.SelectedGroupItem = mGroupItem;
        if (HotUpdateLoading.IsUseHotUpdate)
        {
            HotUpdateLoading.ServerVersion = mMatchVersion;
            HotUpdateLoading.LastestClientVersion = mLastestVersion;
            //改为所有都更新完了后保存文件
//            SaveLastestVersion(mLastestVersion);
        }

        mProgress.Progress = 1.0f;
        if (progressCallback != null)
            progressCallback(mProgress);
    }
    /// <summary>
    /// 停止加载
    /// </summary>
    public void StopLoad()
    {
        //TODO
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
        //TODO
        return false;
    }
    /// <summary>
    /// 是否加载完成
    /// </summary>
    /// <returns></returns>
    public bool IsComplete()
    {
        return (LoadProgress().Progress >= 1.0f);
    }
    /// <summary>
    /// 是否加载成功
    /// </summary>
    /// <returns></returns>
    public bool IsSuccess()
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
    /// 加载当前版本文件
    /// </summary>
    private void __LoadCurrVersionFile()
    {
//         //检查文件是否存在
//         string tmpLocalVersionFilePath = GameCommon.DynamicAbsoluteGameDataPath + HotUpdatePath.RelativeVersion + msRelativeVersionPath;
//         string tmpStrFileObj = "";
//         if (!File.Exists(tmpLocalVersionFilePath))
//         {
//             tmpLocalVersionFilePath = "Version" + msRelativeVersionPath;
//             string tmpLocalVersionFileNoExt = tmpLocalVersionFilePath.Remove(tmpLocalVersionFilePath.IndexOf("."));
//             UnityEngine.Object tmpObj = Resources.Load(tmpLocalVersionFileNoExt);
//             TextAsset tmpTAObj = tmpObj as TextAsset;
//             if (tmpTAObj == null)
//             {
//                 mOldVersion = new ClientVersion();
//                 return;
//             }
//             tmpStrFileObj = Encoding.ASCII.GetString(tmpTAObj.bytes);
//         }
//         else
//         {
//             FileStream tmpFS = new FileStream(tmpLocalVersionFilePath, FileMode.Open);
//             byte[] tmpFileData = new byte[tmpFS.Length];
//             tmpFS.Read(tmpFileData, 0, tmpFileData.Length);
//             tmpFS.Close();
//             tmpStrFileObj = Encoding.ASCII.GetString(tmpFileData);
//         }
//         mOldVersion = JCode.Decode<ClientVersion>(tmpStrFileObj);
        mOldVersion = HotUpdateLoading.OldClientVersion;
    }

    /// <summary>
    /// 下载Group.txt
    /// </summary>
    /// <returns></returns>
    private IEnumerator __LoadGroupFile(Action<float> progressCallback)
    {
        if (CoroutineOperation == null)
        {
            mLoadSuccess = false;
            HotUpdateLog.Log("VersionConfigUpdate CoroutineOperation is null.");
            yield break;
        }

        float tmpProgressStep = 1.0f / 2.0f;
        float tmpCurrProgressStep = 0.0f;
        float tmpCurrProgress = 0.0f;

        //        string tmpFilePath = mConfigServer + msRelativeGroupPath;
        string tmpFilePath = msRelativeGroupPath;
        ResourceLoading tmpResourceLoading = new ResourceLoading();
        tmpResourceLoading.CoroutineOperation = CoroutineOperation;
        yield return CoroutineOperation.StartCoroutine(tmpResourceLoading.StartLoad(tmpFilePath, (LoadingProgressParam tmpProgress) =>
        {
            //进度
            tmpCurrProgress = tmpCurrProgressStep + tmpProgressStep * tmpProgress.Progress;
            if (progressCallback != null)
                progressCallback(tmpCurrProgress);
        }, mHotUpdateParam));
        tmpCurrProgressStep += tmpProgressStep;

        //转换数据
        string tmpStrDataObj = Encoding.UTF8.GetString(tmpResourceLoading.www.bytes);
        VersionConfigGroup[] tmpListGroup = JCode.Decode<VersionConfigGroup[]>(tmpStrDataObj);
        //查找匹配项
        GameVersion tmpCurrVersion = new GameVersion(mOldVersion.version);
        int tmpCurrChannel = int.Parse(mOldVersion.channel);
        List<VersionConfigGroup> tmpListMatchGroup = new List<VersionConfigGroup>();
        for (int i = 0, count = tmpListGroup.Length; i < count; i++)
        {
            VersionConfigGroup tmpGroup = tmpListGroup[i];
            if (__IsMatchSuccess(tmpCurrVersion, new GameVersion(tmpGroup.version)))
            {
                for (int j = 0, jCount = tmpGroup.channel.Length; j < jCount; j++)
                {
                    int tmpChannel = int.Parse(tmpGroup.channel[j].id);
                    if (tmpCurrChannel == tmpChannel)
                    {
                        tmpListMatchGroup.Add(tmpGroup);
                        break;
                    }
                }
            }
            tmpCurrProgress = tmpCurrProgressStep + tmpProgressStep * ((float)(i + 1) / (float)count);
            if (progressCallback != null)
                progressCallback(tmpCurrProgress);
            yield return null;
        }
        tmpCurrProgressStep += tmpProgressStep;
        //按照GroupID进行升序排序
        tmpListMatchGroup.Sort((VersionConfigGroup tmpL, VersionConfigGroup tmpR) =>
        {
            int tmpLValue = int.Parse(tmpL.id);
            int tmpRValue = int.Parse(tmpR.id);
            if (tmpLValue < tmpRValue)
                return -1;
            else if (tmpLValue == tmpRValue)
                return 0;
            else
                return 1;
        });
        //选取第一个匹配项
        if (tmpListMatchGroup.Count <= 0)
        {
            HotUpdateLog.Log("未找到" + msRelativeGroupPath + "中数组匹配项\n当前版本：" + mOldVersion.version + "，当前渠道：" + mOldVersion.channel);
            yield break;
        }
        VersionConfigGroup tmpMatchedGroup = tmpListMatchGroup[0];
        mMatchVersion = new ClientVersion()
        {
            version = tmpMatchedGroup.version,
            channel = mOldVersion.channel
        };
        HotUpdateLog.Log("Matched version = " + mMatchVersion.version + ", channel = " + mMatchVersion.channel);
        mMatchedApkUpdateUrl = "";
        for (int i = 0, count = tmpMatchedGroup.channel.Length; i < count; i++)
        {
            int tmpChannel = int.Parse(tmpMatchedGroup.channel[i].id);
            if (tmpChannel == tmpCurrChannel)
            {
                mMatchedApkUpdateUrl = tmpMatchedGroup.channel[i].updUrl;
                break;
            }
        }
        mGroupID = tmpMatchedGroup.group;

        if (progressCallback != null)
            progressCallback(1.0f);
    }
    /// <summary>
    /// 是否匹配成功
    /// </summary>
    /// <param name="src"></param>
    /// <param name="dst"></param>
    /// <returns></returns>
    private bool __IsMatchSuccess(GameVersion src, GameVersion dst)
    {
        List<GAME_VERSION_FIELD_DIFF> tmpFullDiff = GameVersion.GetFullDifferentInfo(src, dst);
        bool tmpIsMatch = true;
        //只用前3个配置项来匹配
        for (int i = 0, count = 3; i < count; i++)
        {
            if (tmpFullDiff[i] == GAME_VERSION_FIELD_DIFF.LOW)
                break;
            else if (tmpFullDiff[i] == GAME_VERSION_FIELD_DIFF.EQUAL)
                continue;
            else if (tmpFullDiff[i] == GAME_VERSION_FIELD_DIFF.HIGH)
            {
                tmpIsMatch = false;
                break;
            }
        }
        return tmpIsMatch;
    }

    /// <summary>
    /// 下载GroupItem.txt
    /// </summary>
    /// <returns></returns>
    private IEnumerator __LoadGroupItemFile(Action<float> progressCallback)
    {
        if (CoroutineOperation == null)
        {
            mLoadSuccess = false;
            HotUpdateLog.Log("VersionConfigUpdate CoroutineOperation is null.");
            yield break;
        }

        float tmpProgressStep = 1.0f;
        float tmpCurrProgressStep = 0.0f;
        float tmpCurrProgress = 0.0f;

//        string tmpFilePath = mConfigServer + string.Format(msRelativeGroupItemPath, mGroupID);
        string tmpFilePath = string.Format(msRelativeGroupItemPath, mGroupID);
        ResourceLoading tmpResourceLoading = new ResourceLoading();
        tmpResourceLoading.CoroutineOperation = CoroutineOperation;
        yield return CoroutineOperation.StartCoroutine(tmpResourceLoading.StartLoad(tmpFilePath, (LoadingProgressParam tmpProgress) =>
        {
            //TODO 进度
            tmpCurrProgress = tmpCurrProgressStep + tmpProgressStep * tmpProgress.Progress;
            if (progressCallback != null)
                progressCallback(tmpCurrProgress);
        }, mHotUpdateParam));
        tmpCurrProgressStep += tmpProgressStep;

        GameVersion tmpCurrGameVersion = new GameVersion(mOldVersion.version);

        //转换数据
        string tmpStrDataObj = Encoding.UTF8.GetString(tmpResourceLoading.www.bytes);
        mGroupItem = JCode.Decode<VersionConfigGroupItem>(tmpStrDataObj);

        if (progressCallback != null)
            progressCallback(1.0f);
    }
    /// <summary>
    /// 检查是否需要更新apk
    /// </summary>
    /// <returns></returns>
    private ApkUpdateType __CheckUpdateApk()
    {
        GameVersion tmpCurr = new GameVersion(mOldVersion.version);
        GameVersion tmpMin = new GameVersion(mGroupItem.minversion);
        GameVersion tmpMax = new GameVersion(mGroupItem.maxversion);
        GameVersion tmpNew = new GameVersion(mMatchVersion.version);
        if (tmpCurr < tmpMin)   //检查最低版本
            return ApkUpdateType.MustUpdate;
        else if (tmpCurr < tmpNew)
        {
            if (tmpCurr > tmpMax)
                return ApkUpdateType.MustUpdate;
            else
                return ApkUpdateType.NeedUpdate;
        }
        return ApkUpdateType.NoUpdate;
    }

    /// <summary>
    /// 下载配置开关文件
    /// </summary>
    /// <param name="progressCallback"></param>
    /// <returns></returns>
    private IEnumerator __LoadConfigToggle(Action<float> progressCallback)
    {
        if (CoroutineOperation == null)
        {
            mLoadSuccess = false;
            HotUpdateLog.Log("VersionConfigUpdate CoroutineOperation is null.");
            yield break;
        }

        float tmpProgressStep = 1.0f;
        float tmpCurrProgressStep = 0.0f;
        float tmpCurrProgress = 0.0f;

//        string tmpFilePath = mConfigServer + string.Format(msRelativeConfigToggle, mGroupID);
        string tmpFilePath = string.Format(msRelativeConfigToggle, mGroupID);
        ResourceLoading tmpResourceLoading = new ResourceLoading();
        tmpResourceLoading.CoroutineOperation = CoroutineOperation;
        yield return CoroutineOperation.StartCoroutine(tmpResourceLoading.StartLoad(tmpFilePath, (LoadingProgressParam tmpProgress) =>
        {
            //TODO 进度
            tmpCurrProgress = tmpCurrProgressStep + tmpProgressStep * tmpProgress.Progress;
            if (progressCallback != null)
                progressCallback(tmpCurrProgress);
        }, mHotUpdateParam));
        tmpCurrProgressStep += tmpProgressStep;

        //转换数据
        string tmpStrDataObj = Encoding.UTF8.GetString(tmpResourceLoading.www.bytes);
        ConfigToggle tmpConfigToggle = JCode.Decode<ConfigToggle>(tmpStrDataObj);
        if (tmpConfigToggle == null)
        {
            HotUpdateLog.Log("ConfigToggle error.");
            yield break;
        }
        HotUpdateLog.Log("ConfigToggle - " + tmpStrDataObj);
        if (tmpConfigToggle.isCheckResolution == null)
        {
            HotUpdateLog.Log("ConfigToggle isCheckResolution is required.");
            yield break;
        }

        //整包更新
        HotUpdateLoading.IsCheckApkUpdate = tmpConfigToggle.isCheckApkUpdate;
        //热更新
        HotUpdateLoading.IsUseHotUpdate = tmpConfigToggle.isUseHotUpdate;
        //SDK
        CommonParam.isUseSDK = tmpConfigToggle.isUseSDK;
        //是否开启热更新日志
        HotUpdateLog.IsLog = tmpConfigToggle.isOpenHotUpdateLog;
        //是否开启热更新临时日志
        HotUpdateLog.IsTempLog = tmpConfigToggle.isOpenHotUpdateTempLog;
        //新手引导
        Guide.EnableGuide = tmpConfigToggle.isEnableGuide;
        Guide.EnablePrologue = tmpConfigToggle.isEnablePrologue;
        //是否在游戏中使用https
        CommonParam.isUseHttps = tmpConfigToggle.isUseHttps;
        //是否检查最小分辨率
        HotUpdateLoading.MinResolution = tmpConfigToggle.isCheckResolution;
        HotUpdateLoading.MinResolution.CalcCurrentResolutionValue();
    }

    /// <summary>
    /// 保存最新的配置到本地文件
    /// </summary>
    public static void SaveLastestVersion(ClientVersion tmp)
    {
        string tmpLocalVersionFilePath = GameCommon.DynamicAbsoluteGameDataPath + HotUpdatePath.RelativeVersion + msRelativeVersionPath;
        string tmpStrDataObj = JCode.Encode(tmp);
        byte[] tmpDataObj = Encoding.UTF8.GetBytes(tmpStrDataObj);
        GameCommon.SaveFile(tmpLocalVersionFilePath, tmpDataObj);
    }
    /// <summary>
    /// 删除本地记录的最新版本号文件
    /// </summary>
    public static void DeleteLastestVersion()
    {
        string tmpLocalVersionFilePath = GameCommon.DynamicAbsoluteGameDataPath + HotUpdatePath.RelativeVersion + msRelativeVersionPath;
        GameCommon.DeleteFile(tmpLocalVersionFilePath);
    }
}
