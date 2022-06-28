using UnityEngine;
using System.Collections;
using Logic;
using DataTable;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class MainSelectRole : MonoBehaviour {

	// Use this for initialization
	void Start () {
		DataCenter.RegisterData("SelectRoleUI", new SelectRoleUIData());
        DataCenter.RegisterData("MessageWindow", new RoleMessageWindow());
		DataCenter.RegisterData("InputNameWindow", new InputNameWindow());
		DataCenter.RegisterData("LandingWindow", new LandingWindow());
       
//        DataCenter.RegisterData("BackWindow", new BackWindow());
		//		DataCenter.SetData("LANDING_WINDOW", "OPEN", true);

        //DataCenter.OpenWindow("LOGIN_WINDOW");        

	}
	
    void OnDestroy()
    {
        DataCenter.Remove("SelectRoleUI");
        DataCenter.Remove("SELECT_CREATE_ROLE_WINDOW");
		DataCenter.Remove("MessageWindow");
		DataCenter.Remove("InputNameWindow");
		DataCenter.Remove("LandingWindow");
    }
}


public class SelectRoleUIData : tLogicData
{	
	public override void onChange(string key, object value )
	{
		if (key == "CLOSE")
		{
            DataCenter.SetData("SELECT_CREATE_ROLE_WINDOW", "CLOSE", true);
			DataCenter.SetData("MessageWindow", "CLOSE", true);
			DataCenter.SetData("InputNameWindow", "CLOSE", true);
			DataCenter.SetData("LandingWindow", "CLOSE", true);
		}
	}
}

//-------------------------------------------------------------------------
//-------------------------------------------------------------------------
public class Button_change_server_area : CEvent
{
    public override bool _DoEvent()
    {
        GlobalModule.ClearAllWindow();
		DataCenter.SetData ("SELECT_SERVER_WINDOW", "BACK_WINDOW", getObject ("BACK_WINDOW").ToString ());
        DataCenter.OpenWindow("SELECT_SERVER_WINDOW");
        return true;
    }
}

public class Button_bind_id_button : CEvent
{
	public override bool _DoEvent ()
	{
		DataCenter.OpenWindow ("REGISTRATION_WINDOW", "LANDING_WINDOW");
		DataCenter.CloseWindow ("LANDING_WINDOW");
		return true;
	}
}
public class Button_UserAgreement : CEvent
{
	public override bool _DoEvent ()
	{
		DataCenter.OpenWindow ("USER_AGREEMENT_WINDOW");
		return true;
	}
}
public class EnterGameWindow : tWindow
{
	public override void Init ()
	{
		EventCenter.Self.RegisterEvent("Button_change_id_button", new DefineFactory<Button_change_id_button>());
		EventCenter.Self.RegisterEvent("Button_change_server_area", new DefineFactory<Button_change_server_area>());
		EventCenter.Register ("Button_bind_id_button", new DefineFactory<Button_bind_id_button>());
		EventCenter.Register ("Button_UserAgreement", new DefineFactory<Button_UserAgreement>());
	}

	public override void OnOpen ()
	{
// #if !UNITY_EDITOR && !NO_USE_SDK
//         if(CommonParam.isUseSDK) {
//             if(U3DSharkSDK.Instance.IsHasRequest(U3DSharkAttName.SUPPORT_PERSON_CENTER))
//                 U3DSharkSDK.Instance.ShowPersonCenter();    
//         }
// #endif
        
		base.OnOpen ();
        //by chenliang
        //begin

        /// <summary>
        /// 根据SDK设置UI
        /// </summary>
        __CheckSDKUI();

        //end
        bool isTourists = false;
		if(PlayerPrefs.HasKey ("IS_TOURISTS") && PlayerPrefs.GetString("IS_TOURISTS") == "isTourists")
			isTourists = true;

		SetVisible ("bind_id_button", isTourists);
		SetVisible ("change_id_button", !isTourists);
		SetVisible ("landing_label", false);
		SetVisible ("LandingPushGoinButton", true);

		bool isNotJustRegisterAccount = get ("OPEN");
        DEBUG.Log("isNotJustRegisterAccount:---"+isNotJustRegisterAccount.ToString());
		if(isNotJustRegisterAccount)
			DataCenter.OpenWindow("LOGIN_WINDOW");
		else 
            Refresh (null);
        //DataCenter.OpenWindow(UIWindowString.announce_info);
        //GlobalModule.DoCoroutine(WaitAnd_RefreshZoneState());
	}
    private const float WAIT_SECONDS = 10f; //> 等待刷新间隔
    private const float CHECK_SECONDS = 0.5f;   //> 检测单位时间
    public static bool mIsNeedStop = false; //> 是否需要停止请求 
    private int mCoroutineCount = 0;        //> 定时请求服务器状态的协程数量
    //added by xuke
    private void RefreshZoneState() 
    {
        tWindow _win = DataCenter.GetData("LANDING_WINDOW") as tWindow;
        //1.先请求一次
        if (_win.IsOpen()) 
        {
            LoginNet.RequestServerState();
        }
        //mIsNeedStop = false;
        //tWindow _win = DataCenter.GetData("LANDING_WINDOW") as tWindow;
        ////1.先请求一次
        //if (_win.IsOpen())
        //{
        //    LoginNet.RequestServerState();
        //}
        //else 
        //{
        //    yield break;
        //}
        //int _timeCount = 0;
        //int _totalCount = (int)(WAIT_SECONDS / CHECK_SECONDS);
        //while (true) 
        //{
        //    yield return new WaitForSeconds(CHECK_SECONDS);
        //    _timeCount++;
        //    if (_win.IsOpen())
        //    {
        //        if (mIsNeedStop)
        //            break;
        //        if (_timeCount >= _totalCount)
        //        {
        //            _timeCount = 0;
        //            LoginNet.RequestServerState();
        //        }
        //    }
        //    else 
        //    {
        //        break;
        //    }
        //}
    }
    //end

