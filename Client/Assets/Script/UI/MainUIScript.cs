using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Logic;
using DataTable;
using System.Linq;
using Utilities;
using System;

public enum MAIN_WINDOW_INDEX
{
	None_Window = -1,
    RoleSelWindow,
    PVEBattleReadyWindow,
    PVPBattleReadyWindow,
    AllRoleAttributeInfoWindow,
	AllPetAttributeInfoWindow,
    //SelectLevelWindow,
	MissionWindow,
	FriendWindow,
	FriendVisitWindow,
	MailWindow,
    ShopWindow,
	BossRaidWindow,
//	InviteCodeWindow,
	WorldMapWindow,
    BoardWindow,
	PVPRankWindow,
	PVPAttributeWindow,
	PVPFourVsFour,
	UnionWindow,
    RankWindow,
	ActiveListWindow,

    RammbockWindow,     //群魔乱舞
	ActivityWindow,		//活动
    UnionPkPrepareWindow,     //宗门副本
}

public class MainUIScript : MonoBehaviour {

    public Dictionary<MAIN_WINDOW_INDEX, string> mDicMainWindowName = new Dictionary<MAIN_WINDOW_INDEX, string>();
    public Dictionary<MAIN_WINDOW_INDEX, GameObject> mDicMainWindowObj = new Dictionary<MAIN_WINDOW_INDEX, GameObject>();

	const string strPath = "Prefabs/UI/";
	public static MainUIScript Self;

    public GameObject mPVEBattleReadyWindow;
    public GameObject mPVPBattleReadyWindow;
    public GameObject mSelectLevelWindow;

    static public MAIN_WINDOW_INDEX mCurIndex = MAIN_WINDOW_INDEX.RoleSelWindow;
    static public MAIN_WINDOW_INDEX mLastIndex = MAIN_WINDOW_INDEX.RoleSelWindow;

	public UILabel mStaminaLabel;
	public UILabel mGoldNumLabel;
	public UILabel mDiamondNumLabel;

	public GameObject mIllustratedMark;
	public GameObject mFriendMark;
	public GameObject mCharacterMark;
	public GameObject mPetMark;
	public GameObject mNoticeMark;
	public GameObject mMailMark;
	public GameObject mTaskMark;
	
	public UILabel mRoleNameLabel;	

	
	[HideInInspector]
	public float roate_Speed = 100.0f;//旋转速度

	[HideInInspector]
	public string mStrAllRoleAttInfoPageWindowName = "ROLE_INFO_WINDOW";
	[HideInInspector]
	public string mStrShopPageWindowName = "";
    [HideInInspector]
    public string mStrWorldMapSubWindowName = "";
	
	public Action mWindowBackAction = null;
    public static Action mLoadingFinishAction = null;
    public static bool mbAlwaysOpenMenu = false;

    //added by xuke
    public Action mShopBackAction = null;
    //end
	GameObject mCamera;

	static public int mHaveBossCount = 0;

	// Awake this instance.
	void Awake () 
	{
		Self = this;
	}

	// Use this for initialization
	public void InitStart ()
	{
		Init();

		if(mCurIndex != MAIN_WINDOW_INDEX.RoleSelWindow)
		{
			
            //by chenliang
            //begin

// 			OpenRoleSelWindow();
// 			
// 			
// 			DataCenter.SetData("ROLE_SEL_BOTTOM_GROUP", "UPDATE_ROLE_SELECT", true);
//--------------
            OpenRoleSelWindow(null);
            DataCenter.SetData("ROLE_SEL_BOTTOM_GROUP", "UPDATE_ROLE_SELECT", true);

            //end
		}
		else
		{
            //by chenliang
            //begin

//			OpenMainWindowByIndex(mCurIndex);
//----------------
            GlobalModule.DoCoroutine(OpenMainWindowByIndexAsync(mCurIndex));

            //end
		}

		InitWindow();

		// TODO
//		InitEquipData();
//		InitRoleData();
	}

