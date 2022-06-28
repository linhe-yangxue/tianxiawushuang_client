//#define RELEASE_APP
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Logic;
using DataTable;

using System.Text;
using System.IO;
using System;
using Utilities;
using Utilities.Routines;


public class MainProcess : MonoBehaviour 
{
	static public MainProcess Self;

	BaseObject mTestMonster;

	static public Camera mMainCamera;
	static public GameObject mCameraObject;
    static public MouseCoord mMouseCoord;
    static public bool mbPress = false;

    static public CameraMoveEvent mCameraMoveTool;

    static public StageBattle mStage;           // 关卡

    static public Camera mUICamera;
    static public bool mLockCharacterPos = false;
    static public bool mbStartPetAIOnCreate = true;

    //static private bool needCheckClickFlagHasReserved = false;    // 战斗之前是否限制点击频率的状态是否已记录
    //static private bool reservedNeedCheckClickFlag = true;    // 记录战斗之前是否限制点击频率，用于退出战斗后恢复

    public BattleController mController;    // 关卡控制器
    public BattleAssist mAssist;            // 关卡辅助器
    private IBattleIntervencer mIntervencer;  // 关卡干预器
    private List<IBattleMonitor> mMonitorList = new List<IBattleMonitor>();    // 关卡监视器列表
	//GameObject mMainCharatorer;
	//--------------------------------------------

	void Start () 
	{
#if !UNITY_EDITOR && !NO_USE_SDK
        if(CommonParam.isUseSDK) {
            if(U3DSharkSDK.Instance.IsHasRequest(U3DSharkAttName.SUPPORT_SHARE))
                U3DSharkSDK.Instance.ShowPersonCenter();    
        }
#endif
        Self = this;
        mbStartPetAIOnCreate = true;
       
        mMainCamera = GameCommon.GetMainCamera();

        if (mMainCamera != null)
        {
            mCameraObject = mMainCamera.transform.parent.gameObject;
        }

		GameObject tempCam = GameCommon.FindObject(gameObject, "Camera");
        Transform tempTrans = null;

        if (tempCam != null)
        {
            tempTrans = tempCam.transform.parent;
        }

        if (tempTrans != null && mCameraObject != null && tempTrans.gameObject != mCameraObject)
		{
            mCameraObject.transform.position = tempTrans.position;
            mCameraObject.transform.rotation = tempTrans.rotation;
            mCameraObject.transform.localScale = tempTrans.localScale;
            //Directional light
            
            // 设置灯光
            GameObject LightObj = GameCommon.FindObject(mCameraObject, "SceneLight");
            if (LightObj == null)
            {
                LightObj = new GameObject("SceneLight");
                LightObj.AddComponent<Light>();
                LightObj.transform.parent = mMainCamera.transform;
            }
            Light light = LightObj.GetComponent<Light>();
            
            // 获取场景中“Camera_Z”下的“Directional light”的灯光数据，然后将数据赋给world_center中的Light
            GameObject Camera_Z = GameObject.Find("Camera_Z");
            GameObject Camera_Z_LightObj = GameCommon.FindObject(Camera_Z, "Directional light");
            if (Camera_Z_LightObj != null)
            {
                LightObj.transform.localPosition = Camera_Z_LightObj.transform.localPosition;
                LightObj.transform.localEulerAngles = Camera_Z_LightObj.transform.localEulerAngles;
                LightObj.transform.localScale = Camera_Z_LightObj.transform.localPosition;

                Light Camera_z_light = Camera_Z_LightObj.GetComponent<Light>();
                if (Camera_z_light != null)
                {
                    light.color = Camera_z_light.color;
                    light.intensity = Camera_z_light.intensity;
                }
                Camera_Z_LightObj.SetActive(false);
            }
            else
            {
                LightObj.transform.localPosition = Vector3.zero;
                LightObj.transform.localEulerAngles = new Vector3(5, 27, 0);
                LightObj.transform.localScale = Vector3.one;

                light.color = new Color(208 / 255.0f, 238 / 255.0f, 231 / 255.0f);
                light.intensity = 0.5f;
            }
            light.type = LightType.Directional;
            light.cullingMask = (1 << LayerMask.NameToLayer("Player")) | (1 << LayerMask.NameToLayer("Charater"));
            
            RenderSettings.ambientLight = Color.white;

            tempCam.SetActive(false);

            //GameCommon.SetMainCameraEnable(true);
            mMainCamera.enabled = true;

			// set fog color to main camera background color 
            mMainCamera.backgroundColor = RenderSettings.fogColor;
            mMainCamera.fieldOfView = tempCam.GetComponent<Camera>().fieldOfView;
		}     

        mCameraMoveTool = EventCenter.Start("CameraMoveEvent") as CameraMoveEvent;
        mCameraMoveTool.SetCameraShakeEnabled(false);
        //mCameraMoveTool.InitConfig(mStage != null ? (int)mStage.GetCameraType() : (int)CAMERA_TYPE.DEFAULT);  
        //mCameraMoveTool.StartUpdate();
        //Effect_ShakeCamera.InitCamera(mMainCamera.gameObject);
       
        //by chenliang
        //begin

//        mMouseCoord = EventCenter.Start("MouseCoord") as MouseCoord;
//        mMouseCoord.InitCreate(null, null, 0);
//
//        if (mStage != null)// && !GuideManager.IsInPrologue())
//        {
//            mStage.InitBattleUI();
//
//            if (!mStage.IsPVP())
//            {
//                mController = new BattleController();
//                mAssist = new BattleAssist();
//                gameObject.StartRoutine(mAssist);
//            }
//        }
//
//		GlobalModule.SetResolution(mMainCamera);
//        isOnCheating = false;
//---------------------
        GlobalModule.SetResolution(mMainCamera);
        mIsInited = false;

        //end
	}