    //by chenliang
    //begin

    /// <summary>
    /// 检测SDK
    /// </summary>
    private void __CheckSDKUI()
    {
        GameObject tmpGOCurrIDInfo = GameCommon.FindObject(mGameObjUI, "current_id_info");
        UISprite tmpBG = GameCommon.FindComponent<UISprite>(mGameObjUI, "bg_sprite");
        if (tmpGOCurrIDInfo != null)
            tmpGOCurrIDInfo.SetActive(!CommonParam.isUseSDK);
        if (tmpBG != null)
            tmpBG.height = CommonParam.isUseSDK ? 130 : 204;
    }

    //end

	public void OpenFinish()
	{
		GameObject obj = GameCommon.FindObject(mGameObjUI, "LandingPushGoinButton");
        if (obj != null)
		{
			TweenScale scale = obj.GetComponent<TweenScale>();
			if(scale != null)
			{
				obj.SetActive(true);

				scale.RemoveOnFinished(new EventDelegate(OpenFinish));
				scale.ResetToBeginning();
			}
		}
	}

	public void CloseFinish()
	{
		GameObject obj = GameCommon.FindObject(mGameObjUI, "LandingPushGoinButton");
		if (obj != null)
		{
			obj.SetActive(false);
		}
	}
	

    public override void onChange(string keyButtonName, object value)
    {
        base.onChange(keyButtonName, value);

		if (keyButtonName == "AGREE") 
		{
			UIImageButton btn = GameCommon.FindObject(mGameObjUI,"LandingPushGoinButton").GetComponent<UIImageButton>();
			btn.isEnabled = CommonParam.isUserAgree;
		}
        else if (keyButtonName == "REFRESH_ZONE_STATE") 
        {
            RefreshZoneState();
        }
        else if (keyButtonName == "REFRESH_ZONE_STATE_AFTER_SUCCESS") 
        {
            GlobalModule.DoLater(() => { RefreshZoneState(); }, 10f);
        }
    }

	public override bool Refresh(object param)
    {
		NiceData buttonData = GameCommon.GetButtonData (GetSub ("change_server_area"));
		buttonData.set ("BACK_WINDOW", "LANDING_WINDOW");

        //if(LoginNet.msServerName=="") LoginNet.msServerName="002绝世无双(内网)";

		SetText("server_name", LoginNet.msServerName);


		LoginNet.SetServerStateText( GetSub("server_state") );
        //SetText("server_name","一区");

//        bool bOkEnterGame = DataCenter.Get("ENTER_GS");
//        if (bOkEnterGame)
//        {
//            SetVisible("try_login_button", false);
//			SetVisible ("LandingPushGoinButton", true);
//			SetVisible ("current_id_info", true);
//			SetVisible ("current_server_info", true);
//        }
//        else
//		{
// 			SetVisible ("LandingPushGoinButton", false);
//			SetVisible ("current_id_info", false);
//			SetVisible ("current_server_info", false);
//		}

//		SetText ("version_number", CommonParam.ClientVer);
		DataCenter.OpenWindow ("VERSION_NUMBER_WINDOW");
		string[] r = GameCommon.GetSavedLoginDataFromUnity();
        DEBUG.Log("string[] r"+(r==null).ToString());
		if (r != null)
		{
			string account = r[0];
			string password = r[1];
			SetText ("id_name", account);
			if(account != "" && password != "")
			{
                //by chenliang
                //begin

//				SetVisible ("current_id_info", true);
//-----------------------
                SetVisible("current_id_info", true && !CommonParam.isUseSDK);

                //end
				SetVisible ("current_server_info", true);
				SetVisible ("LandingPushGoinButton", true);
			}
		}

		if(param != null)
			SetText ("id_name", param.ToString ());

		return true;
    }
    
	public override void Close ()
	{
		DataCenter.CloseWindow ("VERSION_NUMBER_WINDOW");
		base.Close ();
	}

}

public class LandingWindow : tWindow
{
	//GameObject mWinObject;
	
	public override void Init()
	{
		mGameObjUI = GameCommon.FindUI("LandingWindow");
		
//		Close();
	}

    public override bool Refresh(object param)
    {        
		return true;
    }
	public override void onChange (string keyIndex, object objVal)
	{
		base.onChange (keyIndex, objVal);

	}
	//public override void Open(object param) { mWinObject.SetActive(true); }
	//public override void Close() { mWinObject.SetActive(false); }
}


public class SelectModelBaseWindow : tWindow
{
	public string mStrSceneName = "create_scene";
	public int mCurSelIndex = 0;
	public int mCurModelIndex = 0;
	GameObject card ;
	int modelMaxNum = 0;

