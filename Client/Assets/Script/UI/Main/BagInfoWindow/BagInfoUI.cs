using UnityEngine;
using System.Collections;
using DataTable;
using System.Collections.Generic;
using System;
using System.Linq;
using Logic;

public enum GEM_TYPE
{
	RED_SMALL,
	RED_MIDDLE,
	RED_LARGE,
	BLUE_SMALL,
	BLUE_MIDDLE,
	BLUE_LARGE,
	GREEN_SMALL,
	GREEN_MIDDLE,
	GREEN_LARGE,
	GOLD_SMALL,
	GOLD_MIDDLE,
	GOLD_LARGE,
	SHADOW_SMALL,
	SHADOW_MIDDLE,
	SHADOW_LARGE,
	CHAOS_SMALL,
	CHAOS_MIDDLE,
	CHAOS_LARGE,
	GEM_MAX,
}

public enum BAG_INFO_TITLE_TYPE
{
    Bag_Pet_Window_SpriteTitle,
    Bag_Pet_Window_ButtonTitle,
    Bag_Stone_Window_ButtonTitle,
    Bag_Stone_Window,
    RoleEquipWindow,
    PetEquipWindow,
}

public class BagInfoUI : MonoBehaviour {

	public GameObject mTitle;
	public GameObject mPetAndStoneSelBtn;
	public GameObject mPetGroup;
	public GameObject mStoneGroup;
    public GameObject mRoleEquipWindow;
    public GameObject mPetEquipWindow;

	// Use this for initialization
	void Awake () {
		//DataCenter.Self.registerData("BAG_INFO_WINDOW", new BagInfoWindow());
        DataCenter.Self.registerData("RoleEquipWindow", new RoleEquipWindow(GameCommon.FindObject(gameObject, "RoleEquipWindow")));
		DataCenter.SetData("RoleEquipWindow", "CLOSE", true);
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnDestroy()
    {
//        DataCenter.Remove("BAG_INFO_WINDOW");
        DataCenter.Remove("RoleEquipWindow");
	}

}


public enum SORT_TYPE
{
    TEAM_POS_TYPE,
	STAR_LEVEL,
	LEVEL,
    TID,
	ELEMENT_INDEX,
	STRENGTHEN_LEVEL,
	NUMBER,
	AUTO_JOIN,
    CAN_COMPOSE,
    BREAK_LEVEL,
}

public class BagInfoWindow : tWindow
{
	public GameObject mTitle;
	public GameObject mPetAndStoneSelBtn;
	public GameObject mPetGroup;
	public GameObject mStoneGroup;
	UILabel mPetNumLabel;

    GemLogicData mGemLogicData;
    
	public GameObject[] mStoneIconVec;

	const int m_iPosX0 = 0;
	const int m_iPosY0 = 0;
	const int m_iDX = 0;
	const int m_iDY = 0;
	const int m_iHeight = 110;
	const int m_iWidth = 110;
	const int m_iMaxColumn = 3;

	bool mbIsShowPetGroup = true;

	UIGridContainer mPetGrid;
	UIGridContainer mPetUpgradeGrid;
	UIGridContainer mGemGrid;

	int iCurUpgradeDBID = 0;
	bool mbIsUpgradeList = false;
	SORT_TYPE mSortType = SORT_TYPE.STAR_LEVEL;

	List<PetData> mDecomposePets = new List<PetData>();

    public static bool isNeedRefresh = true;

	public BagInfoWindow()
	{

	}

	public override void Init()
	{

	}

	public void InitWindow()
	{
        // TODO [8/21/2015 LC]
        return;
		mTitle = GameCommon.FindObject(mGameObjUI, "title");
		mPetAndStoneSelBtn = GameCommon.FindObject(mGameObjUI, "PetAndStoneSelBtn");
		mPetGroup = GameCommon.FindObject(mGameObjUI, "pet_group");
		mStoneGroup = GameCommon.FindObject(mGameObjUI, "stone_group");
		
		mPetNumLabel = GameCommon.FindObject(mGameObjUI, "pet_num_label").GetComponent<UILabel>();
		
		mPetGrid = GameCommon.FindObject(mPetGroup, "pet_icon_grid").GetComponent<UIGridContainer>();
		mPetUpgradeGrid = GameCommon.FindObject(mPetGroup, "pet_icon_upgrade_grid").GetComponent<UIGridContainer>();
		mGemGrid = GameCommon.FindObject(mStoneGroup, "stone_icon_grid").GetComponent<UIGridContainer>();

		DataCenter.Self.registerData("RoleEquipWindow", new RoleEquipWindow(GameCommon.FindObject(mGameObjUI, "RoleEquipWindow")));
		DataCenter.SetData("RoleEquipWindow", "CLOSE", true);

		// init pet
		InitPetIcons();
		
		// init stone
		InitStoneIcons();


		DataCenter.CloseWindow("BAG_INFO_WINDOW");
	}

