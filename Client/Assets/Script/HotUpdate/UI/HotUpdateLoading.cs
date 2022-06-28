using UnityEngine;
using System.Collections;
using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using DataTable;
using System.Diagnostics;
using Logic;

/// <summary>
/// 热更新阶段
/// </summary>
public enum HOTUPDATE_LOAD_PHASE
{
    CHECK_LEBIAN,               //检查乐变
    CHECK_UPDATE,               //检查更新
    INIT_LOCAL_DATA,            //初始化本地数据
    HOT_UPDATE,                 //进行热更新
    INIT_LOCAL_DATA_COMPLETE,   //本地数据初始化完成
}

/// <summary>
/// 热更新状态
/// </summary>
public enum HOT_UPDATE_STATE
{
    SUCCESS,            //无错误
    FAILED,             //失败
    OK_CANCEL_MESSAGEBOX,   //确认、取消框
    OK_MESSAGEBOX,      //取消框
    NO_INTERNET,        //无网络
    NEED_UPDATE_APK,    //需要更新apk
    MUST_UPDATE_APK,    //必须更新apk
    NO_MATCH_GROUP_ID,  //没有匹配到GroupID
    REQUEST_LOAD_DATAPACKAGE,   //询问下载数据包
    LOAD_DATAPACKAGE,   //下载数据包
    NO_LOAD_DATAPACKGE, //不下载数据包
    NEED_RESTART_HOTUPDATE,     //需要重新启动热更新
    NO_ENOUGH_MEMORY,   //内存不足
    INVALID_RESOLUTION, //分辨率不符合要求
}

/// <summary>
/// 热更新状态回调函数参数
/// </summary>
public class HotUpdateCallbackParam
{
    public object Param;
    public Action<object> Callback;
}

/// <summary>
/// 热更新操作参数，用于热更新操作之间传递
/// </summary>
public class HotUpdateParam
{
    private List<GAME_VERSION_FIELD_DIFF> mListVersionDiff;
    private Action<HOT_UPDATE_STATE, object> mHotUpdateStateAction;           //热更新状态操作
    public Action<HOT_UPDATE_STATE, object> StateActionHandle
    {
        set { mHotUpdateStateAction = value; }
        get { return mHotUpdateStateAction; }
    }

    /// <summary>
    /// 当前可用的服务器IP列表
    /// </summary>
    private List<string> mListAddress = new List<string>();
    public List<string> ListAddress
    {
        set
        {
            mListAddress = value;
            if (mListAddress != null && mCurrListAddressIndex >= 0 && mCurrListAddressIndex < mListAddress.Count)
            {
                string tmpCurrAddress = mListAddress[mCurrListAddressIndex];
                HotUpdateLog.Log("Current use address " + tmpCurrAddress);
            }
        }
        get { return mListAddress; }
    }
    /// <summary>
    /// 当前服务器索引
    /// </summary>
    private int mCurrListAddressIndex = 0;
    public int CurrentListAddressIndex
    {
        set
        {
            mCurrListAddressIndex = value;
            if (mListAddress != null && mCurrListAddressIndex >= 0 && mCurrListAddressIndex < mListAddress.Count)
            {
                string tmpCurrAddress = mListAddress[mCurrListAddressIndex];
                HotUpdateLog.Log("Current use address " + tmpCurrAddress);
            }
        }
        get { return mCurrListAddressIndex; }
    }
    /// <summary>
    /// 获取当前服务器IP
    /// </summary>
    /// <returns></returns>
    public string CurrentServer()
    {
        if (mListAddress == null || mListAddress.Count <= 0)
            return "";
        if (mCurrListAddressIndex >= mListAddress.Count)
            return "";

        return mListAddress[mCurrListAddressIndex];
    }
    /// <summary>
    /// 获取当前服务器IP，并将当前服务器索引后移
    /// </summary>
    /// <returns></returns>
    public string NextServer()
    {
        if (mListAddress == null || mListAddress.Count <= 0)
            return "";
        if (mCurrListAddressIndex >= mListAddress.Count)
            return "";

        int tmpCurrIndex = mCurrListAddressIndex++;
        string tmpCurrAddress = mListAddress[tmpCurrIndex];
        HotUpdateLog.Log("Current use address " + tmpCurrAddress);
        return tmpCurrAddress;
    }
    /// <summary>
    /// 是否还有可用的服务器IP
    /// </summary>
    /// <returns></returns>
    public bool HasValidServer()
    {
        if (mListAddress == null || mListAddress.Count <= 0)
            return false;
        if (mCurrListAddressIndex >= mListAddress.Count)
            return false;
        return true;
    }

    public string Address { set; get; }
    public ICoroutineOperation CoroutineOperation { set; get; }
    public List<GAME_VERSION_FIELD_DIFF> ListVersionDiff
    {
        set { mListVersionDiff = value; }
        get { return mListVersionDiff; }
    }

    public object Param { set; get; }       //额外数据
}

/// <summary>
/// 热更新文件操作类型
/// </summary>
public enum HOT_UPDATE_FILE_OP
{
    OVERRIDE,       //覆盖
    REMOVE          //移除
}
/// <summary>
/// 热更新文件操作数据
/// </summary>
public class HotUpdateFileData
{
    public string Path { set; get; }                    //文件路径
    public HOT_UPDATE_FILE_OP FileOP { set; get; }      //文件更新操作
    public ResourceListIDData LocalData { set; get; }   //本地文件数据
    public ResourceListIDData ServerData { set; get; }  //网络文件数据
}

/// <summary>
/// 热更新相对路径
/// </summary>
public class HotUpdatePath
{
    public static string RelativeVersion = "/Version";      //版本
    public static string RelativeConfig = "/Config";        //配置文件
    public static string RelativeData = "/Data";            //数据
}

/// <summary>
/// 热更新日志
/// </summary>
public class HotUpdateLog
{
    private static bool ms_IsLog = false;           //是否开启日志
    public static bool IsLog
    {
        set { ms_IsLog = value; }
        get { return ms_IsLog; }
    }

    private static bool ms_IsTempLog = false;       //是否记录临时日志
    public static bool IsTempLog
    {
        set { ms_IsTempLog = value; }
        get { return ms_IsTempLog; }
    }

    public static void Log(string info)
    {
        if (!ms_IsLog)
            return;

        DEBUG.Log("HotUpdate - " + info + "\n");
    }

    /// <summary>
    /// 临时日志
    /// </summary>
    /// <param name="info"></param>
    public static void TempLog(string info)
    {
        if (!ms_IsLog || !ms_IsTempLog)
            return;

        Log("Temp - " + info);
    }
}

/// <summary>
/// 热更新界面
/// </summary>
public class HotUpdateLoading : GameLoading
{
    [SerializeField]
    private GameObject m_GOLoadPhase;     //加载阶段对象
    [SerializeField]
    private UILabel m_LBLoadPhaseInfo;    //加载阶段信息

    [SerializeField]
    private UILabel m_LBProgress;       //加载进度
    [SerializeField]
    private UILabel m_LBProgressExtraInfo;   //加载进度信息

    private static VersionConfigGroupItem ms_SelectedGroupItem;     //符合要求的GroupItem
    private static ClientVersion ms_ApkClientVersion;               //包内客户端版本号
    private static ClientVersion ms_OldClientVersion;               //旧客户端版本号
    private static ClientVersion ms_LastClientVersion;              //保存成静态，游戏成功启动时修改GlobalModule里的版本字段
    private static ClientVersion ms_ServerVersion;                  //服务器版本号
    private static bool ms_IsUseHotUpdate = false;                  //是否热更新
    private static bool ms_IsCheckApkUpdate = false;                //是否检查整包更新

    private Stopwatch m_Stopwatch = new Stopwatch();                //用于热更新计时

    private HotUpdateParam m_HotUpdateParam;
    private FileListLoading m_FileListLoading;
    //速度统计
    private float m_LoadSpeedTime = 0.0f;           //速度统计经过时间
    [SerializeField]
    private float m_LoadSpeedResetTime = 0.3f;      //速度统计重置时间
    private long m_LoadDataSize = 0;                //已下载数据大小
    private float m_LoadSpeed = 0.0f;               //当前下载速度

    private string m_LoadExtraProgessInfo = "";     //下载进度信息，用于显示
    private string m_LoadExtraSpeedInfo = "";       //下载速度信息，用于显示

