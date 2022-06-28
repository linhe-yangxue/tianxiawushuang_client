//console whether building version
//#define DEBUG_VERSION
//#define DEBUG_NET
using UnityEngine;
using System.Collections;
using System.IO;
using System;
using Logic;
using RManager.AssetLoader;
using Asset;
using Common;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

//by chenliang
//begin

//public class GlobalModule : MonoBehaviour {
//----------
//让GlobalModule兼容ICoroutineOperation操作
public class GlobalModule : MonoBehaviour, ICoroutineOperation
{

//end

    public static volatile GlobalModule mInstance = null;
    private AsyncOperation mAsyncOperation = null; // use for asynchronous scene load
	private GameObject	mSceneRoot = null;
    static public bool sbInitGameData = false;
	public static int mClickEventCurFrame = 0;
    public static bool sbIsNeedJudge = true;
    private readonly Color[] mWindowDisableColor = new Color[]
    {
        Color.grey, Color.grey, Color.grey
    };
    public Color[] mWindowColor = new Color[3];
    public Color[] mWindowGuideColor;

    static float heartbeatDelta = 0.0f;
	static CS_Heartbeat heartbeat = null;



    //by chenliang
    //begin

//    public static bool isHotLoading = false;
//-------------
    //改在记录在HotUpdateLoading.IsLoadResFromServer中

    //end
    public static event Action onUpdate;
    public static event Action onLateUpdate;

	// late modify by lgl
	public static bool usePeerNetwork = false;
    //by chenliang
    //begin

    private CountdownTimer mCountdownTimer;         //全局倒计时

    //热更新获取的服务器列表
    private static List<VersionConfigGroupItemServer> mServerList = new List<VersionConfigGroupItemServer>();
    public static List<VersionConfigGroupItemServer> ServerList
    {
        get { return mServerList; }
    }

    private static bool mNeedCheckNetConnectable = false;       //是否需要检查网络连接状态
    public static bool NeedCheckNetConnectable
    {
        set { mNeedCheckNetConnectable = value; }
        get { return mNeedCheckNetConnectable; }
    }

    private static AsyncOperation mLoadSceneAsyncOP;
    private static Action<float> mLoadSceneProgress;
    private static bool mIsSceneLoadComplete = false;

    /// <summary>
    /// 异步加载场景
    /// </summary>
    public static AsyncOperation LoadSceneAsyncOP
    {
        set { mLoadSceneAsyncOP = value; }
        get { return mLoadSceneAsyncOP; }
    }
    /// <summary>
    /// 加载场景进度
    /// </summary>
    public static Action<float> LoadSceneProgress
    {
        set { mLoadSceneProgress = value; }
        get { return mLoadSceneProgress; }
    }
    /// <summary>
    /// 场景是否加载完成
    /// </summary>C#
    public static bool IsSceneLoadComplete
    {
        set { mIsSceneLoadComplete = value; }
        get { return mIsSceneLoadComplete; }
    }

    //end

    private bool isGamePause = false;// 游戏是否暂停
    private bool isGameFocus = false; // 游戏是否激活

    private bool isLog = false; // 是否显示崩溃log

    //by chenliang
    //begin

//    public static Stopwatch stopwatch;
//-------------
    public static Stopwatch stopwatch = new Stopwatch();
    public static Stopwatch heartbeatstopwatch = new Stopwatch();
    public static Stopwatch wwwstopwatch = new Stopwatch();
    //end
    //-------------------------------------------------------------------------
    static bool InitLoadConfigTable()
    {
        if (!CommonParam.UseUpdate || !TableManager.GetMe().LoadFromPack("t_configtable.pak", "Config/ConfigTable.bytes", false))
        {
            string info;

#if UNITY_STANDALONE||UNITY_EDITOR
            info = TableManager.Self.LoadConfig("Config/ConfigTable.csv", LOAD_MODE.UNICODE);
#else
			//info = TableManager.Self.LoadConfig("Config/ConfigTable.csv", LOAD_MODE.UNICODE);
        	info = TableManager.Self.LoadConfig("Config/ConfigTable.csv", LOAD_MODE.BYTES_RES);
#endif
        }
		UnityEngine.Debug.Log ("jiazaibiaoge11111=========");
		DEBUG.Log ("jiazaibiaoge11111=========");
        DataCenter.Self.LoadConfigData();

//         //TODO chenliang Http Crypto
//         string tmpStr = JCode.Encode(new string[] { "速度发生的发生的发生" });// "123456789";
//         TripleDESCrypto tmpTripleDES = new TripleDESCrypto();
//         DataTable.DataRecord tmpRecord = DataCenter.mGlobalConfig.GetRecord("UTILITY_TD_I");
//         string tmpStrKey = tmpRecord.getObject("VALUE").ToString();
//         tmpTripleDES.Key = tmpStrKey;
//         string tmpTripleEncode = System.Convert.ToBase64String(tmpTripleDES.Encode(tmpStr));
//         string tmpTripleDecode = tmpTripleDES.Decode(System.Convert.FromBase64String(tmpTripleEncode));
//         SHA1Crypto tmpSHA1 = new SHA1Crypto();
//         string tmpSHA1Encode = tmpSHA1.Encode("123456789");
//         string[] tmpStrDecode = JCode.Decode<string[]>(tmpStr);


        return true;
    }

    static public void RestartGame()
    {
        MainProcess.ClearBattle();
        ClearAllWindow();
        //Net.msServerIP = "";
        //Net.msServerPort = 0;

        // Instantiation of game data
        //GuideManager.FinishGuide();
        //OptionalGuide.ClearGuide();
        //PVEStageBattle.mbAutoBattle = false;
        PVEStageBattle.mBattleControl = BATTLE_CONTROL.MANUAL;
        GuildBossBattle.mBattleControl = BATTLE_CONTROL.MANUAL;
        MainUIScript.mCurIndex = MAIN_WINDOW_INDEX.RoleSelWindow;
        DataCenter.Set("CURRENT_STAGE", 0);
        Notification.Clear();
        Notification.RefuseAll();
//		DataCenter.Set("CURRENT_SELECT_FRIEND_ID", 0);
		if(FriendLogicData.mSelectPlayerData != null)
			FriendLogicData.mSelectPlayerData = null;
		if(PetLogicData.mFreindPetData != null)
			PetLogicData.mFreindPetData = null;

		RoleSelTopLeftWindow.FightStrengthNum = 0;
        TeamManager.ResetData();
        // Open try udp get server ip info window, Show current server area and Connect server area

        //DataCenter.OpenWindow("TRY_REQUEST_SERVER_IP_WINDOWS");

		DataCenter.OpenWindow("START_GAME_LOADING_WINDOW");

        /// NOTE: Run when click try request server ip in TRY window
        //LoginNet.StartLogin();
        //DataCenter.OpenWindow("START_GAME_LOADING_WINDOW");
        
    }
    //by chenliang
    //begin