	public override void OnOpen ()
	{
		DataCenter.Set ("DESCENDING_ORDER", true);
		if(GetSub ("order_button") != null && GetSub ("order_button").GetComponentInChildren<UISprite>() != null)
		{
			GetSub ("order_button").GetComponentInChildren<UISprite>().spriteName = "ui_j";
			GameCommon.GetButtonData (GetSub ("order_button")).set ("WINDOW_NAME", mWinName);
		}
		GameCommon.CloseUIPlayTween (mSortType);
	}
	
    public Vector3 GetPostion(int iIndex)
    {
        return GameCommon.GetPostion(m_iPosX0, m_iPosY0, m_iDX, m_iDY, m_iWidth, m_iHeight, iIndex, m_iMaxColumn);
    }

	public void DestroyPetIconsByParent(string strParentName)
	{
		GameObject petIconcGroup = GameCommon.FindUI(strParentName);
		if(petIconcGroup  != null)
		{
			foreach(Transform trans in petIconcGroup.transform)
			{
				if(trans.gameObject != null)
					GameObject.Destroy(trans.gameObject);
			}
		}
	}

	public void RefreshPetBagNum()
	{
		if(mPetNumLabel != null)
		{
			PetLogicData petLogicData = DataCenter.GetData("PET_DATA") as PetLogicData;
			if(petLogicData != null)
			{
				mPetNumLabel.text = petLogicData.mDicPetData.Count.ToString() + " / " + RoleLogicData.Self.mMaxPetNum.ToString();
			}
		}
	}

	public bool SetStoneIcon(int iIndex)
	{
		if(iIndex < 0 || iIndex >= mGemLogicData.mGemList.Length)
			return false;

		UISprite sprite = GameCommon.FindObject(mStoneIconVec[iIndex], "Background").GetComponent<UISprite>();
		if(sprite != null)
		{
			string strAtlasName = TableCommon.GetStringFromStoneTypeIconConfig(iIndex + 1000, "STONE_ATLAS_NAME");
			string strSpriteName = TableCommon.GetStringFromStoneTypeIconConfig(iIndex + 1000, "STONE_SPRITE_NAME");

            UIAtlas tu = GameCommon.LoadUIAtlas(strAtlasName);
			sprite.atlas = tu;
			sprite.spriteName = strSpriteName;
			sprite.MakePixelPerfect();
		}

		SetStoneNum(iIndex);

		return true;
	}

	public void SetStoneNum(int iIndex)
	{
		if(iIndex >= 0 && iIndex < mStoneIconVec.Length)
		{
			GameObject stoneNumLabelObj = GameCommon.FindObject(mStoneIconVec[iIndex], "StoneNumLabel");			
			GameObject StoneIconCoverBtnObj = GameCommon.FindObject(mStoneIconVec[iIndex], "StoneIconCoverBtn");

			UILabel label = stoneNumLabelObj.GetComponent<UILabel>();

			int iCount = mGemLogicData.mGemList[iIndex].mCount;
			if(iCount > 0)
			{
				label.text = iCount.ToString();
				stoneNumLabelObj.SetActive(true);
				StoneIconCoverBtnObj.SetActive(false);
			}
			else
			{
                label.text = "0";
				stoneNumLabelObj.SetActive(false);
				StoneIconCoverBtnObj.SetActive(true);
			}
		}
	}

	public void SetAllStoneDisable()
	{
		for(int i = 0; i < mStoneIconVec.Length; i++)
		{
			GameObject stoneNumLabelObj = GameCommon.FindObject(mStoneIconVec[i], "StoneNumLabel");
			GameObject StoneIconCoverBtnObj = GameCommon.FindObject(mStoneIconVec[i], "StoneIconCoverBtn");
			GameObject StoneIconCoverNumLabelObj = GameCommon.FindObject(StoneIconCoverBtnObj, "Label");

			StoneIconCoverBtnObj.SetActive(true);

			UILabel label = stoneNumLabelObj.GetComponent<UILabel>();
			int iCount = mGemLogicData.mGemList[i].mCount;
			if(iCount > 0)
			{
				label.text = iCount.ToString();
				stoneNumLabelObj.SetActive(true);
				StoneIconCoverNumLabelObj.SetActive(false);
			}
			else
			{
				stoneNumLabelObj.SetActive(false);
				StoneIconCoverNumLabelObj.SetActive(true);
			}
		}
	}