	public override void onChange(string keyIndex, object objVal)
	{
		base.onChange(keyIndex, objVal);
		if(keyIndex == "INIT_ROLE_SELECT")
		{			
			InitRoleSel();
		}
		else if(keyIndex == "CLEAR_GRID")
		{
			ClearGrid();
		}
		else if(keyIndex == "HIDE_ALL_EFFECT")
		{
			HideAllUIEffect();
		}
		else if(keyIndex == "SET_INDEX")
		{
			mCurSelIndex = (int)objVal;
			SetChoseEffect((int)objVal);
			InitData(mCurSelIndex);
			Refresh(null);
		}
	}

	void SetChoseEffect(int iIndex)
	{

		UIGridContainer  roleChoseGrid = GameCommon.FindComponent<UIGridContainer>(mGameObjUI, "role_chose_grid");
		roleChoseGrid.MaxCount = modelMaxNum;

		for(int i = 0; i < roleChoseGrid.MaxCount; i++)
		{
			GameObject obj = roleChoseGrid.controlList[i];
			GameObject roleIconChoseObj = obj.transform.Find ("role_info_chose_button/role_icon_chose").gameObject;
           
			if(i == iIndex)
			{
				roleIconChoseObj.SetActive (true);
			}else
			{
				roleIconChoseObj.SetActive (false);
			}
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		foreach (KeyValuePair<int, DataRecord> v in DataCenter.mRoleUIConfig.GetAllRecord ())
		{
			if(v.Key != null)
			{
				modelMaxNum ++;
			}
		}

		card = GameCommon.FindObject(mGameObjUI, "select_role_model_index_card");
		GameCommon.InitCardWithoutBackground(card, 1.0f, mGameObjUI);
        //by chenliang
        //begin

        //将主角模型z值调成正值，不挡住别的窗口
        GameObject tmpUIPoint = GameCommon.FindObject(card, "UIPoint");
        if (tmpUIPoint != null)
        {
            Vector3 tmpPos = tmpUIPoint.transform.localPosition;
            tmpPos.z = -tmpPos.z;
            tmpUIPoint.transform.localPosition = tmpPos;
        }

        //end
		SetChoseEffect(0);
		InitModelOrScene();
	}


	public virtual void InitModelOrScene()
	{
        //if(!GameCommon.bIsLogicDataExist("SELECT_SCENE_WINDOW"))
        //{
        //    GlobalModule.Instance.LoadScene("Role_Creat_01", false);
        //}
        //else
		{
			InitRoleSel();
		}
	}

	public virtual void ClearGrid()
	{
		if(mGameObjUI != null)
		{
			UIGridContainer cardGrid = mGameObjUI.transform.Find("grid").GetComponent<UIGridContainer>();
			cardGrid.MaxCount = 0;
		}
	}

	public virtual void InitRoleSel()
	{
		Rest();

		InitData(mCurSelIndex);
//
//		SetTrans();

		Refresh(null);
	}

	public void Rest()
	{
		mCurSelIndex = 0;
		mCurModelIndex = 0;
	}

	public virtual void InitGrid()
	{
		UISelModelMastButton mastButton = mGameObjUI.transform.Find("grid").GetComponent<UISelModelMastButton>();
		mastButton.Init();
	}

	public virtual void SetTrans()
	{
		GameObject bgScene = GameObject.Find(mStrSceneName);
		Transform uiGroup = bgScene.transform.Find("ui_group");
		UIGridContainer grid = mGameObjUI.transform.Find("grid").GetComponent<UIGridContainer>();
		GameObject group = grid.controlList[0];
		group.transform.parent = uiGroup;
		group.transform.localPosition = Vector3.zero;
		group.transform.localRotation = Quaternion.identity;
		group.transform.localScale = Vector3.one;
	}
	
	public void InitData(int iIndex)
	{
		if(mGameObjUI != null)
		{
//			InitGrid();

			int iModeIndex = TableCommon.GetNumberFromRoleUIConfig (iIndex, "MODEL");
			GameCommon.SetCardInfo(card.name, iModeIndex, 0, 0, mGameObjUI);

//			mCardGrid = mGameObjUI.transform.Find("card_grid").GetComponent<UIGridContainer>();
//			mCardGrid.MaxCount = 1;
//			mCardGroup = mCardGrid.controlList[0];
			
//			int iMaxNum = 4;
//			mRoleAndPets = new GameObject[iMaxNum];
//			for(int i = 0; i < iMaxNum; i++)
//			{
//				GameObject card = mCardGroup.transform.Find("card" + i.ToString()).gameObject;
//				mRoleAndPets[i] = card;
//			}
//			
//			mRolePointList = new GameObject[iMaxNum];
//			for(int i = 0; i < iMaxNum; i++)
//			{
//				GameObject point = mRoleAndPets[i].transform.Find("UIPoint").gameObject;
//				mRolePointList[i] = point;
//			}
//			
//			mInfoList = new GameObject[iMaxNum];
//			for(int i = 0; i < iMaxNum; i++)
//			{
//				GameObject info = mGameObjUI.transform.Find("info" + i.ToString()).gameObject;
//				mInfoList[i] = info;
//			}
		}
	}

	public virtual void SetModelBackEffect()
	{

	}

	public virtual void HideAllUIEffect()
	{

	}

	public override void Close()
	{
		HideAllUIEffect();
		base.Close();
	}

}
// select main role model after create account

public class SelectCreateRoleWindow : SelectModelBaseWindow
{
//	public const int MODLE_MAX_NUM = 3;
	public int modelMaxNum = 0;
    public const string mInputNamePath = "transform_tween_root/transform_root_2/";
    public const string mRoleInfoPath = "transform_tween_root/transform_root_1/introducegroup/";
	public SelectCreateRoleWindow()
	{
		mStrSceneName = "create_scene";
	}