    //清除当前账号数据、界面
    public static void ClearAccountUIAndData()
    {
        GameCommon.ResetWorldCameraColor();
        MainProcess.ClearBattle();
        ClearAllWindow();

        Guide.Terminate(false);
        //GuideManager.FinishGuide();
        //OptionalGuide.ClearGuide();
        //PVEStageBattle.mbAutoBattle = false;
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

        //停止心跳
        StaticDefine.useHeartbeat = false;

        RammbockBattle.IsInRammbockBattle = false;

        NewShopWindow tmpShopWin = DataCenter.GetData("SHOP_WINDOW") as NewShopWindow;
        if (tmpShopWin != null)
            tmpShopWin.set("ALREADY_OPEN_VIP_SHOP", null);

        RoleInfoTimerManager.Instance.Clear();

        LoginData.Instance.IsLoginTokenValid = false;
        LoginData.Instance.IsGameTokenValid = false;
        LoginData.Instance.IsInGameScene = false;
    }

    //end

	static public void ChangeAccount()
	{
        GameCommon.ResetWorldCameraColor();
		MainProcess.ClearBattle();
		ClearAllWindow();

        Guide.Terminate(false);
        //GuideManager.FinishGuide();
        //OptionalGuide.ClearGuide();
        //PVEStageBattle.mbAutoBattle = false;
        PVEStageBattle.mBattleControl = BATTLE_CONTROL.MANUAL;
        GuildBossBattle.mBattleControl = BATTLE_CONTROL.MANUAL;
		MainUIScript.mCurIndex = MAIN_WINDOW_INDEX.RoleSelWindow;
		DataCenter.Set("CURRENT_STAGE", 0);
		Notification.Clear();
		Notification.RefuseAll();
        ScrollWorldMapWindow.Reset();
        if (GlobalModule.Instance != null)
            GlobalModule.Instance.StopAllCoroutines();

		if(FriendLogicData.mSelectPlayerData != null)
			FriendLogicData.mSelectPlayerData = null;
		if(PetLogicData.mFreindPetData != null)
			PetLogicData.mFreindPetData = null;

		RoleSelTopLeftWindow.FightStrengthNum = 0;
        TeamManager.ResetData();
        //by chenliang
        //begin

// 		DataCenter.SetData ("LOGIN_WINDOW", "BACK_WINDOW_NAME", "LANDING_WINDOW");
// 		DataCenter.OpenWindow("LOGIN_WINDOW", false);
//
//        GameCommon.SetBackgroundSound("Sound/Opening", 0.5f);
//----------------
        //标记，重新获取队伍、背包数据
        CommonParam.isTeamManagerInit = false;
        CommonParam.isDataInit = false;

        //停止心跳
        StaticDefine.useHeartbeat = false;

        RammbockBattle.IsInRammbockBattle = false;

        NewShopWindow tmpShopWin = DataCenter.GetData("SHOP_WINDOW") as NewShopWindow;
        if (tmpShopWin != null)
            tmpShopWin.set("ALREADY_OPEN_VIP_SHOP", null);

        RoleInfoTimerManager.Instance.Clear();

        LoginData.Instance.IsLoginTokenValid = false;
        LoginData.Instance.IsGameTokenValid = false;
        LoginData.Instance.IsInGameScene = false;
#if !UNITY_EDITOR && !NO_USE_SDK
        //根据SDK做不同操作
        if (CommonParam.isUseSDK)
        {
            DataCenter.OpenWindow("FIRST_LOAD_SDK_WINDOW");
            //停止心跳包
            StaticDefine.useHeartbeat = false;
            //不自动登录
//            GlobalModule.DoOnNextUpdate(() =>
//            {
//                GlobalModule.DoLater(
//                    () =>
//                    {
//                        U3DSharkSDK.Instance.Login();
//                    }, 0.1f);
//            });
        }
        else
#endif
        {
            DataCenter.SetData("LOGIN_WINDOW", "BACK_WINDOW_NAME", "LANDING_WINDOW");
            DataCenter.OpenWindow("LOGIN_WINDOW", false);

            GameCommon.SetBackgroundSound("Sound/Opening", 0.7f);
        }

        //end

        // private_chat
        DataCenter.SetData("CHAT_PRIVATE_WINDOW", "TARGET_NAME", "");
        DataCenter.SetData("CHAT_PRIVATE_WINDOW", "TARGET_ID", "");
	}


    //WARN: Must clear all game logic data before reload config
    static public bool ReloadConfigTable()
    {
        TableManager.GetMe().ClearAll();
        InitLoadConfigTable();
        
        DataCenter.Self.InitResetGameData();

		return true;
    }
    
    static public void InitGameData()
    {
        InitGameDataWithoutTable();
        InitLoadConfigTable();
    }

    static public void InitGameDataWithoutTable()
    {
        if (sbInitGameData)
            return;

        sbInitGameData = true;

        UnityEngine.Random.seed = Environment.TickCount;

        EventCenter.Self = new EventCenter();
        EventCenter.Self.InitSetTimeManager(new GameTimeManager());
        EventCenter.Self.SetLog(new CLog());

        GameEvent.RegisterAIEvent();
        GameEvent.RegisterEffect();
        GameEvent.RegisterUIEvent();

        GameEvent.RegisterTimeEvent();

        ObjectManager.Self.RegisterObjectFactory(new CharatorerFactory());
        ObjectManager.Self.RegisterObjectFactory(new MonsterFactory());
        ObjectManager.Self.RegisterObjectFactory(new PetFactory());
        ObjectManager.Self.RegisterObjectFactory(new MonsterBossFactory());
        ObjectManager.Self.RegisterObjectFactory(new BigBossFactory());
        ObjectManager.Self.RegisterObjectFactory(new PVPActiveFactory());
		ObjectManager.Self.RegisterObjectFactory(new OpponentPetFactory());
		ObjectManager.Self.RegisterObjectFactory(new OpponentCharacterFactory());
		
		// task need data
		tLogicData taskNeedData = new tLogicData();
		
		DataCenter.RegisterData("TASK_NEED_DATA", taskNeedData);

       
        //by chenliang
        //begin

        RammbockBattle.IsInRammbockBattle = false;

        //end
    }