    public void InitForSimulator()
    {
        __Init();
    }

    void OnDestroy()
    {
#if !UNITY_EDITOR && !NO_USE_SDK
        if(CommonParam.isUseSDK) {
            if(U3DSharkSDK.Instance.IsHasRequest(U3DSharkAttName.SUPPORT_SHARE))
                U3DSharkSDK.Instance.HidePersonCenter();    
        }
        
#endif
        //ClearBattle();
        isOnCheating = false;
        GlobalModule.ReleaseCheckClickFlag(this); // 如果战斗未正常结束，则在此释放点击检测状态锁
    }
    //by chenliang
    //begin

    private bool mIsInited = false;     //是否已经初始化

    /// <summary>
    /// 初始化数据
    /// </summary>
    private void __Init()
    {
        mMouseCoord = EventCenter.Start("MouseCoord") as MouseCoord;
        mMouseCoord.InitCreate(null, null, 0);

        if (mStage != null)// && !GuideManager.IsInPrologue())
        {
            mStage.InitBattleUI();

            if (!mStage.IsPVP() && !mStage.IsBOSS())
            {
                mController = new BattleController();

                if (!(mStage is SimulatedStage))
                {
                    mAssist = new BattleAssist();
                    gameObject.StartRoutine(mAssist);
                }
            }
        }

		GlobalModule.SetResolution(mMainCamera);
        isOnCheating = false;
    }

    //end

    //public static void SetNeedCheckClickFlag(bool flag)
    //{
    //    if (!needCheckClickFlagHasReserved)
    //    {
    //        needCheckClickFlagHasReserved = true;
    //        reservedNeedCheckClickFlag = UICamera.NeedCheckClick;
    //    }

    //    UICamera.NeedCheckClick = flag;
    //}

    //public static void ReserveNeedCheckClickFlag()
    //{
    //    if (needCheckClickFlagHasReserved)
    //    {
    //        needCheckClickFlagHasReserved = false;
    //        UICamera.NeedCheckClick = reservedNeedCheckClickFlag;
    //    }
    //}

    static public Vector3 WorldToUIPosition(Vector3 worldPos)
    {
		if (mUICamera==null)
			mUICamera = GameCommon.FindUI("Camera").GetComponent<Camera>();

		if (mUICamera!=null)
		{
        	Vector3 pos = GameCommon.GetMainCamera().WorldToScreenPoint(worldPos);
        	pos = mUICamera.ScreenToWorldPoint(pos);
        	pos.z = 0;
        	return pos;
		}
		DEBUG.LogWarning("No exist ui camera");
		return worldPos;
    }

    static public void UpdateMouseCoord()
    {
        Vector3 hit;

        if (GetMouseHitPoint(out hit))
        {
            UpdateMouseCoord(hit);
        }
    }

    static public void UpdateMouseCoord(Vector3 coordPos)
    {
            mMouseCoord.SetActive(true);
            //coordPos.y += 0.5f;
            if (mMouseCoord != null)
                mMouseCoord.SetPosition(coordPos);
    }

    // Update is called once per frame
	void Update () 
	{
        //by chenliang
        //begin

//         if (mbPress && !Character.Self.IsNeedMove() && !(Character.Self.mCurrentAI is AttackSkill))
//             MoveCharatorer();
//---------------------

        // 设置摄像机参数——暂作测试，实际设置不应放在Update
        //if (mMainCamera != null)
        //{
        //    mMainCamera.transform.localPosition = new Vector3(0, 8.0f, 13.0f);
        //    mMainCamera.transform.localEulerAngles = new Vector3(34f, 180f, 0);
        //    mMainCamera.transform.localScale = new Vector3(1, 1, 1);
        //    mMainCamera.fieldOfView = 36f;
        //}

        if (GlobalModule.IsSceneLoadComplete && !mIsInited)
        {
            mIsInited = true;
            __Init();
        }
        if (!GlobalModule.IsSceneLoadComplete)
            return;
        //加空值判断
        if (mbPress && Character.Self != null && !Character.Self.IsNeedMove() && !(Character.Self.mCurrentAI is AttackSkill))
            MoveCharatorer();

        //end

        if (mStage != null)
            mStage.Process(Time.deltaTime);


		#if UNITY_IPHONE
		#elif UNITY_ANDROID
		if(Application.platform == RuntimePlatform.Android && Input.GetKeyDown(KeyCode.Escape))
		{
			Settings.QuitGame();
		}
		if(Application.platform == RuntimePlatform.Android && Input.GetKeyDown(KeyCode.Home))
		{
			DataCenter.OpenMessageWindow("我是home....Update....ANDROID");
		}
		#else
		if(Application.platform == RuntimePlatform.Android && Input.GetKeyDown(KeyCode.Escape))
		{
			Settings.QuitGame();
		}
		if(Application.platform == RuntimePlatform.Android && Input.GetKeyDown(KeyCode.Home))
		{
			DataCenter.OpenMessageWindow("我是home....Update");
		}
		#endif
    }

   
	
	public void OnPress(bool bPress)
	{
        //mbPress = bPress;
        ////DEBUG.Log("Press > " + bPress.ToString());
        //if (!mbPress)
        //{
        //    MoveCharatorer();
        //    //Character.Self.FriendsToFollow();
        //}
        
        //UpdateMouseCoord();  
	}


    //bool mbDrag = false;

