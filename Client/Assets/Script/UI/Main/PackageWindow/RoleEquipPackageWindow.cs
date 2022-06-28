using UnityEngine;
using System.Collections;
using Logic;
using System.Collections.Generic;
using System.Linq;

public class RoleEquipPackageWindow : ItemPackageBaseWindow {

	// role equip
	public RoleEquipLogicData mRoleEquipLogicData;
	public SORT_TYPE mSortType;

	RoleEquipData mCurSelEquipData = null;

	public override void Init ()
	{
		base.Init ();

		EventCenter.Self.RegisterEvent("Button_package_role_equip_star_btn", new DefineFactory<Button_PackageRoleEquipStarBtn>());
		EventCenter.Self.RegisterEvent("Button_package_role_equip_strengthen_level_btn", new DefineFactory<Button_PackageRoleEquipStrengthenLevelBtn>());
		EventCenter.Self.RegisterEvent("Button_package_role_equip_attribute_btn", new DefineFactory<Button_PackageRoleEquipAttributeBtn>());

		EventCenter.Self.RegisterEvent("Button_package_role_equip_icon_btn", new DefineFactory<Button_PackageRoleEquipIconBtn>());
		EventCenter.Self.RegisterEvent("Button_role_equip_package_sale_button", new DefineFactory<Button_RoleEquipPackageSaleButton>());
		EventCenter.Self.RegisterEvent("Button_role_equip_package_control_button", new DefineFactory<Button_RoleEquipPackageControlButton>());

	}
	public override void onChange (string keyIndex, object objVal)
	{
		base.onChange (keyIndex, objVal);
		if(keyIndex == "SORT_ROLE_EQUIP_ICONS")
		{
			SortRoleEquipIcons((SORT_TYPE)objVal);
		}
		else if(keyIndex == "SALE")
		{
            // gain gold
            int iGainGlod = TableCommon.GetNumberFromRoleEquipConfig(mCurSelEquipData.tid, "PRICE");
            DataCenter.OpenMessageOkWindow(STRING_INDEX.ERROR_ROLE_EQUIP_SALE, iGainGlod.ToString(), () => SaleOK());
		}
		else if(keyIndex == "SALE_RESULT")
		{
			SaleResult();
		}
	}

	public override void OnOpen ()
	{
		InitButtonIsUnLockOrNot();
		base.OnOpen ();
	}

	void InitButtonIsUnLockOrNot()
	{
		GameCommon.ButtonEnbleButCanClick (mGameObjUI, "role_equip_package_control_button", UNLOCK_FUNCTION_TYPE.FA_BAO);
	}

	public override void InitVariable()
	{
		mRoleEquipLogicData = DataCenter.GetData("EQUIP_DATA") as RoleEquipLogicData;
		if(mRoleEquipLogicData != null)
		{
			mSortType = SORT_TYPE.STAR_LEVEL;
			InitPackageIcons();
			RefreshBagNum();
		}

		Refresh(0);
	}

	public override bool Refresh (object param)
	{
		base.Refresh (param);

		mCurSelEquipData = mRoleEquipLogicData.GetEquipDataByGridIndex(mCurSelGridIndex) as RoleEquipData;

		RefreshInfoWindow();

		return true;
	}