	public bool SetPetIcon(GameObject obj, PetData petData, bool bIsUsed)
	{
		if (obj == null)
			return false;
		
		GameObject background = GameCommon.FindObject(obj, "Background");
		GameObject emptyBG = GameCommon.FindObject(obj, "empty_bg");
		
		if(petData == null)
		{
			background.SetActive(false);
			emptyBG.SetActive(true);
			
			UIToggle toggle = obj.GetComponent<UIToggle>();
			if (toggle != null)
				toggle.value = false;
		}
		else
		{
			background.SetActive(true);
			emptyBG.SetActive(false);

			UIToggle toggle = obj.GetComponent <UIToggle>();
			if(toggle != null)
				toggle.value = bIsUsed;
			
			// set pet icon
			GameCommon.SetPetIcon(obj, petData.tid);
			
			// set element icon
			//		int iElementIndex = TableCommon.GetNumberFromActiveCongfig(petData.mModelIndex, "ELEMENT_INDEX");
			//        GameCommon.SetElementIcon(obj, iElementIndex);
			
			// set level
			GameCommon.SetLevelLabel(obj, petData.level);
			
			// set star level
			GameCommon.SetStarLevelLabel(obj, petData.starLevel);
			
			// set strengthen level text
			GameCommon.SetStrengthenLevelLabel(obj, petData.strengthenLevel);
		}
		return true;
	}

	public void UpdateDecomposePetIcons()
	{
		if(mPetGrid != null)
		{
			mbIsUpgradeList = false;
			mPetGrid.MaxCount = RoleLogicData.Self.mMaxPetNum;
			int index = 0;

			PetLogicData petLogicData = DataCenter.GetData("PET_DATA") as PetLogicData;
			if (petLogicData != null)
			{
				List<PetData> petList = petLogicData.mDicPetData.Values.ToList();
				petList = SortList(petList);
				for(int i = 0; i < petList.Count; i++)
				{
					PetData pet = petList[i];
					if(pet != null)
					{
						if(petLogicData.IsPetUsed(pet.itemId) || IsInListByDBID (pet.itemId, mDecomposePets))
						{
							continue;
						}
				
						GameObject tempObj = mPetGrid.controlList[index];
						GameObject obj = tempObj.transform.Find("pet_icon_check_btn").gameObject;
						if(obj != null)
						{
							UIButtonEvent buttonEvent = obj.GetComponent<UIButtonEvent>();
							buttonEvent.mData.set("ID", pet.itemId);

							SetPetIcon(obj, pet, false);

							index ++;
						}
					}
				}
				
				for (int j = index; j < RoleLogicData.Self.mMaxPetNum; j++)
				{
					GameObject tempObj = mPetGrid.controlList[index];
					GameObject obj = tempObj.transform.Find("pet_icon_check_btn").gameObject;
					if (obj != null)
					{
						UIButtonEvent buttonEvent = obj.GetComponent<UIButtonEvent>();
						buttonEvent.mData.set("ID", 0);
						SetPetIcon(obj, null, false);
						index++;
					}
				}
			}
			
			GameCommon.SetUIVisiable(mPetGroup, "PetIconGroup", true);
			GameCommon.SetUIVisiable(mPetGroup, "PetIconUpgradeGroup", false);
		}
	}

	bool IsInListByDBID(int DBID, List<PetData>datas)
	{
		foreach(PetData d in datas)
		{
			if(d.itemId == DBID)
				return true;
		}

		return false;
	}

    public void InitPetIcons()
    {
        //if (!isNeedRefresh)
        //    return;
        //isNeedRefresh = false;

		if(mPetGrid != null)
		{
			mbIsUpgradeList = false;
			mPetGrid.MaxCount = RoleLogicData.Self.mMaxPetNum;
			
			int iIndex = 0;
			
			InitUsedPetIcons(ref iIndex);
			
			InitUnUsedPetIcons(iIndex, -1);
			
			GameCommon.SetUIVisiable(mPetGroup, "PetIconGroup", true);
			GameCommon.SetUIVisiable(mPetGroup, "PetIconUpgradeGroup", false);
			
//			ResetScrollViewPosition("PetIconScrollView");
		}
    }

