using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class RoleEquipWindow : tWindow
{
    RoleEquipLogicData mRoleEquipLogicData;
	UIGridContainer mEquipGrid;
	UIGridContainer mEquipStrengthenGrid;

	GameObject mRoleEquipGroup;
	GameObject mRoleEquipStrengthenGroup;

	UILabel mRoleEquipNumLabel;

	public SORT_TYPE mSortType = SORT_TYPE.STAR_LEVEL;

	public EQUIP_TYPE mEquipType = EQUIP_TYPE.MAX;

    public RoleEquipWindow(GameObject obj)
    {
        mGameObjUI = obj;
    }

	public override void Init()
	{
		mRoleEquipGroup = GameCommon.FindObject(mGameObjUI, "role_equip_group");
		mRoleEquipStrengthenGroup = GameCommon.FindObject(mGameObjUI, "role_equip_strengthen_group");

		mEquipGrid = mRoleEquipGroup.transform.Find("scroll_view/grid").GetComponent<UIGridContainer>();
		mEquipStrengthenGrid = mRoleEquipStrengthenGroup.transform.Find("scroll_view/grid").GetComponent<UIGridContainer>();

		mRoleEquipNumLabel = GameCommon.FindObject(mGameObjUI, "role_equip_num_label").GetComponent<UILabel>();

		Refresh(null);
	}

	public void SetCurRoleEquipLogicData()
	{
		mRoleEquipLogicData = DataCenter.GetData("EQUIP_DATA") as RoleEquipLogicData;
	}

    public void InitRoleEquipIcons()
    {
		mEquipType = EQUIP_TYPE.MAX;

		RoleLogicData roleLogic = RoleLogicData.Self;
		mEquipGrid.MaxCount = roleLogic.mMaxRoleEquipNum;
        
        int iIndex = 0;

        InitUsedEquipIcons(ref iIndex);

        InitUnUsedEquipIcons(iIndex);

		mRoleEquipGroup.SetActive(true);
		mRoleEquipStrengthenGroup.SetActive(false);

		RefreshRoleEquipBagNum();
    }

    public void InitUsedEquipIcons(ref int iIndex)
    {
		int curRoleIndex = RoleLogicData.GetMainRole().mIndex;
		if(mRoleEquipLogicData.mDicRoleUseEquip.ContainsKey(curRoleIndex))
		{
			List<EquipData> equipList = mRoleEquipLogicData.mDicRoleUseEquip[curRoleIndex].Values.ToList();
			equipList = SortList(equipList);
			for(int i = 0; i < equipList.Count; i++)
			{
				EquipData tempEquip = equipList[i];
				if(tempEquip != null)
				{
					tempEquip.mGridIndex = iIndex;
					GameObject tempObj = mEquipGrid.controlList[iIndex];
					GameObject obj = tempObj.transform.Find("role_equip_icon_btn").gameObject;
					UIButtonEvent buttonEvent = obj.GetComponent<UIButtonEvent>();
					if (buttonEvent != null)
						buttonEvent.mData.set("GRID_INDEX", tempEquip.mGridIndex);
					
					SetEquipIcon(obj, tempEquip, true);
					iIndex++;
				}
			}
		}
    }

    public void InitUnUsedEquipIcons(int iIndex)
    {
		List<EquipData> equipList = mRoleEquipLogicData.mDicEquip.Values.ToList();
		equipList = SortList(equipList);
		for(int i = 0; i < equipList.Count; i++)
		{
			EquipData tempEquip = equipList[i];
			if(tempEquip != null)
			{
                if (!TeamManager.IsEquipInTeam(tempEquip.itemId))
                {
                    if (tempEquip.mUserID != 0)
                    {
                        tempEquip.mGridIndex = -1;
                    }
                    else
                    {
                        tempEquip.mGridIndex = iIndex;
                        GameObject tempObj = mEquipGrid.controlList[iIndex];
                        GameObject obj = tempObj.transform.Find("role_equip_icon_btn").gameObject;
                        UIButtonEvent buttonEvent = obj.GetComponent<UIButtonEvent>();
                        if (buttonEvent != null)
                            buttonEvent.mData.set("GRID_INDEX", iIndex);

                        SetEquipIcon(obj, tempEquip, false);
                        iIndex++;
                    }
                }
			}
		}

		RoleLogicData roleLogic = RoleLogicData.Self;
		for (int j = iIndex; j < roleLogic.mMaxRoleEquipNum; j++)
		{
			GameObject tempObj = mEquipGrid.controlList[iIndex];
			GameObject obj = tempObj.transform.Find("role_equip_icon_btn").gameObject;

			UIButtonEvent buttonEvent = obj.GetComponent<UIButtonEvent>();
			if(buttonEvent != null)
				buttonEvent.mData.set("GRID_INDEX", iIndex);

			if (obj != null)
			{
				SetEquipIcon(obj, null, false);
				iIndex++;
			}
		}
    }

	public void InitRoleEquipIconsForType(int iType)
	{
		RoleLogicData roleLogic = RoleLogicData.Self;
		mEquipGrid.MaxCount = roleLogic.mMaxRoleEquipNum;
		
		int iIndex = 0;
		
		InitUsedEquipIconsForType(ref iIndex, iType);
		
		InitUnUsedEquipIconsForType(iIndex, iType);
	}

	public void InitUsedEquipIconsForType(ref int iIndex, int iType)
	{
		EquipData equip = mRoleEquipLogicData.GetUseEquip(iType);
		if (equip != null)
		{
			equip.mGridIndex = iIndex;
			GameObject tempObj = mEquipGrid.controlList[iIndex];
			GameObject obj = tempObj.transform.Find("role_equip_icon_btn").gameObject;
			UIButtonEvent buttonEvent = obj.GetComponent<UIButtonEvent>();
			if (buttonEvent != null)
				buttonEvent.mData.set("GRID_INDEX", equip.mGridIndex);
			
			SetEquipIcon(obj, equip, true);
			iIndex++;
		}
	}
	
	public void InitUnUsedEquipIconsForType(int iIndex, int iType)
	{
		List<EquipData> allEquipList = mRoleEquipLogicData.mDicEquip.Values.ToList();
		List<EquipData> equipList = new List<EquipData>();
		for(int i = 0; i < allEquipList.Count; i++)
		{
			EquipData equip = allEquipList[i];
			if(TableCommon.GetNumberFromRoleEquipConfig(equip.tid, "ROLEEQUIP_TYPE") == iType)
				equipList.Add(equip);
		}

		equipList = SortList(equipList);
		for(int i = 0; i < equipList.Count; i++)
		{
			EquipData tempEquip = equipList[i];
			if(tempEquip != null)
			{
                if (!TeamManager.IsEquipInTeam(tempEquip.itemId))
                {
                    if (tempEquip.mUserID != 0)
                    {
                        tempEquip.mGridIndex = -1;
                    }
                    else
                    {
                        tempEquip.mGridIndex = iIndex;
                        GameObject tempObj = mEquipGrid.controlList[iIndex];
                        GameObject obj = tempObj.transform.Find("role_equip_icon_btn").gameObject;
                        UIButtonEvent buttonEvent = obj.GetComponent<UIButtonEvent>();
                        if (buttonEvent != null)
                            buttonEvent.mData.set("GRID_INDEX", iIndex);

                        SetEquipIcon(obj, tempEquip, false);
                        iIndex++;
                    }
                }
			}
		}
		
		RoleLogicData roleLogic = RoleLogicData.Self;
		for (int j = iIndex; j < roleLogic.mMaxRoleEquipNum; j++)
		{
			GameObject tempObj = mEquipGrid.controlList[iIndex];
			GameObject obj = tempObj.transform.Find("role_equip_icon_btn").gameObject;
			
			UIButtonEvent buttonEvent = obj.GetComponent<UIButtonEvent>();
			if(buttonEvent != null)
				buttonEvent.mData.set("GRID_INDEX", iIndex);
			
			if (obj != null)
			{
				SetEquipIcon(obj, null, false);
				iIndex++;
			}
		}
	}

	public void InitRoleEquipStrengrhenIcons(int iGridIndex)
	{
		mEquipType = EQUIP_TYPE.MAX;
		
		RoleLogicData roleLogic = RoleLogicData.Self;
		mEquipStrengthenGrid.MaxCount = roleLogic.mMaxRoleEquipNum;
		
		int iIndex = 0;		
		InitRoleEquipStrengrhenIcons(iIndex, iGridIndex);

//		ClearUsedEquipGridIndex();
		
		mRoleEquipGroup.SetActive(false);
		mRoleEquipStrengthenGroup.SetActive(true);
		
		RefreshRoleEquipBagNum();
	}

	public void ClearUsedEquipGridIndex()
	{
		int curRoleIndex = RoleLogicData.GetMainRole().mIndex;
		if(mRoleEquipLogicData.mDicRoleUseEquip.ContainsKey(curRoleIndex))
		{
			foreach(KeyValuePair<int, EquipData> pair in mRoleEquipLogicData.mDicRoleUseEquip[curRoleIndex])
			{
				pair.Value.mGridIndex = -1;
			}
		}
	}
	
	public void InitRoleEquipStrengrhenIcons(int iIndex, int iGridIndex)
	{
		EquipData equip = mRoleEquipLogicData.GetEquipDataByGridIndex(iGridIndex);
		if(equip != null)
		{
			List<EquipData> equipList = mRoleEquipLogicData.mDicEquip.Values.ToList();
			equipList = SortList(equipList);
			for(int i = 0; i < equipList.Count; i++)
			{
				EquipData tempEquip = equipList[i];
				if(tempEquip != null)
				{
                    if (!TeamManager.IsEquipInTeam(tempEquip.itemId) && tempEquip.mUserID == 0 && equip.itemId != tempEquip.itemId)
					{
//						tempEquip.mGridIndex = iIndex;
						GameObject tempObj = mEquipStrengthenGrid.controlList[iIndex];
						GameObject obj = tempObj.transform.Find("role_equip_strengthen_icon_btn").gameObject;
						UIButtonEvent buttonEvent = obj.GetComponent<UIButtonEvent>();
						if(buttonEvent != null)
							buttonEvent.mData.set("DBID", tempEquip.itemId);

						bool bIsSel = false;
						if(GameCommon.bIsLogicDataExist("ExpansionInfoWindow"))
						{
							ExpansionInfoWindow expansionInfoWindow = DataCenter.GetData("ExpansionInfoWindow") as ExpansionInfoWindow;
							
							bIsSel = expansionInfoWindow.IsSelEquipByID(tempEquip.itemId);
						}

                        SetEquipIcon(obj, tempEquip, bIsSel);
						iIndex++;
					}
//					else
//					{
//						tempEquip.mGridIndex = -1;
//					}
				}
			}
			
			RoleLogicData roleLogic = RoleLogicData.Self;
			for (int j = iIndex; j < roleLogic.mMaxRoleEquipNum; j++)
			{
				GameObject tempObj = mEquipStrengthenGrid.controlList[iIndex];
				GameObject obj = tempObj.transform.Find("role_equip_strengthen_icon_btn").gameObject;
				
				UIButtonEvent buttonEvent = obj.GetComponent<UIButtonEvent>();
                if (buttonEvent != null)
                    buttonEvent.mData.set("DBID", -1);
				
				if (obj != null)
				{
					SetEquipIcon(obj, null, false);
					iIndex++;
				}
			}
		}
	}

	public void SetStrenthenBtnToggleState(int iItemId, bool bIsTrue)
	{
		for(int i = 0; i < mEquipStrengthenGrid.MaxCount; i++)
		{
			GameObject tempObj = mEquipStrengthenGrid.controlList[i];
			GameObject obj = tempObj.transform.Find("role_equip_strengthen_icon_btn").gameObject;
			if(obj != null)
			{
				UIButtonEvent buttonEvent = obj.GetComponent<UIButtonEvent>();
                if (buttonEvent.mData.get("DBID") == iItemId)
				{
					bool bIsSel = bIsTrue;
					
					UIToggle toggle = obj.GetComponent<UIToggle>();
					if (toggle != null)
						toggle.value = bIsTrue;
				}
			}
		}
	}

	public List<EquipData> SortList(List<EquipData> list)
	{
		if(list != null && list.Count > 0)
		{
			switch(mSortType)
			{
			case SORT_TYPE.STAR_LEVEL:
				list = GameCommon.SortList (list, GameCommon.SortEquipDataByStarLevel);
//				list = list.OrderByDescending(p => p.mStarLevel).
//					ThenByDescending(p => p.mStrengthenLevel).
//						ThenBy(p => p.mModelIndex).
//						ThenBy(p => (int)p.mElementType).
//						ToList();
				break;
			case SORT_TYPE.STRENGTHEN_LEVEL:
				list = GameCommon.SortList (list, GameCommon.SortEquipDataByStrengthenLevel);
//				list = list.OrderByDescending(p => p.mStrengthenLevel).
//					ThenByDescending(p => p.mStarLevel).
//						ThenBy(p => p.mModelIndex).
//						ThenBy(p => (int)p.mElementType).
//						ToList();
				break;
			case SORT_TYPE.ELEMENT_INDEX:
				list = GameCommon.SortList (list, GameCommon.SortEquipDataByElement);
//				list = list.OrderBy(p => (int)p.mElementType).
//					ThenByDescending(p => p.mStarLevel).
//						ThenByDescending(p => p.mStrengthenLevel).
//						ThenBy(p => p.mModelIndex).
//						ToList();
				break;
			}
		}
		
		return list;
	}

    public bool SetEquipIcon(GameObject obj, EquipData equipData, bool bIsUsed)
    {
        if (obj == null)
            return false;

		GameObject background = GameCommon.FindObject(obj, "icon_sprite");
		GameObject emptyBG = GameCommon.FindObject(obj, "empty_bg");

		if(equipData == null)
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
//			emptyBG.SetActive(false);

			UIToggle toggle = obj.GetComponent<UIToggle>();
			if (toggle != null)
				toggle.value = bIsUsed;
			
			// set equip icon
			GameCommon.SetEquipIcon(obj, equipData.tid);

			// set element icon and set equip element background icon
			int iElementIndex = (int)equipData.mElementType;
			GameCommon.SetEquipElementBgIcons(obj, equipData.tid, iElementIndex);
						
			// set star level
			GameCommon.SetStarLevelLabel(obj, equipData.mStarLevel);
			
			// set strengthen level text
			GameCommon.SetStrengthenLevelLabel(obj, equipData.strengthenLevel);
		}

        return true;
    }

	public override void OnOpen ()
	{
		base.OnOpen ();

		DataCenter.Set ("DESCENDING_ORDER", true);
		GameObject orderButton = GameCommon.FindUI ("bag_info_window", "order_button");
		if(orderButton != null && orderButton.GetComponentInChildren<UISprite>() != null)
		{
			orderButton.GetComponentInChildren<UISprite>().spriteName = "ui_j";
			GameCommon.GetButtonData (orderButton).set ("WINDOW_NAME", "RoleEquipWindow");
		}
		GameCommon.CloseUIPlayTween (mSortType);

		Refresh(null);
	}

	public override bool Refresh (object param)
	{
		SetCurRoleEquipLogicData();
		InitRoleEquipIcons();

		return true;
	}

    public void RefreshRoleEquipBagNum()
    {
        if (mRoleEquipNumLabel != null)
        {
            mRoleEquipNumLabel.text = GameCommon.GetCurRoleEquipNum().ToString() + " / " + RoleLogicData.Self.mMaxRoleEquipNum.ToString();
        }
    }

	public void SortRoleEquipIcons(SORT_TYPE sortType)
	{
		if(mEquipType == EQUIP_TYPE.MAX)
		{
			mSortType = sortType;
			InitRoleEquipIcons();
		}
		else
		{
			SortRoleEquipIconsForType(sortType);
		}
	}

	public void SortRoleEquipIconsForType(SORT_TYPE sortType)
	{
		mSortType = sortType;
		
		InitRoleEquipIconsForType((int)mEquipType);
	}

	public override void onChange (string keyIndex, object objVal)
	{
		base.onChange(keyIndex, objVal);
		
		if(keyIndex == "UPDATE_ROLE_EQUIP_ICONS")
		{
			InitRoleEquipIcons();
		}
		else if(keyIndex == "UPDATE_ROLE_EQUIP_STRENGTHEN_ICONS")
		{
			// init upgrade pet
			InitRoleEquipStrengrhenIcons((int)objVal);
		}
		else if(keyIndex == "UPDATE_ROLE_EQUIP_ICONS_FOR_TYPE")
		{
			mEquipType = (EQUIP_TYPE)objVal;
			InitRoleEquipIconsForType((int)objVal);
		}
		else if(keyIndex == "SORT_ROLE_EQUIP_ICONS")
		{
			if(mRoleEquipStrengthenGroup.activeSelf)
			{
				mSortType = (SORT_TYPE)objVal;
				InitRoleEquipStrengrhenIcons((int)get ("UPDATE_ROLE_EQUIP_STRENGTHEN_ICONS"));
			}
			else
				SortRoleEquipIcons((SORT_TYPE)objVal);
		}
		else if(keyIndex == "ORDER_RULE")
		{
			if(mRoleEquipStrengthenGroup.activeSelf)
				InitRoleEquipStrengrhenIcons((int)get ("UPDATE_ROLE_EQUIP_STRENGTHEN_ICONS"));
			else
				SortRoleEquipIcons(mSortType);
		}
		else if(keyIndex == "SORT_ROLE_EQUIP_ICONS_FOR_TYPE")
		{
			SortRoleEquipIconsForType((SORT_TYPE)objVal);
		}
		else if(keyIndex == "SHOW_SELECT_ICON")
		{
			ShowSelIcon((int)objVal);
		}
		else if(keyIndex == "HIDE_SELECT_ICON")
		{
			HideSelIcon((int)objVal);
		}
        else if (keyIndex == "SET_STRENTHEN_BTN_TOGGLE_STATE")
        {
            SetStrenthenBtnToggleState((int)objVal, true);
        }
	}

	public void ShowSelIcon(int iIndex)
	{
		GameObject tempObj = mEquipGrid.controlList[iIndex];
		GameObject ensureSprite = GameCommon.FindObject(tempObj, "ensure_sprite");
		if(ensureSprite != null)
		{
			ensureSprite.SetActive(true);
		}
	}

	public void HideSelIcon(int iIndex)
	{
		GameObject tempObj = mEquipGrid.controlList[iIndex];
		GameObject ensureSprite = GameCommon.FindObject(tempObj, "ensure_sprite");
		if(ensureSprite != null)
		{
			ensureSprite.SetActive(false);
		}
	}
}
