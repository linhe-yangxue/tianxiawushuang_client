using UnityEngine;
using System;
using System.Collections;
using Logic;
using System.Collections.Generic;
using DataTable;

public class IllustratedHandbookWindow : tWindow {

    public List<UIGridContainer> mGridList = new List<UIGridContainer>();

    TujianLogicData mLogicData = null;

    ELEMENT_TYPE mCurElementType = ELEMENT_TYPE.RED;

	public GameObject mAllBtn;

	public override void Init()
	{
        EventCenter.Self.RegisterEvent("Button_pet_identify_window_back_button", new DefineFactory<IllustratedHandbookWindowBack>());
        EventCenter.Self.RegisterEvent("Button_illustrated_handbook_shop_btn", new DefineFactory<Button_IllustratedHandbookShopBtn>());

        EventCenter.Self.RegisterEvent("Button_button_0", new DefineFactory<ElementTypeEvent>());
		EventCenter.Self.RegisterEvent("Button_button_1", new DefineFactory<ElementTypeEvent>());
		EventCenter.Self.RegisterEvent("Button_button_2", new DefineFactory<ElementTypeEvent>());
		EventCenter.Self.RegisterEvent("Button_button_3", new DefineFactory<ElementTypeEvent>());
		EventCenter.Self.RegisterEvent("Button_button_4", new DefineFactory<ElementTypeEvent>());

		EventCenter.Self.RegisterEvent("Button_item_icon", new DefineFactory<llustratedHandbookPetIconBtn>());

		EventCenter.Self.RegisterEvent("Button_tujian_attack", new DefineFactory<TujianAnimateEvent>());
		EventCenter.Self.RegisterEvent("Button_tujian_stun", new DefineFactory<TujianAnimateEvent>());
		EventCenter.Self.RegisterEvent("Button_tujian_run", new DefineFactory<TujianAnimateEvent>());
		EventCenter.Self.RegisterEvent("Button_tujian_win", new DefineFactory<TujianAnimateEvent>());

		EventCenter.Self.RegisterEvent("Button_tujian_award_btn", new DefineFactory<Button_TujianAwardBtn>());
	}

    public void InitVariable()
    {
        mGridList.Clear();
        for (int i = 0; i < 5; i++)
        {
            UIGridContainer grid = mGameObjUI.transform.Find("pet_identify_window/TabContextPanel/ScrollviewContext_" + i.ToString() + "/ScrollView/Grid").GetComponent<UIGridContainer>();
			grid.MaxCount = 0;
            mGridList.Add(grid);
        }

        mLogicData = DataCenter.GetData("TUJIAN_DATA") as TujianLogicData;

        mCurElementType = ELEMENT_TYPE.RED;

		mAllBtn = mGameObjUI.transform.Find("pet_identify_window/allbutton").gameObject;
		GameCommon.ToggleTrue(mAllBtn.transform.Find("button_" + ((int)mCurElementType).ToString()).gameObject);

		// set cur_gain_number_label
		SetCurGainNumberLabel();

		SetAllBtnNewNum();

    }

	public override void Open (object param)
	{
		base.Open (param);
		DataCenter.OpenWindow ("BACK_GROUP_ILLUSTRATED_WINDOW");
	}

	public override void Close ()
	{
		base.Close ();
		DataCenter.CloseWindow ("BACK_GROUP_ILLUSTRATED_WINDOW");
	}

    public override void OnOpen()
    {
        base.OnOpen();

        // init 
        InitVariable();

        Refresh(null);
    }

    public override void onChange(string keyIndex, object objVal)
    {
        base.onChange(keyIndex, objVal);
        switch(keyIndex)
        {
            case "SHOW_WINDOW":
				mCurElementType = (ELEMENT_TYPE)((int)objVal);
                ShowPetGrid((int)objVal);
                break;
			case "REFRESH_GRID_ITEM_INFO":
				int iGridIndex = (int)get ("GRID_INDEX");
				int iModelIndex = (int)get ("MODEL_INDEX");
				TujianData tujianData = mLogicData.GetTujianDataByModelIndex(iModelIndex);
				tujianData.mStatus = (int)objVal;

                GameObject numIcon = mAllBtn.transform.Find("button_" + ((int)mCurElementType).ToString() + "/num_icon").gameObject;
                GameObject awardGroup = mAllBtn.transform.Find("button_" + ((int)mCurElementType).ToString() + "/award_group").gameObject;
                SetAllBtnNewNum();
				SetGridItemInfo((int)mCurElementType, iGridIndex, iModelIndex);
				break;

        }
    }