	public void InitWindow()
	{
        //DataCenter.OpenWindow("BAG_INFO_WINDOW");
        //DataCenter.SetData("BAG_INFO_WINDOW", "INIT_WINDOW", true);

        //DataCenter.OpenWindow("PET_INFO_SINGLE_WINDOW", PET_INFO_WINDOW_TYPE.PET);
        //DataCenter.SetData("PET_INFO_SINGLE_WINDOW", "INIT_WINDOW", true);
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void Init () {
		InitRoleSel();
		InitMain();

		InitData();
	}
	
	public void InitData()
	{
		mCamera = transform.parent.parent.gameObject;
	}


	void InitRoleSel() 
	{
        //DataCenter.OpenWindow("ROLE_SEL_BOTTOM_GROUP");
        //DataCenter.OpenWindow("ROLE_SEL_BOTTOM_LEFT_GROUP");
        //DataCenter.OpenWindow("ROLE_SEL_BOTTOM_RIGHT_GROUP");
        //DataCenter.OpenWindow("ROLE_SEL_TOP_GROUP");
        //DataCenter.OpenWindow("ROLE_SEL_TOP_RIGHT_GROUP");
        //DataCenter.OpenWindow("ROLE_SEL_TOP_LEFT_GROUP");
        //DataCenter.OpenWindow("ROLE_SEL_CENTER_GROUP");

        //DataCenter.SetData("ROLE_SEL_TOP_LEFT_GROUP", "UPDATE_ROLE_NAME", true);

        //by chenliang
        //begin

        //临时进入斗法二级界面窗口
        DataCenter.OpenWindow("ROLE_TMP_PVP_ENTER_GROUP");

        //end

        DataCenter.OpenWindow("PVP_RECORD_SHOW_WINDOW");
	}

	void InitMain()
	{
		mDicMainWindowName.Clear();
        //mDicMainWindowName.Add(MAIN_WINDOW_INDEX.SelectLevelWindow, "select_levels_window");
        mDicMainWindowName.Add(MAIN_WINDOW_INDEX.PVEBattleReadyWindow, "PVE_battle_ready_window");
		mDicMainWindowName.Add(MAIN_WINDOW_INDEX.PVPBattleReadyWindow, "PVP_battle_ready_window");
		mDicMainWindowName.Add(MAIN_WINDOW_INDEX.AllRoleAttributeInfoWindow, "all_role_attribute_info_window");
		mDicMainWindowName.Add(MAIN_WINDOW_INDEX.AllPetAttributeInfoWindow, "all_pet_attribute_info_window");
		mDicMainWindowName.Add(MAIN_WINDOW_INDEX.MissionWindow, "mission_window");
		mDicMainWindowName.Add(MAIN_WINDOW_INDEX.FriendWindow, "friend_window");
		mDicMainWindowName.Add(MAIN_WINDOW_INDEX.FriendVisitWindow, "friend_visit_window");
		mDicMainWindowName.Add(MAIN_WINDOW_INDEX.MailWindow, "mail_window");
        mDicMainWindowName.Add(MAIN_WINDOW_INDEX.ShopWindow, "shop_window");
        mDicMainWindowName.Add(MAIN_WINDOW_INDEX.BossRaidWindow, "boss_raid_window");
		mDicMainWindowName.Add(MAIN_WINDOW_INDEX.WorldMapWindow, "scroll_world_map_window");
        mDicMainWindowName.Add(MAIN_WINDOW_INDEX.BoardWindow, "board_window");
		mDicMainWindowName.Add(MAIN_WINDOW_INDEX.UnionWindow, "union_window");
        mDicMainWindowName.Add(MAIN_WINDOW_INDEX.RankWindow, "rank_window");
		mDicMainWindowName.Add(MAIN_WINDOW_INDEX.ActiveListWindow, "active_list_window");

        mDicMainWindowName.Add(MAIN_WINDOW_INDEX.RammbockWindow, "rammbock_window");        //群魔乱舞
		mDicMainWindowName.Add(MAIN_WINDOW_INDEX.ActivityWindow, "activity_window");        //活动
        mDicMainWindowName.Add(MAIN_WINDOW_INDEX.UnionPkPrepareWindow, "union_pk_prepare_window");                //宗门副本

		DataCenter.OpenWindow("MAIN_CENTER_GROUP");
		DataCenter.OpenWindow("INFO_GROUP_WINDOW");
//		DataCenter.SetData("INFO_GROUP_WINDOW", "REFRESH", true);
//      DataCenter.SetData("INFO_GROUP_WINDOW", "UPDATE_VITAKITY", true);
//      DataCenter.SetData("INFO_GROUP_WINDOW", "UPDATE_GOLD", true);
//       
	}

	public void CheckIsNeedOpenOtherWindow()
	{
		if(CommonParam.OpenWindowIndex != MAIN_WINDOW_INDEX.None_Window)
		{
			MainUIScript.Self.OpenMainWindowByIndex(CommonParam.OpenWindowIndex);
			CommonParam.OpenWindowIndex = MAIN_WINDOW_INDEX.None_Window;
		}
	}

    public void LoadMainWindowPrefabs(MAIN_WINDOW_INDEX mainWindowIndex)
	{
		string strName = "";
        mDicMainWindowName.TryGetValue(mainWindowIndex, out strName);

        //宗门副本打开
        if (mainWindowIndex == MAIN_WINDOW_INDEX.UnionPkPrepareWindow)
        {
            DataCenter.OpenWindow(UIWindowString.union_pk_prepare_window);
            return;
        }
		GameObject obj = GameCommon.LoadAndIntanciateUIPrefabs( strName, "main_center_group");

		if(obj != null)
		{
            if (mDicMainWindowObj.ContainsKey(mainWindowIndex))
                mDicMainWindowObj[mainWindowIndex] = obj;
            else
                mDicMainWindowObj.Add(mainWindowIndex, obj);            
		}
		else
		{
			if(mainWindowIndex == MAIN_WINDOW_INDEX.PVPRankWindow)
			{
				BaseUI.OpenWindow("PVP_SEARCH_OPPONENT", null, true);
//				Logic.EventCenter.Start("Button_speel_pk_button").DoEvent();
			}
			else if(mainWindowIndex == MAIN_WINDOW_INDEX.PVPAttributeWindow)
			{
				BaseUI.OpenWindow("PVP_ATTR_READY_WINDOW", null, true);
			}
			else if(mainWindowIndex == MAIN_WINDOW_INDEX.PVPFourVsFour)
			{
				BaseUI.OpenWindow("PVP_FOUR_VS_FOUR_READY_WINDOW", null, true);
			}
			Logic.EventCenter.Log(LOG_LEVEL.WARN, "this prefab is not exist>"+strName);
		}
	}

	void RenderRefresh(GameObject obj)
	{

		if(obj != null)
		{
			UIPanel uiPanel = obj.GetComponent<UIPanel>();
			if(uiPanel != null)
				uiPanel.Refresh();

			foreach(Transform trans in obj.transform)
			{
				if(trans.gameObject != null)
				{
					RenderRefresh(trans.gameObject);
				}
			}

		}

	}
	void OnDestroy()
	{
//		DataCenter.Remove("RoleSelWindow");
	}

	public void GoBack()
	{
        OpenMainWindowByIndex(mLastIndex);
	}
    public void OpenMainWindowByIndex(MAIN_WINDOW_INDEX index)
	{
		mLastIndex = mCurIndex;
		//GlobalModule.Instance.LoadScene("empty", false);
        if (MAIN_WINDOW_INDEX.RoleSelWindow == index)
		{
//			BaseUI.OpenWindow("MAIN_UI_WIDNOW", null, true);

			//by chenliang

//            OpenRoleSelWindow();
//            //added by xuke
//            DataCenter.OpenWindow("INFO_GROUP_WINDOW");
//            //end
//            DataCenter.SetData("ROLE_SEL_BOTTOM_GROUP", "UPDATE_ROLE_SELECT", true);
//
//            GameObject Mainmenu_bg = GameObject.Find("Mainmenu_bg");
//
//            NetManager.RequestGetNotification();
//------------------
            OpenRoleSelWindow(() =>
            {
                GameObject Mainmenu_bg = GameObject.Find("Mainmenu_bg");

                GlobalModule.DoCoroutine(GetNotification_Requester.StartRequester());
            });
            DataCenter.OpenWindow("INFO_GROUP_WINDOW");
            DataCenter.SetData("ROLE_SEL_BOTTOM_GROUP", "UPDATE_ROLE_SELECT", true);
            //GameObject obj = GameCommon.FindObject(Mainmenu_bg, "ec_ui_tianmo");//GameObject.Find("UI Root").transform.Find("ec_ui_tianmo").gameObject;
            //if (obj != null)
            //{
            //    obj.SetActive(GameCommon.FunctionIsUnlock(UNLOCK_FUNCTION_TYPE.BOSS_RAIN) && SystemStateManager.GetNotifState(SYSTEM_STATE.NOTIF_BOSS));
            //}
            //end

            // 设置播放背景音乐
            GameCommon.SetBackgroundSound("Sound/Opening", 0.7f);

		}
		else
		{
//			DateTime happenTime = DateTime.Now;
//			DEBUG.LogError(".......................");
//			GlobalModule.Instance.LoadScene("empty", false);
//			DEBUG.LogError(" Total use [" + (DateTime.Now - happenTime).Milliseconds.ToString() + "]milsecond");
            //by chenliang
            //begin

// 			OpenMainUI();
//             OpenWindowByIndex(index);
//--------------
            GlobalModule.DoCoroutine(__OpenMainWindowByIndexAddonAsync(index));

            //end
		}
		mCurIndex = index;
	}
    //by chenliang
    //begin

    public IEnumerator OpenMainWindowByIndexAsync(MAIN_WINDOW_INDEX index)
    {
        mLastIndex = mCurIndex;
        if (MAIN_WINDOW_INDEX.RoleSelWindow == index)
        {
            // 设置播放背景音乐
            GameCommon.SetBackgroundSound("Sound/Opening", 0.7f);
            DataCenter.OpenWindow("INFO_GROUP_WINDOW");
            DataCenter.SetData("ROLE_SEL_BOTTOM_GROUP", "UPDATE_ROLE_SELECT", true);
            yield return null;
            OpenRoleSelWindow(() =>
            {
                GameObject Mainmenu_bg = GameObject.Find("Mainmenu_bg");

                GlobalModule.DoCoroutine(GetNotification_Requester.StartRequester());
            });
        }
        else
            GlobalModule.DoCoroutine(__OpenMainWindowByIndexAddonAsync(index));
        mCurIndex = index;
    }

    /// <summary>
    /// 将打开主场景作为异步
    /// </summary>
    /// <returns></returns>
    private IEnumerator __OpenMainWindowByIndexAddonAsync(MAIN_WINDOW_INDEX index)
    {
        OpenMainUI();
        OpenWindowByIndex(index);
        yield return null;
    }

    //end

    

    public void OpenRoleSelWindow()
	{
		DataCenter.CloseWindow("MAIN_CENTER_GROUP");
		DataCenter.CloseWindow("BAG_INFO_WINDOW");
		DataCenter.OpenWindow("ROLE_SEL_CENTER_GROUP");
		DataCenter.OpenWindow("ROLE_SEL_BOTTOM_GROUP");
		DataCenter.OpenWindow("ROLE_SEL_BOTTOM_LEFT_GROUP");
		DataCenter.OpenWindow("ROLE_SEL_BOTTOM_RIGHT_GROUP");
		DataCenter.OpenWindow("ROLE_SEL_TOP_GROUP");
		DataCenter.OpenWindow("ROLE_SEL_TOP_RIGHT_GROUP");
		DataCenter.OpenWindow("ROLE_SEL_TOP_LEFT_GROUP");

        DataCenter.OpenWindow("ROLE_TMP_PVP_ENTER_GROUP");
		DataCenter.OpenWindow("PVP_RECORD_SHOW_WINDOW");

        if (mbAlwaysOpenMenu)
        {
            GameObject closeBtn = GameCommon.FindUI("close_menu_button");

            if (closeBtn.activeSelf)
            {
                EventCenter.Start("Button_close_menu_button").DoEvent();
            }
        }

        DestroyAllMainWindow();
	}
    //by chenliang
    //begin

    /// <summary>
    /// 异步操作
    /// </summary>
    /// <returns></returns>
    public void OpenRoleSelWindow(Action openSuccessCallback)
    {
        GlobalModule.DoCoroutine(__OpenRoleSelWindow(openSuccessCallback));
    }
    private IEnumerator __OpenRoleSelWindow(Action openSuccessCallback)
    {
        DataCenter.CloseWindow("MAIN_CENTER_GROUP");
        DataCenter.CloseWindow("BAG_INFO_WINDOW");
        DataCenter.OpenWindow("ROLE_SEL_CENTER_GROUP");
        DataCenter.OpenWindow("ROLE_SEL_BOTTOM_GROUP");
        yield return null;
        DestroyAllMainWindow();
        DataCenter.OpenWindow("ROLE_SEL_BOTTOM_LEFT_GROUP");
        yield return null;
        DataCenter.OpenWindow("ROLE_SEL_BOTTOM_RIGHT_GROUP");
        DataCenter.OpenWindow("ROLE_SEL_TOP_GROUP");
        DataCenter.OpenWindow("ROLE_SEL_TOP_RIGHT_GROUP");
        yield return null;
        DataCenter.OpenWindow("ROLE_SEL_TOP_LEFT_GROUP");

        yield return null;
        DataCenter.OpenWindow("ROLE_TMP_PVP_ENTER_GROUP");
        yield return null;
        DataCenter.OpenWindow("PVP_RECORD_SHOW_WINDOW");

        if (mbAlwaysOpenMenu)
        {
            GameObject closeBtn = GameCommon.FindUI("close_menu_button");

            if (closeBtn.activeSelf)
            {
                EventCenter.Start("Button_close_menu_button").DoEvent();
            }
        }

        if (openSuccessCallback != null)
            openSuccessCallback();
    }

    //end
	public void OpenMainUI()
	{
		DataCenter.OpenWindow("MAIN_CENTER_GROUP");
		
		DataCenter.CloseWindow("ROLE_SEL_BOTTOM_GROUP");
		DataCenter.CloseWindow("ROLE_SEL_BOTTOM_LEFT_GROUP");
		DataCenter.CloseWindow("ROLE_SEL_BOTTOM_RIGHT_GROUP");
		DataCenter.CloseWindow("ROLE_SEL_CENTER_GROUP");
		DataCenter.CloseWindow("ROLE_SEL_TOP_GROUP");
		DataCenter.CloseWindow("ROLE_SEL_TOP_RIGHT_GROUP");
		DataCenter.CloseWindow("ROLE_SEL_TOP_LEFT_GROUP");

		DataCenter.CloseWindow("BAG_INFO_WINDOW");

        DataCenter.CloseWindow("ROLE_TMP_PVP_ENTER_GROUP");
		DataCenter.CloseWindow("PVP_RECORD_SHOW_WINDOW");
        DataCenter.CloseWindow("FAIRYLAND_WINDOW");
		// pvp
		DataCenter.CloseWindow("PVP_SELECT_ENTER");
		DataCenter.CloseWindow("PVP_SIX_ENTER_WINDOW");
		DataCenter.CloseWindow("PVP_ATTR_ENTER_WINDOW");

		DataCenter.SetData("ROLE_SEL_BOTTOM_GROUP", "CLEAR_GRID", true);
	}
	

    public void OpenWindowByIndex(MAIN_WINDOW_INDEX mainWindowIndex)
	{
        DestroyMainWindowByIndex();
        LoadMainWindowPrefabs(mainWindowIndex);
	}

	//added by xuke
	public void HideMainBGUI ()
	{
		OpenMainUI ();
	}

	public void ShowMainBGUI()
	{
		OpenMainWindowByIndex (MAIN_WINDOW_INDEX.RoleSelWindow);
	}
	//end

    public void DestroyAllMainWindow()
	{
        Dictionary<MAIN_WINDOW_INDEX, GameObject>.ValueCollection objValues = mDicMainWindowObj.Values;
		foreach(GameObject obj in objValues)
		{
			Destroy(obj);
		}
        mDicMainWindowObj.Clear();
	}

    public void DestroyMainWindowByIndex()
	{
		GameObject obj = null;
        mDicMainWindowObj.TryGetValue(mLastIndex, out obj);
		if(obj != null)
		{
			Destroy(obj);

            mDicMainWindowObj.Remove(mLastIndex);
            if (mDicMainWindowObj.Count <= 0)
			{
                mDicMainWindowObj.Clear();
			}
		}
	}
}

public class RoleSelWindow : tWindow
{
}

public class MainRootWindow : tWindow
{
	public override void Open (object param)
	{
		base.Open (param);

        //by chenliang
        //begin

// 		MainUIScript.Self.InitStart();
//         Notification.RefuseNone();
//-------------
        MainUIScript.Self.InitStart();
        //延迟一帧，分散窗口打开时压力
         GlobalModule.DoOnNextUpdate(() =>
         {
//            MainUIScript.Self.InitStart();
            Notification.RefuseNone();
        });

        //end
	}
}

public class MainWindow : tWindow
{
}

public class ActiveObjData: tLogicData
{
	static public List<BaseObject>   mActiveObjectList = new List<BaseObject>();
	
	public override void onChange(string keyIndex, object objVal)
	{
		
	}
	static public void InitReset()
	{
		mActiveObjectList.Clear();
	}
	
	static public void AddObj(BaseObject obj)
	{
		mActiveObjectList.Add(obj);
	}
}

public class RoleSelUI_PlayIdleEvent : CEvent
{
	public override void CallBack(object caller) 
	{
		BaseObject obj = caller as BaseObject;
		if (obj != null)
			obj.ResetIdle();
	}
}

public class PetSelUI_PlayAttackEvent : CEvent
{
	public override void CallBack(object caller) 
	{
		BaseObject obj = caller as BaseObject;
		if (obj != null)
		{
			int iIndex = get ("INDEX");
			if(iIndex >=1 && iIndex <= 2)
			{
				set ("INDEX", iIndex + 1);
				obj.PlayMotion("attack_" + iIndex.ToString(), this);
			}
			else if(iIndex >= 3)
			{
				obj.ResetIdle();
			}
		}
	}
}

public class RoleSelBottomWindow : tWindow
{
	public GameObject[] mRolePointList;
	public GameObject[] mInfoList;
	public GameObject[] mRoleAndPets;
	public GameObject mCardGroup;
	public UIGridContainer mCardGrid;
	public int miCurIndex = 0;

	GameObject mCamera;
	string usrName;
	[HideInInspector]
	public float mfScaleRatio = 1.0f;

	public void UpdateRoleSel()
	{
        //by chenliang
        //begin

//		InitActiveBirthForUI();
//--------------
        GlobalModule.DoCoroutine(InitActiveBirthForUIAsync());

        //end
	}

	public void UpdateFriendRoleSel(SC_ResponeVisitPlayer visitEvent)
	{
		InitFriendActiveBirthForUI(visitEvent);
	}

	public void UpdateScene()
	{
		int iIndex = UnityEngine.Random.Range((int)ELEMENT_TYPE.RED, (int)ELEMENT_TYPE.MAX);
		// TODO
		iIndex = 4;

		if(!GameCommon.bIsLogicDataExist("MAIN_ROLE_SCENE_WINDOW"))
		{
			DataCenter.OpenWindow("MAIN_SCENE_LOADING_WINDOW");
//			GlobalModule.Instance.LoadScene("Mainmenu_bg_" + iIndex.ToString(), false);
		}
		else
		{
			InitRoleSel(iIndex);
		}
	}

	public override void onChange(string keyIndex, object objVal)
	{
        base.onChange(keyIndex, objVal);
		if (keyIndex == "UPDATE_ROLE_SELECT") {
			DataCenter.SetData ("ROLE_SEL_BOTTOM_GROUP", "CLEAR_GRID", true);
			UpdateScene ();
		} else if (keyIndex == "INIT_ROLE_SELECT") {
			InitRoleSel ((int)objVal);
		} else if (keyIndex == "INIT_FRIEND_ROLE_SELECT") {
			InitFriendRoleSel ((SC_ResponeVisitPlayer)objVal);

		} else if (keyIndex == "CLEAR_GRID") {
			if (mGameObjUI != null) {
				mCardGrid = mGameObjUI.transform.Find ("card_grid").GetComponent<UIGridContainer> ();
				mCardGrid.MaxCount = 0;
			}
		} else if (keyIndex == "UPDATE_MARK") {
			UpdateMarks ();
		} 
	}