    public override void Init()
    {
		EventCenter.Self.RegisterEvent("Button_random_name_button", new DefineFactory<Button_RandomNameButton>());
		EventCenter.Self.RegisterEvent("Button_role_info_chose_button", new DefineFactory<Button_role_info_chose_button>());
		EventCenter.Self.RegisterEvent("Button_CreateRoleOKButton", new DefineFactory<Button_CreateRoleOKButton>());
        EventCenter.Self.RegisterEvent("Button_gm_Button", new DefineFactory<Button_gm_Button>());

		foreach (KeyValuePair<int, DataRecord> v in DataCenter.mRoleUIConfig.GetAllRecord ())
		{
			if(v.Key != null)
			{
				modelMaxNum ++;
			}
		}

    }

//	public override void InitModelOrScene()
//	{
//		if(GameCommon.bIsWindowExist("SELECT_SCENE_WINDOW"))
//		{
//
//		}
//		else
//		{
//			GlobalModule.Instance.LoadScene("Role_Creat_01", false);
//		}
//	}

	public override void Open(object param)
	{
		base.Open (param);
        SetVisible("gm_Button", CommonParam.isOpenGM);
		RandomName();
	}

	public override bool Refresh (object param)
	{
		DataCenter.CloseWindow ("REGISTRATION_WINDOW");
        GuideManager.Notify(GuideIndex.SelectRole);

		mCurModelIndex = TableManager.GetData("RoleUIConfig", mCurSelIndex, "MODEL");

		SetInfo();
		SetModelBackEffect();

		SetRolePhoto();

		return base.Refresh (param);
	}
	public void SetRolePhoto()
	{
		UIGridContainer  roleChoseGrid = GameCommon.FindComponent<UIGridContainer>(mGameObjUI, "role_chose_grid");
		roleChoseGrid.MaxCount = modelMaxNum;
		
		for(int i = 0; i < roleChoseGrid.MaxCount; i++)
		{
			GameObject obj = roleChoseGrid.controlList[i];
			int modelIndex = TableCommon.GetNumberFromRoleUIConfig (i, "MODEL");
			string atlasName = TableCommon.GetStringFromRoleUIConfig (i, "ATLAS");
			string atlasIconName = TableCommon.GetStringFromRoleUIConfig (i, "SPRITE");
			
			UISprite  sprite = obj.transform.Find ("role_info_chose_button/role_icon").GetComponent<UISprite>();
			UISprite  spriteChose = obj.transform.Find ("role_info_chose_button/role_icon_chose").GetComponent<UISprite>();
			GameCommon.SetIcon (sprite, atlasName, atlasIconName);
			GameCommon.SetIcon (spriteChose, atlasName, atlasIconName);

			GameCommon.GetButtonData(obj.transform.Find ("role_info_chose_button").gameObject).set("ROLE_MODEL_INDEX", i);
		}
	}

	public void SetInfo()
	{
		UILabel nameLabel = mGameObjUI.transform.Find("group/background/name_label").GetComponent<UILabel>();
		//角色介绍
        UILabel roleNameLabel = mGameObjUI.transform.Find (mRoleInfoPath+"role_name_label").GetComponent<UILabel>();
        UILabel roleSexLabel = mGameObjUI.transform.Find(mRoleInfoPath + "role_sex_label").GetComponent<UILabel>();
        UILabel roleAgeLabel = mGameObjUI.transform.Find(mRoleInfoPath + "role_age_label").GetComponent<UILabel>();
        UILabel roleProfessionLabel = mGameObjUI.transform.Find(mRoleInfoPath + "role_profession_label").GetComponent<UILabel>();
        UILabel roleBirthPlaceLabel = mGameObjUI.transform.Find(mRoleInfoPath + "role_birthplace_label").GetComponent<UILabel>();
        UILabel roleWeaponLabel = mGameObjUI.transform.Find(mRoleInfoPath + "role_weapon_label").GetComponent<UILabel>();
        UILabel roleCharacterLabel = mGameObjUI.transform.Find(mRoleInfoPath + "role_character_label").GetComponent<UILabel>();
        UILabel rolePoemLabel = mGameObjUI.transform.Find(mRoleInfoPath + "role_poem_label").GetComponent<UILabel>();

        if (nameLabel != null && roleNameLabel != null && roleSexLabel != null && roleAgeLabel != null && roleProfessionLabel != null && roleBirthPlaceLabel != null && roleWeaponLabel != null && roleCharacterLabel != null && rolePoemLabel !=null)
		{
			string strName = TableCommon.GetStringFromActiveCongfig(mCurModelIndex, "NAME");
            //设置背景图片
            string SpriteName = TableCommon.GetStringFromRoleUIConfig(mCurSelIndex, "BACKGROUND_SPRITE_NAME");
            UITexture texture = mGameObjUI.transform.Find("transform_tween_root/transform_root_1/bg_sprite/Sprite/role_bg_texture").GetComponent<UITexture>();
            if (texture != null)
            {
                string strPicName = "textures/UItextures/Background/" + SpriteName;
                texture.mainTexture = Resources.Load(strPicName, typeof(Texture)) as Texture;
            }
            //if (mCurSelIndex == 2)
            //{ 
            //    strName = "?";       
            //}
            nameLabel.text = strName;
            //角色介绍赋值
            string describeText = GameCommon.ReplaceWithSpace(TableCommon.GetStringFromRoleUIConfig(mCurSelIndex, "ROLE_INFO"));
            string[] describeArr = describeText.Split(new char[]{'|'});
            roleNameLabel.text = describeArr[0];
            roleSexLabel.text = describeArr[1];
            roleAgeLabel.text = describeArr[2];
            roleProfessionLabel.text = describeArr[3];
            roleBirthPlaceLabel.text = describeArr[4];
            roleWeaponLabel.text = describeArr[5];
            roleCharacterLabel.text = describeArr[6];
            rolePoemLabel.text = describeArr[7];

//            mGameObjUI.transform.Find("group/SelectRoleOKButton").gameObject.SetActive(strName != "?");

            mGameObjUI.transform.Find("effect_group/ec_ui_mainzhujiao").gameObject.SetActive(strName != "?");
		}
	}