	// Use this for initialization
	void Start () {
#if UNITY_ANDROID || UNITY_IPHONE
        //by chenliang
        //begin

//		Handheld.PlayFullScreenMovie ("game_mv.mp4", Color.black, FullScreenMovieControlMode.CancelOnInput);
//----------------

        //end
#endif
        mInstance = this;

        //////////////////////////////
        //// Asset Loader init
        //AssetManager.Init(this);
        //SceneAsset.DontDestroyOnLoad(this.gameObject);

        //BundleAssetManager.AssetsRoot = Helper.CombinePath(BundleAssetManager.AssetsRoot, Application.streamingAssetsPath);
        ////////////////////////
        //by chenliang
        //begin

        SDKPayHelper.Instance.coroutineOperation = this;
        SceneObjLoader.Instance.coroutineOperation = this;

        //如果没通过热更新，这里初始化客户端版本
        if (!HotUpdateLoading.IsUseHotUpdate)
            HotUpdateLoading.LoadLocalClientVersion(false);

        //end

        // screen resolution
        SetResolution();

        //by chenliang
        //begin

//        if (isHotLoading)
//--------------
        if (HotUpdateLoading.IsUseHotUpdate)

        //end
            InitLoadConfigTable();
        else
            InitGameData();

		mSceneRoot = GameObject.Find("RootObject");

        Net.Init();
		HpBar.Init();
        SystemStateManager.Init();
		//added by xuke begin
		GetPathHandlerDic.Init ();
        BattleFailManager.Self.Init();
        //NewOpenFuncDic.Init();
        //NewOpenFuncDic.InitNewFuncSpriteDic();
        if (ChangeTipManager.Self != null) 
        {
            ChangeTipManager.Self.InitTargetDic();
        }
		//end

        DataCenter.Set("CURRENT_STAGE", 1000);

        DataCenter.Self.InitResetGameData();

        if (CommonParam.isUseSDK)
        {
            PushMessageManager.Self.RemoveAllLocalPush();
            PushMessageManager.Self.InitReadPushInfo();
        }

        //by chenliang
        //begin

//		DataCenter.OpenWindow("START_GAME_LOADING_WINDOW");
//-------------
        //转移到__PlayCG函数中，在动画播放之后

        //end

#if DEBUG_VERSION
		Application.RegisterLogCallback(HandleDebugInfo);
#endif

        Notification.RefuseAll();

        //by chenliang
        //begin

//         stopwatch = new Stopwatch();
//         stopwatch.Reset();
//-----------------
        if(stopwatch == null)
            stopwatch = new Stopwatch();
        stopwatch.Reset();

//#if UNITY_ANDROID || UNITY_IPHONE
//        Handheld.PlayFullScreenMovie("game_mv.mp4", Color.black, FullScreenMovieControlMode.CancelOnInput);
//#endif
        StartCoroutine(__PlayCG());

        //end
        stopwatch.Start();

        // 战斗音效管理
        gameObject.AddComponent<FightingSoundControler>();
        FightingSoundControler controler = gameObject.GetComponent<FightingSoundControler>();
        controler.Init(
            (int)DataCenter.mGlobalConfig.GetData("MAX_SCENE_SOUND", "VALUE"),
            DataCenter.mSoundControl,
            (float)DataCenter.mGlobalConfig.GetData("ATTENUATION_TIME", "VALUE")
            );
	}
    //by chenliang
    //begin

	//播放过场动画，因为要发送BI数据，所以放在单独协同函数里，等待2帧，必要数据初始化完成后发送BI数据、播放CG
    private IEnumerator __PlayCG()
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        //BI播放过场动画
#if UNITY_ANDROID || UNITY_IOS
        Handheld.PlayFullScreenMovie("game_mv.mp4", Color.black, FullScreenMovieControlMode.CancelOnInput);
#endif

        DataCenter.OpenWindow("START_GAME_LOADING_WINDOW");