	public void SetCurGainNumberLabel()
	{
		UILabel curGainNumberLabel = mGameObjUI.transform.Find("pet_identify_window/cur_gain_number_label").GetComponent<UILabel>();
		UISlider bar = GameCommon.FindObject (mGameObjUI, "cur_gain_progress_bar").GetComponent<UISlider>();
		int iCurPetNum = mLogicData.mTujianList.Length;
		int iTotalPetNum = GetMaxCount();
		curGainNumberLabel.text = iCurPetNum.ToString() + "/" + iTotalPetNum.ToString() + "(" + ((int)(iCurPetNum * 100.0f/iTotalPetNum)).ToString() + "%)";
		float petRate = (float)iCurPetNum/(float)iTotalPetNum ;
		bar.value = petRate;
	}
    public override bool Refresh(object param)
    {
        ShowPetGrid((int)mCurElementType);

        return base.Refresh(param);
    }

	public void SetAllBtnNewNum()
	{
        int iTotalNewCount = 0;
        int iTotalRewardCount = 0;
        for (int i = 0; i < mGridList.Count; i++)
        {
            SetBtnNewNum(i, ref iTotalNewCount, ref iTotalRewardCount);
        }

        RoleLogicData.Self.mbHaveNewTuJian = iTotalNewCount > 0;
        RoleLogicData.Self.mbHaveAwardTuJian = iTotalRewardCount > 0;
	}

    public void SetBtnNewNum(int iElementType, ref int iTotalNewCount, ref int iTotalRewardCount)
	{
		if (iElementType >= (int)ELEMENT_TYPE.RED && iElementType <= (int)ELEMENT_TYPE.SHADOW)
		{
            int iNewCount = 0;
            int iRewardCount = 0;
			foreach (KeyValuePair<int, DataRecord> pair in DataCenter.mActiveConfigTable.GetAllRecord())
			{
				if ((int)pair.Value.get("INDEX") > 0 
				    && (string)pair.Value.get("CLASS") == "PET" 
				    && (int)pair.Value.get("ELEMENT_INDEX") == iElementType
                    && (int)pair.Value.get("IS_TUJIAN_SHOW") == 1)
				{
					// new num
					int iModelIndex = pair.Key;
					TujianData tujianData = mLogicData.GetTujianDataByModelIndex(iModelIndex);
					if(tujianData != null)
					{
						if(tujianData.mStatus == (int)TUJIAN_STATUS.TUJIAN_NEW)
						{
                            iNewCount++;
						}
                        if (tujianData.mStatus == (int)TUJIAN_STATUS.TUJIAN_REWARD)
                        {
                            iRewardCount++;
                        }
					}
				}
			}

            iTotalNewCount += iNewCount;
            iTotalRewardCount += iRewardCount;            
			GameObject numIcon = mAllBtn.transform.Find("button_" + iElementType.ToString() + "/num_icon").gameObject;
            GameObject awardGroup = mAllBtn.transform.Find("button_" + iElementType.ToString() + "/award_group").gameObject;
			if(numIcon != null)
			{
                numIcon.SetActive(iRewardCount <= 0 && iNewCount > 0);
                awardGroup.SetActive(iRewardCount > 0);
				UILabel newNumLabel = numIcon.transform.Find ("num_label").GetComponent<UILabel>();
				if(newNumLabel != null)
				{
                    newNumLabel.text = iNewCount.ToString();
				}
			}
		}
	}

    public void ShowPetGrid(int iElementType)
    {
        for (int i = 0; i < mGridList.Count; i++)
        {
            bool bIsVisible = (i == iElementType);
            mGridList[i].transform.parent.parent.gameObject.SetActive(bIsVisible);

            if (bIsVisible && mGridList[i].MaxCount == 0)
            {
                // add item
                RefreshPetGrid(i);
            }
        }
    }

    public void RefreshPetGrid(int iElementType)
    {
        if (iElementType >= (int)ELEMENT_TYPE.RED && iElementType <= (int)ELEMENT_TYPE.SHADOW)
        {
            UIGridContainer grid = mGridList[iElementType];
            if (grid != null)
            {
                grid.MaxCount = GetMaxCountByElementType(iElementType);
                int iCount = 0;
				int iNewCount = 0;
                foreach (KeyValuePair<int, DataRecord> pair in DataCenter.mActiveConfigTable.GetAllRecord())
                {
					if ((int)pair.Value.get("INDEX") > 0 
					    && (string)pair.Value.get("CLASS") == "PET" 
					    && (int)pair.Value.get("ELEMENT_INDEX") == iElementType
                        && (int)pair.Value.get("IS_TUJIAN_SHOW") == 1)
                    {
						int iModelIndex = pair.Key;
						SetGridItemInfo(iElementType, iCount, iModelIndex);
                        iCount++;
                    }
                }
            }
        }
        
    }