    public void InitUsedPetIcons(ref int iIndex)
    {
		PetLogicData petLogicData = DataCenter.GetData("PET_DATA") as PetLogicData;		
		int iTeam = petLogicData.mCurrentTeam;

        for (byte iUsePos = 1; iUsePos <= 3; iUsePos++)
        {
			PetData petData = petLogicData.GetPetDataByPos(iTeam, iUsePos);
			if(petData != null)
			{
				// set icon
				GameObject tempObj = mPetGrid.controlList[iIndex];
				GameObject obj = tempObj.transform.Find("pet_icon_check_btn").gameObject;
				if(obj != null)
				{
					UIButtonEvent buttonEvent = obj.GetComponent<UIButtonEvent>();
					buttonEvent.mData.set("ID", petData.itemId);
					// set icon
					SetPetIcon(obj, petData, true);

					iIndex++;
				}
			}
        }
    }


    public List<PetData> SortList(List<PetData> list)
	{
		if(list != null && list.Count > 0)
		{
			switch(mSortType)
			{
			case SORT_TYPE.STAR_LEVEL:
				list = GameCommon.SortList (list, GameCommon.SortPetDataByStarLevel);
//				list = list.OrderByDescending (p => p.mStarLevel).
//                    ThenByDescending(p => p.mLevel).
//                    ThenBy(p => p.mModelIndex).
//                    ThenBy(p => TableCommon.GetNumberFromActiveCongfig(p.mModelIndex, "ELEMENT_INDEX")).
//                    ToList();
				break;
			case SORT_TYPE.LEVEL:
				list = GameCommon.SortList (list, GameCommon.SortPetDataByLevel);
//                list = list.OrderByDescending(p => p.mLevel).
//                    ThenByDescending(p => p.mStarLevel).
//                    ThenBy(p => p.mModelIndex).
//                    ThenBy(p => TableCommon.GetNumberFromActiveCongfig(p.mModelIndex, "ELEMENT_INDEX")).
//                    ToList();
				break;
			case SORT_TYPE.ELEMENT_INDEX:
				list = GameCommon.SortList (list, GameCommon.SortPetDataByElement);
//				list = list.OrderBy(p => TableCommon.GetNumberFromActiveCongfig(p.mModelIndex, "ELEMENT_INDEX")).
//					ThenByDescending(p => p.mStarLevel).
//					ThenByDescending(p => p.mLevel).
//					ThenBy(p => p.mModelIndex).
//                    ToList();
				break;
			}
		}

		return list;
	}

	public List<PetData> SortAutoJoinList(List<PetData> list)
	{
		if(list != null && list.Count > 0)
		{
			list = GameCommon.SortList (list, GameCommon.SortAutoJoinPetDataList);
//			list = list.OrderByDescending (p => p.mStarLevel).
//					ThenByDescending(p => p.mLevel).
//					ThenByDescending(p => p.mStrengthenLevel).
//					ThenBy(p => TableCommon.GetNumberFromActiveCongfig(p.mModelIndex, "ELEMENT_INDEX")).
//					ThenBy(p => p.mModelIndex).
//					ToList();
		}
		
		return list;
	}

	// init unused pets without iID
	public void InitUnUsedPetIcons(int iIndex, int iID)
	{
		PetLogicData petLogicData = DataCenter.GetData("PET_DATA") as PetLogicData;
		if (petLogicData != null)
		{
			List<PetData> petList = petLogicData.mDicPetData.Values.ToList();
			petList = SortList(petList);
			for(int i = 0; i < petList.Count; i++)
			{
				PetData pet = petList[i];
				if(pet != null)
				{
					if(pet.itemId == iID || petLogicData.GetPosInCurTeam(pet.itemId) != 0)
						continue;

					GameObject tempObj = mPetGrid.controlList[iIndex];
					GameObject obj = tempObj.transform.Find("pet_icon_check_btn").gameObject;
					if(obj != null)
					{
						UIButtonEvent buttonEvent = obj.GetComponent<UIButtonEvent>();
						buttonEvent.mData.set("ID", pet.itemId);
						// set icon
						SetPetIcon(obj, pet, false);
						
						iIndex++;
					}
				}
			}

//			foreach(KeyValuePair<int, PetData> iter in petLogicData.mPetList)
//			{
//				if(iter.Key == iID || petLogicData.GetPosInCurTeam(iter.Value.mDBID) != 0)
//					continue;
//
//				GameObject tempObj = mPetGrid.controlList[iIndex];
//				GameObject obj = tempObj.transform.Find("pet_icon_check_btn").gameObject;
//				if(obj != null)
//				{
//					UIButtonEvent buttonEvent = obj.GetComponent<UIButtonEvent>();
//					buttonEvent.mData.set("ID", iter.Key);
//					// set icon
//					SetPetIcon(obj, iter.Value, false);
//					
//					iIndex++;
//				}
//			}

			for (int j = iIndex; j < RoleLogicData.Self.mMaxPetNum; j++)
			{
				GameObject tempObj = mPetGrid.controlList[iIndex];
				GameObject obj = tempObj.transform.Find("pet_icon_check_btn").gameObject;
				if (obj != null)
				{
					UIButtonEvent buttonEvent = obj.GetComponent<UIButtonEvent>();
					buttonEvent.mData.set("ID", 0);
					SetPetIcon(obj, null, false);
					iIndex++;
				}
			}
		}
	}