	public void InitRoleSel(int iIndex)
	{
		miCurIndex = iIndex;
		InitData();
		SetTrans();
		UpdateRoleSel();
        //UpdateMarks();
	}



    private void UpdateMarks()
    {
        DataCenter.SetData("ROLE_SEL_TOP_LEFT_GROUP", "UPDATE_ROLE_SELECT_SCENE", true);
        DataCenter.SetData("ROLE_SEL_TOP_RIGHT_GROUP", "UPDATE_ROLE_SELECT_SCENE", true);
        DataCenter.SetData("ROLE_SEL_BOTTOM_LEFT_GROUP", "UPDATE_ROLE_SELECT_SCENE", true);
        DataCenter.SetData("ROLE_SEL_BOTTOM_RIGHT_GROUP", "UPDATE_ROLE_SELECT_SCENE", true);
        //		DataCenter.SetData("INFO_GROUP_WINDOW", "UPDATE_ROLE_SELECT_SCENE", true);
        GameCommon.SetWindowData("ROLE_SEL_TOP_LEFT_GROUP", "UPDATE_MAIL_MARK", true);

		//DataCenter.SetData("ROLE_SEL_TOP_LEFT_GROUP", "UPDATE_FRIEND_MARK", true);

        GameObject parentObj = GameObject.Find("Mainmenu_bg");
        if (null == parentObj)
            return;
        GameObject obj = GameCommon.FindObject(parentObj, "ec_ui_tianmo");
        GameObject putongObj = GameCommon.FindObject(parentObj, "ec_mainputong");
        if (obj != null && putongObj != null)
        {
            obj.SetActive(SystemStateManager.GetNotifState(SYSTEM_STATE.NOTIF_BOSS));
            putongObj.SetActive(!SystemStateManager.GetNotifState(SYSTEM_STATE.NOTIF_BOSS));
        }
    }

	public void InitFriendRoleSel(SC_ResponeVisitPlayer visitEvent)
	{
		InitData();
		SetTrans();
		UpdateFriendRoleSel(visitEvent);

		DataCenter.OpenWindow("ROLE_SEL_BOTTOM_GROUP");
	}

	public void SetTrans()
	{
		GameObject bgScene = GameObject.Find("Mainmenu_bg");
		Transform trans = bgScene.transform.Find("Camera/group");

		mCardGroup.transform.parent = trans;
		mCardGroup.transform.localPosition = Vector3.zero;
		mCardGroup.transform.localRotation = Quaternion.identity;
		mCardGroup.transform.localScale = Vector3.one;

//		mCardGroup.transform.

//		cameraTrans
//
//		mCamera.transform.position = trans.position;
//		mCamera.transform.rotation = trans.rotation;
	}

	public void InitData()
	{
		if(mGameObjUI != null)
		{
			mCamera = mGameObjUI.transform.parent.parent.gameObject;
			mCardGrid = mGameObjUI.transform.Find("card_grid").GetComponent<UIGridContainer>();
            mCardGrid.fastResize = false;
			mCardGrid.MaxCount = 1;
			mCardGroup = mCardGrid.controlList[0];
			
			int iMaxNum = 4;
			mRoleAndPets = new GameObject[iMaxNum];
			for(int i = 0; i < iMaxNum; i++)
			{
				GameObject card = mCardGroup.transform.Find("card" + i.ToString()).gameObject;
				mRoleAndPets[i] = card;
			}
			
			mRolePointList = new GameObject[iMaxNum];
			for(int i = 0; i < iMaxNum; i++)
			{
				GameObject point = mRoleAndPets[i].transform.Find("UIPoint").gameObject;
				mRolePointList[i] = point;
			}

			mInfoList = new GameObject[iMaxNum];
			for(int i = 0; i < iMaxNum; i++)
			{
				GameObject info = mGameObjUI.transform.Find("info" + i.ToString()).gameObject;
				mInfoList[i] = info;
                //by chenliang
                //begin

//                初始化时先隐藏
                info.SetActive(false);

                //end
			}
		}
	}

	public void InitActiveBirthForUI()
	{
		ActiveObjData.InitReset();

//		ObjectManager.Self.ClearAll();
		foreach (GameObject obj in mRolePointList)
		{
			ActiveBirthForUI activeBirthForUI = obj.GetComponent<ActiveBirthForUI>();
			if (activeBirthForUI != null)
			{
				if (activeBirthForUI.mIndex == (int)TEAM_POS.CHARACTER)
				{
					RoleLogicData logicData = RoleLogicData.Self;
					activeBirthForUI.mBirthConfigIndex = RoleLogicData.GetMainRole().tid;
					mfScaleRatio = 1.0f;
					
					SetInfoUI(activeBirthForUI, RoleLogicData.GetMainRole().level, false);
					
					GameObject infoObj = mInfoList[activeBirthForUI.mIndex];
					if(infoObj != null)
					{
						UISprite roleTitle = infoObj.transform.Find("background/role_title").GetComponent<UISprite>(); 
						GameCommon.SetRoleTitle(roleTitle);
					}
				}
				else  if (activeBirthForUI.mIndex == 4)
				{
					//					// 随机玩家符灵
					//					
					//					// TODO: 临时
					
					//					int iModelIndex = UnityEngine.Random.Range(195000, 195020 + 1);
					//
					//					activeBirthForUI.mBirthConfigIndex = iModelIndex;
					//					mfScaleRatio = 1.2f;
					//
					//					SetInfoUI(activeBirthForUI, UnityEngine.Random.Range(1, 30), true);
				}
				else
				{
                    // 符灵
                    PetData petData = TeamManager.GetPetDataByTeamPos(activeBirthForUI.mIndex);
                    if (petData != null)
                    {
                        activeBirthForUI.mBirthConfigIndex = petData.tid;
                    }
                    else
                    {
                        activeBirthForUI.mBirthConfigIndex = -1;
                    }

                    if (activeBirthForUI.mIndex == (int)TEAM_POS.PET_1)
                    {
                        mfScaleRatio = 1.0f;
                    }
                    else if (activeBirthForUI.mIndex == (int)TEAM_POS.PET_2)
                    {
                        mfScaleRatio = 1.0f;
                    }
                    else if (activeBirthForUI.mIndex == (int)TEAM_POS.PET_3)
                    {
                        mfScaleRatio = 1.0f;
                    }

                    int iLevel = 0;
                    if (petData != null)
                        iLevel = petData.level;

                  	SetInfoUI(activeBirthForUI, iLevel, true);

                    // effect
                    if (petData == null)
                        continue;

                    //GameObject ec_ui_mainfour_group = GameCommon.FindObject(mCardGroup, "ec_ui_mainfour_group" + activeBirthForUI.mIndex.ToString());
                    //GameObject ec_ui_mainfive_group = GameCommon.FindObject(mCardGroup, "ec_ui_mainfive_group" + activeBirthForUI.mIndex.ToString());
                    //ec_ui_mainfour_group.SetActive(petData.starLevel == 4);
                    //ec_ui_mainfive_group.SetActive(petData.starLevel == 5);

                    //int iElementIndex = TableCommon.GetNumberFromActiveCongfig(petData.tid, "ELEMENT_INDEX");
                    //for (int i = (int)ELEMENT_TYPE.RED; i < (int)ELEMENT_TYPE.MAX; i++)
                    //{
                    //    GameObject four_huan = GameCommon.FindObject(ec_ui_mainfour_group, "four_huan_" + i.ToString());
                    //    GameObject five_huan = GameCommon.FindObject(ec_ui_mainfive_group, "five_huan_" + i.ToString());
                    //    four_huan.SetActive(iElementIndex == i);
                    //    five_huan.SetActive(iElementIndex == i);
                    //}
                    
				}
				activeBirthForUI.Init(true, mfScaleRatio, true);
			}
		}
	}
    //by chenliang
    //begin

    public IEnumerator InitActiveBirthForUIAsync()
    {
        ActiveObjData.InitReset();

        foreach (GameObject obj in mRolePointList)
        {
            yield return null;
            if (obj == null)
                continue;
            ActiveBirthForUI activeBirthForUI = obj.GetComponent<ActiveBirthForUI>();
            if (activeBirthForUI != null)
            {
                if (activeBirthForUI.mIndex == (int)TEAM_POS.CHARACTER)
                {
                    RoleLogicData logicData = RoleLogicData.Self;
                    activeBirthForUI.mBirthConfigIndex = RoleLogicData.GetMainRole().tid;
                    mfScaleRatio = 1.0f;

                    SetInfoUI(activeBirthForUI, RoleLogicData.GetMainRole().level, false);

                    GameObject infoObj = mInfoList[activeBirthForUI.mIndex];
                    if (infoObj != null)
                    {
                        UISprite roleTitle = infoObj.transform.Find("background/role_title").GetComponent<UISprite>();
                        GameCommon.SetRoleTitle(roleTitle);
                    }
                }
                else if (activeBirthForUI.mIndex == 4)
                {
                    //					// 随机玩家符灵
                    //					
                    //					// TODO: 临时

                    //					int iModelIndex = UnityEngine.Random.Range(195000, 195020 + 1);
                    //
                    //					activeBirthForUI.mBirthConfigIndex = iModelIndex;
                    //					mfScaleRatio = 1.2f;
                    //
                    //					SetInfoUI(activeBirthForUI, UnityEngine.Random.Range(1, 30), true);
                }
                else
                {
                    // 符灵
                    PetData petData = TeamManager.GetPetDataByTeamPos(activeBirthForUI.mIndex);
                    if (petData != null)
                    {
                        activeBirthForUI.mBirthConfigIndex = petData.tid;
                    }
                    else
                    {
                        activeBirthForUI.mBirthConfigIndex = -1;
                    }

                    if (activeBirthForUI.mIndex == (int)TEAM_POS.PET_1)
                    {
                        mfScaleRatio = 1.0f;
                    }
                    else if (activeBirthForUI.mIndex == (int)TEAM_POS.PET_2)
                    {
                        mfScaleRatio = 1.0f;
                    }
                    else if (activeBirthForUI.mIndex == (int)TEAM_POS.PET_3)
                    {
                        mfScaleRatio = 1.0f;
                    }

                    int iLevel = 0;
                    if (petData != null)
                        iLevel = petData.level;

                    SetInfoUI(activeBirthForUI, iLevel, true);

                    // effect
                    if (petData == null)
                        continue;

                    //GameObject ec_ui_mainfour_group = GameCommon.FindObject(mCardGroup, "ec_ui_mainfour_group" + activeBirthForUI.mIndex.ToString());
                    //GameObject ec_ui_mainfive_group = GameCommon.FindObject(mCardGroup, "ec_ui_mainfive_group" + activeBirthForUI.mIndex.ToString());
                    //ec_ui_mainfour_group.SetActive(petData.starLevel == 4);
                    //ec_ui_mainfive_group.SetActive(petData.starLevel == 5);

                    //int iElementIndex = TableCommon.GetNumberFromActiveCongfig(petData.tid, "ELEMENT_INDEX");
                    //for (int i = (int)ELEMENT_TYPE.RED; i < (int)ELEMENT_TYPE.MAX; i++)
                    //{
                    //    GameObject four_huan = GameCommon.FindObject(ec_ui_mainfour_group, "four_huan_" + i.ToString());
                    //    GameObject five_huan = GameCommon.FindObject(ec_ui_mainfive_group, "five_huan_" + i.ToString());
                    //    four_huan.SetActive(iElementIndex == i);
                    //    five_huan.SetActive(iElementIndex == i);
                    //}

                }
                activeBirthForUI.Init(true, mfScaleRatio, true);
            }
        }
    }

    //end