        //代码备份
        //提供可跳过视频提示开关
        /*
        if (!CommonParam.IsUseMVSkipTip)
        {
            MVControlWindow.EnableAudioVedioPlayer(false);
            yield return new WaitForEndOfFrame();      //隔一帧让EnableAudioVedioPlayer生效
#if UNITY_ANDROID || UNITY_IOS
            Handheld.PlayFullScreenMovie("game_mv.mp4", Color.black, FullScreenMovieControlMode.CancelOnInput);
#endif
            DataCenter.OpenWindow("START_GAME_LOADING_WINDOW");
        }
        else
        {
            MVControlWindow.EnableAudioVedioPlayer(true);
            yield return new WaitForEndOfFrame();      //隔一帧让EnableAudioVedioPlayer生效
            string[] path = new string[]
            {
                "game_mv.ogv",              //视频路径（StreamingAssets文件夹下）
                "Sound/game_mv_audio"       //视频对应音频的路径(Resources/Sound文件夹下)
            };

            DataCenter.OpenWindow("MV_CONTROL_WINDOW", new MVControlParam()
            {
                AtlasName = "CommonUIAtlas",
                SpriteName = "a_ui_tiaoguodonghua",
                Path = path
            });

            Action tmpCompleteCallback = () =>
            {
                DataCenter.OpenWindow("START_GAME_LOADING_WINDOW");
            };
            DataCenter.SetData("MV_CONTROL_WINDOW", "COMPLETE_CALLBACK", tmpCompleteCallback);
        }
         * */
    }

    //end

	public void SetResolution()
	{
        //Screen.SetResolution(1280, 800, true, 60);
        //Transform cameraggTrans = transform.Find("Camera");
        //Camera mainCamera = cameraggTrans.GetComponent<Camera>();
        //mainCamera.aspect = 1.78f;
        //return;
        if (Screen.currentResolution.width > 2000)
        {
            Screen.SetResolution(1920, 1080, true);
        }

		float fScreenResoution = Screen.width * 1.0f/Screen.height;
		Transform cameraTrans = transform.Find("Camera");
		if(fScreenResoution >= (1136-35*2)/640.0f && fScreenResoution < 16.0f/9)
		{
			return;
		}
		else if(fScreenResoution < (1136-35*2)/640.0f && fScreenResoution >= 3.0f/2)
		{
			float ratio = fScreenResoution / ((1136-35*2)/640.0f);
			for(int i = 0; i < cameraTrans.childCount; i++)
			{
				cameraTrans.GetChild (i).localScale = new Vector3(ratio, ratio, ratio);
			}
		}
		else if(fScreenResoution < 3.0f/2)
		{
            float ratio = (3.0f / 2) / ((1136 - 35 * 2) / 640.0f);
            for (int i = 0; i < cameraTrans.childCount; i++)
            {
                cameraTrans.GetChild(i).localScale = new Vector3(ratio, ratio, ratio);
            }

            Camera camera = cameraTrans.GetComponent<Camera>();
            SetResolution(camera);
		}
	}

	public static void SetResolution(Camera camera)
	{
		float fScreenResoution = Screen.width * 1.0f/Screen.height;
		if(fScreenResoution < 3.0f/2)
		{
            float fViewPortRatio = fScreenResoution / (3.0f / 2);
			if(camera != null)
			{
				camera.rect = new Rect(0, (1 - fViewPortRatio)/2, 1, fViewPortRatio);
			}
		}
	}

    public static void SetResolutionEx(Camera camera)
    {
        if (camera == null)
            return;
        float fScreenResoution = Screen.width * 1.0f / Screen.height;
        Transform cameraTrans = camera.transform;
        if (fScreenResoution >= 16.0f / 9.0f)
        {
            float ratio = fScreenResoution / ((1136 - 35 * 2) / 640.0f);
            for (int i = 0; i < cameraTrans.childCount; i++)
            {
                cameraTrans.GetChild(i).localScale = new Vector3(ratio, ratio, ratio);
            }
            camera.rect = new Rect(0, 0, 1, 1);
        }
        else if (fScreenResoution >= (1136 - 35 * 2) / 640.0f && fScreenResoution < 16.0f / 9)
        {
            float ratio = fScreenResoution / ((1136 - 35 * 2) / 640.0f);
            for (int i = 0; i < cameraTrans.childCount; i++)
            {
                cameraTrans.GetChild(i).localScale = new Vector3(ratio, ratio, ratio);
            }
            camera.rect = new Rect(0, 0, 1, 1);
        }
        else if (fScreenResoution < (1136 - 35 * 2) / 640.0f && fScreenResoution >= 3.0f / 2)
        {
            float ratio = fScreenResoution / ((1136 - 35 * 2) / 640.0f);
            for (int i = 0; i < cameraTrans.childCount; i++)
            {
                cameraTrans.GetChild(i).localScale = new Vector3(ratio, ratio, ratio);
            }
            camera.rect = new Rect(0, 0, 1, 1);
        }
        else if (fScreenResoution < 3.0f / 2)
        {
            float ratio = (3.0f / 2) / ((1136 - 35 * 2) / 640.0f);
            for (int i = 0; i < cameraTrans.childCount; i++)
            {
                cameraTrans.GetChild(i).localScale = new Vector3(ratio, ratio, ratio);
            }

            float fViewPortRatio = fScreenResoution / (3.0f / 2);
            camera.rect = new Rect(0, (1 - fViewPortRatio) / 2, 1, fViewPortRatio);
        }
    }

	public static float GetResolutionRatio()
	{
		float fScreenResoution = Screen.width * 1.0f/Screen.height;
		float ratio = 1.0f;
		if(fScreenResoution < (1136-35*2)/640.0f && fScreenResoution >= 3.0f/2)
		{
			ratio = fScreenResoution / ((1136-35*2)/640.0f);
		}
		else if(fScreenResoution < 3.0f/2)
		{
			ratio = (3.0f/2) / ((1136-35*2)/640.0f);
		}
		return ratio;
	}
    //by chenliang
    //begin

    /// <summary>
    /// 设置分辨率
    /// </summary>
    /// <param name="camera"></param>
    /// <param name="radio"></param>
    public static void SetCameraViewPoint(Camera camera, float radio)
    {
        if (camera == null)
            return;

        Rect tmpRect = new Rect();

        if (radio >= 1.0f)
        {
            tmpRect.width = 1.0f;
            tmpRect.height = 1.0f / radio;
        }
        else
        {
            tmpRect.width = radio;
            tmpRect.height = 1.0f;
        }

        camera.rect = tmpRect;
    }

    //end

	public static bool bIsCanClick(bool bIsNeedJudge)
	{
		if(bIsNeedJudge && sbIsNeedJudge)
		{
			int nowFrame = Time.frameCount;
//			DEBUG.LogError("nowFrame = " + nowFrame + "mClickEventCurFrame = " + mClickEventCurFrame);
			if(mClickEventCurFrame != nowFrame)
			{
				mClickEventCurFrame = nowFrame;
			}
			else
			{
				return false;
			}
		}

		return true;
	}

	public void Update()
	{
        if (StaticDefine.useHeartbeat)
        {
            heartbeatDelta += Time.deltaTime;

            if (heartbeatDelta > StaticDefine.heartbeatInterval)
            {
                DEBUG.LogWarning("heartbeatDelta = " + heartbeatDelta + ", Time.deltaTime = " + Time.deltaTime);

                //by chenliang
                //begin

//                 if (heartbeat == null)
//                 {
//                     heartbeat = new CS_Heartbeat();
//                 }
//----------------------
                if (heartbeat == null)
                    heartbeat = new CS_Heartbeat();
                //令牌需要重新设置
                heartbeat.ResetBaseData();

                //end

                HttpModule.Instace.SendGameServerMessageT<CS_Heartbeat>(heartbeat, NetManager.HeartBeatSuccess, NetManager.HeartBeatFail, false);
                heartbeatDelta = 0.0f;
                if (CommonParam.PingLogOnScreen)//屏幕输出ping信息
                {
                    heartbeatstopwatch.Reset();
                    heartbeatstopwatch.Start();
                    StartCoroutine(ScreenShowPingData());
                }
            }
        }

        Net.gNetEventCenter.Process(Time.deltaTime);
        Logic.EventCenter.Self.Process(Time.deltaTime);
        ObjectManager.Self.Update(Time.deltaTime);

        if (mAsyncOperation != null)
        {
            if (mAsyncOperation.isDone)
            {
                mAsyncOperation = null;
            }
        }

        //LuaBehaviour.Update();

        if (onUpdate != null)
        {
            onUpdate();
        }

        onUpdateSchedule.Update();

        if (Input.GetKeyDown(KeyCode.F))
        {
            stopwatch.Reset();
            stopwatch.Start();
        }

#if DEBUG_VERSION
		if(Input.GetKeyDown(KeyCode.H))
		{
			UiControl.self.ShowDebugInfoPanel(false);
		}
		else if(Input.GetKeyDown(KeyCode.G))
		{
			UiControl.self.ShowDebugInfoPanel(true);
		}

#endif

#if UNITY_IPHONE
#elif UNITY_ANDROID
		if(Input.GetKeyDown(KeyCode.Escape))
		{
			Settings.QuitGame();
		}
        //by chenliang
        //begin

// 		if(Input.GetKeyDown(KeyCode.Home))
// 		{
// 			DataCenter.OpenMessageWindow("??home....OnClick....ANDROID");
// 		}
//-----------------
        //去除Home键提示

        //end
#else
        //if (Application.platform == RuntimePlatform.Android && Input.GetKeyDown(KeyCode.Escape))
        //{
        //    Settings.QuitGame();
        //}
        //if (Application.platform == RuntimePlatform.Android && Input.GetKeyDown(KeyCode.Home))
        //{
        //    DataCenter.OpenMessageWindow("??home....OnClick");
        //}
#endif

        //by chenliang
        //begin

//        //检查是否网络可连接
//        if (mNeedCheckNetConnectable && !NetManager.HasAnyNetConnectable())
//        {
//            //如果在游戏里，重新获取数据，如果还未进游戏，就重新登录
//            Action tmpChangeAccount = GlobalModule.ChangeAccount;
//            NetManager.OpenNoNetConnectableWindow(
//                (LoginData.Instance.IsGameTokenValid && LoginData.Instance.IsInGameScene) ? null : tmpChangeAccount);
//        }
        //改为只在请求时检测

        if (mLoadSceneAsyncOP != null)
        {
            if (mLoadSceneProgress != null)
                mLoadSceneProgress(mLoadSceneAsyncOP.progress);
            if (mLoadSceneAsyncOP.isDone)
            {
                mLoadSceneAsyncOP = null;
                mLoadSceneProgress = null;
            }
        }

        //end
	}

    private void LateUpdate()
    {
        if (onLateUpdate != null)
        {
            onLateUpdate();
        }

        onLateUpdateSchedule.Update();
    }

	// Update is called once 0.02 ms