    public void OnPressing(bool bPress)
    {
        //mbPress = bPress;
        //DEBUG.Log("Press > " + bPress.ToString());
        //if (!mbPress && !(Character.Self.mCurrentAI is AttackSkill))
        //{
        //    MoveCharatorer();
        //    //Character.Self.FriendsToFollow();
        //}
        //
        //UpdateMouseCoord();
        if (bPress && Character.Self != null && Character.Self.SelectMonster() == null && !(Character.Self.mCurrentAI != null && Character.Self.mCurrentAI.HoldControl()))
        {
            MoveCharatorer();
            UpdateMouseCoord();
        }
    }

	public void OnSelect()
	{
	}   

    public void OnDragStart()
    {
        mbPress = true;
    }
    public void OnDragEnd()
    {
        mbPress = false;
    }
    public void OnDrag(Vector2 d)
    {
        MoveCharatorer();
        UpdateMouseCoord();
        //Character.Self.FriendsToFollow();
    }

	public void OnClick()
	{
        mbPress = false;
        //if (Character.Self != null && Character.Self.SelectMonster() == null && !(Character.Self.mCurrentAI != null && Character.Self.mCurrentAI.HoldControl()))
        //{
        //    MoveCharatorer();
        //    UpdateMouseCoord();
        //}

		#if UNITY_IPHONE
		#elif UNITY_ANDROID
		if(Application.platform == RuntimePlatform.Android && Input.GetKeyDown(KeyCode.Escape))
		{
			Settings.QuitGame();
		}
		if(Application.platform == RuntimePlatform.Android && Input.GetKeyDown(KeyCode.Home))
		{
			DataCenter.OpenMessageWindow("我是home....OnClick....ANDROID");
		}
		#else
		if(Application.platform == RuntimePlatform.Android && Input.GetKeyDown(KeyCode.Escape))
		{
			Settings.QuitGame();
		}
		if(Application.platform == RuntimePlatform.Android && Input.GetKeyDown(KeyCode.Home))
		{
			DataCenter.OpenMessageWindow("我是home....OnClick");
		}
		#endif
	}

	public void MoveCharatorer()
	{
        //if (Character.Self == null || mStage == null || mStage.mbBattleFinish || mLockCharacterPos || (Character.Self.mCurrentAI != null && Character.Self.mCurrentAI.HoldControl()))
		//	return;

        if (Character.Self == null || mStage == null || mStage.mbBattleFinish || mLockCharacterPos)
        {
            return;
        }

		RaycastHit rayInfo;

		int mask = 1<<CommonParam.ObstructLayer;
		mask = ~mask;        
		Ray ray = mMainCamera.ScreenPointToRay(Input.mousePosition);
		if (Physics.Raycast(ray, out rayInfo, 9999999, mask))
		{
			//mCameraObject.transform.position = rayInfo.point;
			//mMainCharatorer.MoveTo(rayInfo.point);

            UpdateMouseCoord(rayInfo.point);
            //AI_MainCharMove mto = Character.Self.StartCharMoveAI() as AI_MainCharMove; // ForceStartAI("AI_MoveTo") as AI_MoveTo;
            //if (mto != null)
            //{
            //    mto.mTargetPos = rayInfo.point;
            //    mto.DoEvent();
            //
            //    Character.Self.FriendsToFollow(rayInfo.point);
            //}

            if (mController != null)
            {
                mController.OnPressGroud(rayInfo.point);
            }
            //OnBattleCustomerActionObserver.Instance.Notify("ClickGroud", rayInfo.point);
            //Character.Self.FriendsToFollow(rayInfo.point);
		}

        //Character.Self.FriendsToFollow();
	}

