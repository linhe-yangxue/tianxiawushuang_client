
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.IO;
using DataTable;
using Logic;
using Utilities;
//--------------------------------------------------------------------------------------------------

public class CommonParam
{
#if UNITY_IPHONE || UNITY_ANDROID || DEBUG_IO
    static public bool RunOnWindow = false;
#else
    static public bool RunOnWindow = true;
#endif

#if UNITY_EDITOR
    static public string ResourceTheater = "default";
#elif UNITY_IPHONE
    static public string ResourceTheater = "iphdefault";
#elif UNITY_ANDROID
    static public string ResourceTheater = "anddefault";
#else 
    static public string ResourceTheater = "default";
#endif


    static public string UIRootName = "UI Root";
    //static public bool IsSaveByte = false;
    static public bool UseUpdate = true;
    static public bool NeedLog = true;  // read from GlobalConfig.csv > LOG YES or NO
    static public bool LogOnScreen = false; // read from GlobalConfig.csv > LOG_ON_SCREEN YES or NO
    static public bool LogOnConsole = false; // read from GlobalConfig.csv > LOG_ON_CONSOLE YES or NO
    static public bool UseSDK = false;
    static public bool UsePointMark = true; //窗口像素点标识
    static public bool PingLogOnScreen = false;// read from GlobalConfig.csv > PING_LOG_ON_SCREEN YES or NO

    //by chenliang
    //begin

//    static public string ClientVer = "1.01";
//------------
    //为空时未设置客户端版本，此变量仅为发送服务器用
    static public string ClientVer = "";
    //当前客户端真实版本号
    static public string RealClientVer = "";

    //end
    static public int SelfCamp = 1000;
    static public int EnemyCamp = 2000;
	static public int PlayerLayer = 8;
    static public int MonsterLayer = 9;
    static public int OutlineLayer = 30;
	static public int ObstructLayer = 11;
    static public int GroundLayer = 12;
	static public int EffectLayer = 13;
	static public int AbsorptionLayer = 14;
    static public int CharacterLayer = 15;
    static public int UILayer = 20;
    static public int UI_3D = 30;

    static public float DefaultVolume = 1;
    static public float MusicVolume = 0.4f;

    static public float FollowBound = 3.5f;
    static public float LeaveBound = 12.0f;
	static public float BirthBound = 16.0f;
    static public bool StartBirth = false;

	static public float MiniMapMaskSize = 128.0f;
	static public float CreateMiniMapSize = 192.0f;
	static public float MonsterPointTextureSize = 16.0f;
    static public int MonsterCamp = 2000;
    static public int PlayerCamp = 1000;
    static public int PVP_PET_START_POS = 16;
	static public bool bIsNetworkGame = true;

    //static public int battleMainStaminaCost = 5;        // 主线副本体力消耗
    static public int battleActiveStaminaCost = 1;      // 活动副本体力消耗
    static public int stagePerMap = 20;                 // 每张地图的副本数量-能达到的最大数量 --

    static public readonly float navRadiusScale = 1f;           // 寻路组件绝对半径与最终碰撞半径(ImpactRadius)的比例
    static public readonly float shadowRadiusScale = 3f;        // 脚底光环相对半径与基础点选半径(BaseSelectRadius)的比例
    static public readonly float lockRadiusScale = 4f;          // 锁定标记相对半径与基础点选半径(BaseSelectRadius)的比例

    static public float speedX2 = 1.5f;            // 2倍速
    static public float speedX3 = 2f;              // 3倍速
    static public int speedX2OpenLevel = 5;        // 2倍速战斗开启等级
    static public int speedX2OpenVIP = 1;          // 2倍速战斗开启VIP等级
    static public int speedX3OpenLevel = 25;       // 3倍速战斗开启等级
    static public int speedX3OpenVIP = 1;          // 3倍速战斗开启VIP等级
    static public int btnAutoBattleOpenLevel = 2;   // 自动战斗按钮开启等级
    static public int btnSpeedUpOpenLevel = 3;      // 加速按钮开启等级
    static public int btnFollowOpenLevel = 4;       // 跟随按钮开启等级

    static public NiceTable mConfig;


    static public Int64 mServerTime = 0;
    static public Int64 mServerTimeOff = 0;

	static public MAIN_WINDOW_INDEX OpenWindowIndex = MAIN_WINDOW_INDEX.None_Window;

    static public STAGE_TYPE mCurrentLevelType = STAGE_TYPE.MAIN_COMMON;
    static public ELEMENT_TYPE mCurrentLevelElement = ELEMENT_TYPE.GREEN;

    static public string mAccount = "";
    static public string mStrToken = "";
    static public string mUId = "";
    static public string mZoneID = "1";
    static public string LoginIP = "";
    static public string LoginPort = "";

	static public string mOpenDate = "";			//开服时间
	static public string mCreateDate = "";			//创建时间

    //by chenliang
    //begin

    static public string mZoneName = "";            //区名称
    static public int mZoneState = -1;              //区状态
    static public string mStrLoginToken = "";       //登录令牌
    static public bool isUseHttps = false;          //是否在游戏中使用https
    private static bool mIsUseMVSkipTip = true;      //是否开启MV跳过提示

    private static bool mIsVerifyNumberValue = true;     //是否验证游戏里关键数字
    public static bool IsVerifyNumberValue
    {
        get { return mIsVerifyNumberValue; }
    }

    private static string mSDKUserID = "";          //SDK服务器返回的用户ID
    public static string SDKUserID
    {
        set { mSDKUserID = value; }
        get { return mSDKUserID; }
    }
    private static string mSDKToken = "";           //SDK服务器返回的Token
    public static string SDKToken
    {
        set { mSDKToken = value; }
        get { return mSDKToken; }
    }

    /// <summary>
    /// 是否开启MV跳过提示
    /// </summary>
    public static bool IsUseMVSkipTip
    {
        set { mIsUseMVSkipTip = value; }
        get { return mIsUseMVSkipTip; }
    }

    //end
    
    //by lhc
    //begin
    static public bool isUseSDK=false;
    static public bool isCloseBtn=false;
    static public bool isCloseMail=false;
    //end
    static public bool isNeedPushMessage = false;
    // add by LC
    static public bool isTeamManagerInit = false;
    static public bool isDataInit = false;
    // end
    
    //added by xuke 
    static public bool mIsNewAccount = false;   //> 是否是新建账号
    static public bool mOpenDynamicUI = true;   //> 是否打开动态UI
    static public bool mOpenMorrowLand = false; //> 是否开启明日有礼
    
    static public bool mIsRefreshPetInfo = true; //> 当前是否需要刷新符灵属性面板信息
    static public bool mIsRefreshEquipStrengthenInfo = true;    //> 当前是否需要刷新装备强化面板信息
    static public bool mIsRefreshEquipRefineInfo = true;        //> 当前是否需要刷新装备精炼面板信息
    static public bool mIsRefreshMagicStrengthenInfo = true;    //> 当前是否刷新法器强化面板信息
    static public bool mIsRefreshMagicRefineInfo = true;        //> 当前是否刷新法器精炼面板信息
    static public bool mIsRefreshPetUpgradeInfo = true;         //> 当前是否刷新符灵升级面板的信息
    static public bool mIsRefreshPetBreakInfo = true;           //> 当前是否刷新符灵突破面板的信息

    static public int AureoleRenderQueue = 2950;                //角色脚底光环RenderQueue

    //end

    static public bool ShowLoadingTime = false;// 是否需要输出loading起止时间

	static public bool isOnLineVersion;//是否线上版本  
    static public bool isOpenGM;      //是否开启GM超级账号
	static public bool isUserAgree = true;//用户是否同意用户协议 
    static public int chatTextMaxNum;

	static public bool isOpenMoveTips;//是否开启队伍界面的动态飘字 

    public const int packageOverlapLimit = 999999;  // 背包物品叠加上限(可叠加包裹物品：符灵碎片，装备法器碎片，各种进包裹的消耗品)
    public const int goldLimit = 999999999;         // 硬币上限
    public const int diamondLimit = 999999999;      // 元宝上限
    public const int staminaLimit = 9999999;        // 体力上限
    public const int soulPointLimit = 9999999;      // 符魂上限
    public const int spiritLimit = 9999999;         // 精力上限
    public const int reputationLimit = 9999999;     // 声望上限
    public const int prestigeLimit = 9999999;       // 威名上限
    public const int battleAchvLimit = 9999999;     // 战功上限
    public const int unionContrLimit = 9999999;     // 公会贡献上限
    public const int beatDemonCardLimit = 9999999;  // 降魔令上限
    public const int characterLevelLimit = 120;     // 主角等级上限
    public const int expLimit = 999999999;          // 经验上限
    public const int rechageDepth = 55;            //充值界面通用层级
    public const int rechagePosZ = -1000;            //充值界面通用层级
    public const int petDetailPosZ = -1285;            //petdetail需要显示在充值界面上面


	static public int isPveBossFirst = 1;            //首次进入PVE Boss界面,1是的

    static public void InitConfig()
    {
        mConfig = TableManager.GetTable("Global");
        if (mConfig == null)
        {
            Logic.EventCenter.Log(true, "Global config table is not exist");
            return;
        }
        FollowBound = GetConfig("FOLLOW_BOUND");
        LeaveBound = GetConfig("LEAVE_BOUND");
        isOnLineVersion = mConfig.GetData("IS_ONLINE_VERSION", "VALUE") == "YES";
		isOpenMoveTips = mConfig.GetData ("IS_OPEN_MOVE_TIPS", "VALUE") == "YES";
        isOpenGM = mConfig.GetData("GM_SUPER", "VALUE") == "YES";
        chatTextMaxNum = (int)mConfig.GetData("CHAT_TEXT_MAX_NUM", "VALUE");
        //by chenliang
		//begin IS_OPEN_MOVE_TIPS

//         LoginIP = GetConfig("LOGIN_IP");
//         LoginPort = GetConfig("LOGIN_PORT");
//
//        #if !UNITY_EDITOR
//        isUseSDK=(string)GetConfig("IS_USE_SDK")=="YES";
//        #else
//        CommonParam.UseSDK = false;
//        #endif
//-----------------
        //登录IP、port获取方式改为通过groupitem.txt中serverlist获取，这里检测之前是否已设置IP、port值
		Debug.Log ("游戏那日---");
        if (LoginIP == "" || LoginPort == "")
        {
            LoginIP = GetConfig("LOGIN_IP");
            LoginPort = GetConfig("LOGIN_PORT");
        }

        if (!HotUpdateLoading.IsUseHotUpdate)
        {
#if !UNITY_EDITOR
            isUseSDK=(string)GetConfig("IS_USE_SDK")=="YES";
#else
            CommonParam.isUseSDK = false;
#endif
        }

        //end

        //isCloseBtn = (string)GetConfig("IS_CLOSE_BTN") == "YES";
        isCloseMail = (string)GetConfig("IS_CLOSE_MAIL") == "YES";
        isNeedPushMessage = (string)DataCenter.mGlobalConfig.GetData("USE_PUSHMESSAGE", "VALUE") == "YES";

        DEBUG.LogWarning(isUseSDK.ToString());

        ShowLoadingTime = (string)GetConfig("LOADING_TIME") == "YES";
        //by chenliang
        //begin

        //输出登录IP信息
        if ((string)GetConfig("LOG_IP") == "YES")
            Logic.EventCenter.Log(LOG_LEVEL.GENERAL, "Login IP : " + LoginIP + ", port : " + LoginPort);

        //end
        //added by xuke
        CommonParam.mOpenDynamicUI = ((string)GetConfig("IS_DYNAMIC_UI")) == "YES";
        //end
        DEBUG.Log("persistentDataPath : " + Application.persistentDataPath);
    }

    public static Data GetConfig(string key)
    {
        DataRecord re =  mConfig.GetRecord(key);
        if (re!=null)
            return re.getData("VALUE");
		Logic.EventCenter.Log(true, "Global config table is not exist config >"+key);
        return Data.NULL;
    }

    public static void SetServerTime(UInt64 serverTime)
    {
        mServerTime = (Int64)serverTime;
        mServerTimeOff = (Int64)serverTime - (Int64)Time.realtimeSinceStartup;
    }

    static public Int64 NowServerTime()
    {
		return (Int64)Time.realtimeSinceStartup + mServerTimeOff;
    }
}
//by chenliang
//beign
public class DEBUG
{
    static public void Log(string info)
    {
        if (CommonParam.NeedLog)
        {
            Logic.EventCenter.Log(LOG_LEVEL.GENERAL, info);
        }

        if (CommonParam.LogOnScreen)
        {
            ScreenLogger.Log(info, LOG_LEVEL.GENERAL);
        }

        if (CommonParam.LogOnConsole)
        {
            Debug.Log(info);
        }
    }

    static public void LogError(string info)
    {
        logError(info);
    }
    static public void logError(string info)
    {
        if (CommonParam.NeedLog)
        {
            Logic.EventCenter.Log(LOG_LEVEL.ERROR, info);
            Debug.LogError(info);
        }

        if (CommonParam.LogOnScreen)
        {
            ScreenLogger.Log(info, LOG_LEVEL.ERROR);
        }

        if (CommonParam.LogOnConsole)
        {
            Debug.LogError(info);
        }
    }

    static public void LogWarning(string info)
    {
        logWarn(info);
    }
    static public void LogWarn(string info)
    {
        logWarn(info);
    }
    static public void logWarn(string info)
    {
        if (CommonParam.NeedLog)
        {
            Logic.EventCenter.Log(LOG_LEVEL.WARN, info);
        }

        if (CommonParam.LogOnScreen)
        {
            ScreenLogger.Log(info, LOG_LEVEL.WARN);
        }

        if (CommonParam.LogOnConsole)
        {
            Debug.LogWarning(info);
        }
    }
}
//end
//-------------------------------------------------------------------------
public class CallBack
{
    public object mObject;
    public string mFunction;
    public object mParam;

    public CallBack(object obj, string fun, object param)
    {
        mObject = obj;
        mFunction = fun;
        mParam = param;
    }

    public object Run()
    {
        if (mObject == null)
        {
            DEBUG.LogError("CallBack object is null");
            return null;
        }
        System.Reflection.MethodInfo info = mObject.GetType().GetMethod(mFunction);
        if (info != null)
        {
            object[] p = new object[1];
            p[0] = mParam;
            try
            {
                return info.Invoke(mObject, p);
            }
            catch
            {
				DEBUG.LogError( "CallBack object function param is error >" + mFunction);
                return null;
            }
        }
        else
        {
			DEBUG.LogError( "CallBack object no exist function >"+mFunction);
            return null;
        }

        return null;
    }
}
//-------------------------------------------------------------------------
public class GameCommon
{
    public enum ResourceType
    {
        RES_GENERIC     = 0,
        RES_PREFAB      = 1,
        RES_TEXTURE     = 2,
        RES_MODEL       = 3
    }

    public enum ResourceGroup
    {
        RESG_ALL        = 0,
        RESG_UI         = 1,
        RESG_EFFECT     = 2
    }

	//把相同tid的itemDataBase堆叠起来
	public static void MergeItemDataBase(List<ItemDataBase> itemDataListOld, out List<ItemDataBase> itemDataListNew)
	{
		//把相同的item跌起来
		itemDataListNew = new List<ItemDataBase> ();
		foreach(ItemDataBase itemBase in itemDataListOld)
		{
			int itemId = itemBase.itemId;
			int tid = itemBase.tid;
			DEBUG.Log("itemId = " + itemId + ", tid = " + tid);
			int num = 0;
			itemDataListOld.Foreach(cc => {
				if(cc.tid == tid)
				{
					num = num + cc.itemNum;
				}
			});
			bool hasAdded = false;
			itemDataListNew.Foreach(pp =>
			   {
                   if(pp.tid == tid)
					hasAdded = true;
			   }
			);
			if(!hasAdded)
			{
				ItemDataBase itemBaseTemp = new ItemDataBase();
				itemBaseTemp.itemId = itemId;
				itemBaseTemp.itemNum = num;
				itemBaseTemp.tid = itemBase.tid;
				itemDataListNew.Add (itemBaseTemp);
			}
		}
	}

    //按服务器角色存储内存数据-防止切换账号bug
    public static void SetDataByZoneUid(string norKey, string str)
    {
        string key = norKey + CommonParam.mZoneID + CommonParam.mUId;
        DataCenter.Set(key, str); 
    }

    public static string GetDataByZoneUid(string norKey)
    {
        string key = norKey + CommonParam.mZoneID + CommonParam.mUId;
        return DataCenter.Get(key);
    }


    //by chenliang
    //begin

    /// <summary>
    /// 重置世界摄像机颜色
    /// </summary>
    public static void ResetWorldCameraColor()
    {
        GameObject tmpWorldCamera = GameObject.Find("world_center");
        if (tmpWorldCamera == null)
            return;
        Transform tmpTransCamera = tmpWorldCamera.transform.Find("Prefabs/Camera_Z/Camera");
        foreach (Transform tmpTrans in tmpWorldCamera.transform)
        {
            if(tmpTrans.name == "Prefabs/Camera_Z")
            {
                tmpTransCamera = tmpTrans.Find("Camera");
                break;
            }
        }
        if (tmpTransCamera == null)
            return;
        Camera tmpCamera = tmpTransCamera.gameObject.GetComponent<Camera>();
        if (tmpCamera == null)
            return;
        tmpCamera.backgroundColor = Color.black;
    }
	
    /// <summary>
    /// 动态游戏数据根目录
    /// </summary>
    public static string DynamicGameDataRootPath
    {
        get
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            return Application.dataPath;
#else
            return Application.persistentDataPath;
#endif
        }
    }
    /// <summary>
    /// 动态游戏数据相对路径
    /// </summary>
    public static string DynamicGameDataPath
    {
        get
        {
            return "/GameData";
        }
    }
    /// <summary>
    /// 动态游戏数据绝对路径
    /// </summary>
    public static string DynamicAbsoluteGameDataPath
    {
        get
        {
            return DynamicGameDataRootPath + DynamicGameDataPath;
        }
    }
    /// <summary>
    /// 动态数据临时目录
    /// </summary>
    public static string DynamicGameDataTempPath
    {
        get
        {
            return DynamicGameDataPath + "/Temp";
        }
    }
    /// <summary>
    /// 动态游戏数据临时目录绝对路径
    /// </summary>
    public static string DynamicAbsoluteGameDataTempPath
    {
        get
        {
            return DynamicGameDataRootPath + DynamicGameDataTempPath;
        }
    }
    /// <summary>
    /// 游戏动态二进制数据路径
    /// </summary>
    public static string DynamicGameDataBinaryPath
    {
        get
        {
            return DynamicGameDataPath + "/Binary";
        }
    }
    /// <summary>
    /// 游戏动态二进制数据绝对路径
    /// </summary>
    public static string DynamicAbsoluteGameDataBinaryPath
    {
        get
        {
            return DynamicGameDataRootPath + DynamicGameDataBinaryPath;
        }
    }
    /// <summary>
    /// 检查并且创建游戏数据路径
    /// </summary>
    public static void CheckAndCreateDynamicGameDataPath()
    {
        CheckAndCreateDirectory(DynamicAbsoluteGameDataPath);
        CheckAndCreateDirectory(DynamicAbsoluteGameDataTempPath);
    }
    /// <summary>
    /// 检查并且创建指定路径
    /// </summary>
    /// <param name="folder"></param>
    public static void CheckAndCreateDirectory(string folder)
    {
        string tmpFolder = Path.GetDirectoryName(folder);
        if (!Directory.Exists(tmpFolder))
            Directory.CreateDirectory(tmpFolder);
    }

    /// <summary>
    /// 保存数据到指定文件
    /// </summary>
    /// <param name="path"></param>
    /// <param name="fileData"></param>
    public static void SaveFile(string path, byte[] fileData)
    {
        CheckAndCreateDirectory(path);
        FileStream tmpFS = new FileStream(path, FileMode.Create);
        tmpFS.Write(fileData, 0, fileData.Length);
        tmpFS.Close();
    }
    /// <summary>
    /// 移动文件
    /// </summary>
    /// <param name="srcPath">文件原路径</param>
    /// <param name="dstPath">文件目标路径</param>
    /// <param name="isDelSrc">是否删除原文件</param>
    public static void MoveFile(string srcPath, string dstPath, bool isDelSrc)
    {
        if (!File.Exists(srcPath))
            return;
        if (File.Exists(dstPath))
        {
            //先删除目标文件
            File.Delete(dstPath);
        }
        CheckAndCreateDirectory(dstPath);
        File.Move(srcPath, dstPath);
        if (isDelSrc)
            File.Delete(srcPath);
    }
    /// <summary>
    /// 删除文件
    /// </summary>
    /// <param name="srcPath">文件路径</param>
    public static void DeleteFile(string filePath)
    {
        if (!File.Exists(filePath))
            return;
        File.Delete(filePath);
    }

    /// <summary>
    /// 数据验证失败处理
    /// </summary>
    public static void DataVerifyFailedHandle()
    {
        DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_DATA_HAS_TAMPER, () =>
        {
#if !UNITY_EDITOR && !NO_USE_SDK
            U3DSharkEventListener.LogoutCallback = HotUpdateLoading.ReloadHotUpdateScene;
            U3DSharkSDK.Instance.Logout();
#else
            HotUpdateLoading.ReloadHotUpdateScene();
#endif
        });
    }

    /// <summary>
    /// 打开充值，默认回调操作（打开主界面）
    /// </summary>
    /// <param name="openData"></param>
    /// <param name="panelDepth">Panel层级，小于0时用默认值</param>
    public static void OpenRecharge(RECHARGE_PAGE page, int panelDepth = CommonParam.rechageDepth)
    {
        RechargeContainerOpenData tmpOpenData = new RechargeContainerOpenData()
        {
            Page = page,
            PanelDepth = panelDepth
        };
		Debug.Log ("打开充值界面111-----");
		DEBUG.Log ("打开充值界面111-----");
        RechargeContainerWindow.OpenMyself(tmpOpenData);
    }

	/// <summary>
	/// 充值提示
    /// actionOpen--打开充值界面时执行的操作
    /// actionClose--关闭充值界面执行的操作
	/// </summary>
    public static void ToGetDiamond(string tips = "", Action actionOpen = null, Action actionClose = null)
	{
        if(tips == "")
        {
            tips = TableCommon.getStringFromStringList(STRING_INDEX.RECHAGE_MAIN_TIPS);
        }
        DataCenter.OpenMessageOkWindow(tips, () => {
            if (actionOpen != null)
            {
                actionOpen();
            }
            GetDioman(actionClose); 
        }, false);
		DataCenter.RefreshMessageOkText(STRING_INDEX.RECHAGE_MAIN_TIPS_LEFT, STRING_INDEX.RECHAGE_MAIN_TIPS_RIGHT);
	}

	// <summary>
	//前往充值界面
	static void GetDioman(Action action)	
	{
		GameCommon.OpenRecharge(RECHARGE_PAGE.RECHARGE, action, CommonParam.rechageDepth);
	}

    /// <summary>
    /// 打开充值
    /// </summary>
    /// <param name="openData"></param>
    /// <param name="closeCallback"></param>
    /// <param name="panelDepth">Panel层级，小于0时用默认值</param>
    public static void OpenRecharge(RECHARGE_PAGE page, Action closeCallback, int panelDepth = CommonParam.rechageDepth)
    {
        RechargeContainerOpenData tmpOpenData = new RechargeContainerOpenData()
        {
            Page = page,
            PanelDepth = panelDepth,
            CloseCallback = closeCallback
        };
		Debug.Log ("打开充值界面222-----");
		DEBUG.Log ("打开充值界面222-----");
        RechargeContainerWindow.OpenMyself(tmpOpenData);
    }

    //end

    /// <summary>
    /// NOTE: DO NOT use mResources outside of resource manager; use GameCommon's public method instead
    /// </summary>
    static public MResources mResources = new MResources();
	static public GameObject FindObject(GameObject parentObject, string targetObjName)
	{
		if (parentObject==null)
			return null;

		foreach( Transform f in parentObject.transform)
		{
			string n = f.gameObject.name;

			if (n==targetObjName)
				return f.gameObject;

			GameObject o = GameCommon.FindObject(f.gameObject, targetObjName);
			if (o!=null)
				return o;
		}	

		return null;
	}

    static public void SetLayer(GameObject gameObject, int layer)
    {
        if (gameObject==null)
			return;

        gameObject.layer = layer;

		foreach( Transform f in gameObject.transform)
		{			
            SetLayer(f.gameObject, layer);
        }
    }


	static public string MakeResourceName(string resName)
	{
		string temp = resName.ToLower();
		char[] p = { '/' };
		string[] str = temp.Split(p);
		if (str[0]=="resources")
			temp = resName.Substring(10, resName.Length-10);
		string path = Path.GetDirectoryName(temp);
		return path + "/" + Path.GetFileNameWithoutExtension(temp);
	}

    public static void Swap<T>(ref T t1, ref T t2)
    {
        T t = t1;
        t1 = t2;
        t2 = t;
    }

    static public bool InBound(float scrX, float scrY, float destX1, float destY1, float destX2, float destY2)
    {
        return scrX >= destX1
            && scrX <= destX2
            && scrY >= destY1
            && scrY <= destY2;
    }

    

    static public bool BoundIntersect(float scrX1, float scrY1, float scrX2, float scrY2,  float destX1, float destY1, float destX2, float destY2)
    {
        return !
            (scrX1 > destX2
            || scrX2 < destX1
            || scrY1 > destY2
            || scrY2 < destY1
            );
    }


    public static string MakePathFileNameForWWW(string strFileName)
    {
        string fileName = "file://";

#if !(UNITY_EDITOR || UNITY_STANDALONE)
	    fileName += Application.persistentDataPath + "/" + strFileName ;
#else
        fileName += Application.dataPath + "/" + strFileName;
#endif
        return fileName;
    }
	
	public static string MakeGamePathFileName(string strFileName)
    {
        string fileName = "";

#if !(UNITY_EDITOR || UNITY_STANDALONE)
	    fileName += Application.persistentDataPath + "/" + strFileName ;
#else
        //fileName+=Application.persistentDataPath+"/"+strFileName;
        fileName += Application.dataPath + "/" + strFileName;
#endif
        return fileName;
    }

	public static void SaveUsrLoginDataFromUnity(string usrName, string pw) {
		if(!string.IsNullOrEmpty(usrName)) {
			PlayerPrefs.SetString("USRNAME", usrName);
		}else {
			DEBUG.Log("Save USRNAME is Null!");
		}

		if(!string.IsNullOrEmpty(pw)) {
			PlayerPrefs.SetString("PW", pw);
		}else {
			DEBUG.Log("Save PW is Null!");
		}
		PlayerPrefs.Save();
	}

	public static string[] GetSavedLoginDataFromUnity() {
		string[] data = new string[2];
		data[0] = PlayerPrefs.GetString("USRNAME");
		data[1] = PlayerPrefs.GetString("PW");
		return data;
	}

    public static void ReadyMesh(ref Mesh mesh, bool bCenter)
    {
        // ready mesh.
        Vector3[] vec = new Vector3[4];
       
        float zF = -0.5f;
        float iF = 0.5f;

        if (!bCenter)
        {
            zF = 0;
            iF = 1;
        }

        //zF = (zF - 0.5f) * scale;
        //iF = (iF - 0.5f) * scale;
        vec[0] = new Vector3(zF, zF, 0.0f);
        vec[1] = new Vector3(iF, zF, 0.0f);
        vec[2] = new Vector3(zF, iF, 0.0f);
        vec[3] = new Vector3(iF, iF, 0.0f);

        Color[] color = new Color[4];
        color[0] = Color.white;
        color[1] = Color.white;
        color[2] = Color.white;
        color[3] = Color.white;

        int[] tg = { 2, 1, 0, 3, 1, 2 }; // 0, 1, 2, 2, 1, 3 };

        mesh.vertices = vec;        
        mesh.triangles = tg;

        //if (bUV)
        {
			Vector2[] uv = new Vector2[4];
            float u2 = 1.0f;
            float v2 = 1.0f;
			zF = 0.0f;
            uv[0] = new Vector2(zF, zF);
            uv[1] = new Vector2(u2, zF);
            uv[2] = new Vector2(zF, v2);
            uv[3] = new Vector2(u2, v2);

            mesh.uv = uv;
        }

        
        Vector3[] normal = new Vector3[4];
        for (int i=0; i<4; ++i)
        {
            normal[i] = Vector3.forward;
        }
        mesh.normals = normal;

        mesh.colors = color;
    }

    public static Vector4[] ReadyTextureAnimaiton(string textureFileName, string xmlDataFile, out Texture destTexture, out float SpiritWidth, out float SpiritHeight)
    {
        destTexture = new Texture2D(0, 0, TextureFormat.RGBA32, false, false);
        destTexture.wrapMode = TextureWrapMode.Clamp;
        string fileName = MakePathFileNameForWWW(textureFileName);
        WWW z = new WWW(fileName);
        z.LoadImageIntoTexture((Texture2D)destTexture);
		
		return LoadUVAnimation(destTexture, xmlDataFile, out SpiritWidth, out SpiritHeight);
	}
	public static Vector4[] LoadUVAnimation(Texture tex, string xmlDataFile, out float SpiritWidth, out float SpiritHeight)
	{
        SpiritWidth = 0;
        SpiritHeight = 0;
        Vector4[] uvInfo = null;
        XmlDocument xml = new XmlDocument();
        try
        {
            string xmlFileName = MakePathFileNameForWWW(xmlDataFile);
            xml.Load(xmlFileName);
            XmlNodeList node = xml.GetElementsByTagName("Frame");

            float width = (float)tex.width;
            float height = (float)tex.height;

            int mCount = node.Count;

            if (mCount <= 0)
                return uvInfo;

            uvInfo = new Vector4[mCount];
            for (int i = 0; i < mCount; ++i)
            {
                XmlNode n = node[i];
                int h = int.Parse(n.Attributes[0].Value);
                int w = int.Parse(n.Attributes[2].Value);
                int x = int.Parse(n.Attributes[3].Value);
                int y = int.Parse(n.Attributes[4].Value);

                uvInfo[i].x = (float)x;
                uvInfo[i].y = (float)y;

                uvInfo[i].w = (float)w;
                uvInfo[i].z = (float)h;
            }
            SpiritWidth = uvInfo[0].w;
            SpiritHeight = uvInfo[0].z;
            for (int i = 0; i < mCount; ++i)
            {
                uvInfo[i].w = (uvInfo[i].x + uvInfo[i].w) / width;
                uvInfo[i].z = (height - (uvInfo[i].y + uvInfo[i].z)) / height;
                uvInfo[i].x /= width;
                uvInfo[i].y = (height - uvInfo[i].y) / height;
            }
            return uvInfo;
        }
        catch (System.Exception e)
        {
            uvInfo = null;
            Logic.EventCenter.Log(LOG_LEVEL.GENERAL, e.ToString());
        }
        return uvInfo;
    }

    public static Vector4 ReadyTextureUV(string xmlDataFile, int pos, float texWidth, float texHeight, out float SpiritWidth, out float SpiritHeight)
    {
        SpiritWidth = 0;
        SpiritHeight = 0;
        Vector4 uvInfo = new Vector4();
        XmlDocument xml = new XmlDocument();
        try
        {
            string xmlFileName = MakePathFileNameForWWW(xmlDataFile);
            xml.Load(xmlFileName);
            XmlNodeList node = xml.GetElementsByTagName("Frame");

            float width = texWidth;
            float height = texHeight;

            int mCount = node.Count;

            if (mCount <= pos)
                return uvInfo;
           
            XmlNode n = node[pos];
            int h = int.Parse(n.Attributes[4].Value);
            int w = int.Parse(n.Attributes[3].Value);
            int x = int.Parse(n.Attributes[1].Value);
            int y = int.Parse(n.Attributes[2].Value);

            uvInfo.x = (float)x;
            uvInfo.y = (float)y;

            uvInfo.w = (float)w;
            uvInfo.z = (float)h;

            SpiritWidth = uvInfo.w;
            SpiritHeight = uvInfo.z;
            uvInfo.w = (uvInfo.x + uvInfo.w) / width;
            uvInfo.z = (height - (uvInfo.y + uvInfo.z)) / height;
            uvInfo.x /= width;
            uvInfo.y = (height - uvInfo.y) / height;

            return uvInfo;
        }
        catch (System.Exception e)
        {           
            DEBUG.Log(e.ToString());
        }
        return uvInfo;
    }

    public static void SetMeshUV(ref Mesh mesh, Vector4 uv)
    {
        Vector2[] mMeshUV = mesh.uv;

        mMeshUV[0].x = uv.x;
        mMeshUV[0].y = uv.y;

        mMeshUV[1].x = uv.z;
        mMeshUV[1].y = uv.y;

        mMeshUV[2].x = uv.x;
        mMeshUV[2].y = uv.w;

        mMeshUV[3].x = uv.z;
        mMeshUV[3].y = uv.w;

        mesh.uv = mMeshUV;
    }

    //public static Texture LoadTexture(string textureFileName)
    //{
    //    Texture destTexture = new Texture2D(0, 0, TextureFormat.RGBA32, false, false);
    //    destTexture.wrapMode = TextureWrapMode.Clamp;
    //    string fileName = MakePathFileNameForWWW(textureFileName);
    //    WWW z = new WWW(fileName);
    //    z.LoadImageIntoTexture((Texture2D)destTexture);
    //    return destTexture;
    //}

    // auto load resources, first try load from file data (run dir for update), then load from resources.
    public static DataBuffer LoadData(string fileName, LOAD_MODE loadMode)
    {
        DataBuffer data = null;
        switch(loadMode)
        {
            case LOAD_MODE.BYTES:
                if (RuntimePlatform.WindowsWebPlayer != Application.platform)
                {
                    Game.Resource res = Game.ResourceManager.Self.GetResource(fileName, "FILE");
                    data = res.ReadData();
                }
                break;

            case LOAD_MODE.BYTES_RES:
			{
                Game.Resource res = Game.ResourceManager.Self.GetResource(fileName, "UNITY");
                data = res.ReadData();
                break;
			}
            case LOAD_MODE.BYTES_TRY_PATH:
			{
                Game.Resource res = Game.ResourceManager.Self.GetResource(fileName, "FILE");
                data = res.ReadData();
                if (data==null)
                {
                    res = Game.ResourceManager.Self.GetResource(fileName, "UNITY");
                    data = res.ReadData();
                }
                break;
			}
            case LOAD_MODE.BYTES_TRY_RES:
			{
                Game.Resource res = Game.ResourceManager.Self.GetResource(fileName, "UNITY");
                data = res.ReadData();
                if (data==null)
                {
                    res = Game.ResourceManager.Self.GetResource(fileName, "FILE");
                    data = res.ReadData();
                }
                break;
			}
        }
        return data;
    }

    //public static Texture LoadTextureFromResources(string textureFile)
    //{
    //    char[] pa = { '.' };
    //    string[] re = textureFile.Split(pa);
    //    if (re.Length >= 1)
    //    {
    //        //Texture2D destTexture = new Texture2D(0, 0, TextureFormat.RGBA32, false, false);
    //        UnityEngine.Object obj = Resources.Load(re[0], typeof(Texture));
    //        return obj as Texture;
    //    }
    //    return null;
    //}

    static public Texture LoadTexture(string textureFile)
    {
        return mResources.LoadTexture(textureFile);
        //return Resources.Load(textureFile, typeof(Texture)) as Texture;
        //return Resources.Load<Texture>(textureFile);
    }

    public static Texture LoadTexture(string textureFile, LOAD_MODE loadMode)
    {
        Texture2D destTexture = null;

        switch (loadMode)
        {
            case LOAD_MODE.SCR_FILE:
            case LOAD_MODE.WWW:
                {
                    destTexture = new Texture2D(0, 0, TextureFormat.RGBA32, false, false);
                    destTexture.wrapMode = TextureWrapMode.Clamp;
                    string fileName = MakePathFileNameForWWW(textureFile);
                    WWW z = new WWW(fileName);
                    z.LoadImageIntoTexture((Texture2D)destTexture);
                }
                break;
            case LOAD_MODE.RESOURCE:
                {								
					destTexture = LoadTexture(MakeResourceName(textureFile)) as Texture2D;
					if (destTexture!=null)
                     	destTexture.wrapMode = TextureWrapMode.Clamp;                    
					else 
					{
						int b = 2;
					}
                }
                break;
			
            default:
                DataBuffer data = LoadData(textureFile, loadMode);
                if (data != null)
                {
                    destTexture = new Texture2D(0, 0, TextureFormat.RGBA32, false, false);

                    destTexture.LoadImage(data.getData());
                    destTexture.wrapMode = TextureWrapMode.Clamp;
                    Logic.EventCenter.Log(false, "succeed load texture >" + textureFile);
                    return destTexture;
                }
                else
                    Logic.EventCenter.Log(true, "fail load textrue >" + textureFile);
			break;
        }
        return destTexture;
    }

    public static GameObject LoadModel(string resourcePath)
    {
        UnityEngine.Object obj = mResources.LoadModel(resourcePath);
        if(obj != null)
            return GameObject.Instantiate(obj) as GameObject;

        return CreateObject(resourcePath, ResourceType.RES_MODEL);
    }

    public static RuntimeAnimatorController LoadController(string resourcePath)
    {
        var obj = mResources.LoadController(resourcePath);
        if(obj != null)
            return obj;

        return Resources.Load(resourcePath, typeof(RuntimeAnimatorController)) as RuntimeAnimatorController;
    }

    //public static bool LoadTableOnUnity(string tableName, out NiceTable destTable)
    //{
    //    destTable = new NiceTable();
    //    UnityEngine.Object obj = Resources.Load(tableName, typeof(TextAsset));
    //    if (obj == null)
    //        return false;

    //    TextAsset byteTextAsset = (TextAsset)obj;
    //    DataBuffer data = new DataBuffer();
    //    data._setData(byteTextAsset.bytes);
    //    bool b = destTable.restore(ref data);        
    //    Resources.UnloadUnusedAssets();
    //    return b;
    //}

    public static NiceTable LoadTable(string tableName, LOAD_MODE loadMode)
    {        
        NiceTable   resultTable = new NiceTable();
        bool bResult = false;
        switch (loadMode)
        {
            case LOAD_MODE.UNICODE:
                //by chenliang
                //begin

//                bResult = resultTable.LoadTable(GameCommon.MakeGamePathFileName(tableName), System.Text.Encoding.UTF8, loadMode);
                //---------------------
			Debug.Log("pppppppppp=====" + tableName.IndexOf("Config/StaticConfig") + "ooooooo===" + ResourceLocalLoadInfoManager.Instance.IsLoadFromApk(tableName, true));
			DEBUG.Log("pppppppppp=====" + tableName.IndexOf("Config/StaticConfig") + "ooooooo===" + ResourceLocalLoadInfoManager.Instance.IsLoadFromApk(tableName, true));
                if (tableName.IndexOf("Config/StaticConfig") != -1 || ResourceLocalLoadInfoManager.Instance.IsLoadFromApk(tableName, true))//HotUpdateLoading.IsUseApkData)
                    bResult = resultTable.LoadTable(GameCommon.MakeGamePathFileName(tableName), System.Text.Encoding.UTF8, loadMode);
                else
                    bResult = resultTable.LoadTable(DynamicAbsoluteGameDataPath + "/" + tableName, System.Text.Encoding.UTF8, loadMode);

                //end
            break;

            case LOAD_MODE.ANIS:
                //by chenliang
                //begin

//                bResult = resultTable.LoadTable(GameCommon.MakeGamePathFileName(tableName), System.Text.Encoding.Default, loadMode);
//-------------------
            if (tableName.IndexOf("Config/StaticConfig") != -1 || ResourceLocalLoadInfoManager.Instance.IsLoadFromApk(tableName, true))//HotUpdateLoading.IsUseApkData)
                bResult = resultTable.LoadTable(GameCommon.MakeGamePathFileName(tableName), System.Text.Encoding.Default, loadMode);
            else
                bResult = resultTable.LoadTable(DynamicAbsoluteGameDataPath + "/" + tableName, System.Text.Encoding.Default, loadMode);

                //end
            break;

            case LOAD_MODE.BYTES:
            case LOAD_MODE.BYTES_RES:
            case LOAD_MODE.BYTES_TRY_PATH:
            case LOAD_MODE.BYTES_TRY_RES: 
                //by chenliang
                //begin

//                DataBuffer data = LoadData(tableName, loadMode);
//---------------------------
                DataBuffer data = null;
                if(tableName.IndexOf("Config/StaticConfig") != -1 || !HotUpdateLoading.IsUseHotUpdate || ResourceLocalLoadInfoManager.Instance.IsLoadFromApk(tableName, true))//HotUpdateLoading.IsUseApkData)
                    data = LoadData(tableName, loadMode);
                else
                {
                    //直接读取文件数据
                    string tmpTablePath = DynamicAbsoluteGameDataPath + "/" + tableName;
//					string tmpTablePath = Application.streamingAssetsPath + "/" + tableName;
					DEBUG.Log("获取文件路径---" + tmpTablePath);
					//将后缀改为.bytes
                    tmpTablePath = tmpTablePath.Remove(tmpTablePath.LastIndexOf(".")) + ".bytes";
                    try
                    {
                        FileStream tmpFS = new FileStream(tmpTablePath, FileMode.Open);
                        int tmpFileLength = (int)tmpFS.Length;
                        data = new DataBuffer(tmpFileLength);
                        if (tmpFS.Read(data.getData(), 0, tmpFileLength) != tmpFileLength)
                        {
                            DEBUG.LogError("Read file \"" + tmpTablePath + "\" length error");
                            data = null;
                        }
                        tmpFS.Close();
                    }
                    catch(Exception excep)
                    {
                        DEBUG.LogError("Can't open file \"" + tmpTablePath + "\", error = " + excep.ToString());
                    }
                }
                if(data == null)
                    break;

                //end
                bResult = resultTable.restore(ref data);

            break;
        }
        //DataBuffer data = AutoLoadData(tableName);
        //if (null != data)
        //{
        //    NiceTable destTable = new NiceTable();
        //    if (destTable.restore(ref data))
        //        return destTable;
        //}
        if (true)
            return resultTable;

        return null;
    }

    //public static NiceTable LoadTableOnPath(string tableName, System.Text.Encoding fileEncoding)
    //{
    //    NiceTable t = new NiceTable();
    //    if (t.LoadTable(MakeGamePathFileName(tableName), fileEncoding))
    //        return t;
    //    return null;
    //}

	static public GameObject LoadAndIntanciateUIPrefabs(string strPrefabsName, string strParentName)
	{
        return LoadAndIntanciateUIPrefabs(UICommonDefine.strUIPrefabsPath, strPrefabsName, strParentName);
	}

	static public GameObject LoadUIPrefabs(string strPrefabsName, GameObject parentObj)
	{
        return LoadAndIntanciateUIPrefabs(UICommonDefine.strUIPrefabsPath, strPrefabsName, parentObj);
	}

    static public GameObject LoadAndIntanciateUIPrefabs(string strPath, string strPrefabsName, string strParentName)
    {
        if(string.IsNullOrEmpty(strPrefabsName))
            return null;

        GameObject obj = GameCommon.CreateObject(strPath + strPrefabsName, strParentName, ResourceType.RES_PREFAB, ResourceGroup.RESG_UI);

        if (obj == null)
        {
            Logic.EventCenter.Log(LOG_LEVEL.WARN, "this prefab is not exist>" + strPrefabsName);
        }
        return obj;
    }

    static public GameObject LoadAndIntanciateUIPrefabs(string strPath, string strPrefabsName, GameObject parentObj)
    {
        GameObject obj = GameCommon.CreateObject(strPath + strPrefabsName, parentObj, ResourceType.RES_PREFAB, ResourceGroup.RESG_UI);

        if (obj == null)
        {
            Logic.EventCenter.Log(LOG_LEVEL.WARN, "this prefab is not exist>" + strPrefabsName);
        }
        else
            obj.name = strPrefabsName;
        return obj;
    }

    static public GameObject LoadAndIntanciateEffectPrefabs(string strPrefabsName, string strParentName)
    {
        GameObject obj = GameCommon.CreateObject(strPrefabsName, strParentName, ResourceType.RES_PREFAB, ResourceGroup.RESG_EFFECT);

        if (obj == null)
        {
            Logic.EventCenter.Log(LOG_LEVEL.WARN, "this prefab is not exist>" + strPrefabsName);
        }
        return obj;
    }

    static public GameObject LoadAndIntanciateEffectPrefabs(string strPrefabsName, GameObject parentObj)
    {
        GameObject obj = GameCommon.CreateObject(strPrefabsName, parentObj, ResourceType.RES_PREFAB, ResourceGroup.RESG_EFFECT);

        if (obj == null)
        {
            Logic.EventCenter.Log(LOG_LEVEL.WARN, "this prefab is not exist>" + strPrefabsName);
        }
        else
            obj.name = strPrefabsName;
        return obj;
    }

    static public GameObject LoadAndIntanciateEffectPrefabs(string prefabName)
    {
        GameObject uiObject = CreateObject(prefabName, ResourceType.RES_PREFAB, ResourceGroup.RESG_EFFECT);
        return uiObject;
    }

    static public GameObject LoadAndIntanciatePrefabs(string strPrefabsName, string strParentName)
    {
        GameObject obj = GameCommon.CreateObject(strPrefabsName, strParentName, ResourceType.RES_PREFAB);

        if (obj == null)
        {
            Logic.EventCenter.Log(LOG_LEVEL.WARN, "this prefab is not exist>" + strPrefabsName);
        }
        return obj;
    }

    static public GameObject LoadAndIntanciatePrefabs(string strPrefabsName, GameObject parentObj)
    {
        GameObject obj = GameCommon.CreateObject(strPrefabsName, parentObj, ResourceType.RES_PREFAB);

        if (obj == null)
        {
            Logic.EventCenter.Log(LOG_LEVEL.WARN, "this prefab is not exist>" + strPrefabsName);
        }
        else
            obj.name = strPrefabsName;
        return obj;
    }

    static public GameObject LoadAndIntanciatePrefabs(string prefabName)
    {
        return CreateObject(prefabName, ResourceType.RES_PREFAB);
    }

    static public GameObject LoadPrefabs(string prefabName)
    {
        return mResources.LoadPrefab(prefabName, "");
    }

    static public GameObject LoadPrefabs(string prefabName, string hint)
    {
        return mResources.LoadPrefab(prefabName, hint);
    }

    static public AudioClip LoadAudioClip(string soundResName)
    {
        //return Resources.Load(soundResName, typeof(AudioClip)) as AudioClip;
        return mResources.LoadSound(soundResName);
    }

    static public Shader FindShader(string shaderName)
    {
        var shader = mResources.LoadShader(shaderName);
        if(shader != null)
            return shader;
        return Shader.Find(shaderName);
    }

    static public void LoadLevel(string scene)
    {
        mResources.LoadLevel(scene);
    }

    static public IEnumerator LoadLevelAsync(string scene)
    {
        //mResources.LoadLevel();
        yield break;
    }
	//将配置表中的"//n"改为"/n" 
	public static string ReplaceWithSpace(string text)
	{
		string result = text.Replace ("\\n","\n");
		return result;
	}
	//将更改Label的文字默认颜色  
	public static void SetTextColor(GameObject obj,string color)
	{
		UILabel label = obj.GetComponent<UILabel> ();
		if (label == null)
			return;
		label.text = color+label.text;
	}
	public static void SetTextColor(GameObject obj)
	{
		SetTextColor (obj,"[fffeaa]");
	}
	static public string FormatTime(double fSecordTime)
    {
        TimeSpan span = new TimeSpan((long)fSecordTime * 10000000);
        return span.ToString().Replace(".", "天");
    }

    static public DateTime TotalSeconds2DateTime(Int64 serverTime)
    {
        TimeSpan elapse = new TimeSpan(serverTime * 10000000);
        DateTime begin = new DateTime(1970, 1, 1, 8, 0, 0); // China Standard Time is 1970-1-1 8:0:0 when UTC Time is 1970-1-1 0:0:0
        return begin + elapse;
    }

    static public Int64 DateTime2TotalSeconds(DateTime data)
    {
        DateTime begin = new DateTime(1970, 1, 1, 8, 0, 0); // China Standard Time is 1970-1-1 8:0:0 when UTC Time is 1970-1-1 0:0:0
        TimeSpan elapse = data - begin;
        return (Int64)elapse.TotalSeconds;
    }

    static public DateTime NowDateTime()
    {
        return TotalSeconds2DateTime(CommonParam.NowServerTime());
    }

    /// <summary>
    /// 加载的公用窗口
    /// </summary>
    /// <param name="strPrefabsName">预制件名字</param>
    /// <param name="strParentName">需要挂载的父窗口名字</param>
    /// <returns></returns>
	static public GameObject InitCommonUI(string strPrefabsName, string strParentName)
	{
		GameObject commonUI = null;
        if (!GameCommon.bIsLogicDataExist(strPrefabsName))
		{
			commonUI = LoadAndIntanciateUIPrefabs(strPrefabsName, strParentName);
			commonUI.transform.localPosition = new Vector3(0, 0, -1000.0f);
		}
		return commonUI;
	}

    static public void SetWindowZ(GameObject obj, int windowZ)
    {
        obj.transform.localPosition = new Vector3(0, 0, windowZ);
    }

    static public GameObject CreateObject(string resourceNamePath)
    {
        return CreateObject(resourceNamePath, ResourceType.RES_GENERIC, ResourceGroup.RESG_ALL);
    }

    static public GameObject CreateObject(string resourceNamePath, ResourceType resType)
    {
        return CreateObject(resourceNamePath, resType, ResourceGroup.RESG_ALL);
    }


    static public GameObject CreateObject(string resourceNamePath, ResourceType resType, ResourceGroup resGroup)
    {
        UnityEngine.Object obj = null;

        switch(resType)
        {
            case ResourceType.RES_PREFAB:
                if(resGroup == ResourceGroup.RESG_EFFECT)
                    obj = mResources.LoadPrefab(resourceNamePath, "EFFECT");
                else if(resGroup == ResourceGroup.RESG_UI)
                    obj = mResources.LoadPrefab(resourceNamePath, "UI");
                else 
                    obj = mResources.LoadPrefab(resourceNamePath, "");
                break;
            case ResourceType.RES_MODEL:
                obj = mResources.LoadModel(resourceNamePath);
                break;
            case ResourceType.RES_TEXTURE:
                obj = mResources.LoadTexture(resourceNamePath);
                break;
            default:
                obj = Resources.Load(resourceNamePath, typeof(UnityEngine.Object)); //
                break;
        }

        if (obj != null)
            return GameObject.Instantiate(obj) as GameObject;

        return null;
    }

    static public GameObject CreateObject(string resourceNamePath, string strParentName)
    {
        return CreateObject(resourceNamePath, strParentName, ResourceType.RES_GENERIC, ResourceGroup.RESG_ALL);
    }

    static public GameObject CreateObject(string resourceNamePath, string strParentName, ResourceType resType)
    {
        return CreateObject(resourceNamePath, strParentName, resType, ResourceGroup.RESG_ALL);
    }

	static public GameObject CreateObject(string prefabName, string strParentName, ResourceType resType, ResourceGroup resGroup)
	{
		GameObject ui = GameCommon.FindUI(strParentName);
		if (ui==null)
		{
			Logic.EventCenter.Log(LOG_LEVEL.ERROR, "No exist gameobject > "+strParentName);
			return null;
		}
		GameObject uiObject = CreateObject(prefabName, resType, resGroup);
        if (uiObject != null)
		{
            uiObject.transform.parent = ui.transform;
            uiObject.transform.localPosition = new Vector3(0, 0, 0);
            uiObject.transform.localScale = new Vector3(1, 1, 1);
		}
		return uiObject;
	}

    static public GameObject CreateObject(string resourceNamePath, GameObject parentObj)
    {
        return CreateObject(resourceNamePath, parentObj, ResourceType.RES_GENERIC, ResourceGroup.RESG_ALL);
    }

    static public GameObject CreateObject(string resourceNamePath, GameObject parentObj, ResourceType resType)
    {
        return CreateObject(resourceNamePath, parentObj, resType, ResourceGroup.RESG_ALL);
    }

	static public GameObject CreateObject(string prefabName,  GameObject parentObj, ResourceType resType, ResourceGroup resGroup)
	{
		if (parentObj==null)
		{
			Logic.EventCenter.Log(LOG_LEVEL.ERROR, "No exist gameobject parentObj ");
			return null;
		}
		GameObject uiObject = CreateObject(prefabName, resType, resGroup);
		if (uiObject != null)
		{
			uiObject.transform.parent = parentObj.transform;
			uiObject.transform.localPosition = new Vector3(0, 0, 0);
			uiObject.transform.localScale = new Vector3(1, 1, 1);
		}
		return uiObject;
	}
	
	static public bool PlaySound(string soundResName, Vector3 playPos)
    {
        if (!Settings.IsSoundEffectEnabled())
            return false;

        if (soundResName != "")
        {
            AudioClip clip = LoadAudioClip(soundResName);
			PlaySound(clip, playPos);
        }
		return false;
    }

    static public bool PlaySound(string soundResName, Vector3 playPos, int soundtype)
    {
        if (soundtype >= 0)   // 音效管理
        {
            if (FightingSoundControler.Self != null)
            {
                FightingSoundControler.Self.PlayFightEffectSound(soundtype, soundResName, playPos);
            }
        }
        else 
        {
            GameCommon.PlaySound(soundResName, playPos);
        }
        return false;
    }

	static public bool PlaySound(AudioClip clip, Vector3 playPos)
	{
		if (!Settings.IsSoundEffectEnabled())
			return false;

		if (clip != null)
		{
			AudioSource.PlayClipAtPoint(clip, playPos, CommonParam.DefaultVolume);
			///DEBUG.Log(" ! play back music > " + clip.name);
			return true;
		}

		return false;
	}
	
	static public bool RandPlaySound(string soundResName, Vector3 playPos)
	{
		char[] pa = { ' ' };
		string[] soundList = soundResName.Split(pa);
        if (soundList.Length <= 0)
            return false;

        int r = UnityEngine.Random.Range(0, soundList.Length);
        return PlaySound(soundList[r], playPos);
    }

	static public bool SetBackgroundSound(string strPath, float fVolume)
	{
		GameObject cameraObj = GetMainCameraObj();
		if(cameraObj != null)
		{
			AudioSource sound = cameraObj.GetComponent<AudioSource>();
			if (sound==null || !sound.enabled)
				return false;
			
			string backSound = strPath;
            AudioClip clip = LoadAudioClip(backSound);

			if (clip != null)
			{
				//sound.clip = clip;
				sound.loop = true;
				float v = fVolume;

				if (v > 0.001)
					sound.volume = 0.35f;
				else
					sound.volume = CommonParam.MusicVolume;

                if (sound.clip != clip || !sound.isPlaying)
                {
                    sound.clip = clip;

                    if (Settings.IsMusicEnabled())
                    {
                        sound.Play();
                    }
                }
                				
				return true;
			}
		}
		return false;
	}
	
	static public bool RemoveBackgroundSound()
	{
		GameObject cameraObj = GetMainCameraObj();
		if(cameraObj != null)
		{
			AudioSource sound = cameraObj.GetComponent<AudioSource>();
			if (sound==null || !sound.enabled)
				return false;

			sound.clip = null;
				
			return true;
		}
		return false;
	}
	
	static public GameObject FindUI(string windowName)
    {
        GameObject obj = GameObject.Find(CommonParam.UIRootName);
        if (obj != null)
            return FindObject(obj, windowName);

        return null;
    }
       
    static public bool SetUIText(GameObject winObj, string lableOwnerName, string textInfo)
    {
        GameObject obj = GameCommon.FindObject(winObj, lableOwnerName);
        if (obj != null)
        {
			UILabel lable = obj.GetComponent<UILabel>();
			if (lable==null)
            	lable = obj.GetComponentInChildren<UILabel>();

			if(textInfo != null)
			{
				textInfo = textInfo.Replace ("\\n", "\n");
			}
            if (lable != null)
            {
				lable.text = textInfo;
                //by chenliang
                //begin

                lable.SetDirty();

                //end
                return true;
            }
        }
        return false;
    }

    static public void  SetUIPosition(GameObject winObj, string OwnerName, Vector3 vector3)
    {
        GameObject obj = GameCommon.FindObject(winObj, OwnerName);
        if (obj != null && obj.transform != null && vector3 != null)
        {
            obj.transform.localPosition = vector3;
        }
    }

    static public void SetUITextRecursively(GameObject winObj, string labelOwnerName, string textInfo)
    {
        GameObject obj = GameCommon.FindObject(winObj, labelOwnerName);

        if (obj != null)
        {
            UILabel[] labels = obj.GetComponentsInChildren<UILabel>();

            foreach (var label in labels)
                label.text = textInfo;
        }
    }

    static public bool SetUIVisiable(GameObject winObj, string uiName, bool bVisiable)
    {
         GameObject obj = GameCommon.FindObject(winObj, uiName);
         if (obj != null)
         {
             obj.SetActive(bVisiable);
             return true;
         }
         return false;
    }

	static public bool SetUIVisiable(GameObject parentObj, bool bVisiable, params string[] uiNames)
	{
		foreach(string s in uiNames)
		{
			GameObject obj = GameCommon.FindObject (parentObj, s);
			if(obj != null)
				obj.SetActive (bVisiable);
			else 
				DEBUG.LogWarning ("can not  find " + s + "in" + obj.ToString ());
		}
		return true;
	}

    static public bool SetUISprite(GameObject winObj, string SpriteOwnerName, string argSpriteName)
    {
        GameObject obj = GameCommon.FindObject(winObj, SpriteOwnerName);
        if (obj != null)
        {
            UISprite sprite = obj.GetComponentInChildren<UISprite>();
            if (sprite != null)
            {
                sprite.spriteName = argSpriteName;
                return true;
            }
        }
        return false;
    }

    static public bool SetUISprite(GameObject winObj, string SpriteOwnerName, string argSpriteName, UIAtlas argAtlas)
    {
        GameObject obj = GameCommon.FindObject(winObj, SpriteOwnerName);
        if (obj != null)
        {
            UISprite sprite = obj.GetComponentInChildren<UISprite>();
            if (sprite != null)
            {
                sprite.atlas = argAtlas;
                sprite.spriteName = argSpriteName;
                //sprite.MakePixelPerfect();
                return true;
            }
        }
        return false;
    }

    static public bool SetUISprite(GameObject winObj, string SpriteOwnerName, string argAtlasName, string argSpriteName)
    {
        UIAtlas atlas = LoadUIAtlas(argAtlasName);
        if (atlas == null)
        {
            DEBUG.LogError("No exist atlas >" + argAtlasName);
            return false;
        }
        return SetUISprite(winObj, SpriteOwnerName, argSpriteName, atlas);
    }

    static public bool SetSprite(GameObject spriteObject, string atlasName, string atlasSpriteName)
    {
        if (spriteObject == null)
            return false;

        UISprite sprite = spriteObject.GetComponent<UISprite>();
        if (sprite != null)
        {
            sprite.atlas = LoadUIAtlas(atlasName);
            sprite.spriteName = atlasSpriteName;           
            return true;
        }
        return false;
    }

    static public void SetItemIconWithNum(GameObject kParentObj, ItemDataBase kItemData, string kSpriteName = "item_icon", string kLblName = "item_num_label") 
    {
        if(kParentObj == null || kItemData == null)
            return;
        GameObject _spriteObj = GameCommon.FindObject(kParentObj,kSpriteName);
        GameCommon.SetOnlyItemIcon(_spriteObj, kItemData.tid);

        GameCommon.SetUIText(kParentObj,kLblName,"x" + kItemData.itemNum);
    }

    static public UIAtlas LoadUIAtlas(string argAtlasName)
    {
        if(string.IsNullOrEmpty(argAtlasName))
            return null;

        string fullAtlasName = "textures/UItextures/" + argAtlasName;
        return mResources.LoadUIAtlas(fullAtlasName, "UIAtlas");
    }

    static public bool SetMonsterIcon(GameObject parentWin, string iconUIName, int monstConfigID)
    {
        string strAtlasName = TableCommon.GetStringFromActiveCongfig(monstConfigID, "HEAD_ATLAS_NAME");
        string strSpriteName = TableCommon.GetStringFromActiveCongfig(monstConfigID, "HEAD_SPRITE_NAME");

        UIAtlas tu = GameCommon.LoadUIAtlas(strAtlasName);
        return SetUISprite(parentWin, iconUIName, strSpriteName, tu);
    }

    static public void SetMonsterIcon(GameObject iconObj, int index)
    {
        MonsterIcon icon = new MonsterIcon(iconObj);
        icon.Reset();
        icon.Set(index);
    }

	static public void SetRoleTitle(UISprite sprite)
	{
		SetRoleTitle(sprite, RoleLogicData.GetMainRole().starLevel);
	}

	static public void SetRoleTitle(UISprite sprite, int iStarLevel)
	{
		SetIcon(sprite, "CardAtlas", "ui_chenghao_" + iStarLevel.ToString());
	}

    static public void SetRoleTitle(UILabel label)
    {
        SetRoleTitle(label, RoleLogicData.GetMainRole().starLevel);
    }

    static public void SetRoleTitle(UILabel label, int iStarLevel)
    {
        switch (iStarLevel)
        {
            case 1:
                label.text = "驱鬼师";
                break;
            case 2:
                label.text = "御妖师";
                break;
            case 3:
                label.text = "伏魔师";
                break;
            default:
                label.text = "";
                break;
        }
    }

	public enum ROLE_ICON_TYPE
	{
		NONE,
		CIRCLE,		//> 圆头像
		BATTLE,		//> 战斗头像
		PHOTO,		//>带底头像

	}
	static public void SetRoleIcon(UISprite sprite,int tId,ROLE_ICON_TYPE type)
	{
        if (DataCenter.mRoleSkinConfig == null)
            return;
		DataRecord _roleRecord = DataCenter.mRoleSkinConfig.GetRecord(tId);
        if (_roleRecord == null) 
        {
            DEBUG.LogError("设置角色头像出错，tid：" + tId);
            return;
        }
		string _atlasName = string.Empty;
		string _spriteName = string.Empty;
		switch (type) 
		{
		case ROLE_ICON_TYPE.CIRCLE:
			_atlasName = _roleRecord.getObject ("ICON_ATLAS").ToString();
			_spriteName = _roleRecord.getObject("ICON_SPRITE").ToString();
			break;
		case ROLE_ICON_TYPE.BATTLE:
			_atlasName = _roleRecord.getObject ("ATLAS").ToString();
			_spriteName = _roleRecord.getObject("SPRITE").ToString();
			break;
		case ROLE_ICON_TYPE.PHOTO:
			_atlasName = TableCommon.GetStringFromActiveCongfig (tId, "HEAD_ATLAS_NAME");
			_spriteName = TableCommon.GetStringFromActiveCongfig (tId, "HEAD_SPRITE_NAME");
			break;
		default:
			break;
		}
		SetIcon (sprite,_atlasName,_spriteName);
	}

	static public void SetIcon(UISprite sprite, string atlasName, string spriteName)
	{
		if(sprite != null)
		{
			UIAtlas atlas = LoadUIAtlas(atlasName);
			sprite.atlas = atlas;
			sprite.spriteName = spriteName;
		}
	}

	static public void SetIcon(GameObject parentObj, string iconName, string strSpriteName, string atlasName)
	{
		string strAtlasName = atlasName;
		
		UIAtlas tu = GameCommon.LoadUIAtlas(strAtlasName);
        
        GameObject obj = GameCommon.FindObject (parentObj, iconName);
        if(obj != null)
        {
			UISprite icon = obj.GetComponent<UISprite>();

			SetIcon(icon, atlasName, strSpriteName);
        }
	}

    static public NiceData GetButtonData(GameObject winObject, string buttonName)
    {
		GameObject obj;
        if (winObject == null)
            obj = FindUI(buttonName);
        else
            obj = FindObject(winObject, buttonName);

        if (obj != null)
        {
            UIButtonEvent butEvt = obj.GetComponent<UIButtonEvent>();
            if (butEvt != null)
                return butEvt.mData;
        }
        return null;
    }

    static public NiceData GetButtonData(GameObject buttonObj)
    {
		if(buttonObj == null)
			return null;
        UIButtonEvent butEvt = buttonObj.GetComponent<UIButtonEvent>();
        if (butEvt != null)
            return butEvt.mData;
        return null;
    }

    static public UIImageButton GetUIButton(GameObject winObject, string buttonName)
    {
        GameObject obj;
        if (winObject == null)
            obj = FindUI(buttonName);
        else
            obj = FindObject(winObject, buttonName);

        if (obj != null)
        {
            return obj.GetComponent<UIImageButton>();
        }
        return null;
    }

    static public string MakeMD5(string scrString)
    {
        System.Security.Cryptography.MD5CryptoServiceProvider md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
        byte[] bytValue, bytHash;
        bytValue = StaticDefine.GetBytes(scrString);
        bytHash = md5.ComputeHash(bytValue);
        md5.Clear();

        string result = "";
        for (int i = 0; i < bytHash.Length; i++)
        {
            result += bytHash[i].ToString("X").PadLeft(2, '0');
        }
        //return result.ToLower();
		return scrString;
    }

    static public string MakeMD5(byte[] scrData, int size)
    {
        System.Security.Cryptography.MD5CryptoServiceProvider md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
        byte[] bytHash;
        
        bytHash = md5.ComputeHash(scrData, 0, size);
        md5.Clear();

        string result = "";
        for (int i = 0; i < bytHash.Length; i++)
        {
            result += bytHash[i].ToString("X").PadLeft(2, '0');
        }
        return result.ToLower();
    }

    static public void SetObjectTexture(GameObject obj, Texture tex)
    {
        if (obj == null)
            return;

        Renderer[] rend = obj.GetComponentsInChildren<Renderer>();
        foreach (Renderer mR in rend)
        {
            mR.material.mainTexture = tex;
        }
    }

	static public void SetCardInfo(string strName, int iModelIndex, int iLevel, int iStrengthenLevel, GameObject obj)
	{
		DataCenter.SetData("CardGroupWindow" + strName, "SET_SELECT_PET_COLLIDER_UI", obj);
		SetCardInfo(strName, iModelIndex, iLevel, iStrengthenLevel, true);
	}

	static public void SetCardInfo(string strName, int iModelIndex, int iLevel, int iStrengthenLevel, bool bIsCanOperate)
	{
		DataCenter.SetData("CardGroupWindow" + strName, "SET_SELECT_PET_OPERATE_STATE", bIsCanOperate);
		DataCenter.SetData("CardGroupWindow" + strName, "SET_SELECT_PET_LEVEL", iLevel);
		DataCenter.SetData("CardGroupWindow" + strName, "SET_SELECT_PET_STRENGTHEN_LEVEL", iStrengthenLevel);
		DataCenter.SetData("CardGroupWindow" + strName, "SET_SELECT_PET_BY_MODEL_INDEX", iModelIndex);
	}

    //根据符灵tid设置符灵的元素类型名称
    static public UILabel SetPetElementName(GameObject kParent,string kLblName,int kPetTid) 
    {
        int iElement = TableCommon.GetNumberFromActiveCongfig(kPetTid, "ELEMENT_INDEX");
        UILabel _elementLbl = GameCommon.FindComponent<UILabel>(kParent,kLblName);
        if (_elementLbl != null) 
        {
            _elementLbl.text = TableCommon.GetStringFromElement(iElement, "ELEMENT_NAME");
        }
        return _elementLbl;
    }
    //根据符灵tid设置符灵的元素类型图标
    static public UISprite SetPetElementIcon(GameObject kParent, string kIconName, int kPetTid) 
    {
        int iElement = TableCommon.GetNumberFromActiveCongfig(kPetTid, "ELEMENT_INDEX");
        UISprite _elementSprite = GameCommon.FindComponent<UISprite>(kParent, kIconName);
        if (_elementSprite != null) 
        {
            string strAtlasName = TableCommon.GetStringFromElement(iElement, "ELEMENT_ATLAS_NAME");
            UIAtlas tu = GameCommon.LoadUIAtlas(strAtlasName);
            string strSpriteName = TableCommon.GetStringFromElement(iElement, "ELEMENT_SPRITE_NAME");

            _elementSprite.atlas = tu;
            _elementSprite.spriteName = strSpriteName;
        }
        return _elementSprite;
    }
    //根据符灵tid设置符灵的攻击类型
    static public UILabel SetPetTypeName(GameObject kParent,string kLblName,int kPetTid) 
    {
        UILabel _attackTypeLbl = GameCommon.FindComponent<UILabel>(kParent,kLblName);
        if (_attackTypeLbl != null) 
        {
            _attackTypeLbl.text = GameCommon.GetAttackType(kPetTid);  
        }
        return _attackTypeLbl;
    }
    //根据符灵tid设置符灵攻击类型图标
    static public UISprite SetPetTypeIcon(GameObject kParent, string kIconName, int kPetTid) 
    {
        UISprite _attackTypeSprite = GameCommon.FindComponent<UISprite>(kParent, kIconName);
        if (_attackTypeSprite != null) 
        {
            string strAtlasName = GameCommon.GetAttackType(kPetTid, "TPYE_ATLAS_NAME");
            UIAtlas tu = GameCommon.LoadUIAtlas(strAtlasName);
            string strSpriteName = GameCommon.GetAttackType(kPetTid, "TYPE_SPRITE_NAME");
            _attackTypeSprite.atlas = tu;
            _attackTypeSprite.spriteName = strSpriteName;
        }
        return _attackTypeSprite;       
    }

    static GameObject msDebugInfoObject;
    static public void ShowDebugInfo(float screenX, float screenY, string debugInfo)
    {
        DEBUG.LogError("DEBUG >" + debugInfo);
        return;

        if (msDebugInfoObject == null)
        {
            GameObject uiCamera = FindUI("Camera");
            msDebugInfoObject = LoadUIPrefabs("prefabs/" + "DebugInfo", uiCamera);
            msDebugInfoObject.name = "_debug_info_";
			msDebugInfoObject.transform.localPosition = Vector3.zero;
            msDebugInfoObject.transform.localScale = Vector3.one;
        }
        msDebugInfoObject.GetComponentInChildren<UILabel>().text = debugInfo;
		//msDebugInfoObject.transform.position = new Vector3(screenX, screenY, 0);
    }

    public static ELEMENT_RELATION GetElementRelation(int iSrcElement, int iAimElement)
    {
        if (iSrcElement < (int)ELEMENT_TYPE.RED || iSrcElement >= (int)ELEMENT_TYPE.MAX
            || iAimElement < (int)ELEMENT_TYPE.RED || iAimElement >= (int)ELEMENT_TYPE.MAX)
            return ELEMENT_RELATION.BLANCE;

        if (iSrcElement <= (int)ELEMENT_TYPE.GREEN && iAimElement <= (int)ELEMENT_TYPE.GREEN)
        {
            if ((iSrcElement + 3 - iAimElement) % 3 == 1)
            {
                return ELEMENT_RELATION.ADVANTAGEOUS;
            }
            if ((iAimElement + 3 - iSrcElement) % 3 == 1)
            {
                return ELEMENT_RELATION.INFERIOR;
            }
        }

        if (iSrcElement >= (int)ELEMENT_TYPE.GOLD && iAimElement >= (int)ELEMENT_TYPE.GOLD)
        {
            if (iAimElement != iSrcElement)
                return ELEMENT_RELATION.ADVANTAGEOUS;
        }

        return ELEMENT_RELATION.BLANCE;
    }

	static public int GetActiveObjectElement(DataRecord record)
	{
		return GetActiveObjectElement(record, false);
	}

	static public int GetActiveObjectElement(DataRecord record, bool bIsPvpOpponent)
    {
        string type = record.get("CLASS");

        if (type == "CHAR")
        {
			string strRoleEquipData = "EQUIP_DATA";
			if(bIsPvpOpponent)
			{
				strRoleEquipData = "";
				return record.get("ELEMENT_INDEX");
			}

            RoleEquipLogicData roleEquipLogicData = DataCenter.GetData("EQUIP_DATA") as RoleEquipLogicData;

            if (roleEquipLogicData != null)
            {
                EquipData curUseEquip = roleEquipLogicData.GetUseEquip();
                return curUseEquip == null ? record.get("ELEMENT_INDEX") : (int)curUseEquip.mElementType;
            }

            return record.get("ELEMENT_INDEX");
        }
        else
        {
            return record.get("ELEMENT_INDEX");
        }
    }

	static public ELEMENT_RELATION GetElmentRaletion(DataRecord srcDataRecord, DataRecord aimDataRecord)
	{
		return GetElmentRaletion(srcDataRecord, false, aimDataRecord, false);
	}

	static public ELEMENT_RELATION GetElmentRaletion(DataRecord srcDataRecord, bool srcIsPvpOpponent, DataRecord aimDataRecord, bool aimIsPvpOpponent)
	{
		if(srcDataRecord == null && aimDataRecord == null)
			return ELEMENT_RELATION.BLANCE;

		int iSrcElement = GetActiveObjectElement(srcDataRecord, srcIsPvpOpponent);
		int iAimElement = GetActiveObjectElement(aimDataRecord, aimIsPvpOpponent);

        return GetElementRelation(iSrcElement, iAimElement);
	}

    //static public float GetElmentRaletionRatio(DataRecord srcDataRecord, bool srcIsPvpOpponent, DataRecord aimDataRecord, bool aimIsPvpOpponent)
    //{
    //    float fRatio = 1.0f;
    //    ELEMENT_RELATION elementRelation = GameCommon.GetElmentRaletion(srcDataRecord, srcIsPvpOpponent, aimDataRecord, aimIsPvpOpponent);
    //    if(elementRelation == ELEMENT_RELATION.ADVANTAGEOUS)
    //    {
    //        fRatio *= (1.0f + GetConfig("ELEMENT_RATIO"));
    //    }

    //    return fRatio;
    //}

    // Note: result Y default zero
    static public Vector3 RandInCircularGround(Vector3 originPosition, float rangeRadius)
    {
		float rY = UnityEngine.Random.Range(0, 360);
        Quaternion ra = Quaternion.Euler(0, rY, 0);
		Vector3 result = ra * (Vector3.forward * UnityEngine.Random.Range(0, rangeRadius));
        return originPosition + result;
    }

    // Note: result Y default zero
    static public Vector3 RandInRectGround(Vector3 leftTop, Vector3 rightBotton)
    {
		return new Vector3(UnityEngine.Random.Range(leftTop.x, rightBotton.x), 0, UnityEngine.Random.Range(leftTop.z, rightBotton.z));
    }

    static public int SplitToInt(string scrString, char splitFlag, out int[] result)
    {
        char[] p = { splitFlag };

        if (scrString=="")
        {
            result = null;
            return 0;
        }

        string[] x = scrString.Split(p);
        result = new int[x.Length];
        for(int i=0; i<x.Length; ++i)
        {
            try{
                result[i] = int.Parse(x[i]);
            }
            catch
            {
                result[i] = 0;
            }
        }
        return result.Length;
    }

    static public bool SplitToVector(string scrString, out Vector3 resultVec)
    {
        char[] p = { ' ' };

        if (scrString == "")
        {
			resultVec = Vector3.zero;
            return false;
        }

        string[] x = scrString.Split(p);

        if (x.Length >= 3)
        {
            resultVec = new Vector3();
            resultVec.x = float.Parse(x[0]);
            resultVec.y = float.Parse(x[1]);
            resultVec.z = float.Parse(x[2]);

            return true;
        }
		resultVec = Vector3.zero;
        return false;
    }

    static public Vector2 GetRowAndColumnByIndex(int iIndex, int iMaxColumn)
    {
        int iRow = iIndex / iMaxColumn;
        int iColumn = iIndex % iMaxColumn;
        return new Vector2(iRow, iColumn);
    }

    static public Vector3 GetPostion(int iPosX0, int iPosY0, int iDX, int iDY, int iWidth, int iHeight, Vector2 vec)
    {
        int iPosX = iPosX0 + (iDX + iWidth) * (int)vec.y;
        int iPosY = (iPosY0 + (iDY + iHeight) * (int)vec.x) * (-1);
        return new Vector3(iPosX, iPosY, 0);
    }

    static public Vector3 GetPostion(int iPosX0, int iPosY0, int iDX, int iDY, int iWidth, int iHeight, int iIndex, int iMaxColumn)
    {
        Vector2 vec = GameCommon.GetRowAndColumnByIndex(iIndex, iMaxColumn);
        return GetPostion(iPosX0, iPosY0, iDX, iDY, iWidth, iHeight, vec);
    }

    static public bool bIsLogicDataExist(string strName)
    {
        object obj;
        return DataCenter.Self.getData(strName, out obj);
    }

    static public bool bIsWindowOpen(string strWindowName)
    {
        object obj;
        DataCenter.Self.getData(strWindowName, out obj);

        if (obj != null)
        {
            tWindow win = obj as tWindow;

            if (win != null)
            {
                return win.mGameObjUI != null && win.mGameObjUI.activeSelf;
            }
        }

        return false;
    }

    static public bool SetWindowData(string strWindowName, string keyIndex, object objVal)
    {
        bool bIsExist = bIsLogicDataExist(strWindowName);
        if (bIsExist)
            DataCenter.SetData(strWindowName, keyIndex, objVal);

        return bIsExist;
    }

    static public void ToggleTrue(GameObject obj)
    {
        if (obj != null)
        {
            UIToggle toggle = obj.GetComponent<UIToggle>();
            if(toggle != null)
                toggle.value = true;
        }
    }

    static public void ToggleFalse(GameObject obj)
    {
        if (obj != null)
        {
            UIToggle toggle = obj.GetComponent<UIToggle>();
            if (toggle != null)
                toggle.value = false;
        }
    }

    //---------------------------------------------------------------------------------
    // attribute type
    static public AFFECT_TYPE ToAffectTypeEnum(string strEnumName)
    {
        return GetEnumFromString<AFFECT_TYPE>(strEnumName, AFFECT_TYPE.NONE);
        //switch (strEnumName)
        //{
        //    case "ATTACK":
        //        return AFFECT_TYPE.ATTACK;

        //    case "ATTACK_RATE":
        //        return AFFECT_TYPE.ATTACK_RATE;

        //    case "DEFENCE":
        //        return AFFECT_TYPE.DEFENCE;

        //    case "DEFENCE_RATE":
        //        return AFFECT_TYPE.DEFENCE_RATE;

        //    case "HP":
        //        return AFFECT_TYPE.HP;

        //    case "HP_RATE":
        //        return AFFECT_TYPE.HP_RATE;

        //    case "HP_MAX":
        //        return AFFECT_TYPE.HP_MAX;

        //    case "HP_MAX_RATE":
        //        return AFFECT_TYPE.HP_MAX_RATE;

        //    case "MP":
        //        return AFFECT_TYPE.MP;

        //    case "MP_RATE":
        //        return AFFECT_TYPE.MP_RATE;

        //    case "MP_MAX":
        //        return AFFECT_TYPE.MP_MAX;

        //    case "MP_MAX_RATE":
        //        return AFFECT_TYPE.MP_MAX_RATE;

        //    //case "HIT_TARGET":
        //    //    return AFFECT_TYPE.HIT_TARGET;

        //    case "HIT_TARGET_RATE":
        //        return AFFECT_TYPE.HIT_TARGET_RATE;

        //    //case "CRITICAL_STRIKE":
        //    //    return AFFECT_TYPE.CRITICAL_STRIKE;

        //    case "CRITICAL_STRIKE_RATE":
        //        return AFFECT_TYPE.CRITICAL_STRIKE_RATE;

        //    //case "HIT_CRITICAL_STRIKE_RATE":
        //    //    return AFFECT_TYPE.HIT_CRITICAL_STRIKE_RATE;

        //    //case "DEFENCE_CHARM_RATE":
        //    //    return AFFECT_TYPE.DEFENCE_CHARM_RATE;

        //    //case "DEFENCE_FEAR_RATE":
        //    //    return AFFECT_TYPE.DEFENCE_FEAR_RATE;

        //    case "DEFENCE_WOOZY_RATE":
        //        return AFFECT_TYPE.DEFENCE_WOOZY_RATE;

        //    case "DEFENCE_ICE_RATE":
        //        return AFFECT_TYPE.DEFENCE_ICE_RATE;

        //    case "DEFENCE_BEATBACK_RATE":
        //        return AFFECT_TYPE.DEFENCE_BEATBACK_RATE;

        //    case "DEFENCE_DOWN_RATE":
        //        return AFFECT_TYPE.DEFENCE_DOWN_RATE;

        //    case "DEFENCE_HIT_RATE":
        //        return AFFECT_TYPE.DEFENCE_HIT_RATE;

        //    case "MOVE_SPEED":
        //        return AFFECT_TYPE.MOVE_SPEED;

        //    case "MOVE_SPEED_RATE":
        //        return AFFECT_TYPE.MOVE_SPEED_RATE;

        //    case "ATTACK_SPEED":
        //        return AFFECT_TYPE.ATTACK_SPEED;

        //    case "ATTACK_SPEED_RATE":
        //        return AFFECT_TYPE.ATTACK_SPEED_RATE;

        //    case "DODGE":
        //        return AFFECT_TYPE.DODGE;

        //    case "DODGE_RATE":
        //        return AFFECT_TYPE.DODGE_RATE;

        //    //case "DEFENCE_CRITICAL_DAMAGE_RATE":
        //    //    return AFFECT_TYPE.DEFENCE_CRITICAL_DAMAGE_RATE;

        //    case "RED_REDUCTION_RATE":
        //        return AFFECT_TYPE.RED_REDUCTION_RATE;

        //    case "GREEN_REDUCTION_RATE":
        //        return AFFECT_TYPE.GREEN_REDUCTION_RATE;

        //    case "BLUE_REDUCTION_RATE":
        //        return AFFECT_TYPE.BLUE_REDUCTION_RATE;

        //    case "SHADOW_REDUCTION_RATE":
        //        return AFFECT_TYPE.SHADOW_REDUCTION_RATE;

        //    case "GOLD_REDUCTION_RATE":
        //        return AFFECT_TYPE.GOLD_REDUCTION_RATE;

        //    case "PHYSICAL_DEFENCE":
        //        return AFFECT_TYPE.PHYSICAL_DEFENCE;

        //    case "MAGIC_DEFENCE":
        //        return AFFECT_TYPE.MAGIC_DEFENCE;

        //    case "PHYSICAL_DEFENCE_RATE":
        //        return AFFECT_TYPE.PHYSICAL_DEFENCE_RATE;

        //    case "MAGIC_DEFENCE_RATE":
        //        return AFFECT_TYPE.MAGIC_DEFENCE_RATE;

        //default:
        //        return AFFECT_TYPE.NONE;
        //}
    }

    static public string ToAffectTypeString(AFFECT_TYPE affectType)
    {
        return affectType.ToString();
        //switch (affectType)
        //{
        //    case AFFECT_TYPE.ATTACK:
        //        return "ATTACK";

        //    case AFFECT_TYPE.ATTACK_RATE:
        //        return "ATTACK_RATE";

        //    case AFFECT_TYPE.DEFENCE:
        //        return "DEFENCE";

        //    case AFFECT_TYPE.DEFENCE_RATE:
        //        return"DEFENCE_RATE";

        //    case AFFECT_TYPE.HP:
        //        return "HP";

        //    case AFFECT_TYPE.HP_RATE:
        //        return "HP_RATE";

        //    case AFFECT_TYPE.HP_MAX:
        //        return "HP_MAX";

        //    case AFFECT_TYPE.HP_MAX_RATE:
        //        return "HP_MAX_RATE";

        //    case AFFECT_TYPE.MP:
        //        return "MP";

        //    case AFFECT_TYPE.MP_RATE:
        //        return "MP_RATE";

        //    case AFFECT_TYPE.MP_MAX:
        //        return "MP_MAX";

        //    case AFFECT_TYPE.MP_MAX_RATE:
        //        return "MP_MAX_RATE";

        //    //case AFFECT_TYPE.HIT_TARGET:
        //    //    return "HIT_TARGET";

        //    case AFFECT_TYPE.HIT_TARGET_RATE:
        //        return "HIT_TARGET_RATE";

        //    //case AFFECT_TYPE.CRITICAL_STRIKE:
        //    //    return "CRITICAL_STRIKE";

        //    case AFFECT_TYPE.CRITICAL_STRIKE_RATE:
        //        return "CRITICAL_STRIKE_RATE";

        //    //case AFFECT_TYPE.HIT_CRITICAL_STRIKE_RATE:
        //    //    return "HIT_CRITICAL_STRIKE_RATE";

        //    //case AFFECT_TYPE.DEFENCE_CHARM_RATE:
        //    //    return "DEFENCE_CHARM_RATE";

        //    //case AFFECT_TYPE.DEFENCE_FEAR_RATE:
        //    //    return "DEFENCE_FEAR_RATE";

        //    case AFFECT_TYPE.DEFENCE_WOOZY_RATE:
        //        return "DEFENCE_WOOZY_RATE";

        //    case AFFECT_TYPE.DEFENCE_ICE_RATE:
        //        return "DEFENCE_ICE_RATE";

        //    case AFFECT_TYPE.DEFENCE_BEATBACK_RATE:
        //        return "DEFENCE_BEATBACK_RATE";

        //    case AFFECT_TYPE.DEFENCE_DOWN_RATE:
        //        return "DEFENCE_DOWN_RATE";

        //    case AFFECT_TYPE.DEFENCE_HIT_RATE:
        //        return "DEFENCE_HIT_RATE";

        //    case AFFECT_TYPE.MOVE_SPEED:
        //        return "MOVE_SPEED";

        //    case AFFECT_TYPE.MOVE_SPEED_RATE:
        //        return "MOVE_SPEED_RATE";

        //    case AFFECT_TYPE.ATTACK_SPEED:
        //        return "ATTACK_SPEED";

        //    case AFFECT_TYPE.ATTACK_SPEED_RATE:
        //        return "ATTACK_SPEED_RATE";

        //    case AFFECT_TYPE.DODGE:
        //        return "DODGE";

        //    case AFFECT_TYPE.DODGE_RATE:
        //        return "DODGE_RATE";

        //    //case AFFECT_TYPE.DEFENCE_CRITICAL_DAMAGE_RATE:
        //    //    return "DEFENCE_CRITICAL_DAMAGE_RATE";

        //    case AFFECT_TYPE.RED_REDUCTION_RATE:
        //        return "RED_REDUCTION_RATE";

        //    case AFFECT_TYPE.GREEN_REDUCTION_RATE:
        //        return "GREEN_REDUCTION_RATE";

        //    case AFFECT_TYPE.BLUE_REDUCTION_RATE:
        //        return "BLUE_REDUCTION_RATE";

        //    case AFFECT_TYPE.SHADOW_REDUCTION_RATE:
        //        return "SHADOW_REDUCTION_RATE";

        //    case AFFECT_TYPE.GOLD_REDUCTION_RATE:
        //        return "GOLD_REDUCTION_RATE";

        //    case AFFECT_TYPE.PHYSICAL_DEFENCE:
        //        return "PHYSICAL_DEFENCE";

        //    case AFFECT_TYPE.MAGIC_DEFENCE:
        //        return "MAGIC_DEFENCE";

        //    case AFFECT_TYPE.PHYSICAL_DEFENCE_RATE:
        //        return "PHYSICAL_DEFENCE_RATE";

        //    case AFFECT_TYPE.MAGIC_DEFENCE_RATE:
        //        return "MAGIC_DEFENCE_RATE";

        //default:
        //        return "NONE";
        //}
    }

    static public bool IsAffectTypeRate(AFFECT_TYPE affectType)
    {
        string strType = GameCommon.ToAffectTypeString(affectType);
        return strType.LastIndexOf("_RATE") > 0;
    }

    static public AFFECT_TYPE GetAffactType(AFFECT_TYPE affectType)
    {
        string strType = ToAffectTypeString(affectType);
        if (strType.LastIndexOf("_RATE") > 0)
        {
            strType.Replace("_RATE", "");
        }

        return ToAffectTypeEnum(strType);
    }

    static public AFFECT_TYPE GetAffactRateType(AFFECT_TYPE affectType)
    {
        string strType = ToAffectTypeString(affectType);
        if (strType.LastIndexOf("_RATE") <= 0)
        {
            strType = strType + "_RATE";
        }

        return ToAffectTypeEnum(strType);
    }

	//---------------------------------------------------------------------------------
	// set pet icon
	static public void SetPetIcon(GameObject obj, int iModelIndex)
	{
		SetPetIcon(obj, iModelIndex, "Background");
	}

	static public void SetPetIcon(GameObject obj, int iModelIndex, string strObjName)
	{
		string strAtlasName = "CommonUIAtlas";
		string strSpriteName = "ui_background_sz";
        if (iModelIndex > 0)
        {
            // use pet
            strAtlasName = TableCommon.GetStringFromActiveCongfig(iModelIndex, "HEAD_ATLAS_NAME");
            strSpriteName = TableCommon.GetStringFromActiveCongfig(iModelIndex, "HEAD_SPRITE_NAME");
        }

        SetIcon(obj, strObjName, strSpriteName, strAtlasName);
	}
	
	// set role equip icon
	static public void SetEquipIcon(GameObject obj, int iModelIndex)
	{
		SetEquipIcon(obj, iModelIndex, "icon_sprite");
	}

	static public void SetEquipIcon(GameObject obj, int iModelIndex, string strObjName)
	{
        string strAtlasName = TableCommon.GetStringFromRoleEquipConfig(iModelIndex, "ICON_ATLAS_NAME");
        string strSpriteName = TableCommon.GetStringFromRoleEquipConfig(iModelIndex, "ICON_SPRITE_NAME");

        SetIcon(obj, strObjName, strSpriteName, strAtlasName);
	}
	
	// set pet equip icon
	static public void SetPetEquipIcon(GameObject obj, int iModelIndex)
	{
		SetPetEquipIcon(obj, iModelIndex, "icon_sprite");
	}

	static public void SetPetEquipIcon(GameObject obj, int iModelIndex, string strObjName)
	{
        string strAtlasName = TableCommon.GetStringFromRoleEquipConfig(iModelIndex, "ICON_ATLAS_NAME");
        string strSpriteName = TableCommon.GetStringFromRoleEquipConfig(iModelIndex, "ICON_SPRITE_NAME");

        SetIcon(obj, strObjName, strSpriteName, strAtlasName);
	}

	// set element icon
	static public void SetElementIcon(GameObject obj, int iIndex)
	{
		SetElementIcon(obj, "Element", iIndex);
	}
	
	static public void SetElementIcon(GameObject obj, string elementObject, int iIndex)
	{
		string strAtlasName = TableCommon.GetStringFromElement(iIndex, "ELEMENT_ATLAS_NAME");
		string strSpriteName = TableCommon.GetStringFromElement(iIndex, "ELEMENT_SPRITE_NAME");
		
		SetIcon(obj, elementObject, strSpriteName, strAtlasName);
	}

    // set role equip element icon
    static public void SetRoleEquipElementIcon(GameObject obj, int iIndex)
	{
        SetRoleEquipElementIcon(obj, "Element", iIndex);
	}
    static public void SetRoleEquipElementIcon(GameObject obj, string elementObject, int iIndex)
    {
        string strAtlasName = TableCommon.GetStringFromElement(iIndex, "EQUIP_ELEMENT_ATLAS_NAME");
        string strSpriteName = TableCommon.GetStringFromElement(iIndex, "EQUIP_ELEMENT_SPRITE_NAME");

        SetIcon(obj, elementObject, strSpriteName, strAtlasName);
    }

	// set element fragement icon
	static public void SetElementFragmentIcon(GameObject obj, int iIndex)
	{
		SetElementFragmentIcon(obj, "sprite", iIndex);
	}

    static public void SetElementFragmentIcon(GameObject obj, string elementObject, int iIndex)
    {
        string strAtlasName = TableCommon.GetStringFromElement(iIndex, "FRAGMENT_ATLAS_NAME");
        string strSpriteName = TableCommon.GetStringFromElement(iIndex, "FRAGMENT_SPRITE_NAME");

        SetIcon(obj, elementObject, strSpriteName, strAtlasName);
    }

    static public void SetElementBackground(GameObject obj, string backgroundObject, int iIndex)
    {
        string strAtlasName = TableCommon.GetStringFromElement(iIndex, "ELEMENT_ATLAS_NAME");
        string strSpriteName = TableCommon.GetStringFromElement(iIndex, "ELEMENT_ICON_SPRITE_NAME");

        SetIcon(obj, backgroundObject, strSpriteName, strAtlasName);
    }

	// set equip element background icon
	static public void SetEquipElementBgIcon(GameObject obj, int iIndex)
	{
		SetEquipElementBgIcon(obj, "background_sprite", iIndex);
	}

	static public void SetEquipElementBgIcon(GameObject obj, string elementObject, int iIndex)
    {
		string strAtlasName = TableCommon.GetStringFromElement(iIndex, "ELEMENT_ICON_ATLAS_NAME");
		string strSpriteName = TableCommon.GetStringFromElement(iIndex, "ELEMENT_ICON_SPRITE_NAME");

        SetIcon(obj, elementObject, strSpriteName, strAtlasName);
    }
	
	static public void SetEquipElementBgIcons(GameObject obj, int iIndex, int iElementIndex)
	{
		SetEquipElementBgIcons(obj, "Element", "background_sprite", iIndex, iElementIndex);
	}

	static public void SetEquipElementBgIcons(GameObject obj, string elementObject, string backgroundObject, int iIndex, int iElementIndex)
	{
		SetEquipElementIcon(obj, elementObject, iIndex, iElementIndex);
		SetEquipElementBgIcon(obj, backgroundObject, iIndex, iElementIndex);
	}

	static public void SetEquipElementBgIcon(GameObject obj, string backgroundObject, int iIndex, int iElementIndex)
	{
		if(TableCommon.GetNumberFromRoleEquipConfig(iIndex, "ROLEEQUIP_TYPE") == (int)EQUIP_TYPE.ELEMENT_EQUIP)
		{
			// set equip element background icon
			GameCommon.SetEquipElementBgIcon(obj, backgroundObject, iElementIndex);
		}
		else
		{
			GameCommon.SetIcon(obj, backgroundObject, UICommonDefine.strEquipIconBackgroundSprite, UICommonDefine.strEquipIconBackgroundAtlas);
		}
	}

	static public void SetEquipElementIcon(GameObject obj, string elementObject, int iIndex, int iElementIndex)
	{
		if(TableCommon.GetNumberFromRoleEquipConfig(iIndex, "ROLEEQUIP_TYPE") == (int)EQUIP_TYPE.ELEMENT_EQUIP)
		{
			// set element icon
            GameCommon.SetRoleEquipElementIcon(obj, elementObject, iElementIndex);
		}
		else
		{
			GameCommon.SetIcon(obj, elementObject, "", "");
		}
	}

	// set level label
	static public void SetLevelLabel(GameObject obj, int iValue)
	{
		SetLevelLabel(obj, iValue, "LevelLabel");
	}

	static public void SetLevelLabel(GameObject obj, int iValue, string strObjName)
	{
		GameObject levelLabelObj = GameCommon.FindObject(obj, strObjName);
		UILabel label = levelLabelObj.GetComponent<UILabel>();
		label.text = "Lv." + iValue.ToString();
		//label.text = iValue.ToString();
	}

    // set star level label
    static public void SetStarLevelLabel(GameObject obj, int iValue)
    {       
		SetStarLevelLabel(obj, iValue, "star_level_label");
    }

	static public void SetStarLevelLabel(GameObject obj, int iValue, string strObjName)
	{
        // 由于采用新布局，出于兼容考虑，此处逻辑修改为：若存在grid则采用grid布局，否则采用label布局
        // 如果采用grid布局，请手动隐藏或删除相应label布局控件
        GameObject grid = GameCommon.FindObject(obj, "stars_grid");

        if (grid != null)
        {
            Vector3 pos = grid.transform.localPosition;
            UIGridContainer container = grid.GetComponent<UIGridContainer>();
            container.MaxCount = iValue;
            grid.transform.localPosition = new Vector3((1 - iValue) * container.CellWidth / 2f, pos.y, pos.z);
        }
        else
        {
            UILabel starLevelLabel = GameCommon.FindComponent<UILabel>(obj, strObjName);
            if (starLevelLabel != null)
                starLevelLabel.text = iValue.ToString();
        }
	}
	
	// set strengthen level label
    static public void SetStrengthenLevelLabel(GameObject obj, int iValue)
    {
		SetStrengthenLevelLabel(obj, iValue, "StrengthenLevelLabel");
    }

	static public void SetStrengthenLevelLabel(GameObject obj, int iValue, string strObjName)
	{
		GameObject numLabelObj = GameCommon.FindObject(obj, strObjName);	
		UILabel label = numLabelObj.GetComponent<UILabel>();

        // 由于新布局下label有阴影背景，因此无强化时改用隐藏label而非设置空字符串的方式
        if (iValue > 0)
        {
            numLabelObj.SetActive(true);
            label.text = "+" + iValue.ToString();
            return;
        }
        else 
        {
            numLabelObj.SetActive(false);
        }
	}
	
	//---------------------------------------------------------------------------------------
	// role change glod
	static public void RoleChangeGold(int iGoldNum)
	{
		RoleLogicData logicData = RoleLogicData.Self;
		if(logicData != null)
			logicData.AddGold(iGoldNum);
	}

    // role change stamina
	static public void RoleChangeStamina(int iStamina)
	{
		RoleLogicData logicData = RoleLogicData.Self;
		if(logicData != null)
			logicData.AddStamina(iStamina);
	}

	// role change diamond
	static public void RoleChangeDiamond(int iDiamond)
	{
		RoleLogicData logicData = RoleLogicData.Self;
		if(logicData != null)
			logicData.AddDiamond(iDiamond);
	}
    
    // role change spirit
	static public void RoleChangeSpirit(int iSpirit)
	{
		RoleLogicData logicData = RoleLogicData.Self;
		if(logicData != null)
			logicData.AddSpirit(iSpirit);
	}

    // role change soulPoint
    static public void RoleChangeSoulPoint(int iSoulPoint)
    {
        RoleLogicData logicData = RoleLogicData.Self;
        if (logicData != null)
            logicData.AddSoulPoint(iSoulPoint);
    }

    // role change reputation
    static public void RoleChangeReputation(int iReputation)
    {
        RoleLogicData logicData = RoleLogicData.Self;
        if (logicData != null)
            logicData.AddReputation(iReputation);
    }

    // role change prestige
    static public void RoleChangePrestige(int iPrestige)
    {
        RoleLogicData logicData = RoleLogicData.Self;
        if (logicData != null)
            logicData.AddPrestige(iPrestige);
    }

    // role change battleAchv
    static public void RoleChangeBattleAchv(int iBattleAchv)
    {
        RoleLogicData logicData = RoleLogicData.Self;
        if (logicData != null)
            logicData.AddBattleAchv(iBattleAchv);
    }

    // role change unionContr
    static public void RoleChangeUnionContr(int iUnionContr)
    {
        RoleLogicData logicData = RoleLogicData.Self;
        if (logicData != null)
            logicData.AddUnionContr(iUnionContr);
    }

    // role change beatDemonCard
    static public void RoleChangeBeatDemonCard(int iBeatDemonCard)
    {
        RoleLogicData logicData = RoleLogicData.Self;
        if (logicData != null)
            logicData.AddBeatDemonCard(iBeatDemonCard);
    }


   









    // role add mail
    static public void RoleChangeMail(int iMail)
    {
        RoleLogicData logicData = RoleLogicData.Self;
        if (logicData != null)
			logicData.AddMailNum (iMail);
    }

	//role functional prop
	static public void RoleChangeFunctionalProp(ITEM_TYPE type, int count)
	{
		RoleLogicData logicData = RoleLogicData.Self;
		if(logicData != null)
			logicData.AddFunctionalProp (type, count);
	}

	static public void RoleChangeFunctionalProp(int index, int count)
	{
		ITEM_TYPE type = ITEM_TYPE.SAODANG_POINT;
		if(index == 1001)
			type = ITEM_TYPE.RESET_POINT;
		else if(index == 1002)
			type = ITEM_TYPE.SAODANG_POINT;
		else if(index == 1003)
			type = ITEM_TYPE.LOCK_POINT;
		else 
			return;

		RoleChangeFunctionalProp(type, count);
	}

	static public void RoleChangeNumericalAboutRole(int iItemType, int iItemCount)
	{
		switch(iItemType)
		{
            case (int)ITEM_TYPE.YUANBAO:
                GameCommon.RoleChangeDiamond(iItemCount);
                break;
            case (int)ITEM_TYPE.GOLD:
                GameCommon.RoleChangeGold(iItemCount);
                break;
            case (int)ITEM_TYPE.POWER:
                GameCommon.RoleChangeStamina(iItemCount);
                break;
            case (int)ITEM_TYPE.PET_SOUL:
                GameCommon.RoleChangeSoulPoint(iItemCount);
                break;
            case (int)ITEM_TYPE.SPIRIT:
                GameCommon.RoleChangeSpirit(iItemCount);
                break;
            case (int)ITEM_TYPE.REPUTATION:
                GameCommon.RoleChangeReputation(iItemCount);
                break;
            case (int)ITEM_TYPE.PRESTIGE:
                GameCommon.RoleChangePrestige(iItemCount);
                break;
            case (int)ITEM_TYPE.BATTLEACHV:
                GameCommon.RoleChangeBattleAchv(iItemCount);
                break;
            case (int)ITEM_TYPE.UNIONCONTR:
                GameCommon.RoleChangeUnionContr(iItemCount);
                break;
            case (int)ITEM_TYPE.BEATDEMONCARD:
                GameCommon.RoleChangeBeatDemonCard(iItemCount);
                break;
            case (int)ITEM_TYPE.CHARACTER_EXP:
                RoleLogicData.GetMainRole().AddExp(iItemCount);
                break;

            case (int)ITEM_TYPE.HONOR_POINT:
                RoleLogicData.Self.AddHonorPoint(iItemCount);
                break;
            case (int)ITEM_TYPE.SAODANG_POINT:
            case (int)ITEM_TYPE.RESET_POINT:
            case (int)ITEM_TYPE.LOCK_POINT:
                GameCommon.RoleChangeFunctionalProp((ITEM_TYPE)iItemType, iItemCount);
                break;
        }
	}

    static public void RoleChangeItem(IItemDataProvider provider)
    {
        int count = provider.GetCount();

        for (int i = 0; i < count; ++i)
        {
            ItemData data = provider.GetItem(i);
            RoleChangeNumericalAboutRole(data.mType, data.mNumber);
        }
    }

    //---------------------------------------------------------------------------------------
    static public void SetPetIcon(UISprite sprite, int index)
    {
        if (sprite == null)
        {
            return;
        }

        if (index <= 0)
        {
            sprite.atlas = null;
            return;
        }
        string atlasName = TableCommon.GetStringFromActiveCongfig(index, "HEAD_ATLAS_NAME");
        string spriteName = TableCommon.GetStringFromActiveCongfig(index, "HEAD_SPRITE_NAME");
        SetIcon(sprite, atlasName, spriteName);
    }

    //---------------------------------------------------------------------------------------
    static public void SetPetFragemntIcon(UISprite sprite, int index)
    {
        if (sprite == null)
        {
            return;
        }

        if (index <= 0)
        {
            sprite.atlas = null;
            return;
        }

        int petTid = TableCommon.GetNumberFromFragment(index, "ITEM_ID");
        string atlasName = TableCommon.GetStringFromActiveCongfig(petTid, "HEAD_ATLAS_NAME");
        string spriteName = TableCommon.GetStringFromActiveCongfig(petTid, "HEAD_SPRITE_NAME");
        SetIcon(sprite, atlasName, spriteName);
    }

    static public void SetGemIcon(UISprite sprite, int index)
    {
        if (sprite == null)
        {
            return;
        }

        if (index <= 0)
        {
            sprite.atlas = null;
            return;
        }
        string atlasName = TableCommon.GetStringFromStoneTypeIconConfig(index, "STONE_ATLAS_NAME");
        string spriteName = TableCommon.GetStringFromStoneTypeIconConfig(index, "STONE_SPRITE_NAME");
        SetIcon(sprite, atlasName, spriteName);
    }

    static public void SetElementIcon(UISprite sprite, int index)
    {
        if (sprite == null)
        {
            return;
        }

        if (index < 0 || index > 4)
        {
            sprite.atlas = null;
            return;
        }
        string atlasName = TableCommon.GetStringFromElement(index, "ELEMENT_ATLAS_NAME");
        string spriteName = TableCommon.GetStringFromElement(index, "ELEMENT_SPRITE_NAME");
        SetIcon(sprite, atlasName, spriteName);
    }

    static public void SetElementBackground(UISprite sprite, int iIndex)
    {
        string strAtlasName = TableCommon.GetStringFromElement(iIndex, "ELEMENT_ATLAS_NAME");
        string strSpriteName = TableCommon.GetStringFromElement(iIndex, "ELEMENT_ICON_SPRITE_NAME");

        SetIcon(sprite, strSpriteName, strAtlasName);
    }

	static public void SetPetSoulIcon(UISprite sprite)
	{
		string atlasName = DataCenter.mItemIcon.GetData((int)ITEM_TYPE.PET_SOUL, "ITEM_ATLAS_NAME");
		string spriteName = DataCenter.mItemIcon.GetData((int)ITEM_TYPE.PET_SOUL, "ITEM_SPRITE_NAME");
		SetIcon(sprite, atlasName, spriteName);
	}

	static public void SetPrestigeSoulIcon(UISprite sprite)
	{
		string atlasName = DataCenter.mItemIcon.GetData((int)ITEM_TYPE.PRESTIGE, "ITEM_ICON_ATLAS");
		string spriteName = DataCenter.mItemIcon.GetData((int)ITEM_TYPE.PRESTIGE, "ITEM_ICON_SPRITE");
		SetIcon(sprite, atlasName, spriteName);
	}

    static public void SetGoldIcon(UISprite sprite)
    {
        string atlasName = DataCenter.mItemIcon.GetData((int)ITEM_TYPE.GOLD, "ITEM_ICON_ATLAS");
        string spriteName = DataCenter.mItemIcon.GetData((int)ITEM_TYPE.GOLD, "ITEM_ICON_SPRITE");
        SetIcon(sprite, atlasName, spriteName);
    }

    static public void SetDiamondIcon(UISprite sprite)
    {
        string atlasName = DataCenter.mItemIcon.GetData((int)ITEM_TYPE.YUANBAO, "ITEM_ICON_ATLAS");
        string spriteName = DataCenter.mItemIcon.GetData((int)ITEM_TYPE.YUANBAO, "ITEM_ICON_SPRITE");
        SetIcon(sprite, atlasName, spriteName);
    }

    static public void SetPowerIcon(UISprite sprite)
    {
        string atlasName = DataCenter.mItemIcon.GetData((int)ITEM_TYPE.POWER, "ITEM_ICON_ATLAS");
        string spriteName = DataCenter.mItemIcon.GetData((int)ITEM_TYPE.POWER, "ITEM_ICON_SPRITE");
        SetIcon(sprite, atlasName, spriteName);
    }

	static public void SetHonorPointIcon(UISprite sprite)
	{
		string atlasName = DataCenter.mItemIcon.GetData((int)ITEM_TYPE.HONOR_POINT, "ITEM_ICON_ATLAS");
		string spriteName = DataCenter.mItemIcon.GetData((int)ITEM_TYPE.HONOR_POINT, "ITEM_ICON_SPRITE");
		SetIcon(sprite, atlasName, spriteName);
	}


	static public void SetSpiritIcon(UISprite sprite)
	{
		string atlasName = DataCenter.mItemIcon.GetData((int)ITEM_TYPE.SPIRIT, "ITEM_ICON_ATLAS");
		string spriteName = DataCenter.mItemIcon.GetData((int)ITEM_TYPE.SPIRIT, "ITEM_ICON_SPRITE");
		SetIcon(sprite, atlasName, spriteName);
	}

	static public void SetNumericalIcon(UISprite sprite, ITEM_TYPE type)
	{
		string atlasName = DataCenter.mItemIcon.GetData((int)type, "ITEM_ICON_ATLAS");
		string spriteName = DataCenter.mItemIcon.GetData((int)type, "ITEM_ICON_SPRITE");
		SetIcon(sprite, atlasName, spriteName);
	}

	static public void SetConsumeIcon(UISprite sprite, int index)
	{
		string strAtlas = TableCommon.GetStringFromConsumeConfig (index, "ITEM_ATLAS_NAME");
		string strSpriteName = TableCommon.GetStringFromConsumeConfig (index, "ITEM_SPRITE_NAME");
		SetIcon (sprite, strAtlas, strSpriteName);
	}

    static public void SetMaterialIcon(UISprite sprite, int index)
    {
        string strAtlas = TableCommon.GetStringFromMaterialConfig(index, "ICON_ATLAS_NAME");
        string strSpriteName = TableCommon.GetStringFromMaterialConfig(index, "ICON_SPRITE_NAME");
        SetIcon(sprite, strAtlas, strSpriteName);
    }

    static public void SetMaterialFragmentIcon(UISprite sprite, int index)
    {
        int iMaterialIndex = TableCommon.GetNumberFromMaterialFragment(index, "MATERIAL_INDEX");
        string strAtlas = TableCommon.GetStringFromMaterialConfig(iMaterialIndex, "ICON_ATLAS_NAME");
        string strSpriteName = TableCommon.GetStringFromMaterialConfig(iMaterialIndex, "ICON_SPRITE_NAME");
        SetIcon(sprite, strAtlas, strSpriteName);
    }

	static public void SetEquipIcon(UISprite sprite, int index)
	{
        if (sprite == null)
        {
            return;
        }

		if (index <= 0)
		{
			sprite.atlas = null;
			return;
		}
		string atlasName = TableCommon.GetStringFromRoleEquipConfig(index, "ICON_ATLAS_NAME");
		string spriteName = TableCommon.GetStringFromRoleEquipConfig(index, "ICON_SPRITE_NAME");
		SetIcon(sprite, atlasName, spriteName);
	}

    static public void SetEquipFragmentIcon(UISprite sprite, int index)
    {
        if (sprite == null)
        {
            return;
        }

        if (index <= 0)
        {
            sprite.atlas = null;
            return;
        }

        int equipTid = TableCommon.GetNumberFromFragment(index, "ITEM_ID");

        string atlasName = TableCommon.GetStringFromRoleEquipConfig(equipTid, "ICON_ATLAS_NAME");
        string spriteName = TableCommon.GetStringFromRoleEquipConfig(equipTid, "ICON_SPRITE_NAME");
        SetIcon(sprite, atlasName, spriteName);
    }

    static public void SetItemGrid(GameObject gridContainer, IItemDataProvider dataProvider)
    {
        UIGridContainer container = gridContainer.GetComponent<UIGridContainer>();

        if (container == null)
        {
            container = gridContainer.GetComponentInChildren<UIGridContainer>();
        }

        if (container != null)
        {
            ItemGrid grid = new ItemGrid(container);
            grid.Reset();
            grid.Set(dataProvider);
        }      
    }

    static public void SetItemIcon(GameObject iconObj, ItemData itemData)
    {
        ItemIcon icon = new ItemIcon(iconObj);
        icon.Reset();
        icon.Set(itemData);
    }

    static public void SetItemIcon(GameObject iconObj, ItemDataBase data)
    {
        ItemData d = new ItemData();
        d.mID = data.tid;
        d.mNumber = data.itemNum;
        d.mType = (int)PackageManager.GetItemTypeByTableID(data.tid);

        ItemIcon icon = new ItemIcon(iconObj);
        icon.Reset();
        icon.Set(d);
    }

    static public void SetItemIcon(GameObject obj, string iconName, int iTid)
    {
        UISprite sprite = GameCommon.FindComponent<UISprite>(obj, iconName);
        //DEBUG.Log(sprite==null);
        //DEBUG.Log(iTid);

        GameCommon.SetItemIcon(sprite, PackageManager.GetItemTypeByTableID(iTid), iTid);
    }
	
    static public int getMagicEquipRefineMaxLevel()
    {
        Dictionary<int, DataRecord> record = DataCenter.mMagicEquipRefineConfig.GetAllRecord();
        int level = 0;
        foreach (KeyValuePair<int, DataRecord> pair in record)
        {
            if(level < pair.Key)
            {
                level = pair.Key;
            }
        }
        return level;
    }
    
    /*@offTime
     * 时间描述文字
     */
    static public string GetStringByOffTime(long offTime)
    {
        string ret = "";
        long iOffTime = CommonParam.NowServerTime() - offTime;//friendInfo.mLoginTime;
        if (iOffTime < 60)
            ret =  "0分钟前";
        else if (iOffTime < 3600 && iOffTime >= 60)
            ret = ((iOffTime / 60).ToString() + "分钟前");
        else if (iOffTime < 3600 * 24 && iOffTime >= 3600)
            ret = ((iOffTime / 3600).ToString() + "小时前");
        else if (iOffTime >= 3600 * 24)
            ret = ((iOffTime / (3600 * 24)).ToString() + "天前");
        return "";
    }

    /*@offTime
     * 时间描述文字
     */
    static public void VisitGameFriend(string friendId, string friendName, string retWindow)
    {
        Button_visit_friend_button btn = new Button_visit_friend_button();
        btn.set("FRIEND_ID", friendId);
        btn.set("FRIEND_NAME", friendName);
        btn.set("WINDOW_NAME", retWindow);
        btn._DoEvent();
    }

    /*@obj-父节点
     *@ scrollView -节点名字
     * 时间描述文字
     */
    static public void ResetScrollViewPosiTion(GameObject obj, string scrollView)
    {
        if (obj != null)
        {
            UIScrollView uiScrollView = GameCommon.FindComponent<UIScrollView>(obj, scrollView);
            if (uiScrollView != null)
            {
                uiScrollView.ResetPosition();
            }
        }
    }

    /*@obj-父节点
     *@ scrollView -节点名字
     * 时间描述文字
     */
    static public void SetScrollViewAmount(GameObject obj, string scrollView, float drag)
    {
        if (obj != null)
        {
            UIScrollView uiScrollView = GameCommon.FindComponent<UIScrollView>(obj, scrollView);
            if (uiScrollView != null)
            {
                uiScrollView.SetDragAmount(0, drag, false);
            }
        }
    }

    /// <summary>
    /// 获取当前消耗品信息
    /// </summary>
    /// <returns></returns>
    public static Dictionary<NewShopWindow.SHOP_PUMPING_TYPE, ItemDataBase> GetFinalUseItemsForNewShopWindow()
    {
        Dictionary<NewShopWindow.SHOP_PUMPING_TYPE, ItemDataBase> tmpDicItems = new Dictionary<NewShopWindow.SHOP_PUMPING_TYPE, ItemDataBase>();
        foreach (KeyValuePair<int, DataRecord> tmpPair in DataCenter.mPumpingConfig.GetAllRecord())
        {
            ItemDataBase tmpItem = new ItemDataBase() { tid = -1, itemNum = 0, itemId = -1 };
            int tmpMultiple = (int)tmpPair.Value.getObject("PUMPING_MULTIPLE");
            int tmpPrice1 = (int)tmpPair.Value.getObject("PUMPING_PRICE_1") * tmpMultiple;
            int tmpPrice2 = (int)tmpPair.Value.getObject("PUMPING_PRICE_2") * tmpMultiple;
            int tmpPrice3 = (int)tmpPair.Value.getObject("PUMPING_PRICE_3") * tmpMultiple;
            int tmpPrice1Own = PackageManager.GetItemLeftCount((int)ITEM_TYPE.SILVER_FL_COMMAND);
            int tmpPrice2Own = PackageManager.GetItemLeftCount((int)ITEM_TYPE.GOLD_FL_COMMAND);
            int tmpPrice3Own = RoleLogicData.Self.diamond;
            if (tmpPrice1 > 0 && tmpPrice1Own >= tmpPrice1)
            {
                tmpItem.tid = (int)ITEM_TYPE.SILVER_FL_COMMAND;
                tmpItem.itemId = GameCommon.GetItemId((int)ITEM_TYPE.SILVER_FL_COMMAND);
                tmpItem.itemNum = tmpPrice1;
            }
            else if (tmpPrice2 > 0 && tmpPrice2Own >= tmpPrice2)
            {
                tmpItem.tid = (int)ITEM_TYPE.GOLD_FL_COMMAND;
                tmpItem.itemId = GameCommon.GetItemId((int)ITEM_TYPE.GOLD_FL_COMMAND);
                tmpItem.itemNum = tmpPrice2;
            }
            else if (tmpPrice3 > 0 && tmpPrice3Own >= tmpPrice3)
            {
                tmpItem.tid = (int)ITEM_TYPE.YUANBAO;
                tmpItem.itemNum = tmpPrice3;
            }
            else
            {
                if (tmpPrice1 > 0)
                {
                    tmpItem.tid = (int)ITEM_TYPE.SILVER_FL_COMMAND;
                    tmpItem.itemId = GameCommon.GetItemId((int)ITEM_TYPE.SILVER_FL_COMMAND);
                    tmpItem.itemNum = -tmpPrice1;
                }
                else if (tmpPrice2 > 0)
                {
                    tmpItem.tid = (int)ITEM_TYPE.YUANBAO;
                    tmpItem.itemId = GameCommon.GetItemId((int)ITEM_TYPE.YUANBAO);
                    tmpItem.itemNum = -tmpPrice3;
                }
                else if (tmpPrice3 > 0)
                {
                    tmpItem.tid = (int)ITEM_TYPE.YUANBAO;
                    tmpItem.itemNum = -tmpPrice3;
                }
            }
            tmpDicItems[(NewShopWindow.SHOP_PUMPING_TYPE)tmpPair.Key] = tmpItem;
        }
        return tmpDicItems;
    }

	/*@iBig 
	  true --大图有背景
	  false--小图没有背景
	  by chenzhixin 
	*/
	static public void SetItemIconNew(GameObject obj, string iconName, int iTid, bool iBig = true, string starName = "", ROLE_ICON_TYPE roleType = ROLE_ICON_TYPE.PHOTO)
	{
		UISprite sprite = GameCommon.FindComponent<UISprite>(obj, iconName);
		DEBUG.Log((sprite==null).ToString());
		DEBUG.Log(iTid.ToString());

		ITEM_TYPE type = PackageManager.GetItemTypeByTableID (iTid); 
		switch(type)
		{
			case ITEM_TYPE.PRESTIGE:
			case ITEM_TYPE.PET_SOUL:
		    case ITEM_TYPE.GOLD:
			case ITEM_TYPE.YUANBAO:
		    case ITEM_TYPE.POWER:
            case ITEM_TYPE.REPUTATION:
		    case ITEM_TYPE.HONOR_POINT:
		    case ITEM_TYPE.SPIRIT:
			{
				string atlasName = DataCenter.mItemIcon.GetData ((int)type, iBig ? "ITEM_ATLAS_NAME" : "ITEM_ICON_ATLAS");
				string spriteName = DataCenter.mItemIcon.GetData((int)type, iBig ? "ITEM_SPRITE_NAME" : "ITEM_ICON_SPRITE");
			    SetIcon(sprite, atlasName, spriteName);
			}
				break;

            case ITEM_TYPE.CHARACTER:
                SetRoleIcon(sprite, iTid, roleType);
                break;
			case ITEM_TYPE.PET:
				SetPetIcon(sprite, iTid);
				break;
			case ITEM_TYPE.PET_FRAGMENT:
				//SetPetFragemntIcon(sprite, iTid);
				SetOnlyItemIcon(sprite.gameObject, iTid);
				break;
			case ITEM_TYPE.GEM:
				SetGemIcon(sprite, iTid);
				break;
			case ITEM_TYPE.EQUIP:
			case ITEM_TYPE.MAGIC:
				SetEquipIcon(sprite, iTid);
				break;
			case ITEM_TYPE.EQUIP_FRAGMENT:
			case ITEM_TYPE.MAGIC_FRAGMENT:
                //by chenliang
                //begin

//				SetEquipFragmentIcon(sprite, iTid);
//---------------------
                //读表有错
                SetOnlyItemIcon(sprite.gameObject, iTid);

                //end
				break;
			case ITEM_TYPE.SAODANG_POINT:
			case ITEM_TYPE.RESET_POINT:
			case ITEM_TYPE.LOCK_POINT:
				SetNumericalIcon (sprite, type);
				break;
			case ITEM_TYPE.CONSUME_ITEM:
				SetConsumeIcon (sprite, iTid);
				break;
			case ITEM_TYPE.MATERIAL:
				SetMaterialIcon(sprite, iTid);
				break;
			case ITEM_TYPE.MATERIAL_FRAGMENT:
				SetMaterialFragmentIcon(sprite, iTid);
				break;
            case ITEM_TYPE.MONSTER:
                {
                    if (iBig)
                    {
                        SetMonsterIconWithOutStar(obj, iconName, iTid);
                    }
                    else
                    {
                        SetMonsterIconWithStar(obj, iconName, starName, iTid);
                    }
                }
                break;
			default:
				break;
		}

        //不是碎片的话 把碎片隐藏
        SetFragmentIcon(obj, iTid, "FragmentMask");
		
	}

    //直接设obj的图标，不能设置monster的图标
    static public void SetItemIconNew(GameObject obj, int iTid, bool iBig = true, string starName = "", ROLE_ICON_TYPE roleType = ROLE_ICON_TYPE.PHOTO)     
    {
        UISprite sprite = obj.GetComponent<UISprite>();
        DEBUG.Log((sprite == null).ToString());
        DEBUG.Log(iTid.ToString());

        ITEM_TYPE type = PackageManager.GetItemTypeByTableID(iTid);
        switch (type)
        {
            case ITEM_TYPE.PRESTIGE:
            case ITEM_TYPE.PET_SOUL:
            case ITEM_TYPE.GOLD:
            case ITEM_TYPE.YUANBAO:
            case ITEM_TYPE.POWER:
            case ITEM_TYPE.REPUTATION:
            case ITEM_TYPE.HONOR_POINT:
            case ITEM_TYPE.SPIRIT:
                {
                    string atlasName = DataCenter.mItemIcon.GetData((int)type, iBig ? "ITEM_ATLAS_NAME" : "ITEM_ICON_ATLAS");
                    string spriteName = DataCenter.mItemIcon.GetData((int)type, iBig ? "ITEM_SPRITE_NAME" : "ITEM_ICON_SPRITE");
                    SetIcon(sprite, atlasName, spriteName);
                }
                break;

            case ITEM_TYPE.CHARACTER:
                SetRoleIcon(sprite, iTid, roleType);
                break;
            case ITEM_TYPE.PET:
                SetPetIcon(sprite, iTid);
                break;
            case ITEM_TYPE.PET_FRAGMENT:
                SetPetFragemntIcon(sprite, iTid);
                break;
            case ITEM_TYPE.GEM:
                SetGemIcon(sprite, iTid);
                break;
            case ITEM_TYPE.EQUIP:
            case ITEM_TYPE.MAGIC:
                SetEquipIcon(sprite, iTid);
                break;
            case ITEM_TYPE.EQUIP_FRAGMENT:
            case ITEM_TYPE.MAGIC_FRAGMENT:
                //读表有错
                SetOnlyItemIcon(sprite.gameObject, iTid);
                break;
            case ITEM_TYPE.SAODANG_POINT:
            case ITEM_TYPE.RESET_POINT:
            case ITEM_TYPE.LOCK_POINT:
                SetNumericalIcon(sprite, type);
                break;
            case ITEM_TYPE.CONSUME_ITEM:
                SetConsumeIcon(sprite, iTid);
                break;
            case ITEM_TYPE.MATERIAL:
                SetMaterialIcon(sprite, iTid);
                break;
            case ITEM_TYPE.MATERIAL_FRAGMENT:
                SetMaterialFragmentIcon(sprite, iTid);
                break;
            default:
                break;
        }

        //不是碎片的话 把碎片隐藏
        SetFragmentIcon(obj, iTid, "FragmentMask");
    }
	static public void SetItemIcon(UISprite sprite, ITEM_TYPE type, int index)
    {
        switch (type)
        {
            case ITEM_TYPE.PET:
                SetPetIcon(sprite, index);
                break;
		    case ITEM_TYPE.PET_SOUL:
			    SetPetSoulIcon(sprite);
				break;
		    case ITEM_TYPE.PRESTIGE:
				SetPrestigeSoulIcon(sprite);
				break;
            case ITEM_TYPE.PET_FRAGMENT:
                SetPetFragemntIcon(sprite, index);
                break;
            case ITEM_TYPE.GEM:
                SetGemIcon(sprite, index);
                break;
            case ITEM_TYPE.GOLD:
                SetGoldIcon(sprite);
                break;
            case ITEM_TYPE.YUANBAO:
                SetDiamondIcon(sprite);
                break;
            case ITEM_TYPE.POWER:
                SetPowerIcon(sprite);
                break;
			case ITEM_TYPE.HONOR_POINT:
				SetHonorPointIcon(sprite);
				break;
			case ITEM_TYPE.EQUIP:
            case ITEM_TYPE.MAGIC:
                SetEquipIcon(sprite, index);
                break;
            case ITEM_TYPE.EQUIP_FRAGMENT:
            case ITEM_TYPE.MAGIC_FRAGMENT:
                SetEquipFragmentIcon(sprite, index);
                break;
			case ITEM_TYPE.SPIRIT:
				SetSpiritIcon(sprite);
				break;
			case ITEM_TYPE.SAODANG_POINT:
			case ITEM_TYPE.RESET_POINT:
			case ITEM_TYPE.LOCK_POINT:
				SetNumericalIcon (sprite, type);
				break;
			case ITEM_TYPE.CONSUME_ITEM:
				SetConsumeIcon (sprite, index);
                break;
            case ITEM_TYPE.MATERIAL:
                SetMaterialIcon(sprite, index);
                break;
            case ITEM_TYPE.MATERIAL_FRAGMENT:
                SetMaterialFragmentIcon(sprite, index);
				break;
        }
    }

    static public void SetItemIcon(UISprite sprite, int type, int index)
    {
        SetItemIcon(sprite, (ITEM_TYPE)type, index);
    }

    static public void SetItemSmallIcon(UISprite sprite, int type)
    {
        string atlasName = DataCenter.mItemIcon.GetData(type, "ITEM_ATLAS_NAME");
        string spriteName = DataCenter.mItemIcon.GetData(type, "ITEM_SPRITE_NAME");
        SetIcon(sprite, atlasName, spriteName);
    }

    static public void SetItemSmallIcon(UISprite sprite, ITEM_TYPE type)
    {
        SetItemSmallIcon(sprite, (int)type);
    }

	public static float GetEquipAffectAttribute(DataRecord configRecord, AFFECT_TYPE affectType, float fValue, bool bIsPvpOpponent)
    {
        float fAffectValue = fValue;
		if (configRecord == null)
            return fAffectValue;

		string strClassName = configRecord.get ("CLASS");
        if (strClassName == "CHAR")
        {
			string equipLogicDataName = "EQUIP_DATA";
			if(bIsPvpOpponent)
			{
				equipLogicDataName = "OPPONENT_ROLE_EQUIP_DATA";
				return fAffectValue;
			}

			RoleEquipLogicData logic = DataCenter.GetData(equipLogicDataName) as RoleEquipLogicData;

            if (logic != null)
            {
				for(int i = (int)EQUIP_TYPE.ARM_EQUIP; i < (int)EQUIP_TYPE.MAX; i++)
				{
					EquipData equip = logic.GetUseEquip(i);
					if (equip != null)
					{
						fAffectValue += (equip.ApplyAffect(affectType, fValue) - fValue);
					}
				}
            }
        }
        else if (strClassName == "PET")
        {

        }

        return fAffectValue;
    }

	//--------------------------------------------------------------------------------------------------
	// affect attribute

	// max hp
    //public static int GetMaxHP(int iModelIndex, int iLevel, int iStrengthenLevel)
    //{
    //    return GetMaxHP(iModelIndex, iLevel, iStrengthenLevel, false);
    //}

    //public static int GetMaxHP(int iModelIndex, int iLevel, int iStrengthenLevel, bool bIsPvpOpponent)
    //{
    //    if(iModelIndex < 100000)
    //        return 1;

    //    DataRecord dataRecord = DataCenter.mActiveConfigTable.GetRecord(iModelIndex);
    //    return GetMaxHP(dataRecord, iLevel, iStrengthenLevel, bIsPvpOpponent);
    //}

    //public static int GetMaxHP(DataRecord dataRecord, int iLevel, int iStrengthenLevel)
    //{
    //    return GetMaxHP (dataRecord, iLevel, iStrengthenLevel, false);
    //}

    //public static int GetMaxHP(DataRecord dataRecord, int iLevel, int iStrengthenLevel, bool bIsPvpOpponent)
    //{
    //    float fValue = GetBaseMaxHP(dataRecord, iLevel, iStrengthenLevel);
    //    if(dataRecord != null)
    //    {
    //        string strClass = dataRecord.get ("CLASS");
    //        if(strClass == "CHAR" || strClass == "PET")
    //        {
    //            fValue = GameCommon.GetEquipAffectAttribute(dataRecord, AFFECT_TYPE.HP_MAX, fValue, bIsPvpOpponent);
    //            if (strClass == "CHAR")
    //            {
    //                fValue += GetRoleSkinHP(dataRecord);
    //            }
    //        }
    //    }
		
    //    return (int)fValue;
    //}

    public static int GetRoleSkinHP(DataRecord dataRecord)
    {
        float fValue = 0;

        // RoleSkin表暂时无效
        //if (dataRecord != null)
        //{
        //    string strClass = dataRecord.get("CLASS");
        //    if (strClass == "CHAR" || strClass == "PET")
        //    {
        //        fValue = TableCommon.GetNumberFromRoleSkinConfig(dataRecord["INDEX"], "ROLE_SKIN_HP");
        //    }
        //}

        return (int)fValue;
    }

    // 获得总的HP
    public static int GetTotalMaxHP(ActiveData activeData)
    {
        if (activeData == null)
            return 0;

        ActiveData tmpData = TeamManager.GetActiveDataByTeamPos(activeData.teamPos);
        float tmpHP = GameCommon.GetBaseMaxHP(activeData.tid, activeData.level, activeData.breakLevel);
        float tmpFinalHP = 0.0f;
        if (tmpData == null)
            tmpFinalHP = tmpHP;
        else
        {
            //Affect tmpAffect = AffectFunc.GetPetAllAffect(activeData.teamPos);
            //Affect relAffect = Relationship.GetTotalAffect(activeData.teamPos);
            //tmpFinalHP = tmpAffect.Final(AFFECT_TYPE.HP_MAX, tmpHP);
            //tmpFinalHP = relAffect.Final(AFFECT_TYPE.HP_MAX, tmpFinalHP);
            tmpFinalHP = AffectFunc.Final(activeData.teamPos, AFFECT_TYPE.HP_MAX, tmpHP);
        }

        return Convert.ToInt32(tmpFinalHP);
    }

	public static int GetBaseMaxHP(int iModelIndex, int iLevel, int iBreakLevel)
	{
        //if(iModelIndex < 100000)
        //    return 1;

        DataRecord dataRecord = ObjectManager.Self.GetObjectConfig(iModelIndex);//DataCenter.mActiveConfigTable.GetRecord(iModelIndex);
        return GetBaseMaxHP(dataRecord, iLevel, iBreakLevel);
	}

	public static int GetBaseMaxHP(DataRecord dataRecord, int iLevel, int iBreakLevel)
	{
		if (dataRecord == null)
		{
			DEBUG.LogError("GetBaseMaxHP..........dataRecord == null");
			return 1;
		}
		int aptitude = dataRecord ["APTITUDE_LEVEL"];
		int index = aptitude * 1000 + iBreakLevel;
        int baseValue = dataRecord["BASE_HP"];
		string strClass = dataRecord.get ("CLASS");

		if(strClass == "CHAR" || strClass == "PET")
		{
            //int addValue = dataRecord["ADD_HP"];
            //int breakValue = dataRecord["BREAK_HP"];
            //int baseBreakValue = dataRecord["BASE_BREAK_HP"];
            //return baseValue + (iLevel - 1) * addValue + iBreakLevel * baseBreakValue + (iLevel - 1) * iBreakLevel * breakValue;
			DataRecord mBreakAttribute = DataCenter.mBreakAttribute.GetRecord (index);
			if(mBreakAttribute != null)
			{
				int breakValue = mBreakAttribute["BASE_BREAK_HP"];
				int addValue = mBreakAttribute["BREAK_HP"];
				return baseValue + breakValue + addValue * iLevel ;
			}
		}

        return baseValue;
	}

	// max mp
    //public static int GetMaxMP(int iModelIndex, int iLevel, int iStrengthenLevel)
    //{
    //    return GetMaxMP(iModelIndex, iLevel, iStrengthenLevel, false);
    //}

    //public static int GetMaxMP(int iModelIndex, int iLevel, int iStrengthenLevel, bool bIsPvpOpponent)
    //{
    //    if(iModelIndex < 100000)
    //        return 1;
		
    //    DataRecord dataRecord = DataCenter.mActiveConfigTable.GetRecord(iModelIndex);
    //    return GetMaxMP(dataRecord, iLevel, iStrengthenLevel,  bIsPvpOpponent);
    //}

    //public static int GetMaxMP(DataRecord dataRecord, int iLevel, int iStrengthenLevel, bool bIsPvpOpponent)
    //{
    //    float fValue = GetBaseMaxMP(dataRecord, iLevel, iStrengthenLevel);
    //    if(dataRecord != null)
    //    {
    //        string strClass = dataRecord.get ("CLASS");
    //        if(strClass == "CHAR")
    //        {
    //            fValue = GameCommon.GetEquipAffectAttribute(dataRecord, AFFECT_TYPE.MP_MAX, fValue, bIsPvpOpponent);

    //            fValue += GetRoleSkinMP(dataRecord);
    //        }
    //    }

    //    return (int)fValue;
    //}

    public static int GetRoleSkinMP(DataRecord dataRecord)
    {
        float fValue = 0;

        // RoleSkin表暂时无效
        //if (dataRecord != null)
        //{
        //    string strClass = dataRecord.get("CLASS");
        //    if (strClass == "CHAR")
        //    {
        //        fValue = TableCommon.GetNumberFromRoleSkinConfig(dataRecord["INDEX"], "ROLE_SKIN_MP");
        //    }
        //}

        return (int)fValue;
    }

    public static int GetBaseMaxMP(int iModelIndex, int iLevel, int iBreakLevel)
	{
        //if(iModelIndex < 100000)
        //    return 1;

        DataRecord dataRecord = ObjectManager.Self.GetObjectConfig(iModelIndex);//DataCenter.mActiveConfigTable.GetRecord(iModelIndex);
        return GetBaseMaxMP(dataRecord, iLevel, iBreakLevel);
	}

	public static int GetBaseMaxMP(DataRecord dataRecord, int iLevel, int iBreakLevel)
	{
        if (dataRecord == null)
        {
            DEBUG.LogError("GetBaseMaxMP..........dataRecord == null");
            return 1;
        }

        int baseValue = dataRecord["BASE_MP"];
        string strClass = dataRecord.get("CLASS");

        if (strClass == "CHAR")
        {
            int addValue = dataRecord["ADD_MP"];
            int breakValue = dataRecord["BREAK_MP"];
            int baseBreakValue = dataRecord["BASE_BREAK_MP"];
            return baseValue + (iLevel - 1) * addValue + iBreakLevel * baseBreakValue + (iLevel - 1) * iBreakLevel * breakValue;
        }

        return baseValue;
	}    

	// attack
    //public static float GetAttack(int iModelIndex, int iLevel, int iStrengthenLevel)
    //{
    //    return GetAttack(iModelIndex, iLevel, iStrengthenLevel, false);
    //}

    //public static float GetAttack(int iModelIndex, int iLevel, int iStrengthenLevel, bool bIsPvpOpponent)
    //{
    //    if(iModelIndex < 100000)
    //        return 1.0f;
		
    //    DataRecord dataRecord = DataCenter.mActiveConfigTable.GetRecord(iModelIndex);
    //    return GetAttack(dataRecord, iLevel, iStrengthenLevel, bIsPvpOpponent);
    //}

    //public static float GetAttack(DataRecord dataRecord, int iLevel, int iStrengthenLevel)
    //{
    //    return GetAttack (dataRecord, iLevel, iStrengthenLevel, false);
    //}

    //public static float GetAttack(DataRecord dataRecord, int iLevel, int iStrengthenLevel, bool bIsPvpOpponent)
    //{
    //    float fValue = GetBaseAttack(dataRecord, iLevel, iStrengthenLevel);
    //    if(dataRecord != null)
    //    {
    //        string strClass = dataRecord.get ("CLASS");
    //        if(strClass == "CHAR" || strClass == "PET")
    //        {
    //            fValue = GameCommon.GetEquipAffectAttribute(dataRecord, AFFECT_TYPE.ATTACK, fValue, bIsPvpOpponent);

    //            if (strClass == "CHAR")
    //            {
    //                fValue += GetRoleSkinAttack(dataRecord);
    //            }
    //        }
    //    }
    //    return fValue;
    //}

    public static int GetRoleSkinAttack(DataRecord dataRecord)
    {
        float fValue = 0;

        // RoleSkin表暂时无效
        //if (dataRecord != null)
        //{
        //    string strClass = dataRecord.get("CLASS");
        //    if (strClass == "CHAR")
        //    {
        //        fValue = TableCommon.GetNumberFromRoleSkinConfig(dataRecord["INDEX"], "ROLE_SKIN_ATTACK");
        //    }
        //}
        //
        return (int)fValue;
    }

    // 获得总的攻击力
    public static int GetTotalAttack(ActiveData activeData)
    {
        if (activeData == null)
            return 0;

        ActiveData tmpData = TeamManager.GetActiveDataByTeamPos(activeData.teamPos);
        float tmpATK = GameCommon.GetBaseAttack(activeData.tid, activeData.level, activeData.breakLevel);
        float tmpFinalATK = 0.0f;
        if (tmpData == null)
            tmpFinalATK = tmpATK;
        else
        {
            //Affect tmpAffect = AffectFunc.GetPetAllAffect(activeData.teamPos);
            //Affect relAffect = Relationship.GetTotalAffect(activeData.teamPos);
            //tmpFinalATK = tmpAffect.Final(AFFECT_TYPE.ATTACK, tmpATK);
            //tmpFinalATK = relAffect.Final(AFFECT_TYPE.ATTACK, tmpFinalATK);
            tmpFinalATK = AffectFunc.Final(activeData.teamPos, AFFECT_TYPE.ATTACK, tmpATK);
        }

        return Convert.ToInt32(tmpFinalATK);
    }

	public static int GetBaseAttack(int iModelIndex, int iLevel, int iBreakLevel)
	{
        //if(iModelIndex < 100000)
        //    return 1;

        DataRecord dataRecord = ObjectManager.Self.GetObjectConfig(iModelIndex);//DataCenter.mActiveConfigTable.GetRecord(iModelIndex);
		return GetBaseAttack(dataRecord, iLevel, iBreakLevel);
	}

	public static int GetBaseAttack(DataRecord dataRecord, int iLevel, int iBreakLevel)
	{
        if (dataRecord == null)
        {
            DEBUG.LogError("GetBaseAttack..........dataRecord == null");
            return 1;
        }
		int aptitude = dataRecord ["APTITUDE_LEVEL"];//资质
		int index = aptitude * 1000 + iBreakLevel; //索引
        int baseValue = dataRecord["BASE_ATTACK"];//基础属性
        string strClass = dataRecord.get("CLASS");
        if (strClass == "CHAR" || strClass == "PET")
        {
           // int addValue = dataRecord["ADD_ATTACK"];
			//int addValue = dataRecord["BREAK_ATTACK"];
            //int breakValue = dataRecord["BREAK_ATTACK"];
            //int baseBreakValue = dataRecord["BASE_BREAK_ATTACK"];
            //return baseValue + (iLevel - 1) * addValue + iBreakLevel * baseBreakValue + (iLevel - 1) * iBreakLevel * breakValue;
			DataRecord mBreakAttribute = DataCenter.mBreakAttribute.GetRecord (index);
			if(mBreakAttribute != null)
			{
				int breakValue = mBreakAttribute["BASE_BREAK_ATTACK"];
				int addValue= mBreakAttribute["BREAK_ATTACK"];
				return baseValue + breakValue + addValue * iLevel ;
			}
        }
        return baseValue;
	}

	// defence
    //public static int GetDefence(int iModelIndex, bool bIsPvpOpponent)
    //{
    //    if(iModelIndex < 100000)
    //        return 0;
		
    //    DataRecord dataRecord = DataCenter.mActiveConfigTable.GetRecord(iModelIndex);
    //    return GetDefence(dataRecord, bIsPvpOpponent);
    //}

    //public static int GetDefence(DataRecord dataRecord)
    //{
    //    return GetDefence (dataRecord, false);
    //}

    //public static int GetDefence(DataRecord dataRecord, bool bIsPvpOpponent)
    //{
    //    float fValue = GetBaseDefence(dataRecord);
    //    if(dataRecord != null)
    //    {
    //        string strClass = dataRecord.get ("CLASS");
    //        if(strClass == "CHAR" || strClass == "PET")
    //        {
    //            fValue = GameCommon.GetEquipAffectAttribute(dataRecord, AFFECT_TYPE.DEFENCE, fValue,  bIsPvpOpponent);
    //        }
    //    }
    //    return (int)fValue;
    //}

	public static int GetBaseDefence(int iModelIndex)
	{
        //if(iModelIndex < 100000)
        //    return 0;
		
        //DataRecord dataRecord = DataCenter.mActiveConfigTable.GetRecord(iModelIndex);
        //return GetBaseDefence(dataRecord);
        return 0;
	}

	public static int GetBaseDefence(DataRecord dataRecord)
	{
        //if (dataRecord == null)
        //{
        //    DEBUG.LogError("GetBaseMaxMP..........dataRecord == null");
        //    return 0;
        //}

        //float fBaseValue = dataRecord.get("BASE_PHYSICAL_DEFENCE");
        //return fBaseValue;
        return 0;
	}

    // 获得总的物防
    public static int GetTotalPhysicalDefence(ActiveData activeData)
    {
        if (activeData == null)
            return 0;

        ActiveData tmpData = TeamManager.GetActiveDataByTeamPos(activeData.teamPos);
        float tmpPD = GameCommon.GetBasePhysicalDefence(activeData.tid, activeData.level, activeData.breakLevel);
        float tmpFinalPD = 0.0f;
        if (tmpData == null)
            tmpFinalPD = tmpPD;
        else
        {
            //Affect tmpAffect = AffectFunc.GetPetAllAffect(activeData.teamPos);
            //Affect relAffect = Relationship.GetTotalAffect(activeData.teamPos);
            //tmpFinalPD = tmpAffect.Final(AFFECT_TYPE.PHYSICAL_DEFENCE, tmpPD);
            //tmpFinalPD = relAffect.Final(AFFECT_TYPE.PHYSICAL_DEFENCE, tmpFinalPD);
            tmpFinalPD = AffectFunc.Final(activeData.teamPos, AFFECT_TYPE.PHYSICAL_DEFENCE, tmpPD);
        }

        return Convert.ToInt32(tmpFinalPD);
    }

    public static int GetBasePhysicalDefence(int iModelIndex, int iLevel, int iBreakLevel)
    {
        //if (iModelIndex < 100000)
        //    return 1;

        DataRecord dataRecord = ObjectManager.Self.GetObjectConfig(iModelIndex);//DataCenter.mActiveConfigTable.GetRecord(iModelIndex);
        return GetBasePhysicalDefence(dataRecord, iLevel, iBreakLevel);
    }

    public static int GetBasePhysicalDefence(DataRecord dataRecord, int iLevel, int iBreakLevel)
    {
        if (dataRecord == null)
        {
            DEBUG.LogError("GetBasePhysicalDefence..........dataRecord == null");
            return 1;
        }
		int aptitude = dataRecord ["APTITUDE_LEVEL"];
		int index = aptitude * 1000 + iBreakLevel;
        int baseValue = dataRecord["BASE_PHYSICAL_DEFENCE"];
        string strClass = dataRecord.get("CLASS");

        if (strClass == "CHAR" || strClass == "PET")
        {
            //int addValue = dataRecord["ADD_PHYSICAL_DEFENCE"];
            //int breakValue = dataRecord["BREAK_PHYSICAL_DEFENCE"];
            //int baseBreakValue = dataRecord["BASE_BREAK_PHYSICAL_DEFENCE"];
            //return baseValue + (iLevel - 1) * addValue + iBreakLevel * baseBreakValue + (iLevel - 1) * iBreakLevel * breakValue;
			DataRecord mBreakAttribute = DataCenter.mBreakAttribute.GetRecord (index);
			if(mBreakAttribute != null)
			{
				int breakValue = mBreakAttribute["BASE_BREAK_PHYSICAL_DEFENCE"];
				int addValue= mBreakAttribute["BREAK_PHYSICAL_DEFENCE"];
				return baseValue + breakValue + addValue * iLevel ;
			}
        }

        return baseValue;
    }

    // 获得总的法防
    public static int GetTotalMagicDefence(ActiveData activeData)
    {
        if (activeData == null)
            return 0;

        ActiveData tmpData = TeamManager.GetActiveDataByTeamPos(activeData.teamPos);
        float tmpMD = GameCommon.GetBaseMagicDefence(activeData.tid, activeData.level, activeData.breakLevel);
        float tmpFinalMD = 0.0f;
        if (tmpData == null)
            tmpFinalMD = tmpMD;
        else
        {
            //Affect tmpAffect = AffectFunc.GetPetAllAffect(activeData.teamPos);
            //Affect relAffect = Relationship.GetTotalAffect(activeData.teamPos);
            //tmpFinalMD = tmpAffect.Final(AFFECT_TYPE.MAGIC_DEFENCE, tmpMD);
            //tmpFinalMD = relAffect.Final(AFFECT_TYPE.MAGIC_DEFENCE, tmpFinalMD);
            tmpFinalMD = AffectFunc.Final(activeData.teamPos, AFFECT_TYPE.MAGIC_DEFENCE, tmpMD);
        }

        return Convert.ToInt32(tmpFinalMD);
    }

    public static int GetBaseMagicDefence(int iModelIndex, int iLevel, int iBreakLevel)
    {
        //if (iModelIndex < 100000)
        //    return 1;

        DataRecord dataRecord = ObjectManager.Self.GetObjectConfig(iModelIndex);//DataCenter.mActiveConfigTable.GetRecord(iModelIndex);
        return GetBaseMagicDefence(dataRecord, iLevel, iBreakLevel);
    }

    public static int GetBaseMagicDefence(DataRecord dataRecord, int iLevel, int iBreakLevel)
    {
        if (dataRecord == null)
        {
            DEBUG.LogError("GetBasePhysicalDefence..........dataRecord == null");
            return 1;
        }
		int aptitude = dataRecord ["APTITUDE_LEVEL"];
		int index = aptitude * 1000 + iBreakLevel;
        int baseValue = dataRecord["BASE_MAGIC_DEFENCE"];
        string strClass = dataRecord.get("CLASS");

        if (strClass == "CHAR" || strClass == "PET")
        {
            //int addValue = dataRecord["ADD_MAGIC_DEFENCE"];
            //int breakValue = dataRecord["BREAK_MAGIC_DEFENCE"];
            //int baseBreakValue = dataRecord["BASE_BREAK_MAGIC_DEFENCE"];
            //return baseValue + (iLevel - 1) * addValue + iBreakLevel * baseBreakValue + (iLevel - 1) * iBreakLevel * breakValue;
			DataRecord mBreakAttribute = DataCenter.mBreakAttribute.GetRecord (index);
			if(mBreakAttribute != null)
			{
				int breakValue = mBreakAttribute["BASE_BREAK_MAGIC_DEFENCE"];
				int addValue= mBreakAttribute["BREAK_MAGIC_DEFENCE"];
				return baseValue + breakValue + addValue * iLevel ;
			}
        }

        return baseValue;
    }

	//defence rate

	// critical strike rate
    //public static float GetCriticalStrikeRate(int iModelIndex, bool bIsPvpOpponent)
    //{
    //    if(iModelIndex < 100000)
    //        return 0;
		
    //    DataRecord dataRecord = DataCenter.mActiveConfigTable.GetRecord(iModelIndex);
    //    return GetCriticalStrikeRate(dataRecord, bIsPvpOpponent);
    //}

    //public static float GetCriticalStrikeRate(DataRecord dataRecord)
    //{
    //    return GetCriticalStrikeRate (dataRecord, false);
    //}

    //public static float GetCriticalStrikeRate(DataRecord dataRecord, bool bIsPvpOpponent)
    //{
    //    float fValue = GetBaseCriticalStrikeRate(dataRecord);
    //    if(dataRecord != null)
    //    {
    //        string strClass = dataRecord.get ("CLASS");
    //        if(strClass == "CHAR" || strClass == "PET")
    //        {
    //            fValue = GameCommon.GetEquipAffectAttribute(dataRecord, AFFECT_TYPE.CRITICAL_STRIKE_RATE, fValue, bIsPvpOpponent);
    //        }
    //    }
    //    return fValue;
    //}

	public static float GetBaseCriticalStrikeRate(int iModelIndex)
	{
        //if(iModelIndex < 100000)
        //    return 0;

        DataRecord dataRecord = ObjectManager.Self.GetObjectConfig(iModelIndex);//DataCenter.mActiveConfigTable.GetRecord(iModelIndex);
		return GetBaseCriticalStrikeRate(dataRecord);
	}
	
	public static float GetBaseCriticalStrikeRate(DataRecord dataRecord)
	{
		if (dataRecord == null)
		{
			DEBUG.LogWarning("GetBaseCriticalStrikeRate..........dataRecord == null");
			return 0;
		}

		float fBaseValue = dataRecord.get("CRITICAL_STRIKE") / 10000.0f;
		return fBaseValue;
	}

    //DEFENCE_CRITICAL_STRIKE_RATE 
    public static float GetBaseDefenceCriticalStrikeRate(int iModelIndex)
    {
        return 0f;
    }

    public static float GetBaseDefenceCriticalStrikeRate(DataRecord dataRecord)
    {
        return 0f;
    }

	// critical strike damage rate
    //public static float GetCriticalStrikeDamageRate(int iModelIndex, bool bIsPvpOpponent)
    //{
    //    if(iModelIndex < 100000)
    //        return 0;
		
    //    DataRecord dataRecord = DataCenter.mActiveConfigTable.GetRecord(iModelIndex);
    //    return GetCriticalStrikeDamageRate(dataRecord, bIsPvpOpponent);
    //}

    //public static float GetCriticalStrikeDamageRate(DataRecord dataRecord, bool bIsPvpOpponent)
    //{
    //    float fValue = GetBaseCriticalStrikeDamageRate(dataRecord);
    //    if(dataRecord != null)
    //    {
    //        string strClass = dataRecord.get ("CLASS");
    //        if(strClass == "CHAR" || strClass == "PET")
    //        {
    //            fValue = GameCommon.GetEquipAffectAttribute(dataRecord, AFFECT_TYPE.HIT_CRITICAL_STRIKE_RATE, fValue, bIsPvpOpponent);
    //        }
    //    }
    //    return fValue;
    //}

    //public static float GetBaseCriticalStrikeDamageRate(int iModelIndex)
    //{
    //    if(iModelIndex < 100000)
    //        return 0;
		
    //    DataRecord dataRecord = DataCenter.mActiveConfigTable.GetRecord(iModelIndex);
    //    return GetBaseCriticalStrikeDamageRate(dataRecord);
    //}

    //public static float GetBaseCriticalStrikeDamageRate(DataRecord dataRecord)
    //{
    //    if (dataRecord == null)
    //    {
    //        DEBUG.LogWarning("GetBaseCriticalStrikeDamageRate..........dataRecord == null");
    //        return 0;
    //    }

    //    float fBaseValue = dataRecord.get("CRITICAL_STRIKE_DAMAGE") / 100.0f;
    //    return fBaseValue;
    //}

	// critical strike damage derate rate
    //public static float GetCriticalStrikeDamageDerateRate(int iModelIndex, bool bIsPvpOpponent)
    //{
    //    if(iModelIndex < 100000)
    //        return 0;
		
    //    DataRecord dataRecord = DataCenter.mActiveConfigTable.GetRecord(iModelIndex);
    //    return GetCriticalStrikeDamageDerateRate(dataRecord, bIsPvpOpponent);
    //}

    //public static float GetCriticalStrikeDamageDerateRate(DataRecord dataRecord, bool bIsPvpOpponent)
    //{
    //    float fValue = GetBaseCriticalStrikeDamageDerateRate(dataRecord);
    //    if(dataRecord != null)
    //    {
    //        string strClass = dataRecord.get ("CLASS");
    //        if(strClass == "CHAR" || strClass == "PET")
    //        {
    //            fValue = GameCommon.GetEquipAffectAttribute(dataRecord, AFFECT_TYPE.DEFENCE_CRITICAL_DAMAGE_RATE, fValue, bIsPvpOpponent);
    //        }
    //    }

    //    return fValue;
    //}

    //public static float GetBaseCriticalStrikeDamageDerateRate(int iModelIndex)
    //{
    //    if(iModelIndex < 100000)
    //        return 0;
		
    //    DataRecord dataRecord = DataCenter.mActiveConfigTable.GetRecord(iModelIndex);
    //    return GetBaseCriticalStrikeDamageDerateRate(dataRecord);
    //}

    //public static float GetBaseCriticalStrikeDamageDerateRate(DataRecord dataRecord)
    //{
    //    if (dataRecord == null)
    //    {
    //        DEBUG.LogWarning("GetBaseCriticalStrikeDamageDerateRate..........dataRecord == null");
    //        return 0;
    //    }

    //    float fBaseValue = dataRecord.get("CRITICAL_DAMAGE_DERATE") / 100.0f;
    //    return fBaseValue;
    //}

	// hit rate
    //public static float GetHitRate(int iModelIndex, bool bIsPvpOpponent)
    //{
    //    if(iModelIndex < 100000)
    //        return 0;
		
    //    DataRecord dataRecord = DataCenter.mActiveConfigTable.GetRecord(iModelIndex);
    //    return GetHitRate(dataRecord, bIsPvpOpponent);
    //}

    //public static float GetHitRate(DataRecord dataRecord)
    //{
    //    return GetHitRate (dataRecord, false);
    //}

    //public static float GetHitRate(DataRecord dataRecord, bool bIsPvpOpponent)
    //{
    //    float fValue = GetBaseHitRate(dataRecord);
    //    if(dataRecord != null)
    //    {
    //        string strClass = dataRecord.get ("CLASS");
    //        if(strClass == "CHAR" || strClass == "PET")
    //        {
    //            fValue = GameCommon.GetEquipAffectAttribute(dataRecord, AFFECT_TYPE.HIT_TARGET_RATE, fValue, bIsPvpOpponent);
    //        }
    //    }

    //    return fValue;
    //}

	public static float GetBaseHitRate(int iModelIndex)
	{
        //if(iModelIndex < 100000)
        //    return 0;

        DataRecord dataRecord = ObjectManager.Self.GetObjectConfig(iModelIndex);//DataCenter.mActiveConfigTable.GetRecord(iModelIndex);
		return GetBaseHitRate(dataRecord);
	}

	public static float GetBaseHitRate(DataRecord dataRecord)
	{
		if (dataRecord == null)
		{
			DEBUG.LogWarning("GetBaseHitRate..........dataRecord == null");
			return 0;
		}

		float fBaseValue = dataRecord.get("HIT") / 10000.0f;
		return fBaseValue;
	}

	// dodge rate
    //public static float GetDodgeRate(int iModelIndex, bool bIsPvpOpponent)
    //{
    //    if(iModelIndex < 100000)
    //        return 0;
		
    //    DataRecord dataRecord = DataCenter.mActiveConfigTable.GetRecord(iModelIndex);
    //    return GetDodgeRate(dataRecord, bIsPvpOpponent);
    //}

    //public static float GetDodgeRate(DataRecord dataRecord)
    //{
    //    return  GetDodgeRate(dataRecord, false);
    //}

    //public static float GetDodgeRate(DataRecord dataRecord, bool bIsPvpOpponent)
    //{
    //    float fValue = GetBaseDodgeRate(dataRecord);
    //    if(dataRecord != null)
    //    {
    //        string strClass = dataRecord.get ("CLASS");
    //        if(strClass == "CHAR" || strClass == "PET")
    //        {
    //            fValue = GameCommon.GetEquipAffectAttribute(dataRecord, AFFECT_TYPE.DODGE_RATE, fValue, bIsPvpOpponent);
    //        }
    //    }

    //    return fValue;
    //}

	public static float GetBaseDodgeRate(int iModelIndex)
	{
        //if(iModelIndex < 100000)
        //    return 0;

        DataRecord dataRecord = ObjectManager.Self.GetObjectConfig(iModelIndex);//DataCenter.mActiveConfigTable.GetRecord(iModelIndex);
		return GetBaseDodgeRate(dataRecord);
	}

	public static float GetBaseDodgeRate(DataRecord dataRecord)
	{
		if (dataRecord == null)
		{
			DEBUG.LogWarning("GetBaseDodgeRate..........dataRecord == null");
			return 0;
		}

		float fBaseValue = dataRecord.get("DODGE") / 10000.0f;
		return fBaseValue;
	}

	// attack speed
    //public static float GetAttackSpeed(int iModelIndex, bool bIsPvpOpponent)
    //{
    //    if(iModelIndex < 100000)
    //        return 0;
		
    //    DataRecord dataRecord = DataCenter.mActiveConfigTable.GetRecord(iModelIndex);
    //    return GetAttackSpeed(dataRecord, bIsPvpOpponent);
    //}

    //public static float GetAttackSpeed(DataRecord dataRecord, bool bIsPvpOpponent)
    //{
    //    float fValue = GetBaseAttackSpeed(dataRecord);
    //    if(dataRecord != null)
    //    {
    //        string strClass = dataRecord.get ("CLASS");
    //        if(strClass == "CHAR" || strClass == "PET")
    //        {
    //            fValue = GameCommon.GetEquipAffectAttribute(dataRecord, AFFECT_TYPE.ATTACK_SPEED_RATE, fValue, bIsPvpOpponent);
    //        }
    //    }

    //    return fValue;
    //}

	public static float GetBaseAttackSpeed(int iModelIndex)
	{
        //if(iModelIndex < 100000)
        //    return 0f;

        DataRecord dataRecord = ObjectManager.Self.GetObjectConfig(iModelIndex);//DataCenter.mActiveConfigTable.GetRecord(iModelIndex);
		return GetBaseAttackSpeed(dataRecord);
	}

	public static float GetBaseAttackSpeed(DataRecord dataRecord)
	{
		if (dataRecord == null)
		{
			DEBUG.LogWarning("GetBaseAttackSpeed..........dataRecord == null");
			return 0;
		}
		
		float fBaseValue = dataRecord.get("ATTACKSPEED") / 10000.0f + 1.0f;
		return fBaseValue;
	}

	// damege mitigation rate
    //public static float GetDamegeMitigationRate(int iModelIndex, bool bIsPvpOpponent)
    //{
    //    if(iModelIndex < 100000)
    //        return 0;
		
    //    DataRecord dataRecord = DataCenter.mActiveConfigTable.GetRecord(iModelIndex);
    //    return GetDamegeMitigationRate(dataRecord, bIsPvpOpponent);
    //}

    //public static float GetDamegeMitigationRate(DataRecord dataRecord)
    //{
    //    return GetDamegeMitigationRate (dataRecord, false);
    //}

    //public static float GetDamegeMitigationRate(DataRecord dataRecord, bool bIsPvpOpponent)
    //{
    //    float fValue = GetBaseDamegeMitigationRate(dataRecord);
    //    if(dataRecord != null)
    //    {
    //        string strClass = dataRecord.get ("CLASS");
    //        if(strClass == "CHAR" || strClass == "PET")
    //        {
    //            fValue = GameCommon.GetEquipAffectAttribute(dataRecord, AFFECT_TYPE.DEFENCE_HIT_RATE, fValue, bIsPvpOpponent);
    //        }
    //    }

    //    return fValue;
    //}

	public static float GetBaseDamageMitigationRate(int iModelIndex)
	{
        //if(iModelIndex < 100000)
        //    return 0;

        DataRecord dataRecord = ObjectManager.Self.GetObjectConfig(iModelIndex);//DataCenter.mActiveConfigTable.GetRecord(iModelIndex);
		return GetBaseDamageMitigationRate(dataRecord);
	}

	public static float GetBaseDamageMitigationRate(DataRecord dataRecord)
	{
		if (dataRecord == null)
		{
            DEBUG.LogWarning("GetBaseDamageMitigationRate..........dataRecord == null");
			return 0;
		}
		
		float fBaseValue = dataRecord.get("MITIGATIONG") / 10000.0f;
		return fBaseValue;
	}


    public static float GetBaseDamageEnhanceRate(int iModelIndex)
    {
        return 0f;
    }

    public static float GetBaseDamageEnhanceRate(DataRecord dataRecord)
    {
        return 0f;
    }

    public static float GetBaseDamageReboundRate(int iModelIndex)
    {
        return 0f;
    }

    public static float GetBaseDamageReboundRate(DataRecord dataRecord)
    {
        return 0f;
    }

    public static float GetBaseBloodSuckingRate(int iModelIndex)
    {
        return 0f;
    }

    public static float GetBaseBloodSuckingRate(DataRecord dataRecord)
    {
        return 0f;
    }

	//Element reduction rate
    //public static float GetElementReductionRate(DataRecord dataRecord, int elementType)
    //{
    //    return GetElementReductionRate (dataRecord, elementType, false);
    //}

    //public static float GetElementReductionRate(DataRecord dataRecord, int elementType, bool bIsPvpOpponent)
    //{
    //    if(elementType >= (int)ELEMENT_TYPE.MAX)
    //        return 0;

    //    float fValue = GetBaseElementReductionRate(dataRecord, elementType);
    //    if(dataRecord != null)
    //    {
    //        string strClass = dataRecord.get ("CLASS");
    //        if(strClass == "CHAR" || strClass == "PET")
    //        {
    //            AFFECT_TYPE type = (AFFECT_TYPE)(elementType + (int)AFFECT_TYPE.RED_REDUCTION_RATE);

    //            fValue = GameCommon.GetEquipAffectAttribute(dataRecord, type, fValue, bIsPvpOpponent);
    //        }
    //    }
		
    //    return fValue;
    //}

    //public static float GetBaseElementReductionRate(DataRecord dataRecord, int elementType)
    //{
    //    if(elementType >= (int)ELEMENT_TYPE.MAX)
    //        return 0;

    //    if (dataRecord == null)
    //    {
    //        DEBUG.LogWarning("GetBaseElementReductionRate..........dataRecord == null");
    //        return 0;
    //    }

    //    string strField = "RED_REDUCTION";
    //    strField = GameCommon.ToAffectTypeString ((AFFECT_TYPE)(elementType +  (int)AFFECT_TYPE.RED_REDUCTION_RATE));
    //    strField = strField.Replace ("_RATE", "");

    //    float fBaseValue = dataRecord.get(strField) / 100.0f;
    //    return fBaseValue;
    //}

    // Move Speed
    //public static float GetMoveSpeed(int iModelIndex, bool bIsPvpOpponent)
    //{
    //    if (iModelIndex < 100000)
    //        return 0;

    //    DataRecord dataRecord = DataCenter.mActiveConfigTable.GetRecord(iModelIndex);
    //    return GetMoveSpeed(dataRecord, bIsPvpOpponent);
    //}

    //public static float GetMoveSpeed(DataRecord dataRecord, bool bIsPvpOpponent)
    //{
    //    float fValue = GetBaseMoveSpeed(dataRecord);
    //    if (dataRecord != null)
    //    {
    //        string strClass = dataRecord.get("CLASS");
    //        if (strClass == "CHAR" || strClass == "PET")
    //        {
    //            fValue = GameCommon.GetEquipAffectAttribute(dataRecord, AFFECT_TYPE.MOVE_SPEED, fValue, bIsPvpOpponent);
    //        }
    //    }

    //    return fValue;
    //}

    public static float GetBaseMoveSpeed(int iModelIndex)
    {
        //if (iModelIndex < 100000)
        //    return 0;

        DataRecord dataRecord = ObjectManager.Self.GetObjectConfig(iModelIndex);//DataCenter.mActiveConfigTable.GetRecord(iModelIndex);
        return GetBaseMoveSpeed(dataRecord);
    }

    public static float GetBaseMoveSpeed(DataRecord dataRecord)
    {
        if (dataRecord == null)
        {
            DEBUG.LogWarning("GetBaseMoveSpeed..........dataRecord == null");
            return 0;
        }

        float fBaseValue = dataRecord.get("MOVE_SPEED");
        return fBaseValue;
    }

    // Defence Charm Rate
    //public static float GetDefenceCharmRate(int iModelIndex, bool bIsPvpOpponent)
    //{
    //    if (iModelIndex < 100000)
    //        return 0;

    //    DataRecord dataRecord = DataCenter.mActiveConfigTable.GetRecord(iModelIndex);
    //    return GetDefenceCharmRate(dataRecord,  bIsPvpOpponent);
    //}

    //public static float GetDefenceCharmRate(DataRecord dataRecord, bool bIsPvpOpponent)
    //{
    //    float fValue = GetBaseDefenceCharmRate(dataRecord);
    //    if (dataRecord != null)
    //    {
    //        string strClass = dataRecord.get("CLASS");
    //        if (strClass == "CHAR" || strClass == "PET")
    //        {
    //            fValue = GameCommon.GetEquipAffectAttribute(dataRecord, AFFECT_TYPE.DEFENCE_CHARM_RATE, fValue, bIsPvpOpponent);
    //        }
    //    }

    //    return fValue;
    //}

    //public static float GetBaseDefenceCharmRate(int iModelIndex)
    //{
    //    if (iModelIndex < 100000)
    //        return 0;

    //    DataRecord dataRecord = DataCenter.mActiveConfigTable.GetRecord(iModelIndex);
    //    return GetBaseDefenceCharmRate(dataRecord);
    //}

    //public static float GetBaseDefenceCharmRate(DataRecord dataRecord)
    //{
    //    if (dataRecord == null)
    //    {
    //        DEBUG.LogWarning("GetBaseDefenceCharmRate..........dataRecord == null");
    //        return 0;
    //    }

    //    float fBaseValue = dataRecord.get("CHARM_DEF") / 100f;
    //    return fBaseValue;
    //}


    // Defence Fear Rate
    //public static float GetDefenceFearRate(int iModelIndex, bool bIsPvpOpponent)
    //{
    //    if (iModelIndex < 100000)
    //        return 0;

    //    DataRecord dataRecord = DataCenter.mActiveConfigTable.GetRecord(iModelIndex);
    //    return GetDefenceFearRate(dataRecord,  bIsPvpOpponent);
    //}

    //public static float GetDefenceFearRate(DataRecord dataRecord, bool bIsPvpOpponent)
    //{
    //    float fValue = GetBaseDefenceFearRate(dataRecord);
    //    if (dataRecord != null)
    //    {
    //        string strClass = dataRecord.get("CLASS");
    //        if (strClass == "CHAR" || strClass == "PET")
    //        {
    //            fValue = GameCommon.GetEquipAffectAttribute(dataRecord, AFFECT_TYPE.DEFENCE_FEAR_RATE, fValue, bIsPvpOpponent);
    //        }
    //    }

    //    return fValue;
    //}

    //public static float GetBaseDefenceFearRate(int iModelIndex)
    //{
    //    if (iModelIndex < 100000)
    //        return 0;

    //    DataRecord dataRecord = DataCenter.mActiveConfigTable.GetRecord(iModelIndex);
    //    return GetBaseDefenceFearRate(dataRecord);
    //}

    //public static float GetBaseDefenceFearRate(DataRecord dataRecord)
    //{
    //    if (dataRecord == null)
    //    {
    //        DEBUG.LogWarning("GetBaseDefenceFearRate..........dataRecord == null");
    //        return 0;
    //    }

    //    float fBaseValue = dataRecord.get("FEAR_DEF") / 100f;
    //    return fBaseValue;
    //}

    // Defence Ice Rate
    //public static float GetDefenceIceRate(int iModelIndex, bool bIsPvpOpponent)
    //{
    //    if (iModelIndex < 100000)
    //        return 0;

    //    DataRecord dataRecord = DataCenter.mActiveConfigTable.GetRecord(iModelIndex);
    //    return GetDefenceIceRate(dataRecord, bIsPvpOpponent);
    //}

    //public static float GetDefenceIceRate(DataRecord dataRecord, bool bIsPvpOpponent)
    //{
    //    float fValue = GetBaseDefenceIceRate(dataRecord);
    //    if (dataRecord != null)
    //    {
    //        string strClass = dataRecord.get("CLASS");
    //        if (strClass == "CHAR" || strClass == "PET")
    //        {
    //            fValue = GameCommon.GetEquipAffectAttribute(dataRecord, AFFECT_TYPE.DEFENCE_ICE_RATE, fValue, bIsPvpOpponent);
    //        }
    //    }

    //    return fValue;
    //}

    public static float GetBaseDefenceIceRate(int iModelIndex)
    {
        //if (iModelIndex < 100000)
        //    return 0;

        DataRecord dataRecord = ObjectManager.Self.GetObjectConfig(iModelIndex);//DataCenter.mActiveConfigTable.GetRecord(iModelIndex);
        return GetBaseDefenceIceRate(dataRecord);
    }

    public static float GetBaseDefenceIceRate(DataRecord dataRecord)
    {
        if (dataRecord == null)
        {
            DEBUG.LogWarning("GetBaseDefenceIceRate..........dataRecord == null");
            return 0;
        }

        float fBaseValue = dataRecord.get("FREEZE_DEF") / 10000f;
        return fBaseValue;
    }


    // Defence Woozy Rate
    //public static float GetDefenceWoozyRate(int iModelIndex, bool bIsPvpOpponent)
    //{
    //    if (iModelIndex < 100000)
    //        return 0;

    //    DataRecord dataRecord = DataCenter.mActiveConfigTable.GetRecord(iModelIndex);
    //    return GetDefenceWoozyRate(dataRecord, bIsPvpOpponent);
    //}

    //public static float GetDefenceWoozyRate(DataRecord dataRecord, bool bIsPvpOpponent)
    //{
    //    float fValue = GetBaseDefenceWoozyRate(dataRecord);
    //    if (dataRecord != null)
    //    {
    //        string strClass = dataRecord.get("CLASS");
    //        if (strClass == "CHAR" || strClass == "PET")
    //        {
    //            fValue = GameCommon.GetEquipAffectAttribute(dataRecord, AFFECT_TYPE.DEFENCE_WOOZY_RATE, fValue, bIsPvpOpponent);
    //        }
    //    }

    //    return fValue;
    //}

    public static float GetBaseDefenceWoozyRate(int iModelIndex)
    {
        //if (iModelIndex < 100000)
        //    return 0;

        DataRecord dataRecord = ObjectManager.Self.GetObjectConfig(iModelIndex);//DataCenter.mActiveConfigTable.GetRecord(iModelIndex);
        return GetBaseDefenceWoozyRate(dataRecord);
    }

    public static float GetBaseDefenceWoozyRate(DataRecord dataRecord)
    {
        if (dataRecord == null)
        {
            DEBUG.LogWarning("GetBaseDefenceWoozyRate..........dataRecord == null");
            return 0;
        }

        float fBaseValue = dataRecord.get("STUN_DEF") / 10000f;
        return fBaseValue;
    }


    // Defence BeatBack Rate
    //public static float GetDefenceBeatBackRate(int iModelIndex, bool bIsPvpOpponent)
    //{
    //    if (iModelIndex < 100000)
    //        return 0;

    //    DataRecord dataRecord = DataCenter.mActiveConfigTable.GetRecord(iModelIndex);
    //    return GetDefenceBeatBackRate(dataRecord, bIsPvpOpponent);
    //}

    //public static float GetDefenceBeatBackRate(DataRecord dataRecord, bool bIsPvpOpponent)
    //{
    //    float fValue = GetBaseDefenceWoozyRate(dataRecord);
    //    if (dataRecord != null)
    //    {
    //        string strClass = dataRecord.get("CLASS");
    //        if (strClass == "CHAR" || strClass == "PET")
    //        {
    //            fValue = GameCommon.GetEquipAffectAttribute(dataRecord, AFFECT_TYPE.DEFENCE_BEATBACK_RATE, fValue,  bIsPvpOpponent);
    //        }
    //    }

    //    return fValue;
    //}

    //public static float GetBaseDefenceBeatBackRate(int iModelIndex)
    //{
    //    if (iModelIndex < 100000)
    //        return 0;

    //    DataRecord dataRecord = DataCenter.mActiveConfigTable.GetRecord(iModelIndex);
    //    return GetBaseDefenceBeatBackRate(dataRecord);
    //}

    //public static float GetBaseDefenceBeatBackRate(DataRecord dataRecord)
    //{
    //    if (dataRecord == null)
    //    {
    //        DEBUG.LogWarning("GetBaseDefenceBeatBackRate..........dataRecord == null");
    //        return 0;
    //    }

    //    float fBaseValue = dataRecord.get("BEATBACK_DEF") / 100f;
    //    return fBaseValue;
    //}

    // Defence Down Rate
    //public static float GetDefenceDownRate(int iModelIndex, bool bIsPvpOpponent)
    //{
    //    if (iModelIndex < 100000)
    //        return 0;

    //    DataRecord dataRecord = DataCenter.mActiveConfigTable.GetRecord(iModelIndex);
    //    return GetDefenceDownRate(dataRecord, bIsPvpOpponent);
    //}

    //public static float GetDefenceDownRate(DataRecord dataRecord, bool bIsPvpOpponent)
    //{
    //    float fValue = GetBaseDefenceDownRate(dataRecord);
    //    if (dataRecord != null)
    //    {
    //        string strClass = dataRecord.get("CLASS");
    //        if (strClass == "CHAR" || strClass == "PET")
    //        {
    //            fValue = GameCommon.GetEquipAffectAttribute(dataRecord, AFFECT_TYPE.DEFENCE_DOWN_RATE, fValue,  bIsPvpOpponent);
    //        }
    //    }

    //    return fValue;
    //}

    //public static float GetBaseDefenceDownRate(int iModelIndex)
    //{
    //    if (iModelIndex < 100000)
    //        return 0;

    //    DataRecord dataRecord = DataCenter.mActiveConfigTable.GetRecord(iModelIndex);
    //    return GetBaseDefenceDownRate(dataRecord);
    //}

    //public static float GetBaseDefenceDownRate(DataRecord dataRecord)
    //{
    //    if (dataRecord == null)
    //    {
    //        DEBUG.LogWarning("GetBaseDefenceDownRate..........dataRecord == null");
    //        return 0;
    //    }

    //    float fBaseValue = dataRecord.get("KNOCKDOWN_DEF") / 100f;
    //    return fBaseValue;
    //}


    // Auto HP
    //public static float GetAutoHp(int iModelIndex, bool bIsPvpOpponent)
    //{
    //    if (iModelIndex < 100000)
    //        return 0;

    //    DataRecord dataRecord = DataCenter.mActiveConfigTable.GetRecord(iModelIndex);
    //    return GetAutoHp(dataRecord, bIsPvpOpponent);
    //}

    //public static float GetAutoHp(DataRecord dataRecord, bool bIsPvpOpponent)
    //{
    //    float fValue = GetBaseAutoHp(dataRecord);
    //    if (dataRecord != null)
    //    {
    //        string strClass = dataRecord.get("CLASS");
    //        if (strClass == "CHAR" || strClass == "PET")
    //        {
    //            fValue = GameCommon.GetEquipAffectAttribute(dataRecord, AFFECT_TYPE.AUTO_HP, fValue, bIsPvpOpponent);
    //        }
    //    }

    //    return fValue;
    //}

    //public static float GetBaseAutoHp(int iModelIndex)
    //{
    //    if (iModelIndex < 100000)
    //        return 0;

    //    DataRecord dataRecord = DataCenter.mActiveConfigTable.GetRecord(iModelIndex);
    //    return GetBaseAutoHp(dataRecord);
    //}

    //public static float GetBaseAutoHp(DataRecord dataRecord)
    //{
    //    if (dataRecord == null)
    //    {
    //        DEBUG.LogWarning("GetBaseAutoHp..........dataRecord == null");
    //        return 0;
    //    }

    //    float fBaseValue = dataRecord.get("AUTO_HP");
    //    return fBaseValue;
    //}


    // Auto MP
    //public static float GetAutoMp(int iModelIndex, bool bIsPvpOpponent)
    //{
    //    if (iModelIndex < 100000)
    //        return 0;

    //    DataRecord dataRecord = DataCenter.mActiveConfigTable.GetRecord(iModelIndex);
    //    return GetAutoMp(dataRecord, bIsPvpOpponent);
    //}

    //public static float GetAutoMp(DataRecord dataRecord, bool bIsPvpOpponent)
    //{
    //    float fValue = GetBaseAutoMp(dataRecord);
    //    if (dataRecord != null)
    //    {
    //        string strClass = dataRecord.get("CLASS");
    //        if (strClass == "CHAR" || strClass == "PET")
    //        {
    //            fValue = GameCommon.GetEquipAffectAttribute(dataRecord, AFFECT_TYPE.AUTO_MP, fValue, bIsPvpOpponent);
    //        }
    //    }

    //    return fValue;
    //}

    //public static float GetBaseAutoMp(int iModelIndex)
    //{
    //    if (iModelIndex < 100000)
    //        return 0;

    //    DataRecord dataRecord = DataCenter.mActiveConfigTable.GetRecord(iModelIndex);
    //    return GetBaseAutoMp(dataRecord);
    //}

    //public static float GetBaseAutoMp(DataRecord dataRecord)
    //{
    //    if (dataRecord == null)
    //    {
    //        DEBUG.LogWarning("GetBaseAutoMp..........dataRecord == null");
    //        return 0;
    //    }

    //    float fBaseValue = dataRecord.get("AUTO_MP");
    //    return fBaseValue;
    //}


    /// <summary>
    /// 获取类型索引
    /// </summary>
    /// <param name="tid"> tid </param>
    /// <returns> 创角类型索引 </returns>
    public static int GetRoleType(int tid)
    {
        if(tid >= 50000 && tid < 60000)
        {
            return (tid / 100) % 10;
        }

        return -1;
    }

    /// <summary>
    /// 获取主角类型索引
    /// </summary>
    /// <returns> 创角类型索引 </returns>
    public static int GetMainRoleType()
    {
        return GetRoleType(RoleLogicData.GetMainRole().tid);
    }


    //---------------------------------------------------------------------------------------

    public static BaseObject ShowModel(int modelIndex, GameObject uiPoint, float scale)
    {
        uiPoint.SetActive(false);
        BaseObject obj = ObjectManager.Self.CreateObject(modelIndex, true, false);

        if (obj == null || obj.mMainObject == null)
            return null;
        //lhcworldmap
        NavMeshAgent agent = obj.mMainObject.GetComponent<NavMeshAgent>();
        if (agent != null)
            agent.enabled = false;

        float originScale = obj.mConfigRecord.get("UI_SCALE") * 100f;
        obj.mMainObject.transform.parent = uiPoint.transform.parent;
        obj.SetPosition(uiPoint.transform.position);
        obj.mMainObject.transform.localScale = Vector3.one * originScale * scale;
        obj.mMainObject.transform.localEulerAngles = new Vector3(0f, 180f, 0f);
        //obj.mMainObject.transform.Rotate(Vector3.up, 180, Space.World);
        obj.SetVisible(true);
        obj.OnIdle();
        GameCommon.SetLayer(obj.mMainObject, CommonParam.UILayer);

		if(ObjectManager.Self.GetObjectType(modelIndex) == OBJECT_TYPE.CHARATOR)
		{
			obj.SetLightColor(new Color(0, 0, 0, 1));
            Renderer[] rendlist = obj.mBodyObject.GetComponentsInChildren<Renderer>();
			foreach (Renderer render in rendlist)
			{
				if (render != null)
				{
                    //render.castShadows = false;
                    //render.receiveShadows = false;
                    //render.material.shader = GameCommon.FindShader("Easy/TwoPassTransparent");
				}
			}
		}

        return obj;
    }

    public static BaseObject ShowCharactorModel(GameObject uiPoint, float scale)
    {
        uiPoint.SetActive(false);
        RoleData mainRole = RoleLogicData.GetMainRole();
        if (mainRole == null)
        {
            return null;
        }
        int modelIndex = mainRole.tid;
        if (modelIndex > 0)
        {
			//JudgeRestoreCharacterConfigRecord();

            return ShowModel(modelIndex, uiPoint, scale);
        }     
        return null;
    }

    public static BaseObject ShowCharactorModel(GameObject uiPoint, int modelIndex, float scale)
    {
        uiPoint.SetActive(false);
        for (int m = 0; m < uiPoint.transform.parent.childCount; m++ )
        {
            GameObject obj = uiPoint.transform.parent.GetChild(m).gameObject;
            if (obj.activeSelf)
                GameObject.Destroy(obj);
        }
        if (modelIndex > 0)
        {
			//JudgeRestoreCharacterConfigRecord();
		
            return ShowModel(modelIndex, uiPoint, scale);
        }
        return null;
    }

    //public static void StoreCharacterConfigRecord()
    //{
    //    DataRecord record = DataCenter.mActiveConfigTable.GetRecord (RoleLogicData.GetMainRole().tid);
    //    RolePartCongifRecord data = new RolePartCongifRecord();
    //    data.mIndex = record["INDEX"];
    //    data.mModelIndex = record["MODEL"];
    //    data.mAttackSkill = record["ATTACK_SKILL"];
    //    data.mScale = record["SCALE"];
    //    data.mUIScale = record["UI_SCALE"];
		
    //    DataCenter.Set ("ORIGINAL_ROLE_MODEL_RECORD", data);
    //}

    ////Note : must need store  before  change
    //public static void ChangeCharacterConfigRecord()
    //{
    //    ConsumeItemLogicStatus logic = DataCenter.GetData ("CONSUME_ITEM_STATUS") as ConsumeItemLogicStatus;
    //    if(logic != null && logic.GetChangeModelIndex () != 0)
    //    {
    //        StoreCharacterConfigRecord();

    //        DataRecord record = DataCenter.mActiveConfigTable.GetRecord (RoleLogicData.GetMainRole().tid);
    //        int changeModelIndex = logic.GetChangeModelIndex ();

    //        float fScale = DataCenter.mActiveConfigTable.GetRecord (changeModelIndex)["SCALE"];
    //        float fUIScale = DataCenter.mActiveConfigTable.GetRecord (changeModelIndex)["UI_SCALE"];
    //        record.set ("MODEL", TableCommon.GetNumberFromActiveCongfig (changeModelIndex, "MODEL"));
    //        record.set ("ATTACK_SKILL", TableCommon.GetNumberFromActiveCongfig (changeModelIndex, "ATTACK_SKILL"));
    //        record.set ("EFFECT", TableCommon.GetNumberFromActiveCongfig (changeModelIndex, "EFFECT"));
    //        record.set ("UI_EFFECT", TableCommon.GetNumberFromActiveCongfig (changeModelIndex, "UI_EFFECT"));
    //        record.set ("SCALE", fScale);
    //        record.set ("UI_SCALE", fUIScale);
    //    }
    //}

    //public static void JudgeRestoreCharacterConfigRecord()
    //{
    //    ConsumeItemLogicStatus logic = DataCenter.GetData ("CONSUME_ITEM_STATUS") as ConsumeItemLogicStatus;
    //    if(logic == null || (logic != null && logic.GetChangeModelIndex () == 0))
    //        RestoreCharacterConfigRecord ();
    //}
	
    //public static bool RestoreCharacterConfigRecord()
    //{
    //    object obj;
    //    DataCenter.Self.getData ("ORIGINAL_ROLE_MODEL_RECORD", out obj);
    //    if(obj != null)
    //    {
    //        RolePartCongifRecord data = obj as RolePartCongifRecord;
    //        if(data != null)
    //        {
    //            DataRecord record = DataCenter.mActiveConfigTable.GetRecord (data.mIndex);
    //            record.set ("MODEL", data.mModelIndex);
    //            record.set ("ATTACK_SKILL", data.mAttackSkill);
    //            record.set ("EFFECT", data.mEffect);
    //            record.set ("UI_EFFECT", data.mUIEffect);
    //            record.set ("SCALE", data.mScale);
    //            record.set ("UI_SCALE", data.mUIScale);

    //            DataCenter.Set ("ORIGINAL_ROLE_MODEL_RECORD", null);

    //            return true;
    //        }
    //    }

    //    return false;
    //}

    public static T FindComponent<T>(GameObject parentObject, string targetObjName) where T : Component
    {
        GameObject obj = FindObject(parentObject, targetObjName);
        if (obj != null) {
            return obj.GetComponent<T>();
        }
        else
            DEBUG.Log(targetObjName + "is null");
        return null;
    }

    public static T FindComponent<T>(GameObject parentObject, params string[] targetObjName) where T : Component
    {
        GameObject obj = FindObject(parentObject, targetObjName);
        if (obj != null)
        {
            return obj.GetComponent<T>();
        }

        return null;
    }

    static public void SetBufferIcon(UISprite sprite, int index)
    {
        if (index <= 0)
        {
            sprite.atlas = null;
            return;
        }

        string atlasName = TableCommon.GetStringFromAffectBuffer(index, "BUFFER_ATLAS_NAME");
        string spriteName = TableCommon.GetStringFromAffectBuffer(index, "BUFFER_SPRITE_NAME");
        sprite.atlas = LoadUIAtlas(atlasName);
        sprite.spriteName = spriteName;
    }

    static public void SetPetIconWithElementAndStar(GameObject obj, string iconName, string elementName, string starName, int modelIndex)
    {
        int elementIndex = TableCommon.GetNumberFromActiveCongfig(modelIndex, "ELEMENT_INDEX");
        int starLevel = TableCommon.GetNumberFromActiveCongfig(modelIndex, "STAR_LEVEL");
        UISprite icon = FindComponent<UISprite>(obj, iconName);
        UISprite element = FindComponent<UISprite>(obj, elementName);
        SetPetIcon(icon, modelIndex);
        SetElementIcon(element, elementIndex);
        //SetUIText(obj, starName, starLevel.ToString());
        SetStarLevelLabel(obj, starLevel, starName);
    }

	static public void SetPetIconWithElementAndStarAndLevel(GameObject obj, string iconName, string elementName, string starName, string levelName, int levelNum, int modelIndex)
	{
		SetPetIconWithElementAndStar(obj, iconName, elementName, starName, modelIndex);
		GameCommon.SetUIText (obj, levelName, "Lv." + levelNum.ToString ());
	}

    static public void SetMonsterIconWithStar(GameObject obj, string iconName, string starName, int index)
    {
        if (index <= 0)
            return;

        DataRecord record = DataCenter.mMonsterObject.GetRecord(index);

        if (record == null)
            return;

        string atlasName = record.getData("HEAD_ATLAS_NAME");
        string spriteName = record.getData("HEAD_SPRITE_NAME");
        int starLevel = record.getData("STAR_LEVEL");
        SetIcon(obj, iconName, spriteName, atlasName);
        SetUIText(obj, starName, starLevel.ToString());
    }

    static public void SetMonsterIconWithOutStar(GameObject obj, string iconName, int index)
    {
        if (index <= 0)
            return;

        DataRecord record = DataCenter.mMonsterObject.GetRecord(index);

        if (record == null)
            return;

        string atlasName = record.getData("HEAD_ATLAS_NAME");
        string spriteName = record.getData("HEAD_SPRITE_NAME");
        int starLevel = record.getData("STAR_LEVEL");
        SetIcon(obj, iconName, spriteName, atlasName);
    }

    static public void RemoveFromDictionary<T>(Dictionary<int, T> dic, Predicate<T> match)
    {
        List<int> wantToRemoveKeys = new List<int>();

        foreach (KeyValuePair<int, T> pair in dic)
        {
            if (match(pair.Value))
            {
                wantToRemoveKeys.Add(pair.Key);
            }
        }

        foreach (int key in wantToRemoveKeys)
        {
            dic.Remove(key);
        }
    }

    static public void RemoveFromNiceTable(NiceTable table, Predicate<DataRecord> match)
    {
        List<int> wantToRemoveKeys = new List<int>();
    
        foreach (KeyValuePair<int, DataRecord> pair in table.GetAllRecord())
        {
            if (match(pair.Value))
            {
                wantToRemoveKeys.Add(pair.Key);
            }
        }
    
        foreach (int key in wantToRemoveKeys)
        {
            table.DeleteRecord(key);
        }  
    }

    static public void RemoveAirFromItemTable(NiceTable itemTable, string itemTypeFieldName)
    {
        RemoveFromNiceTable(itemTable, r => r[itemTypeFieldName] == (int)ITEM_TYPE.AIR);   
    }

    static public bool TryGetEnumFromString<T>(string enumString, ref T enumValue)
    {
        Type enumType = typeof(T);
        if (Enum.IsDefined(enumType, enumString))
        {
            enumValue = (T)Enum.Parse(enumType, enumString);
            return true;
        }
        return false;
    }

    static public T GetEnumFromString<T>(string enumString, T defaultEnumValue)
    {
        T enumValue = defaultEnumValue;
        TryGetEnumFromString<T>(enumString, ref enumValue);
        return enumValue;
    }

    static public T GetEnumFromString<T>(string enumString)
    {
        return GetEnumFromString<T>(enumString, default(T));
    }

    static public GameObject FindObject(GameObject parentObj, params string[] pathNames)
    {
        GameObject current = parentObj;

        foreach (string targetName in pathNames)
        {
            if (current == null)
                return null;

            current = FindObject(current, targetName);
        }

        return current;
    }

    static public GameObject FindUI(params string[] pathNames)
    {
        GameObject obj = GameObject.Find(CommonParam.UIRootName);

        if (obj != null)
            return FindObject(obj, pathNames);

        return null;
    }

    static public bool SetUIButtonEnabled(GameObject parentObj, string btnName, bool isEnabled)
    {
        GameObject btn = FindObject(parentObj, btnName);
        
        if (btn == null)
            return false;

        UIButton uiButton = btn.GetComponent<UIButton>();

        if (uiButton == null)
        {
            UIImageButton uiImageButton = btn.GetComponent<UIImageButton>();

            if (uiImageButton == null)
            {
                return false;
            }
            else
            {
                uiImageButton.isEnabled = isEnabled;
                return true;
            }
        }
        else
        {
            uiButton.isEnabled = isEnabled;
            return true;
        }
    }

	public static GameObject GetMainCameraObj()
	{
		GameObject worldCenter = GameObject.Find("world_center");
		if(worldCenter != null)
		{
			GameObject cameraObj = GameCommon.FindObject(worldCenter, "Camera");
			if(cameraObj != null)
				return cameraObj;
		}

		return null;
	}

	public static Camera GetMainCamera()
	{
		GameObject cameraObj = GetMainCameraObj();
		if(cameraObj != null)
		{
			return cameraObj.GetComponent<Camera>();
		}
		return null;
	}

	public static bool SetMainCameraEnable(bool isEnable)
	{
		Camera camera = GameCommon.GetMainCamera();
		if(camera != null)
		{
			camera.enabled = isEnable;
			return true;
		}
		return false;
	}

	public static int GetFightingStrength(int index, int level, int strengthenLevel)
	{
		return GetFightingStrength (index, level, strengthenLevel, false);
	}

    public static int GetFightingStrength(int index, int level, int strengthenLevel, bool bIsPvpOpponent)
    {
        if (index == 0)
            return 0;

        DataRecord record = DataCenter.mActiveConfigTable.GetRecord(index);

        if (record == null)
            return 0;

		int hp = GameCommon.GetBaseMaxHP(index, level, strengthenLevel);
		int mp = GameCommon.GetBaseMaxMP(index, level, strengthenLevel);
		int attack = GameCommon.GetBaseAttack(index, level, strengthenLevel);
		int defence = GameCommon.GetBaseDefence(record);

		float hitRate = GameCommon.GetBaseHitRate(record);
		float dodgeRate = GameCommon.GetBaseDodgeRate(record);
		float criticalRate = GameCommon.GetBaseCriticalStrikeRate(record);
		//float criticalDamage = GameCommon.GetCriticalStrikeDamageRate(record, bIsPvpOpponent);
		//float criticalDamageDerate = GameCommon.GetCriticalStrikeDamageDerateRate(record, bIsPvpOpponent);
		float attackSpeed = GameCommon.GetBaseAttackSpeed(record);
		//float charmDef = GameCommon.GetDefenceCharmRate(record, bIsPvpOpponent);
		//float fearDef = GameCommon.GetDefenceFearRate(record, bIsPvpOpponent);
		float freezeDef = GameCommon.GetBaseDefenceIceRate(record);
		float woozyDef = GameCommon.GetBaseDefenceWoozyRate(record);
		//float beatbackDef = GameCommon.GetDefenceBeatBackRate(record, bIsPvpOpponent);
		//float knockdownDef = GameCommon.GetDefenceDownRate(record, bIsPvpOpponent);
		float mitigation = GameCommon.GetBaseDamageMitigationRate(record);

        float num1 = hp * 1f + attack * 15f + defence * 10f + mp * 5f;

        float num2 = 1f + hitRate * 1f + dodgeRate * 1f + criticalRate * 2f + attackSpeed * 1f
            + freezeDef * 1f + woozyDef * 1f
            + mitigation * 2f;

        int strength = (int)(num1 * num2 / 6f);

        if (ObjectManager.Self.GetObjectType(index) == OBJECT_TYPE.CHARATOR)
        {
            strength += DataCenter.mRoleSkinConfig.GetData(RoleLogicData.GetMainRole().tid, "FIGHT_STRENGTH");
        }
        else if (ObjectManager.Self.GetObjectType(index) == OBJECT_TYPE.OPPONENT_CHARACTER && OpponentCharacter.mInstance != null)
        {
            strength += DataCenter.mRoleSkinConfig.GetData(OpponentCharacter.mInstance.mConfigIndex, "FIGHT_STRENGTH");            
        }

        return strength;
    }

    public static int GetTotalFightingStrength()
    {
        RoleData roleData = RoleLogicData.Self.character;
        int sum = 0;

        if (roleData != null)
        {
            sum += GameCommon.GetFightingStrength(roleData.tid, roleData.level, 0);
        }

        PetData[] petDatas = PetLogicData.Self.GetPetDatasByTeam(PetLogicData.Self.mCurrentTeam);

        foreach (var data in petDatas)
        {
            if (data != null)
            {
                sum += GameCommon.GetFightingStrength(data.tid, data.level, data.strengthenLevel);
            }
        }

        return sum;
    }

    public static string GetAttackType(int iPetIndex, string strName = "NAME")
	{
        if (PackageManager.GetItemTypeByTableID(iPetIndex) == ITEM_TYPE.PET)
        {
            int iPetType = TableCommon.GetNumberFromActiveCongfig(iPetIndex, "PET_TYPE");
            return (TableCommon.GetStringFromPetType(iPetType, strName));
        }

        return "";
	}
	
	static public void SplitConsumeData(ref ConsumeItemData data, ConsumeItemLogicData logicData)
	{
		if(data.itemNum > 900)
		{
			data.itemNum -= 900;
			ConsumeItemData d = new ConsumeItemData();
			d.itemNum = 900;
			d.mCountdownTime = data.mCountdownTime;
			d.tid = data.tid;
			logicData.mConsumeItemList.Add (d);
			SplitConsumeData (ref data, logicData);
		}
	}

	static public void AddConsumeData(int index, int count, ConsumeItemLogicData logicData)
	{
		if(count <= 0)
			return;

		int num = logicData.mConsumeItemList.Count;
		for(int i = 0; i < num; i++)
		{
			ConsumeItemData consumeData = logicData.mConsumeItemList[i];
			if(consumeData.tid == index)
			{
				logicData.mConsumeItemList.Remove (consumeData);
				i --;
				num --;
			}
		}

		ConsumeItemData consumeItemData = new ConsumeItemData();
		consumeItemData.tid = index;
		consumeItemData.itemNum = count;
		GameCommon.SplitConsumeData(ref consumeItemData, logicData);
		
		logicData.mConsumeItemList.Add (consumeItemData);
	}

    // 该函数已不使用
	static public bool FunctionIsUnlock(UNLOCK_FUNCTION_TYPE type)
	{
        return true;
		int iType = (int)type;
		if(iType == 0)
			return true;

		int iTypeInConfig = 0;
		foreach( KeyValuePair<int, DataRecord> re in DataCenter.mPlayerLevelConfig.GetAllRecord())
		{
			if(iType == re.Value["UNLOCK_FUNCTION_1"] || iType == re.Value["UNLOCK_FUNCTION_2"])
			{
				iTypeInConfig++;
				int iNeedPlayerLevel = re.Key;
				if(RoleLogicData.Self.character.level >= iNeedPlayerLevel)
					return true;
			}
		}

		if(iTypeInConfig == 0)
			return true;

		return false;
	}

	static public int FunctionUnlockNeedLevel(UNLOCK_FUNCTION_TYPE type)
	{
		int iType = (int)type;
		foreach( KeyValuePair<int, DataRecord> re in DataCenter.mPlayerLevelConfig.GetAllRecord())
		{
			if(iType == re.Value["UNLOCK_FUNCTION_1"] || iType == re.Value["UNLOCK_FUNCTION_2"])
			{
				return re.Key;
			}
		}
		return 0;
	}

	static public void AddPlayerLevelExpWhenPveWin()
	{
		int curStageIndex = DataCenter.Get ("CURRENT_STAGE");
		int playerAddExp = TableCommon.GetNumberFromStageConfig (curStageIndex, "PLAYER_EXP");
		RoleLogicData.Self.AddPlayerLevelExp (playerAddExp);
	}

	static public void ToggleCloseButCanClick(GameObject obj, UNLOCK_FUNCTION_TYPE type, string strVisibleName)
	{
		if(obj == null)
			return;

		obj.GetComponent<UIButtonEvent>().mData.set ("TYPE", type);

		bool bIsUnlock = FunctionIsUnlock (type);
		obj.GetComponent<UIToggle>().enabled = bIsUnlock;
		FindObject (obj, strVisibleName).SetActive (bIsUnlock);

		UILabel[] labels = obj.GetComponentsInChildren<UILabel>();
		Color color = bIsUnlock ? Color.white : Color.gray;
	}

	static public void ButtonEnbleButCanClick(GameObject obj, string strName, UNLOCK_FUNCTION_TYPE type)
	{
		bool bVisible = FunctionIsUnlock (type);
		SetUIVisiable (obj, strName , bVisible);
		SetUIVisiable (obj, strName + "_gray", !bVisible);
		GetButtonData (obj, strName + "_gray").set ("TYPE", type);
	}

	static public bool ButtonEnbleShowInfo(UNLOCK_FUNCTION_TYPE type)
	{
		if(!FunctionIsUnlock (type))
		{
			DataCenter.OpenWindow ("PLAYER_LEVEL_UP_SHOW_WINDOW", false);
			DataCenter.SetData ("PLAYER_LEVEL_UP_SHOW_WINDOW", "NEED_LEVEL", type);
		}

		return !FunctionIsUnlock (type);
	}

	static public int GetRoleEquipSetIndex()
	{
		RoleEquipLogicData roleEquipLogicData = DataCenter.GetData("EQUIP_DATA") as RoleEquipLogicData;
		Dictionary<int, EquipData> dictUseEquips = roleEquipLogicData.GetCurRoleAllUseEquips();

		List<int> vecEquipIndex = new List<int>();
		for(int i = (int)EQUIP_TYPE.ARM_EQUIP; i < (int)EQUIP_TYPE.MAX; i++)
		{
			EquipData equipData = roleEquipLogicData.GetUseEquip(i);
			if(equipData != null)
			{
				vecEquipIndex.Add(equipData.tid);
			}
		}

		if(vecEquipIndex.Count >= 4)
		{
			foreach(KeyValuePair<int, DataRecord> v in DataCenter.mSetEquipConfig.GetAllRecord())
			{
				if(v.Value.get("SET_EQUIP_0") == vecEquipIndex[0]
				   && v.Value.get("SET_EQUIP_1") == vecEquipIndex[1]
				   && v.Value.get("SET_EQUIP_2") == vecEquipIndex[2]
				   && v.Value.get("SET_EQUIP_3") == vecEquipIndex[3])
				{
					return v.Key;
				}
			}
		}
		return 0;
	}

	public static int GetEquipAttachAttributeIndex(int iModelIndex, int iAttributeType)
	{
		int iGroupID = TableCommon.GetNumberFromRoleEquipConfig(iModelIndex, "ATTACHATTRIBUTE_GROUP");
		Dictionary<int, DataRecord> dictRecord = DataCenter.mEquipAttachAttributeConfig.GetAllRecord();
		foreach(KeyValuePair<int, DataRecord> pair in dictRecord)
		{
			if(pair.Value["ATTACHATTRIBUTE_GROUP"] == iGroupID
			   && pair.Value["TYPE"] == iAttributeType)
			{
				return pair.Key;
			}
		}
		return 0;
	}

	public static List<T> SortList<T>(List<T>mList, Comparison<T>mSort)
	{
		mList.Sort (mSort);
		return mList;
	}

	public static int Sort(int a, int b, bool isDescending)
	{
		if(a == b) return 0;
		if(isDescending) return  a > b ? -1 : 1;
		else return a > b ? 1 : -1;
	}
	
	public static int Sort(bool a, bool b, bool isDescengding)
	{
		if(a == b) return 0; 
		if(isDescengding) return a? -1 : 1;
		else return a? 1 : -1;
	}
	
	public static int Sort(bool bA, bool bB, bool bIsDescending, params SortListParam[] mParam)
	{
		if(Sort (bA, bB, bIsDescending) == 0)
		{
			return Sort (mParam);
//			if(Sort (iA1 , iB1, isDescending1) == 0)
//			{
//				if(Sort (iA2 , iB2, isDescending2) == 0) return(Sort (iA3 , iB3, isDescending3));
//				else return Sort (iA2 , iB2, isDescending2);
//			}
//			else return Sort (iA1 , iB1, isDescending1);
		}
		else return Sort (bA, bB, bIsDescending);
	}

	public static int Sort(params SortListParam[] mParam)
	{
		for(int i = 0; i < mParam.Length; i++)
		{
			if(Sort (mParam[i].mParam1, mParam[i].mParam2, mParam[i].bIsDescending) == 0)
			{
				if(i == mParam.Length - 2) return Sort (mParam[i+1].mParam1, mParam[i+1].mParam2, mParam[i+1].bIsDescending);
				else continue;
			}
			else return Sort (mParam[i].mParam1, mParam[i].mParam2, mParam[i].bIsDescending);
		}

		return Sort (mParam[0].mParam1, mParam[0].mParam2, mParam[0].bIsDescending);
	}

    //public static void GetSortListParamFromPetData(SORT_TYPE type, PetData a, PetData b, out SortListParam sStarLevel, out SortListParam sLevel, out SortListParam sModelIndex, out SortListParam sElement)
    //{
    //    sStarLevel = new SortListParam{mParam1 = a.starLevel, mParam2 = b.starLevel, bIsDescending = true};
    //    sLevel = new SortListParam{mParam1 = a.level, mParam2 = b.level, bIsDescending = true};
    //    sModelIndex = new SortListParam{mParam1 = a.tid, mParam2 = b.tid, bIsDescending = false};
    //    sElement = new SortListParam{mParam1 = TableCommon.GetNumberFromActiveCongfig(a.tid, "ELEMENT_INDEX"), 
    //        mParam2 = TableCommon.GetNumberFromActiveCongfig(b.tid, "ELEMENT_INDEX"), bIsDescending = false};

    //    if(type != null)
    //    {
    //        switch(type)
    //        {
    //        case SORT_TYPE.STAR_LEVEL :
    //            sStarLevel = new SortListParam{mParam1 = a.starLevel, mParam2 = b.starLevel, bIsDescending = DataCenter.Get ("DESCENDING_ORDER")};
    //            break;
    //        case SORT_TYPE.LEVEL :
    //            sLevel = new SortListParam{mParam1 = a.level, mParam2 = b.level, bIsDescending = DataCenter.Get ("DESCENDING_ORDER")};
    //            break;
    //        case SORT_TYPE.ELEMENT_INDEX:
    //            sElement = new SortListParam{mParam1 = TableCommon.GetNumberFromActiveCongfig(a.tid, "ELEMENT_INDEX"), 
    //                mParam2 = TableCommon.GetNumberFromActiveCongfig(b.tid, "ELEMENT_INDEX"), bIsDescending = !DataCenter.Get ("DESCENDING_ORDER")};
    //            break;
    //        }
    //    }
    //}

    public static void GetSortListParamFromPetData(SORT_TYPE type, PetData a, PetData b, out SortListParam param1, out SortListParam param2, out SortListParam param3, out SortListParam param4, out SortListParam param5)
    {
        param1 = new SortListParam 
        {
            mParam1 = (int)GetTeamPosType(a) , mParam2 = (int)GetTeamPosType(b), bIsDescending = true
        };
        param2 = new SortListParam { mParam1 = GetPetDataQuality(a), mParam2 = GetPetDataQuality(b), bIsDescending = true };
        param3 = new SortListParam { mParam1 = a.breakLevel, mParam2 = b.breakLevel, bIsDescending = true };
        param4 = new SortListParam { mParam1 = a.level, mParam2 = b.level, bIsDescending = true };
        param5 = new SortListParam { mParam1 = a.tid, mParam2 = b.tid, bIsDescending = false };

        if (type != null)
        {
            switch (type)
            {
                case SORT_TYPE.TEAM_POS_TYPE:
                    param1 = new SortListParam { mParam1 = (int)GetTeamPosType(a), mParam2 = (int)GetTeamPosType(b), bIsDescending = DataCenter.Get("DESCENDING_ORDER") };
                    break;
                case SORT_TYPE.STAR_LEVEL:
                    param2 = new SortListParam { mParam1 = GetPetDataQuality(a), mParam2 = GetPetDataQuality(b), bIsDescending = DataCenter.Get("DESCENDING_ORDER") };
                    break;
                case SORT_TYPE.BREAK_LEVEL:
                    param3 = new SortListParam { mParam1 = a.breakLevel, mParam2 = b.breakLevel, bIsDescending = DataCenter.Get("DESCENDING_ORDER") };
                    break;
                case SORT_TYPE.LEVEL:
                    param4 = new SortListParam { mParam1 = a.level, mParam2 = b.level, bIsDescending = DataCenter.Get("DESCENDING_ORDER") };
                    break;
                case SORT_TYPE.TID:
                    param5 = new SortListParam { mParam1 = a.tid, mParam2 = b.tid, bIsDescending = DataCenter.Get("DESCENDING_ORDER") };
                    break;
            }
        }
    }

	public static int SortPetDataByStarLevel(PetData a, PetData b)
	{
        // 经验蛋放在后
        if (!GameCommon.IsExpPet(a) && GameCommon.IsExpPet(b))
            return -1;
        else if (GameCommon.IsExpPet(a) && !GameCommon.IsExpPet(b))
            return 1;
        else
        {
            // 把品质低的宠物放在最前面
            if (a.starLevel != b.starLevel)
                return a.starLevel - b.starLevel;
            else
            {
                // 等级低的在前
                if (a.level != b.level)
                    return a.level - b.level;
                else
                {
                    // 突破等级低的在前
                    if (a.breakLevel != b.breakLevel)
                        return a.breakLevel - b.breakLevel;
                    else
                    {
                        // 按tid排序
                        return a.tid - b.tid;
                    }
                }
            }
        }

        //SortListParam sStarLevel, sLevel, sBreakLevel, sModelIndex, sElement;
        //GetSortListParamFromPetData (SORT_TYPE.STAR_LEVEL, a, b, out sStarLevel, out sLevel, out sModelIndex, out sElement);

        //return Sort (sStarLevel, sLevel, sModelIndex, sElement);
	}

    public static int SortPetDataByTeamPosType(PetData a, PetData b)
    {
        bool bIsQualityDescending = DataCenter.Get("DESCENDING_ORDER_QUALITY");
        // 已上阵宠物在前，助战宠物其次，非上阵宠物在后
        if ((int)GetTeamPosType(a) > (int)GetTeamPosType(b))
            return -1;
        else if ((int)GetTeamPosType(a) < (int)GetTeamPosType(b))
            return 1;
        else
        {
            // 经验蛋放在后
            if (!GameCommon.IsExpPet(a) && GameCommon.IsExpPet(b))
                return -1;
            else if (GameCommon.IsExpPet(a) && !GameCommon.IsExpPet(b))
                return 1;
            else
            {
                // 把品质低的宠物放在最前面
                if (a.starLevel != b.starLevel)
                    return (a.starLevel - b.starLevel) * (bIsQualityDescending ? -1 : 1);
                else
                {
                    // 等级低的在前
                    if (a.level != b.level)
                        return a.level - b.level;
                    else
                    {
                        // 突破等级低的在前
                        if (a.breakLevel != b.breakLevel)
                            return a.breakLevel - b.breakLevel;
                        else
                        {
                            // 按tid排序
                            return a.tid - b.tid;
                        }
                    }
                }
            }
        }
        

        //SortListParam sTeamPosType, sStarLevel, sBreakLevel, sLevel, sModelIndex;
        //GetSortListParamFromPetData(SORT_TYPE.TEAM_POS_TYPE, a, b, out sTeamPosType, out sStarLevel, out sBreakLevel, out sLevel, out sModelIndex);

        //return Sort(sTeamPosType, sStarLevel, sBreakLevel, sLevel, sModelIndex);
    }

    /// <summary>
    /// 根据符灵能够提供的经验进行排序
    /// </summary>
    /// <param name="lhs"></param>
    /// <param name="rhs"></param>
    /// <returns></returns>
    public static int SortPetDataByExp(PetData lhs,PetData rhs) 
    {
        bool _isDescending = true;
        DataRecord _lRecord = DataCenter.mPetLevelExpTable.GetRecord(lhs.level);      
        if(_lRecord == null)
        {
            DEBUG.LogError("Get Data from mPetLevelExpTable failed: level is:" + lhs.level);
            return -1;
        }
        DataRecord _rRecord = DataCenter.mPetLevelExpTable.GetRecord(rhs.level);
        if(_rRecord == null)
        {
            DEBUG.LogError("Get Data from mPetLevelExpTable failed: level is:" + rhs.level);
            return -1;
        }
        int _lExp = TableCommon.GetNumberFromActiveCongfig(lhs.tid, "DROP_EXP") + _lRecord["TOTAL_EXP_" + lhs.starLevel];
        int _rExp = TableCommon.GetNumberFromActiveCongfig(rhs.tid, "DROP_EXP") + _rRecord["TOTAL_EXP_" + lhs.starLevel];
        return (_lExp - _rExp) * (_isDescending ? -1 : 1);
    }

	public static int SortPetDataByLevel(PetData a, PetData b)
	{
        return 0;
        SortListParam sStarLevel, sLevel, sModelIndex, sElement;
        //GetSortListParamFromPetData (SORT_TYPE.LEVEL, a, b, out sStarLevel, out sLevel, out sModelIndex, out sElement);
		
		return Sort (sLevel, sStarLevel, sModelIndex, sElement);
	}

	public static int SortPetDataByElement(PetData a, PetData b)
	{
        return 0;
        SortListParam sStarLevel, sLevel, sModelIndex, sElement;
        //GetSortListParamFromPetData (SORT_TYPE.ELEMENT_INDEX, a, b, out sStarLevel, out sLevel, out sModelIndex, out sElement);
		
		return Sort (sElement, sStarLevel, sLevel, sModelIndex);
	}

	public static int SortAutoJoinPetDataList(PetData a, PetData b)
	{
        return 0;
		SortListParam sStarlevel, sLevel, sModelIndex, sElement, sStrengthenLevel;
		//GetSortListParamFromPetData (SORT_TYPE.AUTO_JOIN, a, b, out sStarlevel, out sLevel, out sModelIndex, out sElement);
		sStrengthenLevel = new SortListParam{mParam1 = a.strengthenLevel, mParam2 = b.strengthenLevel, bIsDescending = true};
		
		return Sort (sStarlevel, sLevel, sStrengthenLevel, sModelIndex, sElement);
	}

	public static void GetSortListParamFromEquipData(SORT_TYPE type, EquipData a, EquipData b, out SortListParam sStarLevel, out SortListParam sStrengthenLevel, out SortListParam sModelIndex, out SortListParam sElement)
	{
		sStarLevel = new SortListParam{mParam1 = a.mStarLevel, mParam2 = b.mStarLevel, bIsDescending = true};
		sStrengthenLevel = new SortListParam{mParam1 = a.strengthenLevel, mParam2 = b.strengthenLevel, bIsDescending = true};
		sModelIndex = new SortListParam{mParam1 = a.tid, mParam2 = b.tid, bIsDescending = false};
		sElement = new SortListParam{mParam1 = (int)a.mElementType, mParam2 = (int)b.mElementType, bIsDescending = false};

		if(type != null)
		{
			switch(type)
			{
			case SORT_TYPE.STAR_LEVEL :
				sStarLevel = new SortListParam{mParam1 = a.mStarLevel, mParam2 = b.mStarLevel, bIsDescending = DataCenter.Get ("DESCENDING_ORDER")};
				break;
			case SORT_TYPE.STRENGTHEN_LEVEL :
				sStrengthenLevel = new SortListParam{mParam1 = a.strengthenLevel, mParam2 = b.strengthenLevel, bIsDescending = DataCenter.Get ("DESCENDING_ORDER")};
				break;
			case SORT_TYPE.ELEMENT_INDEX:
				sElement = new SortListParam{mParam1 = (int)a.mElementType, mParam2 = (int)b.mElementType, bIsDescending = !DataCenter.Get ("DESCENDING_ORDER")};
				break;
			}
		}
	}

	public static int SortEquipDataByStarLevel(EquipData a, EquipData b)
	{
		SortListParam sStarLevel, sStrengthenLevel, sModelIndex, sElement;
		GetSortListParamFromEquipData(SORT_TYPE.STAR_LEVEL, a, b, out sStarLevel, out sStrengthenLevel, out sModelIndex, out sElement);

		return Sort (sStarLevel, sStrengthenLevel, sModelIndex, sElement);
	}

	public static int SortEquipDataByStrengthenLevel(EquipData a, EquipData b)
	{
		SortListParam sStarLevel, sStrengthenLevel, sModelIndex, sElement;
		GetSortListParamFromEquipData(SORT_TYPE.STRENGTHEN_LEVEL ,a, b, out sStarLevel, out sStrengthenLevel, out sModelIndex, out sElement);
		
		return Sort (sStrengthenLevel, sStarLevel, sModelIndex, sElement);
	}

	public static int SortEquipDataByElement(EquipData a, EquipData b)
	{
		SortListParam sStarLevel, sStrengthenLevel, sModelIndex, sElement;
		GetSortListParamFromEquipData(SORT_TYPE.ELEMENT_INDEX, a, b, out sStarLevel, out sStrengthenLevel, out sModelIndex, out sElement);
		
		return Sort (sElement, sStarLevel, sStrengthenLevel, sModelIndex);
	}
	//fabaohecheng paixu 
	public static void GetSortListParamFromEquipsData(SORT_TYPE type, DataRecord a, DataRecord b, out SortListParam sStarLevel)
	{
		sStarLevel = new SortListParam{mParam1 = a["STAR_LEVEL"], mParam2 = b["STAR_LEVEL"], bIsDescending = true};
		if(type != null)
		{
			switch(type)
			{
			case SORT_TYPE.STAR_LEVEL :
				sStarLevel = new SortListParam{mParam1 = a["STAR_LEVEL"], mParam2 = b["STAR_LEVEL"], bIsDescending = DataCenter.Get ("DESCENDING_ORDER")};
				break;
			}
		}
	}

	public static int SortEquipsDataByStarLevel(DataRecord a, DataRecord b)
	{
		SortListParam sStarLevel;
		GetSortListParamFromEquipsData(SORT_TYPE.STAR_LEVEL, a, b, out sStarLevel);
		
		return Sort (sStarLevel);
	}
	//Material rank
	public static void GetSortListParamFromMaterialData(SORT_TYPE type, DataRecord a, DataRecord b, out SortListParam sStarLevel)
	{
		sStarLevel = new SortListParam{mParam1 = a["INDEX"], mParam2 = b["INDEX"], bIsDescending = true};
		
		if(type != null)
		{
			switch(type)
			{
			case SORT_TYPE.STAR_LEVEL :
				sStarLevel = new SortListParam{mParam1 = a["INDEX"], mParam2 = b["INDEX"], bIsDescending = DataCenter.Get ("DESCENDING_ORDER")};
				break;
			}
		}
	}

	public static int SortMaterialDataByStarLevel(DataRecord a, DataRecord b)
	{
		SortListParam sStarLevel;
		GetSortListParamFromMaterialData(SORT_TYPE.STAR_LEVEL, a, b, out sStarLevel);
		
		return Sort (sStarLevel);
	}

    public static void GetSortListParamFromFragmentData(SORT_TYPE type, FragmentBaseData a, FragmentBaseData b, out SortListParam sIsCanCompose, out SortListParam sCount, out SortListParam sStarLevel, out SortListParam sTeamPosType, out SortListParam sModelIndex)
	{
        sIsCanCompose = new SortListParam
        {
            mParam1 = Convert.ToInt32(TableCommon.GetNumberFromFragment(a.tid, "COST_NUM") <= a.itemNum),
            mParam2 = Convert.ToInt32(TableCommon.GetNumberFromFragment(b.tid, "COST_NUM") <= b.itemNum),
            bIsDescending = true
        };		
		sCount = new SortListParam{mParam1 = a.itemNum, mParam2 = b.itemNum, bIsDescending = true};
        sStarLevel = new SortListParam
        {
            mParam1 = TableCommon.GetNumberFromActiveCongfig(a.mComposeItemTid, "STAR_LEVEL"),
            mParam2 = TableCommon.GetNumberFromActiveCongfig(b.mComposeItemTid, "STAR_LEVEL"),
            bIsDescending = true
        };
        sTeamPosType = new SortListParam
        {
            mParam1 = (int)GetTeamPosType(TeamManager.GetPetInTeamByTid(a.mComposeItemTid)),
            mParam2 = (int)GetTeamPosType(TeamManager.GetPetInTeamByTid(b.mComposeItemTid)),
            bIsDescending = true
        };
		sModelIndex = new SortListParam{mParam1 = a.tid, mParam2 = b.tid, bIsDescending = false};

        if (type != null)
        {
            switch (type)
            {
                case SORT_TYPE.CAN_COMPOSE:
                    sIsCanCompose = new SortListParam
                    {
                        mParam1 = Convert.ToInt32(TableCommon.GetNumberFromFragment(a.tid, "COST_NUM") <= a.itemNum),
                        mParam2 = Convert.ToInt32(TableCommon.GetNumberFromFragment(b.tid, "COST_NUM") <= b.itemNum),
                        bIsDescending = DataCenter.Get("DESCENDING_ORDER")
                    };
                    break;
                case SORT_TYPE.NUMBER:
                    sCount = new SortListParam { mParam1 = a.itemNum, mParam2 = b.itemNum, bIsDescending = DataCenter.Get("DESCENDING_ORDER") };
                    break;
                case SORT_TYPE.STAR_LEVEL:
                    sStarLevel = new SortListParam{mParam1 = TableCommon.GetNumberFromActiveCongfig(a.mComposeItemTid, "STAR_LEVEL"), 
                        mParam2 = TableCommon.GetNumberFromActiveCongfig(b.mComposeItemTid, "STAR_LEVEL"), bIsDescending = DataCenter.Get("DESCENDING_ORDER") };
                    break;                
                case SORT_TYPE.TEAM_POS_TYPE:
                    sTeamPosType = new SortListParam
                    {
                        mParam1 = (int)GetTeamPosType(TeamManager.GetPetInTeamByTid(a.mComposeItemTid)),
                        mParam2 = (int)GetTeamPosType(TeamManager.GetPetInTeamByTid(b.mComposeItemTid)),
                        bIsDescending = DataCenter.Get("DESCENDING_ORDER")
                    };
                    break;
                case SORT_TYPE.TID:
                    sModelIndex = new SortListParam { mParam1 = a.tid, mParam2 = b.tid, bIsDescending = !DataCenter.Get("DESCENDING_ORDER") };
                    break;
            }
        }
	}

    public static int SortFragmentDataByIsCanCompose(FragmentBaseData a, FragmentBaseData b)
    {
        SortListParam sIsCanCompose, sCount, sStarLevel, sTeamPosType, sModelIndex;
        GetSortListParamFromFragmentData(SORT_TYPE.CAN_COMPOSE, a, b, out sIsCanCompose, out sCount, out sStarLevel, out sTeamPosType, out sModelIndex);

        return Sort(sIsCanCompose, sCount, sStarLevel, sModelIndex);
    }

    public static int SortFragmentDataByStarLevel(FragmentBaseData a, FragmentBaseData b)
	{
        return 0;
        SortListParam sIsCanCompose, sStarLevel, sCount, sTeamPosType, sModelIndex;
        //GetSortListParamFromFragmentData(SORT_TYPE.STAR_LEVEL, a, b, out sIsCanCompose, out sStarLevel, out sCount, out sTeamPosType, out sModelIndex);

        return Sort(sStarLevel, sIsCanCompose, sCount, sModelIndex);
	}

    public static int SortFragmentDataByNumber(FragmentBaseData a, FragmentBaseData b)
	{
        return 0;
        SortListParam sIsCanCompose, sStarLevel, sCount, sModelIndex;
        //GetSortListParamFromFragmentData(SORT_TYPE.NUMBER, a, b, out sIsCanCompose, out sStarLevel, out sCount, out sModelIndex);

        return Sort(sCount, sIsCanCompose, sStarLevel, sModelIndex);
	}

    public static int SortFragmentDataByElement(FragmentBaseData a, FragmentBaseData b)
	{
        return 0;
        SortListParam sIsCanCompose, sStarLevel, sCount, sModelIndex;
        //GetSortListParamFromFragmentData(SORT_TYPE.TID, a, b, out sIsCanCompose, out sStarLevel, out sCount, out sModelIndex);

        return Sort(sModelIndex, sIsCanCompose, sCount, sStarLevel);
	}

    public static void GetSortListParamFromMaterialData(MaterialData a, MaterialData b, out SortListParam sCount, out SortListParam sIndex)
    {
        sCount = new SortListParam { mParam1 = a.mCount, mParam2 = b.mCount, bIsDescending = true };
        sIndex = new SortListParam { mParam1 = a.mIndex, mParam2 = b.mIndex, bIsDescending = false };
    }

    public static int SortMaterialDataByNumber(MaterialData a, MaterialData b)
    {
        SortListParam sCount, sIndex;
        GetSortListParamFromMaterialData(a, b, out sCount, out sIndex);

        return Sort(sCount, sIndex);
    }

    public static void GetSortListParamFromMaterialFragmentData(MaterialFragmentData a, MaterialFragmentData b, out SortListParam sCount, out SortListParam sFragmentIndex)
    {
        sCount = new SortListParam { mParam1 = a.mCount, mParam2 = b.mCount, bIsDescending = true };
        sFragmentIndex = new SortListParam { mParam1 = a.mFragmentIndex, mParam2 = b.mFragmentIndex, bIsDescending = false };
    }

    public static int SortMaterialFragmentDataByNumber(MaterialFragmentData a, MaterialFragmentData b)
    {
        SortListParam sCount, sFragmentIndex;
        GetSortListParamFromMaterialFragmentData(a, b, out sCount, out sFragmentIndex);

        return Sort(sCount, sFragmentIndex);
    }

    public static CREATE_ROLE_TYPE GetCharacterTypeByModelIndex(int iModelIndex)
    {
        iModelIndex = ((iModelIndex / 1000) - (iModelIndex / 10000)* 10) % (int)CREATE_ROLE_TYPE.max;
        return (CREATE_ROLE_TYPE)iModelIndex;
    }

	public static void CleanALlChatLog()
	{
		DataCenter.SetData ("CHAT_WORLD_WINDOW", "CLEAN_ALL_CHAT", true);
		DataCenter.SetData ("CHAT_PRIVATE_WINDOW", "CLEAN_ALL_CHAT", true);
		DataCenter.SetData ("CHAT_UNION_WINDOW", "CLEAN_ALL_CHAT", true);
	}

	public static void SetPalyerIcon(UISprite icon, int chaUid)
	{
        //by chenliang
        //begin

// 		if(roleIconIndex < 1000) roleIconIndex = 1000;
// 		string spriteName = TableCommon.GetStringFromVipList (roleIconIndex, "VIPICON");
// 		if(roleIconIndex == 1000)
// 		{
// 			string [] names = spriteName.Split ('\\');
// 			if(RoleLogicData.Self.character.tid < 190999)
// 				spriteName = names[0];
// 			else 
// 				spriteName = names[1];
// 		}
// 		SetIcon (icon, TableCommon.GetStringFromVipList (roleIconIndex, "VIPALATS"), spriteName);
//-----------------------------
        string tmpAtlasName, tmpSpriteName;
        GetItemAtlasSpriteName(chaUid, out tmpAtlasName, out tmpSpriteName);
        SetIcon(icon, tmpAtlasName, tmpSpriteName);

        //end
	}

	public static void PlayUIPlayTween(string text)
	{
		UIPlayTween uiPt = null;
		uiPt = GameObject.Find ("pet_group_sifting_box").GetComponent<UIPlayTween >();
		UILabel siftingButtonLabel = GameObject.Find ("pet_group_sifting_button").transform.Find ("label").GetComponent<UILabel >();
		if(text != null)
			siftingButtonLabel.text = text;
		uiPt.Play (true);
	}

	public static void CloseUIPlayTween(SORT_TYPE type)
	{
		if(GameObject.Find ("pet_group_sifting_box") != null && GameObject.Find ("pet_group_sifting_box").transform.localScale.y >= 0.5f)
		{
			UIPlayTween uiPt = GameObject.Find ("pet_group_sifting_box").GetComponent<UIPlayTween >();
			UILabel siftingButtonLabel = GameObject.Find ("pet_group_sifting_button").transform.Find ("label").GetComponent<UILabel >();
			string text = "星级";
			switch(type)
			{
			case SORT_TYPE.ELEMENT_INDEX:
				text = "属性";
				break;
			case SORT_TYPE.STAR_LEVEL:
				text = "星级";
				break;
			case SORT_TYPE.LEVEL:
				text = "等级";
				break;
			}
			siftingButtonLabel.text = text;
			uiPt.Play (true);
		}
	}

    public static int GetPetPassiveSkillIndex(PetData petData, int iIndex)
    {
        int iSkillIndex = 0;
        int iNum = 0;
        int iMaxNum = GetPassiveSkillMaxNum(petData);
        for (int i = 1; i <= 3; i++)
        {
            iSkillIndex = TableCommon.GetNumberFromActiveCongfig(petData.tid, "ATTACK_STATE_" + i.ToString());
            if (iSkillIndex <= 0)
                continue;

            iNum++;
            if (iIndex == iNum)
            {
                return iSkillIndex;
            }
        }

        for (int j = 1; j <= 3; j++)
        {
            iSkillIndex = TableCommon.GetNumberFromActiveCongfig(petData.tid, "AFFECT_BUFFER_" + j.ToString());
            if (iSkillIndex <= 0)
                continue;

            iNum++;

            if (iIndex == iNum)
            {
                return iSkillIndex;
            }
        }

        return 0;
    }

    public static int GetPassiveSkillMaxNum(PetData petData)
    {
        int iMaxNum = 0;
        if (petData != null)
        {
            if (petData.starLevel >= 3 && petData.starLevel < 5)
                iMaxNum = 1;
            else if (petData.starLevel == 5)
                iMaxNum = 2;
        }
        return iMaxNum;
    }

    public static int GetPassiveSkillNum(PetData petData)
    {
        if (petData == null)
            return 0;

        int iNum = 0;
        int iMaxNum = GameCommon.GetPassiveSkillMaxNum(petData);
        for (int i = 1; i <= 3 && iNum <= iMaxNum; i++)
        {
            int skillIndex = TableCommon.GetNumberFromActiveCongfig(petData.tid, "ATTACK_STATE_" + i.ToString());
            if (skillIndex <= 0)
                continue;

            iNum++;
        }

        for (int j = 1; j <= 3 && iNum <= iMaxNum; j++)
        {
            int skillIndex = TableCommon.GetNumberFromActiveCongfig(petData.tid, "AFFECT_BUFFER_" + j.ToString());
            if (skillIndex <= 0)
                continue;

            iNum++;
        }

        return iNum;
    }

    public static int GetNumberFromSkill(int iSkillIndex, string strIndexName)
    {
        //by chenliang
        //begin

//         string str = iSkillIndex.ToString().Substring(0, 1);
//         if (str == "8")
//         {
//             return TableCommon.GetNumberFromSkillConfig(iSkillIndex, strIndexName);
//         }
//         else if (str == "9")
//         {
//             return TableCommon.GetNumberFromAffectBuffer(iSkillIndex, strIndexName);
//         }
// 
//         return TableCommon.GetNumberFromAttackState(iSkillIndex, strIndexName);
//---------------
        return GameCommon.GetItemNumberField(iSkillIndex, strIndexName);

        //end
    }

    public static string GetStringFromSkill(int iSkillIndex, string strIndexName)
    {
        //by chenliang
        //begin

//         string str = iSkillIndex.ToString().Substring(0, 1);
//         if (str == "8")
//         {
//             return TableCommon.GetStringFromSkillConfig(iSkillIndex, strIndexName);
//         }
//         else if (str == "9")
//         {
//             return TableCommon.GetStringFromAffectBuffer(iSkillIndex, strIndexName);
//         }
// 
//         return TableCommon.GetStringFromAttackState(iSkillIndex, strIndexName);
//----------------
        return GameCommon.GetItemStringField(iSkillIndex, strIndexName);

        //end
    }

    public static string GetSkillName(int iSkillIndex, string strIndexName)
    {
        //by chenliang
        //begin

//         string str = iSkillIndex.ToString().Substring(0, 1);
//         if (str == "8")
//         {
//             return SkillGlobal.GetInfo(iSkillIndex).title;
//         }
//         else if (str == "9")
//         {
//             return BuffGlobal.GetInfo(iSkillIndex).title;
//         }
// 
//         return TableCommon.GetStringFromAttackState(iSkillIndex, strIndexName);
//---------------
        return GameCommon.GetItemStringField(iSkillIndex, strIndexName);

        //end
    }

    public static string GetSkillDescription(int iSkillIndex, string strIndexName)
    {
        //by chenliang
        //begin

//         string str = iSkillIndex.ToString().Substring(0, 1);
//         if (str == "8")
//         {
//             return SkillGlobal.GetInfo(iSkillIndex).describe;
//         }
//         else if (str == "9")
//         {
//             return BuffGlobal.GetInfo(iSkillIndex).describe;
//         }
// 
//         return TableCommon.GetStringFromAttackState(iSkillIndex, strIndexName);
//-----------------
        return GameCommon.GetItemStringField(iSkillIndex, strIndexName);

        //end
    }

    public static void SetPetSkillName(GameObject parentObj, int iSkillIndex, string objName)
    {
        // name
        string strName = GameCommon.GetSkillName(iSkillIndex, "NAME");
        string[] stringInfo = strName.Split(' ');
        stringInfo = stringInfo[0].Split('L');
        UILabel nameLabel = GameCommon.FindComponent<UILabel>(parentObj, "skill_name");
        if (nameLabel != null)
            nameLabel.text = stringInfo[0];
    }

    public static void SetPetSkillLevel(GameObject parentObj, int iSkillIndex, string objName, PetData petData)
    {
        if (petData == null) return;

        // level
        int iLevel = petData.GetSkillLevelByIndex(iSkillIndex);
        UILabel levelLabel = GameCommon.FindComponent<UILabel>(parentObj, "skill_level");
        if (levelLabel != null)
            levelLabel.text = "Lv." + iLevel.ToString();
    }

    public static int GetCurRoleEquipNum()
    {
        RoleEquipLogicData roleEquipLogicData = DataCenter.GetData("EQUIP_DATA") as RoleEquipLogicData;
        int iCount = roleEquipLogicData.mDicEquip.Count;
        int iCurUseID = RoleLogicData.GetMainRole().mIndex;

        foreach (KeyValuePair<int, Dictionary<int, EquipData>> pair in roleEquipLogicData.mDicRoleUseEquip)
        {
            if (iCurUseID == pair.Key)
                continue;
            iCount -= pair.Value.Count;
        }
        return iCount;
    }


	public static bool HaveEnoughCurrency(ITEM_TYPE type, int count)
	{
		switch(type)
		{
		case ITEM_TYPE.GOLD:
			if(RoleLogicData.Self.gold < count)
			{
                DataCenter.OpenWindow(UIWindowString.access_to_res_window, (int)ITEM_TYPE.GOLD);
				return false;
			}
			break;
		case ITEM_TYPE.YUANBAO:
			if(RoleLogicData.Self.diamond < count)
			{
				DataCenter.OpenMessageWindow (STRING_INDEX.ERROR_SHOP_NO_ENOUGH_DIAMOND);
				return false;
			}
			break;
		case ITEM_TYPE.SPIRIT:
			if(RoleLogicData.Self.spirit < count)
			{
				DataCenter.OpenMessageWindow (STRING_INDEX.ERROR_SHOP_NO_ENOUGH_FRIEND_POINT);
				return false;
			}
			break;
		case ITEM_TYPE.HONOR_POINT:
			if(RoleLogicData.Self.mHonorPoint < count)
			{
				DataCenter.OpenMessageWindow (STRING_INDEX.ERROR_SHOP_NO_ENOUGH_HONOR_POINT);
				return false;
			}
			break;
		}

		return true;
	}


	static public void SetIconData(GameObject icon, ItemData data)
	{
		//		UIButtonEvent evt = GameCommon.FindComponent<UIButtonEvent>("icon_tips_btn");
		//		NiceData buttonData = GameCommon.GetButtonData(icon , "icon_tips_btn");
		UIButtonEvent evt = GameCommon.FindObject (icon ,"icon_tips_btn").GetComponent<UIButtonEvent >();
		switch(data.mType)
		{
		case (int)ITEM_TYPE.GOLD:
			evt.mData.set ("INDEX", (int)ITEM_TYPE.GOLD);
			evt.mData.set ("TABLE_NAME", "ITEM_TYPE_ITEMICON");
			break;
		case (int)ITEM_TYPE.POWER:
			evt.mData.set ("INDEX", (int)ITEM_TYPE.POWER);
			evt.mData.set ("TABLE_NAME", "ITEM_TYPE_ITEMICON");
			break;
		case (int)ITEM_TYPE.YUANBAO:
			evt.mData.set ("INDEX", (int)ITEM_TYPE.YUANBAO);
			evt.mData.set ("TABLE_NAME", "ITEM_TYPE_ITEMICON");
			break;
		case (int)ITEM_TYPE.SAODANG_POINT:
			evt.mData.set ("INDEX", (int)ITEM_TYPE.SAODANG_POINT);
			evt.mData.set ("TABLE_NAME", "ITEM_TYPE_ITEMICON");
			break;
		case (int)ITEM_TYPE.SPIRIT:
			evt.mData.set ("INDEX", (int)ITEM_TYPE.SPIRIT);
			evt.mData.set ("TABLE_NAME", "ITEM_TYPE_ITEMICON");
			break;
		case (int)ITEM_TYPE.EQUIP:
			evt.mData.set ("INDEX", data.mID);
			evt.mData.set ("TABLE_NAME", "ITEM_TYPE_ROLE_EQUIP");
			break;
		case (int)ITEM_TYPE.PET:
			evt.mData.set ("INDEX", data.mID);
			evt.mData.set ("TABLE_NAME", "ITEM_TYPE_PET");
			break;
		case (int)ITEM_TYPE.GEM:
			evt.mData.set ("INDEX", data.mID);
			evt.mData.set ("TABLE_NAME", "ITEM_TYPE_STONE");
			break;
		case (int)ITEM_TYPE.MATERIAL:
			evt.mData.set ("INDEX", data.mID);
			evt.mData.set ("TABLE_NAME", "ITEM_TYPE_ROLE_EQUIP_MATERIAL");
			break;
		case (int)ITEM_TYPE.MATERIAL_FRAGMENT:
			evt.mData.set ("INDEX", data.mID);
			evt.mData.set ("TABLE_NAME", "ITEM_TYPE_ROLE_EQUIP_MATERIAL_FRAGMENT");
			break;
		case (int)ITEM_TYPE.PET_FRAGMENT:
			evt.mData.set ("INDEX", data.mID);
			evt.mData.set ("TABLE_NAME", "ITEM_TYPE_PET_FRAGMENT");
			break;
		case (int)ITEM_TYPE.CONSUME_ITEM :
		case (int)ITEM_TYPE.LOCK_POINT :
		case (int)ITEM_TYPE.RESET_POINT :
			evt.mData.set ("INDEX", data.mID);
			evt.mData.set ("TABLE_NAME", "ITEM_TYPE_TOOLITEM");
			break;
		}
	}

    public static void InitCard(GameObject cardObj, float fCardScale, GameObject parentPanelObj, string subtype = "")
	{
		GameObject obj = GameCommon.LoadAndIntanciateUIPrefabs("card_group_window" + subtype, cardObj.name);
		
		if(obj != null)
		{
			CardGroupUI uiScript = obj.GetComponent<CardGroupUI>();
            uiScript.InitPetInfo(cardObj.name, fCardScale, parentPanelObj);
		}
		
		cardObj.transform.localScale = cardObj.transform.localScale * fCardScale;
	}

    public static void InitCardWithoutBackground(GameObject cardObj, float fCardScale, GameObject parentPanelObj)
    {
        InitCard(cardObj, fCardScale, parentPanelObj);

        // hide
        HideUI(cardObj);
    }

    public static void HideUI(GameObject cardObj)
    {
        GameObject info = GameCommon.FindObject(cardObj, "info");
        GameObject infoCardBackground = GameCommon.FindObject(cardObj, "info_card_background");

        if (info != null)
            info.SetActive(false);

        if (infoCardBackground != null)
            infoCardBackground.SetActive(false);
    }

    /// <summary>
    /// 创建基础数据对象——为给服务器发消息准备的
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public static ItemDataBase CreateItemDataBase(ItemDataBase item)
    {
        if (item != null)
        {
            return CreateItemDataBase(item.itemId, item.tid, item.itemNum);
        }

        return null;
    }

    public static ItemDataBase CreateItemDataBase(int iItemId, int iTid, int iItemNum)
    {
        ItemDataBase item = new ItemDataBase();
        item.itemId = iItemId;
        item.tid = iTid;
        item.itemNum = iItemNum;

        return item;
    }

    /// <summary>
    /// 确保UI显示数字不为负数
    /// </summary>
    /// <param name="iNum"></param>
    /// <returns></returns>
    public static int CheckShowNonNegativeNum(int iNum)
    {
        if (iNum < 0)
        {
            iNum = 0;
        }
        return iNum;
    }

    public static float CheckShowNonNegativeNum(float fNum)
    {
        if (fNum < 0)
        {
            fNum = 0;
        }
        return fNum;
    }

    /// <summary>
    /// 显示超出数字UI——根据拥有数量与最大数量关系，来改变颜色
    /// </summary>
    /// <param name="iOwnNum">拥有数量</param>
    /// <param name="iMaxNum">最大数量</param>
    /// <returns>拥有数量显示内容</returns>
    public static string CheckShowHadNumUI(int iOwnNum, int iMaxNum)
    {
        iOwnNum = CheckShowNonNegativeNum(iOwnNum);
		int r = 153, g = 255, b = 102;
		string _colorFlagStr = "["+ r.ToString("x2") + g.ToString("x2")+ b.ToString("x2") +"]";
        if (iOwnNum > iMaxNum && iOwnNum / 10000 <= 0)
        {
			return _colorFlagStr + iOwnNum + "[-]";
		}
		else if(iOwnNum / 10000 > 0)
		{
			iOwnNum = iOwnNum / 10000 ;

			return _colorFlagStr + iOwnNum + "万" + "[-]";
		}
        return iOwnNum.ToString();
    }

	/// <summary>
	/// 显示消耗数字UI——根据消耗数量与拥有数量关系，来改变颜色.
	/// </summary>
	/// <returns>消耗数量.</returns>
	/// <param name="kCostNum">消耗数量.</param>
	/// <param name="kOwnNum">拥有数量.</param>
	public static string CheckCostNumColor(int kCostNum,int kOwnNum)
	{
		kCostNum = CheckShowNonNegativeNum(kCostNum);
		if (kCostNum > kOwnNum)
		{
			return "[ff0000]" + kCostNum + "[-]";
		}
		return kCostNum.ToString();
	}

	/// <summary>
	/// 显示消耗数字UI——根据消耗数量与拥有数量关系，来改变颜色.
	/// </summary>
	/// <returns>当前拥有总量.</returns>
	public static string CheckOwnNumColor(int kCostNum,int kOwnNum)
	{
		kCostNum = CheckShowNonNegativeNum(kCostNum);
		if (kCostNum > kOwnNum)
		{
			return "[ff0000]" + kOwnNum + "[-]";
		}
		return kOwnNum.ToString();
	}
	
	/// <summary>
	/// 显示加数字的UI
    /// </summary>
    /// <param name="iAddNum"></param>
    /// <returns></returns>
    public static string ShowAddNumUI(int iAddNum)
    {
        iAddNum = CheckShowNonNegativeNum(iAddNum);

        if (iAddNum > 0)
        {
            return "+" + iAddNum.ToString();
        }
        return "";
    }

    public static string ShowAddNumUI(float fAddNum)
    {
        fAddNum = CheckShowNonNegativeNum(fAddNum);

        if (fAddNum > 0)
        {
            return "+" + fAddNum.ToString();
        }
        return "";
    }

    public static string GetQualityColor(int iStartLevel)
    {
        string strStartLevelColor = "";

        switch ((ITEM_QUALITY_TYPE)iStartLevel)
        {
            case ITEM_QUALITY_TYPE.LOW:
                break;
            case ITEM_QUALITY_TYPE.MIDDLE:
                strStartLevelColor = "[6bc235]";
                break;
            case ITEM_QUALITY_TYPE.GOOD:
                strStartLevelColor = "[005aab]";
                break;
            case ITEM_QUALITY_TYPE.BETTER:
                strStartLevelColor = "[5a0d43]";
                break;
            case ITEM_QUALITY_TYPE.BEST:
                strStartLevelColor = "[f8d61d]";
                break;
        }

        return strStartLevelColor;
    }

    public static string GetEquipTypeColor(int kEquipType) 
    {
        string strEquipTypeColor = "";

        switch ((EQUIP_QUALITY_TYPE)kEquipType)
        {
            case EQUIP_QUALITY_TYPE.LOW:
                strEquipTypeColor = "[cccccc]";
                break;
            case EQUIP_QUALITY_TYPE.MIDDLE:
                strEquipTypeColor = "[009900]";
                break;
            case EQUIP_QUALITY_TYPE.GOOD:
                strEquipTypeColor = "[0099cc]";
                break;
            case EQUIP_QUALITY_TYPE.BETTER:
                strEquipTypeColor = "[cc33ff]";
                break;
            case EQUIP_QUALITY_TYPE.BEST:
                strEquipTypeColor = "[ffcc00]";
                break;
            case EQUIP_QUALITY_TYPE.PERFECT:
                strEquipTypeColor = "[ff0000]";
                break;
        }

        return strEquipTypeColor;
    }

    /// <summary>
    /// 解析物品列表
    /// 被解析的字符串格式为 XXXX#YY|XXXX#YY|XXXX#YY
    /// 其中X表示tid，Y表示数量，如20001#3|10005#6 表示tid为20001的物品3个及tid为10005的物品6个
    /// 字符串中不能含有空格
    /// </summary>
    /// <param name="text"> 列表字符串 </param>
    /// <returns> 物品列表，其中每个物品的itemId采用默认值-1 </returns>
    public static List<ItemDataBase> ParseItemList(string text)
    {
        List<ItemDataBase> result = new List<ItemDataBase>();
        string[] strs = text.Split('|');

        foreach (string s in strs)
        {      
            int n = s.IndexOf('#');

            if (n < 0)
            {
                int id = int.Parse(s.Trim());
                result.Add(new ItemDataBase() { tid = id, itemNum = 1 });
            }
            else 
            {
                int id = int.Parse(s.Substring(0, n).Trim());
                int num = int.Parse(s.Substring(n + 1, s.Length - n - 1).Trim());
                result.Add(new ItemDataBase() { tid = id, itemNum = num });
            }
        }

        return result;
    }

    public static ItemDataBase ParseItem(string text) {
            int n=text.IndexOf('#');

            if(n<0) {
                int id=int.Parse(text.Trim());
                return new ItemDataBase() { tid=id,itemNum=1 };
            } else {
                int id=int.Parse(text.Substring(0,n).Trim());
                int num=int.Parse(text.Substring(n+1,text.Length-n-1).Trim());
                return new ItemDataBase() { tid=id,itemNum=num };
            }
        

        
    }


    /// <summary>
    /// 获取指定GroupID的物品列表
    /// </summary>
    /// <param name="groupId"> GroupID </param>
    /// <param name="withLootTime"> 物品数量是否考虑LootTime，一般活动中会采用LootTime倍率 </param>
    /// <returns> 物品列表，其中每个物品的itemId采用默认值-1 </returns>
    public static List<ItemDataBase> GetItemGroup(int groupId, bool withLootTime)
    {
        List<ItemDataBase> result = new List<ItemDataBase>();

        foreach (KeyValuePair<int, DataRecord> pair in DataCenter.mGroupIDConfig.GetAllRecord())
        {
            DataRecord r = pair.Value;

            if (r["GROUP_ID"] == groupId)
            {
                int count = r["ITEM_COUNT"];

                if (withLootTime)
                {
                    count *= r["LOOT_TIME"];
                }

                result.Add(new ItemDataBase() { tid = r["ITEM_ID"], itemNum = count, itemId = -1 });
            }
        }

        return result;
    }

    //stage loot use this
    public static List<ItemDataBase> GetmStageLootGroup(int groupId)
    {
        List<ItemDataBase> result = new List<ItemDataBase>();
        foreach (var tmpPair in DataCenter.mStageLootGroupIDConfig.GetAllRecord())
        {
            if (tmpPair.Value.get("GROUP_ID") == groupId && tmpPair.Value.get("ITEM_TYPE") != (int)ITEM_TYPE.AIR && tmpPair.Value.get("ITEM_DROP_WEIGHT") > 0)
            {
                int tmpTid = (int)tmpPair.Value.getObject("ITEM_ID");
                result.Add(new ItemDataBase()
                {
                    tid = tmpTid
                });
            }
        }

        return result;
    }

    // set equip attribute icon
    public static void SetAttributeIcon(GameObject obj, int iAttributeType, string strIconName)
    {
        string strAtlasName = TableCommon.GetStringFromEquipAttributeIconConfig(iAttributeType, "ICON_ATLAS_NAME");
        string strSpriteName = TableCommon.GetStringFromEquipAttributeIconConfig(iAttributeType, "ICON_SPRITE_NAME");

        UIAtlas tu = GameCommon.LoadUIAtlas(strAtlasName);
        UISprite sprite = GameCommon.FindObject(obj, strIconName).GetComponent<UISprite>();
        sprite.atlas = tu;
        sprite.spriteName = strSpriteName;
        //sprite.MakePixelPerfect();
    }

    // set attribute name
    public static void SetAttributeName(GameObject obj, int iAttributeType, string strName)
    {
        if (obj == null || iAttributeType <= (int)AFFECT_TYPE.NONE || iAttributeType >= (int)AFFECT_TYPE.MAX)
            return;

        UILabel nameLabel = GameCommon.FindComponent<UILabel>(obj, strName);
        if (nameLabel != null)
            nameLabel.text = TableCommon.GetStringFromEquipAttributeIconConfig(iAttributeType, "NAME");
            nameLabel.gradientBottom = new Color(255,255,255);
    }

    //by chenliang
    //begin

    /// <summary>
    /// 获取宠物字段
    /// </summary>
    /// <param name="tid"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    public static object GetPetField(int tid, GET_ITEM_FIELD_TYPE type)
    {
        if (DataCenter.mActiveConfigTable == null)
        {
            DEBUG.LogWarning("ActiveConfigTable is null.");
            return null;
        }
        string fieldName = "";
        switch (type)
        {
            case GET_ITEM_FIELD_TYPE.NAME: fieldName = "NAME"; break;
            case GET_ITEM_FIELD_TYPE.DESC: fieldName = "DESCRIBE"; break;
            case GET_ITEM_FIELD_TYPE.STAR_LEVEL: fieldName = "STAR_LEVEL"; break;
            case GET_ITEM_FIELD_TYPE.ATLAS_NAME: fieldName = "HEAD_ATLAS_NAME"; break;
            case GET_ITEM_FIELD_TYPE.SPRITE_NAME: fieldName = "HEAD_SPRITE_NAME"; break;
        }
        if (fieldName == "")
            return null;
        return GetPetField(tid, fieldName);
    }
    /// <summary>
    /// 获取宠物字段
    /// </summary>
    /// <param name="tid"></param>
    /// <param name="fieldName"></param>
    /// <returns></returns>
    public static object GetPetField(int tid, string fieldName)
    {
        if (DataCenter.mActiveConfigTable == null)
        {
            DEBUG.LogWarning("ActiveConfigTable is null.");
            return null;
        }
        if (fieldName == "")
            return null;
        DataRecord tmpConfig = DataCenter.mActiveConfigTable.GetRecord(tid);
        if (tmpConfig == null)
            return null;
        return tmpConfig.getObject(fieldName);
    }
    /// <summary>
    /// 获取宠物碎片字段
    /// </summary>
    /// <param name="tid"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    public static object GetPetFragmentField(int tid, GET_ITEM_FIELD_TYPE type)
    {
        if (DataCenter.mFragment == null)
        {
            DEBUG.LogWarning("Fragment is null.");
            return null;
        }
        string fieldName = "";
        switch (type)
        {
            case GET_ITEM_FIELD_TYPE.NAME: fieldName = "NAME"; break;
            case GET_ITEM_FIELD_TYPE.DESC: fieldName = "DESCRIPTION"; break;
            case GET_ITEM_FIELD_TYPE.STAR_LEVEL:
            case GET_ITEM_FIELD_TYPE.ATLAS_NAME:
            case GET_ITEM_FIELD_TYPE.SPRITE_NAME:
                {
                    int tmpTid = TableCommon.GetNumberFromFragment(tid, "ITEM_ID");
                    return GetPetField(tmpTid, type);
                } break;
        }
        if (fieldName == "")
            return null;
        return GetPetFragmentField(tid, fieldName);
    }
    /// <summary>
    /// 获取宠物碎片字段
    /// </summary>
    /// <param name="tid"></param>
    /// <param name="fieldName"></param>
    /// <returns></returns>
    public static object GetPetFragmentField(int tid, string fieldName)
    {
        if (DataCenter.mFragment == null)
        {
            DEBUG.LogWarning("Fragment is null.");
            return null;
        }
        if (fieldName == "")
            return null;
        DataRecord tmpConfig = DataCenter.mFragment.GetRecord(tid);
        if (tmpConfig == null)
            return null;
        return tmpConfig.getObject(fieldName);
    }
    /// <summary>
    /// 获取装备字段
    /// </summary>
    /// <param name="tid"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    public static object GetEquipField(int tid, GET_ITEM_FIELD_TYPE type)
    {
        if (DataCenter.mRoleEquipConfig == null)
        {
            DEBUG.LogWarning("RoleEquipConfig is null.");
            return null;
        }
        string fieldName = "";
        switch (type)
        {
            case GET_ITEM_FIELD_TYPE.NAME: fieldName = "NAME"; break;
            case GET_ITEM_FIELD_TYPE.DESC: fieldName = "DESCRIPTION"; break;
            case GET_ITEM_FIELD_TYPE.STAR_LEVEL: fieldName = "QUALITY"; break;
            case GET_ITEM_FIELD_TYPE.ATLAS_NAME: fieldName = "ICON_ATLAS_NAME"; break;
            case GET_ITEM_FIELD_TYPE.SPRITE_NAME: fieldName = "ICON_SPRITE_NAME"; break;
        }
        if (fieldName == "")
            return null;
        return GetEquipField(tid, fieldName);
    }
    /// <summary>
    /// 获取装备字段
    /// </summary>
    /// <param name="tid"></param>
    /// <param name="fieldName"></param>
    /// <returns></returns>
    public static object GetEquipField(int tid, string fieldName)
    {
        if (DataCenter.mRoleEquipConfig == null)
        {
            DEBUG.LogWarning("RoleEquipConfig is null.");
            return null;
        }
        if (fieldName == "")
            return null;
        DataRecord tmpConfig = DataCenter.mRoleEquipConfig.GetRecord(tid);
        if (tmpConfig == null)
            return null;
        return tmpConfig.getObject(fieldName);
    }
    /// <summary>
    /// 获取装备碎片字段
    /// </summary>
    /// <param name="tid"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    public static object GetEquipFragmentField(int tid, GET_ITEM_FIELD_TYPE type)
    {
        if (DataCenter.mFragment == null)
        {
            DEBUG.LogWarning("Fragment is null.");
            return null;
        }
        string fieldName = "";
        switch (type)
        {
            case GET_ITEM_FIELD_TYPE.NAME: fieldName = "NAME"; break;
            case GET_ITEM_FIELD_TYPE.DESC: fieldName = "DESCRIPTION"; break;
            case GET_ITEM_FIELD_TYPE.STAR_LEVEL:
            case GET_ITEM_FIELD_TYPE.ATLAS_NAME:
            case GET_ITEM_FIELD_TYPE.SPRITE_NAME:
                {
                    int tmpTid = TableCommon.GetNumberFromFragment(tid, "ITEM_ID");
                    return GetEquipField(tmpTid, type);
                } break;
        }
        if (fieldName == "")
            return null;
        return GetEquipFragmentField(tid, fieldName);
    }
    /// <summary>
    /// 获取装备碎片字段
    /// </summary>
    /// <param name="tid"></param>
    /// <param name="fieldName"></param>
    /// <returns></returns>
    public static object GetEquipFragmentField(int tid, string fieldName)
    {
        if (DataCenter.mFragment == null)
        {
            DEBUG.LogWarning("Fragment is null.");
            return null;
        }
        if (fieldName == "")
            return null;
        DataRecord tmpConfig = DataCenter.mFragment.GetRecord(tid);
        if (tmpConfig == null)
            return null;
        return tmpConfig.getObject(fieldName);
    }
    /// <summary>
    /// 获取法器碎片字段
    /// </summary>
    /// <param name="tid"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    public static object GetMagicFragmentField(int tid, GET_ITEM_FIELD_TYPE type)
    {
        if (DataCenter.mFragmentAdminConfig == null)
        {
            DEBUG.LogWarning("FragmentAdminConfig is null");
            return null;
        }
        string fieldName = "";
        switch (type)
        {
            case GET_ITEM_FIELD_TYPE.NAME: fieldName = "NAME"; break;
            case GET_ITEM_FIELD_TYPE.DESC: fieldName = "NAME"; break;
            case GET_ITEM_FIELD_TYPE.STAR_LEVEL:
            case GET_ITEM_FIELD_TYPE.ATLAS_NAME:
            case GET_ITEM_FIELD_TYPE.SPRITE_NAME:
                {
                    DataRecord tmpConfig = DataCenter.mFragmentAdminConfig.GetRecord(tid);
                    if (tmpConfig == null)
                        return null;
                    int tmpTid = (int)tmpConfig.getObject("ROLEEQUIPID");
                    return GetEquipField(tmpTid, type);
                } break;
        }
        if (fieldName == "")
            return null;
        return GetMagicFragmentField(tid, fieldName);
    }
    /// <summary>
    /// 获取法器碎片字段
    /// </summary>
    /// <param name="tid"></param>
    /// <param name="fieldName"></param>
    /// <returns></returns>
    public static object GetMagicFragmentField(int tid, string fieldName)
    {
        if (DataCenter.mFragmentAdminConfig == null)
        {
            DEBUG.LogWarning("FragmentAdminConfig is null");
            return null;
        }
        if (fieldName == "")
            return null;
        DataRecord tmpConfig = DataCenter.mFragmentAdminConfig.GetRecord(tid);
        if (tmpConfig == null)
            return null;
        return tmpConfig.getObject(fieldName);
    }

	private static int[] _expMagicEquipTid = new int[]{15201,15202,15203};
	public static bool CheckIsExpMagicEquip(int kTid)
	{
		bool _isExpMagic = false;
		for (int i = 0; i < _expMagicEquipTid.Length; i++) 
		{
			if(_expMagicEquipTid[i] == kTid)
			{
				_isExpMagic = true;
				return _isExpMagic;
			}
		}
		return _isExpMagic;
	}

    /// <summary>
    /// 获取消耗品字段
    /// </summary>
    /// <param name="tid"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    public static object GetConsumeField(int tid, GET_ITEM_FIELD_TYPE type)
    {
        if (DataCenter.mConsumeConfig == null)
        {
            DEBUG.LogWarning("ConsumeConfig is null.");
            return null;
        }
        string filedName = "";
        switch (type)
        {
            case GET_ITEM_FIELD_TYPE.NAME: filedName = "ITEM_NAME"; break;
            case GET_ITEM_FIELD_TYPE.DESC: filedName = "DESCRIBE"; break;
            case GET_ITEM_FIELD_TYPE.ATLAS_NAME: filedName = "ITEM_ATLAS_NAME"; break;
            case GET_ITEM_FIELD_TYPE.SPRITE_NAME: filedName = "ITEM_SPRITE_NAME"; break;
        }
        if (filedName == "")
            return null;
        return GetConsumeField(tid, filedName);
    }
    /// <summary>
    /// 获取消耗品字段
    /// </summary>
    /// <param name="tid"></param>
    /// <param name="fieldName"></param>
    /// <returns></returns>
    public static object GetConsumeField(int tid, string fieldName)
    {
        if (DataCenter.mConsumeConfig == null)
        {
            DEBUG.LogWarning("ConsumeConfig is null.");
            return null;
        }
        if (fieldName == "")
            return null;
        DataRecord tmpConfig = DataCenter.mConsumeConfig.GetRecord(tid);
        if (tmpConfig == null)
            return null;
        return tmpConfig.getObject(fieldName);
    }
    /// <summary>
    /// 获取角色字段
    /// </summary>
    /// <param name="tid"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    public static object GetRoleField(int tid, GET_ITEM_FIELD_TYPE type)
    {
        if (DataCenter.mItemIcon == null)
        {
            DEBUG.LogWarning("ItemIcon is null.");
            return null;
        }
        string filedName = "";
        switch (type)
        {
            case GET_ITEM_FIELD_TYPE.NAME: filedName = "NAME"; break;
            case GET_ITEM_FIELD_TYPE.DESC: filedName = "NAME"; break;
            case GET_ITEM_FIELD_TYPE.ATLAS_NAME: filedName = "ITEM_ATLAS_NAME"; break;
            case GET_ITEM_FIELD_TYPE.SPRITE_NAME: filedName = "ITEM_SPRITE_NAME"; break;
        }
        if (filedName == "")
            return null;
        return GetRoleField(tid, filedName);
    }
    /// <summary>
    /// 获取角色字段
    /// </summary>
    /// <param name="tid"></param>
    /// <param name="fieldName"></param>
    /// <returns></returns>
    public static object GetRoleField(int tid, string fieldName)
    {
        if (DataCenter.mItemIcon == null)
        {
            DEBUG.LogWarning("ItemIcon is null.");
            return null;
        }
        if (fieldName == "")
            return null;
        DataRecord tmpConfig = DataCenter.mItemIcon.GetRecord(tid);
        if (tmpConfig == null)
            return null;
        return tmpConfig.getObject(fieldName);
    }
    /// <summary>
    /// 获取主动技能数据
    /// </summary>
    /// <param name="tid"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    public static object GetSkillField(int tid, GET_ITEM_FIELD_TYPE type)
    {
        if (DataCenter.mSkillConfigTable == null)
        {
            DEBUG.LogWarning("SkillConfigTable is null.");
            return null;
        }
        string fieldName = "";
        switch (type)
        {
            case GET_ITEM_FIELD_TYPE.NAME: fieldName = "NAME"; break;
            case GET_ITEM_FIELD_TYPE.DESC: fieldName = "INFO"; break;
            case GET_ITEM_FIELD_TYPE.ATLAS_NAME: fieldName = "SKILL_ATLAS_NAME"; break;
            case GET_ITEM_FIELD_TYPE.SPRITE_NAME: fieldName = "SKILL_SPRITE_NAME"; break;
        }
        if (fieldName == "")
            return null;
        return GetSkillField(tid, fieldName);
    }
    /// <summary>
    /// 获取主动技能数据
    /// </summary>
    /// <param name="tid"></param>
    /// <param name="fieldName"></param>
    /// <returns></returns>
    public static object GetSkillField(int tid, string fieldName)
    {
        if (DataCenter.mSkillConfigTable == null)
        {
            DEBUG.LogWarning("SkillConfigTable is null.");
            return null;
        }
        if (fieldName == "")
            return null;
        DataRecord tmpConfig = DataCenter.mSkillConfigTable.GetRecord(tid);
        if (tmpConfig == null)
            return null;
        return tmpConfig.getObject(fieldName);
    }
    /// <summary>
    /// 获取被动技能数据
    /// </summary>
    /// <param name="tid"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    public static object GetSkillPassiveField(int tid, GET_ITEM_FIELD_TYPE type)
    {
        if (DataCenter.mAttackState == null)
        {
            DEBUG.LogWarning("AttackState is null.");
            return null;
        }
        string fieldName = "";
        switch (type)
        {
            case GET_ITEM_FIELD_TYPE.NAME: fieldName = "NAME"; break;
            case GET_ITEM_FIELD_TYPE.DESC: fieldName = "INFO"; break;
            case GET_ITEM_FIELD_TYPE.ATLAS_NAME: fieldName = "SKILL_ATLAS_NAME"; break;
            case GET_ITEM_FIELD_TYPE.SPRITE_NAME: fieldName = "SKILL_SPRITE_NAME"; break;
        }
        if (fieldName == "")
            return null;
        return GetSkillPassiveField(tid, fieldName);
    }
    /// <summary>
    /// 获取被动技能数据
    /// </summary>
    /// <param name="tid"></param>
    /// <param name="fieldName"></param>
    /// <returns></returns>
    public static object GetSkillPassiveField(int tid, string fieldName)
    {
        if (DataCenter.mAttackState == null)
        {
            DEBUG.LogWarning("AttackState is null.");
            return null;
        }
        if (fieldName == "")
            return null;
        DataRecord tmpConfig = DataCenter.mAttackState.GetRecord(tid);
        if (tmpConfig == null)
            return null;
        return tmpConfig.getObject(fieldName);
    }
    /// <summary>
    /// 获取BUFF数据
    /// </summary>
    /// <param name="tid"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    public static object GetBuffField(int tid, GET_ITEM_FIELD_TYPE type)
    {
        if (DataCenter.mAffectBuffer == null)
        {
            DEBUG.LogWarning("AffectBuffer is null.");
            return null;
        }
        string fieldName = "";
        switch (type)
        {
            case GET_ITEM_FIELD_TYPE.NAME: fieldName = "NAME"; break;
            case GET_ITEM_FIELD_TYPE.DESC: fieldName = "INFO"; break;
            case GET_ITEM_FIELD_TYPE.ATLAS_NAME: fieldName = "BUFFER_ATLAS_NAME"; break;
            case GET_ITEM_FIELD_TYPE.SPRITE_NAME: fieldName = "BUFFER_SPRITE_NAME"; break;
        }
        if (fieldName == "")
            return null;
        return GetBuffField(tid, fieldName);
    }
    /// <summary>
    /// 获取Buff数据
    /// </summary>
    /// <param name="tid"></param>
    /// <param name="fieldName"></param>
    /// <returns></returns>
    public static object GetBuffField(int tid, string fieldName)
    {
        if (DataCenter.mAffectBuffer == null)
        {
            DEBUG.LogWarning("AffectBuffer is null.");
            return null;
        }
        if (fieldName == "")
            return null;
        DataRecord tmpConfig = DataCenter.mAffectBuffer.GetRecord(tid);
        if (tmpConfig == null)
            return null;
        return tmpConfig.getObject(fieldName);
    }
    /// <summary>
    /// 获取物品数据
    /// </summary>
    /// <param name="tid"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    public static object GetItemField(int tid, GET_ITEM_FIELD_TYPE type)
    {
        object fieldValue = null;
        ITEM_TYPE tmpItemType = PackageManager.GetItemRealTypeByTableID(tid);

        switch (tmpItemType)
        {
            case ITEM_TYPE.CHARACTER:
            case ITEM_TYPE.PET: fieldValue = GetPetField(tid, type); break;
            case ITEM_TYPE.PET_FRAGMENT: fieldValue = GetPetFragmentField(tid, type); break;
            case ITEM_TYPE.EQUIP:
            case ITEM_TYPE.MAGIC: fieldValue = GetEquipField(tid, type); break;
            case ITEM_TYPE.EQUIP_FRAGMENT: fieldValue = GetEquipFragmentField(tid, type); break;
            case ITEM_TYPE.MAGIC_FRAGMENT: fieldValue = GetMagicFragmentField(tid, type); break;
            case ITEM_TYPE.ROLE_ATTRIBUTE: fieldValue = GetRoleField(tid, type); break;
            case ITEM_TYPE.NOVICE_PACKS: 
            case ITEM_TYPE.CONSUME_ITEM: fieldValue = GetConsumeField(tid, type); break;

            case ITEM_TYPE.SAODANG_POINT:
            case ITEM_TYPE.RESET_POINT:
            case ITEM_TYPE.LOCK_POINT:
            case ITEM_TYPE.HONOR_POINT: fieldValue = GetRoleField(tid, type); break;
            case ITEM_TYPE.GEM:
            case ITEM_TYPE.MATERIAL:
            case ITEM_TYPE.MATERIAL_FRAGMENT: break;

            case ITEM_TYPE.SKILL_TYPE_ACTIVE: fieldValue = GetSkillField(tid, type); break;
            case ITEM_TYPE.SKILL_TYPE_PASSIVE: fieldValue = GetSkillPassiveField(tid, type); break;
            case ITEM_TYPE.SKILL_TYPE_BUFF_PASSIVE:
            case ITEM_TYPE.SKILL_TYPE_BUFF_BREAK:
            case ITEM_TYPE.SKILL_TYPE_BUFF_EQUIP:
            case ITEM_TYPE.SKILL_TYPE_BUFF_EXT_1:
            case ITEM_TYPE.SKILL_TYPE_BUFF_EXT_2:
            case ITEM_TYPE.SKILL_TYPE_BUFF_EXT_3:
            case ITEM_TYPE.SKILL_TYPE_BUFF_RELATIONSHIP:
            case ITEM_TYPE.SKILL_TYPE_BUFF_EXT_4:
            case ITEM_TYPE.SKILL_TYPE_BUFF_EXT_5:
            case ITEM_TYPE.SKILL_TYPE_BUFF_EXT_6: fieldValue = GetBuffField(tid, type); break;
        }

        return fieldValue;
    }
    /// <summary>
    /// 获取物品数据
    /// </summary>
    /// <param name="tid"></param>
    /// <param name="fieldName"></param>
    /// <returns></returns>
    public static object GetItemField(int tid, string fieldName)
    {
        object fieldValue = null;
        ITEM_TYPE tmpItemType = PackageManager.GetItemRealTypeByTableID(tid);

        switch (tmpItemType)
        {
            case ITEM_TYPE.CHARACTER:
            case ITEM_TYPE.PET: fieldValue = GetPetField(tid, fieldName); break;
            case ITEM_TYPE.PET_FRAGMENT: fieldValue = GetPetFragmentField(tid, fieldName); break;
            case ITEM_TYPE.EQUIP:
            case ITEM_TYPE.MAGIC: fieldValue = GetEquipField(tid, fieldName); break;
            case ITEM_TYPE.EQUIP_FRAGMENT: fieldValue = GetEquipFragmentField(tid, fieldName); break;
            case ITEM_TYPE.MAGIC_FRAGMENT: fieldValue = GetMagicFragmentField(tid, fieldName); break;
            case ITEM_TYPE.ROLE_ATTRIBUTE: fieldValue = GetRoleField(tid, fieldName); break;
            case ITEM_TYPE.CONSUME_ITEM: fieldValue = GetConsumeField(tid, fieldName); break;

            case ITEM_TYPE.SAODANG_POINT:
            case ITEM_TYPE.RESET_POINT:
            case ITEM_TYPE.LOCK_POINT:
            case ITEM_TYPE.HONOR_POINT: fieldValue = GetRoleField(tid, fieldName); break;
            case ITEM_TYPE.GEM:
            case ITEM_TYPE.MATERIAL:
            case ITEM_TYPE.MATERIAL_FRAGMENT: break;

            case ITEM_TYPE.SKILL_TYPE_ACTIVE: fieldValue = GetSkillField(tid, fieldName); break;
            case ITEM_TYPE.SKILL_TYPE_PASSIVE: fieldValue = GetSkillPassiveField(tid, fieldName); break;
            case ITEM_TYPE.SKILL_TYPE_BUFF_PASSIVE:
            case ITEM_TYPE.SKILL_TYPE_BUFF_BREAK:
            case ITEM_TYPE.SKILL_TYPE_BUFF_EQUIP:
            case ITEM_TYPE.SKILL_TYPE_BUFF_EXT_1:
            case ITEM_TYPE.SKILL_TYPE_BUFF_EXT_2:
            case ITEM_TYPE.SKILL_TYPE_BUFF_EXT_3:
            case ITEM_TYPE.SKILL_TYPE_BUFF_RELATIONSHIP:
            case ITEM_TYPE.SKILL_TYPE_BUFF_EXT_4:
            case ITEM_TYPE.SKILL_TYPE_BUFF_EXT_5:
            case ITEM_TYPE.SKILL_TYPE_BUFF_EXT_6: fieldValue = GetBuffField(tid, fieldName); break;
        }

        return fieldValue;
    }
    /// <summary>
    /// 获取物品数字数据
    /// </summary>
    /// <param name="tid"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    public static int GetItemNumberField(int tid, GET_ITEM_FIELD_TYPE type)
    {
        object tmpValue = GetItemField(tid, type);
        if (tmpValue == null)
            return 0;
        return System.Convert.ToInt32(tmpValue);
    }
    /// <summary>
    /// 获取物品数字数据
    /// </summary>
    /// <param name="tid"></param>
    /// <param name="fieldName"></param>
    /// <returns></returns>
    public static int GetItemNumberField(int tid, string fieldName)
    {
        object tmpValue = GetItemField(tid, fieldName);
        if (tmpValue == null)
            return 0;
        return System.Convert.ToInt32(tmpValue);
    }
    /// <summary>
    /// 获取物品字符串数据
    /// </summary>
    /// <param name="tid"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    public static string GetItemStringField(int tid, GET_ITEM_FIELD_TYPE type)
    {
        object tmpValue = GetItemField(tid, type);
        if (tmpValue == null)
            return "";
        return System.Convert.ToString(tmpValue);
    }
    /// <summary>
    /// 获取物品字符串数据
    /// </summary>
    /// <param name="tid"></param>
    /// <param name="fieldName"></param>
    /// <returns></returns>
    public static string GetItemStringField(int tid, string fieldName)
    {
        object tmpValue = GetItemField(tid, fieldName);
        if (tmpValue == null)
            return "";
        return System.Convert.ToString(tmpValue);
    }
    /// <summary>
    /// 获取物品浮点型数据
    /// </summary>
    /// <param name="tid"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    public static float GetItemFloatField(int tid, GET_ITEM_FIELD_TYPE type)
    {
        object tmpValue = GetItemField(tid, type);
        if (tmpValue == null)
            return 0.0f;
        return System.Convert.ToSingle(tmpValue);
    }
    /// <summary>
    /// 获取物品浮点型数据
    /// </summary>
    /// <param name="tid"></param>
    /// <param name="fieldName"></param>
    /// <returns></returns>
    public static float GetItemFloatField(int tid, string fieldName)
    {
        object tmpValue = GetItemField(tid, fieldName);
        if (tmpValue == null)
            return 0.0f;
        return System.Convert.ToSingle(tmpValue);
    }
    /// <summary>
    /// 获取物品名称
    /// </summary>
    /// <param name="tid"></param>
    /// <returns></returns>
    public static string GetItemName(int tid)
    {
        return GetItemStringField(tid, GET_ITEM_FIELD_TYPE.NAME);
    }
    /// <summary>
    /// 获取物品描述
    /// </summary>
    /// <param name="tid"></param>
    /// <returns></returns>
    public static string GetItemDesc(int tid)
    {
        return GetItemStringField(tid, GET_ITEM_FIELD_TYPE.DESC);
    }
    /// <summary>
    /// 获取物品星级
    /// </summary>
    /// <param name="tid"></param>
    /// <returns></returns>
    public static int GetItemStarLevel(int tid)
    {
        return GetItemNumberField(tid, GET_ITEM_FIELD_TYPE.STAR_LEVEL);
    }

    /// <summary>
    /// 获取指定物品itemId
    /// </summary>
    /// <param name="tid"></param>
    /// <returns></returns>
    public static int GetItemId(int tid)
    {
        ITEM_TYPE tmpType = PackageManager.GetItemTypeByTableID(tid);
        int itemId = -1;
        switch (tmpType)
        {
            case ITEM_TYPE.PET_FRAGMENT:
                {
                    PetFragmentData tmpPetFragData = PetFragmentLogicData.Self.GetItemDataByTid(tid);
                    if (tmpPetFragData != null)
                        itemId = tmpPetFragData.itemId;
                } break;
            case ITEM_TYPE.MAGIC_FRAGMENT:
                {
                    MagicFragmentData tmpMagicFragData = MagicFragmentLogicData.Self.GetItemDataByTid(tid) as MagicFragmentData;
                    if (tmpMagicFragData != null)
                        itemId = tmpMagicFragData.itemId;
                }break;
            case ITEM_TYPE.CONSUME_ITEM: itemId = ConsumeItemLogicData.Self.GetItemIdByTid(tid); break;
        }
        return itemId;
    }

    /// <summary>
    /// 获取指定物品的图集、精灵名称
    /// </summary>
    /// <param name="tid"></param>
    /// <param name="strAtlasName"></param>
    /// <param name="strSpriteName"></param>
    public static void GetItemAtlasSpriteName(int tid, out string strAtlasName, out string strSpriteName)
    {
        strAtlasName = GetItemStringField(tid, GET_ITEM_FIELD_TYPE.ATLAS_NAME);
        strSpriteName = GetItemStringField(tid, GET_ITEM_FIELD_TYPE.SPRITE_NAME);
    }
    /// <summary>
    /// 根据物品tid设置物品图标
    /// </summary>
    /// <param name="goIcon"></param>
    /// <param name="tid"></param>
    /// <param name="count"></param>
    public static void SetItemIconEx(GameObject goIcon, int tid, int count)
    {
        if (goIcon == null)
            return;

        //设置图标
        UISprite tmpSpriteIcon = goIcon.GetComponent<UISprite>();
        if (tmpSpriteIcon == null)
            tmpSpriteIcon = FindComponent<UISprite>(goIcon, "itemIcon");
        if (tmpSpriteIcon != null)
        {
            string tmpAtlasName, tmpSpriteName;
            GetItemAtlasSpriteName(tid, out tmpAtlasName, out tmpSpriteName);
            SetIcon(tmpSpriteIcon, tmpAtlasName, tmpSpriteName);
        }

        //设置名称
        UILabel tmpLabelName = FindComponent<UILabel>(goIcon, "itemName");
        if (tmpLabelName != null)
        {
            string tmpItemName = GetItemName(tid);
            tmpLabelName.text = tmpItemName;
        }

        //设置数量
        UILabel tmpLabelCount = FindComponent<UILabel>(goIcon, "itemCount");
        if (tmpLabelCount != null)
            tmpLabelCount.text = "x" + count.ToString();
    }
    /// <summary>
    /// 根据物品tid设置物品图标
    /// </summary>
    /// <param name="goIconParent"></param>
    /// <param name="iconName"></param>
    /// <param name="tid"></param>
    /// <param name="count"></param>
    public static void SetItemIconEx(GameObject goIconParent, string iconName, int tid, int count)
    {
        if (goIconParent == null)
            return;

        SetItemIconEx(GameCommon.FindObject(goIconParent, iconName), tid, count);
    }
    /// <summary>
    /// 根据物品tid设置物品图标
    /// </summary>
    /// <param name="goIcon"></param>
    /// <param name="tid"></param>
    /// <param name="count"></param>
	public static void SetOnlyItemIcon(GameObject goIcon, int tid,string kFragIconName = "FragmentMask")
    {
        if (goIcon == null)
            return;
    
        //设置图标
        UISprite tmpSpriteIcon = goIcon.GetComponent<UISprite>();
        if (tmpSpriteIcon == null)
            tmpSpriteIcon = FindComponent<UISprite>(goIcon, "itemIcon");
        if (tmpSpriteIcon != null)
        {
            string tmpAtlasName, tmpSpriteName;
            GetItemAtlasSpriteName(tid, out tmpAtlasName, out tmpSpriteName);
            SetIcon(tmpSpriteIcon, tmpAtlasName, tmpSpriteName);
			// 设置碎片
			SetFragmentIcon(tmpSpriteIcon.gameObject,tid,kFragIconName);
        }
    }
	public static void AddButtonAction(GameObject button, Action action)
	{
		var evt = button.GetComponent<UIButtonEvent>();
		if (evt == null) DEBUG.LogError("No exist button event > " + button.name);
		else evt.AddAction(action);
	}
    /// <summary>
    /// 根据物品tid设置物品个数
    /// </summary>
    /// <param name="goIcon"></param>
    /// <param name="tid"></param>
    /// <param name="count"></param>
    public static void SetOnlyItemCount(GameObject goIconParent, string countName, int count)
    {
        if (goIconParent == null)
            return;

        SetOnlyItemCount(GameCommon.FindObject(goIconParent, countName), count);
    }
    /// <summary>
    /// 根据物品tid设置物品个数
    /// </summary>
    /// <param name="goIcon"></param>
    /// <param name="tid"></param>
    /// <param name="count"></param>
    public static void SetOnlyItemCount(GameObject goIcon, int count)
    {
        if (goIcon == null)
            return;

        //设置个数
        UILabel tmpLBCount = goIcon.GetComponent<UILabel>();
        if (tmpLBCount == null)
            tmpLBCount = FindComponent<UILabel>(goIcon, "itemCount");
        if (tmpLBCount != null)
            tmpLBCount.text = "x" + count;
    }

	private static void SetFragmentIcon(GameObject kParentObj,int kTid,string kFragIconName)
	{
		GameObject _fragIconObj = GameCommon.FindObject (kParentObj,kFragIconName);
		bool _isFrag = GameCommon.CheckIsFragmentByTid (kTid);
		UISprite _fragSprite = null;
		if (!_isFrag && _fragIconObj != null) 
		{
			_fragIconObj.SetActive (false);
		}
		else if (_isFrag && _fragIconObj != null) 
		{
			_fragSprite = _fragIconObj.GetComponent<UISprite>();
			_fragSprite.atlas = LoadUIAtlas("ElementAtlas");
			_fragSprite.spriteName = "a_ui_suipian";
            _fragSprite.gameObject.SetActive(true);
		}
		else if (_isFrag && _fragIconObj == null) 
		{
			GameObject tmpGO = new GameObject(kFragIconName);
			_fragSprite = tmpGO.AddComponent<UISprite>();
			_fragSprite.gameObject.name = kFragIconName;
			_fragSprite.transform.parent = kParentObj.transform;
			_fragSprite.transform.localPosition = Vector3.zero;
			_fragSprite.transform.localScale = Vector3.one;
			_fragSprite.atlas = LoadUIAtlas("ElementAtlas");
			_fragSprite.spriteName = "a_ui_suipian"; 
			_fragSprite.gameObject.SetActive(true);

			UISprite _parentIcon = kParentObj.GetComponent<UISprite>();
			//1.设置大小
			_fragSprite.width = _parentIcon.width;
			_fragSprite.height = _parentIcon.height;
			//2.设置层级
			_fragSprite.depth = _parentIcon.depth + 1;
		}

	}

    /// <summary>
    /// 根据物品tid设置物品图标
    /// </summary>
    /// <param name="goIconParent"></param>
    /// <param name="iconName"></param>
    /// <param name="tid"></param>
    /// <param name="count"></param>
    public static void SetOnlyItemIcon(GameObject goIconParent, string iconName, int tid)
    {
        if (goIconParent == null)
            return;

        SetOnlyItemIcon(GameCommon.FindObject(goIconParent, iconName), tid);
    }

	/// <summary>
	/// 根据有无边框设置资源图标
	/// </summary>
	/// <param name="kParentObj">K parent object.</param>
	/// <param name="kSpriteObjName">K sprite object name.</param>
	/// <param name="kTid">K tid.</param>
	/// <param name="kHasFrame">If set to <c>true</c> k has frame.</param>
	public static void SetResIcon(GameObject kParentObj,string kSpriteObjName,int kTid,bool kHasFrame,bool kIsAveragreSize = false)
	{
        GameObject _spriteObj = GameCommon.FindObject(kParentObj,kSpriteObjName);
        if(_spriteObj == null)
            return;
        UISprite _sprite = _spriteObj.GetComponent<UISprite>();
        if (_sprite == null)
            return;
		string _atlasColName = string.Empty;
		string _spriteColName = string.Empty;
		if (kHasFrame) 
		{
			_atlasColName = "ITEM_ATLAS_NAME";
			_spriteColName = "ITEM_SPRITE_NAME";
		}
		else 
		{
			_atlasColName = "ITEM_ICON_ATLAS";
			_spriteColName = "ITEM_ICON_SPRITE";
		}
		DataRecord _iconRecord = DataCenter.mItemIcon.GetRecord (kTid);
		string _atlasName = _iconRecord.getObject (_atlasColName).ToString();
		string _spriteName = _iconRecord.getObject (_spriteColName).ToString();
		SetIcon (_sprite,_atlasName,_spriteName);

		if (kIsAveragreSize) 
		{
			_sprite.MakePixelPerfect();
			_sprite.keepAspectRatio = UIWidget.AspectRatioSource.BasedOnWidth;
			if(_sprite.width > 40)
				_sprite.width = 40;
			else
				_sprite.keepAspectRatio = UIWidget.AspectRatioSource.Free;
		}
	}

    /// <summary>
    /// 获取VIP配置
    /// </summary>
    /// <param name="vipLevel"></param>
    /// <returns></returns>
    public static DataRecord GetVIPConfig(int vipLevel)
    {
        DataRecord tmpVIPConfig = DataCenter.mVipListConfig.GetRecord(vipLevel);
        return tmpVIPConfig;
    }

    public static int GetMaxVipLevel()
	{
        int ret = 0;
		for(int i = 0; i < DataCenter.mVipListConfig.GetRecordCount (); i++)
		{
            DataRecord record = DataCenter.mVipListConfig.GetRecord(i);
            int vipLevel = record["INDEX"];
            if (ret < vipLevel)
            {
                ret = vipLevel;
            }
		}
        return ret;
	}

    private static int mMaxBreakLevel = 0;
    /// <summary>
    /// 获得最大突破等级
    /// </summary>
    /// <returns></returns>
    public static int GetMaxBreakLevelByTid(int kTid) 
    {
        int _petMaxBreakLevel = TableCommon.GetNumberFromActiveCongfig(kTid, "MAX_LEVEL_BREAK");
        if (mMaxBreakLevel == 0) 
        {
            foreach (KeyValuePair<int, DataRecord> v in DataCenter.mBreakLevelConfig.GetAllRecord())
            {
                if (v.Key != null)
                    mMaxBreakLevel++;
            }
        }
        return Mathf.Min(_petMaxBreakLevel, mMaxBreakLevel);
    }

    private static int mMaxFateLevel = int.MinValue;
    public static int GetMaxFateLevel()
    {
        if (mMaxFateLevel != int.MinValue)
            return mMaxFateLevel;
        foreach (KeyValuePair<int, DataRecord> v in DataCenter.mFateConfig.GetAllRecord())
        {
            if (v.Key != null)
            {
                mMaxFateLevel++;
            }
        }
        return mMaxFateLevel;
    }


    //1970时间
    private static DateTime ms1970 = new DateTime(1970, 1, 1, 8, 0, 0);
    /// <summary>
    /// 将服务器时间（以秒为单位）转为1970时间
    /// </summary>
    /// <param name="time"></param>
    /// <returns></returns>
    public static DateTime ConvertServerSecTimeTo1970(long time)
    {
        DateTime dateTime = new DateTime(time * TimeSpan.TicksPerSecond);
        dateTime = dateTime.Add(new TimeSpan(ms1970.Ticks));
        return dateTime;
    }
    /// <summary>
    /// 将服务器时间（以毫秒为单位）转为1970时间
    /// </summary>
    /// <param name="time"></param>
    /// <returns></returns>
    public static DateTime ConvertServerMSTimeTo1970(long time)
    {
        DateTime dateTime = new DateTime(time * TimeSpan.TicksPerMillisecond);
        dateTime = dateTime.Add(new TimeSpan(ms1970.Ticks));
        return dateTime;
    }

    /// <summary>
    /// 获取指定UILabel包含的CountdownUI
    /// </summary>
    /// <param name="goLabel"></param>
    /// <returns></returns>
    public static CountdownUI GetCountdownUI(GameObject goLabel)
    {
        if (goLabel == null)
            return null;

        CountdownUI tmpCountdown = null;
        UILabel tmpLabel = goLabel.GetComponent<UILabel>();
        if (tmpLabel != null)
            tmpCountdown = goLabel.GetComponent<CountdownUI>();
        return tmpCountdown;
    }
    /// <summary>
    /// 获取指定UILabel包含的CountdownUI
    /// </summary>
    /// <param name="goParent"></param>
    /// <param name="labelName"></param>
    /// <returns></returns>
    public static CountdownUI GetCountdownUI(GameObject goParent, string labelName)
    {
        if (goParent == null)
            return null;

        CountdownUI tmpCountdown = null;
        GameObject tmpGOLabel = GameCommon.FindObject(goParent, labelName);
        return GetCountdownUI(tmpGOLabel);
    }

    public const float PowerDelta = 3.0f;     //客户端、服务器计算战力相差最大值
    /// <summary>
    /// 获取当前战斗力
    /// </summary>
    /// <returns></returns>
    public static float GetPower()
    {
        float tmpPower = 0.0f;
        for (int i = (int)TEAM_POS.CHARACTER, count = (int)TEAM_POS.MAX; i < count; i++)
        {
            float tmpTeamPower = GetPowerByTeamPos((TEAM_POS)i);
            tmpPower += tmpTeamPower;
        }
        return tmpPower;
    }
    /// <summary>
    /// 获取队伍位置战斗力
    /// </summary>
    /// <param name="teamPos"></param>
    /// <returns></returns>
    public static float GetPowerByTeamPos(TEAM_POS teamPos)
    {
        float tmpPower = 0.0f;
        int tmpTeamPos = (int)teamPos;
        ActiveData tmpData = TeamManager.GetActiveDataByTeamPos(tmpTeamPos);
        if (tmpData == null)
            return 0.0f;
        int tmpTid = tmpData.tid;
        //Affect tmpAffect = AffectFunc.GetPetAllAffect(tmpTeamPos);
        //Affect relAffect = Relationship.GetTotalAffect(tmpTeamPos);
        Affect affect = AffectFunc.GetPetAllAffect(tmpTeamPos);

        //if (tmpAffect == null)
        //    tmpAffect = new Affect();

        //if (relAffect == null)
        //    relAffect = new Affect();

        //攻击力
        float tmpAttack = GetBaseAttack(tmpTid, tmpData.level, tmpData.breakLevel);
        //tmpAttack = tmpAffect.Final(AFFECT_TYPE.ATTACK, tmpAttack);
        //tmpAttack = relAffect.Final(AFFECT_TYPE.ATTACK, tmpAttack);
        tmpAttack = AffectFunc.Final(affect, AFFECT_TYPE.ATTACK, tmpAttack);
        AddAttributePower(ref tmpPower, tmpAttack, AFFECT_TYPE.ATTACK);

        //物防
        float tmpPhysicsDef = GetBasePhysicalDefence(tmpTid, tmpData.level, tmpData.breakLevel);
        //tmpPhysicsDef = tmpAffect.Final(AFFECT_TYPE.PHYSICAL_DEFENCE, tmpPhysicsDef);
        //tmpPhysicsDef = relAffect.Final(AFFECT_TYPE.PHYSICAL_DEFENCE, tmpPhysicsDef);
        tmpPhysicsDef = AffectFunc.Final(affect, AFFECT_TYPE.PHYSICAL_DEFENCE, tmpPhysicsDef);
        AddAttributePower(ref tmpPower, tmpPhysicsDef, AFFECT_TYPE.PHYSICAL_DEFENCE);

        //法防
        float tmpMagicDef = GetBaseMagicDefence(tmpTid, tmpData.level, tmpData.breakLevel);
        //tmpMagicDef = tmpAffect.Final(AFFECT_TYPE.MAGIC_DEFENCE, tmpMagicDef);
        //tmpMagicDef = relAffect.Final(AFFECT_TYPE.MAGIC_DEFENCE, tmpMagicDef);
        tmpMagicDef = AffectFunc.Final(affect, AFFECT_TYPE.MAGIC_DEFENCE, tmpMagicDef);
        AddAttributePower(ref tmpPower, tmpMagicDef, AFFECT_TYPE.MAGIC_DEFENCE);

        //生命
        float tmpHp = GetBaseMaxHP(tmpTid, tmpData.level, tmpData.breakLevel);
        //tmpHp = tmpAffect.Final(AFFECT_TYPE.HP_MAX, tmpHp);
        //tmpHp = relAffect.Final(AFFECT_TYPE.HP_MAX, tmpHp);
        tmpHp = AffectFunc.Final(affect, AFFECT_TYPE.HP_MAX, tmpHp);
        AddAttributePower(ref tmpPower, tmpHp, AFFECT_TYPE.HP_MAX);

        //暴击率
        float tmpCriticalStrikeRate = GetBaseCriticalStrikeRate(tmpTid);
        //tmpCriticalStrikeRate = tmpAffect.Final(AFFECT_TYPE.CRITICAL_STRIKE_RATE, tmpCriticalStrikeRate);
        //tmpCriticalStrikeRate = relAffect.Final(AFFECT_TYPE.CRITICAL_STRIKE_RATE, tmpCriticalStrikeRate);
        tmpCriticalStrikeRate = AffectFunc.Final(affect, AFFECT_TYPE.CRITICAL_STRIKE_RATE, tmpCriticalStrikeRate);
        AddAttributePower(ref tmpPower, tmpCriticalStrikeRate, AFFECT_TYPE.CRITICAL_STRIKE_RATE);

        //抗暴率
        float tmpDefenceCriticalStrikeRate = GetBaseDefenceCriticalStrikeRate(tmpTid);
        //tmpDefenceCriticalStrikeRate = tmpAffect.Final(AFFECT_TYPE.DEFENCE_CRITICAL_STRIKE_RATE, tmpDefenceCriticalStrikeRate);
        //tmpDefenceCriticalStrikeRate = relAffect.Final(AFFECT_TYPE.DEFENCE_CRITICAL_STRIKE_RATE, tmpDefenceCriticalStrikeRate);
        tmpDefenceCriticalStrikeRate = AffectFunc.Final(affect, AFFECT_TYPE.DEFENCE_CRITICAL_STRIKE_RATE, tmpDefenceCriticalStrikeRate);
        AddAttributePower(ref tmpPower, tmpDefenceCriticalStrikeRate, AFFECT_TYPE.DEFENCE_CRITICAL_STRIKE_RATE);

        //命中率
        float tmpHitTargetRate = GetBaseHitRate(tmpTid) - 1;// 去掉基础命中
        //tmpHitTargetRate = tmpAffect.Final(AFFECT_TYPE.HIT_TARGET_RATE, tmpHitTargetRate);
        //tmpHitTargetRate = relAffect.Final(AFFECT_TYPE.HIT_TARGET_RATE, tmpHitTargetRate);
        tmpHitTargetRate = AffectFunc.Final(affect, AFFECT_TYPE.HIT_TARGET_RATE, tmpHitTargetRate);
        AddAttributePower(ref tmpPower, tmpHitTargetRate, AFFECT_TYPE.HIT_TARGET_RATE);

        //闪避率
        float tmpDodgeRate = GetBaseDodgeRate(tmpTid);
        //tmpDodgeRate = tmpAffect.Final(AFFECT_TYPE.DODGE_RATE, tmpDodgeRate);
        //tmpDodgeRate = relAffect.Final(AFFECT_TYPE.DODGE_RATE, tmpDodgeRate);
        tmpDodgeRate = AffectFunc.Final(affect, AFFECT_TYPE.DODGE_RATE, tmpDodgeRate);
        AddAttributePower(ref tmpPower, tmpDodgeRate, AFFECT_TYPE.DODGE_RATE);

        //伤害率
        float tmpDamageEnhanceRate = GetBaseDamageEnhanceRate(tmpTid);
        //tmpDamageEnhanceRate = tmpAffect.Final(AFFECT_TYPE.HIT_RATE, tmpDamageEnhanceRate);
        //tmpDamageEnhanceRate = relAffect.Final(AFFECT_TYPE.HIT_RATE, tmpDamageEnhanceRate);
        tmpDamageEnhanceRate = AffectFunc.Final(affect, AFFECT_TYPE.HIT_RATE, tmpDamageEnhanceRate);
        AddAttributePower(ref tmpPower, tmpDamageEnhanceRate, AFFECT_TYPE.HIT_RATE);

        //减伤率
        float tmpDamageMitigationRate = GetBaseDamageMitigationRate(tmpTid);
        //tmpDamageMitigationRate = tmpAffect.Final(AFFECT_TYPE.DEFENCE_HIT_RATE, tmpDamageMitigationRate);
        //tmpDamageMitigationRate = relAffect.Final(AFFECT_TYPE.DEFENCE_HIT_RATE, tmpDamageMitigationRate);
        tmpDamageMitigationRate = AffectFunc.Final(affect, AFFECT_TYPE.DEFENCE_HIT_RATE, tmpDamageMitigationRate);
        AddAttributePower(ref tmpPower, tmpDamageMitigationRate, AFFECT_TYPE.DEFENCE_HIT_RATE);

        //tmpPower += ((tmpCriticalStrikeRate + tmpDefenceCriticalStrikeRate + tmpHitTargetRate + tmpDodgeRate + tmpDamageEnhanceRate + tmpDamageMitigationRate) * 160.0f * 100);

        //主动技能
        // add by LC
        // begin

        //int tmpSkillId = GetItemNumberField(tmpTid, "PET_SKILL_1");
        //float tmpSkillStart = GetItemFloatField(tmpSkillId, "START_BATTLE");
        //float tmpSkillUpgrade = GetItemFloatField(tmpSkillId, "UPGRADE_BATTLE");
        //tmpPower += (tmpSkillStart + tmpSkillUpgrade * tmpData.skillLevel[0]);

        // 遍历宠物的所有技能
        if (teamPos != (int)TEAM_POS.CHARACTER)
        {
            for (int index = 1; index <= tmpData.skillLevel.Length; index++)
            {
                int tmpSkillId = GetItemNumberField(tmpTid, "PET_SKILL_" + index);
                float tmpSkillStart = GetItemFloatField(tmpSkillId, "START_BATTLE");
                float tmpSkillUpgrade = GetItemFloatField(tmpSkillId, "UPGRADE_BATTLE");
                tmpPower += (tmpSkillStart + tmpSkillUpgrade * tmpData.skillLevel[index - 1]);
            }
        }
        
        // end

        return tmpPower;
    }


    private static float GetAttributeRatio(AFFECT_TYPE affectType)
    {
        DataRecord dataRecord = DataCenter.mEquipAttributeIconConfig.GetRecord((int)affectType);
        if (dataRecord != null)
            return (float)dataRecord.get("ATTRIBUTE_BATTLE");
        return 1;
    }

    private static float GetAttributePower(float value, AFFECT_TYPE affectType)
    {
        return value * GetAttributeRatio(affectType);
    }

    private static void AddAttributePower(ref float power, float value, AFFECT_TYPE affectType)
    {
        power += GetAttributePower(value, affectType);
    }

    /// <summary>
    /// 直接通过战场对象的静态属性计算战斗力
    /// </summary>
    /// <param name="obj"> 战场对象 </param>
    /// <returns> 战斗力 </returns>
    public static float GetPowerByObject(BaseObject obj)
    {
        float tmpPower = 0.0f;

        //攻击力
        tmpPower += (obj.mStaticAttack * 1.25f);

        //物防
        tmpPower += (obj.mStaticPhysicalDefence * 1.65f);

        //法防
        tmpPower += (obj.mStaticMagicDefence * 1.65f);

        //生命
        tmpPower += (obj.mStaticMaxHp * 0.095f);

        // 其他属性
        tmpPower += ((obj.mStaticCriticalStrikeRate + obj.mStaticDefenceCriticalStrikeRate + obj.mStaticHitRate - 1f + obj.mStaticDodgeRate + obj.mStaticDamageEnhanceRate + obj.mStaticDamageMitigationRate) * 160.0f * 100);

        // 技能
        Pet pet = obj as Pet;

        if (pet != null)
        {
            for (int index = 1; index <= 4; index++)
            {
                int tmpSkillId = GetItemNumberField(obj.mConfigIndex, "PET_SKILL_" + index);

                if (tmpSkillId > 0)
                {
                    float tmpSkillStart = GetItemFloatField(tmpSkillId, "START_BATTLE");
                    float tmpSkillUpgrade = GetItemFloatField(tmpSkillId, "UPGRADE_BATTLE");
                    tmpPower += (tmpSkillStart + tmpSkillUpgrade * pet.GetConfigSkillLevel(index - 1));
                }            
            }
        }

        return tmpPower;
    }

    /// <summary>
    /// 获取当前队伍总的攻击力
    /// </summary>
    /// <returns> 攻击力 </returns>
    public static int GetTeamTotalAttack()
    {
        int attack = 0;

        for (int i = (int)TEAM_POS.CHARACTER, count = (int)TEAM_POS.MAX; i < count; i++)
        {
            attack += GetAttackByTeamPos((TEAM_POS)i);
        }

        return attack;
    }

    /// <summary>
    /// 获取指定TeamPos的攻击力
    /// </summary>
    /// <param name="teamPos"> 阵位 </param>
    /// <returns> 攻击力 </returns>
    public static int GetAttackByTeamPos(TEAM_POS teamPos)
    {
        int tmpTeamPos = (int)teamPos;
        ActiveData tmpData = TeamManager.GetActiveDataByTeamPos(tmpTeamPos);

        if (tmpData == null)
            return 0;

        int tmpTid = tmpData.tid;
        int attack = GetBaseAttack(tmpTid, tmpData.level, tmpData.breakLevel);
        return (int)AffectFunc.Final(tmpTeamPos, AFFECT_TYPE.ATTACK, attack);
    }

    /// <summary>
    /// 获取包裹名称
    /// </summary>
    /// <param name="packageType"></param>
    /// <returns></returns>
    public static string GetPackageName(PACKAGE_TYPE packageType)
    {
        string tmpPackageName = "";
        switch (packageType)
        {
            case PACKAGE_TYPE.PET: tmpPackageName = "灵将"; break;
            case PACKAGE_TYPE.PET_FRAGMENT: tmpPackageName = "灵将碎片"; break;
            case PACKAGE_TYPE.EQUIP: tmpPackageName = "装备"; break;
            case PACKAGE_TYPE.EQUIP_FRAGMENT: tmpPackageName = "装备碎片"; break;
            case PACKAGE_TYPE.MAGIC: tmpPackageName = "神器"; break;
            case PACKAGE_TYPE.MAGIC_FRAGMENT: tmpPackageName = "神器碎片"; break;
            case PACKAGE_TYPE.CONSUME_ITEM: tmpPackageName = "消耗品"; break;
        }
        return tmpPackageName;
    }

//    //通用排序
//    public static void Sort<T>(List<T> a, List<Func<T, T, int>> compair)
//    {
//        a.Sort((T tmpL, T tmpR) =>
//        {
//            for (int i = 0, count = compair.Count; i < count; i++)
//            {
//                int tmp = compair[i](tmpL, tmpR);
//                if (tmp == 0)
//                    continue;
//                return tmp;
//            }
//            return 0;
//        });
//    }

    public static void ApplicationQuit()
    {
#if !UNITY_EDITOR && !NO_USE_SDK
        if (CommonParam.isUseSDK)
            U3DSharkSDK.Instance.ExitGame();
        else
#endif
            Application.Quit();
    }

    public static void OpenToLoginWindow(STRING_INDEX stringIndex, Action closeCallback)
    {
        string tmpWinName = "CONNECT_ERROR_TOLOGIN_WINDOW";
        DataCenter.SetData(tmpWinName, "WINDOW_CONTENT", stringIndex);
        DataCenter.SetData(tmpWinName, "CLOSE_CALLBACK", closeCallback);
    }

    //end

    /// <summary>
    /// 获得物品数据对象
    /// </summary>
    /// <param name="itemDataBase"></param>
    /// <returns></returns>
    public static ItemDataBase GetItemDataBase(ItemDataBase itemDataBase)
    {
        if(itemDataBase != null)
        {
            return GetItemDataBase(itemDataBase.itemId, itemDataBase.tid);
        }
        return null;
    }

    public static ItemDataBase GetItemDataBase(int iItemId, int iTid)
    {
        switch (PackageManager.GetItemTypeByTableID(iTid))
        {
            case ITEM_TYPE.PET: return PetLogicData.Self.GetPetDataByItemId(iItemId);
            case ITEM_TYPE.PET_FRAGMENT: return PetFragmentLogicData.Self.GetItemDataByTid(iTid);
            case ITEM_TYPE.EQUIP: return RoleEquipLogicData.Self.GetEquipDataByItemId(iItemId);
            case ITEM_TYPE.EQUIP_FRAGMENT: return RoleEquipFragmentLogicData.Self.GetItemDataByTid(iTid);
            case ITEM_TYPE.MAGIC: return MagicLogicData.Self.GetEquipDataByItemId(iItemId);
            case ITEM_TYPE.MAGIC_FRAGMENT: return MagicFragmentLogicData.Self.GetItemDataByTid(iTid);
            case ITEM_TYPE.CONSUME_ITEM: return ConsumeItemLogicData.Self.GetDataByTid(iTid);
        }
        return null;
    }

    /// <summary>
    /// 切换按钮
    /// 当多个按钮位于同一个UIToggle组中时
    /// 使用此方法可以切换至特定按钮，同时调用该按钮的响应方法（UIButtonEvent.Send）
    /// 如果在初始化窗口时调用，请确保所有按钮的startingState没有被勾选
    /// </summary>
    /// <param name="target"> 被切换的按钮 </param>
    public static void ToggleButton(GameObject target)
    {
        UIToggle uiToggle = target.GetComponent<UIToggle>();

        if (uiToggle != null)
        {
            uiToggle.value = true;
        }

        UIButtonEvent uiButton = target.GetComponent<UIButtonEvent>();

        if (uiButton != null)
        {
            uiButton.Send();
        }
    }

    /// <summary>
    /// 切换按钮
    /// 当多个按钮位于同一个UIToggle组中时
    /// 使用此方法可以切换至特定按钮，同时调用该按钮的响应方法（UIButtonEvent.Send）
    /// 如果在初始化窗口时调用，请确保所有按钮的startingState没有被勾选
    /// </summary>
    /// <param name="parentObj"> 父物体 </param>
    /// <param name="targetName"> 目标按钮名称 </param>
    public static void ToggleButton(GameObject parentObj, string targetName)
    {
        GameObject target = GameCommon.FindObject(parentObj, targetName);

        if (target != null)
        {
            ToggleButton(target);
        }
    }

    /// <summary>
    /// 获取按钮上挂载的UIToggledObjects组件上的第一个activate对象
    /// </summary>
    /// <param name="target"> 目标按钮 </param>
    /// <returns></returns>
    public static GameObject GetToggledObject(GameObject target)
    {
        UIToggledObjects uiToggled = target.GetComponent<UIToggledObjects>();
        return uiToggled != null ? uiToggled.activate[0] : null;
    }

    /// <summary>
    /// 获取按钮上挂载的UIToggledObjects组件上的第一个activate对象
    /// </summary>
    /// <param name="parentObj"> 父物体 </param>
    /// <param name="targetName"> 目标按钮名称 </param>
    /// <returns></returns>
    public static GameObject GetToggledObject(GameObject parentObj, string targetName)
    {
        GameObject target = GameCommon.FindObject(parentObj, targetName);

        if (target != null)
        {
            return GetToggledObject(target);
        }

        return null;
    }

    public static TEAM_POS_TYPE GetTeamPosType(PetData itemData)
    {
        if(itemData != null)
        {
            if(itemData.teamPos >= (int)TEAM_POS.PET_1 && itemData.teamPos <= (int)TEAM_POS.PET_3)
                return TEAM_POS_TYPE.IN_TEAM;
            else if(itemData.teamPos >= (int)TEAM_POS.RELATE_1 && itemData.teamPos <= (int)TEAM_POS.RELATE_6)
                return TEAM_POS_TYPE.IN_RELATE;
        }
        return TEAM_POS_TYPE.NOT_IN_TEAM;
    }

    public static int GetPetDataQuality(PetData itemData)
    {
        if (itemData != null && itemData.tid != 30293 && itemData.tid != 30299 && itemData.tid != 30305)
        {
            return itemData.starLevel;
        }
        else if (itemData.tid == 30305)
        {
            return -1;
        }
        else if (itemData.tid == 30299)
        {
            return -2;
        }
        else if (itemData.tid == 30293)
        {
            return -3;
        }
        return -5;
    }
    public static bool IsExpPet(int kTid)
    {
        if (kTid == 30293 || kTid == 30299 || kTid == 30305)
        {
            return true;
        }
        return false;
    }
    public static bool IsExpPet(PetData itemData)
    {
        if (itemData == null)
            return false;
        return IsExpPet(itemData.tid);
    }

    public static IEnumerator CreateMainBossInFrames(int modelIndex,GameObject mMainObject) {
        /*lhc?*/

        DEBUG.Log(modelIndex.ToString());

        foreach(Transform form in mMainObject.transform) {
            form.gameObject.SetActive(false);
            GameObject.Destroy(form.gameObject);
        }

        DEBUG.Log("GetRecord-Start:"+Time.time);

        DataRecord record = DataCenter.mMonsterObject.GetRecord(modelIndex);
        DEBUG.Log((record==null).ToString());
        DEBUG.Log(((int)record.getData("MODEL")).ToString());
        DataRecord modelRecord=DataCenter.mModelTable.GetRecord((int)record.getData("MODEL"));
        if(modelRecord!=null) {
            string anim=modelRecord.getData("ANIMATION");
            string body=modelRecord.getData("BODY");
            DEBUG.Log(anim+"____"+body);
            DEBUG.Log("GetRecord-End:"+Time.time);
            yield return new WaitForSeconds(0.1f);

            if(anim!=""&&body!="") {
                DEBUG.Log("GetAnimatior-Start:"+Time.time);
                RuntimeAnimatorController control=GameCommon.LoadController(anim);
                DEBUG.Log("GetAnimatior-End:"+Time.time);
//                yield return null;
                yield return new WaitForSeconds(0.2f);

                if(control==null)
                    EventCenter.Log(LOG_LEVEL.ERROR,"Anim control no exist > "+anim);

                string tex1=modelRecord.getData("BODY_TEX_1");
                //mBodyObject=_CreatePart(body,mMainObject,control,tex1);


                GameObject obj=GameCommon.LoadModel(body);
                GameCommon.SetLayer(obj,mMainObject.layer);
                Animator animator=obj.GetComponentInChildren<Animator>();
               
                if(tex1!="") {
                    Texture tex=GameCommon.LoadTexture(tex1,LOAD_MODE.RESOURCE);
                    if(tex!=null) {
                        GameCommon.SetObjectTexture(obj,tex);
                    }
                }
                yield return new WaitForSeconds(0.3f);

                //yield return null;
                
                if(animator!=null) {
                    animator.applyRootMotion=false;
                    animator.runtimeAnimatorController=control;
                }
                //yield return new WaitForSeconds(0.4f);
                                
//                yield return null;
                
                obj.transform.parent=mMainObject.transform;
                obj.transform.localScale=Vector3.one;
                obj.transform.localRotation=Quaternion.identity;
                obj.transform.localPosition=Vector3.zero;
                obj.name="lhcObj";
            }
        }
   
    }

	/// <summary>
	/// 根据tid来花费相应的货币
	/// </summary>
	/// <param name="kTid">K tid.</param>
	/// <param name="kNum">K number.</param>
	public static void ConsumeCoinByTid(int kTid,int kNum)
	{
		int _consumeNum = -1 * kNum;
		switch (kTid) 
		{
		case 1000001:
			RoleLogicData.Self.AddDiamond(_consumeNum);
			break;
		case 1000002:
			RoleLogicData.Self.AddGold(_consumeNum);
			break;
		case 1000003:
			RoleLogicData.Self.AddStamina(_consumeNum);
			break;
		case 1000004:
			RoleLogicData.Self.AddSoulPoint(_consumeNum);
			break;
		case 1000005:
			RoleLogicData.Self.AddSpirit(_consumeNum);
			break;
		case 1000006:
			RoleLogicData.Self.AddReputation(_consumeNum);
			break;
		case 1000007:
			RoleLogicData.Self.AddPrestige(_consumeNum);
			break;
		case 1000008:
			RoleLogicData.Self.AddBattleAchv(_consumeNum);
			break;
		case 1000009:
			RoleLogicData.Self.AddUnionContr(_consumeNum);
			break;
		case 1000010:
			RoleLogicData.Self.AddBeatDemonCard(_consumeNum);
			break;
		default:
			DEBUG.LogError("该方法中未提供该tid对应的货币处理");
			break;
		}
	}

	/// <summary>
	/// g
	/// </summary>
	/// <returns><c>true</c>, if is fragment by tid was checked, <c>false</c> otherwise.</returns>
	/// <param name="kTid">K tid.</param>
	public static bool CheckIsFragmentByTid(int kTid)
	{
		ITEM_TYPE _type = PackageManager.GetItemTypeByTableID (kTid);
		if (_type == ITEM_TYPE.EQUIP_FRAGMENT || _type == ITEM_TYPE.PET_FRAGMENT || _type == ITEM_TYPE.MAGIC_FRAGMENT) 
			return true;
		return false;
	}

    public float GetTotalAddByLevel(int skillTid,int skillLv,string field) 
    {
        string[] addvalue = TableCommon.GetStringFromAffectBuffer(skillTid, field).Split('/');
        float sum = 0;
        foreach (string s in addvalue)
        {
            float e;
            float.TryParse(s, out e);
            sum +=e;
        }
        int n = addvalue.Length;
        int x = (skillLv - 1) / n;
        int y = (skillLv - 1) % n - 1;
        float z = 0;
        while (y >= 0)
        {
            float e;
            float.TryParse(addvalue[y], out e);
            z += e;
            y--;
        }

        float _addAffectValue = x * sum + z;
        return _addAffectValue;
    }

    public int GetTotalAddDamageByLevel(int skillTid, int skillLv, string field)
    {
        string[] addvalue = TableCommon.GetStringFromSkillConfig(skillTid, field).Split('/');
        int sum = 0;
        foreach (string s in addvalue)
        {
            int e;
            int.TryParse(s, out e);
            sum += e;
        }
        int n = addvalue.Length;
        int x = (skillLv - 1) / n;
        int y = (skillLv - 1) % n - 1;
        int z = 0;
        while (y >= 0)
        {
            int e;
            int.TryParse(addvalue[y], out e);
            z += e;
            y--;
        }

        int _addDamageValue = x * sum + z;
        return _addDamageValue;
    }
	// 根据占位符初始化技能描述信息
	public static string InitSkillDescription(int kSkillLv,int kSkillTid)
    {
        string _skillInfo = string.Empty;
        if (kSkillTid > 0)
        {      
            int buffBaseProbability = TableCommon.GetNumberFromSkillConfig(kSkillTid, "BUFF_PROBABILITY_BASE");
            string[] addprobability = TableCommon.GetStringFromSkillConfig(kSkillTid, "BUFF_PROBABILITY_ADD").Split('/');
            float _sum = 0;
            foreach (string s in addprobability)
            {
                float e;
                float.TryParse(s, out e);
                _sum += e;
            }
            int _n = addprobability.Length;
            int _x = (kSkillLv - 1) / _n;
            int _y = (kSkillLv - 1) % _n - 1;
            float _z = 0;
            while (_y >= 0)
            {
                float e;
                float.TryParse(addprobability[_y], out e);
                _z += e;
                _y--;
            }

            float addBuffProbability = _x * _sum + _z;
            float totalprobability = (buffBaseProbability + addBuffProbability) / 100f;
            //1.首先判断当前技能是什么类型,目前只有两张表Skill表和AffectBuffer表
            if ((kSkillTid < 200000) || 400000 <= kSkillTid)
            {
                // 如果是主动技能
                _skillInfo = TableCommon.GetStringFromSkillConfig(kSkillTid, "INFO");
                int _damage = TableCommon.GetNumberFromSkillConfig(kSkillTid, "DAMAGE");
                string[] addvalue = TableCommon.GetStringFromSkillConfig(kSkillTid, "ADD_DAMAGE").Split('/');
                int sum = 0;
                foreach (string s in addvalue)
                {
                    int e;
                    int.TryParse(s, out e);
                    sum += e;
                }
                int n = addvalue.Length;
                int x = (kSkillLv - 1) / n;
                int y = (kSkillLv - 1) % n - 1;
                int z = 0;
                while (y >= 0)
                {
                    int e;
                    int.TryParse(addvalue[y], out e);
                    z += e;
                    y--;
                }
                int _addDamageValue = x * sum + z;
                float total_damage = (float)(_damage + _addDamageValue) / 100f;
                //float _damageRate = TableCommon.GetFloatFromSkillConfig(kSkillTid, "DAMAGE_RATE");
                //float _addDamageRate = TableCommon.GetFloatFromSkillConfig(kSkillTid, "ADD_DAMAGE_RATE");
                int _needMp = TableCommon.GetNumberFromSkillConfig(kSkillTid, "NEED_MP");
                int _addNeedMap = TableCommon.GetNumberFromSkillConfig(kSkillTid, "ADD_NEED_MP");
                float _CDTime = TableCommon.GetFloatFromSkillConfig(kSkillTid, "CD_TIME");
                float _addCDTime = TableCommon.GetFloatFromSkillConfig(kSkillTid, "ADD_CD_TIME");
          
                int _curLv = Mathf.Max(0, (kSkillLv));
                int _damageNum = TableCommon.GetNumberFromSkillConfig(kSkillTid, "DAMAGE_NUM");
                int _addDamageNum = TableCommon.GetNumberFromSkillConfig(kSkillTid, "ADD_DAMAGE_NUM");
                int _finalDamage = _damageNum + _addDamageNum * (_curLv - 1);

                // {0} 使用在主动技能中，表示当前（技能等级-1）的伤害数值（DAMAGE+ADD_DAMAGE×（技能等级-1）÷100）
                _skillInfo = _skillInfo.Replace("{0}", (total_damage).ToString("f1")).Replace("\\n","\n");
                // {1} 使用在主动技能中，表示当前（技能等级-1）的触发buff概率（(BUFF_PROBABILITY_BASE＋BUFF_PROBABILITY_ADD)÷100）
                _skillInfo = _skillInfo.Replace("{1}", (totalprobability).ToString().Replace("\\n", "\n"));
                //{2} 使用在主动技能中，表示当前技能等级的耗蓝（NEED_MP+ADD_NEED_MP×技能等级）
                _skillInfo = _skillInfo.Replace("{2}", (_needMp + _addNeedMap * _curLv).ToString()).Replace("\\n", "\n");
                //{3} 使用在主动技能中，表示当前技能等级的冷却时间（CD_TIME+ADD_CD_TIME×技能等级）
                _skillInfo = _skillInfo.Replace("{3}", (_CDTime + _addCDTime * _curLv).ToString()).Replace("\\n", "\n");
                //{7} 使用在主动技能中，表示当前技能等级的几率
                //_skillInfo = _skillInfo.Replace("{7}", (totalprobability).ToString());
                //{9}使用在主动技能中，表示当前（技能等级-1）的伤害固定数值（DAMAGE_MUM+ADD_DAMAGE_NUM×（技能等级-1）） 
                _skillInfo = _skillInfo.Replace("{9}", _finalDamage.ToString()).Replace("\\n", "\n");
                                                                                                    
                //added by xuke begin 主动技能中可能有关联的BUFF技能,需要替换{4}和{5}
                int _buffAdditionID = TableCommon.GetNumberFromSkillConfig(kSkillTid, "BUFF_ADDITION");
                if (_buffAdditionID != 0) 
                {
                    float _buffValue_0 = GetAffectBuffValue(_buffAdditionID, kSkillLv, "AFFECT_VALUE", "ADD_AFFECT_VALUE");
                    float _buffValue_1 = GetAffectBuffValue(_buffAdditionID, kSkillLv, "AFFECT_VALUE_1", "ADD_AFFECT_VALUE_1");
                    //{4} 使用在被动技能中，表示当前buff的第一条属性值（AFFECT_VALUE+ADD_AFFECT_VALUE×（技能等级-1）÷100）
                    _skillInfo = _skillInfo.Replace("{4}", Mathf.Abs(_buffValue_0).ToString()).Replace("\\n", "\n");
                    //{5} 使用在被动技能中，表示当前buff的第二条属性值
                    _skillInfo = _skillInfo.Replace("{5}", Mathf.Abs(_buffValue_1).ToString()).Replace("\\n", "\n");
                    //{10}使用在BUFF技能中，表示当前buff的第一条属性类型的攻击力加成值（base_coeff+add_coeff×（技能等级-1））*100
                    string coeffParamText = TableCommon.GetStringFromAffectBuffer(_buffAdditionID, "VALUE_COEFF");
                    ConfigParam coeffParam = new ConfigParam(coeffParamText);
                    float coeff = coeffParam.GetFloat("base_coeff") * 100 + GameCommon.LevelUpValue(coeffParam.GetFloatArray("add_coeff"), kSkillLv - 1) * 100;
                    coeff = coeff >= 0 ? coeff : coeff * -1;
                    _skillInfo = _skillInfo.Replace("{10}",coeff.ToString()).Replace("\\n", "\n");
                    //{11}使用在BUFF技能中，表示当前buff的第二条属性类型的攻击力加成值（base_coeff_1+add_coeff_1×（技能等级-1））*100}
                    float coeff1 = coeffParam.GetFloat("base_coeff_1") * 100 + GameCommon.LevelUpValue(coeffParam.GetFloatArray("add_coeff_1"), kSkillLv - 1) * 100;
                    coeff1 = coeff1 >= 0 ? coeff1 : coeff1 * -1;
                    _skillInfo = _skillInfo.Replace("{11}", coeff1.ToString()).Replace("\\n", "\n");   
                }
                //end
            }
            else if (300000 <= kSkillTid && kSkillTid < 400000)
            {
                _skillInfo = TableCommon.GetStringFromAffectBuffer(kSkillTid, "INFO");
                // 如果是被动技能
                int _affectValue = TableCommon.GetNumberFromAffectBuffer(kSkillTid, "AFFECT_VALUE");
                string[] addvalue = TableCommon.GetStringFromAffectBuffer(kSkillTid, "ADD_AFFECT_VALUE").Split('/');

                float _buffValue_0 = GetAffectBuffValue(kSkillTid, kSkillLv, "AFFECT_VALUE", "ADD_AFFECT_VALUE");
                float _buffValue_1 = GetAffectBuffValue(kSkillTid, kSkillLv, "AFFECT_VALUE_1", "ADD_AFFECT_VALUE_1");

                //{4} 使用在被动技能中，表示当前buff的第一条属性值（AFFECT_VALUE+ADD_AFFECT_VALUE×（技能等级-1）÷100）
                _skillInfo = _skillInfo.Replace("{4}", Mathf.Abs(_buffValue_0).ToString()).Replace("\\n", "\n");
                //{5} 使用在被动技能中，表示当前buff的第二条属性值
                _skillInfo = _skillInfo.Replace("{5}", Mathf.Abs(_buffValue_1).ToString()).Replace("\\n", "\n");
                //{6} 使用在被动技能中，表示当前buff的持续时间
                float baseDuration = (float)(TableCommon.GetObjectFromAffectBuffer(kSkillTid, "TIME"));
                float addDuration = 0;
                float duration = baseDuration + addDuration;
                _skillInfo = _skillInfo.Replace("{6}", duration.ToString()).Replace("\\n", "\n");
                //{8} 使用在被动技能中，表示当前技能等级的几率
                _skillInfo = _skillInfo.Replace("{8}", (totalprobability).ToString()).Replace("\\n", "\n");   
            }
            return _skillInfo;
        }
		return _skillInfo;
	}

    private static float GetAffectBuffValue(int kAffectBufferID,int kSkillLv,string kBaseColName,string kAddColName) 
    {
        int _affectValue = TableCommon.GetNumberFromAffectBuffer(kAffectBufferID, kBaseColName);
        string[] addvalue = TableCommon.GetStringFromAffectBuffer(kAffectBufferID, kAddColName).Split('/');
        float sum = 0;
        foreach (string s in addvalue)
        {
            float e;
            float.TryParse(s, out e);
            sum += e;
        }
        int n = addvalue.Length;
        int x = (kSkillLv - 1) / n;
        int y = (kSkillLv - 1) % n - 1;
        float z = 0;
        while (y >= 0)
        {
            float e;
            float.TryParse(addvalue[y], out e);
            z += e;
            y--;
        }
        float _addAffectValue = x * sum + z;     //获取buff总的成长属性（第一条属性，按各等级相加）  

        string affectType = TableCommon.GetStringFromAffectBuffer(kAffectBufferID, "AFFECT_TYPE");
        float totalAffectValue = 0;
        if (affectType.EndsWith("_RATE"))
        {
            totalAffectValue = (_affectValue + _addAffectValue) / 100f;
        }
        else
        {
            totalAffectValue = _affectValue + _addAffectValue;
        }
        return totalAffectValue;
    }

	// 判断是否要显示"暂无获取途径"的提示
	private static void CheckShowNoPahtInfo(UIGridContainer grid)
	{
		if (grid == null)
			return;
		GameObject _infoObj = GameCommon.FindObject (grid.transform.parent.gameObject,"no_path_to_get_label");
        if(_infoObj != null)
		    _infoObj.SetActive (grid.MaxCount == 0);
	}


    private static int GetGainFuncConfigIndexByDifficulty(int kDifficulty) 
    {
        switch (kDifficulty) 
        {
            case 1: //> 轻松副本
                return 10021;
            case 2: //> 普通副本
                return 10022;
        }
        return 0;
    }
	///<summary>
	/// get Path 
	/// </summary>
	public static void GetParth(UIGridContainer grid, int iTid)
	{
		int iWayNum = 0;
		string gainByStage = TableCommon.GetStringFromResourceGainConfig(iTid, "GAIY_BY_STAGE");
		string gainByFunction = TableCommon.GetStringFromResourceGainConfig(iTid, "GAIN_BY_FUNCTION");
		int gainByShopindex = TableCommon.GetNumberFromResourceGainConfig (iTid, "GAIN_BY_SHOPINDEX");
		string[] gainByStages = gainByStage.Split ('|');
		string[] gainByFunctions = gainByFunction.Split ('|');
		int iStageNum = gainByStages.Length;
		int iFunctionNum = gainByFunctions.Length;
		
		if(gainByShopindex == 0)
		{
			iWayNum = gainByStages.Length + gainByFunctions.Length;
			for(int i = 0; i < gainByStages.Length; i++)
			{
				if(gainByStages[i] == "0" || gainByStages[i] == "")
				{
					iWayNum--;
					iStageNum--;
				}
			}
			for(int j = 0; j < gainByFunctions.Length; j++)
			{
				if(gainByFunctions[j] == "0" || gainByFunctions[j] == "")
				{
					iWayNum --;
					iFunctionNum--;
				}
			}
		}else
		{
			iWayNum = gainByStages.Length + gainByFunctions.Length + 1;
			for(int i = 0; i < gainByStages.Length; i++)
			{
				if(gainByStages[i] == "0" || gainByStages[i] == "")
				{
					iWayNum--;
					iStageNum--;
				}
			}
			for(int j = 0; j < gainByFunctions.Length; j++)
			{
				if(gainByFunctions[j] == "0" || gainByFunctions[j] == "")
				{
					iWayNum --;
					iFunctionNum--;
				}
			}
		}
		if (grid != null)
		{
			grid.MaxCount = iWayNum;
			int j = 0;
			for (int i = 0; i < grid.MaxCount; i++)
			{
				GameObject obj = grid.controlList[i];
				
				if (obj != null)
				{
					GameObject goToGetBtnObj = GameCommon.FindObject (obj, "go_to_get_btn").gameObject;
					GameObject goToGetBtnGrayObj = GameCommon.FindObject (obj, "go_to_get_btn_gray").gameObject;
					UILabel tipsLabel = GameCommon.FindObject (obj, "tips").GetComponent<UILabel>();
					UILabel titleNameLabel = GameCommon.FindObject (obj, "title_name").GetComponent<UILabel>();
					UISprite iconSprite = GameCommon.FindObject (obj, "icon").GetComponent<UISprite>();
					UILabel numLabel = GameCommon.FindObject (obj, "num").GetComponent<UILabel>();
					goToGetBtnObj.SetActive(false);
					goToGetBtnGrayObj.SetActive (true);
					numLabel.gameObject.SetActive (false);
					tNiceData data = GameCommon.GetButtonData(obj, "go_to_get_btn");
					if(j < iStageNum)
					{
						if((gainByStages[i] != "0" || gainByStages[i] != ""))
						{
							int iIndex = System.Convert.ToInt32 (gainByStages[i]);

                            titleNameLabel.text = "主线副本";
                            string _stageNumber = TableCommon.GetStringFromStageConfig(iIndex, "STAGENUMBER");
                            string _stageName = TableCommon.GetStringFromStageConfig(iIndex, "NAME");
                            tipsLabel.text = _stageNumber + " " + _stageName;//TableCommon.GetStringFromStageConfig (iIndex, "NAME");
							j++;

							int iDifficulty = TableCommon.GetNumberFromStageConfig(iIndex, "DIFFICULTY");
                            //added by xuke 设置关卡图标
                            string atlasName = TableCommon.GetStringFromGainFunctionConfig(GetGainFuncConfigIndexByDifficulty(iDifficulty), "ICON_ATLAS_NAME");
                            string spriteName = TableCommon.GetStringFromGainFunctionConfig(GetGainFuncConfigIndexByDifficulty(iDifficulty), "ICON_SPRITE_NAME");
                            GameCommon.SetIcon(iconSprite, atlasName, spriteName);
                            //end
							List<DataRecord> stageID = TableCommon.FindAllRecords (DataCenter.mStagePoint, lhs => {return lhs["STAGEID"] == iIndex;});
                            if (stageID.Count == 0) break;
							int levelIndex = stageID[0].get ("INDEX");
							if (data != null)
							{
								data.set("FUNCTION_INDEX", -iIndex);
								data.set ("LEVEL_INDEX",levelIndex );
								switch ((STAGE_DIFFICULTY)iDifficulty)
								{
								case STAGE_DIFFICULTY.COMMON:
									data.set("DIFFICULTY_INDEX", 1);
									break;
								case STAGE_DIFFICULTY.DIFFICUL:
									data.set("DIFFICULTY_INDEX", 2);
									break;
								case STAGE_DIFFICULTY.MASTER:
									data.set("DIFFICULTY_INDEX", 3);
									break;								
								}
							}

							StageProperty stageIndex = StageProperty.Create(iIndex);
							if(stageIndex.unlocked)
							{
								goToGetBtnObj.SetActive(true);
								goToGetBtnGrayObj.SetActive (false);
							}
						}
					}else if(j >= iStageNum && j < iStageNum + iFunctionNum)
					{
						if((gainByFunctions[i - iStageNum] != "0" || gainByFunctions[i - iStageNum] != "") )
						{
							int iIndex = System.Convert.ToInt32 (gainByFunctions[j - iStageNum]);
							
							titleNameLabel.text = TableCommon.GetStringFromGainFunctionConfig (iIndex, "TITLE");
							tipsLabel.text = TableCommon.GetStringFromGainFunctionConfig (iIndex, "DESC");
							string atlasName =  TableCommon.GetStringFromGainFunctionConfig(iIndex, "ICON_ATLAS_NAME");
							string spriteName = TableCommon.GetStringFromGainFunctionConfig (iIndex, "ICON_SPRITE_NAME");
							GameCommon.SetIcon (iconSprite, atlasName, spriteName);
							j++;
							if (data != null)
							{
								data.set("FUNCTION_INDEX", iIndex);
							}
							//if (TableCommon.GetNumberFromFunctionConfig (iIndex, "FUNC_OPEN") != 0)
							if(IsFuncOpen(iIndex))
                            {
								goToGetBtnObj.SetActive(true);
								goToGetBtnGrayObj.SetActive (false);
							}
						}
					}else if(j >= iStageNum + iFunctionNum )
					{
						if(gainByShopindex != 0)
						{
							tipsLabel.text = gainByShopindex.ToString ();
							j++;
							if (data != null)
							{
								data.set("FUNCTION_INDEX", -1);
							}
							goToGetBtnObj.SetActive(true);
							goToGetBtnGrayObj.SetActive (false);
						}
					}
				}
			}
		}
		CheckShowNoPahtInfo(grid);
	}

    /// <summary>
    /// 判断当前关卡是否还有剩余挑战次数
    /// </summary>
    /// <param name="kStageID"></param>
    /// <returns></returns>
    public static bool HasLeftChallengeTimes(int kStageID)
    {
        MapData _mapData = MapLogicData.Instance.GetMapDataByStageIndex(kStageID);
        int _todaySuccessTimes = _mapData.todaySuccess;
        int _maxChallengeTimes = TableCommon.GetNumberFromStageConfig(kStageID, "CHALLENGE_NUM");
        return _todaySuccessTimes < _maxChallengeTimes;
    }

    /// <summary>
    /// 根据当前VIP等级判断拥有更多重置次数的下一级VIP等级，如果达到最大等级，则返回-1
    /// </summary>
    /// <returns></returns>
    public static int GetNextVipLevel()
    {
        // 当前等级的最大重置次数
        int _curVipLvResetTimes = TableCommon.GetNumberFromVipList(RoleLogicData.Self.vipLevel, "COPYRESET_NUM");
        foreach (DataRecord record in DataCenter.mVipListConfig.Records())
        {
            int _resetTimes = record.getData("COPYRESET_NUM");
            if (_resetTimes > _curVipLvResetTimes)
                return record.getData("INDEX");
        }
        return -1;
    }

    /// <summary>
    /// 判断指定跳转的功能是否开启
    /// </summary>
    /// <returns></returns>
    public static bool IsFuncOpen(int kGoToIndex) 
    {
        DataRecord _record = DataCenter.mGainFunctionConfig.GetRecord(kGoToIndex);
        if(_record == null)
        {
            DEBUG.LogError("mGainFunctionConfig 中不存在对应的index:" + kGoToIndex);
            return false;
        }
          
        int _funcID = (int)_record["Function_ID"];
        //> 开关是否开启
        bool _isOpenBySwitch = (TableCommon.GetNumberFromFunctionConfig (_funcID, "FUNC_OPEN")) != 0;  
        //> 当前等级是否开启
        bool _isOpenByLv = IsFuncCanUse(_funcID);
        return _isOpenBySwitch && _isOpenByLv;
    }

    public static int GetFuncCanUseLevelByFuncID(int kFuncID) 
    {
        // 使用等级
        int _openLv = 0;
        int _vipLv = RoleLogicData.Self.vipLevel;

        string _funcConditionUse = TableCommon.GetStringFromFunctionConfig(kFuncID, "FUNC_CONDITION_USE");
        string[] _lvArr = _funcConditionUse.Split('|');
        // 所有VIP等级的使用等级都一样
        if (_lvArr.Length == 1)
        {
            int.TryParse(_lvArr[0], out _openLv);
        }
        else
        {
            if (_lvArr.Length <= _vipLv)
            {
                DEBUG.LogError("FunctionConfig 中 funcID:(" + kFuncID + ")对应配置的 FUNC_CONDITION_USE 数量不正确");
                return int.MaxValue;
            }
            int.TryParse(_lvArr[_vipLv], out _openLv);
        }
        return _openLv;
    }

    /// <summary>
    /// 判断指定ID的功能是否开启使用
    /// </summary>
    /// <param name="funcID"></param>
    /// <returns></returns>
    public static bool IsFuncCanUse(int funcID)
    {
        // 使用等级
        int _openLv = 0;
        int _vipLv = RoleLogicData.Self.vipLevel;

        string _funcConditionUse = TableCommon.GetStringFromFunctionConfig(funcID, "FUNC_CONDITION_USE");
        string[] _lvArr = _funcConditionUse.Split('|');
        // 所有VIP等级的使用等级都一样
        if (_lvArr.Length == 1)
        {
            int.TryParse(_lvArr[0], out _openLv);
        }
        else 
        {            
            if (_lvArr.Length <= _vipLv) 
            {
                DEBUG.LogError("FunctionConfig 中 funcID:(" + funcID + ")对应配置的 FUNC_CONDITION_USE 数量不正确");
                return false;
            }
            int.TryParse(_lvArr[_vipLv],out _openLv);
        }

        return _openLv <= RoleLogicData.GetMainRole().level;
    }

    /// <summary>
    /// 判断指定ID的功能是否开启显示
    /// </summary>
    /// <param name="kFuncID"></param>
    /// <returns></returns>
    public static bool IsFuncCanShowByFuncID(int kFuncID) 
    {
        int _showLv = 0;
        int _vipLv = RoleLogicData.Self.vipLevel;
        
        string _vipShowLevelInfo = TableCommon.GetStringFromFunctionConfig(kFuncID, "VIP_SHOW_LEVEL");
        string[] _lvArr = _vipShowLevelInfo.Split('|');

        if (_lvArr.Length == 1) 
        {
            int.TryParse(_lvArr[0], out _showLv);            
        }
        else
        {
            if (_lvArr.Length <= _vipLv)
            {
                DEBUG.LogError("FunctionConfig 中 funcID:(" + kFuncID + ")对应配置的 FUNC_CONDITION_USE 数量不正确");
                return false;
            }
            int.TryParse(_lvArr[_vipLv], out _showLv);
        }
		DEBUG.Log ("当前等级---" + _showLv + "对比等级--" + RoleLogicData.GetMainRole().level);
        return _showLv <= RoleLogicData.GetMainRole().level;
    }

    public static bool IsFuncCanShow(GameObject parentObj, string buttonName)
    {
        bool isShowByLv = true;
        GameObject buttonObj = GameCommon.FindObject(parentObj, buttonName);
        if(buttonObj != null)
        {
            var evt = buttonObj.GetComponent<UIButtonEvent>();
            if (evt != null)
            {
                int btnId = evt.GetbtnID();
                isShowByLv = IsFuncCanShowByFuncID(btnId);//TableCommon.GetNumberFromFunctionConfig(btnId, "SHOW_LEVEL") <= RoleLogicData.GetMainRole().level;
            }
        }
        return isShowByLv;
    }

    public static DataRecord GetSkillDataRecord(int kSkillID)
    {
        if (kSkillID >= 300001 && kSkillID < 400001)
            return DataCenter.mAffectBuffer.GetRecord(kSkillID);
        else if ((0 < kSkillID && kSkillID < 20000) || kSkillID > 40000)
            return DataCenter.mSkillConfigTable.GetRecord(kSkillID);
        return null;
    }

    // 设置装备/法器基础属性信息
    public static void SetBaseAttrInfo(GameObject kParent, int kAttrType, int kAttrValue)
    {
        GameCommon.SetAttributeIcon(kParent, kAttrType, "icon");
        GameCommon.SetAttributeName(kParent, kAttrType, "name");
        UILabel _attrValue = GameCommon.FindComponent<UILabel>(kParent, "num_label");
        float _realAffectValue = kAttrValue / 10000f;
        if (GameCommon.IsAffectTypeRate((AFFECT_TYPE)kAttrType))
        {
            _realAffectValue *= 100f;
            _attrValue.text = _realAffectValue.ToString("f2") + "%";
        }
        else
            _attrValue.text = _realAffectValue.ToString("f0");
    }

    //设置精炼属性
    public static void SetRefineAttribute(GameObject obj, int attrType, int attrValue)
    {
        GameCommon.SetAttributeIcon(obj, attrType, "icon");
        GameCommon.SetAttributeName(obj, attrType, "name");
        if (GameCommon.IsAffectTypeRate((AFFECT_TYPE)attrType))
            GameCommon.SetUIText(obj, "num_label", (attrValue / 100f).ToString("f2") + "%");
        else
            GameCommon.SetUIText(obj, "num_label", (attrValue / 10000f).ToString());
    }

    public static void ShowResNotEnoughWin(RESOURCE_HINT_TYPE kResType) 
    {
        ResourceHintData _resourceHintData = new ResourceHintData();
        _resourceHintData.tid = (int)kResType;
        _resourceHintData.ResourceType = kResType;
        DataCenter.OpenWindow("ADD_VITALITY_UP_WINDOW", _resourceHintData);
    }

    private static string[] mEquipTypeDesc = { "武器","饰品","衣服","鞋子","防御","攻击","防御"};
    /// <summary>
    /// 获得装备类型描述
    /// </summary>
    /// <param name="kType"></param>
    /// <returns></returns>
    public static string GetEquipTypeDesc(int kType) 
    {
        if(kType - 1 < 0)
            return "";
        return "【"+mEquipTypeDesc[kType - 1]+"】";
    }
    public static string GetEquipTypeName(int kType)
    {
        if (kType - 1 < 0)
            return "";
        return mEquipTypeDesc[kType - 1];
    }

	static Color[] colorName = new Color[]
	{
		new Color (1f, 1f, 1f),	
		new Color (0.8f, 0.8f, 0.8f),		 //白色阶
		new Color (0f, 0.6f, 0f),            //绿色阶
		new Color (0f, 0.6f, 0.8f),          //蓝色阶
		new Color (0.8f, 0.2f, 1f),          //紫色阶
		new Color (1f, 0.6f, 0f),            //橙色阶
		new Color (1f, 0f, 0f),              //红色阶
		new Color (1f, 0.4f, 0.6f)			 //粉色阶

	};

	static string[] QualityName = new string[]
	{
		" ",
		"白色品质",
		"绿色品质",
		"蓝色品质",
		"紫色品质",
		"橙色品质",
		"红色品质",
		"粉色品质",
	};
	static string[] StrQualityColor = new string[]
	{
		" ",
		"[cccccc]",
		"[009900]",
		"[0099cc]",
		"[cc33ff]",
		"[ff9900]",
		"[ff0000]",
		"[ff6699]",
	};

    public static string ClearRelateStrColorPrefix(string kRelateInfo) 
    {
        string _retStr = string.Empty;
        _retStr = kRelateInfo.Replace("[-]", "");
        for (int i = 0, count = colorName.Length; i < count; i++) 
        {
            _retStr = _retStr.Replace(ExtensionString.GetColorPrefixByColor(colorName[i]), "");
        }
        return _retStr;
    }
	public static Color GetNameColor(int tid)
	{
		int iQualityType = GetItemQuality(tid);
		if (iQualityType < 0|| iQualityType > 6)
			return colorName[0];
		return colorName[iQualityType];
	}

    public static Color GetNameColorByQuality( int quality)
    {
        if (quality < 0 || quality > colorName.Length)
            return colorName[0];
        return colorName[quality];
    }

	//根据品质类型 设置 品质颜色
	public static Color SetQualityColor(int iQualityType)
	{
		if (iQualityType < 0|| iQualityType > 6)
			return colorName[0];
		return colorName[iQualityType];
	}
	//根据品质类型 设置 品质名字
	public static string SetQualityName(int iQualityType)
	{
		if (iQualityType < 0|| iQualityType > 6)
			return QualityName[0];
		return QualityName[iQualityType];
	}
	//根据品质类型 设置 品质颜色
	public static string SetStrQualityColor(int iQualityType)
	{
		if (iQualityType < 0|| iQualityType > 6)
			return StrQualityColor[0];
		return StrQualityColor[iQualityType];
	}

    //通过法器碎片id获取法器碎片名字的颜色
    public static Color GetMagicFragColor(int magicFragId)
    {
        DataRecord fragConfig = DataCenter.mFragmentAdminConfig.GetRecord(magicFragId);
        if (fragConfig == null)
        {
            return Color.white;
        }
        int magicId = (int)fragConfig.getData("ROLEEQUIPID");
        return GetNameColor(magicId);
    }
	public static Color GetNameEffectColor()
	{
		return new Color (0.258f, 0.189f, 0.137f);
	}

	public static int GetItemQuality(int tid)
	{
		int iQualityValue = 0;
		ITEM_TYPE tmpItemType = PackageManager.GetItemRealTypeByTableID(tid);

		switch (tmpItemType)
		{
		case ITEM_TYPE.CHARACTER:
		case ITEM_TYPE.PET: iQualityValue = (int)GetPetField(tid, "STAR_LEVEL"); break;
        //by chenliang
        //begin

//		case ITEM_TYPE.PET_FRAGMENT: iQualityValue = 0 /*(int)GetPetFragmentField(tid, "")*/; break;
//------------------
        case ITEM_TYPE.PET_FRAGMENT:
            {
                int tmpTid = GetItemNumberField(tid, "ITEM_ID");
                if (tmpTid != 0)
                    iQualityValue = GetItemNumberField(tmpTid, "STAR_LEVEL");
            } break;

        //end
		case ITEM_TYPE.EQUIP:
		case ITEM_TYPE.MAGIC: iQualityValue = (int)GetEquipField(tid, "QUALITY"); break;
        //by chenliang
        //begin

// 		case ITEM_TYPE.EQUIP_FRAGMENT: iQualityValue = 0 /*(int)GetEquipFragmentField(tid, "")*/; break;
//---------------------
        case ITEM_TYPE.EQUIP_FRAGMENT:
            {
                int tmpTid = GetItemNumberField(tid, "ITEM_ID");
                if (tmpTid != 0)
                    iQualityValue = GetItemNumberField(tmpTid, "QUALITY");
            } break;

        //end
        case ITEM_TYPE.MAGIC_FRAGMENT: iQualityValue = 0 /*(int)GetMagicFragmentField(tid, "")*/; break;

		case ITEM_TYPE.CONSUME_ITEM:
		case ITEM_TYPE.SAODANG_POINT:
		case ITEM_TYPE.RESET_POINT:
		case ITEM_TYPE.LOCK_POINT:
		case ITEM_TYPE.HONOR_POINT:
		case ITEM_TYPE.GEM:
		case ITEM_TYPE.MATERIAL:
		case ITEM_TYPE.MATERIAL_FRAGMENT:  iQualityValue = 0; break;
		}
		return iQualityValue;
	}

    public static Color GetDefineColor(CommonColorType kColorType)
    {
        switch (kColorType) 
        {
            case CommonColorType.WHITE:
                return Color.white;
            case CommonColorType.DES_RED:
                return new Color(1f,0.2f,0.2f);
            case CommonColorType.GREEN:
                return new Color(0.5f,1f,0.4f);
            case CommonColorType.HINT_RED:
                return new Color(1f,0f,0f);
        }
        return Color.white;
    }

	//拥有数量显示内容
	public static string ShowNumUI(long iOwnNum)
	{
		if (iOwnNum < 0)
		{
			iOwnNum = 0;
		}

		if ( iOwnNum / 1000000 <= 0)
		{
			return iOwnNum.ToString ();
		}
		else if(iOwnNum / 1000000 > 0)
		{
			iOwnNum = iOwnNum / 10000 ;
			
			return iOwnNum + "万";
		}else if(iOwnNum / 100000000 > 0)
		{
			iOwnNum = iOwnNum / 100000000 ;
			
			return iOwnNum + "亿";
		}
		return iOwnNum.ToString();
	}

    /// <summary>
    /// 是否是机器人
    /// </summary>
    /// <param name="uid">uid</param>
    /// <returns></returns>
    public static bool IsRobot(string uid)
    {
        return uid != string.Empty && uid[0] == '-';
    }
    public static int GetMainStageGold(int kRoleLevel)
    {
        //主线金币公式：每次副本经验产出=(基数A+等级*系数A)*体力消耗
        DataRecord _record_gold = DataCenter.mPowerEnergy.GetRecord(2);
        int gold = (int)(_record_gold["BASE"] + kRoleLevel * _record_gold["QUOTIETY"]) * StageProperty.GetCurrentStageStaminaCost();
        return gold;
    }

    public static int GetMainStageExp(int kRoleLevel)
    {
        DataRecord _record_exp = DataCenter.mPowerEnergy.GetRecord(1);
        int roleExp = (int)(_record_exp["BASE"] + kRoleLevel * _record_exp["QUOTIETY"]) * StageProperty.GetCurrentStageStaminaCost();
        return roleExp;
    }

    /// <summary>
    /// 获得符灵能够提供的经验值
    /// </summary>
    /// <param name="kPetData"></param>
    /// <returns></returns>
    public static int GetPetExp(PetData kPetData) 
    {
        int iExp = 0;
        DataRecord dataRecord = DataCenter.mPetLevelExpTable.GetRecord(kPetData.level);
        if (dataRecord != null)
        {
            iExp += TableCommon.GetNumberFromActiveCongfig(kPetData.tid, "DROP_EXP") + dataRecord["TOTAL_EXP_" + kPetData.starLevel];
        }
        else 
        {
            DEBUG.LogError("get data from mPetLevelExpTable failed: kPetdata's level is:" + kPetData.level);
        }
        return iExp;
    }
    #region ETC1优化
    public static string[] mTexRootPaths = new string[] 
    {
        "EffectRes/Maps/",
        "textures/",     
        "textures/UItextures/Background/",
        "textures/UItextures/",
        "textures/UItextures/Loading/",
        "textures/WorldMap/",
        "EffectRes/Maps/bossziti/",
        "SceneModel/SceneModel/tree/texture/",
        "SceneModel/building/texture/",
        "SceneModel/building/texture/texture/",
        "SceneModel/ground/texture/",

    };

    public static Texture GetTexutreByName(string kTexName)
    {
        Texture _tex = null;
        for (int i = 0, count = GameCommon.mTexRootPaths.Length; i < count; i++)
        {
            _tex = Resources.Load<Texture>(GameCommon.mTexRootPaths[i] + kTexName);
            if (_tex != null)
                return _tex;
        }
        DEBUG.Log(kTexName+"不在图片搜索列表中,需要添加相应的搜索路径");
        return _tex;
    }

    public static void SetMat_ETC1(Material kChangedMat)
    {
        if (kChangedMat == null)
            return;
        if (kChangedMat.mainTexture == null)
            return;

        kChangedMat.SetTexture("_MainTex_Alpha", GetTexutreByName(kChangedMat.mainTexture.name + "_Alpha"));
    }
    #endregion
    #region 窗口动态UI
    //added by xuke
    public static int mCurOpenTweenScaleIndex = 0;            //> 当前使用的Tween组件下标
    public static int mCurCloseTweenScaleIndex = 0;           //> 当前使用的Tween组件下标
    public const int MAX_TWEEN_SCALE_COUNT = 5;               //> 最多TweenScale组件数量
    public static List<UITweener> mOpenTweenScaleList = new List<UITweener>();
    public static List<UITweener> mCloseTweenScaleList = new List<UITweener>();
    public static T CopyComponent<T>(T original,GameObject destination) where T : Component 
    {
        System.Type _type = original.GetType();
        Component copy = destination.AddComponent(_type);
        System.Reflection.FieldInfo[] fields = _type.GetFields();
        foreach (System.Reflection.FieldInfo field in fields) 
        {
            field.SetValue(copy,field.GetValue(original));
        }
        return copy as T;
    }
    public static void InitTweenPrefab(UITweener kOpenTween,UITweener kCloseTween,GameObject kObj) 
    {
		mOpenTweenScaleList.Clear();
        mCloseTweenScaleList.Clear();
        for (int i = 0; i < MAX_TWEEN_SCALE_COUNT; i++) 
        {
            mOpenTweenScaleList.Add(CopyComponent<UITweener>(kOpenTween, kObj));
            mCloseTweenScaleList.Add(CopyComponent<UITweener>(kCloseTween,kObj));
        }
    }
    public static void PlayOpenTweenScale(Transform kTrans) 
    {
        if (!CommonParam.mOpenDynamicUI)
            return;
        TweenScale _tween = (TweenScale)mOpenTweenScaleList[(mCurOpenTweenScaleIndex++) % MAX_TWEEN_SCALE_COUNT];
        _tween.cachedTransform = kTrans;
        _tween.ResetToBeginning();
        _tween.PlayForward();
    }
    //by chenliang
    //begin

    private static bool mIsPlayCloseTween = true;
    public static bool IsPlayCloseTween
    {
        set { mIsPlayCloseTween = value; }
        get { return mIsPlayCloseTween; }
    }

    //end
    public static void PlayCloseTweenScale(Transform kTrans,Action kAction) 
    {
        if (!CommonParam.mOpenDynamicUI)
            return;
        TweenScale _tween = (TweenScale)mCloseTweenScaleList[(mCurCloseTweenScaleIndex) % MAX_TWEEN_SCALE_COUNT];
        _tween.onFinished.Clear();
        _tween.onFinished.Add(new EventDelegate(() => { kAction(); _tween.ResetToBeginning(); })
        {
            oneShot = true
        });
        _tween.cachedTransform = kTrans;
        _tween.ResetToBeginning();
        _tween.PlayForward();

        mCurCloseTweenScaleIndex++;
    }
    //end
    #endregion
    /// <summary>
    /// 判断商店购买物品的货币是否足够
    /// </summary>
    /// <param name="costTypeArr"></param>
    /// <param name="costNumArr"></param>
    /// <returns></returns>
    public static bool IsCoinEnough(int[] costTypeArr, int[] costNumArr)
    {
        if (costTypeArr.Length != costNumArr.Length)
        {
            DEBUG.LogError("costTypeArr 和 costNumArr 长度不匹配");
            return false;
        }
        bool _isEough = true;
        for (int i = 0, count = costTypeArr.Length; i < count; i++)
        {
            if (costTypeArr[i] == 0)
            {
                _isEough = true && _isEough;
            }
            else
            {
                _isEough = PackageManager.GetItemLeftCount(costTypeArr[i]) >= costNumArr[i] && _isEough;
            }
            // 如果有一种货币不够则直接返回失败
            if (!_isEough)
                return false;
        }
        return _isEough;
    }

    
    /// <summary>
    /// 设置红点的显示隐藏
    /// </summary>
    /// <param name="kObj"></param>
    /// <param name="kVisible"></param>
    /// <param name="kNewMarkName"></param>
    public static void SetNewMarkVisible(GameObject kObj, bool kVisible, string kNewMarkName = "NewMark")
    {
        GameObject _newMark = GameCommon.FindObject(kObj, kNewMarkName);
        if (_newMark != null)
            _newMark.SetActive(kVisible);
    }

    //public static bool CheckNeedShowAureole(int kActiveObjectIndex) 
    //{
    //    if (PET_QUALITY.ORANGE == (PET_QUALITY)GameCommon.GetItemQuality(kActiveObjectIndex))
    //    {
    //        return true;
    //    }
    //    return false;
    //}

	static public void SetItemDetailsWindow(int iTid)
	{
		if(iTid/1000 == 2000 || iTid/1000 == 1000)
		{
			//其它物品
			DataCenter.OpenWindow("CONSUMBLES_DETAILS_WINDOW", iTid);
		}else if(iTid/1000 == 30)
		{
			//宠物
			DataCenter.OpenWindow(UIWindowString.petDetail, iTid);			
		}else if(iTid/1000 == 40)
		{
			//宠物碎片
//			iTid = TableCommon.GetNumberFromFragment (iTid, "ITEM_ID");
            iTid = GetItemNumberField(iTid, "ITEM_ID");
            if (iTid != 0)
            {
                DataCenter.OpenWindow(UIWindowString.petDetail, iTid);
                DataCenter.SetData(UIWindowString.petDetail, "RELATE_TO_GET_PATH", iTid);
            }
		}else if(iTid/1000 == 14 || iTid/1000 == 15)
		{
			// 装备、 法器
			DataCenter.OpenWindow("GRABTREASURE_MAGIC_EQUIP_DETAIL_WINDOW", iTid);
		}else if(iTid/1000 == 16 || iTid/1000 == 17)
		{
			// 装备碎片、 法器碎片
			if(iTid/1000 == 16)
			{
				iTid = TableCommon.GetNumberFromFragment (iTid, "ITEM_ID");
			}else
			{
				iTid= TableCommon.GetNumberFromFragmentAdmin(iTid, "ROLEEQUIPID");
			}

			DataCenter.OpenWindow("GRABTREASURE_MAGIC_EQUIP_DETAIL_WINDOW", iTid);
			DataCenter.SetData("GRABTREASURE_MAGIC_EQUIP_DETAIL_WINDOW","RELATE_TO_GET_PATH", iTid);
		}
	}
	static public void SetAccountItemDetailsWindow(int iTid)
	{
		DataCenter.OpenWindow("CONSUMBLES_DETAILS_WINDOW", iTid);
	}
    static public void BindItemDescriptionEvent(GameObject kTargetObj,int kTid)
    {
        AddButtonAction(kTargetObj, () => { SetAccountItemDetailsWindow(kTid); });
    }

	static public int GetMinMosterLevel(string strStrengMasterName, List<EquipData> _equipList, int _iLevel)
	{
//		int _iLevel = GetMinStrengthLevel(_equipList);
		int _curMasLV = 0;
		
		foreach (KeyValuePair<int, DataRecord> v in DataCenter.mStrengMaster.GetAllRecord ())
		{
			if((int)v.Value.getObject(strStrengMasterName) > _iLevel)
			{
				_curMasLV = (int)v.Value.getObject ("INDEX") - 1;
				break;
			}
		}
		return _curMasLV;
	}
	
	/// 被解析的字符串格式为 XXXX#YY#ZZ|XXXX#YY#ZZ|XXXX#YY#ZZ
	/// 如1#1#100|1#1#100 表示玩家主角等级，对应条件数值1=玩家主角等级下限，条件数值2=玩家主角等级上限
	public static List<TipsInfoBase> ParseTipsList(string text)
	{
		List<TipsInfoBase> result = new List<TipsInfoBase>();
		string[] strs = text.Split('|');
		
		foreach (string s in strs)
		{      
			int n = s.IndexOf('#');
			string[] sStrs = s.Split('#');
			if (n < 1)
			{
				int type = Convert.ToInt32(sStrs[0]);
				result.Add(new TipsInfoBase() {iType = type, iMinNum = 0, iMaxNum = 9999 });
			}else if(n < 2)
			{
				int type = Convert.ToInt32(sStrs[0]);
				int minNum = Convert.ToInt32(sStrs[1]);
				result.Add(new TipsInfoBase() { iType = type, iMinNum = minNum,iMaxNum = 9999 });
			}else
			{
				int type = Convert.ToInt32(sStrs[0]);
				int minNum = Convert.ToInt32(sStrs[1]);
				int maxNum = Convert.ToInt32(sStrs[3]);
				result.Add(new TipsInfoBase() { iType = type, iMinNum = minNum,iMaxNum = maxNum });
			}
		}		
		return result;
	}	
	public static TipsInfoBase ParseTips(string text) 
	{
		int n=text.IndexOf('#');
		string[] sStrs = text.Split('#');

		if (n < 1)
		{
			int type = Convert.ToInt32(sStrs[0]);
			return new TipsInfoBase() {iType = type, iMinNum = 0, iMaxNum = 9999 };
		}else if(n < 2)
		{
			int type = Convert.ToInt32(sStrs[0]);
			int minNum = Convert.ToInt32(sStrs[1]);
			return new TipsInfoBase() { iType = type, iMinNum = minNum,iMaxNum = 9999 };
		}else 
		{
			int type = Convert.ToInt32(sStrs[0]);
			int minNum = Convert.ToInt32(sStrs[1]);
			int maxNum = Convert.ToInt32(sStrs[3]);
			return new TipsInfoBase() { iType = type, iMinNum = minNum,iMaxNum = maxNum };
		}	
	}
	//根据泡泡的条件判断泡泡是否满足
	public static bool SetIsCountion(List<TipsInfoBase> _list)
	{
		bool bIsCan = false;
		int iCount = 0 ;
		for(int i = 0; i < _list.Count; i++)
		{
			if(_list[i].iType == 1)
			{
				if(RoleLogicData.Self.character.level >= _list[i].iMinNum && RoleLogicData.Self.character.level<= _list[i].iMaxNum)
				{
					iCount++;
				}else 
					break;
			}else if(_list[i].iType == 2)
			{
				if(RoleLogicData.Self.vipLevel >= _list[i].iMinNum && RoleLogicData.Self.vipLevel <= _list[i].iMaxNum)
					iCount++;
				else break;
			}
		}
		if(iCount >= _list.Count)
			bIsCan = true;
		else 
			bIsCan = false;
		
		return bIsCan;
	}

    /// <summary>
    /// 计算升级总增加值，每升1级的增加值循环变化
    /// </summary>
    /// <param name="levelUpArray"> 升级增加值数组， 如数组为[1, 2, 3]，则每次升级的增加值依次为1, 2, 3, 1, 2, 3 ... </param>
    /// <param name="deltaLevel"> 总增加级别 </param>
    /// <returns> 总增加数值 </returns>
    public static int LevelUpValue(int[] levelUpArray, int deltaLevel)
    {
        int arrayLen = levelUpArray.Length;

        if (arrayLen == 0)
            return 0;
        else if (arrayLen == 1)
            return deltaLevel * levelUpArray[0];

        int loop = deltaLevel / arrayLen;
        int step = deltaLevel % arrayLen;
        int deltaValue = 0;

        if (loop > 0)
        {
            int onceLoopSum = 0;

            for (int i = 0; i < arrayLen; ++i)
            {
                onceLoopSum += levelUpArray[i];
            }

            deltaValue = loop * onceLoopSum;
        }

        while (step > 0)
        {
            deltaValue += levelUpArray[step - 1];
            --step;
        }

        return deltaValue;
    }

    /// <summary>
    /// 计算升级总增加值，每升1级的增加值循环变化
    /// </summary>
    /// <param name="levelUpArray"> 升级增加值数组， 如数组为[0.1, 0.2, 0.3]，则每次升级的增加值依次为0.1, 0.2, 0.3, 0.1, 0.2, 0.3 ... </param>
    /// <param name="deltaLevel"> 总增加级别 </param>
    /// <returns> 总增加数值 </returns>
    public static float LevelUpValue(float[] levelUpArray, int deltaLevel)
    {
        int arrayLen = levelUpArray.Length;

        if (arrayLen == 0)
            return 0;
        else if (arrayLen == 1)
            return deltaLevel * levelUpArray[0];

        int loop = deltaLevel / arrayLen;
        int step = deltaLevel % arrayLen;
        float deltaValue = 0f;

        if (loop > 0)
        {
            float onceLoopSum = 0f;

            for (int i = 0; i < arrayLen; ++i)
            {
                onceLoopSum += levelUpArray[i];
            }

            deltaValue = loop * onceLoopSum;
        }

        while (step > 0)
        {
            deltaValue += levelUpArray[step - 1];
            --step;
        }

        return deltaValue;
    }

    /// <summary>
    /// 检测指定关卡能否进入
    /// </summary>
    /// <param name="kEvetn"></param>
    /// <param name="kStageIndex"></param>
    /// <returns></returns>
    public static bool CheckCanEnterBattle(CEvent kEvetn)
    {
        //判断掉落物品的背包是否足够
        int _stageIndex = (int)kEvetn.getObject("STAGE_INDEX");
        List<ItemDataBase> tmpItems = GameCommon.ParseItemList(TableCommon.GetStringFromStageConfig(_stageIndex, "LOOT_PREVEIW"));
        List<PACKAGE_TYPE> tmpTypes = PackageManager.GetPackageTypes(tmpItems);
        if (CheckPackage.Instance.CanEnterBattle(tmpTypes))
            return true;
        return false;
    }

    public static void ChangeRenderQueue(Transform kTrans, int kRenerQueue)
    {
        if (kTrans == null)
            return;
        foreach (Transform trans in kTrans.transform)
        {
            if (trans != null && trans.renderer != null && trans.renderer.material != null)
            {
                trans.renderer.material.renderQueue = kRenerQueue;
            }
            ChangeRenderQueue(trans, kRenerQueue);
        }
    }

    public static string GetAureoleEffectNameByQuality(int kQuality,AUREOLE_TYPE kAureoleType) 
    {
        string _effectName = string.Empty;
        DataRecord _qualityConfig = DataCenter.mQualityConfig.GetRecord(kQuality);
        if (_qualityConfig == null)
            return _effectName;
        string _aureoleTypeName = string.Empty;
        switch (kAureoleType) 
        {
            case AUREOLE_TYPE.UI:
                _aureoleTypeName = "UI_AUREOLE";
                break;
            case AUREOLE_TYPE.MAIN_UI:
                _aureoleTypeName = "MAIN_UI_AUREOLE";
                break;
            case AUREOLE_TYPE.FIGHT_UI:
                _aureoleTypeName = "FIGHT_UI_AUREOLE";
                break;
        }
        _effectName = _qualityConfig[_aureoleTypeName] + "(Clone)";
        return _effectName;
    }

    public static IEnumerator IE_ChangeRenderQueue(int kPetTid,GameObject kObj,int kRenderQueue) 
    {
        if (kObj == null)
            yield break;
        int _quality = GameCommon.GetItemQuality(kPetTid);
        string _effectName = GetAureoleEffectNameByQuality(_quality,AUREOLE_TYPE.UI);

        GameObject _aureoleObj = GameCommon.FindObject(kObj,_effectName);
        if (_aureoleObj == null)
            yield break;

        yield return null;
        if (_aureoleObj == null)
        {
            _aureoleObj = GameCommon.FindObject(kObj, _effectName);
            if (_aureoleObj == null)
            {
                yield break;
            }
        }
        ChangeParticleRenderQueue _cpr = _aureoleObj.GetComponent<ChangeParticleRenderQueue>();
        if (_cpr != null)
        {
            _cpr.enabled = false;
        }
        if (_aureoleObj != null)
        {
            ChangeRenderQueue(_aureoleObj.transform, kRenderQueue);
        }
    }

    public static List<int> GetTidByItemData(List<ItemDataBase> kItemDataList) 
    {
        List<int> _tidList = new List<int>();
        if (kItemDataList == null)
            return _tidList;
        for (int i = 0, count = kItemDataList.Count; i < count; ++i) 
        {
            _tidList.Add(kItemDataList[i].tid);
        }
        return _tidList;
    }

    /// <summary>
    /// 如果目标所在Panel的初始化操作已经错过当前帧的刷新时序，则强制其Panel在初始化时立刻调用刷新的方法，以避免刷新延迟到下一帧的问题
    /// </summary>
    /// <param name="obj"> 待刷新的目标 </param>
    public static void ForceUpdateUI(GameObject obj)
    {
        UIPanel panel = obj.FindComponentUpwards<UIPanel>();

        if (panel != null)
            panel.forceUpdateOnStart = true;
    }
}



//by chenliang
//begin

/// <summary>
/// 物品类型，用来获取物品字段
/// </summary>
public enum GET_ITEM_FIELD_TYPE
{
    NAME,               //物品名称
    DESC,               //物品描述
    STAR_LEVEL,         //物品星级（宠物）、品质（装备）
    ATLAS_NAME,         //物品图集名称
    SPRITE_NAME,        //物品图集精灵名称
}

//end

public enum GET_PARTH_TYPE
{
    NONE = 0,          
    PET_LEVELUP = 1,        //符灵升级
    PET_BREAK = 2,          //符灵突破
    PET_FATE = 3,           //符灵天命
    PET_SKILL = 4,          //符灵技能
    EQUIP_STRENGTHEN = 5,   //装备强化
    EQUIP_REFINE = 6,       //装备精炼
    MAGIC_STRENGTHEN = 7,   //法器强化
    MAGIC_REFINE = 8,       //法器精炼
    PVP = 9,                //巅峰挑战
    GRABTREASURE = 10,      //夺宝
    FRIEND = 11,            //好友
	UNION_SHOP = 12,		//军团商店
	JI_SHI_ITEM = 13,	    //集市——道具页签
    RAMMBOCK_SHOP = 14,     //灵核商店
    JI_SHI_CHOUKA = 15,	    //集市——抽卡页签
    PVP_PRESTIGE_SHOP = 16, //声望商店
    HEI_SHI = 17,           //黑市
    TIANMO_SHOP = 18,       //天魔商店
	XUN_XIAN = 19,			//寻仙
	FENG_LING_TA = 20,		//封灵塔
    ADVENTURE_EASY = 21,    //轻松难度副本
    ADVENTURE_NORMAL = 22,  //普通难度副本
	FRIEND_GIVE = 23,       //好友赠送
    TIAN_MO = 24,			//天魔
    CHARGE = 25,            //充值
    DIAN_XING = 26,	        //点星
	UNION = 27,			    //公会
    DAILY_PVE = 28,         //日常副本
    RECOVER = 29,           //回收
    SAO_DANG = 30,          //扫荡
    TEAM = 31,              //队伍界面
    ASSISTANT = 32,         //助战
}

//added by xuke begin
/// <summary>
/// 委托字典
/// </summary>
public class GetPathHandlerDic
{
	private static Dictionary<GET_PARTH_TYPE,System.Action> m_HandlerDic = new Dictionary<GET_PARTH_TYPE,System.Action>();
	public static Dictionary<GET_PARTH_TYPE,System.Action> HandlerDic { private set{m_HandlerDic = value;} get { return m_HandlerDic; } }

    public static bool ExecuteDelegate(int kGotoIndex)
    {
        DataRecord _record = DataCenter.mGainFunctionConfig.GetRecord(kGotoIndex);
        if (_record == null)
            return false;
        GET_PARTH_TYPE _parthType = (GET_PARTH_TYPE)((int)_record["Type"]);
        return ExecuteDelegate(_parthType);
    }
    public static bool ExecuteDelegate(GET_PARTH_TYPE kPathType)
    {
        Action _action;
        if (HandlerDic.TryGetValue(kPathType, out _action)) 
        {
            _action();
            return true;
        }
        return false;
    }

	public static void Init()
	{
        m_HandlerDic.Clear ();
        //跳转到符灵升级
        m_HandlerDic.Add(GET_PARTH_TYPE.PET_LEVELUP, () => {
            DataCenter.SetData("TEAM_WINDOW", "OPEN", TEAM_PAGE_TYPE.PET_PACKAGE);
            MainUIScript.Self.HideMainBGUI();
        });
        //跳转到符灵突破
        m_HandlerDic.Add(GET_PARTH_TYPE.PET_BREAK, () => {
            DataCenter.SetData("TEAM_WINDOW", "OPEN", TEAM_PAGE_TYPE.PET_PACKAGE);
            MainUIScript.Self.HideMainBGUI();
        });
        //跳转到天命
        m_HandlerDic.Add(GET_PARTH_TYPE.PET_FATE,() => {
            DataCenter.SetData("TEAM_WINDOW", "OPEN", TEAM_PAGE_TYPE.PET_PACKAGE);
            MainUIScript.Self.HideMainBGUI();
        });
        //跳转到符灵技能
        m_HandlerDic.Add(GET_PARTH_TYPE.PET_SKILL, () => {
            DataCenter.SetData("TEAM_WINDOW", "OPEN", TEAM_PAGE_TYPE.PET_PACKAGE);
            MainUIScript.Self.HideMainBGUI();
        });
        //跳转到装备强化
        m_HandlerDic.Add(GET_PARTH_TYPE.EQUIP_STRENGTHEN, () => {
            DataCenter.OpenWindow("PACKAGE_EQUIP_WINDOW");
            MainUIScript.Self.HideMainBGUI();
        });
        //跳转到装备精炼
        m_HandlerDic.Add(GET_PARTH_TYPE.EQUIP_REFINE, () => {
            DataCenter.OpenWindow("PACKAGE_EQUIP_WINDOW");
            MainUIScript.Self.HideMainBGUI();
        });
        //跳转到法器强化
        m_HandlerDic.Add(GET_PARTH_TYPE.MAGIC_STRENGTHEN, () => {
            DataCenter.OpenWindow("PACKAGE_EQUIP_WINDOW",3);
            MainUIScript.Self.HideMainBGUI();
        });
        //跳转到法器精炼
        m_HandlerDic.Add(GET_PARTH_TYPE.MAGIC_REFINE, () => {
            DataCenter.OpenWindow("PACKAGE_EQUIP_WINDOW", 3);
            MainUIScript.Self.HideMainBGUI();
        });
        //跳转到竞技场
        m_HandlerDic.Add(GET_PARTH_TYPE.PVP, () => {
            DataCenter.OpenWindow("TRIAL_WINDOW");
            DataCenter.OpenWindow("TRIAL_WINDOW_BACK");
            //GlobalModule.DoCoroutine(Button_peak_pvp_enter_PVPBtn.DoButton());
            MainUIScript.Self.OpenMainUI();
            DataCenter.OpenWindow(UIWindowString.arena_main_window);
            DataCenter.Set("FUNC_ENTER_INDEX", FUNC_ENTER_INDEX.PVP);
        });
		//跳转到夺宝
        m_HandlerDic.Add(GET_PARTH_TYPE.GRABTREASURE, () => {
            DataCenter.OpenWindow("TRIAL_WINDOW");
            DataCenter.OpenWindow("TRIAL_WINDOW_BACK");
            DataCenter.OpenWindow("GRABTREASURE_WINDOW");
        });
        //跳转到好友
        m_HandlerDic.Add(GET_PARTH_TYPE.FRIEND, () => {
            MainUIScript.Self.OpenMainWindowByIndex(MAIN_WINDOW_INDEX.FriendWindow);
        });
        //跳转到军团商店
        m_HandlerDic.Add(GET_PARTH_TYPE.UNION_SHOP, () =>
        {
            HttpModule.CallBack requestSuccess = text =>
            {
                var item = JCode.Decode<SC_GetGuildId>(text);
                UnionBase.SetGuildBossRedPoint(item.guildBossRedPoint);//秘境红点
                RoleLogicData.Self.guildId = item.guildId;
                DEBUG.Log(RoleLogicData.Self.guildId);
                if (RoleLogicData.Self.guildId != "")
                {
                    DataCenter.OpenWindow(UIWindowString.union_main);
                    DataCenter.Set("UNION_SHOP_OPEN_FLAG", 1); 
                    DataCenter.CloseWindow(UIWindowString.union_main);
                }
                else DataCenter.OpenWindow(UIWindowString.union_list);
                MainUIScript.Self.HideMainBGUI();
            };
            CS_GetGuildId cs = new CS_GetGuildId();
            HttpModule.Instace.SendGameServerMessageT<CS_GetGuildId>(cs, requestSuccess, NetManager.RequestFail);
        });
		//跳转到集市抽卡界面
        m_HandlerDic.Add(GET_PARTH_TYPE.JI_SHI_CHOUKA, () =>
        {
            //GlobalModule.ClearAllWindow();          
            //DataCenter.OpenWindow("SHOP_WINDOW");
            GlobalModule.DoOnNextUpdate(5, () => 
            {
                EventCenter.Start("Button_ShopBtn").DoEvent();            
            });
		});
        //跳转到黑市
        m_HandlerDic.Add(GET_PARTH_TYPE.HEI_SHI, () => 
        {
            DataCenter.OpenWindow("NEW_MYSTERIOUS_SHOP_WINDOW");
            DataCenter.OpenBackWindow("NEW_MYSTERIOUS_SHOP_WINDOW", "a_ui_heishilogo", () => MainUIScript.Self.ShowMainBGUI(), 184);
            MainUIScript.Self.HideMainBGUI();
        });
        //跳转到灵核商店
        m_HandlerDic.Add(GET_PARTH_TYPE.RAMMBOCK_SHOP, () =>
        {
            DataCenter.OpenWindow("TRIAL_WINDOW");
			DataCenter.OpenWindow("TRIAL_WINDOW_BACK");
			EventCenter.Start ("Button_rammbock_enter_PVPBtn").DoEvent ();
            DataCenter.OpenWindow("SHOP_RENOWN_WINDOW");
        });
		//跳转到集市道具界面
        m_HandlerDic.Add(GET_PARTH_TYPE.JI_SHI_ITEM, () =>
        {
            GlobalModule.DoOnNextUpdate(5, () => 
            {
                MainUIScript.Self.HideMainBGUI();
                GlobalModule.ClearAllWindow();
                DataCenter.OpenWindow("SHOP_WINDOW", 2);
            });       
		});
        //跳转到声望商店
        m_HandlerDic.Add(GET_PARTH_TYPE.PVP_PRESTIGE_SHOP, () =>
        {
            DataCenter.OpenWindow("TRIAL_WINDOW");
            DataCenter.OpenWindow("TRIAL_WINDOW_BACK");
            MainUIScript.Self.OpenMainUI();
            DataCenter.OpenWindow(UIWindowString.arena_main_window);
            DataCenter.Set("FUNC_ENTER_INDEX", FUNC_ENTER_INDEX.PVP);
            DataCenter.OpenWindow("PVP_PRESTIGE_SHOP_WINDOW",1);
        });
        //跳转到天魔商店
        m_HandlerDic.Add(GET_PARTH_TYPE.TIANMO_SHOP, () =>
        {
            DataCenter.OpenWindow("TRIAL_WINDOW");
            DataCenter.OpenWindow("TRIAL_WINDOW_BACK");
            EventCenter.Start("Button_map_boss_list").DoEvent();
            DataCenter.OpenWindow("SHOP_FEATS_WINDOW_BACK");
            EventCenter.Start("Button_feats_btn").DoEvent();
        });
        //跳转到寻仙
        m_HandlerDic.Add(GET_PARTH_TYPE.XUN_XIAN, () => 
        {
            DataCenter.OpenWindow("TRIAL_WINDOW");
            DataCenter.OpenWindow("TRIAL_WINDOW_BACK");
            EventCenter.Start("Button_fairyland_enter_PVEBtn").DoEvent();
        });
        //跳转到历练、封灵塔
		m_HandlerDic.Add (GET_PARTH_TYPE.FENG_LING_TA,()=>{
            DataCenter.OpenWindow("TRIAL_WINDOW");
            DataCenter.OpenWindow("TRIAL_WINDOW_BACK");
			EventCenter.Start ("Button_rammbock_enter_PVPBtn").DoEvent ();});
        //跳转到轻松难度副本
        m_HandlerDic.Add(GET_PARTH_TYPE.ADVENTURE_EASY, () => {
            //ScrollWorldMapWindow
            GlobalModule.DoOnNextUpdate(2, () => 
            {
                MainUIScript.Self.OpenMainWindowByIndex(MAIN_WINDOW_INDEX.WorldMapWindow);
                GlobalModule.DoLater(() =>
                {
                    DataCenter.SetData("SCROLL_WORLD_MAP_WINDOW", "SELECT_DIFFICULTY", 1);
                    DataCenter.SetData("SCROLL_WORLD_MAP_BOTTOM_LEFT", "INIT_POPUP_LIST", 1);
                }, 0f);
                //DataCenter.SetData("SCROLL_WORLD_MAP_WINDOW", "SELECT_POINT", pointID);
                DataCenter.CloseWindow(UIWindowString.total_task);
            });          
        });
        //跳转到普通难度副本
        m_HandlerDic.Add(GET_PARTH_TYPE.ADVENTURE_NORMAL, () =>
        {
            //ScrollWorldMapWindow
            GlobalModule.DoOnNextUpdate(2, () =>
            {
               MainUIScript.Self.OpenMainWindowByIndex(MAIN_WINDOW_INDEX.WorldMapWindow);
               GlobalModule.DoLater(() =>
               {
                   DataCenter.SetData("SCROLL_WORLD_MAP_WINDOW", "SELECT_DIFFICULTY", 2);
                   DataCenter.SetData("SCROLL_WORLD_MAP_BOTTOM_LEFT", "INIT_POPUP_LIST", 2);
               }, 0f);
               DataCenter.CloseWindow(UIWindowString.total_task);
            });
        });
        //跳转到赠送好友体力
        m_HandlerDic.Add(GET_PARTH_TYPE.FRIEND_GIVE, () =>
        {
            MainUIScript.Self.OpenMainWindowByIndex(MAIN_WINDOW_INDEX.FriendWindow);
        });
        //跳转到历练、天魔降临
        m_HandlerDic.Add(GET_PARTH_TYPE.TIAN_MO, () =>
        {
            DataCenter.OpenWindow("TRIAL_WINDOW");
            DataCenter.OpenWindow("TRIAL_WINDOW_BACK");
            EventCenter.Start("Button_map_boss_list").DoEvent();
        });
        //跳转到充值界面
        m_HandlerDic.Add(GET_PARTH_TYPE.CHARGE, () =>
        {
            GameCommon.OpenRecharge(RECHARGE_PAGE.RECHARGE, CommonParam.rechageDepth);
        });
        //跳转到点星
        m_HandlerDic.Add(GET_PARTH_TYPE.DIAN_XING, () => 
        {
            DataCenter.OpenWindow("ASTROLOGY_UI_WINDOW");
            MainUIScript.Self.HideMainBGUI();
        });
        //跳转到宗门
        m_HandlerDic.Add(GET_PARTH_TYPE.UNION, () => 
        {
            DataCenter.CloseWindow("BOSS_APPEAR_WINDOW");
            HttpModule.CallBack requestSuccess = text =>
            {
                var item = JCode.Decode<SC_GetGuildId>(text);
                UnionBase.SetGuildBossRedPoint(item.guildBossRedPoint);  //秘境红点
                RoleLogicData.Self.guildId = item.guildId;
                DEBUG.Log(RoleLogicData.Self.guildId);
                if (RoleLogicData.Self.guildId != "")
                {
                    DataCenter.OpenWindow(UIWindowString.union_main);
                }
                else DataCenter.OpenWindow(UIWindowString.union_list);
                MainUIScript.Self.HideMainBGUI();
            };
            CS_GetGuildId cs = new CS_GetGuildId();
            HttpModule.Instace.SendGameServerMessageT<CS_GetGuildId>(cs, requestSuccess, NetManager.RequestFail);
        });
        //跳转到日常副本
        m_HandlerDic.Add(GET_PARTH_TYPE.DAILY_PVE, () => 
        {
           EventCenter.Start("Button_PVEBtn").DoEvent();
            GlobalModule.DoOnNextUpdate(() =>
            {
                DataCenter.CloseWindow(UIWindowString.total_task);
                EventCenter.Start("Button_daily_pve_btn").DoEvent();
                //DataCenter.CloseWindow("SCROLL_WORLD_MAP_WINDOW");
                //DataCenter.OpenWindow(UIWindowString.daily_stage_main_window);
            });
            
        });
        //跳转到回收界面
        m_HandlerDic.Add(GET_PARTH_TYPE.RECOVER, () => 
        {
            EventCenter.Start("Button_RecoverBtn").DoEvent();
        });
        //跳转到扫荡（跳转到冒险界面）
        m_HandlerDic.Add(GET_PARTH_TYPE.SAO_DANG, () => 
        {
            Action _action;
            if (m_HandlerDic.TryGetValue(GET_PARTH_TYPE.ADVENTURE_EASY, out _action))
            {
                _action();
            }           
        });
        //跳转到队伍界面
        m_HandlerDic.Add(GET_PARTH_TYPE.TEAM, () => 
        {
            DataCenter.SetData("TEAM_WINDOW", "OPEN", TEAM_PAGE_TYPE.TEAM);
            MainUIScript.Self.HideMainBGUI();
        });
        //助战界面
        m_HandlerDic.Add(GET_PARTH_TYPE.ASSISTANT, () => 
        {
            DataCenter.SetData("TEAM_WINDOW", "OPEN", TEAM_PAGE_TYPE.TEAM);
            GlobalModule.DoCoroutine(IE_GoToFunc(() => { DataCenter.SetData("TEAM_INFO_WINDOW", "RELATE_INFO", null); }, 6));
        });
	}

    private static IEnumerator IE_GoToFunc(Action kAction,int kWaitFrameCount = 0)
    {
        if (kAction == null)
            yield break;
        for (int i = 0; i < kWaitFrameCount; ++i) 
        {
            yield return null;
        }
        kAction();
    }
}

//--------------------------------------------------------------------------------------------------

public enum CommonColorType
{
    WHITE = 0,  //> 白色
    DES_RED,    //> 描述类红色
    HINT_RED,   //> 提示类红色
    GREEN,      //> 绿色
}
public static class ExtensionString 
{
    public static void SetLabelColor(this UILabel kLbl, CommonColorType kColorType)
    {
        if (kLbl == null)
            return;
        kLbl.color = GameCommon.GetDefineColor(kColorType);
    }

    public static void SetProgressColor(this UILabel kLbl,CommonColorType kColorType)
    {
        string _colorPrefix = GetColorPrefixByColor(GameCommon.GetDefineColor(kColorType));
        string[] strs = kLbl.text.Split('/');
        kLbl.text = _colorPrefix + strs[0] + "[-]/" + strs[1];
    }

    public static string GetColorPrefixByColor(Color kColor)
    {
        string _r = string.Format("{0:X2}", Convert.ToInt32(kColor.r * 255)); //Convert.ToString(Convert.ToInt32(kColor.r * 255), 16);
        string _g = string.Format("{0:X2}", Convert.ToInt32(kColor.g * 255)); //Convert.ToString(Convert.ToInt32(kColor.g * 255), 16);//(kLbl.color.g * 255).ToString("x");
        string _b = string.Format("{0:X2}", Convert.ToInt32(kColor.b * 255)); //Convert.ToString(Convert.ToInt32(kColor.b * 255), 16);//(kLbl.color.g * 255).ToString("x");
        return "[" + _r + _g + _b + "]";
    }
    public static void SetNumLabel(this UILabel kLbl,int kNum)
    {
        kLbl.text = "x" + kNum;
    }

}