    private SyncActionQueue m_HotUpdateSyncQueue = new SyncActionQueue();       //热更新操作队列
    private SyncActionQueue m_LocalDataSyncQueue = new SyncActionQueue();       //本地数据操作队列

    private static int ms_LeBianUpdateState = -2;       //乐变更新状态
    public static int LeBianUpdateState
    {
        set { ms_LeBianUpdateState = value; }
        get { return ms_LeBianUpdateState; }
    }
    private const float mc_LeBianMaxWaitTime = 1.0f * 60.0f;     //等待乐变响应最长时间
    private float m_LeBianWaitDelta = 0.0f;                     //当前已经等待乐变响应时间

    private static bool m_isAgreeCarrierDataNet = false;  //是否同意通过蜂窝网络下载
    public static bool IsAgreeCarrierDataNet
    {
        set { m_isAgreeCarrierDataNet = value; }
        get { return m_isAgreeCarrierDataNet; }
    }
    private static bool m_isUseApkData = false;          //是否使用包内部数据
    public static bool IsUseApkData
    {
        set { m_isUseApkData = value; }
        get { return m_isUseApkData; }
    }
    private static ConfigToggleCheckResolution m_minResolution = new ConfigToggleCheckResolution();
    public static ConfigToggleCheckResolution MinResolution
    {
        set { m_minResolution = value; }
        get { return m_minResolution; }
    }

    protected override void _OnStart()
    {
        JCode.InitLitJson();
        __InitAllGameData();
        __InitCamera();
        GlobalModule.InitGameDataWithoutTable();
        __LoadRequirementConfig();
        CommonParam.isUseSDK = true;        //如果用了热更新，默认使用sdk
#if !UNITY_EDITOR && !NO_USE_SDK && (UNITY_ANDROID || UNITY_IOS)
        try
        {
            U3DSharkSDK.Instance.InitSDK();
        }
        catch(Exception excep)
        {
        }
#endif
        HotUpdateLog.TempLog("persistentDataPath " + GameCommon.DynamicGameDataRootPath);
        __StartUpdate();
    }

    protected override void _OnLateUpdate()
    {
        __UpdateLoadSpeed();

        __UpdateLoadExtraInfo();

        if (Input.GetKeyDown(KeyCode.Escape))
            ApplicationQuit();
    }

    public static void ApplicationQuit()
    {
#if !UNITY_EDITOR && !NO_USE_SDK
        if (CommonParam.isUseSDK)
        {
            bool is_SdkExit = U3DSharkSDK.Instance.IsHasRequest(U3DSharkAttName.IS_USE_SDK_EXIT_WINDOW);
            if (is_SdkExit)
                U3DSharkSDK.Instance.ExitGame();
            else
                Application.Quit();
        }
        else
#endif
            Application.Quit();
    }

    /// <summary>
    /// 初始化摄像机
    /// </summary>
    private void __InitCamera()
    {
        GameObject tmpGORoot = GameObject.Find("UI Root");
        if (tmpGORoot == null)
            return;
        GameObject tmpGOCamera = GameCommon.FindObject(tmpGORoot, "Camera");
        if (tmpGOCamera == null || tmpGOCamera.GetComponent<AutoCameraResolution>() != null)
            return;
        tmpGOCamera.AddComponent<AutoCameraResolution>();
    }

    /// <summary>
    /// 从游戏场景重新加载热更新场景
    /// </summary>
    public static void ReloadHotUpdateScene()
    {
        RoleInfoTimerManager.Instance.Clear();
        GlobalModule.ClearAllWindow();
        __InitAllGameData();

        GameObject tmpAVP = GameObject.Find("Audio_Vedio_Player");
        if (tmpAVP != null)
            GameObject.DestroyImmediate(tmpAVP);

        GameObject tmpGORoot = GameObject.Find(CommonParam.UIRootName);
        //跳到更新界面
        GameCommon.LoadLevel("HotUpdateLoading");
        if (tmpGORoot != null)
            GameObject.DestroyImmediate(tmpGORoot);

        MainUIScript.mCurIndex = MAIN_WINDOW_INDEX.RoleSelWindow;
        MainUIScript.mLastIndex = MAIN_WINDOW_INDEX.RoleSelWindow;

        GameObject tmpAIRoutine = GameObject.Find("Aid Routines Root");
        if (tmpAIRoutine != null)
            GameObject.DestroyImmediate(tmpAIRoutine);

        GameObject tmpGOGuide = GameObject.Find("guide_helper");
        if (tmpGOGuide != null)
            GameObject.DestroyImmediate(tmpGOGuide);

        GameObject tmpWorldCenter = GameObject.Find("world_center");
        if (tmpWorldCenter != null)
            GameObject.DestroyImmediate(tmpWorldCenter);
    }

    /// <summary>
    /// 开始更新
    /// </summary>
    private void __StartUpdate()
    {
        m_HotUpdateParam = new HotUpdateParam();
        m_HotUpdateParam.CoroutineOperation = this;
        m_HotUpdateParam.StateActionHandle = __CheckHotUpdateState;

#if !UNITY_EDITOR && !NO_USE_SDK && UNITY_ANDROID
        __InitLeBianUpdateAction();
#else
        __InitLoadConfigToggleAction();
#endif
    }

    /// <summary>
    /// 初始化所有游戏数据
    /// </summary>
    private static void __InitAllGameData()
    {
        NetManager.dicSuccessCallBack.Clear();
        NetManager.dicFailCallBack.Clear();
        TableManager.Self.ClearAll();               //清除所有的表
        if (EventCenter.Self != null && EventCenter.Self.mTimeEventManager != null)
            EventCenter.Self.mTimeEventManager.RemoveAll();          //清除所有的事件
        if (EventCenter.Self != null && EventCenter.Self.mFactoryMap != null)
            EventCenter.Self.mFactoryMap.Clear();
        if (ObjectManager.Self != null)
            ObjectManager.Self.ClearAll();
        GlobalModule.sbInitGameData = false;
        GlobalModule.sbIsNeedJudge = true;
        StaticDefine.useHeartbeat = false;

        DataCenter.Self.RemoveAll();
        HttpModule.Instace.Init();

        Notification.Stop();

        AutoBattleAI.InitReset();
        MainProcess.ClearPaoPao();
        Character.Self = null;

        if (MainUI.Self != null)
            MainUI.Self.OnChangeSceneBefore();

        MainProcess.mStage = null;
        if (MainProcess.mMouseCoord != null)
        {
            MainProcess.mMouseCoord.Finish();
            MainProcess.mMouseCoord = null;
        }

        if (Guide.isActive)
            Guide.Terminate(false);
        PVEStageBattle.mBattleControl = BATTLE_CONTROL.MANUAL;
        GuildBossBattle.mBattleControl = BATTLE_CONTROL.MANUAL;
        MainUIScript.mCurIndex = MAIN_WINDOW_INDEX.RoleSelWindow;
        DataCenter.Set("CURRENT_STAGE", 0);
        Notification.Clear();
        Notification.RefuseAll();
        ScrollWorldMapWindow.Reset();
        if (GlobalModule.Instance != null)
            GlobalModule.Instance.StopAllCoroutines();

        if (FriendLogicData.mSelectPlayerData != null)
            FriendLogicData.mSelectPlayerData = null;
        if (PetLogicData.mFreindPetData != null)
            PetLogicData.mFreindPetData = null;

        RoleSelTopLeftWindow.FightStrengthNum = 0;
        TeamManager.ResetData();

        CommonParam.isTeamManagerInit = false;
        CommonParam.isDataInit = false;

        LoginData.Instance.IsLoginTokenValid = false;
        LoginData.Instance.IsGameTokenValid = false;
        LoginData.Instance.IsInGameScene = false;

        //TODO
    }

    /// <summary>
    /// 初始化乐变更新操作
    /// </summary>
    private void __InitLeBianUpdateAction()
    {
        m_HotUpdateSyncQueue.Clear();
        m_HotUpdateSyncQueue.Push(new SyncActionQueueElem()
        {
            Operation = (Action<bool> complete) =>
            {
                __ActionChangePhase(HOTUPDATE_LOAD_PHASE.CHECK_LEBIAN, complete);
            }
        });
        m_HotUpdateSyncQueue.Push(new SyncActionQueueElem() { Operation = __InitLeBianData });
        m_HotUpdateSyncQueue.Push(new SyncActionQueueElem() { Operation = __QueryLeBianUpdate });
        m_HotUpdateSyncQueue.Push(new SyncActionQueueElem()
        {
            Operation = (Action<bool> complete) =>
            {
                StartCoroutine(__CheckLeBianUpdate(complete));
            }
        });
        m_HotUpdateSyncQueue.Push(new SyncActionQueueElem()
        {
            Operation = (Action<bool> callback) =>
            {
                __InitLoadConfigToggleAction();
            }
        });
        m_HotUpdateSyncQueue.Next();
    }