	public override void SetModelBackEffect()
	{
		//GameObject bgScene = GameObject.Find(mStrSceneName);
		//bgScene.transform.Find("Camera/effect_group/ec_ui_status_zhujiao").gameObject.SetActive(true);
	}

//	public override void Close ()
//	{
//		base.Close ();
//
//		if(mGameObjUI != null)
//			GameObject.Destroy(mGameObjUI);
//	}

	public override Data get(string indexName)
    {
        if (indexName == "INPUT_NAME")
        {
			UIInput input = mGameObjUI.GetComponentInChildren<UIInput>();
            if (input != null)
            {
				string strName = input.value;
				if(strName == "")
				{
					strName = input.defaultText;
				}
				return new Data(strName);
            }
            return Data.NULL;
        }

        return base.get(indexName);
    }

	public override void onChange(string keyIndex, object objVal)
	{
		base.onChange(keyIndex, objVal);

		switch (keyIndex)
		{
		case "RANDOM_NAME":
			RandomName();
			break;
		case "SET_BUTTON_STATE":
			SetButtonState((bool)objVal);
			break;
		}
	}
	void SetButtonState(bool isDone)
	{
		if(mGameObjUI != null)
		{
			UIImageButton btn = GameCommon.FindObject(mGameObjUI,"CreateRoleOKButton").GetComponent<UIImageButton>();
			if(btn != null)
				btn.isEnabled = isDone;
		}
	}

	public void RandomName()
	{
		int iRow = DataCenter.mFirstName.GetAllRecord ().Count;
		int iCurRow = RandomRow(iRow);
		string strFirstName = DataCenter.mFirstName.GetData(iCurRow, "FIRSTNAME");
		
		iCurRow = RandomRow(iRow);
		string strMiddleName = DataCenter.mFirstName.GetData(iCurRow, "MIDDLENAME");
		
		iCurRow = RandomRow(iRow);
		string strLastName = DataCenter.mFirstName.GetData(iCurRow, "LASTNAME");
		
		UIInput name = mGameObjUI.transform.Find(mInputNamePath+"input_group/background/sprite/input_name").GetComponent<UIInput>();
		name.value = strFirstName + strMiddleName + strLastName;
	}
	public int RandomRow(int iRow)
	{
		return Random.Range(1000, 1000 + iRow);
	}


	public override void HideAllUIEffect()
	{
		if(mGameObjUI != null)
		{
			if(mGameObjUI != null)
			{
				GameObject effect = mGameObjUI.transform.Find("effect_group/ec_ui_mainzhujiao").gameObject;
				effect.SetActive(false);
			}

			
			GameObject bgScene = GameObject.Find(mStrSceneName);
			if(bgScene != null)
			{
//				bgScene.transform.Find("Camera/effect_group/ec_ui_status_zhujiao").gameObject.SetActive(false);
			}
		}
	}

	public override void InitRoleSel()
	{
		base.InitRoleSel();

		DataCenter.Set("ENTER_GS", false);
		
		DataCenter.CloseWindow("LOGIN_WINDOW");

        if (GameCommon.bIsWindowOpen("LANDING_WINDOW"))
        {
            DataCenter.SetData("LANDING_WINDOW", "REFRESH", true);
            DataCenter.CloseWindow("LANDING_WINDOW");
        }

        if (GameCommon.bIsWindowOpen("SelectRoleUI"))
        {
            DataCenter.SetData("SelectRoleUI", "CLOSE", true);
        }
	}
}

public class Button_RandomNameButton : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.SetData ("SELECT_CREATE_ROLE_WINDOW", "RANDOM_NAME", true);
		return true;
	}
}
public class Button_role_info_chose_button : CEvent
{
	public override bool _DoEvent()
	{
		int iModelIndex = get("ROLE_MODEL_INDEX");
		DataCenter.SetData ("SELECT_CREATE_ROLE_WINDOW", "SET_INDEX", iModelIndex);
		return true;
	}
}