    static public bool GetMouseHitPoint(out Vector3 hitPos)
    {

        RaycastHit rayInfo;        
        int mask = 1<<CommonParam.ObstructLayer;
        mask = ~mask;
        Ray ray = mMainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out rayInfo, 600, mask))
        {
            hitPos = rayInfo.point;
            return true;
        }
        hitPos = Vector3.zero;

        //NavMeshHit meshHit;
        //if (NavMesh.Raycast(ray.origin, ray.origin + ray.direction * 9000, out meshHit, -1))
        //{
        //    hitPos = meshHit.position;
        //    return true;
        //}
        //hitPos = Vector3.zero;

        return false;
    }

	static public void ClearBattle()
	{
        try
        {
            DEBUG.Log("Enter clear battle");
            //LuaBehaviour.ClearObjects(false);
            Notification.Stop();

            DEBUG.Log("Clear battle data");
            EventCenter.Self.mTimeEventManager.RemoveAll();
            EventCenter.Self.RemoveAllEvent();
            AutoBattleAI.InitReset();
            ObjectManager.Self.ClearAll();
            ClearPaoPao();
            Character.Self = null;

            DEBUG.Log("Clear battle change scene before");
            if (MainUI.Self != null)
                MainUI.Self.OnChangeSceneBefore();

            mStage = null;

            DEBUG.Log("MouseCoord finish");
            if (mMouseCoord != null)
            {
                mMouseCoord.Finish();
                mMouseCoord = null;
            }

            //Resources.UnloadUnusedAssets();
            //System.GC.Collect();

            DEBUG.Log("Clear battle load empty scene");
            GlobalModule.Instance.LoadScene("empty", false);
            DEBUG.Log("Clear battle notification start");
            Notification.Start();
            DEBUG.Log("Exit clear battle");

            NewFuncOpenWindow.SetOpenFlagInfo();
        }
        catch (Exception exp)
        {
            DEBUG.Log("ClearBattle exception = " + exp.Message + ", stack = " + exp.StackTrace);
        }
	}

    static public void ClearPaoPao()
    {
        //GameObject textPanel = GameCommon.FindUI("text_panel");
        //
        //if (textPanel != null)
        //{
        //    foreach (Transform child in textPanel.transform)
        //    {
        //        GameObject.Destroy(child.gameObject);
        //    }
        //}
        PaoPaoTextPool.Clear();
    }

    static public void RequestAppearBoss(eACTIVE_AFTER_APPEAR afterActive)
    {
        //TODO 请求天魔进入界面
        /*
        CS_RequestAppearBoss evt = Net.StartEvent("CS_RequestAppearBoss") as CS_RequestAppearBoss;
        evt.mCurrentStageIndex = DataCenter.Get("CURRENT_STAGE");
        evt.mNextActive = afterActive;
        evt.DoEvent();
         * */
    }

    static public void LoadRoleSelScene(bool isDestroy = true)
	{
//        ClearBattle();
		//GlobalModule.Instance.LoadScene("base_ui", false);

//		MainUIScript.Self.DestroyMainWindowByIndex();
        Time.timeScale = 1f;
        BaseUI.OpenWindow("MAIN_UI_WIDNOW", null, true, isDestroy);

//		if(MainUIScript.Self == null)
//			BaseUI.OpenWindow("MAIN_UI_WIDNOW", null, true);
//		else
//		{
//			DataCenter.OpenWindow("MAIN_UI_WIDNOW");
//			MainUIScript.Self.Start ();
//		}


//		DataCenter.CloseWindow("LANDING_WINDOW");
//		DataCenter.OpenWindow("MAIN_UI_WIDNOW");

        // check task
		//TaskSystemMgr.Self.CheckCanAcceptTasksByTaskType(Task_Accept_Condition.No_Condition);
        //TaskSystemMgr.Self.CheckCanAcceptTasksByTaskType(Task_Accept_Condition.Get_Designated_Pet);
        //TaskSystemMgr.Self.CheckCanAcceptTasksByTaskType(Task_Accept_Condition.Level);
        //TaskSystemMgr.Self.CheckCanAcceptTasksByTaskType(Task_Accept_Condition.Task);
        //DataCenter.SetData("LandingWindow", "CLOSE", true);
	}

	static public void LoadRoleSelScene(MAIN_WINDOW_INDEX openWindowIndex)
	{
//		CommonParam.OpenWindowIndex = openWindowIndex;
		MainUIScript.mCurIndex = openWindowIndex;
		LoadRoleSelScene(true);
	}

    //static public void OpenSelectLevelWindow()
    //{
    //	DataCenter.CloseWindow("BATTLE_END_WINDOW");
    //	ClearBattle();
    //	LoadRoleSelScene(MAIN_WINDOW_INDEX.SelectLevelWindow);
    //}

    static public void OpenUnionPkPrepareWindow()
    {
        ClearBattle();
        LoadRoleSelScene(MAIN_WINDOW_INDEX.UnionPkPrepareWindow);
    }

	static public void OpenWordMapWindow()
	{
		ClearBattle ();
		LoadRoleSelScene(MAIN_WINDOW_INDEX.WorldMapWindow);
	}

	static public void OpenBossRaidWindow()
	{
		ClearBattle ();
		LoadRoleSelScene(MAIN_WINDOW_INDEX.BossRaidWindow);
	}

	static public void OpenShopWindow()
	{
		ClearBattle ();
		LoadRoleSelScene(MAIN_WINDOW_INDEX.ShopWindow);		
	}

	static public void LoadLoginScene()
	{
//		GlobalModule.Instance.LoadScene("LandingUI", false);
		DataCenter.SetData("START_GAME_LOADING_WINDOW", "CONTINUE", true);
	}

	static public void LoadBattleLoadingScene(string strLoadingWinName)
	{
//		ObjectManager.Self.ClearAll();
		if(MainUIScript.Self != null)
			MainUIScript.Self.DestroyAllMainWindow();
		BaseUI.OpenWindow(strLoadingWinName, null, true, true);

//		GlobalModule.Instance.LoadScene("BattleLoading", false);

        //SetCheckTaskConditionsNeedData();
        //TaskSystemMgr.Self.CheckFinishAllTaskSubentryByTaskType(TASK_TYPE.PVE_Enter_Satge);
	}

	static public void LoadBattleLoadingScene()
	{
		LoadBattleLoadingScene("BATTLE_LOADING_WINDOW");
	}
	
	static public void LoadBossBattleLoadingScene()
	{
		LoadBattleLoadingScene("BOSS_BATTLE_LOADING_WINDOW");
	}

    static public void LoadGuildBossBattleLoadingScene()
    {
        LoadBattleLoadingScene("GUILD_BOSS_BATTLE_LOADING_WINDOW");
    }
	
	static void SetCheckTaskConditionsNeedData()
    {
        tLogicData taskNeedData = DataCenter.GetData("TASK_NEED_DATA");
		int iStateIndex = DataCenter.Get("CURRENT_STAGE");
		taskNeedData.set("MAP_ID", iStateIndex);
    }

	static public void LoadBattleScene()
	{
		ClearBattle();
        int stageIndex = DataCenter.Get("CURRENT_STAGE");
        if (stageIndex > 0)
        {
            DataRecord r = DataCenter.mStageTable.GetRecord(stageIndex);
            if (r != null)
            {
                int type = r.getData("TYPE");
                STAGE_TYPE stageType = (STAGE_TYPE)type;

                switch (stageType)
                {
                    case STAGE_TYPE.CHAOS:
                        mStage = new BossBattle();
                        break;

                    case STAGE_TYPE.GUILDBOSS:
                        mStage = new GuildBossBattle();
                        break;

                    case STAGE_TYPE.PVP4:
                        mStage = new PVP4Battle();
                        break;

                    //by chenliang
                    //begin

                    case STAGE_TYPE.TOWER:
                        mStage = new RammbockBattle();
                        break;
                    case STAGE_TYPE.GRAB:
                        mStage = new GrabBattle();
                        break;
                    case STAGE_TYPE.FAIRYLAND:
                        mStage = new FairylandBattle();
                        break;

                    //end

                    default:
                        mStage = new PVEStageBattle();
                        break;
                }

				mStage.InitConfig(stageIndex);

                string sceneName = r.getData("SCENE_NAME");
                if (sceneName != "")
                {
                    //GlobalModule.Instance.LoadScene("level_1", false);
                    GlobalModule.Instance.LoadScene(sceneName, false);
                    //GlobalModule.Instance.LoadScene("Text", true);
                    //GlobalModule.Instance.LoadScene("base_ui", true);
//                    GlobalModule.Instance.LoadScene("CommonUI", true);
                  
                    return;
                }
            }
			else
				DEBUG.LogError("Stage config no exist >"+stageIndex.ToString());
        }
                
        EventCenter.Log(LOG_LEVEL.ERROR, "now not set CURRENT_STAGE");              
        
	}
    //by chenliang
    //begin

    /// <summary>
    /// 异步加载战斗场景
    /// </summary>
    /// <param name="progressCallback">加载进度设置函数</param>
    /// <returns></returns>
    static public IEnumerator LoadBattleSceneAsync(Action<float> progressCallback)
    {
        DEBUG.Log("Start clear battle");
        ClearBattle();
        DEBUG.Log("Load battle return null");
        yield return null;
        DEBUG.Log("Select battle stage");
        int stageIndex = DataCenter.Get("CURRENT_STAGE");
        if (stageIndex > 0)
        {
            DataRecord r = DataCenter.mStageTable.GetRecord(stageIndex);
            if (r != null)
            {
                int type = r.getData("TYPE");
                STAGE_TYPE stageType = (STAGE_TYPE)type;

                switch (stageType)
                {
                    case STAGE_TYPE.CHAOS:
                        mStage = new BossBattle();
                        break;
                    case STAGE_TYPE.GUILDBOSS:
                        mStage = new GuildBossBattle();
                        break;
                    case STAGE_TYPE.PVP4:
                        mStage = new PVP4Battle();
                        break;
                    case STAGE_TYPE.TOWER:
                        mStage = new RammbockBattle();
                        break;
                    case STAGE_TYPE.GRAB:
                        mStage = new GrabBattle();
                        break;
                    case STAGE_TYPE.FAIRYLAND:
                        mStage = new FairylandBattle();
                        break;

                    default:
                        mStage = new PVEStageBattle();
                        break;
                }

                mStage.InitConfig(stageIndex);

                string sceneName = r.getData("SCENE_NAME");
                if (sceneName != "")
                {
//                    AsyncOperation tmpAsyncOP = GlobalModule.Instance.LoadSceneAsync(sceneName, false);
//                    while (!tmpAsyncOP.isDone)
//                    {
//                        yield return null;
//                        if (progressCallback != null)
//                            progressCallback(tmpAsyncOP.progress);
//                    }

                    DEBUG.Log("Start load battle scene by " + sceneName);

                    GlobalModule.LoadSceneProgress = progressCallback;
                    GlobalModule.LoadSceneAsyncOP = GlobalModule.Instance.LoadSceneAsync(sceneName, false);

//                    GlobalModule.Instance.LoadScene(sceneName, false);
//                    if (progressCallback != null)
//                        progressCallback(1.0f);
                }
            }
            else
                DEBUG.LogError("Stage config no exist >" + stageIndex.ToString());
        }
        EventCenter.Log(LOG_LEVEL.ERROR, "now not set CURRENT_STAGE");
    }

    //end

    static public bool RefreshCurrentStageProperty()
    {
        int stageIndex = DataCenter.Get("CURRENT_STAGE");

        if (stageIndex > 0)
        {
            StageProperty property = StageProperty.Create(stageIndex);
            if (property != null)
            {
                DataCenter.Set("CURRENT_STAGE_REQUEST", (int)property.GetRequestFightResult());
                DataCenter.Set("CURRENT_STAGE_COST", property.mFightCost);
                return true;
            }
        }
        return false;
    }

    static public bool RequestBattle(Action callback)
    {
        if (!RefreshCurrentStageProperty())
            return false;

        int fightCost = DataCenter.Get("CURRENT_STAGE_COST");
        int result = DataCenter.Get("CURRENT_STAGE_REQUEST");

        switch ((RequestBattleResult)result)
        {
            case RequestBattleResult.ACCEPT:
                callback();
                break;

            case RequestBattleResult.NO_CHANCE:
                DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_SELECT_LEVEL_NO_CHANCE);
                break;

            case RequestBattleResult.OVER_TIME:
                DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_SELECT_LEVEL_OVER_TIME);
                break;

            case RequestBattleResult.NEED_COST_FOR_COMMON:
                DataCenter.OpenMessageOkWindow(STRING_INDEX.ERROR_SELECT_LEVEL_NEED_DIAMOND_FOR_COMMON, fightCost.ToString(), callback);
                break;

            case RequestBattleResult.NEED_COST_FOR_EXTRA:
                DataCenter.OpenMessageOkWindow(STRING_INDEX.ERROR_SELECT_LEVEL_NEED_DIAMOND_FOR_EXTRA, fightCost.ToString(), callback);
                break;

            case RequestBattleResult.NO_ENOUGH_STAMINA:
                DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_SELECT_LEVEL_NO_ENOUGH_STAMINA);
                break;

            case RequestBattleResult.NO_ENOUGH_COST_FOR_COMMON:
                DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_SELECT_LEVEL_NO_ENOUGE_DIAMOND);
                break;

            case RequestBattleResult.NO_ENOUGH_COST_FOR_EXTRA:
                DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_SELECT_LEVEL_NO_ENOUGE_DIAMOND);
                break;
        }

        return true;
    }

    static public bool StartBattle(Action onRequestSucceed, bool skipLoading)
    {
        if (!RefreshCurrentStageProperty())
            return false;

        //EventListenerCenter.RegisterListener("ON_BATTLE_REQUEST_SUCCEED");
        ObserverCenter.Add("ON_BATTLE_REQUEST_SUCCEED", onRequestSucceed);
        int stageIndex = DataCenter.Get("CURRENT_STAGE");

		string friendID = "";
		string myName = RoleLogicData.Self.name;
		if(FriendLogicData.mSelectPlayerData != null) friendID = FriendLogicData.mSelectPlayerData.mID;
	
        int fightCost = DataCenter.Get("CURRENT_STAGE_COST");
        tEvent battleStart = Net.StartEvent("CS_BattleStart");
        battleStart.set("MAP_ID", stageIndex);
        battleStart.set("FRIEND_ID", friendID);
		battleStart.set("FRIEND_NAME", myName);
        battleStart.set("CURRENT_STAGE_COST", fightCost);
        battleStart.set("SKIP_LOADING", skipLoading ? 1 : 0);
        battleStart.DoEvent();
        return true;
    }

    public static void QuitBattle()
    {
        MainProcess.ClearBattle();
//        MainProcess.LoadRoleSelScene(MAIN_WINDOW_INDEX.RoleSelWindow);
    }

    public static bool OpenMainWindow(string winName)
    {
        switch (winName)
        {
//            case "all_pet_attribute_info_window":
//                Logic.EventCenter.Start("Button_change_team_button").DoEvent();
//                return true;
//
//            case "all_role_attribute_info_window":
//                Logic.EventCenter.Start("Button_switch_button").DoEvent();
//                return true;

            default:
                if (!string.IsNullOrEmpty(winName))
                {
                    foreach (KeyValuePair<MAIN_WINDOW_INDEX, string> pair in MainUIScript.Self.mDicMainWindowName)
                    {
                        if (pair.Value == winName)
                        {
							MainProcess.LoadRoleSelScene(pair.Key);
//                            MainUIScript.Self.OpenMainWindowByIndex(pair.Key);
                            return true;
                        }
                    }
                }
                break;
        }

        return false;
    }

    public static void RequestCleanStage()
    {
        RequestBattle(() =>
            {
				if(RoleLogicData.Self.mSweepNum <= 0)
				{
	                if (RoleLogicData.Self.diamond > 0)
	                    DataCenter.OpenMessageOkWindow(STRING_INDEX.ERROR_SAODANG_NEED_DIAMOND, "", () => StartCleanStage());
	                else
	                    DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_SAODANG_NO_ENOUGH_DIAMON);
				}
				else
				{
					GameCommon.RoleChangeFunctionalProp (ITEM_TYPE.SAODANG_POINT, -1);
					StartCleanStage();
				}
            });
    }

    public static void StartCleanStage()
    {
        int stage = DataCenter.Get("CURRENT_STAGE");

        if (stage > 0)
        {
            tEvent evt = Net.StartEvent("CS_RequestSaoDang");
            evt.set("MAP_ID", stage);
            evt.DoEvent();
        }
    }

    public static bool isOnCheating { get; set; }

    /// <summary>
    /// 当作弊验证不通过时调用的方法
    /// </summary>
    public static void OnCheatVerifyFailed()
    {
        if (!isOnCheating)
        {
            if (mStage != null)
            {
                mStage.DisableAI();
            }

            isOnCheating = true;
            //DataCenter.OpenMessageWindow("系统检测到数据异常，请重新登陆", HotUpdateLoading.ReloadHotUpdateScene);
            GameCommon.DataVerifyFailedHandle();
        }
    }

    // 获取战斗中队伍成员的死亡次数
    public static int GetTeamDeadCount()
    {
        if (mStage != null)
        {
            return mStage.mPetDeadCount;
        }

        return 0;
    }

    // 获取战斗时间（从AI激活起算，战斗结束后不再变动）
    public static float GetBattleTime()
    {
        if (mStage != null)
        {
            return mStage.mBattleTime;
        }

        return 0f;
    }

    // 获取队伍剩余HP比例（战斗结束后不再变动）
    public static float GetTeamHpRate()
    {
        if (mStage == null)
        {
            return 1f;
        }
        else if (!mStage.mbBattleFinish && Character.Self != null)
        {
            return Character.Self.GetTeamHpRate();
        }
        else 
        {
            return mStage.mTeamHpRateOnFinish;
        }
    }

    /// <summary>
    /// 添加战斗监视器
    /// </summary>
    /// <param name="monitor"> 待添加的战斗监视器 </param>
    /// <returns> 是否添加成功 </returns>
    public static bool AddBattleMonitor(IBattleMonitor monitor)
    {
        if (Self != null && !Self.mMonitorList.Contains(monitor))
        {
            Self.mMonitorList.Add(monitor);
            return true;
        }

        return false;
    }

    /// <summary>
    /// 移除战斗监视器
    /// </summary>
    /// <param name="monitor"> 待移除的战斗监视器 </param>
    /// <returns> 是否移除成功 </returns>

    public static bool RemoveBattleMonitor(IBattleMonitor monitor)
    {
        if (Self != null)
        {
            return Self.mMonitorList.Remove(monitor);
        }

        return false;
    }

    /// <summary>
    /// 战斗干预器
    /// </summary>
    public static IBattleIntervencer battleIntervencer
    {
        get
        {
            return Self == null ? null : Self.mIntervencer;
        }
        set 
        {
            if (Self != null)
            {
                Self.mIntervencer = value;
            }
        }
    }

    /// <summary>
    /// 当战斗激活时，此方法由关卡调用，不要在其他地方调用
    /// </summary>
    /// <param name="stage"> 激活的关卡 </param>
    public static void OnBattleActive()
    {
        if (Self != null)
        {
            if(Self.mIntervencer != null)
                Self.mIntervencer.OnBattleActive();

            Self.mMonitorList.ForEach(x => x.OnBattleActive());
        }
    }

    /// <summary>
    /// 当战斗结束时，此方法由关卡调用，不要在其他地方调用
    /// </summary>
    /// <param name="stage"> 结束的关卡 </param>
    public static void OnBattleFinish(bool win)
    {
        if (Self != null)
        {
            if (Self.mIntervencer != null)
                Self.mIntervencer.OnBattleFinish(win);

            Self.mMonitorList.ForEach(x => x.OnBattleFinish(win));
        }
    }

    /// <summary>
    /// 当战场对象启动时，此方法由关卡调用，不要在其他地方调用
    /// </summary>
    /// <param name="obj"> 启动的对象 </param>
    public static void OnObjectStart(BaseObject obj)
    {
        if (Self != null)
        {
            if (Self.mIntervencer != null)
                Self.mIntervencer.OnObjectStart(obj);

            Self.mMonitorList.ForEach(x => x.OnObjectStart(obj));
        }
    }

    /// <summary>
    /// 当战场对象死亡时，此方法由关卡调用，不要在其他地方调用
    /// </summary>
    /// <param name="obj"> 死亡的对象 </param>
    public static void OnObjectDead(BaseObject obj)
    {
        if (Self != null)
        {
            if (Self.mIntervencer != null)
                Self.mIntervencer.OnObjectDead(obj);

            Self.mMonitorList.ForEach(x => x.OnObjectDead(obj));
        }
    }


    /// <summary>
    /// 当释放技能时，此方法由关卡调用，不要在其他地方调用
    /// </summary>
    /// <param name="skill"> 释放的技能 </param>
    public static void OnDoSkill(Skill skill)
    {
        if (Self != null)
        {
            if (Self.mIntervencer != null)
                Self.mIntervencer.OnDoSKill(skill);

            Self.mMonitorList.ForEach(x => x.OnDoSKill(skill));
        }
    }


    /// <summary>
    /// 当技能造成伤害时，不包括附加buff，吸血，反伤等附加效果。此方法由关卡调用，不要在其他地方调用
    /// </summary>
    /// <param name="skill"> 技能</param>
    /// <param name="target"> 伤害目标 </param>
    /// <param name="damage"> 伤害值 </param>
    public static void OnSkillDamage(Skill skill, BaseObject target, int damageValue, SKILL_DAMAGE_FLAGS flags)
    {
        if (Self != null)
        {
            if (Self.mIntervencer != null)
                Self.mIntervencer.OnSkillDamage(skill, target, damageValue, flags);

            Self.mMonitorList.ForEach(x => x.OnSkillDamage(skill, target, damageValue, flags));
        }
    }
}