    /// <summary>
    /// 初始化下载配表开关文件
    /// </summary>
    private void __InitLoadConfigToggleAction()
    {
        m_HotUpdateSyncQueue.Clear();
        m_HotUpdateSyncQueue.Push(new SyncActionQueueElem()
        {
            Operation = (Action<bool> complete) =>
            {
                __ActionChangePhase(HOTUPDATE_LOAD_PHASE.CHECK_UPDATE, complete);
            }
        });
        m_HotUpdateSyncQueue.Push(new SyncActionQueueElem() { Operation = __ActionInit });
        m_HotUpdateSyncQueue.Push(new SyncActionQueueElem() { Operation = __ActionLoadGroup });
        m_HotUpdateSyncQueue.Push(new SyncActionQueueElem()
        {
            Operation = (Action<bool> complete) =>
            {
                __InitCheckHotUpdateAction();
            }
        });
        m_HotUpdateSyncQueue.Next();
    }

    /// <summary>
    /// 初始化热更新操作
    /// </summary>
    private void __InitCheckHotUpdateAction()
    {
        if (ms_IsUseHotUpdate)
            __InitAllAction();
        else
            __InitAllNoHotUpdateAction();
    }

    /// <summary>
    /// 将所有操作加入队列
    /// </summary>
    private void __InitAllAction()
    {
        m_HotUpdateSyncQueue.Clear();
        m_HotUpdateSyncQueue.Push(new SyncActionQueueElem() { Operation = __ActionCheckRunRequirement });
        m_HotUpdateSyncQueue.Push(new SyncActionQueueElem()
        {
            Operation = (Action<bool> complete) =>
            {
                //BI首次启动
                BIHelper.SendLaunch();
                if (complete != null)
                    complete(true);
            }
        });
        m_HotUpdateSyncQueue.Push(new SyncActionQueueElem() { Operation = __ActionCheckHotUpdate });
        m_HotUpdateSyncQueue.Push(new SyncActionQueueElem()
        {
            Operation = (Action<bool> complete) =>
            {
                __ActionChangePhase(HOTUPDATE_LOAD_PHASE.HOT_UPDATE, complete);
            }
        });
        m_HotUpdateSyncQueue.Push(new SyncActionQueueElem() { Operation = __ActionHotUpdate });
        m_HotUpdateSyncQueue.Push(new SyncActionQueueElem()
        {
            Operation = (Action<bool> complete) =>
            {
                m_HotUpdateSyncQueue.Clear();
                m_LocalDataSyncQueue.Next();
            }
        });

        m_LocalDataSyncQueue.Clear();
        m_LocalDataSyncQueue.Push(new SyncActionQueueElem()
        {
            Operation = (Action<bool> complete) =>
            {
                __ActionChangePhase(HOTUPDATE_LOAD_PHASE.INIT_LOCAL_DATA, complete);
            }
        });
        m_LocalDataSyncQueue.Push(new SyncActionQueueElem() { Operation = __ActionInitLocalData });
        m_LocalDataSyncQueue.Push(new SyncActionQueueElem()
        {
            Operation = (Action<bool> complete) =>
            {
                __ActionChangePhase(HOTUPDATE_LOAD_PHASE.INIT_LOCAL_DATA_COMPLETE, complete);
            }
        });
        m_LocalDataSyncQueue.Push(new SyncActionQueueElem() { Operation = __ActionLoadGameScene });
        if (m_HotUpdateSyncQueue != null)
            m_HotUpdateSyncQueue.Next();
    }
    /// <summary>
    /// 将除热更新外操作加入队列
    /// </summary>
    private void __InitAllNoHotUpdateAction()
    {
        m_HotUpdateSyncQueue.Clear();
        m_HotUpdateSyncQueue.Push(new SyncActionQueueElem() { Operation = __ActionCheckRunRequirement });
        m_HotUpdateSyncQueue.Push(new SyncActionQueueElem()
        {
            Operation = (Action<bool> complete) =>
            {
                //BI首次启动
                BIHelper.SendLaunch();
                if (complete != null)
                    complete(true);
            }
        });
        m_HotUpdateSyncQueue.Push(new SyncActionQueueElem()
        {
            Operation = (Action<bool> complete) =>
            {
                m_HotUpdateSyncQueue.Clear();
                m_LocalDataSyncQueue.Next();
            }
        });

        m_LocalDataSyncQueue.Clear();
        m_LocalDataSyncQueue.Push(new SyncActionQueueElem()
        {
            Operation = (Action<bool> complete) =>
            {
                __ActionChangePhase(HOTUPDATE_LOAD_PHASE.INIT_LOCAL_DATA, complete);
            }
        });
        m_LocalDataSyncQueue.Push(new SyncActionQueueElem() { Operation = __ActionInitLocalData });
        m_LocalDataSyncQueue.Push(new SyncActionQueueElem()
        {
            Operation = (Action<bool> complete) =>
            {
                __ActionChangePhase(HOTUPDATE_LOAD_PHASE.INIT_LOCAL_DATA_COMPLETE, complete);
            }
        });
        m_LocalDataSyncQueue.Push(new SyncActionQueueElem() { Operation = __ActionLoadGameScene });
        if (m_HotUpdateSyncQueue != null)
            m_HotUpdateSyncQueue.Next();
    }

