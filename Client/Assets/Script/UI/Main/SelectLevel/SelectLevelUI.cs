using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DataTable;
using Logic;

/*
public class SelectLevelUI : MonoBehaviour {

	public GameObject mPlayer;
	public float mScaleRatio = 1.0f;

	void Start ()
	{
//		if(!GameCommon.bIsWindowExist("SELECT_LEVEL_WINDOW"))
//		{
//		}

		DataCenter.Self.registerData("SELECT_LEVEL_WINDOW", new SelectLevelWindow(gameObject));
		DataCenter.SetData("SELECT_LEVEL_WINDOW", "OPEN", true);
	}
	
	void OnDestroy()
	{
			DataCenter.Remove("SELECT_LEVEL_WINDOW");
	}
}

public class SelectLevelWindow : tWindow
{
    private const int STAGE_PER_PAGE = 6;
    private static Dictionary<ELEMENT_TYPE, CustomerState> customerStates = new Dictionary<ELEMENT_TYPE, CustomerState>();

	UIGridContainer mGrid;
	GameObject mSubcellCommon;
	GameObject mSubcellGaoShou;
	GameObject mSubcellMaster;
	GameObject mLevelInfo;

	int mPageNumber = 1;
	int mMaxPageNumber = 1;
	UISprite mSelectTypeIcon;
	NiceTable mStageTable;
	MapLogicData mMapLogicData;
    StagePropertyList mStageList;
    StagePropertyList mStageListInCurrentDifficulty;
    StageProperty mCurrentStageProperty;
	int mCurrentLevelDifficulty = 1;    

	public SelectLevelWindow(GameObject obj)
	{
		mGameObjUI = obj;
		Open(true);
	}
	
	public override void Init()
	{
		Transform scrollview = mGameObjUI.transform.FindChild("Scroll View");
		mGrid = scrollview.FindChild("Grid").GetComponent<UIGridContainer>();
		mSubcellCommon = GameCommon.FindObject (mGrid.gameObject, "subcell_common");
		mSubcellGaoShou = GameCommon.FindObject (mGrid.gameObject, "subcell_gao_shou");
		mSubcellMaster = GameCommon.FindObject (mGrid.gameObject, "subcell_master");
		
		mSelectTypeIcon = GameCommon.FindObject (mGameObjUI, "select_type_icon").GetComponent<UISprite>();
		mLevelInfo = GameCommon.FindObject (mGameObjUI, "level_info");

		mStageTable = TableManager.GetTable("Stage");

		mMapLogicData = DataCenter.GetData("MAP_DATA") as MapLogicData;

        if (CommonParam.mCurrentLevelType == STAGE_TYPE.ACTIVE)
            mStageList = StagePropertyList.Create(STAGE_TYPE.ACTIVE).FindVisible();            
        else
            mStageList = StagePropertyList.Create(STAGE_TYPE.NORMAL, CommonParam.mCurrentLevelElement);

        if (!mStageList.ExistsUnlocked())
        {
            MainUIScript.Self.OpenMainWindowByIndex(MAIN_WINDOW_INDEX.WorldMapWindow);
            return;
        }

        InitUI();
        InitDifficulty();

		if(PetLogicData.mFreindPetData != null) PetLogicData.mFreindPetData = null;
		if(FriendLogicData.mSelectPlayerData != null) FriendLogicData.mSelectPlayerData = null;
	}

    private void InitUI()
    {
        SetTitle();
        InitStageCell();
    }

	public void SetTitle()
	{
		string strTitle = "";

        if (CommonParam.mCurrentLevelType == STAGE_TYPE.ACTIVE)
        {
            strTitle = "ui_smalltitle_gkxx";
            SetTitleButtonActive(false);
        }
        else if (CommonParam.mCurrentLevelElement == ELEMENT_TYPE.RED)
		{
			strTitle = "ui_smalltitle_hzj";
            SetTitleButtonActive(true);
		}
        else if (CommonParam.mCurrentLevelElement == ELEMENT_TYPE.BLUE)
		{
			strTitle = "ui_smalltitle_szj";
            SetTitleButtonActive(true);
		}
        else if (CommonParam.mCurrentLevelElement == ELEMENT_TYPE.GREEN)
		{
			strTitle = "ui_smalltitle_mzj";
            SetTitleButtonActive(true);
		}
        else if (CommonParam.mCurrentLevelElement == ELEMENT_TYPE.GOLD)
		{
			strTitle = "ui_smalltitle_gzj";
            SetTitleButtonActive(true);
		}
        else if (CommonParam.mCurrentLevelElement == ELEMENT_TYPE.SHADOW)
		{
			strTitle = "ui_smalltitle_azj";
            SetTitleButtonActive(true);
		}

		mSelectTypeIcon.spriteName  = strTitle;
	}

    private void SetTitleButtonActive(bool active)
    {
        SetVisible("common_level_button", active);
        SetVisible("gao_shou_level_button", active);
        SetVisible("master_level_button", active);
    }

    private void SetFightButtonActive(bool active)
    {
        GameObject button = GetSub("button_fight");
        button.GetComponent<UIImageButton>().isEnabled = active;
        GameCommon.SetUIVisiable(button, "button_shadow", !active);
    }

    private void InitStageCell()
    {
        if (CommonParam.mCurrentLevelType == STAGE_TYPE.ACTIVE)
        {
            mGrid.controlTemplate = mSubcellGaoShou;
            GameCommon.SetUIVisiable(mSubcellGaoShou, "time_count_down", true);
        }
        else
        {
            mGrid.controlTemplate = mSubcellCommon;
            GameCommon.SetUIVisiable(mSubcellGaoShou, "time_count_down", false);
        }
    }


	public override bool Refresh(object param)
	{
        SetBackAndForwardButton();

        StagePropertyList mStageList = mStageListInCurrentDifficulty.GetByPage(mPageNumber, STAGE_PER_PAGE);//GetCurrentPageStagePropertyList();
		mGrid.MaxCount = mStageList.Count;

		for (int i = 0; i < mGrid.MaxCount; i++)
		{
			StageProperty stageProperty = mStageList[i];
			GameObject obj = mGrid.controlList[i];
            SetStageIcon(obj, stageProperty);
		}

        SetCurrentToggle(mGrid, mStageList);
        SaveCustomerState();
		return true;
	}

    public void RefreshOnStageTimeout(object param)
    {
        mStageListInCurrentDifficulty = mStageListInCurrentDifficulty.FindVisible();
        GuideManager.ExecuteDelayed(() => Refresh(null), 0f);
    }

    private void SetBackAndForwardButton()
    {
        mMaxPageNumber = GetMaxPageNumber();
        GameCommon.SetUIVisiable(mGameObjUI, "level_back_button", true);
        GameCommon.SetUIVisiable(mGameObjUI, "level_forward_button", true);

        if (mPageNumber == 1) 
            GameCommon.SetUIVisiable(mGameObjUI, "level_back_button", false);
        if (mPageNumber == mMaxPageNumber) 
            GameCommon.SetUIVisiable(mGameObjUI, "level_forward_button", false);
    }

    private void SetStageIcon(GameObject obj, StageProperty property)
    {
        GameObject buttonLevelInfoObj = obj.transform.FindChild("button_level_info").gameObject;
        SetLevelButton(buttonLevelInfoObj, property.mIndex);

        //string strWhichLevel = property.mIndex.Remove(0, property.mIndex.Length - 2);
        GameCommon.SetUIText(obj, "which_level_label", StageProperty.GetStageLevel(property.mIndex).ToString());

        int iLevelIcon = TableCommon.GetNumberFromStageConfig(property.mIndex, "HEADICON");
        GameCommon.SetPetIcon(obj, iLevelIcon, "level_icon");

        GameCommon.SetUIVisiable(obj, "element", CommonParam.mCurrentLevelType != STAGE_TYPE.ACTIVE);
        GameCommon.SetUIVisiable(obj, "not_through_background", !property.unlocked);
        GameCommon.SetUIVisiable(obj, "level_through_icon", property.passed);
        GameCommon.SetUIVisiable(obj, "time_count_down", !property.mOpenTime.mOpenForever);

        if (CommonParam.mCurrentLevelType != STAGE_TYPE.ACTIVE)
            GameCommon.SetElementIcon(obj, "element", Convert.ToInt32(CommonParam.mCurrentLevelElement));

        if (property.mFightLimit > 0)
        {
            int leftCount = Mathf.Max(0, property.mFightLimit - property.mFightCount);
            GameCommon.SetUIText(obj, "num_label", leftCount + "次");
        }
        else
        {
            GameCommon.SetUIText(obj, "num_label", "∞");
        }

        if (!property.openForever)
        {
            Int64 overTime = property.closingTime;         
            CallBack timeoutCallback = new CallBack(this, "RefreshOnStageTimeout", null);
            SetCountdownTime(obj, "time_count_down", overTime, timeoutCallback);
        }
    }

    private void SetCurrentPage()
    {
        CustomerState state = GetCustomerState();
        StagePropertyList list = mStageListInCurrentDifficulty.GetByPage(state.page, STAGE_PER_PAGE);

        if (list.ExistsUnlocked())
            mPageNumber = state.page;
        else
            mPageNumber = 1;
    }

    private void SetCurrentToggle(UIGridContainer container, StagePropertyList stageList)
    {
        StageProperty currentProperty = null;
        GameObject currentToggle = null;

        for (int i = 0; i < container.controlList.Count; ++i)
        {
            GameObject buttonLevelInfoObj = GameCommon.FindObject(container.controlList[i], "button_level_info");
            GameCommon.ToggleFalse(buttonLevelInfoObj);
        }

        int index =stageList.GetIndexOfMostPriority();

        if (index >= 0 && stageList[index].unlocked)
        {
            currentToggle = GameCommon.FindObject(container.controlList[index], "button_level_info");
            currentProperty = stageList[index];
        }

        SetCurrentProperty(currentProperty);

        if (currentToggle != null)
        {
            GameCommon.ToggleTrue(currentToggle);
        }
    }

    private void SetCurrentProperty(StageProperty property)
    {
        mCurrentStageProperty = property;
        InitButton();
        InitLevelInfo();

        if (property != null)
        {
            DataCenter.Set("CURRENT_STAGE", property.mIndex);
        }
    }

    private void InitButton()
    {
        SetFightButtonActive(mCurrentStageProperty != null);

        NiceData fightBtnData = GameCommon.GetButtonData(mGameObjUI, "button_fight");
        fightBtnData.set("STAGE_PROPERTY", mCurrentStageProperty);
       
//        NiceData friendHelpStartBtnData = GameCommon.GetButtonData(mFriendHelpWindow, "friend_help_start_button");
//        friendHelpStartBtnData.set("STAGE_PROPERTY", mCurrentStageProperty);
    }

	bool SetLevelButton(GameObject button, int iLevelIndex)
	{
		if (button != null)
		{
			UIButtonEvent buttonEvent = button.GetComponent<UIButtonEvent>();
			if (buttonEvent != null)
			{
				buttonEvent.mData.set("LEVEL_INDEX", iLevelIndex);
				return true;
			}
		}
		return false;
	}

    private void InitDifficulty()
    {
        CustomerState state = GetCustomerState();
        StagePropertyList list = mStageList.FindByDifficuty(state.difficulty);

        if (list.ExistsUnlocked())
        {
            SelectDifficulty(state.difficulty);
            ToggleDifficulty(state.difficulty);
        }
        else
        {
            state.page = 1;

            if (CommonParam.mCurrentLevelType == STAGE_TYPE.ACTIVE)
            {
                SelectDifficulty(0);
            }
            else
            {
                SelectDifficulty(1);
                ToggleDifficulty(1);
            }
        }
    }

    private void ToggleDifficulty(int difficulty)
    {
        GameObject top = GetSub("top_parent");
        GameObject commonBtn = GameCommon.FindObject(top, "common_level_button");
        GameObject gaoshouBtn = GameCommon.FindObject(top, "gao_shou_level_button");
        GameObject masterBtn = GameCommon.FindObject(top, "master_level_button");
        GameCommon.ToggleFalse(commonBtn);
        GameCommon.ToggleFalse(gaoshouBtn);
        GameCommon.ToggleFalse(masterBtn);

        switch (difficulty)
        {
            case 1:
                GameCommon.ToggleTrue(commonBtn);
                break;
            case 2:
                GameCommon.ToggleTrue(gaoshouBtn);
                break;
            case 3:
                GameCommon.ToggleTrue(masterBtn);
                break;
        }
    }

	public void SelectDifficulty(int difficulty)
	{
        mCurrentLevelDifficulty = difficulty;
        mStageListInCurrentDifficulty = mStageList.FindByDifficuty(mCurrentLevelDifficulty); //GetCurrentValidStagePropertyList();
        SetCurrentPage();
        
		mGrid.MaxCount = 0;

        if (difficulty == 1)
		{
			mGrid.controlTemplate = mSubcellCommon;
		}
        if (difficulty == 2)
		{
			mGrid.controlTemplate = mSubcellGaoShou;
		}
        if (difficulty == 3)
		{
			mGrid.controlTemplate = mSubcellMaster;
		}
		
		Refresh (null);
		return;
	}

    public void InitLevelInfo()
	{
        GameCommon.SetUIVisiable(mLevelInfo, "level_difficulty", mCurrentStageProperty != null);
        GameCommon.SetUIVisiable(mLevelInfo, "level_name", mCurrentStageProperty != null);
        GameCommon.SetUIVisiable(mLevelInfo, "level_describe", mCurrentStageProperty != null);
        GameCommon.SetUIVisiable(mLevelInfo, "recomment_element_icon", mCurrentStageProperty != null);
        GameCommon.SetUIVisiable(mLevelInfo, "items_drop_grid", mCurrentStageProperty != null);

        if (mCurrentStageProperty == null)
            return;

		if(mCurrentLevelDifficulty == 1)
		{
			GameCommon.SetUIText (mLevelInfo, "level_difficulty", "(普通)");
		}
		if(mCurrentLevelDifficulty == 2)
		{
			GameCommon.SetUIText (mLevelInfo, "level_difficulty", "(高手)");
		}
		if(mCurrentLevelDifficulty == 3)
		{
			GameCommon.SetUIText (mLevelInfo, "level_difficulty", "(大师)");
		}

        string strLevelName = TableCommon.GetStringFromStageConfig(mCurrentStageProperty.mIndex, "NAME");
        string strLevelDescribe = TableCommon.GetStringFromStageConfig(mCurrentStageProperty.mIndex, "NAME");
		GameCommon.SetUIText (mLevelInfo, "level_name", strLevelName);
		GameCommon.SetUIText (mLevelInfo, "level_describe", strLevelDescribe);

        int iElementIndex = TableCommon.GetNumberFromStageConfig(mCurrentStageProperty.mIndex, "STATUS");
		GameCommon.SetElementIcon(mLevelInfo, "recomment_element_icon", iElementIndex);

		UIGridContainer itemsDropGrid = GameCommon.FindObject (mLevelInfo, "items_drop_grid").GetComponent<UIGridContainer>();
		itemsDropGrid.MaxCount = 3;
		for(int i = 0; i < itemsDropGrid.MaxCount; i++)
		{
            int iItemType = TableCommon.GetNumberFromStageConfig(mCurrentStageProperty.mIndex, "AWARD_TYPE_" + (i + 1).ToString());
            int iItemIndex = TableCommon.GetNumberFromStageConfig(mCurrentStageProperty.mIndex, "AWARD_" + (i + 1).ToString());
			GameObject obj = itemsDropGrid.controlList[i];

			if(iItemType == 0)
			{
				GameCommon.SetPetIconWithElementAndStar(obj, "item_icon", "element", "star_level_label", iItemIndex);
			}
			else 
			{
				GameCommon.SetUIVisiable (obj, "element", false);
				GameCommon.SetUIVisiable (obj, "star_level_label", false);

				UISprite itemSprite = GameCommon.FindObject (obj, "item_icon").GetComponent<UISprite>();
				GameCommon.SetItemIcon (itemSprite, iItemType, iItemIndex);
			}
		}
	}

	public override void onChange (string keyIndex, object objVal)
	{
		base.onChange(keyIndex, objVal);
		 
		switch (keyIndex)
		{
		case "BACK_AND_FORWARD":
			mPageNumber += Convert.ToInt32 (objVal);
			mGrid.MaxCount = 0;
			Refresh (null);
			break;
		case "SELECT_DIFFICULTY":
            SelectDifficulty(Convert.ToInt32(objVal));
			break;
		case "INIT_LEVEL_INFO":
			SelectLevel((int)objVal);
			break;
		
//        case "REQUEST_FRIEND":
//            RequestFriend();
//            break;
		
		}
	}

//    private void OnReceiveFromMessageBox()
//    {
//        RequestFriend();
//    }

//    private void RequestFriend()
//    {
//        tEvent evt = Net.StartEvent("CS_RequestFriendList");
//		  evt.set("IS_GAME_FRIEND_LIST", false);
//        evt.DoEvent();
//    }

    private int GetMaxPageNumber()
    {
        return GetPageNumber(mStageListInCurrentDifficulty.Count + 1);
    }

    private int GetPageNumber(int indexOfList)
    {
        return indexOfList / STAGE_PER_PAGE + 1;
    }

    private StageProperty GetStageProperty(int index)
    {
        return mStageListInCurrentDifficulty.Find(x => x.mIndex == index);
    }

    private void SelectLevel(int index)
    {
        SetCurrentProperty(GetStageProperty(index));
    }

    private CustomerState GetCustomerState()
    {
        CustomerState state = null;

        if (!customerStates.TryGetValue(CommonParam.mCurrentLevelElement, out state))
        {
            state = new CustomerState();
            state.difficulty = CommonParam.mCurrentLevelType == STAGE_TYPE.ACTIVE ? 0 : 1;
            state.page = 1;
            customerStates.Add(CommonParam.mCurrentLevelElement, state);
        }

        return state;
    }

    private void SaveCustomerState()
    {
        CustomerState state = GetCustomerState();
        state.difficulty = mCurrentLevelDifficulty;
        state.page = mPageNumber;
    }

    private class CustomerState
    {
        public int difficulty = 1;
        public int page = 1;
    }
}


//----------------------------------------------------------------------------------
// SelectLevelWindow
public class Button_select_level_back : CEvent
{
	public override bool _DoEvent()
	{
		MainUIScript.Self.OpenMainWindowByIndex(MAIN_WINDOW_INDEX.WorldMapWindow);
		return true;
	}
}

//public class Button_button_fight : CEvent
//{
//	public override bool _DoEvent()
//	{
//        MainProcess.RequestBattle(() => DataCenter.SetData("SELECT_LEVEL_WINDOW", "REQUEST_FRIEND", null));
//		return true;
//	}
//}

public class Button_level_back_button : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.SetData("SELECT_LEVEL_WINDOW","BACK_AND_FORWARD", -1);
		return true;
	}
}

public class Button_level_forward_button : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.SetData("SELECT_LEVEL_WINDOW", "BACK_AND_FORWARD", 1);
		return true;
	}
}

public class Button_common_level_button : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.SetData ("SELECT_LEVEL_WINDOW", "SELECT_DIFFICULTY", 1);
		return true;
	}
}

public class Button_gao_shou_level_button : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.SetData ("SELECT_LEVEL_WINDOW", "SELECT_DIFFICULTY", 2);
		return true;
	}
}

public class Button_master_level_button : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.SetData ("SELECT_LEVEL_WINDOW", "SELECT_DIFFICULTY", 3);
		return true;
	}
}

public class Button_button_level_info : CEvent
{
	public override bool _DoEvent()
	{
		int iLevelIndex = get ("LEVEL_INDEX");
		DataCenter.Set("CURRENT_STAGE", iLevelIndex);
        DataCenter.SetData("SELECT_LEVEL_WINDOW", "INIT_LEVEL_INFO", iLevelIndex);
		return true;
	}
}

*/