public class TM_CreateActiveObject : CEvent
{
	public Vector3 mBirthPos;
	public int mConfigIndex = 0;
	public int mOwner = 2000;

	public override void _OnOverTime()
	{

		if (mConfigIndex>0)
		{						
			BaseObject obj = ObjectManager.Self.CreateObject(mConfigIndex);
			if (obj!=null)
			{
				obj.SetCamp(mOwner);
				obj.InitPosition(mBirthPos);
				obj.Start ();
			}
		}
	}
}


public class Button_DoSkill : CEvent
{
    public override bool _DoEvent()
	{
		if (Character.Self==null || Character.Self.IsDead() /*|| EffectOnDoSkill.isActive*/ || MainProcess.mStage == null || !MainProcess.mStage.mbBattleActive || MainProcess.mStage.mbBattleFinish)
			return false;

		string name = get ("NAME");
        
        tLogicData skillData = DataCenter.GetData("SKILL_UI");
        
        if (skillData==null)
            return false;

        tLogicData d = skillData.getData(name);

        if (d == null)
            return false;

        d.set("ON_CLICK_BUTTON", true);
        return true;
        //if (Vector3.Distance(enemy.GetPosition(), Character.Self.GetPosition()) > Character.Self.SkillBound(skillIndex))
        //{
        //    AI_MoveToTargetThenSkill ai = Character.Self.StartAI("AI_MoveToTargetThenSkill") as AI_MoveToTargetThenSkill;
        //    ai.SetTarget(enemy);
        //    ai.mWaitSkillIndex = skillIndex;
        //    ai.mSkillData = d;
        //    ai.DoEvent();
        //    return true;
        //}

        //Skill s = Character.Self.DoSkill(skillIndex, enemy, d);
        //if (s != null)
        //{
        //    //d.set("START_CD", true);
        //    //DataCenter.SetData("SKILL_BUTTON", name, (float)s.GetConfig("CDTIME"));
        //}
    }
}