    public int GetMaxCountByElementType(int iElementType)
    {
        int iCount = 0;
        foreach (KeyValuePair<int, DataRecord> pair in DataCenter.mActiveConfigTable.GetAllRecord())
        {
			if ((int)pair.Value.get("INDEX") > 0 
			    && (string)pair.Value.get("CLASS") == "PET" 
			    && (int)pair.Value.get("ELEMENT_INDEX") == iElementType
                && (int)pair.Value.get("IS_TUJIAN_SHOW") == 1)
                iCount++;
        }
        return iCount;    
    }

	public int GetMaxCount()
	{
		int iCount = 0;
		foreach (KeyValuePair<int, DataRecord> pair in DataCenter.mActiveConfigTable.GetAllRecord())
		{
			if ((int)pair.Value.get("INDEX") > 0 
			    && (string)pair.Value.get("CLASS") == "PET"
                && (int)pair.Value.get("IS_TUJIAN_SHOW") == 1)
				iCount++;
		}
		return iCount;    
	}
	
	public void SetGridItemInfo(int iElementType, int iGridIndex, int iModelIndex)
    {
		GameObject petItem = mGridList[iElementType].controlList[iGridIndex].transform.Find("item_icon").gameObject;
        if (petItem != null)
        {
            //// set pet icon
            //GameCommon.SetPetIcon(petItem.transform.parent.gameObject, iModelIndex, "pet_icon");
            //
            //// set star level
            //GameCommon.SetStarLevelLabel(petItem, TableCommon.GetNumberFromActiveCongfig(iModelIndex, "STAR_LEVEL"), "star_level_label");
            GameCommon.SetItemIcon(mGridList[iElementType].controlList[iGridIndex], new ItemData() { mType = (int)ITEM_TYPE.PET, mID = iModelIndex });
            // set pet state
            SetPetState(petItem, iModelIndex);

			UIButtonEvent petBtnEvent = petItem.GetComponent<UIButtonEvent>();
			petBtnEvent.mData.set("GRID_INDEX", iGridIndex);
			petBtnEvent.mData.set("MODEL_INDEX", iModelIndex);
        }
    }

    public void SetPetState(GameObject petItem, int iModelIndex)
    {
        if (petItem == null)
            return;

        GameObject stateNewSprite = GameCommon.FindObject(petItem, "state_new_sprite");
        GameObject stateAwardSprite = GameCommon.FindObject(petItem, "state_award_sprite");
        GameObject stateReceivedSprite = GameCommon.FindObject(petItem, "state_received_sprite");
        GameObject stateUnactiveSprite = GameCommon.FindObject(petItem, "state_unactive_sprite");
		stateUnactiveSprite.SetActive(false);

        TujianData tujianData = mLogicData.GetTujianDataByModelIndex(iModelIndex);
        TUJIAN_STATUS tujianState = TUJIAN_STATUS.TUJIAN_NOTHAD;
        if(tujianData != null)
        {
            tujianState = (TUJIAN_STATUS)tujianData.mStatus;
        }
        switch (tujianState)
        {
            case TUJIAN_STATUS.TUJIAN_NOTHAD:
                stateNewSprite.SetActive(false);
                stateAwardSprite.SetActive(false);
                stateReceivedSprite.SetActive(false);
//				stateUnactiveSprite.SetActive(true);
				SetGameObjectGreyColor (petItem, 60f/255);
//				petItem.GetComponent<UIWidget>().color = Color.green;
                break;
            case TUJIAN_STATUS.TUJIAN_NEW:
                stateNewSprite.SetActive(true);
                stateAwardSprite.SetActive(false);
                stateReceivedSprite.SetActive(false);
//              stateUnactiveSprite.SetActive(false);
				SetGameObjectGreyColor (petItem, 1f);
                break;
            case TUJIAN_STATUS.TUJIAN_NORMAL:
                stateNewSprite.SetActive(false);
                stateAwardSprite.SetActive(false);
                stateReceivedSprite.SetActive(false);
//              stateUnactiveSprite.SetActive(false);
				SetGameObjectGreyColor (petItem, 1f);
                break;
            case TUJIAN_STATUS.TUJIAN_REWARD:
                stateNewSprite.SetActive(false);
                stateAwardSprite.SetActive(true);
                stateReceivedSprite.SetActive(false);
//              stateUnactiveSprite.SetActive(false);
				SetGameObjectGreyColor (petItem, 1f);
                break;
            case TUJIAN_STATUS.TUJIAN_FULL:
                stateNewSprite.SetActive(false);
                stateAwardSprite.SetActive(false);
                stateReceivedSprite.SetActive(true);
//				stateUnactiveSprite.SetActive(false);
				SetGameObjectGreyColor (petItem, 1f);
                petItem.GetComponent<UIWidget>().color = Color.white;
                break;
        }
    }