public class Button_CreateRoleOKButton : CEvent
{
	public override bool _DoEvent()
	{
		SelectModelBaseWindow selectRoleWindow = DataCenter.GetData("SELECT_CREATE_ROLE_WINDOW") as SelectModelBaseWindow;
//		SelectModelBaseWindow selectPetWindow = DataCenter.GetData("SELECT_PET_WINDOW") as SelectModelBaseWindow;
		tLogicData winData = DataCenter.GetData("SELECT_CREATE_ROLE_WINDOW");
		string name = winData.get("INPUT_NAME");
		if (IsContainBanedText(name))
		{
			DataCenter.ErrorTipsLabelMessage(STRING_INDEX.ERROR_REGISTRATION_NAME_CONTAIN_BANED_TEXT);
			return false;
		}
        //by chenliang
        //begin

        //判断名称长度是否符合要求
        if(name.Length <= 0)
        {
            DataCenter.ErrorTipsLabelMessage(STRING_INDEX.ERROR_CREATE_ROLE_NAME_CANOT_EMPTY);
            return false;
        }else if (name.Length > 8)
        {
            DataCenter.ErrorTipsLabelMessage(STRING_INDEX.ERROR_CREATE_ROLE_NAME_TOO_LONG);
            return false;
		}else 
		{
			Regex pattern = new Regex(@"[^\u4e00-\u9fa5\w\d]");
			Regex reg2 = new Regex(@"[0-9]");
			if(pattern.IsMatch (name)) 
			{
				DataCenter.ErrorTipsLabelMessage(STRING_INDEX.ERROR_CREATE_ROLE_NAME_NOT_TEXT);
				return false;
			}else if(reg2.Replace (name, "").Length == 0)
			{
				DataCenter.ErrorTipsLabelMessage(STRING_INDEX.ERROR_CREATE_ROLE_NAME_ALL_NUMBER);
				return false;
			}
		}
        //end
		
		LoginNet.RequestCreateRole(selectRoleWindow.mCurModelIndex, name);
		DataCenter.SetData("SELECT_CREATE_ROLE_WINDOW", "SET_BUTTON_STATE", false);
		return true;
		
		tEvent evt = Net.StartEvent("CS_RequestCreateMainRole");
		evt.set("MAIN_MODEL", selectRoleWindow.mCurSelIndex);
//		evt.set("PET_INDEX", selectPetWindow.mCurSelIndex);
		evt.set("NAME", name);
		string filePath = GameCommon.MakeGamePathFileName("Account.info");
		NiceTable t = new NiceTable();
		t.LoadBinary(filePath);
		DataRecord r = t.GetRecord(0);
		if (r != null)
		{
			string account = r["VALUE_1"];
			string password = r["VALUE_2"];
			if (account != "" && password != "")
			{
				evt.set("ACCOUNT", account);
				evt.set("PSWORDMD5", password);
			}
		}
		evt.DoEvent();
		
		DataCenter.SetData("SELECT_CREATE_ROLE_WINDOW", "SET_BUTTON_STATE", false);		
		return true;
	}
	
	bool IsContainBanedText(string chekText)
	{
		foreach(DataRecord r in DataCenter.mBanedTextConfig.Records ())
		{
			string strBanedText = r["BANEDLIST"];
			if(strBanedText != "" && chekText.Contains (strBanedText))
			{
				return true;
			}
		}
		return false;
	}
}

public class SelectCreateNameWindow : tWindow
{
	public float mfCardScale = 1.0f;
	public float mfScaleRatio = 1.0f;

	public override void Init()
	{
		EventCenter.Self.RegisterEvent("Button_select_create_name_back_button", new DefineFactory<Button_SelectCreateNameBackButton>());
		Net.gNetEventCenter.RegisterEvent("CS_RequestCreateMainRole", new DefineFactory<CS_RequestCreateMainRole>());
	}
	
	public override void OnOpen()
	{		
		DataCenter.SetData("SELECT_CREATE_NAME_WINDOW", "SET_BUTTON_STATE", true);
		Refresh (null);
	}

	public override bool Refresh (object param)
	{
        GuideManager.Notify(GuideIndex.SelectName);

		UpdateRole();

		UpdatePet();

		RandomName();

		return base.Refresh (param);
	}

	public void UpdateRole()
	{
		UpdateModelInfo("role_group", "SELECT_CREATE_ROLE_WINDOW");
	}

	public void UpdatePet()
	{
		UpdateModelInfo("pet_group", "SELECT_PET_WINDOW");
	}

	public void RandomName()
	{
		int iRow = DataCenter.mFirstName.GetAllRecord ().Count;
		int iCurRow = RandomRow(iRow);
		string strFirstName = DataCenter.mFirstName.GetData(iCurRow, "FIRSTNAME");

		iCurRow = RandomRow(iRow);
		string strMiddleName = DataCenter.mFirstName.GetData(iCurRow, "MIDDLENAME");

		iCurRow = RandomRow(iRow);
		string strLastName = DataCenter.mFirstName.GetData(iCurRow, "LASTNAME");

        UIInput name = mGameObjUI.transform.Find(SelectCreateRoleWindow.mInputNamePath + "input_group/background/sprite/input_name").GetComponent<UIInput>();
		name.value = strFirstName + strMiddleName + strLastName;
	}

	public int RandomRow(int iRow)
	{
		return Random.Range(1000, 1000 + iRow);
	}

	public int GetModelIndexByWindowName(string strWindowName)
	{
		SelectModelBaseWindow window = DataCenter.GetData(strWindowName) as SelectModelBaseWindow;
		if(window != null)
		{
			return window.mCurModelIndex;
		}

		return -1;
	}

	public int GetIndexByWindowName(string strWindowName)
	{
		SelectModelBaseWindow window = DataCenter.GetData(strWindowName) as SelectModelBaseWindow;
		if(window != null)
		{
			return window.mCurSelIndex;
		}
		
		return -1;
	}