public class Button_Summon : CEvent
{
    public override bool _DoEvent()
    {
        if (Character.Self == null || Character.Self.IsDead() /*|| EffectOnDoSkill.isActive*/ || MainProcess.mStage == null || MainProcess.mStage.mbBattleFinish)
            return false;

        string name = get("NAME");
        int usePos = Convert.ToInt32(name.Substring(11, 1));

        tLogicData skillData = DataCenter.GetData("SKILL_UI");

        if (skillData == null)
            return false;

        tLogicData d = skillData.getData("do_skill_" + usePos);

        if (d == null)
            return false;

        d.set("SUMMON_PET", true);
        return true;
    }
}

public class Button_Again : CEvent
{
	public override bool _DoEvent()
	{
        //if (MainProcess.mStage.mbSucceed && StageProperty.IsCurrentMainStage())
        //{
        //    this.CloseOwnerWindow();
        //    MainProcess.RequestAppearBoss(eACTIVE_AFTER_APPEAR.AGAIN_LEVEL);
        //}
        //else
        //{
        //	if(FriendLogicData.mSelectPlayerData != null)  FriendLogicData.mSelectPlayerData = null;
        //
        //	//EventCenter.Start("Button_friend_help_start_button").DoEvent();
        //    MainProcess.RequestBattle(() => MainProcess.StartBattle(OnRequestSucceed, false));
        //}
        this.CloseOwnerWindow();
        MainProcess.OpenWordMapWindow();
        DataCenter.Set("IS_CURRENT", true);
		return true;
	}

