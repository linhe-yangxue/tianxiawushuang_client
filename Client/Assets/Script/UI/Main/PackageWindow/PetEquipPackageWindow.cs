using UnityEngine;
using System.Collections;
using Logic;

public class PetEquipPackageWindow : ItemPackageBaseWindow {

//    // pet equip
//    public PetEquipLogicData mPetEquipLogicData;
//    public SORT_TYPE mSortType;
	
//    PetEquipData mCurSelEquipData = null;
	
//    public override void Init ()
//    {
//        base.Init ();
		
//        EventCenter.Self.RegisterEvent("Button_package_pet_equip_star_btn", new DefineFactory<Button_PackagePetEquipStarBtn>());
//        EventCenter.Self.RegisterEvent("Button_package_pet_equip_strengthen_level_btn", new DefineFactory<Button_PackagePetEquipStrengthenLevelBtn>());
//        EventCenter.Self.RegisterEvent("Button_package_pet_equip_attribute_btn", new DefineFactory<Button_PackagePetEquipAttributeBtn>());
		
//        EventCenter.Self.RegisterEvent("Button_package_pet_equip_icon_btn", new DefineFactory<Button_PackagePetEquipIconBtn>());
//        EventCenter.Self.RegisterEvent("Button_pet_equip_package_sale_button", new DefineFactory<Button_PetEquipPackageSaleButton>());
//        EventCenter.Self.RegisterEvent("Button_pet_equip_package_control_button", new DefineFactory<Button_PetEquipPackageControlButton>());
		
//    }
//    public override void onChange (string keyIndex, object objVal)
//    {
//        base.onChange (keyIndex, objVal);
//        if(keyIndex == "SORT_PET_EQUIP_ICONS")
//        {
//            SortPetEquipIcons((SORT_TYPE)objVal);
//        }
//        else if(keyIndex == "SALE")
//        {
//            SaleOK();
//        }
//        else if(keyIndex == "SALE_RESULT")
//        {
//            SaleResult();
//        }
//    }
	
//    public override void OnOpen ()
//    {
//        base.OnOpen ();
//    }
	
//    public override void InitVariable()
//    {
//        mPetEquipLogicData = DataCenter.GetData("PET_EQUIP_DATA") as PetEquipLogicData;

//        mSortType = SORT_TYPE.STAR_LEVEL;
//        InitPackageIcons();
//        RefreshBagNum();
		
//        Refresh(0);
//    }
	
//    public override bool Refresh (object param)
//    {
//        base.Refresh (param);
		
////		mCurSelEquipData = mPetEquipLogicData.GetEquipDataByGridIndex(mCurSelGridIndex) as PetEquipData;
		
//        RefreshInfoWindow();
		
//        return true;
//    }
	
	
//    public override void  InitPackageIcons()
//    {
//        RoleLogicData roleLogic = RoleLogicData.Self;
//        mGrid.MaxCount = roleLogic.mMaxPetEquipNum;
		
//        int iIndex = 0;
		
//        InitUsedEquipIcons(ref iIndex);
		
//        InitUnUsedEquipIcons(iIndex);
//    }
	
//    public void InitUsedEquipIcons(ref int iIndex)
//    {
////		PetEquipLogicData logicData = DataCenter.GetData("PET_EQUIP_DATA") as PetEquipLogicData;
////		EquipData equip = mPetEquipLogicData.GetUseEquip();
////		if (equip != null)
////		{
////			equip.mGridIndex = iIndex;
////			GameObject tempObj = mGrid.controlList[iIndex];
////			GameObject obj = tempObj.transform.Find("package_pet_equip_icon_btn").gameObject;
////			UIButtonEvent buttonEvent = obj.GetComponent<UIButtonEvent>();
////			if (buttonEvent != null)
////				buttonEvent.mData.set("GRID_INDEX", equip.mGridIndex);
////			
////			SetEquipIcon(obj, equip, true);
////			iIndex++;
////		}
		
//    }
	
//    public void InitUnUsedEquipIcons(int iIndex)
//    {
//        int iItemId = -1;
////		EquipData equip = mPetEquipLogicData.GetUseEquip();
////		if (equip != null)
////		{
////			iItemId = equip.mDBID;
////		}
////		
////		List<EquipData> equipList = mPetEquipLogicData.mDicEquip.Values.ToList();
////		equipList = SortList(equipList);
////		for(int i = 0; i < equipList.Count; i++)
////		{
////			EquipData tempEquip = equipList[i];
////			if(tempEquip != null)
////			{
////				if(tempEquip.mDBID != iItemId && tempEquip.mUserID == 0)
////				{
////					tempEquip.mGridIndex = iIndex;
////					GameObject tempObj = mGrid.controlList[iIndex];
////					GameObject obj = tempObj.transform.Find("package_pet_equip_icon_btn").gameObject;
////					UIButtonEvent buttonEvent = obj.GetComponent<UIButtonEvent>();
////					if(buttonEvent != null)
////						buttonEvent.mData.set("GRID_INDEX", iIndex);
////					
////					SetEquipIcon(obj, tempEquip, false);
////					iIndex++;
////				}
////			}
////		}
		
//        //        foreach (KeyValuePair<int, EquipData> pair in mPetEquipLogicData.mDicEquip)
//        //        {
//        //            if (pair.Key != iItemId && pair.Value.mUserID == 0)
//        //            {
//        //				pair.Value.mGridIndex = iIndex;
//        //				GameObject tempObj = mGrid.controlList[iIndex];
//        //                GameObject obj = tempObj.transform.Find("package_pet_equip_icon_btn").gameObject;
//        //                UIButtonEvent buttonEvent = obj.GetComponent<UIButtonEvent>();
//        //                if(buttonEvent != null)
//        //					buttonEvent.mData.set("GRID_INDEX", iIndex);
//        //
//        //				SetEquipIcon(obj, pair.Value, false);
//        //				iIndex++;
//        //            }
//        //        }
		
//        RoleLogicData roleLogic = RoleLogicData.Self;
//        for (int j = iIndex; j < roleLogic.mMaxPetEquipNum; j++)
//        {
//            GameObject tempObj = mGrid.controlList[iIndex];
//            GameObject obj = tempObj.transform.Find("package_pet_equip_icon_btn").gameObject;
			
//            UIButtonEvent buttonEvent = obj.GetComponent<UIButtonEvent>();
//            if(buttonEvent != null)
//                buttonEvent.mData.set("GRID_INDEX", iIndex);
			
//            if (obj != null)
//            {
//                SetEquipIcon(obj, null, false);
//                iIndex++;
//            }
//        }
//    }
	
////	public List<EquipData> SortList(List<EquipData> list)
////	{
////		if(list != null && list.Count > 0)
////		{
////			switch(mSortType)
////			{
////			case SORT_TYPE.STAR_LEVEL:
////				list = list.OrderByDescending(p => p.mStarLevel).
////					ThenByDescending(p => p.mStrengthenLevel).
////						ThenBy(p => p.mModelIndex).
////						ThenBy(p => (int)p.mElementType).
////						ToList();
////				break;
////			case SORT_TYPE.STRENGTHEN_LEVEL:
////				list = list.OrderByDescending(p => p.mStrengthenLevel).
////					ThenByDescending(p => p.mStarLevel).
////						ThenBy(p => p.mModelIndex).
////						ThenBy(p => (int)p.mElementType).
////						ToList();
////				break;
////			case SORT_TYPE.ELEMENT_INDEX:
////				list = list.OrderBy(p => (int)p.mElementType).
////					ThenByDescending(p => p.mStarLevel).
////						ThenByDescending(p => p.mStrengthenLevel).
////						ThenBy(p => p.mModelIndex).
////						ToList();
////				break;
////			}
////		}
////		
////		return list;
////	}
	
//    public bool SetEquipIcon(GameObject obj, EquipData equipData, bool bIsUsed)
//    {
//        if (obj == null)
//            return false;
		
//        GameObject background = GameCommon.FindObject(obj, "icon_sprite");
//        GameObject emptyBG = GameCommon.FindObject(obj, "empty_bg");
		
//        if(equipData == null)
//        {
//            background.SetActive(false);
//            emptyBG.SetActive(true);
			
//            UIToggle toggle = obj.GetComponent<UIToggle>();
//            if (toggle != null)
//                toggle.value = false;
//        }
////		else
////		{
////			background.SetActive(true);
////			emptyBG.SetActive(false);
////			
////			UIToggle toggle = obj.GetComponent<UIToggle>();
////			if (toggle != null)
////				toggle.value = bIsUsed;
////			
////			SetIcon(obj, equipData);
////		}
		
//        return true;
//    }
////	
////	public void SetIcon(GameObject obj, EquipData equipData)
////	{
////		if(obj != null && equipData != null)
////		{
////			// set equip icon
////			GameCommon.SetPetEquipIcon(obj, equipData.mModelIndex);
////			
////			// set element icon
////			int iElementIndex = (int)equipData.mElementType;
////			GameCommon.SetElementIcon(obj, iElementIndex);
////			
////			// set equip element background icon
////			GameCommon.SetEquipElementBgIcon(obj, iElementIndex);
////			
////			// set star level
////			GameCommon.SetStarLevelLabel(obj, equipData.mStarLevel);
////			
////			// set strengthen level text
////			GameCommon.SetStrengthenLevelLabel(obj, equipData.mStrengthenLevel);
////		}
////	}
	
//    public override void RefreshBagNum()
//    {
//        UILabel PetEquipNumLabel = mGameObjUI.transform.Find("num_label").GetComponent<UILabel>();
//        if(PetEquipNumLabel != null)
//        {
////			PetEquipNumLabel.text = mPetEquipLogicData.mDicEquip.Count.ToString() + " / " + RoleLogicData.Self.mMaxPetEquipNum.ToString();
//            PetEquipNumLabel.text = "0" + " / " + "60";
//        }
//    }
	
//    public override void RefreshInfoWindow()
//    {
//        GameObject equipGroup = mGameObjUI.transform.Find("info_window/group").gameObject;
//        GameObject equipEmptyGroup = mGameObjUI.transform.Find("info_window/empty_bg").gameObject;
//        if (mCurSelEquipData == null)
//        {
//            equipGroup.SetActive(false);
//            equipEmptyGroup.SetActive(true);
//        }
////		else
////		{
////			equipGroup.SetActive(true);
////			equipEmptyGroup.SetActive(false);
////			
////			SetIcon(equipGroup, mCurSelEquipData);
////			
////			// set star level
////			GameCommon.SetStarLevelLabel(equipGroup, mCurSelEquipData.mStarLevel, "equip_star_level_label");
////			
////			// set name
////			UILabel name = GameCommon.FindObject(equipGroup, "props_name").GetComponent<UILabel>();
////			name.text = TableCommon.GetStringFromPetEquipConfig(mCurSelEquipData.mModelIndex, "NAME");
////			
////			// set description
////			UILabel descrition = GameCommon.FindObject(equipGroup, "introduce_label").GetComponent<UILabel>();
////			descrition.text = TableCommon.GetStringFromPetEquipConfig(mCurSelEquipData.mModelIndex, "DESCRIPTION");
////		}
//    }
	
//    public void SortPetEquipIcons(SORT_TYPE sortType)
//    {
//        mSortType = sortType;
		
//        InitPackageIcons();
//    }
	
//    public void SaleOK()
//    {
//        if(mCurSelEquipData == null)
//            return;
		
//        if(CommonParam.bIsNetworkGame)
//        {
////			CS_RequestPetEquipSale quest = Net.StartEvent("CS_RequestPetEquipSale") as CS_RequestPetEquipSale;
////			quest.set("DBID", mCurSelEquipData.mDBID);
////			quest.mAction = () => DataCenter.SetData("pet_equip_PACKAGE_WINDOW", "SALE_RESULT", true);
////			quest.DoEvent();
//        }
//        else
//        {
//            SaleResult();
//        }
//    }
	
//    public void SaleResult()
//    {
////		// set role equip data		
////		PetEquipLogicData PetEquipLogicData = DataCenter.GetData("PET_EQUIP_DATA") as PetEquipLogicData;
////		if(PetEquipLogicData != null)
////		{
////			PetEquipLogicData.RemovePetEquip(mCurSelEquipData);
////		}
////		
////		// refresh bag role equip icons
////		DataCenter.OpenWindow("pet_equip_PACKAGE_WINDOW");
////		
////		// gain gold
////		int iBaseGainGoldNum = TableCommon.GetNumberFromPetEquipEvolutionConfig(mCurSelEquipData.mStarLevel, "PRICE");
////		int iDGainGoldNum = TableCommon.GetNumberFromPetEquipEvolutionConfig(mCurSelEquipData.mStarLevel, "PRICE_ADD");
////		
////		int iGainGlod = iBaseGainGoldNum + mCurSelEquipData.mStrengthenLevel * iDGainGoldNum;
////		GameCommon.RoleChangeGold(iGainGlod);
//    }
//}

//public class Button_PackagePetEquipStarBtn : CEvent
//{
//    public override bool _DoEvent()
//    {
//        DataCenter.SetData("PET_EQUIP_PACKAGE_WINDOW", "SORT_PET_EQUIP_ICONS", SORT_TYPE.STAR_LEVEL);
//        return true;
//    }
//}

//public class Button_PackagePetEquipStrengthenLevelBtn : CEvent
//{
//    public override bool _DoEvent()
//    {
//        DataCenter.SetData("PET_EQUIP_PACKAGE_WINDOW", "SORT_PET_EQUIP_ICONS", SORT_TYPE.STRENGTHEN_LEVEL);
//        return true;
//    }
//}

//public class Button_PackagePetEquipAttributeBtn : CEvent
//{
//    public override bool _DoEvent()
//    {
//        DataCenter.SetData("PET_EQUIP_PACKAGE_WINDOW", "SORT_PET_EQUIP_ICONS", SORT_TYPE.ELEMENT_INDEX);
//        return true;
//    }
//}

//public class Button_PackagePetEquipIconBtn : CEvent
//{
//    public override bool _DoEvent()
//    {
//        object val;		
//        bool b = getData("BUTTON", out val);
//        GameObject obj = val as GameObject;
//        UIToggle toggle = obj.GetComponent<UIToggle>();
//        toggle.value = !toggle.value;
		
//        PetEquipLogicData logicData = DataCenter.GetData("PET_EQUIP_DATA") as PetEquipLogicData;
//        int iGridIndex = (int)get("GRID_INDEX");
//        EquipData equipData = logicData.GetEquipDataByGridIndex(iGridIndex);
		
//        if(equipData != null)
//        {
//            DataCenter.SetData("PET_EQUIP_PACKAGE_WINDOW", "REFRESH", iGridIndex);
//        }
		
//        return true;
//    }
//}

//public class Button_PetEquipPackageSaleButton : CEvent
//{
//    public override bool _DoEvent()
//    {
//        DataCenter.SetData("PET_EQUIP_PACKAGE_WINDOW", "SALE", true);
		
//        return true;
//    }
//}

//public class Button_PetEquipPackageControlButton : CEvent
//{
//    public override bool _DoEvent()
//    {
////		if (CommonParam.bIsNetworkGame)
////		{
////			CS_RequestPetEquip quest = Net.StartEvent("CS_RequestPetEquip") as CS_RequestPetEquip;
////			quest.set ("WINDOW_NAME", "PET_EQUIP_CULTIVATE_WINDOW");
////			quest.mAction = () => {
////				DataCenter.CloseWindow("PACKAGE_WINDOW");
////				MainUIScript.Self.OpenMainWindowByIndex(MAIN_WINDOW_INDEX.AllRoleAttributeInfoWindow);
////			};
////			quest.mBackAction = () => {
////				EventCenter.Start("Button_package_btn").DoEvent();
////			};
////			quest.DoEvent();
////			
////			tEvent gemQuest = Net.StartEvent("CS_RequestGem");
////			gemQuest.DoEvent();
////		}
////		else
////		{
////			MainUIScript.Self.OpenMainWindowByIndex(MAIN_WINDOW_INDEX.AllRoleAttributeInfoWindow);
////			MainUIScript.Self.mStrAllRoleAttInfoPageWindowName = "PET_EQUIP_CULTIVATE_WINDOW";
////		}
//        return true;
//    }
}