	public void InitFriendActiveBirthForUI(SC_ResponeVisitPlayer visitEvent)
	{
		ActiveObjData.InitReset();
		// 符灵
		DataRecord[] petList = new DataRecord[3];
		/*
		object petObj;
		visitEvent.getData ("PET_DATA", out petObj);
		NiceTable petTable = petObj as NiceTable;
		int i = 0;
		for(i = 0; i < 3; i++)
		{
			object obj;
			visitEvent.getData ("PET_" + (i + 1).ToString (), out obj);
			int DBID = (int)obj;
			if(DBID != 0)
				petList[i] = petTable.GetAllRecord ()[DBID];
		}
		*/
		int iMaxNum = 4;
		for (int i = 0; i < iMaxNum; i++) 
		{
			if(mInfoList[i] != null)
			{
				GameObject infoObj = mInfoList[i];
				infoObj.SetActive(false);
			}
		}
		foreach (GameObject obj in mRolePointList)
		{
			ActiveBirthForUI activeBirthForUI = obj.GetComponent<ActiveBirthForUI>();
			if (activeBirthForUI != null)
			{
				if (activeBirthForUI.mIndex == 0)
				{
					/*
					object friendObj;
					visitEvent.getData ("ROLE_DATA", out friendObj);
					NiceData friendData = friendObj as NiceData;
					*/
					// int model = TableCommon.GetNumberFromActiveCongfig(visitEvent.arr[0].tid, "MODEL");
					activeBirthForUI.mBirthConfigIndex = visitEvent.arr[0].tid; //friendData.get ("CHAR_MODEL");
					mfScaleRatio = 1.0f;

					usrName = visitEvent.name;
					//SetInfoUI(activeBirthForUI, visitEvent.arr[0].level, false);
					SetFriendInfoUI(activeBirthForUI, visitEvent, false);

					GameObject infoObj = mInfoList[activeBirthForUI.mIndex];
					if(infoObj != null)
					{
						UISprite roleTitle = infoObj.transform.Find("background/role_title").GetComponent<UISprite>(); 
						GameCommon.SetRoleTitle(roleTitle);
					}
				}
				else  if (activeBirthForUI.mIndex == 4)
				{
					//					// 随机玩家符灵
					//					
					//					// TODO: 临时
					
					//					int iModelIndex = UnityEngine.Random.Range(195000, 195020 + 1);
					//
					//					activeBirthForUI.mBirthConfigIndex = iModelIndex;
					//					mfScaleRatio = 1.2f;
					//
					//					SetInfoUI(activeBirthForUI, UnityEngine.Random.Range(1, 30), true);
				}
				else
				{
					int iLevel = 0;
					// break foreach loop
					if(activeBirthForUI.mIndex > visitEvent.arr.Length - 1) break;

					VisitPlayerStruct petData = visitEvent.arr[activeBirthForUI.mIndex];
					if(petData == null)
					{
						activeBirthForUI.mBirthConfigIndex = -1;
					}
					else
					{
						activeBirthForUI.mBirthConfigIndex = petData.tid;
						iLevel = petData.level;
					}
					
					if (activeBirthForUI.mIndex == 1)
					{
						mfScaleRatio = 1.0f;
					}
					else if (activeBirthForUI.mIndex == 2)
					{
						mfScaleRatio = 1.0f;
					}
					else if (activeBirthForUI.mIndex == 3)
					{
						mfScaleRatio = 1.0f;
					}
					
					//SetInfoUI(activeBirthForUI, iLevel, true);
					SetFriendInfoUI(activeBirthForUI, visitEvent, true);
				}
				activeBirthForUI.Init(true, mfScaleRatio, true);
			}
		}
	}

	void SetTipsPaoPao(GameObject infoObj)
	{
        if (DataCenter.Get("NO_PAOPAO_MAINUI") == false)
        {
            DataCenter.Set("NO_PAOPAO_MAINUI", true);
            return;
        }
		GameObject RoleTipsObj = GameCommon.FindObject (mGameObjUI, "role_label_tips").gameObject;
		string str = TableCommon.GetStringFromMainUiNotice(1, "NOTICE");
		int iMaxRate = TableCommon.GetNumberFromMainUiNotice(1, "RATE");
		int iMinRate = 0;
		for (int i = 0; i < 5; i++)
		{
			string tempStrLists = TableCommon.GetStringFromMainUiNotice(i + 1, "CONDITION");
			List<TipsInfoBase> tmpList = GameCommon.ParseTipsList(tempStrLists);
			if(GameCommon.SetIsCountion(tmpList))
			{
				int iTipsNum = UnityEngine.Random.Range(0,101);
				if(iTipsNum <= iMaxRate && iTipsNum >= iMinRate)
				{
					str = TableCommon.GetStringFromMainUiNotice(i + 1, "NOTICE");
					CloneRoleTips(infoObj, RoleTipsObj, str);
					break;
				}else 
				{
					if(i < 5 - 1)
					{
						iMaxRate = iMaxRate + TableCommon.GetNumberFromMainUiNotice(i + 2, "RATE");
						iMinRate = iMinRate + TableCommon.GetNumberFromMainUiNotice(i + 1, "RATE");
					}else 
						break;
				}
			}else
				break;
		}
	}
	void CloneRoleTips(GameObject infoObj,GameObject RoleTipsObj, string str)
	{
		GameCommon.FindComponent<UILabel>(RoleTipsObj, "role_tips_label").text = str;
		GameObject obj = GameObject.Instantiate (RoleTipsObj) as GameObject;
		obj.transform.parent = infoObj.transform;
		int roleType = GameCommon.GetRoleType(RoleLogicData.Self.character.tid);
		obj.transform.localPosition = new Vector3(76f, 219f,0f);
		if(roleType == 1)
		{
			obj.transform.localPosition = new Vector3(76f, 243f,0f);
		}
		obj.transform.localScale = Vector3.one;
		obj.SetActive (true);
		GlobalModule.DoLater (() => GameObject.DestroyImmediate (obj), 2.0f);
	}
	public void SetInfoUI(ActiveBirthForUI activeBirthForUI, int iLevel, bool bIsVisible)
	{
		GameObject infoObj = mInfoList[activeBirthForUI.mIndex];
		infoObj.SetActive(false);
		if(iLevel != 0)
		{
			if(infoObj.name == "info0")
				GlobalModule.DoLater ( () => SetTipsPaoPao(infoObj), 0.5f);

			infoObj.SetActive(true);
			UILabel levelLabel = infoObj.transform.Find("level_group/background/level_label").GetComponent<UILabel>();
			UILabel nameLabel = infoObj.transform.Find("name_label").GetComponent<UILabel>();
			UILabel breakLevelLabel = infoObj.transform.Find ("break_level_label").GetComponent<UILabel>();
			UIGridContainer grid = infoObj.transform.Find("stars_grid").GetComponent<UIGridContainer>();
			
			levelLabel.text = iLevel.ToString();

			int tid = activeBirthForUI.mBirthConfigIndex / 100;
			if(activeBirthForUI.mIndex == 110) // split
				nameLabel.text = usrName;//TableCommon.GetStringFromActiveCongfig(activeBirthForUI.mBirthConfigIndex, "NAME");
			else if(tid == 500 || tid == 501)
				nameLabel.text = TableCommon.GetStringFromActiveCongfig(activeBirthForUI.mBirthConfigIndex, "NAME");
			else 
//				nameLabel.text = TableCommon.GetStringFromActiveCongfig(activeBirthForUI.mBirthConfigIndex, "NAME");
				nameLabel.text = GameCommon.GetItemName (activeBirthForUI.mBirthConfigIndex);

            nameLabel.color = GameCommon.GetNameColor(activeBirthForUI.mBirthConfigIndex);
            nameLabel.effectColor = GameCommon.GetNameEffectColor();
            breakLevelLabel.color = nameLabel.color;
            breakLevelLabel.effectColor = nameLabel.effectColor;
            if (TeamManager.GetActiveDataByTeamPos(activeBirthForUI.mIndex).breakLevel > 0)
            {
                GameCommon.FindObject(infoObj, "break_level_label").SetActive(true);
                breakLevelLabel.text = "+" + TeamManager.GetActiveDataByTeamPos(activeBirthForUI.mIndex).breakLevel.ToString();
            }
            else
            {
                GameCommon.FindObject(infoObj, "break_level_label").SetActive(false);
            }

			int iStarLevel = TableCommon.GetNumberFromActiveCongfig(activeBirthForUI.mBirthConfigIndex, "STAR_LEVEL");
			if(bIsVisible)
			{
				grid.MaxCount = iStarLevel;
			}
			else
				grid.MaxCount = 0;
			
			float fDX = 10.0f;
			grid.transform.localPosition = new Vector3(0 + fDX*(6 - iStarLevel), grid.transform.localPosition.y, grid.transform.localPosition.z);
		}
	}

	private void SetFriendInfoUI(ActiveBirthForUI activeBirthForUI,SC_ResponeVisitPlayer playerInfo, bool bIsVisible)
	{
		GameObject infoObj = mInfoList[activeBirthForUI.mIndex];
		infoObj.SetActive(false);

		DEBUG.Log("MainUIScript-SetFriendInfoUI");
		if(playerInfo.arr[0].level != 0)
		{
			infoObj.SetActive(true);
			UILabel levelLabel = infoObj.transform.Find("level_group/background/level_label").GetComponent<UILabel>();
			UILabel nameLabel = infoObj.transform.Find("name_label").GetComponent<UILabel>();
			UIGridContainer grid = infoObj.transform.Find("stars_grid").GetComponent<UIGridContainer>();
            GameCommon.FindObject(infoObj, "break_level_label").SetActive(false);
			if(activeBirthForUI.mIndex < playerInfo.arr.Length)
			{
//				levelLabel.text = playerInfo.arr[0].level.ToString();
				levelLabel.text = playerInfo.arr[activeBirthForUI.mIndex].level.ToString();
				if(activeBirthForUI.mIndex == 0) // split
					nameLabel.text = playerInfo.name;//TableCommon.GetStringFromActiveCongfig(activeBirthForUI.mBirthConfigIndex, "NAME");
				else 
					//				nameLabel.text = TableCommon.GetStringFromActiveCongfig(activeBirthForUI.mBirthConfigIndex, "NAME");
					nameLabel.text = GameCommon.GetItemName (playerInfo.arr[activeBirthForUI.mIndex].tid);
				
				nameLabel.color = GameCommon.GetNameColor(activeBirthForUI.mBirthConfigIndex);
				nameLabel.effectColor = GameCommon.GetNameEffectColor();

				int iStarLevel = TableCommon.GetNumberFromActiveCongfig(activeBirthForUI.mBirthConfigIndex, "STAR_LEVEL");
				if(bIsVisible)
				{
					grid.MaxCount = iStarLevel;
				}
				else
					grid.MaxCount = 0;
				
				float fDX = 10.0f;
				grid.transform.localPosition = new Vector3(0 + fDX*(6 - iStarLevel), grid.transform.localPosition.y, grid.transform.localPosition.z);
			}
			else
			{
				nameLabel.transform.parent.gameObject.SetActive(false);
			}
	
		}
	}

}



public class RoleSelBottomLeftWindow : tWindow
{
    static GameObject rollPlayingGO; // 系统走马灯
    static GameObject rollPlayingGOPlayer; // 玩家信息走马灯
    static float disEachFrame=2;
    static int faultTolerantTime=65;/*容错时间*/
    vp_Timer.Handle timeHandle=new vp_Timer.Handle();

    
    List<RollPlayingPlayer> playerList=new List<RollPlayingPlayer>();

    public class RollPlayingObject {
        public readonly string content;
        public readonly int playInterval;
        public readonly int playCount;
        public readonly Int64 beginTime;
    }
    class CS_GetRollPlaying:GameServerMessage {}
    class SC_GetRollPlaying:RespMessage {
        public readonly RollPlayingObject[] arr;
    }

    public class RollPlayingPlayer {
        public readonly string content;
        public readonly Int64 playTime;

        public bool canPlay {
            get { 
                var curTime=CommonParam.NowServerTime();
                return playTime>=curTime-faultTolerantTime
                    &&playTime<=curTime+faultTolerantTime;
            }
        }
        
        public RollPlayingPlayer(string content,Int64 playTime) {
            this.content=content;
            this.playTime=playTime;
        }

        public IEnumerator DoSystemRollPlaying() {
            var contentTF=GameCommon.FindObject(rollPlayingGO,"content").transform;
            var label=contentTF.GetComponent<UILabel>();
            label.text=content;
            contentTF.localPosition=new Vector2(350+label.width,0);
            while(contentTF.localPosition.x>-350) {
                contentTF.localPosition=new Vector2(contentTF.localPosition.x-disEachFrame,0);
                yield return null;
            }
        }