    private void OnRequestSucceed()
    {
        this.CloseOwnerWindow();
        MainProcess.ClearBattle();
    }
}

public class Button_but_next : CEvent
{
	public override bool _DoEvent()
	{
        //if (MainProcess.mStage.mbSucceed && StageProperty.IsCurrentMainStage())
        //{
        //	MainProcess.RequestAppearBoss(eACTIVE_AFTER_APPEAR.NEXT_LEVEL);
        //}
        //else
        //{
        //      //      MainProcess.OpenWordMapWindow();
        //	MainProcess.QuitBattle ();
        //	MainProcess.LoadRoleSelScene (MAIN_WINDOW_INDEX.RoleSelWindow);
        //}
        this.CloseOwnerWindow();
        MainProcess.OpenWordMapWindow();
        DataCenter.Set("IS_NEXT", true);
		return true;
	}
}

public class Button_but_back_home_page : CEvent
{
	public override bool _DoEvent()
	{
		//if (MainProcess.mStage.mbSucceed && StageProperty.IsCurrentMainStage())
		//{
		//	MainProcess.RequestAppearBoss(eACTIVE_AFTER_APPEAR.BACK_HOME_PAGE);
		//}
		//else
		//{
		//	MainProcess.QuitBattle ();
		//	MainProcess.LoadRoleSelScene (MAIN_WINDOW_INDEX.RoleSelWindow);
		//}
        MainProcess.QuitBattle();
        MainProcess.LoadRoleSelScene(MAIN_WINDOW_INDEX.RoleSelWindow);

		return true;
	}
}