	// init upgrade pets iID
	public void InitUpgradePetIcons(int iIndex, int iID)
	{
		mbIsUpgradeList = true;
		iCurUpgradeDBID = iID;
		PetLogicData petLogicData = DataCenter.GetData("PET_DATA") as PetLogicData;
		if (petLogicData != null && petLogicData.mDicPetData.Count > 0)
		{
			List<PetData> petList = petLogicData.mDicPetData.Values.ToList();
			petList = SortList(petList);
			for(int i = 0; i < petList.Count; i++)
			{
				PetData pet = petList[i];
				if(pet != null)
				{
					if(pet.itemId == iID || petLogicData.IsPetUsed(pet.itemId))
						continue;
					
					GameObject tempObj = mPetUpgradeGrid.controlList[iIndex];
					GameObject obj = tempObj.transform.Find("pet_icon_upgrade_btn").gameObject;
					if(obj != null)
					{
						UIButtonEvent buttonEvent = obj.GetComponent<UIButtonEvent>();
						buttonEvent.mData.set("ID", pet.itemId);

						bool bIsSel = false;
						if(GameCommon.bIsLogicDataExist("PetUpgradeWindow"))
						{
							PetUpgradeWindow petUpgradeWindow = DataCenter.GetData("PetUpgradeWindow") as PetUpgradeWindow;
							
							bIsSel = petUpgradeWindow.IsSelPetByID(pet.itemId);
						}

						// set icon
						SetPetIcon(obj, pet, bIsSel);

						// is can level break
						SetPetLevelBreakIcon(obj, pet);

						iIndex++;
					}
				}
			}

//			foreach(KeyValuePair<int, PetData> iter in petLogicData.mPetList)
//			{
//				if(iter.Key == iID || petLogicData.IsPetUsed(iter.Value.mDBID))
//					continue;
//
//				GameObject tempObj = mPetUpgradeGrid.controlList[iIndex];
//				GameObject obj = tempObj.transform.Find("pet_icon_upgrade_btn").gameObject;
//				if (obj != null)
//				{
//					UIButtonEvent buttonEvent = obj.GetComponent<UIButtonEvent>();
//					buttonEvent.mData.set("ID", iter.Key);
//					// set icon
//					SetPetIcon(obj, iter.Value, false);
//					
//					iIndex++;
//				}
//			}

			for (int j = iIndex; j < RoleLogicData.Self.mMaxPetNum; j++)
			{
				GameObject tempObj = mPetUpgradeGrid.controlList[iIndex];
				GameObject obj = tempObj.transform.Find("pet_icon_upgrade_btn").gameObject;
				if (obj != null)
				{
					UIButtonEvent buttonEvent = obj.GetComponent<UIButtonEvent>();
					buttonEvent.mData.set("ID", 0);
					SetPetIcon(obj, null, false);
					iIndex++;
				}
			}
		}
	}

	public void SetPetLevelBreakIcon(GameObject obj, PetData petData)
	{
		if(obj != null && petData != null)
		{
			PetLogicData petLogicData = DataCenter.GetData("PET_DATA") as PetLogicData;
			PetData curUpgradePetData = petLogicData.GetPetDataByItemId(iCurUpgradeDBID);
			if(curUpgradePetData != null)
			{
				GameObject levelBreakSpriteObj = GameCommon.FindObject(obj, "level_break_sprite");
				if(levelBreakSpriteObj != null)
					levelBreakSpriteObj.SetActive(petData.tid == curUpgradePetData.tid);
			}
		}
	}

	public void SetUpgradeBtnToggleState(int iID, bool bIsTrue)
	{
		for(int i = 0; i < mPetUpgradeGrid.MaxCount; i++)
		{
			GameObject tempObj = mPetUpgradeGrid.controlList[i];
			GameObject obj = tempObj.transform.Find("pet_icon_upgrade_btn").gameObject;
			if(obj != null)
			{
				UIButtonEvent buttonEvent = obj.GetComponent<UIButtonEvent>();
				if(buttonEvent.mData.get("ID") == iID)
				{
					bool bIsSel = bIsTrue;
					
					UIToggle toggle = obj.GetComponent<UIToggle>();
					if (toggle != null)
						toggle.value = bIsTrue;
				}
			}
		}
	}