        public IEnumerator DoPlayerRollPlaying()
        {
            rollPlayingGOPlayer.SetActive(true);
            var contentTF = GameCommon.FindObject(rollPlayingGOPlayer, "content").transform;
            if (contentTF == null)
            {
                DEBUG.LogError("Can't find rollPlayingGOPlayer's content");
                yield break;
            }

            var label = contentTF.GetComponent<UILabel>();
            label.text = "";

            //播放TweenScale开始动画
            TweenScale ts = GameCommon.FindComponent<TweenScale>(rollPlayingGOPlayer, "BG");
            if (ts != null) 
            {
                ts.PlayForward();
                ts.ResetToBeginning();
                ts.enabled = true;
                yield return new WaitForSeconds(1.0f);
            }
            
            label.text = content;
            contentTF.localPosition = new Vector2(350 + label.width, 0);
            //by chenliang
            //begin

//             while (contentTF.localPosition.x > -350)
//             {
//                 contentTF.localPosition = new Vector2(contentTF.localPosition.x - disEachFrame, 0);
//                 yield return null;
//             }
//             rollPlayingGOPlayer.SetActive(false);
//----------------------
            //增加异步判空
            while (rollPlayingGOPlayer != null && rollPlayingGOPlayer.activeSelf &&
                contentTF != null && contentTF.localPosition.x > -350)
            {
                contentTF.localPosition = new Vector2(contentTF.localPosition.x - disEachFrame, 0);
                yield return null;
            }

            //播放TweenScale结束动画
            if (rollPlayingGOPlayer != null)
            {
                ts.PlayReverse();
                yield return new WaitForSeconds(1.0f);
                rollPlayingGOPlayer.SetActive(false);
                //ts.onFinished.Add(new EventDelegate(() => { rollPlayingGOPlayer.SetActive(false); }));
            }
            //end
        }
    }

    void GetRollPlayingObjectFromServer() {
        HttpModule.CallBack requestSuccess=text => {
            var item=JCode.Decode<SC_GetRollPlaying>(text);
            var arr=item.arr;
            playerList.Clear();
            arr.Foreach(ro => {
                for (int i = 0; i < ro.playCount; i++)
                {
                    //DEBUG.LogError(ro.content);
                    playerList.Add(new RollPlayingPlayer(ro.content, ro.beginTime + ro.playInterval * i));
                }
            });

            playerList.Sort((lPlayer,rPlayer) => {
                if(lPlayer.playTime<rPlayer.playTime) return -1;
                else if(lPlayer.playTime>rPlayer.playTime) return 1;
                else return 0;
            });

            vp_Timer.In(10,() => {
                if(mGameObjUI==null) {
                    timeHandle.Cancel();
                    return;
                }
                if (list != null)
                {
                    foreach (RollPlayingPlayer rpp in list)
                    {
                        GlobalModule.Instance.StopCoroutine(rpp.DoSystemRollPlaying());
                    }
                }
                //mGameObjUI.StopAllCoroutines();
                if(arr.Length != 0)
                    mGameObjUI.StartCoroutine(DoAllSystemRollPlaying());
            },
            9999,120,timeHandle);
        };
        HttpModule.Instace.SendGameServerMessageT(new CS_GetRollPlaying(),requestSuccess,NetManager.RequestFail);
    }
    ICollection<RollPlayingPlayer> list = new List<RollPlayingPlayer>(); // 保留协程的引用用于StopCoroutine
    IEnumerator DoAllSystemRollPlaying() {
        rollPlayingGO.SetActive(true);

        // 播放Tween动画之前先将UILabel置空
        var contentTF = GameCommon.FindObject(rollPlayingGOPlayer, "content").transform;
        if (contentTF == null)
        {
            DEBUG.LogError("Can't find rollPlayingGOPlayer's content");
            yield break;
        }
        var label = contentTF.GetComponent<UILabel>();
        label.text = "";

        //播放TweenScale开始动画
        TweenScale ts = GameCommon.FindComponent<TweenScale>(rollPlayingGO, "BG");
        if (ts != null)
        {
            ts.PlayForward();
            ts.ResetToBeginning();
            ts.enabled = true;
            yield return new WaitForSeconds(1.0f);
        }

        //ICollection<RollPlayingPlayer> list=new List<RollPlayingPlayer>();
        for(int i=0;i<playerList.Count;i++) {
            var curPlayer=playerList[i];
            if(curPlayer.canPlay) {
                yield return mGameObjUI.StartCoroutine(curPlayer.DoSystemRollPlaying());
                list.Add(curPlayer);
            }        
        }
        list.Foreach(x => playerList.Remove(x));

        //播放TweenScale结束动画
        if (rollPlayingGO != null)
        {
            ts.PlayReverse();
            yield return new WaitForSeconds(1.0f);
        }
        rollPlayingGO.SetActive(false);
    }


    public GameObject mIllustratedMark;
    public GameObject mFriendMark;
    public GameObject mTaskMark;
    public GameObject mCharacterMark;
    public GameObject mPetMark;


    protected override void OpenInit() {
        base.OpenInit();
        rollPlayingGO=GetCurUIGameObject("rollPlayingGO");
        if (rollPlayingGO == null) return;

        rollPlayingGOPlayer = GetCurUIGameObject("rollPlayingGOPlayer");
        if (rollPlayingGOPlayer == null) return;

        //by chenliang
        //begin

//         GetCurUIGameObject("SendMailBtn").SetActive(!CommonParam.isCloseMail);
//         GetCurUIGameObject("SelectVIPBtn").SetActive(!CommonParam.isCloseMail);
//---------------
//        发送邮件按钮显示开关
        GameObject tmpBtnSendMail = GetCurUIGameObject("SendMailBtn");
        if (tmpBtnSendMail != null)
            tmpBtnSendMail.SetActive(!CommonParam.isCloseMail);
//        设置VIP等级按钮显示开关
        GameObject tmpBtnSelectVIP = GetCurUIGameObject("SelectVIPBtn");
        if (tmpBtnSelectVIP != null)
            tmpBtnSelectVIP.SetActive(!CommonParam.isCloseMail);
//        发送宗门经验按钮显示开关
        GameObject tmpSentGuildExpNumBtn = GetCurUIGameObject("SentGuildExpNumBtn");
        if (tmpSentGuildExpNumBtn != null)
            tmpSentGuildExpNumBtn.SetActive(!CommonParam.isCloseMail);

        //end
    }


	public override void OnOpen ()
	{
		InitButtonIsUnLockOrNot();
		base.OnOpen ();
        if(timeHandle!=null) timeHandle.Cancel();
        if(rollPlayingGO!=null) rollPlayingGO.SetActive(false);
        if (rollPlayingGOPlayer != null) rollPlayingGOPlayer.SetActive(false);
        GetRollPlayingObjectFromServer();
	}

    public override void OnClose() {
        base.OnClose();
        if(timeHandle!=null) timeHandle.Cancel();
        if(rollPlayingGO!=null) rollPlayingGO.SetActive(false);
        if (rollPlayingGOPlayer != null) rollPlayingGOPlayer.SetActive(false);
    }

	void InitButtonIsUnLockOrNot()
	{
		//GameCommon.ButtonEnbleButCanClick (mGameObjUI, "FriendBtn", UNLOCK_FUNCTION_TYPE.ADD_FRIEND);
		GameCommon.ButtonEnbleButCanClick (mGameObjUI, "UnionBtn", UNLOCK_FUNCTION_TYPE.UNION);
	}

	public void UpdateRoleSelScene()
	{
		CheckIllustratedMark();
		CheckFriendMark();
		CheckTask ();
		CheckCharacterMark();   //> 检测队伍界面按钮红点
		CheckPetMark();
        CheckEquipBagMark();    //> 检测装备背包按钮红点
        CheckRecoverMark();     //> 检测回收按钮红点
        CheckAstrologyMark();   //> 检测点星按钮红点
        CheckUnionBtnMark();    //> 检测宗门按钮红点
        CheckTrial();          //> 检测历练
        //CheckConsumeMark();     //> 检测包裹按钮红点
    }

    private void CheckConsumeMark()
    {
        int roleLevel = RoleLogicData.Self.character.level;
        bool _canShow = false;
        GameObject consumeBtn = GameCommon.FindObject(mGameObjUI, "PackageConsumeBtn");
        List<ConsumeItemData> consumeList = new List<ConsumeItemData>(ConsumeItemLogicData.Self.mDicConsumeItemData.Values);
        for (int i = 0; i < consumeList.Count; i++)
        {
            DataRecord r = DataCenter.mConsumeConfig.GetRecord(consumeList[i].tid);
            if (r != null && r["USELEVEL"] != 0 && roleLevel > r["USELEVEL"])
            {
                //显示红点
                _canShow = true;
                GameCommon.SetNewMarkVisible(consumeBtn, _canShow, "NewMark");
                return;
            }
        }
        GameCommon.SetNewMarkVisible(consumeBtn, _canShow, "NewMark");
    }

    private void CheckTrial() 
    {
        TrialNewMarkManager.Self.CheckTrial_NewMark();
    }
    /// <summary>
    /// 检测点星
    /// </summary>
    private void CheckAstrologyMark() 
    {
        if (mGameObjUI == null)
            return;
        bool _canShow = false;
        GameObject _pointStarBtnObj = GameCommon.FindObject(mGameObjUI, "PointStarBtn");
        ConsumeItemLogicData _consumeItemLogicData = DataCenter.GetData("CONSUME_ITEM_DATA") as ConsumeItemLogicData;
        if (_consumeItemLogicData == null)
            return;
        if (PointStarLogicData.Self == null)
            return;
        if (DataCenter.mPointStarConfig == null)
            return;
        ConsumeItemData _consumeItemData = _consumeItemLogicData.GetDataByTid((int)ITEM_TYPE.POINT_STAR_TONE);
        int _needStone = TableCommon.GetNumberFromPointStarConfig(PointStarLogicData.Self.mCurIndex, "COST");
        int _maxStarCount = DataCenter.mPointStarConfig.GetRecordCount();

        if (GameCommon.IsFuncCanUse(1200) && _consumeItemData.itemNum >= _needStone && PointStarLogicData.Self.mCurIndex <= _maxStarCount)
            _canShow = true;
     
        GameCommon.SetNewMarkVisible(_pointStarBtnObj, _canShow);
    }

	public void CheckIllustratedMark()
	{
		if(mGameObjUI == null)
			return;

        mIllustratedMark = mGameObjUI.transform.Find("move_menu_left_bottom/IllustratedHandbookBtn/NewMark").gameObject;
        GameObject awardMark = mGameObjUI.transform.Find("move_menu_left_bottom/IllustratedHandbookBtn/state_award_sprite").gameObject;
        mIllustratedMark.SetActive(!RoleLogicData.Self.mbHaveAwardTuJian && RoleLogicData.Self.mbHaveNewTuJian);
        awardMark.SetActive(RoleLogicData.Self.mbHaveAwardTuJian);
		return;

		bool bIsTip = false;
		TujianLogicData logic = DataCenter.GetData("TUJIAN_DATA") as TujianLogicData;
		if(logic != null)
		{
			for (int iElementType = 0; iElementType < 5; iElementType++)
			{
				if (iElementType >= (int)ELEMENT_TYPE.RED && iElementType <= (int)ELEMENT_TYPE.SHADOW)
				{
					foreach (KeyValuePair<int, DataRecord> pair in DataCenter.mActiveConfigTable.GetAllRecord())
					{
						if ((int)pair.Value.get("INDEX") > 0 
						    && (string)pair.Value.get("CLASS") == "PET" 
						    && (int)pair.Value.get("ELEMENT_INDEX") == iElementType)
						{
							// new num
							int iModelIndex = pair.Key;
							TujianData tujianData = logic.GetTujianDataByModelIndex(iModelIndex);
							if(tujianData != null)
							{
								if(tujianData.mStatus == (int)TUJIAN_STATUS.TUJIAN_NEW
								   ||tujianData.mStatus == (int)TUJIAN_STATUS.TUJIAN_REWARD)
								{

									bIsTip = true;
									return;
								}
							}
						}
					}
				}
			}
			
			mIllustratedMark.SetActive(bIsTip);
		}
	}
	
	public void CheckFriendMark()
	{
		if(mGameObjUI == null)
			return;

		mFriendMark = mGameObjUI.transform.Find("move_menu_left_bottom/FriendBtn/NewMark").gameObject;
		if(RoleLogicData.Self.mInviteNum != 0 && GameCommon.FunctionIsUnlock (UNLOCK_FUNCTION_TYPE.ADD_FRIEND) || SystemStateManager.GetNotifState(SYSTEM_STATE.NOTIF_FRIEND_SPIRIT))
			mFriendMark.SetActive (true);
		else 
			mFriendMark.SetActive (false);
	}

