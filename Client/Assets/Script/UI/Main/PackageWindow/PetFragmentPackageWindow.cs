using UnityEngine;
using Logic;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class PetFragmentPackageWindow : ItemPackageBaseWindow {
	// fragment fragment
	public PetFragmentLogicData mPetFragmentLogicData;
	public SORT_TYPE mSortType;
	
	public PetFragmentData mCurSelPetFragmentData = null;
	
	public override void Init ()
	{
		base.Init ();

		EventCenter.Self.RegisterEvent("Button_package_pet_fragment_star_btn", new DefineFactory<Button_PackagePetFragmentStarBtn>());
		EventCenter.Self.RegisterEvent("Button_package_pet_fragment_number_btn", new DefineFactory<Button_PackagePetFragmentNumberBtn>());
		EventCenter.Self.RegisterEvent("Button_package_pet_fragment_attribute_btn", new DefineFactory<Button_PackagePetFragmentAttributeBtn>());

		EventCenter.Self.RegisterEvent("Button_package_pet_fragment_icon_btn", new DefineFactory<Button_PackagePetFragmentIconBtn>());
		EventCenter.Self.RegisterEvent("Button_pet_fragment_package_use_button", new DefineFactory<Button_PetFragmentPackageUseButton>());
		EventCenter.Self.RegisterEvent("Button_pet_fragment_package_control_button", new DefineFactory<Button_PetFragmentPackageControlButton>());
		
	}

	public override void onChange (string keyIndex, object objVal)
	{
		base.onChange (keyIndex, objVal);
		if(keyIndex == "INIT_PACKAGE_ICONS")
		{
			InitPackageIcons();
		}
		else if(keyIndex == "SORT_PET_FRAGMENT_ICONS")
		{
			SortPetFragmentIcons((SORT_TYPE)objVal);
		}
		else if(keyIndex == "USE")
		{
			UseOK();
		}
		else if(keyIndex == "USE_RESULT")
		{
			UseResult();
		}
		else if(keyIndex == "SHOW_INFO")
		{
			ShowPetInfo();
		}
	}
	
	public override void OnOpen ()
	{
		base.OnOpen ();
	}
	
	public override void InitVariable()
	{
		mSortType = SORT_TYPE.STAR_LEVEL;
		InitPackageIcons();
		RefreshBagNum();
		
		Refresh(0);
	}
	
	public override bool Refresh (object param)
	{
		base.Refresh (param);

		mCurSelPetFragmentData = mPetFragmentLogicData.GetPetFragmentDataByGridIndex(mCurSelGridIndex) as PetFragmentData;

		RefreshInfoWindow();
		
		return true;
	}
	
	
	public override void InitPackageIcons()
	{
		mPetFragmentLogicData = PetFragmentLogicData.Self;
		mGrid.MaxCount = mPetFragmentLogicData.mDicPetFragmentData.Count;
		
		InitFragmentIcons();
	}
	
	public void InitFragmentIcons()
	{
		if (mPetFragmentLogicData != null)
		{
			int iIndex = 0;

			List<PetFragmentData> petFragmentList = mPetFragmentLogicData.mDicPetFragmentData.Values.ToList();
			petFragmentList = SortList(petFragmentList);
			for(int i = 0; i < petFragmentList.Count; i++)
			{
				PetFragmentData petFragment = petFragmentList[i];
				if(petFragment != null)
				{
					petFragment.mGridIndex = iIndex;
					GameObject tempObj = mGrid.controlList[i];
					GameObject obj = tempObj.transform.Find("package_pet_fragment_icon_btn").gameObject;
					if(obj != null)
					{
						UIButtonEvent buttonEvent = obj.GetComponent<UIButtonEvent>();
						buttonEvent.mData.set("GRID_INDEX", petFragment.mGridIndex);
						// set icon
						SetPetFragmentIcon(obj, petFragment, false);

						iIndex++;
					}
				}
			}
		}
	}
	
	public List<PetFragmentData> SortList(List<PetFragmentData> list)
	{
		if(list != null && list.Count > 0)
		{
			switch(mSortType)
			{
			case SORT_TYPE.STAR_LEVEL:
				list =  GameCommon.SortList (list, GameCommon.SortFragmentDataByStarLevel);
//				list = list.OrderByDescending(p => TableCommon.GetNumberFromActiveCongfig(p.mComposeItemTid, "STAR_LEVEL")).
//						ThenByDescending(p => p.mCount).
//						ThenBy(p => p.mModelIndex).
//						ThenBy(p => TableCommon.GetNumberFromActiveCongfig(p.mComposeItemTid, "ELEMENT_INDEX")).
//						ToList();
				break;
			case SORT_TYPE.NUMBER:
				list =  GameCommon.SortList (list, GameCommon.SortFragmentDataByNumber);
//				list = list.OrderByDescending(p => p.mCount).
//						ThenByDescending(p => TableCommon.GetNumberFromActiveCongfig(p.mComposeItemTid, "STAR_LEVEL")).
//						ThenBy(p => p.mModelIndex).
//						ThenBy(p => TableCommon.GetNumberFromActiveCongfig(p.mComposeItemTid, "ELEMENT_INDEX")).
//						ToList();
				break;
			case SORT_TYPE.ELEMENT_INDEX:
				list =  GameCommon.SortList (list, GameCommon.SortFragmentDataByElement);
//				list = list.OrderBy(p => TableCommon.GetNumberFromActiveCongfig(p.mComposeItemTid, "ELEMENT_INDEX")).
//						ThenByDescending(p => TableCommon.GetNumberFromActiveCongfig(p.mComposeItemTid, "STAR_LEVEL")).
//						ThenByDescending(p => p.mCount).
//						ThenBy(p => p.mModelIndex).
//						ToList();
				break;
			}
		}
		
		return list;
	}
	
	public bool SetPetFragmentIcon(GameObject obj, PetFragmentData petFragmentData, bool bIsUsed)
	{
		if (obj == null)
			return false;
		
		GameObject background = GameCommon.FindObject(obj, "Background");
		GameObject emptyBG = GameCommon.FindObject(obj, "empty_bg");
		
		if(petFragmentData == null)
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
			
			UIToggle toggle = obj.GetComponent<UIToggle>();
			if (toggle != null)
				toggle.value = bIsUsed;
			
			SetIcon(obj, petFragmentData);
		}
		
		return true;
	}
	
	public void SetIcon(GameObject obj, PetFragmentData petFragmentData)
	{
		if(obj != null && petFragmentData != null)
		{
			// set pet icon
			GameCommon.SetPetIcon(obj, petFragmentData.mComposeItemTid);
			
			// set element icon
			int iElementIndex = TableCommon.GetNumberFromActiveCongfig(petFragmentData.mComposeItemTid, "ELEMENT_INDEX");
	        GameCommon.SetElementIcon(obj, iElementIndex);

			// set element fragment icon
			GameCommon.SetElementFragmentIcon(obj, iElementIndex);
			
			// set level
			GameCommon.SetLevelLabel(obj, 1);
			
			// set star level
			GameCommon.SetStarLevelLabel(obj, TableCommon.GetNumberFromActiveCongfig(petFragmentData.mComposeItemTid, "STAR_LEVEL"));
			
			// set strengthen level text
			GameCommon.SetUIText(obj, "count_label", petFragmentData.itemNum.ToString());
		}
	}
	
	public override void RefreshBagNum()
	{
		UILabel petFragmentNumLabel = mGameObjUI.transform.Find("num_label").GetComponent<UILabel>();
		if(petFragmentNumLabel != null)
		{
			petFragmentNumLabel.text = mPetFragmentLogicData.mDicPetFragmentData.Count.ToString();
		}
	}
	
	public override void RefreshInfoWindow()
	{
		GameObject fragmentGroup = mGameObjUI.transform.Find("info_window/group").gameObject;
		GameObject fragmentEmptyGroup = mGameObjUI.transform.Find("info_window/empty_bg").gameObject;
		if (mCurSelPetFragmentData == null)
		{
			fragmentGroup.SetActive(false);
			fragmentEmptyGroup.SetActive(true);
		}
		else
		{
			fragmentGroup.SetActive(true);
			fragmentEmptyGroup.SetActive(false);
			
			SetIcon(fragmentGroup, mCurSelPetFragmentData);
			
			// set star level
			//GameCommon.SetStarLevelLabel(fragmentGroup, TableCommon.GetNumberFromActiveCongfig(mCurSelPetFragmentData.mComposeItemTid, "STAR_LEVEL"), "fragment_star_level_label");
            GameCommon.SetUIText(fragmentGroup, "fragment_star_level_label", TableCommon.GetNumberFromActiveCongfig(mCurSelPetFragmentData.mComposeItemTid, "STAR_LEVEL").ToString());
			
			// set name
			UILabel name = GameCommon.FindObject(fragmentGroup, "props_name").GetComponent<UILabel>();
			name.text = TableCommon.GetStringFromFragment(mCurSelPetFragmentData.tid, "NAME");
			//set fragment_consume_number 
			UILabel fragment_consume_number = GameCommon.FindObject(fragmentGroup, "fragment_consume_number").GetComponent<UILabel>();
			fragment_consume_number.text = TableCommon.GetStringFromFragment(mCurSelPetFragmentData.tid, "COST_NUM");
			
			// set description
			UILabel descrition = GameCommon.FindObject(fragmentGroup, "introduce_label").GetComponent<UILabel>();
			descrition.text = TableCommon.GetStringFromFragment(mCurSelPetFragmentData.tid, "DESCRIPTION");
		}
	}
	
	public void SortPetFragmentIcons(SORT_TYPE sortType)
	{
		mSortType = sortType;
		
		InitPackageIcons();
	}

	public bool UseCondition()
	{
		if(mCurSelPetFragmentData != null)
		{
			if(TableCommon.GetNumberFromFragment(mCurSelPetFragmentData.tid, "COST_NUM") > mCurSelPetFragmentData.itemNum)
			{
				DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_PET_FRAGMENT_COUNT);
				return false;
			}
			if(TableCommon.GetNumberFromFragment(mCurSelPetFragmentData.tid, "COST_COIN") > RoleLogicData.Self.gold)
			{
                DataCenter.OpenWindow(UIWindowString.access_to_res_window, (int)ITEM_TYPE.GOLD);
				return false;
			}

			return true;
		}
		return false;
	}

	public void UseOK()
	{
		if(mCurSelPetFragmentData == null)
			return;

		if(UseCondition())
		{
			if(CommonParam.bIsNetworkGame)
			{
				CS_ComposePet quest = Net.StartEvent("CS_ComposePet") as CS_ComposePet;
				quest.set("FRAGMENT_ID", mCurSelPetFragmentData.tid);
				quest.mAction = () => DataCenter.SetData("PET_FRAGMENT_PACKAGE_WINDOW", "USE_RESULT", true);
				quest.DoEvent();
			}
			else
			{
				UseResult();
			}
		}
	}
	
	public void UseResult()
	{
		// set pet fragment data
		if(mPetFragmentLogicData != null)
		{
			int iDNum = TableCommon.GetNumberFromFragment(mCurSelPetFragmentData.tid, "COST_NUM");
            bool isSuccess = mPetFragmentLogicData.ChangeItemDataNum(mCurSelPetFragmentData.itemId, mCurSelPetFragmentData.tid, iDNum * (-1));
		
			// refresh pet fragment icons
			InitPackageIcons();

            if (isSuccess)
			{
				PetFragmentData petFragmentData = mPetFragmentLogicData.GetItemDataByTid(mCurSelPetFragmentData.tid);
				if(petFragmentData != null)
					Refresh(petFragmentData.mGridIndex);
			}
			else
			{
				Refresh(0);
			}
			
			// cost gold
			int iCostGoldNum = TableCommon.GetNumberFromFragment(mCurSelPetFragmentData.tid, "COST_COIN");
			GameCommon.RoleChangeGold(iCostGoldNum * (-1));

			int iModelIndex = TableCommon.GetNumberFromFragment(mCurSelPetFragmentData.tid, "ITEM_ID");
			DataCenter.SetData("PET_INFO_SINGLE_WINDOW", "OPEN", PET_INFO_WINDOW_TYPE.COMPOSE);
			DataCenter.SetData("PET_INFO_SINGLE_WINDOW", "SET_SELECT_PET_BY_MODEL_INDEX", iModelIndex);
		}
	}

	public void ShowPetInfo()
	{
		if(mCurSelPetFragmentData != null)
		{
			int iModelIndex = TableCommon.GetNumberFromFragment(mCurSelPetFragmentData.tid, "ITEM_ID");
			DataCenter.SetData("PET_INFO_SINGLE_WINDOW", "OPEN", PET_INFO_WINDOW_TYPE.TUJIAN);
			DataCenter.SetData("PET_INFO_SINGLE_WINDOW", "SET_SELECT_PET_BY_MODEL_INDEX", iModelIndex);
		}
	}
}