	public void ResetScrollViewPosition(string strName)
	{
		GameObject obj = GameObject.Find(strName);
        ResetScrollViewPosition(obj);
	}

    public void ResetScrollViewPosition(GameObject obj)
    {
		return;
        if (obj == null)
            return;
        UIScrollView sv = obj.GetComponent<UIScrollView>();
        if (sv != null)
        {
            sv.relativePositionOnReset = Vector2.zero;
            sv.ResetPosition();
        }
    }

	public void InitUpgradePetIcons(int iID)
	{
		mPetUpgradeGrid.MaxCount = RoleLogicData.Self.mMaxPetNum;

		int iIndex = 0;
		
		InitUpgradePetIcons(iIndex, iID);

		GameCommon.SetUIVisiable(mPetGroup, "PetIconGroup", false);
		GameCommon.SetUIVisiable(mPetGroup, "PetIconUpgradeGroup", true);
	}

	public void InitStoneIcons()
	{
		mGemLogicData = DataCenter.GetData("GEM_DATA") as GemLogicData;
		DestroyPetIconsByParent("StoneIconBtns");

		mStoneIconVec = new GameObject[mGemLogicData.mGemList.Length];

		for(int i = 0; i < mGemLogicData.mGemList.Length; i++)
		{
			GameObject obj = GameCommon.LoadAndIntanciateUIPrefabs("stone_icon_sel_btn", "StoneIconBtns");
			if(obj != null)
			{
				obj.transform.localPosition = GetPostion(i);
				
				UIButtonEvent buttonEvent = obj.GetComponent<UIButtonEvent>();
				buttonEvent.mData.set("INDEX", i);

				mStoneIconVec[i] = obj;
				// set gam icon
				SetStoneIcon(i);
			}
		}

		ResetScrollViewPosition("StoneScrollView");
	}

	public void SetTitle(object param)
	{
        if ((BAG_INFO_TITLE_TYPE)param == BAG_INFO_TITLE_TYPE.Bag_Pet_Window_SpriteTitle)
		{
			mTitle.SetActive(true);
			mPetAndStoneSelBtn.SetActive(false);
			mbIsShowPetGroup = true;
		}
        else if ((BAG_INFO_TITLE_TYPE)param == BAG_INFO_TITLE_TYPE.Bag_Pet_Window_ButtonTitle)
		{
            mTitle.SetActive(false);
            mPetAndStoneSelBtn.SetActive(true);

            GameCommon.ToggleFalse(mPetAndStoneSelBtn);

			mbIsShowPetGroup = true;
		}
        else if ((BAG_INFO_TITLE_TYPE)param == BAG_INFO_TITLE_TYPE.Bag_Stone_Window_ButtonTitle)
		{
			mTitle.SetActive(false);
			mPetAndStoneSelBtn.SetActive(true);
            GameCommon.ToggleTrue(mPetAndStoneSelBtn);
			
			mbIsShowPetGroup = false;
		}
        else if ((BAG_INFO_TITLE_TYPE)param == BAG_INFO_TITLE_TYPE.Bag_Stone_Window_ButtonTitle)
        {
            mTitle.SetActive(false);
            mPetAndStoneSelBtn.SetActive(true);
            GameCommon.ToggleTrue(mPetAndStoneSelBtn);

            mbIsShowPetGroup = false;
        }
	}

