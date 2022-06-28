using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using System.Text;

/// <summary>
/// 热更新显示
/// </summary>
public interface IHotUpdateUI
{
    /// <summary>
    /// 设置更新项名称
    /// </summary>
    /// <param name="name"></param>
    void SetLoadingName(string name);
    /// <summary>
    /// 设置更新描述
    /// </summary>
    /// <param name="info"></param>
    void SetLoadingDescription(string desc);
    /// <summary>
    /// 设置更新进度
    /// </summary>
    /// <param name="progress"></param>
    void SetProgress(float progress);
}

/// <summary>
/// 检查热更新
/// </summary>
public class HotUpdate
{
    private ILoading[] mLoading;            //更新对象

    private ICoroutineOperation mCoroutineOperation;      //协同
    private IHotUpdateUI mView;                 //热更新显示

    private static ClientVersion msLastClientVersion;       //保存成静态，游戏成功启动时修改GlobalModule里的版本字段

    public HotUpdate()
    {
        mLoading = new ILoading[] {
            new VersionConfigUpdate(),
//            new VersionUpdate(),
            new ConfigUpdate(),
            new ResourceListLoading()
        };
    }

    /// <summary>
    /// 初始化最新客户端版本数据为当前版本
    /// </summary>
    public static void InitLastestClientVersion()
    {
        string tmpLocalVersionFilePath = GameCommon.DynamicAbsoluteGameDataPath + HotUpdatePath.RelativeVersion + VersionConfigUpdate.RelativeVersionPath;
        string tmpStrFileObj = "";
        if (!File.Exists(tmpLocalVersionFilePath))
        {
            //如果外部数据缺失，读包内部数据
            tmpLocalVersionFilePath = "Version" + VersionConfigUpdate.RelativeVersionPath;
            //去后缀
            tmpLocalVersionFilePath = tmpLocalVersionFilePath.Remove(tmpLocalVersionFilePath.LastIndexOf("."));
            UnityEngine.Object tmpObj = Resources.Load<TextAsset>(tmpLocalVersionFilePath);
            TextAsset tmpTAObj = tmpObj as TextAsset;
            if (tmpTAObj != null)
                tmpStrFileObj = Encoding.ASCII.GetString(tmpTAObj.bytes);
        }
        else
        {
            FileStream tmpFS = new FileStream(tmpLocalVersionFilePath, FileMode.Open);
            byte[] tmpFileData = new byte[tmpFS.Length];
            tmpFS.Read(tmpFileData, 0, tmpFileData.Length);
            tmpFS.Close();
            tmpStrFileObj = Encoding.ASCII.GetString(tmpFileData);
        }
        if (tmpStrFileObj != "")
            msLastClientVersion = JCode.Decode<ClientVersion>(tmpStrFileObj);
    }
    /// <summary>
    /// 最新客户端版本号
    /// </summary>
    public static ClientVersion LastestClientVersion
    {
        set { msLastClientVersion = value; }
        get { return msLastClientVersion; }
    }

    /// <summary>
    /// 获取两个版本的字段值差异
    /// </summary>
    /// <param name="src"></param>
    /// <param name="dst"></param>
    /// <returns>只返回有差异的字段名</returns>
    public static List<GAME_VERSION_FIELD> GetDifferentVersionField(GameVersion src, GameVersion dst)
    {
        int tmpMaxFieldCount = (int)GAME_VERSION_FIELD.FIELD_MAX;
        List<GAME_VERSION_FIELD> tmpFields = new List<GAME_VERSION_FIELD>();
        for (int i = 0; i < tmpMaxFieldCount; i++)
        {
            GAME_VERSION_FIELD tmpField = (GAME_VERSION_FIELD)i;
            if (src[tmpField] != dst[tmpField])
                tmpFields.Add(tmpField);
        }
        return tmpFields;
    }
    /// <summary>
    /// 获取两个版本之间每个字段的差异，src作为比较的左值
    /// </summary>
    /// <param name="src"></param>
    /// <param name="dst"></param>
    /// <returns></returns>
    public static List<GAME_VERSION_FIELD_DIFF> GetFullDifferentInfo(GameVersion src, GameVersion dst)
    {
        int tmpMaxFieldCount = (int)GAME_VERSION_FIELD.FIELD_MAX;
        List<GAME_VERSION_FIELD_DIFF> tmpFieldsDiff = new List<GAME_VERSION_FIELD_DIFF>();
        for (int i = 0; i < tmpMaxFieldCount; i++)
        {
            GAME_VERSION_FIELD tmpField = (GAME_VERSION_FIELD)i;
            int tmpSrcFieldValue = src[tmpField];
            int tmpDstFieldValue = dst[tmpField];
            if (tmpSrcFieldValue > tmpDstFieldValue)
                tmpFieldsDiff.Add(GAME_VERSION_FIELD_DIFF.HIGH);
            else if (tmpSrcFieldValue == tmpDstFieldValue)
                tmpFieldsDiff.Add(GAME_VERSION_FIELD_DIFF.EQUAL);
            else
                tmpFieldsDiff.Add(GAME_VERSION_FIELD_DIFF.LOW);
        }
        return tmpFieldsDiff;
    }

    public ICoroutineOperation CoroutineHelper
    {
        set { mCoroutineOperation = value; }
        get { return mCoroutineOperation; }
    }
    public IHotUpdateUI View
    {
        set { mView = value; }
        get { return mView; }
    }

    /// <summary>
    /// 检查更新
    /// </summary>
    /// <param name="hotUpdateIP"></param>
    /// <param name="hotUpdatePort">如果为-1，不用端口</param>
    /// <returns></returns>
    public IEnumerator Check(string hotUpdateIP, string hotUpdatePort)
    {
        HotUpdateLog.Log("Check start");

        HotUpdateParam tmpParam = new HotUpdateParam()
        {
            CoroutineOperation = mCoroutineOperation
        };
        __Progress = 0.0f;

        if (mCoroutineOperation != null && mView != null)
        {
            int tmpLoadingCount = mLoading.Length;
            float tmpProgressStep = 1.0f / (float)tmpLoadingCount;
            float tmpCurrProgressStep = 0.0f;
            for (int i = 0; i < tmpLoadingCount; i++)
            {
                yield return mCoroutineOperation.StartCoroutine(mLoading[i].StartLoad("", (LoadingProgressParam tmpProgress) =>
                {
                    __Progress = tmpCurrProgressStep + tmpProgressStep * tmpProgress.Progress;
                }, tmpParam));
                tmpCurrProgressStep += tmpProgressStep;
                if (!mLoading[i].IsSuccess())
                {
                    //如果热更新有一项没成功，退出游戏
                    HotUpdateLog.Log("Hot update error.\ni = " + i);
#if !UNITY_EDITOR && !NO_USE_SDK
                    if (CommonParam.isUseSDK)
                        U3DSharkSDK.Instance.ExitGame();
                    else
#endif
                        Application.Quit();

                    break;
                }
            }
        }

        __Progress = 1.0f;
        //为了能够显示处进度条达到100%
        yield return new WaitForSeconds(1.0f);

        HotUpdateLog.Log("Check end");
    }

    /// <summary>
    /// 当前更新进度
    /// </summary>
    private float __Progress
    {
        set
        {
            if (mView != null)
                mView.SetProgress(value);
        }
    }
}