public class Button_world_map_button : CEvent
{
	public override bool _DoEvent()
	{
//		DataCenter.CloseWindow("BATTLE_END_WINDOW");
//		MainProcess.ClearBattle();
		
		MainProcess.OpenWordMapWindow();
		return true;
	}
}

public class Button_but_select : CEvent
{
    public override bool _DoEvent()
    {
        this.CloseOwnerWindow();

        //if (MainProcess.mStage.mbSucceed && StageProperty.IsCurrentMainStage())
        //{
        //	MainProcess.RequestAppearBoss(eACTIVE_AFTER_APPEAR.SELECT_LEVEL);
        //}
        //else if(StageProperty.IsCurrentActiveStage())
        //{
        //	MainProcess.OpenWordMapWindow();
        //	DataCenter.OpenWindow("ACTIVE_STAGE_WINDOW");
        //}
        //else
        //{
        //    MainProcess.OpenWordMapWindow();
        //}
        if (DataCenter.Get("IS_DAILYS_STAGE_BATTLE"))
        {
            DataCenter.Set("IS_DAILYS_STAGE_BATTLE", false);
            DataCenter.Set("IS_DAILY_STAGE_BACK", true);
            DataCenter.Set("IS_PVE_STAGE_BACK", false);
        }
        else
        {
            DataCenter.Set("IS_DAILY_STAGE_BACK", false);
            DataCenter.Set("IS_PVE_STAGE_BACK", MainProcess.mStage != null && MainProcess.mStage.mbSucceed);
        }
        
        MainProcess.OpenWordMapWindow();
        return true;
    }
}

public class Button_BattleBack : CEvent
{
	public override bool _DoEvent()
	{
        MainProcess.ClearBattle();
        MainProcess.LoadRoleSelScene();
		return true;
	}
}

public class Button_AgainAndExitCloseBtn : CEvent
{
	public override bool _DoEvent()
	{
		//Character.Self.LevelUp();
		//Character.Self.AddExp(10000);
		//??? DataCenter.SetData("BATTLE_UI", "VISIBLE", true);
		return true;
	}
}

public class Button_Exit : CEvent
{
    public override bool _DoEvent()
    {
        GameCommon.ApplicationQuit();
        return true;
    }
}

public class Button_Pause : CEvent
{
	public override bool _DoEvent()
	{
		//Character.Self.LevelUp();
		//Character.Self.AddExp(10000);
        //??? DataCenter.SetData("BATTLE_UI", "VISIBLE", true);
		return true;
	}
}