	public void CloseAllWindow()
	{
		// window
		mStoneGroup.SetActive(false);
		mPetGroup.SetActive(false);
		DataCenter.SetData("RoleEquipWindow", "CLOSE", true);
		//DataCenter.SetData("RoleEquipWindow", "CLOSE", true);

		// title
		mTitle.SetActive(false);
        
		GameCommon.SetUIVisiable(mGameObjUI, "title_gem", false);
	}
	public void ShowWindow(BAG_INFO_TITLE_TYPE param)
	{
		CloseAllWindow();
		if ((BAG_INFO_TITLE_TYPE)param == BAG_INFO_TITLE_TYPE.Bag_Pet_Window_SpriteTitle)
		{
            mTitle.SetActive(true);
            mPetGroup.SetActive(true);
			mPetAndStoneSelBtn.SetActive(false);
		}
		else if ((BAG_INFO_TITLE_TYPE)param == BAG_INFO_TITLE_TYPE.Bag_Pet_Window_ButtonTitle)
		{
			mPetAndStoneSelBtn.SetActive(true);
			GameCommon.ToggleFalse(mPetAndStoneSelBtn);
			
			mPetGroup.SetActive(true);
			GameCommon.SetUIVisiable(mGameObjUI, "title", true);
		}
		else if ((BAG_INFO_TITLE_TYPE)param == BAG_INFO_TITLE_TYPE.Bag_Stone_Window_ButtonTitle)
        {
            mPetAndStoneSelBtn.SetActive(true);
			GameCommon.ToggleTrue(mPetAndStoneSelBtn);
			
			mStoneGroup.SetActive(true);
			GameCommon.SetUIVisiable(mGameObjUI, "title_gem", true);
		}
        else if ((BAG_INFO_TITLE_TYPE)param == BAG_INFO_TITLE_TYPE.Bag_Stone_Window)
        {
			mPetAndStoneSelBtn.SetActive(false);
            mStoneGroup.SetActive(true);
			GameCommon.SetUIVisiable(mGameObjUI, "title_gem", true);
        }
		else if ((BAG_INFO_TITLE_TYPE)param == BAG_INFO_TITLE_TYPE.RoleEquipWindow)
		{
			mPetAndStoneSelBtn.SetActive(false);
			DataCenter.SetData("RoleEquipWindow", "OPEN", true);
		}
		else if ((BAG_INFO_TITLE_TYPE)param == BAG_INFO_TITLE_TYPE.PetEquipWindow)
		{
			mPetAndStoneSelBtn.SetActive(false);
//			DataCenter.SetData("RoleEquipWindow", "OPEN", true);
		}
	}
	
	public override void onChange (string keyIndex, object objVal)
	{
		base.onChange(keyIndex, objVal);

		if(keyIndex == "INIT_WINDOW")
		{
			InitWindow();
		}
		else if(keyIndex == "SHOW_WINDOW")
		{
			ShowWindow((BAG_INFO_TITLE_TYPE)objVal);
		}
		else if(keyIndex == "SHOW_TITLE")
		{
			//SetTitle(objVal);
		}
		else if(keyIndex == "UPDATE_GEM_ICONS")
		{
			InitStoneIcons();
		}
		else if(keyIndex == "UPDATE_PET_ICONS")
		{
			InitPetIcons();

			RefreshPetBagNum();
		}
		else if(keyIndex == "UPDATE_DECOMPOSE_PET_ICONS")
		{
			mDecomposePets = (List<PetData>)objVal;
			UpdateDecomposePetIcons();
			RefreshPetBagNum();
		}
		else if(keyIndex == "UPDATE_UPGRADE_PET_ICONS")
		{
			// init upgrade pet
			InitUpgradePetIcons((int)objVal);
			RefreshPetBagNum();
		}
		else if(keyIndex == "UPDATE_STONE_CION")
		{
			SetStoneNum((int)objVal);
		}
		else if(keyIndex == "CLEAR_STONE_SELECT_STATE")
		{
			ClearStoneSelState();
		}
		else if(keyIndex == "SET_STONE_SELECT")
		{
			SetStoneSel((int)objVal);
		}
		else if(keyIndex == "SET_ALL_STONE_DISABLED")
		{
			SetAllStoneDisable();
		}
        else if (keyIndex == "SET_ALL_STONE_STATE")
        {
            SetAllStoneState((StoneStateFlags)objVal);
        }
        else if (keyIndex == "SET_STONE_STATE")
        {
            KeyValuePair<int, StoneStateFlags> pair = (KeyValuePair<int, StoneStateFlags>)objVal;
            SetStoneState(pair.Key, pair.Value);
        }
		else if(keyIndex == "SORT_PET_ICONS")
		{
			SortPetIcons((SORT_TYPE)objVal);
		}
		else if(keyIndex == "ORDER_RULE")
		{
			SortPetIcons(mSortType);
		}
		else if(keyIndex == "SET_AUTO_JOIN")
		{
			SetAutoJoin((int)objVal);
		}
		else if(keyIndex == "SET_UPGRATE_BTN_TOGGLE_STATE")
		{
			SetUpgradeBtnToggleState((int)objVal, true);
		}
	}