	public void CheckTask()
	{
		if(mGameObjUI == null)
			return;
		
		mTaskMark = mGameObjUI.transform.Find("move_menu_left_bottom/TaskBtn/NewMark").gameObject;
        TaskLogicData taskLogicData = TaskLogicData.Instance;
		if (taskLogicData == null)
		{
			mTaskMark.SetActive(false);
		}
		else
		{
			mTaskMark.SetActive(taskLogicData.CheckWhetherHadFinishTask());
		}
	}

	public void CheckCharacterMark()
	{
		if(mGameObjUI == null)
			return;

		//mCharacterMark = mGameObjUI.transform.Find("move_menu_left_bottom/CharacterBtn/NewMark").gameObject;
        //added by xuke
        GlobalModule.DoLater(() => 
        {
            GameObject mCharBtnObj = mGameObjUI.transform.Find("move_menu_left_bottom/CharacterBtn").gameObject;
            GameCommon.SetNewMarkVisible(mCharBtnObj, TeamManager.CheckTeamHasNewMark());
        },0);
        //end
	}
    /// <summary>
    /// 检测装备红点
    /// </summary>
    private void CheckEquipBagMark() 
    {
        if (mGameObjUI == null)
            return;
        GlobalModule.DoLater(() => 
        {
            GameObject mEquipBagBtnObj = GameCommon.FindObject(mGameObjUI, "PackageEquipBtn");
            GameCommon.SetNewMarkVisible(mEquipBagBtnObj, EquipBagNewMarkManager.Self.CheckEquipBagNewMark());
        },0f);
    }
    /// <summary>
    /// 检测回收按钮红点
    /// </summary>
    private void CheckRecoverMark() 
    {
        if (mGameObjUI == null)
            return;
        GlobalModule.DoLater(() => 
        {
            GameObject mRecoverBtnObj = GameCommon.FindObject(mGameObjUI, "RecoverBtn");
            GameCommon.SetNewMarkVisible(mRecoverBtnObj,RecoverNewMarkManager.Self.CheckRecoverBtn_NewMark());
        },0f);
    }

	public void CheckPetMark()
	{
		if(mGameObjUI == null)
			return;

		mPetMark = mGameObjUI.transform.Find("move_menu_left_bottom/PetBtn/NewMark").gameObject;

	}

    public void CheckUnionBtnMark()
    {
        if (mGameObjUI == null)
            return;
        GlobalModule.DoLater(() =>
        {
            GameObject mRecoverBtnObj = GameCommon.FindObject(mGameObjUI, "ToUnionBtn");
            GameCommon.SetNewMarkVisible(mRecoverBtnObj, UnionBase.CheckMainUIHasNewMark());
        }, 0f);
    }

    private IEnumerator tmpDoPlayerRollPlaying;
	public override void onChange(string keyIndex, object objVal)
	{
		base.onChange(keyIndex, objVal);
		if(keyIndex == "UPDATE_ROLE_SELECT_SCENE")
		{			
			UpdateRoleSelScene();
		}
        else if ("REFRESH_ATLAS_NEWMARK" == keyIndex) 
        {
            RefreshAtlasNewMark();
        }
        else if (keyIndex == "DO_PLAYER_ROLL_PLAYING")
        {
            WallPaper wp = (WallPaper)objVal;
            // 通配符：{0}=玩家角色名；{1}=获得物品的名称（符灵/装备/法器名称)；{2}=使用的道具名称； //改掉了：{3}=装备/法器名称 改为{1}
            /* {0}在神将抽卡中，运气爆发，抽到了稀有符灵{1}
                {0}使用{2}时，运气爆发，抽到了稀有符灵{1}
                {0}经过不懈努力，合成了稀有符灵{1}  
                {0}经过不懈努力，获得了稀有装备{1}
              */
            string format = TableCommon.GetStringFromAnnouncementConfig(wp.type, "ANNOUNCEMENT_TXT");

            // 给通配符前后加上颜色
            string coloredEquipName = ExtensionString.GetColorPrefixByColor(GameCommon.GetNameColor(wp.tidArr[0])) + GameCommon.GetItemName(wp.tidArr[0]) + "[69503E]";
            string coloredPropName = ExtensionString.GetColorPrefixByColor(GameCommon.GetNameColor(wp.propId)) + GameCommon.GetItemName(wp.propId) + "[69503E]";
            string coloredPalyerName = ExtensionString.GetColorPrefixByColor(GameCommon.GetNameColor(wp.charTid)) + wp.name + "[69503E]";

            string content = string.Format(format, coloredPalyerName, coloredEquipName, coloredPropName);

            for(int i = 1; i < wp.tidArr.Length; i++){
                coloredEquipName = ExtensionString.GetColorPrefixByColor(GameCommon.GetNameColor(wp.tidArr[i])) + GameCommon.GetItemName(wp.tidArr[i]) + "[69503E]";
                content += "                                                                                             "
                    + string.Format(format, coloredPalyerName, coloredEquipName, coloredPropName);
            }

            if (tmpDoPlayerRollPlaying != null)
                GlobalModule.Instance.StopCoroutine(tmpDoPlayerRollPlaying);
            RollPlayingPlayer rp = new RollPlayingPlayer(content, 30);
            tmpDoPlayerRollPlaying = rp.DoPlayerRollPlaying();
            GlobalModule.Instance.StartCoroutine(tmpDoPlayerRollPlaying);
        }
	}

    /// <summary>
    /// 刷新图鉴按钮红点
    /// </summary>
    private void RefreshAtlasNewMark() 
    {
        GameObject _atlasBtnObj = GameCommon.FindObject(mGameObjUI, "ToAlbum");
        GameCommon.SetNewMarkVisible(_atlasBtnObj,SystemStateManager.GetNotifState(SYSTEM_STATE.NOTIF_ATLAS));
    }
    
}

public class RoleSelBottomRightWindow : tWindow
{
	public override void OnOpen ()
	{
		InitButtonIsUnLockOrNot();
		base.OnOpen ();
	}
	
	void InitButtonIsUnLockOrNot()
	{
		//GameCommon.ButtonEnbleButCanClick (mGameObjUI, "PVPBtn", UNLOCK_FUNCTION_TYPE.PEAK_FIGHT);
	}

    public override void onChange(string keyIndex, object objVal)
    {
        base.onChange(keyIndex, objVal);
        switch (keyIndex) 
        {
            case "REFRESH_NEWMARK":
                RefreshNewMark();
                break;
            case "UPDATE_ROLE_SELECT_SCENE":
                UpdateScene();
                break;
        }
    }
    private void UpdateScene() 
    {
        SetBtnState();
    }
    private void SetBtnState() 
    {
        //设置VIP专属客服按钮显示状态
        //GameCommon.SetUIVisiable(mGameObjUI, "vip_exclusive_btn", GameCommon.IsFuncCanShowByFuncID(2500));
		//专属客服不显示
		GameCommon.SetUIVisiable(mGameObjUI, "vip_exclusive_btn", false);
    }
    private void RefreshNewMark() 
    {
        if (mGameObjUI == null)
            return;
        GameObject _trialBtnObj = GameCommon.FindObject(mGameObjUI, "lilian_btn");
        GameObject _adventureBtnObj = GameCommon.FindObject(mGameObjUI, "PVEBtn");
        GameCommon.SetNewMarkVisible(_trialBtnObj,TrialNewMarkManager.Self.TrialVisible);
        GameCommon.SetNewMarkVisible(_adventureBtnObj,AdventureNewMarkManager.Self.AdventureBtnVisible);
    }
}

public class RoleSelTopWindow : tWindow
{
	public override void onChange(string keyIndex, object objVal)
	{
		base.onChange(keyIndex, objVal);
		if(keyIndex == "UPDATE_ROLE_NAME")
		{
			UpdateRoleNameUI();
		}
	}
	
	public void UpdateRoleNameUI()
	{
		RoleLogicData logic = RoleLogicData.Self;
        SetText("role_name", logic.name.ToString()); 
		SetText ("vip_level", logic.vipLevel.ToString ());
		SetText ("vip_level_label", "VIP " + logic.vipLevel.ToString ());
	}

}

public class RoleSelTopRightWindow : tWindow
{
	public GameObject mNoticeMark;
	public GameObject mMailMark;
	public GameObject mTaskMark;
	public GameObject mShopMark;
	public GameObject mMorrowLandMark;
	public GameObject mRevelryButton;
	public GameObject mMorrowLandBtn;
    public GameObject mActivityRankBtn;
	public GameObject mOpenFundBtn;
	public GameObject mFirstRechargeBtn;
    public GameObject mFlashSaleMulti;
    public GameObject mFlashSaleSingle;
	Int64 closeOneDay;

    public bool mTaskAwardAcceptable = false;
	
	public override void Init()
	{

	}
	public override void Open(object param)
	{
		base.Open (param);
		mActivityRankBtn = mGameObjUI.transform.Find("ActivityRankBtn").gameObject;
		mRevelryButton = mGameObjUI.transform.Find ("seven_days_carnival_button").gameObject;
		mMorrowLandBtn = mGameObjUI.transform.Find ("morrow_land_button").gameObject;
		mOpenFundBtn = mGameObjUI.transform.Find ("open_fund_button").gameObject;
		mFirstRechargeBtn = mGameObjUI.transform.Find ("first_recharge_button").gameObject;
        mFlashSaleMulti = mGameObjUI.transform.Find("flash_sale_multi_button").gameObject;
        mFlashSaleSingle = mGameObjUI.transform.Find("flash_sale_single_button").gameObject;
		mFirstRechargeBtn.SetActive (false);  //首充礼包不显示
		mOpenFundBtn.SetActive (false);     //开服基金不显示
	}
	
	public void UpdateRoleSelScene()
	{
		CheckNoticeMarkMark();
		CheckMailMark();
		CheckTask();
		CheckShop ();
		CheckMorrowLand ();
		CheckRevelry();
        CheckActivityRank();
//		CheckOpenFund();
		CheckmFirstRecharge();
        //CheckFLashSale();
	}

    private bool GetActivityBtnNewMarkVisible() 
    {
        RoleData _roleData = RoleLogicData.GetMainRole();
        if (_roleData != null) 
        {
            if (ActivityCumulativeWindow.ACTIVITY_OPEN_LEVEL > _roleData.level)
            {
                SystemStateManager.SetNotifState(SYSTEM_STATE.NOTIF_CUMULATIVE, false);
            }
        }
        for(int i = (int)SYSTEM_STATE.NOTIF_LUXURY_SIGN;i < (int)SYSTEM_STATE.NOTIF_ACTIVITY_MAX;i++)
        {
            if (SystemStateManager.GetNotifState((SYSTEM_STATE)i)) 
            {            
                return true;
            }
        }
        return false;
    }
    private void CheckActivityNewMark() 
    {
        if (mGameObjUI == null)
            return;
        tWindow _tWin = DataCenter.GetData("ROLE_SEL_TOP_RIGHT_GROUP") as tWindow;
        if (!_tWin.IsOpen())
            return;
        GameObject _activityBtnObj = GameCommon.FindObject(mGameObjUI, "active_list_button");
        GameCommon.SetNewMarkVisible(_activityBtnObj, GetActivityBtnNewMarkVisible());
    }

	public void CheckNoticeMarkMark()
	{
		if(mGameObjUI == null)
			return;

		mNoticeMark = mGameObjUI.transform.Find("NoticeBtn/NewMark").gameObject;
	}
	
	public void CheckMailMark()
	{
		if(mGameObjUI == null)
			return;

		mMailMark = mGameObjUI.transform.Find("MailBtn/mail_num_label").gameObject;
		RoleLogicData RoleData = RoleLogicData.Self;
		if(RoleData.mMailNum != 0)
		{
			mMailMark.SetActive (true);
			mMailMark.GetComponent<UILabel>().text = RoleData.mMailNum.ToString ();
		}
		else
			mMailMark.SetActive (false);
	}
	