	public void UpdateModelInfo(string strObjName, string strWindowName)
	{
		if(mGameObjUI != null)
		{
			int iModelIndex = GetModelIndexByWindowName(strWindowName);

			Transform groupTrans = mGameObjUI.transform.Find(strObjName);

			SetName(groupTrans, iModelIndex);
			
			InitActiveBirthForUI(groupTrans, iModelIndex);

			if(strObjName == "pet_group")
				SetPetElementUIEffect(groupTrans, GetIndexByWindowName(strWindowName));
		}
	}

	public void SetName(Transform parentTrans, int iModelIndex)
	{
		if(parentTrans != null && iModelIndex > 0)
		{
			UILabel nameLabel = parentTrans.transform.Find("background/name_label").GetComponent<UILabel>();
			nameLabel.text = TableCommon.GetStringFromActiveCongfig(iModelIndex, "NAME");
		}
	}

	public void InitActiveBirthForUI(Transform parentTrans, int iModelIndex)
	{
		if(parentTrans != null && iModelIndex > 0)
		{
			ActiveBirthForUI activeBirthForUI = parentTrans.Find("group/UIPoint").GetComponent<ActiveBirthForUI>();
			if (activeBirthForUI != null)
			{
				int iChildCount = activeBirthForUI.transform.parent.childCount;
				if (iChildCount > 1)
				{
					for (int i = 0; i < iChildCount; i++)
					{
						GameObject obj = activeBirthForUI.transform.parent.GetChild(i).gameObject;
						if (obj == activeBirthForUI.gameObject)
						{
							continue;
						}
						else
						{
							activeBirthForUI.OnDestroy();
						}
					}
				}
				activeBirthForUI.mBirthConfigIndex = iModelIndex;
				
				activeBirthForUI.Init(true, mfScaleRatio * mfCardScale, null);
				// 设置UI
				
			}
		}
		
	}

	public void SetPetElementUIEffect(Transform parentTrans, int iModelIndex)
	{
		if(parentTrans != null)
		{
			for(int i = (int)ELEMENT_TYPE.RED; i <(int)ELEMENT_TYPE.MAX; i++)
			{
				parentTrans.Find("effect_pet_group/ec_ui_main_" + i.ToString()).gameObject.SetActive(i == iModelIndex);
			}
		}
	}

	public override void Close ()
	{
		base.Close ();

		DataCenter.SetData("SELECT_CREATE_NAME_WINDOW", "SET_BUTTON_STATE", true);

		if(mGameObjUI != null)
			GameObject.Destroy(mGameObjUI);
	}
	
	public override Data get(string indexName)
	{
		if (indexName == "INPUT_NAME")
		{
			UIInput input = mGameObjUI.GetComponentInChildren<UIInput>();
			if (input != null)
			{
				string strName = input.value;
				if(strName == "")
				{
					strName = input.defaultText;
				}
				return new Data(strName);
			}
			return Data.NULL;
		}
		
		return base.get(indexName);
	}
	
	public override void onChange(string keyIndex, object objVal)
	{
		base.onChange(keyIndex, objVal);
        if(keyIndex == "RANDOM_NAME")
		{
			RandomName();
		}
		else if(keyIndex == "SET_BUTTON_STATE")
		{
			if(mGameObjUI != null)
			{
				UIImageButton btn = mGameObjUI.transform.Find("CreateRoleOKButton").GetComponent<UIImageButton>();
				if(btn != null)
					btn.isEnabled = (bool)objVal;
			}
		}
	}
}


public class Button_SelectRoleOKButton : CEvent
{
    public override bool _DoEvent()
    {
		DataCenter.CloseWindow("SELECT_CREATE_ROLE_WINDOW");
		DataCenter.SetData("SELECT_CREATE_ROLE_WINDOW", "CLEAR_GRID", true);
		DataCenter.OpenWindow("SELECT_PET_WINDOW");

        return true;
    }
}

public class Button_SelectCreateNameBackButton : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.CloseWindow("SELECT_CREATE_NAME_WINDOW");
		DataCenter.OpenWindow("SELECT_PET_WINDOW");
		
		return true;
	}
}

public enum CREATE_ROLE_TYPE
{
	girl,
	boy,
    high_girl,
    max,
}

public class CS_RequestCreateMainRole : BaseNetEvent
{
    public override void _OnResp(tEvent restEvt)
    {
        restEvt.Dump();

        int result = restEvt.get("INFOTYPE");

        if (result == (int)LOGIN_INFO.eLogin_CreateDBData_Succeed)
        {
            GuideManager.Notify(GuideIndex.NamingSucceed, () =>
                {
                    DataCenter.CloseWindow("SELECT_CREATE_ROLE_WINDOW");

                    tEvent evtEnterGame = Net.StartEvent("CS_QuestEnterGame");
                    evtEnterGame.set("ACCOUNT", (string)get("ACCOUNT"));
                    evtEnterGame.set("PSWORDMD5", (string)get("PSWORDMD5"));
                    evtEnterGame.set("ENTER_GAME", true);
                    evtEnterGame.DoEvent();
                });
		}
		else
		{
			DataCenter.SetData("SELECT_CREATE_NAME_WINDOW", "SET_BUTTON_STATE", true);
			STRING_INDEX error = STRING_INDEX.ERROR_OTHER;
//            string error = "其它错误";
			switch ((LOGIN_INFO)result)
			{
			case LOGIN_INFO.eLogin_Account_Repeat:
				error = STRING_INDEX.ERROR_NAME_EXIST;
//				error = "角色已经存在";
				break;
				
			case LOGIN_INFO.eLogin_Need_PassWord:
				error = STRING_INDEX.ERROR_NEED_PASSWORD;
//				error = "需要正确密码";
				break;
			}
			DataCenter.OpenMessageWindow(error);
//            DataCenter.SetData("LOGIN_WINDOW", "INFO", error);
		}
    }