	public override void  InitPackageIcons()
	{
		RoleLogicData roleLogic = RoleLogicData.Self;
		mGrid.MaxCount = roleLogic.mMaxRoleEquipNum;
		
		int iIndex = 0;
		
		InitUsedEquipIcons(ref iIndex);
		
		InitUnUsedEquipIcons(iIndex);
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
					GameObject tempObj = mGrid.controlList[iIndex];
					GameObject obj = tempObj.transform.Find("package_role_equip_icon_btn").gameObject;
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
            if (tempEquip != null)
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
                        GameObject tempObj = mGrid.controlList[iIndex];
                        GameObject obj = tempObj.transform.Find("package_role_equip_icon_btn").gameObject;
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
			GameObject tempObj = mGrid.controlList[iIndex];
			GameObject obj = tempObj.transform.Find("package_role_equip_icon_btn").gameObject;
			
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
			emptyBG.SetActive(false);
			
			UIToggle toggle = obj.GetComponent<UIToggle>();
			if (toggle != null)
				toggle.value = bIsUsed;
			
			SetIcon(obj, equipData);

			// set element icon and set equip element background icon
			int iElementIndex = (int)equipData.mElementType;
			GameCommon.SetEquipElementBgIcons(obj, equipData.tid, iElementIndex);
		}
		return true;
	}

	public void SetIcon(GameObject obj, EquipData equipData)
	{
		if(obj != null && equipData != null)
		{
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
	}
	
	public override void RefreshBagNum()
	{
		UILabel roleEquipNumLabel = mGameObjUI.transform.Find("num_label").GetComponent<UILabel>();
		if(roleEquipNumLabel != null)
		{
			roleEquipNumLabel.text = GameCommon.GetCurRoleEquipNum().ToString() + " / " + RoleLogicData.Self.mMaxRoleEquipNum.ToString();
		}
	}

	public override void RefreshInfoWindow()
	{
		GameObject equipGroup = mGameObjUI.transform.Find("info_window/group").gameObject;
		GameObject equipEmptyGroup = mGameObjUI.transform.Find("info_window/empty_bg").gameObject;
		if (mCurSelEquipData == null)
		{
			equipGroup.SetActive(false);
			equipEmptyGroup.SetActive(true);
		}
		else
		{
			equipGroup.SetActive(true);
			equipEmptyGroup.SetActive(false);
			
			SetIcon(equipGroup, mCurSelEquipData);
			
			// set star level
			//GameCommon.SetStarLevelLabel(equipGroup, mCurSelEquipData.mStarLevel, "equip_star_level_label");
            GameCommon.SetUIText(equipGroup, "equip_star_level_label", mCurSelEquipData.mStarLevel.ToString());
			
			// set name
			UILabel name = GameCommon.FindObject(equipGroup, "props_name").GetComponent<UILabel>();
			name.text = TableCommon.GetStringFromRoleEquipConfig(mCurSelEquipData.tid, "NAME");
			
			// set description
			UILabel descrition = GameCommon.FindObject(equipGroup, "introduce_label").GetComponent<UILabel>();
			descrition.text = TableCommon.GetStringFromRoleEquipConfig(mCurSelEquipData.tid, "DESCRIPTION");
		}
	}

	public void SortRoleEquipIcons(SORT_TYPE sortType)
	{
		mSortType = sortType;
		
		InitPackageIcons();
	}

	public void SaleOK()
	{
		if(mCurSelEquipData == null)
			return;
		
		if(CommonParam.bIsNetworkGame)
		{
			CS_RequestRoleEquipSale quest = Net.StartEvent("CS_RequestRoleEquipSale") as CS_RequestRoleEquipSale;
            quest.set("DBID", mCurSelEquipData.itemId);
            quest.mAction = () => DataCenter.SetData("ROLE_EQUIP_PACKAGE_WINDOW", "SALE_RESULT", true);
            quest.DoEvent();
		}
		else
		{
			SaleResult();
		}
	}

	public void SaleResult()
	{
		// set role equip data			
		RoleEquipLogicData roleEquipLogicData = DataCenter.GetData("EQUIP_DATA") as RoleEquipLogicData;
		if(roleEquipLogicData != null)
		{
			if(roleEquipLogicData.RemoveRoleEquip(mCurSelEquipData))
			{
				DEBUG.Log("sale role equip DBID = " + mCurSelEquipData.itemId.ToString() + " is successful, remove it OK");
				// gain gold
                int iGainGlod = TableCommon.GetNumberFromRoleEquipConfig(mCurSelEquipData.tid, "PRICE");
				GameCommon.RoleChangeGold(iGainGlod);
			}
		}

		DataCenter.OpenWindow("ROLE_EQUIP_PACKAGE_WINDOW");
	}
}


public class Button_PackageRoleEquipStarBtn : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.SetData("ROLE_EQUIP_PACKAGE_WINDOW", "SORT_ROLE_EQUIP_ICONS", SORT_TYPE.STAR_LEVEL);
		return true;
	}
}

public class Button_PackageRoleEquipStrengthenLevelBtn : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.SetData("ROLE_EQUIP_PACKAGE_WINDOW", "SORT_ROLE_EQUIP_ICONS", SORT_TYPE.STRENGTHEN_LEVEL);
		return true;
	}
}

public class Button_PackageRoleEquipAttributeBtn : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.SetData("ROLE_EQUIP_PACKAGE_WINDOW", "SORT_ROLE_EQUIP_ICONS", SORT_TYPE.ELEMENT_INDEX);
		return true;
	}
}

public class Button_PackageRoleEquipIconBtn : CEvent
{
	public override bool _DoEvent()
	{
		object val;		
		bool b = getData("BUTTON", out val);
		GameObject obj = val as GameObject;
		UIToggle toggle = obj.GetComponent<UIToggle>();
		toggle.value = !toggle.value;
		
		RoleEquipLogicData logicData = DataCenter.GetData("EQUIP_DATA") as RoleEquipLogicData;
		int iGridIndex = (int)get("GRID_INDEX");
		EquipData equipData = logicData.GetEquipDataByGridIndex(iGridIndex);
		
		if(equipData != null)
		{
			DataCenter.SetData("ROLE_EQUIP_PACKAGE_WINDOW", "REFRESH", iGridIndex);
		}
		
		return true;
	}
}

public class Button_RoleEquipPackageSaleButton : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.SetData("ROLE_EQUIP_PACKAGE_WINDOW", "SALE", true);
		
		return true;
	}
}

public class Button_RoleEquipPackageControlButton : CEvent
{
	public override bool _DoEvent()
	{
		if (CommonParam.bIsNetworkGame)
		{
			CS_RequestRoleEquip quest = Net.StartEvent("CS_RequestRoleEquip") as CS_RequestRoleEquip;
			quest.set ("WINDOW_NAME", "ROLE_EQUIP_CULTIVATE_WINDOW");
			quest.mAction = () => {
				DataCenter.CloseWindow("PACKAGE_WINDOW");
				MainUIScript.Self.OpenMainWindowByIndex(MAIN_WINDOW_INDEX.AllRoleAttributeInfoWindow);
			};
			quest.mBackAction = () => {
				EventCenter.Start("Button_package_btn").DoEvent();
			};
			quest.DoEvent();
			
			tEvent gemQuest = Net.StartEvent("CS_RequestGem");
			gemQuest.DoEvent();
		}
		else
		{
			MainUIScript.Self.OpenMainWindowByIndex(MAIN_WINDOW_INDEX.AllRoleAttributeInfoWindow);
			MainUIScript.Self.mStrAllRoleAttInfoPageWindowName = "ROLE_EQUIP_CULTIVATE_WINDOW";
		}
		return true;
	}
}