	public void SetAutoJoin(int iMaxPos)
	{
		PetLogicData petLogicData = DataCenter.GetData("PET_DATA") as PetLogicData;
		if(petLogicData != null)
		{
			List<PetData> petList = new List<PetData>();
			if(petLogicData.mDicPetData.Count > 0)
			{
				petList = petLogicData.mDicPetData.Values.ToList();
				petList = SortAutoJoinList(petList);
			}

			int iCurIndex = 0;
			for(int iPos = 1; iPos <= iMaxPos; iPos++)
			{
//				PetData teamPet = petLogicData.GetPetDataByPos(iPos);
//				if(teamPet != null)
//					continue;

				for(int i = iCurIndex; i < petList.Count; i++)
				{
					PetData pet = petList[i];
					if(pet != null)
					{
						PetData teamPet = petLogicData.GetPetDataByPos(iPos);
						if(teamPet == null || teamPet.itemId != pet.itemId)
						{
//							if(petLogicData.IsPetUsedInCurTeam(pet.mDBID))
//								continue;

                            DataCenter.SetData("PetInfoWindow", "SET_SELECT_USE_PET_ID", pet.itemId);
							Button_pet_play_flag_btn btnEvent = EventCenter.Start("Button_pet_play_flag_btn") as Button_pet_play_flag_btn;
							btnEvent.set("POS", iPos);
							btnEvent.DoEvent();
						}

						iCurIndex = i + 1;
						break;
					}
				}
			}
		}
	}

	public void SortPetIcons(SORT_TYPE sortType)
	{
		mSortType = sortType;

		if(mbIsUpgradeList)
		{
			InitUpgradePetIcons(iCurUpgradeDBID);
		}
		else
		{
			if(GameCommon.bIsWindowOpen ("PET_DECOMPOSE_WINDOW"))
				UpdateDecomposePetIcons ();
			else
				InitPetIcons();
		}
	}

	public void ClearStoneSelState()
	{
		for(int i = 0; i < mStoneIconVec.Length; i++)
		{
			GameObject obj = mStoneIconVec[i];
			GameObject StoneIconCoverBtnObj = GameCommon.FindObject(obj, "StoneIconCoverBtn");
			if(mGemLogicData.mGemList[i].mCount > 0)
				StoneIconCoverBtnObj.SetActive(false);

            UIToggle toggle = obj.GetComponent<UIToggle>();
			if(toggle.value)
				toggle.value = false;
		}
	}

	public void SetStoneSel(int iIndex)
	{
		if(iIndex >= 0 && iIndex < mStoneIconVec.Length)
		{
            GameCommon.ToggleTrue(mStoneIconVec[iIndex]);
		}
	}

    private void SetStoneState(int index, StoneStateFlags stateFlags)
    {
		if (index <= 0 && index > mStoneIconVec.Length)
        {
            return;
        }
        GameObject btnObj = mStoneIconVec[index];

		if(btnObj == null)
			return;

        if ((stateFlags & StoneStateFlags.Checked) > 0)
        {
            GameCommon.ToggleTrue(btnObj);
        }
        else
        {
            GameCommon.ToggleFalse(btnObj);
        }
        GameObject shadeObj = GameCommon.FindObject(btnObj, "StoneIconCoverBtn");
        shadeObj.SetActive(true);
        shadeObj.GetComponent<UIButton>().isEnabled = (stateFlags & StoneStateFlags.DisableButton) > 0;
        GameCommon.SetUIVisiable(btnObj, "bg", (stateFlags & StoneStateFlags.EnableShade) > 0);
        UILabel label = null;
        if ((stateFlags & StoneStateFlags.EnableShade) > 0)
        {
            GameCommon.SetUIVisiable(btnObj, "StoneNumLabel", false);
            GameCommon.SetUIVisiable(btnObj, "Label", true);
            label = GameCommon.FindObject(btnObj, "Label").GetComponent<UILabel>();
        }
        else 
        {
            GameCommon.SetUIVisiable(btnObj, "StoneNumLabel", true);
            GameCommon.SetUIVisiable(btnObj, "Label", false);
            label = GameCommon.FindObject(btnObj, "StoneNumLabel").GetComponent<UILabel>();
        }
        label.color = ((stateFlags & StoneStateFlags.RedTextColor) == 0) ? new Color(0.961f, 0.929f, 0.792f, 1f) : Color.red;     
    }

    private void SetAllStoneState(StoneStateFlags stateFlags)
    {
		if(mStoneIconVec != null)
		{
	        for (int i = 0; i < mStoneIconVec.Length; i++)
	        {
	            SetStoneState(i, stateFlags);
	        }
		}
    }
}


[Flags]
public enum StoneStateFlags
{
    None = 0,  
    Checked = 1,   
    EnableShade = 2,
    DisableButton = 4,
    RedTextColor = 8,
}