	public override void DoOverTime()
	{
		base.DoOverTime();
		DataCenter.OpenWindow("SELECT_CREATE_ROLE_WINDOW");
		DataCenter.CloseWindow("SELECT_CREATE_NAME_WINDOW");
	}
}

public class CS_RequestSelPet : BaseNetEvent
{
	public override void _OnResp(tEvent restEvt)
	{
		restEvt.Dump();
		
		int iResult = restEvt.get ("RESULT");
		if ((STRING_INDEX)iResult == STRING_INDEX.ERROR_NONE)
        {
            tEvent evt = Net.StartEvent("CS_RequestRoleData");
            evt.DoEvent();
        }
        else
        {
            DataCenter.OpenMessageWindow((STRING_INDEX)iResult);
        }
	}
}
//-------------------------------------------------------------------------

public class RoleMessageWindow : tWindow
{
    GameObject mWinObject;

    public override void Init()
    {
        mWinObject = GameCommon.FindUI("MessageWindow");
       
        Close();
    }

	public override void Open(object param) { mWinObject.SetActive(true); }
	public override void Close() { mWinObject.SetActive(false); }

    public override void onChange(string key, object value)
    {
        if (key == "TEXT")
        {
            UILabel label = mWinObject.GetComponentInChildren<UILabel>();
            if (label != null)
                label.text = value as string;
            return;
        }

        base.onChange(key, value);
    }

	public override bool IsOpen(){ return mWinObject.activeSelf; }
}


public class InputNameWindow : tWindow
{
    GameObject mWinObject;

    public override void Init()
    {
        mWinObject = GameCommon.FindUI("InputNameWindow");

        Close();
    }

	public override void Open(object param) { mWinObject.SetActive(true); }
	public override void Close() { mWinObject.SetActive(false); }

//    public override Data get(string indexName)
//    {
//        if (indexName == "INPUT_NAME")
//        {
//			UIInput input = mWinObject.GetComponentInChildren<UIInput>();
//            if (input != null)
//            {
//				return new Data(input.value);
//            }
//            return Data.NULL;
//        }
//
//        return base.get(indexName);
//    }

	public override bool IsOpen(){ return mWinObject.activeSelf; }
}


public class Button_LandingPushGoinButton : CEvent
{
	public override bool _DoEvent()
	{
        //维护
        if (LoginNet.msServerState == 5 || LoginNet.msServerState == 4) 
        {
            DataCenter.OpenMessageWindow("服务器正在维护");
            return false;
        }
        else if (LoginNet.msServerState == 6) 
        {
            DataCenter.OpenMessageWindow("服务器已经爆满");
            return false;
        }
        if(LoginNet.msServerState==4||LoginNet.msServerState==5) return false;

        

		GameObject obj = getObject ("BUTTON") as GameObject ;
		GameCommon.SetUIVisiable (obj.transform.parent.gameObject, "landing_label", true);
		obj.SetActive (false);

		DataCenter.Set ("ENTER_GAME_IMMEDIATELY", true);
        DEBUG.Log("CommonParam.bIsNetworkGame:---"+CommonParam.bIsNetworkGame.ToString());
		if(CommonParam.bIsNetworkGame)
		{
            bool bOkEnterGame = DataCenter.Get("ENTER_GS");
            DEBUG.Log("bOkEnterGame:---"+bOkEnterGame.ToString());

            if (bOkEnterGame)
            {
				DataCenter.Set ("ENTER_GAME_IMMEDIATELY", false);

                LoginNet.RequestGameServerLogin();


        
                //tEvent questData = Net.StartEvent("CS_RequestRoleData");
                //questData.DoEvent();
            }
            else
                DataCenter.OpenWindow("LOGIN_WINDOW");
		}
		else
		{
            //close button
            //DataCenter.SetData("LandingWindow", "CLOSE", true);					
			if (!CS_RequestRoleData.ReadRoleFromData(null, null))
			{
				DataCenter.SetData("InputNameWindow", "OPEN", true);
				return false;            
			}
			
			// read pet data
			//CS_RequestRoleData.ReadPetFromData(null);
			
			// read item data		
			//CS_RequestRoleData.ReadGemFromData(null);

			// read map data		
			CS_RequestRoleData.ReadMapFromData(null);

			// read tu jian data		
			CS_RequestRoleData.ReadTujianFromData(null);

			// read task data
			//CS_RequestRoleData.ReadTaskFromData(null, false);
			
            // read role equip data
            CS_RequestRoleData.ReadRoleEquipFromData(null);

			MainProcess.LoadRoleSelScene();
		}
		return true;
	}
}

public class Button_UIForMoMoButton : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.SetData("LandingWindow", "CLOSE", true);
		return true;
	}
}

public class Button_DemoPlayButton : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.SetData("LandingWindow", "CLOSE", true);
		return true;
	}
}

public class Button_InputNameRandomButton : CEvent
{
	public override bool _DoEvent()
	{
		return true;
	}
}

public class Button_MessageOKButton : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.SetData("MessageWindow", "CLOSE", true);
		//DataCenter.SetData("InputNameWindow", "OPEN", true);
		return true;
	}
}

public class Button_gm_Button : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.OpenWindow("GM_WINDOW");
		return true;
	}
}