public class Button_PackagePetFragmentStarBtn : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.SetData("PET_FRAGMENT_PACKAGE_WINDOW", "SORT_PET_FRAGMENT_ICONS", SORT_TYPE.STAR_LEVEL);
		return true;
	}
}

public class Button_PackagePetFragmentNumberBtn : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.SetData("PET_FRAGMENT_PACKAGE_WINDOW", "SORT_PET_FRAGMENT_ICONS", SORT_TYPE.NUMBER);
		return true;
	}
}

public class Button_PackagePetFragmentAttributeBtn : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.SetData("PET_FRAGMENT_PACKAGE_WINDOW", "SORT_PET_FRAGMENT_ICONS", SORT_TYPE.ELEMENT_INDEX);
		return true;
	}
}

public class Button_PackagePetFragmentIconBtn : CEvent
{
	public override bool _DoEvent()
	{
		object val;		
		bool b = getData("BUTTON", out val);
		GameObject obj = val as GameObject;
		UIToggle toggle = obj.GetComponent<UIToggle>();
		toggle.value = !toggle.value;
		
		PetFragmentLogicData logicData = DataCenter.GetData("PET_FRAGMENT_DATA") as PetFragmentLogicData;
		int iGridIndex = (int)get("GRID_INDEX");
		PetFragmentData petFragmentData = logicData.GetPetFragmentDataByGridIndex(iGridIndex);

		if(petFragmentData != null)
		{
			DataCenter.SetData("PET_FRAGMENT_PACKAGE_WINDOW", "REFRESH", iGridIndex);
		}
		
		return true;
	}
}

public class Button_PetFragmentPackageUseButton : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.SetData("PET_FRAGMENT_PACKAGE_WINDOW", "USE", true);
		
		return true;
	}
}

public class Button_PetFragmentPackageControlButton : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.SetData("PET_FRAGMENT_PACKAGE_WINDOW", "SHOW_INFO", true);
		return true;
	}
}