	public void CheckTask()
	{
        if (SystemStateManager.GetNotifState(SYSTEM_STATE.NOTIF_TASK_REFRESH))
        {
            TaskState.shouldRefresh = true;
            SystemStateManager.SetNotifState(SYSTEM_STATE.NOTIF_TASK_REFRESH, false);
        }

        if(SystemStateManager.GetNotifState(SYSTEM_STATE.NOTIF_TASK_FINISH))
        {
            TaskState.hasAwardAcceptable = true;
            SystemStateManager.SetNotifState(SYSTEM_STATE.NOTIF_TASK_FINISH, false);
        }

        if (mGameObjUI == null)
            return;

		mTaskMark = mGameObjUI.transform.Find("TaskBtn/NewMark").gameObject;
        mTaskMark.SetActive(TaskState.hasAwardAcceptable);      
	}

	public void CheckShop()
	{
		if(mGameObjUI == null)
			return;
		
		GameObject shopButton = mGameObjUI.transform.FindChild ("ShopBtn").gameObject;
		mShopMark = shopButton.transform.FindChild ("NewMark").gameObject;
		GetShopFreeInfo getShopFreeInfo = shopButton.GetComponent<GetShopFreeInfo>();
		if(getShopFreeInfo != null) 
			MonoBehaviour.Destroy (getShopFreeInfo);

		getShopFreeInfo = shopButton.AddComponent<GetShopFreeInfo>();
		getShopFreeInfo.obj = mShopMark;
	}
    List<GameObject> mBtnList = new List<GameObject>(); 
	void CheckOpenFund()
	{
        if (mOpenFundBtn == null)
            return;
		GameObject mOpenFundMark = mOpenFundBtn.transform.Find ("NewMark").gameObject;
        if (mOpenFundMark == null)
            return;
		mOpenFundMark.SetActive (SystemStateManager.GetNotifState(SYSTEM_STATE.NOTIF_FUND_DIAMOND) || SystemStateManager.GetNotifState(SYSTEM_STATE.NOTIF_FUND_WELFARE));
	}
	void CheckmFirstRecharge()
	{
		GameObject mFirstRechargeMark = mFirstRechargeBtn.transform.Find ("NewMark").gameObject;
		mFirstRechargeMark.SetActive (SystemStateManager.GetNotifState (SYSTEM_STATE.NOTIF_FIRST_RECHARGE));
	}

    void CheckFLashSale()
    {
        GameObject mFlashSaleMultiMark = mFlashSaleMulti.transform.Find("NewMark").gameObject;
        mFlashSaleMultiMark.SetActive(SystemStateManager.GetNotifState(SYSTEM_STATE.NOTIF_FlASH_SALE_MULTI));

        GameObject mFlashSaleSingleMark = mFlashSaleSingle.transform.Find("NewMark").gameObject;
        mFlashSaleSingleMark.SetActive(SystemStateManager.GetNotifState(SYSTEM_STATE.NOTIF_FLASH_SALE_SINGLE));
    }

    void IsHasFlashSale()
    {
        //限时抢购按钮--可能还要消失吧- -
        if (SystemStateManager.GetNotifState(SYSTEM_STATE.NOTIF_FlASH_SALE_MULTI))
        {
            mBtnList.Add(mFlashSaleMulti);
            mFlashSaleMulti.SetActive(true);
        }
        else
        {
            mFlashSaleMulti.SetActive(false);
        }
        if (SystemStateManager.GetNotifState(SYSTEM_STATE.NOTIF_FLASH_SALE_SINGLE))
        {
            mFlashSaleSingle.SetActive(true);
            mBtnList.Add(mFlashSaleSingle);
        }
        else
        {
            mFlashSaleSingle.SetActive(false);
        }
    }

	public void CheckRevelry()
	{
		if (mGameObjUI == null)
			return;

		GameObject mRevelryBtnMark = mRevelryButton.transform.Find ("NewMark").gameObject;
		mRevelryBtnMark.SetActive(SystemStateManager.GetNotifState(SYSTEM_STATE.NOTIF_REVELRY));

        //added by xuke 
        DateTime startData = DateTime.Parse(CommonParam.mOpenDate);
        Int64 startSeconds = GameCommon.DateTime2TotalSeconds(startData);
        Int64 nowSeconds = GameCommon.DateTime2TotalSeconds(GameCommon.NowDateTime());
        closeOneDay = startSeconds + SevenDaysCarnivalWindow.MAX_GET_DAY * 24 * 60 * 60;

        mBtnList.Clear();
        float mBeginPosX = -360f;
        float mStepX = 85f;
        if (CommonParam.isOnLineVersion)
        {
            if (nowSeconds < closeOneDay)
            {
                mBtnList.Add(mRevelryButton);
            }
        }
        else 
        {
            mRevelryButton.SetActive(false);
        }

        if (CommonParam.mOpenMorrowLand)
        {
            mBtnList.Add(mMorrowLandBtn);
        }
        if (mActivityRankBtn.activeSelf == true)
        {
            mBtnList.Add(mActivityRankBtn);
        }

        IsHasFlashSale(); 

        for (int i = 0, count = mBtnList.Count; i < count; i++) 
        {
            mBtnList[i].transform.localPosition = new Vector3(mBeginPosX - i * mStepX,-100f,0f);
        }
        //end

        if (CommonParam.isOnLineVersion)
        {
            mRevelryButton.SetActive(nowSeconds < closeOneDay);
            //if (nowSeconds < closeOneDay)
            //{
            //    mMorrowLandBtn.transform.localPosition = new Vector3(-445f, -100f, 0f);
            //    mActivityRankBtn.transform.localPosition = new Vector3(-530f, -100f, 0f);
            //}
            //else
            //{
            //    mMorrowLandBtn.transform.localPosition = new Vector3(-360f, -100f, 0f);
            //    mActivityRankBtn.transform.localPosition = new Vector3(-445f, -100f, 0f);
            //}
        }
        else
        {
            mRevelryButton.SetActive(false);
            //mMorrowLandBtn.transform.localPosition = new Vector3(-360f, -100f, 0f);
            //mActivityRankBtn.transform.localPosition = new Vector3(-445f, -100f, 0f);
        }
        //end
	}
    public void CheckActivityRank()
    {
        if (mGameObjUI == null)
            return;

        DateTime startData = DateTime.Parse(CommonParam.mOpenDate);
        Int64 startSeconds = GameCommon.DateTime2TotalSeconds(startData);
        Int64 nowSeconds = GameCommon.DateTime2TotalSeconds(GameCommon.NowDateTime());
        closeOneDay = startSeconds + 24 * 60 * 60*7;        
        mActivityRankBtn.SetActive(closeOneDay-nowSeconds>0);
    }

	public void CheckMorrowLand()
	{
		if (mGameObjUI == null)
			return;

		DateTime startData =  DateTime.Parse (CommonParam.mCreateDate);
		Int64 startSeconds = GameCommon.DateTime2TotalSeconds(startData);
		Int64 nowSeconds = GameCommon.DateTime2TotalSeconds(GameCommon.NowDateTime ());
		closeOneDay = startSeconds + 24*60*60;

        if (CommonParam.mOpenMorrowLand)
        {
            mMorrowLandBtn.SetActive(!MorrowLandLogicData.Self.dayRewardItem[0].isReward);

            //		if(closeOneDay <= GameCommon.DateTime2TotalSeconds(GameCommon.NowDateTime ()))

            mMorrowLandMark = mGameObjUI.transform.Find("morrow_land_button/NewMark").gameObject;
            mMorrowLandMark.SetActive(!MorrowLandLogicData.Self.dayRewardItem[0].isReward && closeOneDay <= nowSeconds);
        }
        else 
        {
            mMorrowLandBtn.SetActive(false);
        }
	
        //if(!MorrowLandLogicData.Self.dayRewardItem[0].isReward && closeOneDay <= nowSeconds)
        //{
        //    mActivityRankBtn.transform.localPosition = new Vector3(-530f, -100f, 0f);
        //}else
        //{
        //    mActivityRankBtn.transform.localPosition = new Vector3(-445f, -100f, 0f);
        //}

	}

    private void CheckShopNewNark()
    {
        if (mGameObjUI == null)
            return;
        GameObject _shopBtnObj = GameCommon.FindObject(mGameObjUI, "ShopBtn");
        GameCommon.SetNewMarkVisible(_shopBtnObj, ShopNewMarkManager.Self.ShopBtnVisible);
    }

    private void RefreshMysteryShopMark(object kObjVal)
    {
        if(kObjVal == null && !(kObjVal is bool))
            return;
        if (mGameObjUI == null)
            return;
        bool _hasRefresh = (bool)kObjVal;
        GameObject _mysteryBtnObj = GameCommon.FindObject(mGameObjUI, "peak_enter_MySteriousShop");
        GameCommon.SetNewMarkVisible(_mysteryBtnObj, _hasRefresh);
    }

    private void RefreshRankActivityNewMark(object kObjVal) 
    {
        if (kObjVal == null && !(kObjVal is bool))
            return;
        if (mGameObjUI == null)
            return;
        bool _hasRefresh = (bool)kObjVal;
        GameObject _mysteryBtnObj = GameCommon.FindObject(mGameObjUI, "ActivityRankBtn");
        GameCommon.SetNewMarkVisible(_mysteryBtnObj, _hasRefresh);
    }

    private void RefreshRechargeMark() 
    {
        if (mGameObjUI == null)
            return;
        GameObject _rechargeBtnObj = GameCommon.FindObject(mGameObjUI, "recharge_button");
        GameCommon.SetNewMarkVisible(_rechargeBtnObj,SystemStateManager.GetNotifState(SYSTEM_STATE.NOTIF_SHOP_VIP_GIFT));
    }

	bool flag=true;
	public void SetTime()
	{
		if(flag)
		{
			vp_Timer.In((closeOneDay - GameCommon.DateTime2TotalSeconds(GameCommon.NowDateTime ())),TimeIsOver);
			flag=false;
		}
	}

	public  void TimeIsOver()
	{
//		GlobalModule.DoCoroutine(ddd());	
		CheckMorrowLand ();
		MorrowLandGiftWindow tmpWin = DataCenter.GetData("MORROW_LAND_WINDOW") as MorrowLandGiftWindow ;
		if(tmpWin.IsOpen())
			tmpWin.Refresh(null);
	}

//	private IEnumerator ddd()
//	{
//		yield return GlobalModule.DoCoroutine(_DoAction());	
//		CheckMorrowLand ();
//	}
//
//	private IEnumerator _DoAction()
//	{
//		yield return NetManager.StartWaitMorrowLandQuery();
//
//		MorrowLandGiftWindow tmpWin = DataCenter.GetData("MORROW_LAND_WINDOW") as MorrowLandGiftWindow ;
//		if(tmpWin.IsOpen())
//			tmpWin.Refresh(null);
//	}

	public override void onChange(string keyIndex, object objVal)
	{
		base.onChange(keyIndex, objVal);
		if(keyIndex == "UPDATE_ROLE_SELECT_SCENE")
		{			
			UpdateRoleSelScene();
		}
        else if (keyIndex == "UPDATE_ACTIVITY_LIST_BTN_NEWMARK") 
        {
            CheckActivityNewMark();
			CheckOpenFund();
        }
        else if (keyIndex == "UPDATE_SHOP_MARK") 
        {
            CheckShopNewNark();        
        }
        else if (keyIndex == "UPDATE_MYSTERYSHOP_MARK") 
        {
            RefreshMysteryShopMark(objVal);
        }
        else if (keyIndex == "UPDATE_RECHARGE_MARK") 
        {
            RefreshRechargeMark();
        }
        else if (keyIndex == "UPDATE_FLASH_SALE")
        {
            CheckRevelry();
        }
        else if (keyIndex == "UPDATE_RANK_ACTIVITY") 
        {
            RefreshRankActivityNewMark(objVal);
        }
	}

}

public class RoleSelTopLeftWindow : tWindow
{
	public static int msFightStrengthNum = 0;
    public static int TmpChangeFitting { set; get; }
    public static int FightStrengthNum
    {
        get { return msFightStrengthNum; }
        set
        {
            msFightStrengthNum = value;
            TmpChangeFitting = msFightStrengthNum;
        }
    }

	public UILabel mRoleNameLabel;

	public override void Init()
	{
		
	}
	
	public void UpdateRoleSelScene()
	{
	}
	
	public void CheckOnHookState()
	{
		if(mGameObjUI == null)
			return;

		GameObject btn = mGameObjUI.transform.Find("move_menu_left_top/on_hook_btn/on_hook_award_button").gameObject;
		if(btn != null)
		{
			OnHookLogicData logicData = DataCenter.GetData("ON_HOOK_DATA") as OnHookLogicData;
			btn.SetActive(logicData.mOnHook.mState == ON_HOOK_STATE.UN_GET_AWARD);
		}
	}