//    void FixedUpdate() 
//    {
//        if(StaticDefine.useHeartbeat)
//        {
//            heartbeatDelta += Time.fixedDeltaTime;
//            if (heartbeatDelta > StaticDefine.heartbeatInterval)
//            {
//                //DEBUG.LogError("heartbeatDelta = " + heartbeatDelta + ", Time.fixedDeltaTime = " + Time.fixedDeltaTime);

//                if (heartbeat == null)
//                {
//                    heartbeat = new CS_Heartbeat();
//                }
                
//                HttpModule.Instace.SendGameServerMessageT<CS_Heartbeat>(heartbeat, NetManager.HeartBeatSuccess, NetManager.HeartBeatFail);
//                heartbeatDelta = 0.0f;
//            }
//        }

//        Net.gNetEventCenter.Process(Time.fixedDeltaTime);
//        Logic.EventCenter.Self.Process(Time.fixedDeltaTime);
//        ObjectManager.Self.Update(Time.fixedDeltaTime);

//        if (mAsyncOperation != null)
//        {
//            if (mAsyncOperation.isDone)
//            {
//                mAsyncOperation = null;
//            }
//        }

//#if DEBUG_VERSION
//        if(Input.GetKeyDown(KeyCode.H))
//        {
//            UiControl.self.ShowDebugInfoPanel(false);
//        }
//        else if(Input.GetKeyDown(KeyCode.G))
//        {
//            UiControl.self.ShowDebugInfoPanel(true);
//        }