    /// <summary>
    /// 是否用热更新
    /// </summary>
    /// <returns></returns>
    private void __LoadRequirementConfig()
    {
        NiceTable tmpStaticConfig = null;
        //需要检测是否已加载了StaticConfig
        if (TableManager.GetTable("Tips") == null)
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            TableManager.Self.LoadResouceConfig("StaticConfig", "Config/StaticConfig/StaticConfig.csv", LOAD_MODE.ANIS);
            TableManager.Self.LoadResouceConfig("Tips", "Config/StaticConfig/Tips.csv", LOAD_MODE.ANIS);
#else
        	TableManager.Self.LoadResouceConfig("StaticConfig", "Config/StaticConfig/StaticConfig.csv", LOAD_MODE.BYTES_RES);
        	TableManager.Self.LoadResouceConfig("Tips", "Config/StaticConfig/Tips.csv", LOAD_MODE.BYTES_RES);
#endif
        }
        DataCenter.mTips = TableManager.GetTable("Tips");
        DataCenter.mStaticConfig = TableManager.GetTable("StaticConfig");
    }

    /// <summary>
    /// 加载本地版本号
    /// </summary>
    /// <param name="useSDKChannel">是否用SDK渠道</param>
    public static bool LoadLocalClientVersion(bool useSDKChannel)
    {
        //首先检测最新客户端版本号
        CheckNewestVersion();

        string tmpStrApkVersionFileObj = "";
        string tmpStrSDVersionFileObj = "";

        GetClientVersion(out tmpStrApkVersionFileObj, out tmpStrSDVersionFileObj);
        if (tmpStrApkVersionFileObj != "")
        {
            string tmpStrApkVersion = "";
            string tmpStrSDVersion = "";

            ms_ApkClientVersion = JCode.Decode<ClientVersion>(tmpStrApkVersionFileObj);
            tmpStrApkVersion = ms_ApkClientVersion.version;

            if (tmpStrSDVersionFileObj != "")
            {
                ClientVersion tmpSDClientVersion = JCode.Decode<ClientVersion>(tmpStrSDVersionFileObj);
                tmpStrSDVersion = tmpSDClientVersion.version;
            }

            string tmpStrOldClientVersion = tmpStrSDVersionFileObj;
            if (tmpStrSDVersion != tmpStrApkVersion)
            {
                //取最大值作为客户端版本号
                GameVersion tmpApkGameVersion = new GameVersion(tmpStrApkVersion);
                GameVersion tmpSDGameVersion = new GameVersion(tmpStrSDVersion);
                List<GAME_VERSION_FIELD_DIFF> tmpListDiff = GameVersion.GetFullDifferentInfo(tmpApkGameVersion, tmpSDGameVersion);
                if (tmpApkGameVersion > tmpSDGameVersion || tmpListDiff[(int)GAME_VERSION_FIELD.PATCH] == GAME_VERSION_FIELD_DIFF.HIGH)
                    tmpStrOldClientVersion = tmpStrApkVersionFileObj;
            }
            ms_OldClientVersion = JCode.Decode<ClientVersion>(tmpStrOldClientVersion);

#if !UNITY_EDITOR && !NO_USE_SDK
            if (useSDKChannel)
            {
                //将SDK中渠道号赋值给当前渠道号
                string tmpChannel = "";
                try
                {
                    tmpChannel = U3DSharkSDK.Instance.GetPlatformData().GetData(U3DSharkAttName.CHANNEL_ID);
                }
                catch (System.Exception ex)
                {
                }
                if (tmpChannel != "")
                    ms_OldClientVersion.channel = tmpChannel;
            }
#endif

            ServerVersion = new ClientVersion()
            {
                version = ms_OldClientVersion.version,
                channel = ms_OldClientVersion.channel
            };
            LastestClientVersion = new ClientVersion()
            {
                version = ms_OldClientVersion.version,
                channel = ms_OldClientVersion.channel
            };
        }

        HotUpdateLog.Log("Apk version = " + ms_ApkClientVersion.version + ", channel = " + ms_ApkClientVersion.channel);
        return true;
    }
    /// <summary>
    /// 更新前游戏版本号
    /// </summary>
    public static ClientVersion OldClientVersion
    {
        get { return ms_OldClientVersion; }
    }
    /// <summary>
    /// 最新客户端版本号
    /// </summary>
    public static ClientVersion LastestClientVersion
    {
        set
        {
            ms_LastClientVersion = value;
            CommonParam.RealClientVer = ms_LastClientVersion.version;
        }
        get { return ms_LastClientVersion; }
    }
    public static ClientVersion ServerVersion
    {
        set
        {
            ms_ServerVersion = value;
            CommonParam.ClientVer = ms_ServerVersion.version;
        }
        get { return ms_ServerVersion; }
    }

    /// <summary>
    /// 获取客户端版本号
    /// </summary>
    /// <param name="apkClientVersion">包内版本号</param>
    /// <param name="sdClientVersion">包外版本号</param>
    public static void GetClientVersion(out string apkClientVersion, out string sdClientVersion)
    {
        apkClientVersion = "";
        sdClientVersion = "";

        string tmpStrApkVersionFilePath = "Version" + VersionConfigUpdate.RelativeVersionPath;
        string tmpStrSDVersionFilePath = GameCommon.DynamicAbsoluteGameDataPath + HotUpdatePath.RelativeVersion + VersionConfigUpdate.RelativeVersionPath;

        //处理Apk版本
        //去后缀
        tmpStrApkVersionFilePath = tmpStrApkVersionFilePath.Remove(tmpStrApkVersionFilePath.LastIndexOf("."));
        //加载数据
        UnityEngine.Object tmpApkVersionFileObj = Resources.Load<TextAsset>(tmpStrApkVersionFilePath);
        TextAsset tmpTAApkVersionObj = tmpApkVersionFileObj as TextAsset;
        if (tmpTAApkVersionObj != null)
            apkClientVersion = Encoding.UTF8.GetString(tmpTAApkVersionObj.bytes);

        //处理SD版本
        if (File.Exists(tmpStrSDVersionFilePath))
        {
            FileStream tmpFS = new FileStream(tmpStrSDVersionFilePath, FileMode.Open);
            byte[] tmpFileData = new byte[tmpFS.Length];
            tmpFS.Read(tmpFileData, 0, (int)tmpFS.Length);
            tmpFS.Close();
            sdClientVersion = Encoding.UTF8.GetString(tmpFileData);
        }
    }
    /// <summary>
    /// 检查并设置最新客户端版本号（包内、包外）
    /// </summary>
    public static void CheckNewestVersion()
    {
        string tmpStrApkVersionFileObj = "";
        string tmpStrSDVersionFileObj = "";

        GetClientVersion(out tmpStrApkVersionFileObj, out tmpStrSDVersionFileObj);
        if (tmpStrApkVersionFileObj != "")
        {
            string tmpStrApkVersion = "";
            string tmpStrSDVersion = "";

            ClientVersion tmpApkClientVersion = JCode.Decode<ClientVersion>(tmpStrApkVersionFileObj);
            tmpStrApkVersion = tmpApkClientVersion.version;

            if (tmpStrSDVersionFileObj != "")
            {
                ClientVersion tmpSDClientVersion = JCode.Decode<ClientVersion>(tmpStrSDVersionFileObj);
                tmpStrSDVersion = tmpSDClientVersion.version;
            }

            //如果SD卡里有版本，检查和apk里比哪个版本高
            GameVersion tmpApkGameVersion = new GameVersion(tmpStrApkVersion);
            GameVersion tmpSDGameVersion = new GameVersion(tmpStrSDVersion);
            List<GAME_VERSION_FIELD_DIFF> tmpListDiff = GameVersion.GetFullDifferentInfo(tmpApkGameVersion, tmpSDGameVersion);
            if (tmpApkGameVersion > tmpSDGameVersion || tmpListDiff[(int)GAME_VERSION_FIELD.PATCH] == GAME_VERSION_FIELD_DIFF.HIGH)
            {
                //包内版本高，删除包外配置表列表文件
                string tmpSDConfigListFilePath = GameCommon.DynamicAbsoluteGameDataPath + HotUpdatePath.RelativeConfig + ConfigUpdate.ConfigListRelativePath;
                GameCommon.DeleteFile(tmpSDConfigListFilePath);
            }
        }
    }

    /// <summary>
    /// 初始化乐变数据
    /// </summary>
    /// <param name="retCallback"></param>
    private void __InitLeBianData(Action<bool> retCallback)
    {
        HotUpdateLog.Log("InitLeBianData");
        m_LeBianWaitDelta = 0.0f;
        ms_LeBianUpdateState = -2;
        if (retCallback != null)
            retCallback(true);
    }
    /// <summary>
    /// 询问乐变是否更新
    /// </summary>
    private void __QueryLeBianUpdate(Action<bool> retCallback)
    {
#if !UNITY_EDITOR && !NO_USE_SDK
        HotUpdateLog.Log("QueryLeBianUpdate");
        U3DSharkBaseData tmpBaseData = new U3DSharkBaseData();
        U3DSharkSDK.Instance.DoAnyFunction("LBQueryUpdate", tmpBaseData);
        if (retCallback != null)
            retCallback(true);
#else
        HotUpdateLog.Log("QueryLeBianUpdate - Skip LeBian");
        //跳过乐变
        m_HotUpdateSyncQueue.SkipAll(true);
#endif
    }
    /// <summary>
    /// 检查乐变是否更新
    /// </summary>
    /// <param name="retCallback"></param>
    private IEnumerator __CheckLeBianUpdate(Action<bool> retCallback)
    {
        HotUpdateLog.Log("Start CheckLeBianUpdate");
        while (ms_LeBianUpdateState == -2)
        {
            m_LeBianWaitDelta += Time.deltaTime;
            if (m_LeBianWaitDelta >= mc_LeBianMaxWaitTime)
            {
                m_HotUpdateSyncQueue.SkipAll(true);
                yield break;
            }
            yield return null;
        }
        HotUpdateLog.Log("CheckLeBianUpdate - state = " + ms_LeBianUpdateState);
        if (ms_LeBianUpdateState == 3 || ms_LeBianUpdateState == 4)
        {
            //乐变更新
            while (true)
                yield return null;
        }
        else
        {
            //乐变不更新
            if (retCallback != null)
                retCallback(true);
        }
    }

    /// <summary>
    /// 符合要求的GroupItem
    /// </summary>
    public static VersionConfigGroupItem SelectedGroupItem
    {
        set { ms_SelectedGroupItem = value; }
        get { return ms_SelectedGroupItem; }
    }
    /// <summary>
    /// 选择登录IP、Port
    /// </summary>
    /// <param name="retCallback">参数：是否选择成功</param>
    private IEnumerator __SelectLoginIPPort(Action<bool> retCallback)
    {
        if (ms_SelectedGroupItem == null)
        {
            HotUpdateLog.Log("SelectLoginIPPort error, param hasn't been initialized.");
            if (retCallback != null)
                retCallback(false);
            yield break;
        }
        if (ms_SelectedGroupItem.serverlist.Length <= 0)
        {
            HotUpdateLog.Log("SelectLoginIPPort error, server list is empty.");
            if (retCallback != null)
                retCallback(false);
            yield break;
        }

        string tmpServerIP = "";
        string tmpServerPort = "";
        bool tmpHasValidIPPort = false;
        List<VersionConfigGroupItemServer> tmpServerList = new List<VersionConfigGroupItemServer>();
        for (int i = 0, count = ms_SelectedGroupItem.serverlist.Length; i < count; i++)
            tmpServerList.Add(ms_SelectedGroupItem.serverlist[i]);
        while (tmpServerList.Count > 0)
        {
            int tmpServerIdx = UnityEngine.Random.Range(0, tmpServerList.Count);
            VersionConfigGroupItemServer tmpServer = ms_SelectedGroupItem.serverlist[tmpServerIdx];
            yield return StartCoroutine(__CheckIPPortValid(tmpServer.ip, tmpServer.port, (bool tmpRet) =>
            {
                tmpHasValidIPPort = tmpRet;
            }));
            if (tmpHasValidIPPort)
            {
                tmpServerIP = tmpServer.ip;
                tmpServerPort = tmpServer.port;
                break;
            }
            tmpServerList.RemoveAt(tmpServerIdx);
        }
        if (!tmpHasValidIPPort)
        {
            HotUpdateLog.Log("No valid IP port found.");
            //此时说明无有效IP、port，询问玩家是否再次更新文件
            bool tmpClicked = false;
            OpenRetryMsgBox("服务器正在维护中，请过段时间重试", () =>
            {
                tmpClicked = true;
            });
            while (!tmpClicked)
                yield return null;
            m_HotUpdateSyncQueue.Clear();
            m_LocalDataSyncQueue.Clear();
            //防止界面卡顿
            yield return new WaitForSeconds(0.3f);
            if (retCallback != null)
                retCallback(false);
            __StartUpdate();
            yield break;
        }
        HotUpdateLog.Log("Select IP port complete.");
        CommonParam.LoginIP = tmpServerIP;
        CommonParam.LoginPort = tmpServerPort;
        if (retCallback != null)
            retCallback(true);
    }
    /// <summary>
    /// 检查IP、port有效性
    /// </summary>
    /// <param name="ip"></param>
    /// <param name="port"></param>
    /// <param name="retCallback">完成后回调，参数：IP、port是否有效</param>
    /// <returns></returns>
    private IEnumerator __CheckIPPortValid(string ip, string port, Action<bool> retCallback)
    {
        WWW tmpWWW = new WWW("http://" + ip + ":" + port);
        yield return tmpWWW;
        if (retCallback != null)
        {
            bool tmpValid = false;
            if (tmpWWW.error == "" || tmpWWW.error == null)
                tmpValid = true;
            else if (tmpWWW.error.IndexOf("couldn't connect to host") == -1 && tmpWWW.error.IndexOf("connect failed") == -1)
                tmpValid = true;
            retCallback(tmpValid);
        }
    }

    /// <summary>
    /// 是否热更新
    /// </summary>
    public static bool IsUseHotUpdate
    {
        set { ms_IsUseHotUpdate = value; }
        get { return ms_IsUseHotUpdate; }
    }
    /// <summary>
    /// 是否检查整包更新
    /// </summary>
    public static bool IsCheckApkUpdate
    {
        set { ms_IsCheckApkUpdate = value; }
        get { return ms_IsCheckApkUpdate; }
    }

    /// <summary>
    /// 检查热更新状态
    /// </summary>
    /// <param name="state">状态</param>
    /// <param name="callback">回调函数</param>
    private void __CheckHotUpdateState(HOT_UPDATE_STATE state, object param)
    {
        switch (state)
        {
            case HOT_UPDATE_STATE.FAILED:
                {
                    OpenMiddleOkMsgBox("错误", () =>
                    {
                        ApplicationQuit();
                    });
                } break;
            case HOT_UPDATE_STATE.OK_CANCEL_MESSAGEBOX:
                {
                    HotUpdateCallbackParam tmpCBParam = param as HotUpdateCallbackParam;
                    string tmpStr = (string)tmpCBParam.Param;
                    OpenOkCancelMsgBox(tmpStr,
                        () =>
                        {
                            if (tmpCBParam.Callback != null)
                                tmpCBParam.Callback(1);
                        },
                        () =>
                        {
                            if (tmpCBParam.Callback != null)
                                tmpCBParam.Callback(0);
                        });
                } break;
            case HOT_UPDATE_STATE.OK_MESSAGEBOX:
                {
                    HotUpdateCallbackParam tmpCBParam = param as HotUpdateCallbackParam;
                    string tmpStr = (string)tmpCBParam.Param;
                    OpenMiddleOkMsgBox(tmpStr,
                        () =>
                        {
                            if (tmpCBParam.Callback != null)
                                tmpCBParam.Callback(1);
                        });
                } break;
            case HOT_UPDATE_STATE.NO_INTERNET:
                {
                    HotUpdateCallbackParam tmpCBParam = param as HotUpdateCallbackParam;
                    OpenRetryMsgBox("下载更新失败，请检查网络是否通畅后重试", () =>
                    {
                        if (tmpCBParam != null && tmpCBParam.Callback != null)
                            tmpCBParam.Callback(1);
                    });
                } break;
            case HOT_UPDATE_STATE.NEED_UPDATE_APK:
                {
                    HotUpdateCallbackParam tmpCBParam = param as HotUpdateCallbackParam;
                    string tmpApkURL = tmpCBParam.Param.ToString();
                    OpenOkCancelMsgBox("您的版本过低，需要重新下载游戏安装包，点击确定前往下载游戏最新版本，点击取消继续游戏", () =>
                    {
                        StartCoroutine(__UpdateNewApk(tmpApkURL));
                    },
                    () =>
                    {
                        if (tmpCBParam.Callback != null)
                            tmpCBParam.Callback(0);
                    });
                } break;
            case HOT_UPDATE_STATE.MUST_UPDATE_APK:
                {
                    HotUpdateCallbackParam tmpCBParam = param as HotUpdateCallbackParam;
                    string tmpApkURL = tmpCBParam.Param.ToString();
                    OpenOkCancelMsgBox("您的版本过低，需要重新下载游戏安装包，点击确定前往下载游戏最新版本，点击取消退出游戏", () =>
                    {
                        StartCoroutine(__UpdateNewApk(tmpApkURL));
                    },
                    () =>
                    {
                        ApplicationQuit();
                    });
                } break;
            case HOT_UPDATE_STATE.NO_MATCH_GROUP_ID:
                {
                    OpenMiddleOkMsgBox("您当前版本过低，需要重新下载游戏安装包，点击确定退出游戏", () =>
                    {
                        ApplicationQuit();
                    });
                } break;
            case HOT_UPDATE_STATE.REQUEST_LOAD_DATAPACKAGE:
                {
                    HotUpdateCallbackParam tmpCBParam = param as HotUpdateCallbackParam;
                    float tmpTotalDownload = (float)tmpCBParam.Param;
                    string tmpTips = string.Format(
                        "您现在的网络状态没有在WIFI下哦，更新版本需要下载{0}M的资源包，点击确定后开始下载，点击取消退出游戏，稍后再来",
                        tmpTotalDownload.ToString("0.00"));
                    OpenOkCancelMsgBox(tmpTips,
                        () =>
                        {
                            if (tmpCBParam != null && tmpCBParam.Callback != null)
                                tmpCBParam.Callback(1);
                        },
                        () =>
                        {
                            if (tmpCBParam != null && tmpCBParam.Callback != null)
                                tmpCBParam.Callback(0);
                        });
                } break;
            case HOT_UPDATE_STATE.LOAD_DATAPACKAGE:
                {
                    Action callback = (Action)param;
                    if (m_HotUpdateSyncQueue != null)
                        m_HotUpdateSyncQueue.Next();
                    if (callback != null)
                        callback();
                    StartCoroutine(StartLoad("", null, null));
                    //BI热更新
                    m_Stopwatch.Reset();
                    m_Stopwatch.Start();
                } break;
            case HOT_UPDATE_STATE.NO_LOAD_DATAPACKGE:
                {
                    Action callback = (Action)param;
                    if (m_HotUpdateSyncQueue != null)
                        m_HotUpdateSyncQueue.Clear();
                    if (m_LocalDataSyncQueue != null)
                        m_LocalDataSyncQueue.Next();
                    if (callback != null)
                        callback();
                } break;
            case HOT_UPDATE_STATE.NEED_RESTART_HOTUPDATE:
                {
                    HotUpdateCallbackParam tmpCBParam = param as HotUpdateCallbackParam;
                    OpenMiddleOkMsgBox("更新失败，点击确定重新检查更新", () =>
                    {
                        if (tmpCBParam != null && tmpCBParam.Callback != null)
                            tmpCBParam.Callback(0);
                        ReloadHotUpdateScene();
                    });
                }break;
            case HOT_UPDATE_STATE.NO_ENOUGH_MEMORY:
                {
                    OpenMiddleOkMsgBox("您的设备内存不足，无法进行游戏，点击确定退出游戏", () =>
                    {
                        ApplicationQuit();
                    });
                } break;
            case HOT_UPDATE_STATE.INVALID_RESOLUTION:
                {
                    OpenMiddleOkMsgBox("您的设备分辨率不适合进行游戏，点击确定退出游戏", () =>
                    {
                        ApplicationQuit();
                    });
                } break;
        }
    }

    /// <summary>
    /// 确认、取消按钮
    /// </summary>
    /// <param name="content"></param>
    /// <param name="okCallback"></param>
    /// <param name="cancelCallback"></param>
    public static void OpenOkCancelMsgBox(string content, Action okCallback, Action cancelCallback)
    {
        HotUpdateMessageBox.ShowMessageBox(HOTUPDATE_MESSAGEBOX_TYPE.OK_CANCEL, content);
        HotUpdateMessageBox.SetOkCancelCallback(okCallback, cancelCallback);
    }
    /// <summary>
    /// 确认按钮在中间
    /// </summary>
    public static void OpenMiddleOkMsgBox(string content, Action callback)
    {
        HotUpdateMessageBox.ShowMessageBox(HOTUPDATE_MESSAGEBOX_TYPE.OK, content);
        HotUpdateMessageBox.SetMiddleOkCallback(callback);
    }
    /// <summary>
    /// 重试按钮
    /// </summary>
    /// <param name="content"></param>
    /// <param name="callbck"></param>
    public static void OpenRetryMsgBox(string content, Action callbck)
    {
        HotUpdateMessageBox.ShowMessageBox(HOTUPDATE_MESSAGEBOX_TYPE.RETRY, content);
        HotUpdateMessageBox.SetRetryCallback(callbck);
    }

    /// <summary>
    /// 初始化数据
    /// </summary>
    /// <param name="complete"></param>
    private void __ActionInit(Action<bool> complete)
    {
        HotUpdateLog.Log("ActionInit");
        //当前版本
        if (!LoadLocalClientVersion(true))
        {
            m_HotUpdateSyncQueue.Clear();
            m_LocalDataSyncQueue.Clear();

            return;
        }
        HotUpdateLog.Log("Current version = " + ms_OldClientVersion.version + ", channel = " + ms_OldClientVersion.channel);

        //更新地址
//         DataRecord tmpRecord = DataCenter.mStaticConfig.GetRecord("HOT_UPDATE_IP");
//         string tmpHotUpdateIP = tmpRecord.getObject("VALUE").ToString();
//         tmpRecord = DataCenter.mStaticConfig.GetRecord("HOT_UPDATE_PORT");
//         string tmpHotUpdatePort = tmpRecord.getObject("VALUE").ToString();
//         m_HotUpdateParam.Address = "http://" + tmpHotUpdateIP;
//         if (tmpHotUpdatePort != "-1")
//             m_HotUpdateParam.Address += (":" + tmpHotUpdatePort);
        DataRecord tmpRecord = DataCenter.mStaticConfig.GetRecord("HOT_UPDATE_IP");
        string tmpStrHotUpdateIPList = tmpRecord.getObject("VALUE").ToString();
        string[] tmpHotUpdateIPList = tmpStrHotUpdateIPList.Split(new char[] { '#' });
        m_HotUpdateParam.ListAddress = new List<string>();
        for (int i = 0, count = tmpHotUpdateIPList.Length; i < count; i++)
            m_HotUpdateParam.ListAddress.Add("http://" + tmpHotUpdateIPList[i]);

        tmpRecord = DataCenter.mStaticConfig.GetRecord("HOT_UPDATE_PORT");
        string tmpStrHotUpdatePortList = tmpRecord.getObject("VALUE").ToString();
        string[] tmpHotUpdatePortList = tmpStrHotUpdatePortList.Split(new char[] { '#' });
        string tmpHotUpdateAddress = "";
        for (int i = 0, count = tmpHotUpdateIPList.Length; i < count; i++)
        {
            if (tmpHotUpdatePortList[i] != "-1")
                m_HotUpdateParam.ListAddress[i] += (":" + tmpHotUpdatePortList[i]);
            if (i != 0)
                tmpHotUpdateAddress += ",";
            tmpHotUpdateAddress += (m_HotUpdateParam.ListAddress[i]);
        }
        HotUpdateLog.Log("HotUpdateAddress - " + tmpHotUpdateAddress);

        if (complete != null)
            complete(true);
    }

    /// <summary>
    /// 初始化界面
    /// </summary>
    private void __InitUI()
    {
        if (m_GOLoadPhase != null)
            m_GOLoadPhase.gameObject.SetActive(false);

        if (m_ProgressSlider != null)
            m_ProgressSlider.gameObject.SetActive(false);
    }
    /// <summary>
    /// 更改当前加载阶段
    /// </summary>
    /// <param name="phase"></param>
    /// <param name="complete"></param>
    private void __ActionChangePhase(HOTUPDATE_LOAD_PHASE phase, Action<bool> complete)
    {
        //        __InitUI();
        switch (phase)
        {
            case HOTUPDATE_LOAD_PHASE.CHECK_LEBIAN:
                {
                    if (m_ProgressSlider != null)
                        m_ProgressSlider.gameObject.SetActive(false);

                    if (m_GOLoadPhase != null)
                        m_GOLoadPhase.gameObject.SetActive(true);
                    if (m_LBLoadPhaseInfo != null)
                        m_LBLoadPhaseInfo.text = "正在检查更新信息";
                }break;
            case HOTUPDATE_LOAD_PHASE.CHECK_UPDATE:
                {
                    if (m_ProgressSlider != null)
                        m_ProgressSlider.gameObject.SetActive(false);

                    if (m_GOLoadPhase != null)
                        m_GOLoadPhase.gameObject.SetActive(true);
                    if (m_LBLoadPhaseInfo != null)
                        m_LBLoadPhaseInfo.text = "正在检查更新信息...";
                } break;
            case HOTUPDATE_LOAD_PHASE.HOT_UPDATE:
                {
                    if (m_GOLoadPhase != null)
                        m_GOLoadPhase.gameObject.SetActive(false);

                    if (m_ProgressSlider != null)
                    {
                        m_ProgressSlider.gameObject.SetActive(true);
                        m_ProgressSlider.value = 1.0f;
                        m_LBProgress.text = "";
                        m_LoadExtraProgessInfo = "正在更新客户端 0.00/0.00M";
                        m_LoadExtraSpeedInfo = "（0.00M/s）";
                    }
                } break;
            case HOTUPDATE_LOAD_PHASE.INIT_LOCAL_DATA:
                {
                    if (m_ProgressSlider != null)
                        m_ProgressSlider.gameObject.SetActive(false);

                    if (m_GOLoadPhase != null)
                        m_GOLoadPhase.gameObject.SetActive(true);
                    if (m_LBLoadPhaseInfo != null)
                        m_LBLoadPhaseInfo.text = "初始化本地数据";
                } break;
            case HOTUPDATE_LOAD_PHASE.INIT_LOCAL_DATA_COMPLETE:
                {
                    if (m_ProgressSlider != null)
                        m_ProgressSlider.gameObject.SetActive(false);

                    if (m_GOLoadPhase != null)
                        m_GOLoadPhase.gameObject.SetActive(true);
                    if (m_LBLoadPhaseInfo != null)
                        m_LBLoadPhaseInfo.text = "[99FF66]本地数据初始化完成";
                } break;
        }
        //防止画面停留时间过短
        _DelayAction(0.3f, () =>
        {
            if (complete != null)
                complete(true);
        });
    }

    /// <summary>
    /// 下载group.txt、groupitem.txt
    /// </summary>
    private void __ActionLoadGroup(Action<bool> complete)
    {
        StartCoroutine(__VersionConfigUpdate(() =>
        {
//             if(CommonParam.isUseSDK)
//             {
//                 //初始化SDK
//                 try
//                 {
//                     U3DSharkSDK.Instance.InitSDK();
//                 }
//                 catch (System.Exception ex)
//                 {
//                 }
//             }

            if (complete != null)
                complete(true);
        }));
    }
    private IEnumerator __VersionConfigUpdate(Action complete)
    {
        VersionConfigUpdate tmpVersionConfigUpdate = new VersionConfigUpdate();
        yield return StartCoroutine(tmpVersionConfigUpdate.StartLoad("", null, m_HotUpdateParam));

        bool tmpSelSuccess = false;
        yield return StartCoroutine(__SelectLoginIPPort((bool selSuccess) =>
        {
            tmpSelSuccess = selSuccess;
        }));
        if (!tmpSelSuccess)
            yield break;

        if (complete != null)
            complete();
    }

    /// <summary>
    /// 检查运行游戏要求
    /// </summary>
    private void __ActionCheckRunRequirement(Action<bool> complete)
    {
        //检查内存
        long tmpMemory = __GetMachineMemory();
        long tmpLimitMemory = long.Parse(ms_SelectedGroupItem.limitmemory);
        HotUpdateLog.Log("NowMemory = " + tmpMemory + ", totalMemory = " + SystemInfo.systemMemorySize);
        if (tmpLimitMemory > tmpMemory)
        {
            //内存不足
            __CheckHotUpdateState(HOT_UPDATE_STATE.NO_ENOUGH_MEMORY, null);
            m_HotUpdateSyncQueue.Clear();
        }

        //检查分辨率
        if (m_minResolution != null && m_minResolution.isCheck)
        {
            Vector2 tmpCurr = new Vector2(Screen.width, Screen.height);
            if ((m_minResolution.Min != Vector2.zero && ConfigToggleCheckResolution.IsBigger(m_minResolution.Min, tmpCurr)) ||
                (m_minResolution.Max != Vector2.zero && ConfigToggleCheckResolution.IsBigger(tmpCurr, m_minResolution.Max)))
            {
                //分辨率不符合要求，过低或过高
                __CheckHotUpdateState(HOT_UPDATE_STATE.INVALID_RESOLUTION, null);
                m_HotUpdateSyncQueue.Clear();
            }
        }

        if (complete != null)
            complete(true);
    }
    /// <summary>
    /// M为单位
    /// </summary>
    /// <returns></returns>
    private long __GetMachineMemory()
    {
#if false&&!UNITY_EDITOR && (UNITY_ANDROID || UNITY_IOS)
        try
        {
            string deviceInfo = U3DSharkSDK.Instance.DoPhoneInfo();
            U3DSharkBaseData deviceData = new U3DSharkBaseData();
            deviceData.StringToData(deviceInfo);
            return long.Parse(deviceData.GetData(U3DSharkAttName.MEMOMRY_TOTAL_MB));
        }
        catch(Exception exp)
        {
            return SystemInfo.systemMemorySize;
        }
#else
        return SystemInfo.systemMemorySize;
#endif
    }

    /// <summary>
    /// 检查热更新
    /// </summary>
    private void __ActionCheckHotUpdate(Action<bool> complete)
    {
        StartCoroutine(__DoCheckHotUpdate(complete));
    }
    private IEnumerator __DoCheckHotUpdate(Action<bool> complete)
    {
        HotUpdateLog.TempLog("Start check hot update.");
        //ms_OldClientVersion的version为当前最新的渠道号
        if (ms_ApkClientVersion.version == ms_LastClientVersion.version && ms_OldClientVersion.channel == ms_LastClientVersion.channel)
        {
            HotUpdateLog.TempLog("Use apk data.");
#if !UNITY_EDITOR && (UNITY_ANDROID || UNITY_IOS)
            //使用包内数据
            HotUpdateLoading.IsUseApkData = true;
            HotUpdateLog.TempLog("Use apk data inner.");
#endif
            //跳过热更新
            m_HotUpdateSyncQueue.SkipAll(true);
            HotUpdateLog.TempLog("Use apk data next.");
        }
        else
        {
            GameVersion tmpOldVersion = new GameVersion(ms_OldClientVersion.version);
            GameVersion tmpLastVersion = new GameVersion(ms_LastClientVersion.version);
            List<GAME_VERSION_FIELD_DIFF> tmpListDiff = GameVersion.GetFullDifferentInfo(tmpOldVersion, tmpLastVersion);
            if (tmpListDiff[(int)GAME_VERSION_FIELD.PATCH] != GAME_VERSION_FIELD_DIFF.EQUAL)
            {
                //下载所有的列表数据
                m_FileListLoading = new FileListLoading();
                m_FileListLoading.CoroutineOperation = this;
                HotUpdateLog.TempLog("Start load file list.");
                yield return StartCoroutine(m_FileListLoading.StartLoad("", null, m_HotUpdateParam));

                if (m_FileListLoading.FileTotalSize <= 0)
                {
                    HotUpdateLog.TempLog("Skip hot udpate.");

                    //保存版本号到本地
                    VersionConfigUpdate.SaveLastestVersion(ms_LastClientVersion);

                    //跳过热更新
                    m_HotUpdateSyncQueue.SkipAll(true);
                }
                else
                {
                    HotUpdateLog.TempLog("Hot udpate.");
                    if (complete != null)
                        complete(true);
                }
            }
            else
            {
                HotUpdateLog.TempLog("Skip hot udpate with equal version.");

                //保存版本号到本地
                VersionConfigUpdate.SaveLastestVersion(ms_LastClientVersion);

                //跳过热更新
                m_HotUpdateSyncQueue.SkipAll(true);
            }
        }
    }

    /// <summary>
    /// 热更新
    /// </summary>
    private void __ActionHotUpdate(Action<bool> complete)
    {
        //BI热更新
        m_Stopwatch.Reset();
        m_Stopwatch.Start();
        StartCoroutine(StartLoad("", null, null));
        StartCoroutine(__DoHotUpdate(complete));
    }
    private IEnumerator __DoHotUpdate(Action<bool> complete)
    {
        int tmpFileTotalSize = m_FileListLoading.FileTotalSize;
        int tmpBytesLoaded = 0;
        FileListData tmpFileListData = null;
        LoadingProgressParam tmpCurrProgress = new LoadingProgressParam()
        {
            BytesTotal = tmpFileTotalSize
        };

        //配表更新
        tmpFileListData = m_FileListLoading.GetFileListData(HotUpdatePath.RelativeConfig + ConfigUpdate.ConfigListRelativePath);
        m_HotUpdateParam.Param = tmpFileListData;
        ConfigUpdate tmpConfigUpdate = new ConfigUpdate();
        tmpConfigUpdate.CoroutineOperation = this;
        yield return StartCoroutine(tmpConfigUpdate.StartLoad("", (LoadingProgressParam progress) =>
        {
            //            HotUpdateLog.Log("Progress = loaded : " + progress.BytesLoaded + ", progress : " + progress.Progress);
            long tmpCurrBytesLoaded = tmpBytesLoaded + progress.BytesLoaded;
            m_LoadDataSize += (tmpCurrBytesLoaded - tmpCurrProgress.BytesLoaded);
            tmpCurrProgress.BytesLoaded = tmpCurrBytesLoaded;
            tmpCurrProgress.Progress = (float)tmpCurrProgress.BytesLoaded / (float)tmpCurrProgress.BytesTotal;
            SetLoadingProgress(tmpCurrProgress);
        }, m_HotUpdateParam));
        tmpBytesLoaded += tmpFileListData.Data.Length;

        //保存版本号到本地
        VersionConfigUpdate.SaveLastestVersion(ms_LastClientVersion);

        if (complete != null)
            complete(true);
    }
    /// <summary>
    /// 更新下载速度
    /// </summary>
    private void __UpdateLoadSpeed()
    {
        if (!m_IsLoading)
            return;

        m_LoadSpeedTime += Time.fixedDeltaTime;

        if (m_LoadSpeedTime > 0.0f && m_LoadSpeedTime >= m_LoadSpeedResetTime)
        {
            m_LoadSpeed = LoadingHelper.Instance.ConvertBToMB((float)m_LoadDataSize) / m_LoadSpeedTime;
            m_LoadSpeedTime = 0.0f;
            m_LoadDataSize = 0;

            m_LoadExtraSpeedInfo = "";
            m_LoadExtraSpeedInfo += "（";
            m_LoadExtraSpeedInfo += m_LoadSpeed.ToString("0.00") + "M/s";
            m_LoadExtraSpeedInfo += "）";
        }
    }

    /// <summary>
    /// 更新下载显示信息
    /// </summary>
    private void __UpdateLoadExtraInfo()
    {
        if (m_LBProgressExtraInfo == null)
            return;

        m_LBProgressExtraInfo.text = (
            m_LoadExtraProgessInfo + m_LoadExtraSpeedInfo);
    }

    /// <summary>
    /// 初始化本地数据
    /// </summary>
    /// <param name="complete"></param>
    private void __ActionInitLocalData(Action<bool> complete)
    {
        HotUpdateLog.TempLog("Init local data.");

        //开始处理资源加载路径
        __ConfigLocalLoadHandle();

        if (complete != null)
            complete(true);
    }
    /// <summary>
    /// 处理配表加载路径
    /// </summary>
    private void __ConfigLocalLoadHandle()
    {
#if UNITY_EDITOR
        //不加入配表
        return;
#endif

        string tmpInnerPath = HotUpdatePath.RelativeConfig + ConfigUpdate.ConfigListRelativePath;
        string tmpExternalPath = GameCommon.DynamicAbsoluteGameDataPath + tmpInnerPath;
        ResourceList tmpInner = ResourceList.GetInnerResourceList(tmpInnerPath);
        ResourceList tmpExternal = ResourceList.GetResourceList(tmpExternalPath);
        if (tmpInner != null)
        {
            foreach (KeyValuePair<string, ResourceListIDData> tmpPair in tmpInner.GetDicFileData())
            {
                string tmpPath = HotUpdatePath.RelativeConfig + tmpPair.Key;
                while (tmpPath.IndexOf("/") == 0)
                    tmpPath = tmpPath.Substring(1);
                //去后缀
                int tmpIdx = tmpPath.LastIndexOf(".");
                if (tmpIdx >= 0)
                    tmpPath = tmpPath.Remove(tmpIdx);
                ResourceLocalLoadInfoManager.Instance.AddLoadInfo(tmpPath, true, true);
            }
        }
        if (tmpExternal != null)
        {
            foreach (KeyValuePair<string, ResourceListIDData> tmpPair in tmpExternal.GetDicFileData())
            {
                string tmpPath = HotUpdatePath.RelativeConfig + tmpPair.Key;
                while (tmpPath.IndexOf("/") == 0)
                    tmpPath = tmpPath.Substring(1);
                //去后缀
                int tmpIdx = tmpPath.LastIndexOf(".");
                if (tmpIdx >= 0)
                    tmpPath = tmpPath.Remove(tmpIdx);
                ResourceLocalLoadInfoManager.Instance.AddLoadInfo(tmpPath, false, true);
            }
        }

        //TODO
    }

    /// <summary>
    /// 加载游戏场景
    /// </summary>
    private void __ActionLoadGameScene(Action<bool> complete)
    {
        StartCoroutine(__LoadGameScene(() =>
        {
            if (complete != null)
                complete(true);
        }));
    }
    private IEnumerator __LoadGameScene(Action callback)
    {
        AsyncOperation tmpAsyncOP = Application.LoadLevelAsync("StartGame");
        yield return tmpAsyncOP;
        if (callback != null)
            callback();
    }

    private void __SetProgressUI(LoadingProgressParam progress)
    {
        if (m_LBProgress != null)
        {
            string tmpStrInfo = (Mathf.FloorToInt(progress.Progress * 100.0f) + "%");
            m_LBProgress.text = tmpStrInfo;
        }

        float tmpBytesLoadedM = Mathf.Max(LoadingHelper.Instance.ConvertBToMB((float)progress.BytesLoaded), 0.01f);
        float tmpBytesTotalM = Mathf.Max(LoadingHelper.Instance.ConvertBToMB((float)progress.BytesTotal), 0.01f);

        m_LoadExtraProgessInfo = "";
        m_LoadExtraProgessInfo += ("正在更新客户端 ");
        m_LoadExtraProgessInfo += (tmpBytesLoadedM.ToString("0.00"));
        m_LoadExtraProgessInfo += ("/");
        m_LoadExtraProgessInfo += (tmpBytesTotalM.ToString("0.00"));
        m_LoadExtraProgessInfo += "M";
    }

    /// <summary>
    /// 设置进度条值
    /// </summary>
    /// <param name="progress"></param>
    protected override void _SetProgressUIValue(LoadingProgressParam progress)
    {
        if (m_ProgressSlider == null)
            return;
        m_ProgressSlider.value = 1.0f - progress.Progress;
    }
    protected override void _OnProgressChanged(LoadingProgressParam delta)
    {
        __SetProgressUI(m_RealProgress);
    }
    protected override void _OnLoadFinished()
    {
        //BI热更新结束
        m_Stopwatch.Stop();
        BIHelper.SendAppLoadStep(LoadStepType.TableUpdateFinished);
        BIHelper.SendUpdate(HotUpdateLoading.OldClientVersion.version, HotUpdateLoading.LastestClientVersion.version, (int)(m_Stopwatch.ElapsedMilliseconds / 1000));
        _DelayAction(0.3f, () =>
        {
            if (m_HotUpdateSyncQueue != null)
                m_HotUpdateSyncQueue.Next();
        });
    }

    /// <summary>
    /// 等待相应动作完成
    /// </summary>
    /// <param name="state"></param>
    /// <param name="continueDelay">继续逻辑的延迟时间</param>
    /// <param name="cbNeedBreak">需要中断回调</param>
    /// <returns></returns>
    public static IEnumerator PauseForAction(HotUpdateParam hotUpdateParam, HOT_UPDATE_STATE state, float continueDelay, Action cbNeedBreak)
    {
        return PauseForAction(hotUpdateParam, state, null, continueDelay, cbNeedBreak);
    }
    /// <summary>
    /// 等待相应动作完成
    /// </summary>
    /// <param name="state"></param>
    /// <param name="param"></param>
    /// <param name="continueDelay">继续逻辑的延迟时间</param>
    /// <param name="cbNeedBreak">需要中断回调</param>
    /// <returns></returns>
    public static IEnumerator PauseForAction(HotUpdateParam hotUpdateParam, HOT_UPDATE_STATE state, object param, float continueDelay, Action cbNeedBreak)
    {
        int tmpActionRet = -1;
        if (hotUpdateParam == null || hotUpdateParam.StateActionHandle == null)
        {
            if (cbNeedBreak != null)
                cbNeedBreak();
            yield break;
        }
        else
        {
            HotUpdateCallbackParam tmpCBParam = new HotUpdateCallbackParam()
            {
                Param = param,
                Callback = (object actionRet) =>
                {
                    tmpActionRet = (int)actionRet;
                }
            };
            hotUpdateParam.StateActionHandle(state, tmpCBParam);
        }
        while (true)
        {
            if (tmpActionRet == 1)
            {
                //让程序不卡顿
                yield return new WaitForSeconds(continueDelay);
                break;
            }
            else if (tmpActionRet == 0)
            {
                if (cbNeedBreak != null)
                    cbNeedBreak();
                yield break;
            }
            yield return null;
        }
    }

    /// <summary>
    /// 更新最新的Apk
    /// </summary>
    private IEnumerator __UpdateNewApk(string apkURL)
    {
        if (apkURL == null || apkURL == "")
        {
            //如果更新地址不存在，提示不能更新，请稍后重试
            HotUpdateCallbackParam tmpCBParam = new HotUpdateCallbackParam();
            tmpCBParam.Param = "更新文件失败，请重启游戏后重试，点击确定退出游戏";
            tmpCBParam.Callback = (object param) =>
            {
                ApplicationQuit();
            };
            __CheckHotUpdateState(HOT_UPDATE_STATE.OK_MESSAGEBOX, tmpCBParam);
            while (true)
                yield return null;
        }
        Application.OpenURL(apkURL);
        ApplicationQuit();
    }
}