	public override void OnOpen()
	{
        // TODO [8/21/2015 LC]
        return;
		GameObject btn = mGameObjUI.transform.Find("move_menu_left_top/on_hook_btn").gameObject;
		if(btn != null)
		{
			OnHookLogicData logicData = DataCenter.GetData("ON_HOOK_DATA") as OnHookLogicData;
			btn.SetActive(logicData.mOnHook.mState != ON_HOOK_STATE.LOCKED);

			GetOnHookState(null);
		}

		UpdateRoleNameUI();
        SetFightingStrength();
	}

	public void GetOnHookState(object param)
	{
		OnHookLogicData logicData = DataCenter.GetData("ON_HOOK_DATA") as OnHookLogicData;
		if(logicData.mOnHook.mState != ON_HOOK_STATE.LOCKED)
		{
			tEvent evt = Net.StartEvent("CS_RequestIdleBottingStatus");


			OnHookWindow win = DataCenter.GetData("ON_HOOK_WINDOW") as OnHookWindow;
			if(win.mGameObjUI != null)
				evt.set ("WINDOW_NAME", "ON_HOOK_WINDOW");

			evt.DoEvent();
		}
	}

	public void SetOnHookRemainTime()
	{
		UILabel label = mGameObjUI.transform.Find("move_menu_left_top/on_hook_btn/time_label").GetComponent<UILabel>();
		if(label != null)
		{
			OnHookLogicData logicData = DataCenter.GetData("ON_HOOK_DATA") as OnHookLogicData;
			label.text = logicData.mOnHook.mRemainingTime.ToString();

			label.gameObject.SetActive(logicData.mOnHook.mState == ON_HOOK_STATE.DOING);

			if(logicData.mOnHook.mRemainingTime > 0)
			{
				CountdownUI countdownUI = label.GetComponent<CountdownUI>();
				if(countdownUI != null) 
					MonoBehaviour.Destroy (countdownUI);

				SetCountdownTime(label.name, (Int64)(logicData.mOnHook.mRemainingTime + CommonParam.NowServerTime()), new CallBack(this, "GetOnHookState", null));
			}
		}
	}
	
	public override void onChange(string keyIndex, object objVal)
	{
		base.onChange(keyIndex, objVal);
		if(keyIndex == "UPDATE_ROLE_SELECT_SCENE")
		{			
			UpdateRoleSelScene();
		}
		else if(keyIndex == "UPDATE_ROLE_NAME")
		{
			UpdateRoleNameUI();
			SetFightingStrength();
		}
		else if(keyIndex == "SET_ON_HOOK_REMAIN_TIME")
		{
			SetOnHookRemainTime();
			CheckOnHookState();
		}
		else if(keyIndex == "UPDATE_MAIL_MARK")
		{
			CheckMailMark();
		}
		else if(keyIndex == "UPDATE_FRIEND_MARK" || keyIndex == "UPDATE_SPIRIT_MARK")
		{
			CheckFriendMark();
		}
	}

	private void CheckMailMark()
	{
		if(mGameObjUI == null)
			return;
		
		RoleLogicData RoleData = RoleLogicData.Self;
		GameObject newMarkObj = mGameObjUI.transform.Find("move_menu_left_top/MailBtn/NewMark").gameObject;
        newMarkObj.SetActive(SystemStateManager.GetNotifState(SYSTEM_STATE.NOTIF_MAIL));
	}
	
	public void CheckFriendMark()
	{
		if(mGameObjUI == null)
            return;

        RoleLogicData RoleData = RoleLogicData.Self;
        GameObject newMarkObj = mGameObjUI.transform.Find("move_menu_left_top/FriendBtn/NewMark").gameObject;
        bool bActive = SystemStateManager.GetNotifState(SYSTEM_STATE.NOTIF_FRIEND_REQUEST) ||
            SystemStateManager.GetNotifState(SYSTEM_STATE.NOTIF_FRIEND_SPIRIT);
        newMarkObj.SetActive(bActive);
	}

	void RequestFriendInviteSuccess(string text) 
	{
        GetUILabel("role_name").color = GameCommon.GetNameColor(RoleLogicData.Self.character.tid);
        GetUILabel("role_name").effectColor = GameCommon.GetNameEffectColor();
		FriendLogicData logicData = JCode.Decode<FriendLogicData>(text);
		DataCenter.RegisterData("REQUEST_INVITE_FRIEND_LIST", logicData);
		RoleLogicData roleLogicData = RoleLogicData.Self;
		
		FriendLogicData friendLogicData =  DataCenter.GetData("REQUEST_INVITE_FRIEND_LIST") as FriendLogicData;
		if(friendLogicData != null) roleLogicData.mInviteNum = friendLogicData.arr.Length;
		else roleLogicData.mInviteNum = 0;
		
		SystemStateManager.SetNotifState(SYSTEM_STATE.NOTIF_FRIEND_REQUEST, logicData.arr.Length > 0);
	}

	public void UpdateRoleNameUI()
	{
        //by chenliang
        //begin

        if (mGameObjUI == null)
            return;

        //end
		RoleData d = RoleLogicData.GetMainRole();
		mRoleNameLabel = GameCommon.FindObject(mGameObjUI, "role_name").GetComponent<UILabel>();
		RoleLogicData logic = RoleLogicData.Self;
		mRoleNameLabel.text = logic.name.ToString();

		GameCommon.FindObject (mGameObjUI, "vip_level").gameObject.SetActive (false);
		GameCommon.FindObject (mGameObjUI, "vip_level_label").gameObject.SetActive (false);
		if(CommonParam.isOnLineVersion)
		{
			GameCommon.FindObject (mGameObjUI, "vip_level_label").gameObject.SetActive (true);
			SetText ("vip_level_label", "VIP " + logic.vipLevel.ToString ());
		}else
		{
			GameCommon.FindObject (mGameObjUI, "vip_level").gameObject.SetActive (true);
			SetText ("vip_level", logic.vipLevel.ToString ());
		}   

//		RefreshExpBar(mGameObjUI, d);
		RefreshPlayerLevelExpBar();
		SetText ("fight_strength_number", GameCommon.ShowNumUI(System.Convert.ToInt32(GameCommon.GetPower().ToString("f0"))));

		GameCommon.SetRoleIcon (GetSub("player_icon").GetComponent<UISprite>(),RoleLogicData.Self.character.tid,GameCommon.ROLE_ICON_TYPE.CIRCLE);

//		DataRecord _record = DataCenter.mRoleSkinConfig.GetRecord (logic.character.tid);
//		string _atlasName = _record.getObject ("ATLAS").ToString();
//		string _spriteName = _record.getObject ("SPRITE").ToString ();
//		GameCommon.SetIcon (GetSub ("player_icon").GetComponent<UISprite>(),_atlasName,_spriteName);
		//GameCommon.SetPalyerIcon (GetSub ("player_icon").GetComponent<UISprite>());
	}

	private void RefreshPlayerLevelExpBar()
	{
		int levelUpExp = TableManager.GetData("CharacterLevelExp", RoleLogicData.Self.character.level, "LEVEL_EXP");
		//float exp = RoleLogicData.Self.chaExp;
		float exp = RoleLogicData.GetMainRole().exp;
		float expRate = exp / levelUpExp;
		SetText("role_level_num", RoleLogicData.GetMainRole().level.ToString());
		GameObject bar = GetSub ( "Progress Bar");
		bar.GetComponent<UIProgressBar>().value = expRate;
		GameCommon.SetUIText(bar, "role_level_pct", Float2Percent(expRate));
	}

	private void RefreshExpBar(GameObject obj, RoleData d)
	{
		int levelUpExp = TableManager.GetData("CharacterLevelExp", d.level, "LEVEL_EXP");
		float exp = d.exp;
		float expRate = exp / levelUpExp;
		GameCommon.SetUIText(obj, "role_level_num", d.level.ToString());
		GameObject bar = GameCommon.FindObject(obj, "Progress Bar");
		bar.GetComponent<UIProgressBar>().value = expRate;
		GameCommon.SetUIText(bar, "role_level_pct", Float2Percent(expRate));
	}
	private string Float2Percent(float value)
	{
		return (int)(value * 100f) + "%";
	}
	private void SetFightingStrength()
	{
        int _difFightStrength = Mathf.RoundToInt(GameCommon.GetPower()) - FightStrengthNum;
        if (FightStrengthNum != 0 && _difFightStrength != 0)
		{
            UILabel _addStrengthLbl = GameCommon.FindComponent<UILabel>(mGameObjUI, "fight_strength_add_number");
            if (_addStrengthLbl != null) 
            {
                _addStrengthLbl.color = _difFightStrength > 0 ? GameCommon.GetDefineColor(CommonColorType.GREEN) : GameCommon.GetDefineColor(CommonColorType.HINT_RED);
            }
			SetVisible ("fight_strength_add_number", true);
            string _fuhao = _difFightStrength > 0 ? "+" : "";
            SetText("fight_strength_add_number", _fuhao + _difFightStrength.ToString());
			GameObject _addStrengthObj = GetSub("fight_strength_add_number");
            UITweener _tween = null;
            if(_addStrengthObj != null)
            {
                _tween = _addStrengthObj.GetComponent<UITweener>();
                if(_tween != null)
                {
                    _tween.onFinished.Add(new EventDelegate(TweenOver));
                    PlayTween(_tween);
                }
            }

			//TweenPosition tw = TweenPosition.Begin(GetSub ("fight_strength_add_number"), 1.0f, new Vector3(0, 30, 0));
            
		}
		else
			SetVisible ("fight_strength_add_number", false);
		
		//by chenliang
		//begin
		
		//		msFightStrengthNum = GameCommon.GetTotalFightingStrength();
		//        SetText("fight_strength_number", GameCommon.GetTotalFightingStrength().ToString());
		//-----------------
		//战斗力获取用GameCommon.GetPower()
        float _preFightNum = FightStrengthNum;
		FightStrengthNum = System.Convert.ToInt32 (GameCommon.GetPower ().ToString ("f0"));
		//        SetText("fight_strength_number", msFightStrengthNum.ToString());
		//SetText("fight_strength_number", GameCommon.ShowNumUI(FightStrengthNum));
		
		//end
        //scale tween
        GameObject _strengthObj = GetSub("fight_strength_number");
        if (_strengthObj != null) 
        {
            UITweener _tweenScale = _strengthObj.GetComponent<UITweener>();
            UILabel _strengthLbl = _strengthObj.GetComponent<UILabel>();
            if (FightStrengthNum - _preFightNum != 0)
            {            
                PlayTween(_tweenScale);
                //lerp strength              
                if (_strengthLbl != null)
                {
                    GlobalModule.DoCoroutine(AddNumber(_strengthLbl, _preFightNum, FightStrengthNum, _tweenScale.duration));
                }
            }
            else 
            {
                ResetTween(_tweenScale);
                if (_strengthLbl != null) 
                {
                    _strengthLbl.text = GameCommon.ShowNumUI(FightStrengthNum);
                }
            }
        }
       
	}

    private float mWaitStep = 0.05f;
    private IEnumerator AddNumber(UILabel kLbl,float kOriginNum,float kTargetNum,float kDuration) 
    {
        if (kLbl == null)
            yield break;
        float _totalStepNum = kTargetNum - kOriginNum;
        int _step = (int)(kDuration / mWaitStep);
        float _stepNum = _totalStepNum / _step;
        float _stepShowNum = kOriginNum;
        for (int i = 0; i < _step; ++i) 
        {
            _stepShowNum += _stepNum;
            kLbl.text = GameCommon.ShowNumUI((long)_stepShowNum);
            yield return new WaitForSeconds(mWaitStep);
        }
        kLbl.text = GameCommon.ShowNumUI((long)kTargetNum); ;
    }

	void TweenOver()
	{
		GetSub ("fight_strength_add_number").transform.localPosition = Vector3.zero;
		SetVisible ("fight_strength_add_number", false);
	}
}

public class RolePartCongifRecord
{
	public int mIndex;
	public int mModelIndex;
	public int mAttackSkill;
	public string mEffect = "";
	public string mUIEffect = "";
	public float mScale = 1.0f;
	public float mUIScale = 1.0f ;
}