//#endif
//    }

    public static GlobalModule Instance
    {
        get
        {
            //if (mInstance == null)
            //{
            //    // 查找是否存在GlobalModule对象，若不存在则创建一个
            //    GameObject obj = GameObject.Find("GlobalModule");
            //    if (obj == null)
            //    {
            //        obj = GameObject.Instantiate(Resources.Load("Prefabs/GlobalModule"), Vector3.zero, Quaternion.identity) as GameObject;
            //        obj.name = "GlobalModule";
            //        //obj.GetComponent<AutoScaleCenter>().NeedReset = true;
            //    }

            //    mInstance = obj.GetComponent<GlobalModule>();
            //}

            return mInstance;
        }
    }

    void Awake()
    {

        if (isLog)
        {
            System.AppDomain.CurrentDomain.UnhandledException += _OnUnresolvedExceptionHandler;
            Application.RegisterLogCallback(_OnDebugLogCallbackHandler);
        }

        //by chenliang
        //begin

        mCountdownTimer = gameObject.GetComponent<CountdownTimer>();
        if (mCountdownTimer == null)
            mCountdownTimer = gameObject.AddComponent<CountdownTimer>();

        //end
#if BUILDING_CONFIG
		PlayerPrefs.SetInt("BUILDING_CONFIG", 1);
#endif
        // 不销毁本节点
        DontDestroyOnLoad(gameObject);

        // 全局限帧
        Application.targetFrameRate = 40;

        Screen.orientation = ScreenOrientation.AutoRotation;
        Screen.autorotateToLandscapeRight = true;
        Screen.autorotateToLandscapeLeft = true;
        Screen.autorotateToPortrait = false;
        Screen.autorotateToPortraitUpsideDown = false;
		Screen.fullScreen = true;

        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        Settings.Load();
//		Resolution[] resolutions = Screen.resolutions;
//		foreach (Resolution res in resolutions) {
//			print(res.width + "x" + res.height);
//		}
//		Screen.SetResolution(resolutions[0].width, resolutions[0].height, true);
		
		//LoadScene("LogScene", true);
#if DEBUG_NET
        LoadScene("ui", false);
		LoadScene("Game", true);
#else			
		// 载入第一个场景		        
//		LoadScene("Loading", false);

#endif
    }

    void OnDestroy()
    {
        checkClickFlagLock.ReleaseAll();
        Settings.Save();
    }

	// the _scene must be specify in the building Setting
    public void LoadScene(string _scene, bool _additive)
    {
        /*
        if (!Application.CanStreamedLevelBeLoaded(_scene))
        {
            DEBUG.Log("Can't load level: " + _scene);
			//UnityEditor.EditorUtility.DisplayDialog("Title", "Can't load level: " + _scene, "Button");
            return;
        }*/
        // this is for play animation when load scene, in the future.
        PlayerPrefs.SetString("TargetScene", _scene);

        mAsyncOperation = null;
		
		// play loading animation
		//*****
		
		BeginLoadScene(_additive);
    }

    private void BeginLoadScene(bool _additive)
    {
        if (!_additive)
        {
            //LoadEmptyScene();
        }
        Resources.UnloadUnusedAssets();

        if (_additive)
        {
            //mAsyncOperation = Application.LoadLevelAdditiveAsync(PlayerPrefs.GetString("TargetScene"));
			//Application.LoadLevelAdditive(PlayerPrefs.GetString("TargetScene"));
            GameCommon.mResources.LoadLevelAdditive(PlayerPrefs.GetString("TargetScene"));
        }
        else
        {
            //mAsyncOperation = Application.LoadLevelAsync(PlayerPrefs.GetString("TargetScene"));
			//Application.LoadLevel(PlayerPrefs.GetString("TargetScene"));
            GameCommon.mResources.LoadLevel(PlayerPrefs.GetString("TargetScene"));
        }

    }
    //by chenliang
    //begin

    // the _scene must be specify in the building Setting
    /// <summary>
    /// 异步加载场景
    /// </summary>
    /// <param name="_scene"></param>
    /// <param name="_additive"></param>
    public AsyncOperation LoadSceneAsync(string scene, bool additive)
    {
        PlayerPrefs.SetString("TargetScene", scene);

        // play loading animation
        //*****

        return __BeginLoadSceneAsync(additive);
    }
    /// <summary>
    /// 开始异步加载场景
    /// </summary>
    /// <param name="additive"></param>
    private AsyncOperation __BeginLoadSceneAsync(bool additive)
    {
        Resources.UnloadUnusedAssets();

        if (!PlayerPrefs.HasKey("TargetScene"))
            return null;

        AsyncOperation tmpAsync = null;
        string tmpTargetScene = PlayerPrefs.GetString("TargetScene");
        if (additive)
            tmpAsync = Application.LoadLevelAdditiveAsync(tmpTargetScene);
        else
            tmpAsync = Application.LoadLevelAsync(tmpTargetScene);
        return tmpAsync;
    }

    //end

    private void LoadSceneByAssertBundle(string levelName, bool _additive)
    {

        if (_additive)
        {
            Application.LoadLevelAdditive(levelName);
        }
        else
        {
            Application.LoadLevel(levelName);
        }

    }

    private IEnumerable LoadEmptyScene()
    {
        AsyncOperation ao = Application.LoadLevelAsync("Empty");
        if (ao.isDone)
        {
            yield return ao;
        }
    }

    IEnumerator ScreenShowPingData()
    {
        WWW www_instance = new WWW("115.239.211.112");
        wwwstopwatch.Reset();
        wwwstopwatch.Start();

        yield return www_instance;
        wwwstopwatch.Stop();

        ScreenLogger.Log("baidu: " + wwwstopwatch.ElapsedMilliseconds + " ms", LOG_LEVEL.WARN);
    }

	public void ShowScene(bool b)
	{
		if(null!=mSceneRoot)
		{
			mSceneRoot.SetActive(b);
		}
	}
	
	
	public void HandleDebugInfo(string logString, string stackTrace, LogType type)
	{
		if(type == LogType.Warning || type==LogType.Exception)
		{
			/*
			GameObject item  = (GameObject)GameObject.Instantiate(InfoItem);
			item.transform.position = InfoItem.transform.position;
			item.transform.rotation = InfoItem.transform.rotation;
			item.transform.localScale = InfoItem.transform.localScale;
			//item.transform.Scale = new Vector3(1.0f, 1.0f, 1.0f);
			UILabel label = item.GetComponentInChildren<UILabel>();
			label.text = logString;
			item.transform.parent = InfoItem.transform.parent;
			*/
			//UiControl.self.AddDebugInfo(logString, stackTrace, type);
			
		}
	}

	public virtual void UpdateStaminaTime()
	{
        RoleLogicData.Self.UpdateStaminaTime();
	}
	
	public virtual void StartUpdateStaminaTime()
	{
        if (!IsInvoking("UpdateStaminaTime"))
        {
            RoleLogicData roleLogicData = RoleLogicData.Self;
            //roleLogicData.mStaminaTime = roleLogicData.mStaminaMaxTime;
            InvokeRepeating("UpdateStaminaTime", 1, 1);
        }
	}
	
	public virtual void StopUpdateStaminaTime()
	{
		if(!IsInvoking("UpdateStaminaTime"))
			CancelInvoke("UpdateStaminaTime");
	}


    static public void ClearAllWindow(bool isDestroy = false)
    {
        ClearAllWindow(null, isDestroy);
    }

    static public void ClearAllWindow(IEnumerable<string> ignoreList, bool isDestroy = false)
	{
        Notification.Stop();
        GameObject camObj = GameCommon.FindUI("Camera");
        Transform camera = camObj != null ? camObj.transform : null;

        if (camera != null)
        {
            List<Transform> delTransList = new List<Transform>();
            int iCount = camera.childCount;
            for (int i = 0; i < iCount; i++)
            {
                delTransList.Clear();
                foreach (Transform tran in camera.GetChild(i))
                {
                    if (tran != null 
                        && tran.name != "top_message_window" 
                        && tran.name != "wait_time_effect" 
                        && tran.name != "guide_mask_window"
                        && tran.name != "guide_base_mask_window")
                    {
                        if (ignoreList != null && ignoreList.Any<string>(x => x == tran.name))
                        {
                            continue;
                        }

                        if (isDestroy && tran.GetComponent<GameLoadingUI>() == null)
                        {
                            delTransList.Add(tran);
                        }
                        else
                        {
                            tran.gameObject.SetActive(false);
                        }
                    }
                }

                foreach (Transform tempTran in delTransList)
                {
                    if (tempTran != null)
                    {
                        GameObject.DestroyImmediate(tempTran.gameObject);
                    }
                }
            }
        }
        

        Notification.Start();
	}

    static public void DestroyAllWindow()
    {       
        Transform camera = mInstance.transform.Find("Camera");
        if (camera != null)
        {
            int iCount = camera.childCount;
            for (int i = 0; i < iCount; i++)
            {
                foreach (Transform tran in camera.GetChild(i))
                {
                    if (tran != null && tran.gameObject.name != "top_message_window")                        
                    	GameObject.Destroy(tran.gameObject);
                }
            }
        }       
    }

    static public void DoLater(Action action, float delay)
    {
        Instance.StartCoroutine(Instance._DoLater(action, delay));
    }

    private IEnumerator _DoLater(Action action, float delay)
    {
        if (action != null)
        {
            yield return new WaitForSeconds(delay);
            action();
        }
    }

    static public Coroutine DoCoroutine(IEnumerator co)
    {
        return Instance.StartCoroutine(co);
    }
    //by chenliang
    //begin

    /// <summary>
    /// 获取全局倒计时
    /// </summary>
    public CountdownTimer countdownTimer
    {
        get { return mCountdownTimer; }
    }
    //end

    private static ActionScheduleList onUpdateSchedule = new ActionScheduleList();
    private static ActionScheduleList onLateUpdateSchedule = new ActionScheduleList();

    /// <summary>
    /// 在下一帧的Update中执行一个委托
    /// </summary>
    /// <param name="action"> 准备执行的委托 </param>
    public static void DoOnNextUpdate(Action action)
    {
        DoOnNextUpdate(1, action);
    }

    /// <summary>
    /// 在若干帧后的Update中执行一个委托
    /// </summary>
    /// <param name="frame"> 帧数 </param>
    /// <param name="action"> 准备执行的委托 </param>
    public static void DoOnNextUpdate(int frame, Action action)
    {
        onUpdateSchedule.Add(action, Time.frameCount + frame);
    }

    /// <summary>
    /// 取消准备在Update中执行的某个委托
    /// </summary>
    /// <param name="action"> 取消的委托 </param>
    /// <returns> 被取消的委托是否存在 </returns>
    public static bool CancleOnNextUpdate(Action action)
    {
        return onUpdateSchedule.Cancle(action);
    }

    /// <summary>
    /// 在下一帧的LateUpdate中执行一个委托
    /// </summary>
    /// <param name="action"> 准备执行的委托 </param>
    public static void DoOnNextLateUpdate(Action action)
    {
        DoOnNextLateUpdate(1, action);
    }

    /// <summary>
    /// 在若干帧后的LateUpdate中执行一个委托
    /// </summary>
    /// <param name="frame"> 帧数 </param>
    /// <param name="action"> 准备执行的委托 </param>
    public static void DoOnNextLateUpdate(int frame, Action action)
    {
        onLateUpdateSchedule.Add(action, Time.frameCount + frame);
    }

    /// <summary>
    /// 取消准备在LateUpdate中执行的某个委托
    /// </summary>
    /// <param name="action"> 取消的委托 </param>
    /// <returns> 被取消的委托是否存在 </returns>
    public static bool CancleOnNextLateUpdate(Action action)
    {
        return onLateUpdateSchedule.Cancle(action);
    }

    private class ActionScheduleList
    {
        private class ActionSchedule
        {
            public Action mAction;
            public int mTargetFrame;
        }

        private List<ActionSchedule> mSchedules = new List<ActionSchedule>();

        /// <summary>
        /// 添加或更新一个委托事件
        /// </summary>
        /// <param name="action"> 委托 </param>
        /// <param name="targetFrame"> 委托执行的时间（帧） </param>
        public void Add(Action action, int targetFrame)
        {
            Cancle(action);
            mSchedules.Add(new ActionSchedule { mAction = action, mTargetFrame = targetFrame });
        }

        /// <summary>
        /// 取消一个委托事件
        /// </summary>
        /// <param name="action"> 被移除的委托 </param>
        public bool Cancle(Action action)
        {
            return mSchedules.RemoveAll(x => x.mAction == action) > 0;
        }

        /// <summary>
        /// 取消所有的委托事件
        /// </summary>
        public void CancleAll()
        {
            mSchedules.Clear();
        }

        /// <summary>
        /// 刷新委托事件列表，若存在满足条件的委托则执行并从列表中移除
        /// </summary>
        public void Update()
        {
            int count = mSchedules.Count;

            for (int i = 0; i < count; ++i)
            {
                if (mSchedules[i].mTargetFrame <= Time.frameCount)
                {
                    mSchedules[i].mAction();
                }
            }

            mSchedules.RemoveAll(x => x.mTargetFrame <= Time.frameCount);
        }
    }

    void OnApplicationPause()
    {
        return;
#if UNITY_IPHONE || UNITY_ANDROID
        DEBUG.LogError("OnApplicationPause............:isGamePause = " + isGamePause.ToString() + ", isGameFocus = " + isGameFocus.ToString());
        if (!isGamePause)
        {
            DEBUG.LogError("OnApplicationPause............::isGamePause pauseTime");
            // 强制暂停时，事件
            Settings.SetGamePaused(true);
        }
        else
        {
            Settings.SetGamePaused(false);
            isGameFocus = true;
        }
        isGamePause = true;
#endif
    }

    void OnApplicationFocus()
    {
        return;
#if UNITY_IPHONE || UNITY_ANDROID
        DEBUG.LogError("OnApplicationFocus:isGamePause = " + isGamePause.ToString() + ", isGameFocus = " + isGameFocus.ToString());
        if (isGameFocus)
        {
            DEBUG.LogError("OnApplicationFocus:isGamePause resumeList");
            //“启动”手机时，事件
            //Settings.SetGamePaused(false);
            isGamePause = false;
            isGameFocus = false;
        }
        if (isGamePause)
        {
            isGameFocus = true;
        }
#endif
    }

    void OnEnable()
    {
        isGamePause = false;
        isGameFocus = false;
    }

    // 异常捕获
    private static void _OnUnresolvedExceptionHandler(object sender, System.UnhandledExceptionEventArgs args)
    {
        if (args == null || args.ExceptionObject == null)
        {
            return;
        }

        if (args.ExceptionObject.GetType() != typeof(System.Exception))
        {
            return;
        }

        doLogError((System.Exception)args.ExceptionObject);
    }

    private static void doLogError(System.Exception e)
    {
        System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(e, true);
        string[] classes = new string[stackTrace.FrameCount];
        string[] methods = new string[stackTrace.FrameCount];
        string[] files = new string[stackTrace.FrameCount];
        int[] lineNumbers = new int[stackTrace.FrameCount];

        String message = "";

        for (int i = 0; i < stackTrace.FrameCount; i++)
        {
            System.Diagnostics.StackFrame frame = stackTrace.GetFrame(i);
            classes[i] = frame.GetMethod().DeclaringType.Name;
            methods[i] = frame.GetMethod().Name;
            files[i] = frame.GetFileName();
            lineNumbers[i] = frame.GetFileLineNumber();

            message += classes[i] + ".";
            message += methods[i] + "() (at ";
            message += files[i] + ":";
            message += lineNumbers[i] + ")";
        }
        
        DEBUG.LogError("e.GetType().Name = " + e.GetType().Name + ", message = " + message);

    }
    public void getWindowColorFun(Color[] tmpWindowColor)
    {
        mWindowColor = tmpWindowColor;
    }
    public void SetWindowGuildColor(Color[] color)
    {
        mWindowGuideColor = color;
    }
    private Texture mPointMarkTex;
    /// <summary>
    /// by qiufeng
    /// </summary>
    void OnGUI()
    {
        if ( CommonParam.UsePointMark)
        {
            if (mPointMarkTex == null)
                mPointMarkTex = GameCommon.LoadTexture("textures/window_point_pic");
            Color[] tmpShowColor = null;
            if (mWindowGuideColor == null)
            {
                if (mWindowColor != null && mWindowColor.Length >= 3)
                    tmpShowColor = mWindowColor;
                else
                    tmpShowColor = mWindowDisableColor;
            }
            else
                tmpShowColor = mWindowGuideColor;
            if (tmpShowColor != null)
            {
                Color reColor = GUI.color;
                GUI.color = tmpShowColor[0];
                GUI.DrawTexture(new Rect(1, 0, 1, 1), mPointMarkTex, ScaleMode.ScaleToFit, true, 0);
                GUI.color = tmpShowColor[1];
                GUI.DrawTexture(new Rect(2, 0, 1, 1), mPointMarkTex, ScaleMode.ScaleToFit, true, 0);
                GUI.color = tmpShowColor[2];
                GUI.DrawTexture(new Rect(3, 0, 1, 1), mPointMarkTex, ScaleMode.ScaleToFit, true, 0);
                GUI.color = reColor;
            }
            /*
            if (mWindowColor != null && mWindowColor.Length >= 3)
            {

                //             int[] arr_c1 = mWindowColor[0], arr_c2 = mWindowColor[1], arr_c3 = mWindowColor[2];
                //             float t_r1 = arr_c1[1], t_g1 = arr_c1[2], t_b1 = arr_c1[3];
                //             UnityEngine.Debug.LogError("..................................." + mWindowColor[2].r + mWindowColor[2].g + mWindowColor[2].b + "\t\t" + "................................");
                //             Texture2D tmpTex = new Texture2D(1, 1);
                //             tmpTex.SetPixel(0, 0, new Color(t_r1, t_g1, t_b1));
                //             tmpTex.Apply();
                //             GUI.DrawTexture(new Rect(0, 0, 1, 1), tmpTex);
                Color reColor = GUI.color;
                GUI.color = mWindowColor[0];
                GUI.DrawTexture(new Rect(1, 0, 1, 1), mPointMarkTex, ScaleMode.ScaleToFit, true, 0);
                GUI.color = mWindowColor[1];
                GUI.DrawTexture(new Rect(2, 0, 1, 1), mPointMarkTex, ScaleMode.ScaleToFit, true, 0);
                GUI.color = mWindowColor[2];
                GUI.DrawTexture(new Rect(3, 0, 1, 1), mPointMarkTex, ScaleMode.ScaleToFit, true, 0);
                GUI.color = reColor;

                //                 Texture2D tmpTex = new Texture2D(4, 1);
                //                 tmpTex.SetPixel(1, 0, mWindowColor[0]); UnityEngine.Debug.LogWarning("Color mark - " + mWindowColor[0]);
                //                 tmpTex.SetPixel(2, 0, mWindowColor[1]);
                //                 tmpTex.SetPixel(3, 0, mWindowColor[2]);
                //                 LOG.logWarn("The Draw Color is" + mWindowColor[0] + mWindowColor[1] + mWindowColor[2]);
                //                 tmpTex.Apply();
                //                 GUI.DrawTexture(new Rect(0, 0, tmpTex.width, tmpTex.height), tmpTex);
            }
            else
            {
                Color reColor = GUI.color;
                GUI.color = Color.grey;
                GUI.DrawTexture(new Rect(1, 0, 1, 1), mPointMarkTex, ScaleMode.ScaleToFit, true, 0);
                GUI.color = Color.grey;
                GUI.DrawTexture(new Rect(2, 0, 1, 1), mPointMarkTex, ScaleMode.ScaleToFit, true, 0);
                GUI.color = Color.grey;
                GUI.DrawTexture(new Rect(3, 0, 1, 1), mPointMarkTex, ScaleMode.ScaleToFit, true, 0);
                GUI.color = reColor;
            }
             * */
        }
    }
    /// <summary>
    /// end
    /// </summary>
    /// <param name="name"></param>
    /// <param name="stack"></param>
    /// <param name="type"></param>