	void SetGameObjectGreyColor(GameObject obj, float fColor)
	{
		UISprite[] sprites = obj.GetComponentsInChildren<UISprite>();
		foreach(UISprite s in sprites)
		{
			if(s != null) 
				s.color = new Color(fColor, fColor, fColor);
		}
	}
}

public class Button_IllustratedHandbookBtn : CEvent
{
	public override bool _DoEvent()
	{
		if(CommonParam.bIsNetworkGame)
		{
			tEvent questData = Net.StartEvent("CS_RequestTujian");
			questData.DoEvent();
            MainUIScript.Self.OpenMainUI();
            DataCenter.OpenWindow("ILLUSTRATED_HANDBOOK_WINDOW");
		}
		else
		{
			MainUIScript.Self.OpenMainUI();
			DataCenter.OpenWindow("ILLUSTRATED_HANDBOOK_WINDOW");
		}
		return true;
	}
}

public class Button_IllustratedHandbookShopBtn : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.CloseWindow("ILLUSTRATED_HANDBOOK_WINDOW");
		MainUIScript.Self.OpenMainWindowByIndex(MAIN_WINDOW_INDEX.ShopWindow);
		return true;
	}
}

public class IllustratedHandbookWindowBack : CEvent
{
	public override bool _DoEvent()
    {
        RoleLogicData.Self.mbHaveNewTuJian = false;
        DataCenter.CloseWindow("ILLUSTRATED_HANDBOOK_WINDOW");
		MainUIScript.Self.OpenMainWindowByIndex(MAIN_WINDOW_INDEX.RoleSelWindow);
		//PetAtlasWindowCenter center = DataCenter.GetData("PetAtlasViewCenter") as PetAtlasWindowCenter;
		//center.mGameObjUI.SetActive(false);
		return true;
	}
}

public class ElementTypeEvent : CEvent
{
	public override bool _DoEvent()
	{
		string[] names = GetEventName().Split('_');
        DataCenter.SetData("ILLUSTRATED_HANDBOOK_WINDOW", "SHOW_WINDOW", int.Parse(names[names.Length - 1]));
		return true;
	}
}

public class llustratedHandbookPetIconBtn : CEvent
{
	public override bool _DoEvent ()
	{
		int iGirdIndex = get("GRID_INDEX");
		int iModelIndex = get("MODEL_INDEX");

		DataCenter.SetData("ILLUSTRATED_HANDBOOK_WINDOW", "GRID_INDEX", iGirdIndex);
		DataCenter.SetData("ILLUSTRATED_HANDBOOK_WINDOW", "MODEL_INDEX", iModelIndex);

		DataCenter.OpenWindow("PET_INFO_SINGLE_WINDOW", PET_INFO_WINDOW_TYPE.TUJIAN);
		DataCenter.SetData("PET_INFO_SINGLE_WINDOW", "SET_SELECT_PET_BY_MODEL_INDEX", iModelIndex);
		return true;
	}
}

public class TujianAnimateEvent : CEvent
{
	public override bool _DoEvent()
	{
//		string[] names = GetEventName().Split('_');
//		DataCenter.SetData("PET_INFO_SINGLE_WINDOW", "PLAY_ANIM", names[names.Length - 1]);

		DataCenter.SetData("PET_INFO_SINGLE_WINDOW", "PLAY_ANIM_SEQUENCE", true);
		return true;
	}
}

public class Button_TujianAwardBtn : CEvent
{
	public override bool _DoEvent()
	{
		if(CommonParam.bIsNetworkGame)
		{
			tEvent questData = Net.StartEvent("CS_TujianReward");
			PetInfoSingleWindow win = DataCenter.GetData("PET_INFO_SINGLE_WINDOW") as PetInfoSingleWindow;
			int iModelIndex = win.get ("SET_SELECT_PET_BY_MODEL_INDEX");
			questData.set ("PET_ID", iModelIndex);
			questData.DoEvent();
		}
		return true;
	}
}