#if UNITY_EDITOR
    private static void _OnDebugLogCallbackHandler(string name, string stack, LogType type)
    {

    }
    private static void reportException(int type, string name, string message)
    {

    }
#elif UNITY_IPHONE
    private static void _OnDebugLogCallbackHandler (string name, string stack, LogType type)
	{
		if (LogType.Assert != type && LogType.Exception != type) {
			return;
		}

        if (!GlobalModule.Instance.isLog)
        {
			return;
		}
		
		try {
            DEBUG.LogError("name = " + name + ", stack = " + stack);
			//testinReportCustomizedException (TESTIN_CRASH_TYPE, name, stack);
		} catch (System.Exception e) {
			System.Console.Write ("Unable to log a crash exception to TestinAgent to an unexpected error: " + e.ToString ());
		}
	}
#elif UNITY_ANDROID
    private static void _OnDebugLogCallbackHandler (string name, string stack, LogType type)
	{
		if (LogType.Assert != type && LogType.Exception != type) {
			return;
		}
		
		if (!GlobalModule.Instance.isLog) {
			return;
		}
		
		try {
            DEBUG.LogError("name = " + name + ", stack = " + stack);
			//mTestinPlugin_ANDROID.CallStatic ("reportCustomizedException", TESTIN_CRASH_TYPE, name ,stack);
		} catch (System.Exception e) {
			System.Console.Write ("Unable to log a crash exception to TestinAgent to an unexpected error: " + e.ToString ());
		}
	}
#endif


    private static CheckClickFlagLock checkClickFlagLock = new CheckClickFlagLock();
    
    /// <summary>
    /// 解除对用户点击操作的频率的限制
    /// </summary>
    /// <param name="owner"> 解除限制的对象 </param>
    public static void DontCheckClick(object owner) { checkClickFlagLock.Retain(owner); }

    /// <summary>
    /// 释放“解除用户点击操作频率限制”锁，如果全部持有锁的对象均已释放，则还原限制状态
    /// </summary>
    /// <param name="owner"> 释放锁的对象 </param>
    public static void ReleaseCheckClickFlag(object owner) { checkClickFlagLock.Release(owner); }
}