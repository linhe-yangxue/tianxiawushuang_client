using UnityEngine;
using System.Collections;
using Logic;
using System.Collections.Generic;

public enum LOCK_STATE
{
    UNLOCK,
    LOCKED,
}
public class RoleEquipCultivateWindow : tWindow
{
    public GameObject mRoleEquipInfoGroup;
	GameObject mRoleEquipInfoEmptyBG;
    GameObject mRoleEquipIconInfo;
    GameObject mRoleEquipAttributeInfo;
   
    int mCurSelGridIndex = 0;
	public EquipData mCurSelEquipData;

    public override void OnOpen()
    {
        DataCenter.CloseWindow("ROLE_INFO_WINDOW");
        DataCenter.CloseWindow("STAR_LEVEL_UP_WINDOW");
        DataCenter.CloseWindow("ROLE_EQUIP_COMPOSITION_WINDOW");

        mRoleEquipInfoGroup = mGameObjUI.transform.Find("role_equip_info_group").gameObject;
        mRoleEquipInfoEmptyBG = mGameObjUI.transform.Find("role_equip_info_empty_bg").gameObject;
        mRoleEquipIconInfo = mRoleEquipInfoGroup.transform.Find("RoleEquipIconInfo").gameObject;
        mRoleEquipAttributeInfo = mRoleEquipInfoGroup.transform.Find("RoleEquipAttributeInfo").gameObject;
        mRoleEquipAttributeInfo.SetActive(true);

        SetBtnGroupVisible(true);
        SetRoleEquipInfoBtnGroupVisible(true);
        SetRoleEquipResetInfoVisible(false);

        DataCenter.OpenWindow("BAG_INFO_WINDOW");
        DataCenter.SetData("BAG_INFO_WINDOW", "SHOW_WINDOW", BAG_INFO_TITLE_TYPE.RoleEquipWindow);

        Refresh(0);

        InitButtonIsUnLockOrNot();
    }

	public void SetBtnGroupVisible(bool bIsVisible)
	{
        mRoleEquipInfoGroup.SetActive(bIsVisible);
	}

	public void SetRoleEquipInfoBtnGroupVisible(bool bIsVisible)
	{
		GameObject btnGroup = GameCommon.FindObject(mRoleEquipInfoGroup, "role_equip_info_btn_group");
		if(btnGroup != null)
			btnGroup.SetActive(bIsVisible);
	}

	public void SetRoleEquipResetInfoVisible(bool bIsVisible)
	{
		GameObject btnGroup = GameCommon.FindObject(mRoleEquipInfoGroup, "need_lock_ticket");
		if(btnGroup != null)
		{
			btnGroup.SetActive(bIsVisible);
			
			if(bIsVisible)
			{
				UILabel lockTicketNumLabel = GameCommon.FindObject(mRoleEquipInfoGroup, "lock_ticket_num").GetComponent<UILabel>();
				lockTicketNumLabel.text = RoleLogicData.Self.mLockNum.ToString();
			}
		}
		
		UnlockCurSelRoleEquip();

        if (mCurSelEquipData == null)
            return;

		UIGridContainer grid = mRoleEquipAttributeInfo.transform.Find("added_attribute/Grid").GetComponent<UIGridContainer>();
		if (grid == null)
			return;

		int iLockedNum = 0;
		for(int i = 0; i < grid.MaxCount; i++)
		{
			GameObject obj = grid.controlList[i];
			if(obj != null)
			{
				SetLockBtnState(obj, mCurSelEquipData.mAttachAttributeList[i].mLockState, bIsVisible);
			}
		}
	}

	public void SetLockBtnState(GameObject obj, LOCK_STATE lockState)
	{
		SetLockBtnState(obj, lockState, true);
	}

	public void SetLockBtnState(GameObject obj, LOCK_STATE lockState, bool bIsVisible)
	{
		if(obj == null)
			return;

		UIImageButton lockButton = GameCommon.FindObject(obj, "lock_button").GetComponent<UIImageButton>();
		GameObject hadLockedButton = GameCommon.FindObject(obj, "had_locked_button");

		if(bIsVisible)
		{
			switch(lockState)
			{
			case LOCK_STATE.LOCKED:
				hadLockedButton.SetActive(true);
				lockButton.gameObject.SetActive(false);
				break;
			case LOCK_STATE.UNLOCK:
				if(IsCannotLock())
				{
					hadLockedButton.SetActive(false);
					lockButton.gameObject.SetActive(true);
					lockButton.isEnabled = false;
				}
				else
				{
					hadLockedButton.SetActive(false);
					lockButton.gameObject.SetActive(true);
					lockButton.isEnabled = true;
				}
				break;
			}
		}
		else
		{
			hadLockedButton.SetActive(false);
			lockButton.gameObject.SetActive(false);
		}
	}

	public bool IsCannotLock()
	{
		int iAttributeCount = mCurSelEquipData.mAttachAttributeList.Count;
		int iUnlockCount = iAttributeCount;
		for(int i = 0; i < iAttributeCount; i++)
		{
			if(mCurSelEquipData.mAttachAttributeList[i].mLockState == LOCK_STATE.LOCKED)
				iUnlockCount--;
		}

		return iUnlockCount == 1;
	}

	public void UpdateCurSelRoleEquip(int iGridIndex, LOCK_STATE lockState)
	{
		mCurSelEquipData.mAttachAttributeList[iGridIndex].mLockState = lockState;
	}

	public void UnlockCurSelRoleEquip()
	{
		if (mCurSelEquipData == null)
			return;

		for(int i = 0; i < mCurSelEquipData.mAttachAttributeList.Count; i++)
		{
			mCurSelEquipData.mAttachAttributeList[i].mLockState = LOCK_STATE.UNLOCK;
		}
	}

	public void RefreshLockAttribute(int iGridIndex, LOCK_STATE state)
	{
        UpdateCurSelRoleEquip(iGridIndex, state);

        RefreshLockAttributeUI();
	}

    void RefreshLockAttributeUI()
    {
        UIGridContainer grid = mRoleEquipAttributeInfo.transform.Find("added_attribute/Grid").GetComponent<UIGridContainer>();
        if (grid == null)
            return;

        int iLockedNum = 0;
        for (int i = 0; i < grid.MaxCount; i++)
        {
            GameObject obj = grid.controlList[i];
            if (obj != null)
            {
                LOCK_STATE lockState = mCurSelEquipData.mAttachAttributeList[i].mLockState;
                SetLockBtnState(obj, lockState);
                if (lockState == LOCK_STATE.LOCKED)
                    iLockedNum++;
            }
        }

        UILabel lockTicketNumLabel = GameCommon.FindObject(mRoleEquipInfoGroup, "lock_ticket_num").GetComponent<UILabel>();
        lockTicketNumLabel.text = (RoleLogicData.Self.mLockNum - iLockedNum).ToString();
    }

	void InitButtonIsUnLockOrNot()
	{
		GameCommon.ButtonEnbleButCanClick (mGameObjUI, "RoleEquipStrengthenBtn", UNLOCK_FUNCTION_TYPE.STRENGTHER_FA_BAO);
		GameCommon.ButtonEnbleButCanClick (mGameObjUI, "RoleEquipResetBtn", UNLOCK_FUNCTION_TYPE.RESET_FA_BAO);
	}

    public override void Close()
    {
        DataCenter.CloseWindow("BAG_INFO_WINDOW");
        DataCenter.CloseWindow("ExpansionInfoWindow");

        base.Close();
    }

	public override bool Refresh(object param)
	{
//		DataCenter.SetData("RoleEquipWindow", "HIDE_SELECT_ICON", mCurSelGridIndex);
		mCurSelGridIndex = (int)param;
//		DataCenter.SetData("RoleEquipWindow", "SHOW_SELECT_ICON", mCurSelGridIndex);
        SetCurSelEquipData();
		UpdateRoleEquipInfo();
        DataCenter.SetData("ExpansionInfoWindow", "SET_GRID_INDEX", mCurSelGridIndex);
		return true;
	}

    public void SetCurSelEquipData()
    {
		RoleEquipLogicData roleEquipLogicData = DataCenter.GetData("EQUIP_DATA") as RoleEquipLogicData;
		mCurSelEquipData = roleEquipLogicData.GetEquipDataByGridIndex(mCurSelGridIndex);
    }
    public override void onChange(string keyIndex, object objVal)
    {
        base.onChange(keyIndex, objVal);
        switch (keyIndex)
        {
			case "SET_ATTRIBUTE_INFO_UI_VISIBLE":
				mRoleEquipAttributeInfo.SetActive((bool)objVal);
				break;
			case "SET_BTN_GROUP_VISIBLE":
				SetBtnGroupVisible((bool)objVal);
				break;
			case "SET_ROLE_EQUIP_INFO_BTN_GROUP_VISIBLE":
				SetRoleEquipInfoBtnGroupVisible((bool)objVal);
				break;
			case "SET_ROLE_EQUIP_RESET_INFO_VISIBLE":
				SetRoleEquipResetInfoVisible((bool)objVal);
				break;
			case "UNLOCK_ATTRIBUTE":
                RefreshLockAttribute((int)objVal, LOCK_STATE.UNLOCK);
				break;
			case "LOCK_ATTRIBUTE":
                RefreshLockAttribute((int)objVal, LOCK_STATE.LOCKED);
				break;
			case "USE":
				UseOK();
				break;
			case "UNUSE":
				UnuseOK();
				break;
			case "SALE":
                // gain gold
                int iGainGlod = TableCommon.GetNumberFromRoleEquipConfig(mCurSelEquipData.tid, "PRICE");
                DataCenter.OpenMessageOkWindow(STRING_INDEX.ERROR_ROLE_EQUIP_SALE, iGainGlod.ToString(), () => SaleOK());
				break;
			case "USE_RESULT":
				UseResult();
				break;
			case "SALE_RESULT":
				SaleResult();
				break;
        }
    }
	public void UpdateRoleEquipInfo()
	{
		if (mCurSelEquipData == null)
		{
			mRoleEquipInfoGroup.SetActive(false);
			mRoleEquipInfoEmptyBG.SetActive(true);
		}
		else
		{
			mRoleEquipInfoGroup.SetActive(true);
			mRoleEquipInfoEmptyBG.SetActive(false);
		}

		UpdateRoleEquipIconInfo();
		UpdateRoleEquipAttributeInfo();
	}
	
	public void UpdateRoleEquipIconInfo()
	{
		if (mCurSelEquipData == null)
			return;

		int iEquipType = TableCommon.GetNumberFromRoleEquipConfig(mCurSelEquipData.tid, "ROLEEQUIP_TYPE");

        SetRoleEquipIcon();
        SetEquipName();

		if(iEquipType == (int)EQUIP_TYPE.ELEMENT_EQUIP)
		{
			// set element icon
			GameCommon.SetElementIcon(mRoleEquipIconInfo, (int)mCurSelEquipData.mElementType);
		}
		else
		{
			GameCommon.SetIcon(mRoleEquipIconInfo, "Element", "", "");
		}

		// set use button visible
		bool bIsVisible = true;
		RoleEquipLogicData logic = DataCenter.GetData("EQUIP_DATA") as RoleEquipLogicData;
		if(logic != null)
		{
			RoleEquipData roleEuqip = logic.GetUseEquip(iEquipType) as RoleEquipData;
			if(roleEuqip != null && roleEuqip.itemId == mCurSelEquipData.itemId)
			{
				bIsVisible = false;
			}
		}
		SetUseVisible(bIsVisible);
	}

    public void SetRoleEquipIcon()
    {
		GameObject obj = mRoleEquipIconInfo.transform.Find("role_equip_icon_group").gameObject;

        // set role equip icon
		GameCommon.SetEquipIcon(obj, mCurSelEquipData.tid);

		// set element icon and set equip element background icon
		int iElementIndex = (int)mCurSelEquipData.mElementType;
		GameCommon.SetEquipElementBgIcons(obj, mCurSelEquipData.tid, iElementIndex);
		
        // set star level
		GameCommon.SetStarLevelLabel(obj, mCurSelEquipData.mStarLevel);

        // set strengthen level text
		GameCommon.SetStrengthenLevelLabel(obj, mCurSelEquipData.strengthenLevel);
    }

    public void SetEquipName()
    {
        GameObject equipNameObj = GameCommon.FindObject(mRoleEquipIconInfo, "EquipNameLabel");
        UILabel equipNameLabel = equipNameObj.GetComponent<UILabel>();
		equipNameLabel.text = GetEquipQualityColor() + TableCommon.GetStringFromRoleEquipConfig(mCurSelEquipData.tid, "NAME");
    }

    public string GetEquipQualityColor()
    {
        string strQualityColor = "";

        switch (mCurSelEquipData.mQualityType)
        {
            case EQUIP_QUALITY_TYPE.LOW:
                break;
            case EQUIP_QUALITY_TYPE.MIDDLE:
				strQualityColor = "[6bc235]";
                break;
            case EQUIP_QUALITY_TYPE.GOOD:
				strQualityColor = "[005aab]";
                break;
			case EQUIP_QUALITY_TYPE.BETTER:
				strQualityColor = "[5a0d43]";
				break;
            case EQUIP_QUALITY_TYPE.BEST:
				strQualityColor = "[f8d61d]";
                break;
        }

		return strQualityColor;
    }

	public void UpdateRoleEquipAttributeInfo()
	{
        SetBaseAttribute();
        SetAttachAttribute();
	}

    public void SetBaseAttribute()
    {
        if (mCurSelEquipData == null)
            return;

        for (int i = 0; i < 2; i++)
        {
            int iAttributeType = TableCommon.GetNumberFromRoleEquipConfig(mCurSelEquipData.tid, "ATTRIBUTE_TYPE_" + i.ToString());
            if (iAttributeType <= (int)AFFECT_TYPE.NONE || iAttributeType >= (int)AFFECT_TYPE.MAX)
                return;

            GameObject baseObj = GameCommon.FindObject(mRoleEquipAttributeInfo, "base_attribute_" + i.ToString());
            // set icon
            SetAttributeIcon(baseObj, iAttributeType);

            // set name
            SetAttributeName(baseObj, iAttributeType);

            // set base number
            SetBaseAttributeValue(i, baseObj, iAttributeType, mCurSelEquipData.strengthenLevel);
        }
    }
    

    public void SetAttachAttribute()
    {
        if (mCurSelEquipData == null)
            return;

        UIGridContainer grid = mRoleEquipAttributeInfo.transform.Find("added_attribute/Grid").GetComponent<UIGridContainer>();
        if (grid == null)
            return;

		grid.MaxCount = mCurSelEquipData.mAttachAttributeList.Count;

        for (int i = 0; i < grid.MaxCount; i++)
        {
            EquipAttachAttribute attribute = mCurSelEquipData.mAttachAttributeList[i];
            if(attribute != null)
            {
                GameObject gridObj = grid.controlList[i];
                int iAttributeType = (int)attribute.mType;
                if ((AFFECT_TYPE)iAttributeType == AFFECT_TYPE.NONE)
					return;

				GameObject lockButton = GameCommon.FindObject(gridObj, "lock_button");
				GameObject hadLockedButton = GameCommon.FindObject(gridObj, "had_locked_button");
				UIButtonEvent lockButtonEvent = lockButton.GetComponent<UIButtonEvent>();
				UIButtonEvent hadLockedButtonEvent = hadLockedButton.GetComponent<UIButtonEvent>();
				lockButtonEvent.mData.set ("GRID_INDEX", i);
				hadLockedButtonEvent.mData.set("GRID_INDEX", i);

				// set icon
				SetAttributeIcon(gridObj, iAttributeType);

                // set name
                SetAttributeName(gridObj, iAttributeType);

                // set attach number
                SetAttachAttributeValue(gridObj, iAttributeType, attribute.mValue);

				// set attach label color
				SetAttachAttributeColor(gridObj, iAttributeType);
            }
        }
    }

    // set equip attribute icon
    public void SetAttributeIcon(GameObject obj, int iIndex)
    {
		string strAtlasName = TableCommon.GetStringFromEquipAttributeIconConfig(iIndex, "ICON_ATLAS_NAME");
		string strSpriteName = TableCommon.GetStringFromEquipAttributeIconConfig(iIndex, "ICON_SPRITE_NAME");

        UIAtlas tu = GameCommon.LoadUIAtlas(strAtlasName);
        UISprite sprite = GameCommon.FindObject(obj, "icon").GetComponent<UISprite>();
        sprite.atlas = tu;
        sprite.spriteName = strSpriteName;
        //sprite.MakePixelPerfect();
    }

	// set attribute name
	public void SetAttributeName(GameObject obj, int iAttributeType)
    {
        if (obj == null || iAttributeType <= (int)AFFECT_TYPE.NONE || iAttributeType >= (int)AFFECT_TYPE.MAX)
            return;

        GameObject name = obj.transform.Find("name").gameObject;
        UILabel nameLabel = name.GetComponent<UILabel>();
        if (nameLabel != null)
			nameLabel.text = TableCommon.GetStringFromEquipAttributeIconConfig(iAttributeType, "NAME");
    }

	public void SetBaseAttributeValue(int iIndex, GameObject obj, int iAttributeType, int iStrengthenLevel)
    {
        UILabel numLabel = GetAttributeNumLabel(obj, iAttributeType);
        float fValue = 0.0f;
        if (numLabel != null)
        {
            if (mCurSelEquipData != null)
            {
                fValue = RoleEquipData.GetBaseAttributeValue(iIndex, mCurSelEquipData.tid, iStrengthenLevel);

                SetAttributeValueUI(numLabel, iAttributeType, fValue);
            }
        }
    }

    public void SetAttachAttributeValue(GameObject obj, int iAttributeType, float fValue)
    {
        if (mCurSelEquipData == null || obj == null
            || iAttributeType <= (int)AFFECT_TYPE.NONE || iAttributeType >= (int)AFFECT_TYPE.MAX)
            return;

        UILabel numLabel = GetAttributeNumLabel(obj, iAttributeType);
        if (numLabel != null)
        {
            SetAttributeValueUI(numLabel, iAttributeType, fValue);
        }
    }

	// set attach equip attribute color
	public void SetAttachAttributeColor(GameObject obj, int iAttributeType)
	{
		int iIndex = GameCommon.GetEquipAttachAttributeIndex(mCurSelEquipData.tid, iAttributeType);
		UILabel nameLabel = GameCommon.FindObject(obj, "name").GetComponent<UILabel>();
		UILabel numLabel = GameCommon.FindObject(obj, "num_label").GetComponent<UILabel>();
		if(TableCommon.GetNumberFromEquipAttachAttributeConfig(iIndex, "IS_BEST") == 1)
		{
			nameLabel.text = "[00ff00]" + nameLabel.text;
            numLabel.text = "[00ff00]" + numLabel.text;
		}
		else
		{
			nameLabel.text = "[ffffff]" + nameLabel.text;
			numLabel.text = "[ffffff]" + numLabel.text;
		}
	}

    public UILabel GetAttributeNumLabel(GameObject obj, int iAttributeType)
    {
        if (mCurSelEquipData == null || obj == null
            || iAttributeType <= (int)AFFECT_TYPE.NONE || iAttributeType >= (int)AFFECT_TYPE.MAX)
            return null;

        string strAttributeType = GameCommon.ToAffectTypeString((AFFECT_TYPE)iAttributeType);
        bool bIsRate = strAttributeType.LastIndexOf("_RATE") > 0;

		GameObject number = obj.transform.Find("num_label").gameObject;
        return number.GetComponent<UILabel>();
    }

    public void SetAttributeValueUI(UILabel numLabel, int iAttributeType, float fValue)
    {
        if (mCurSelEquipData == null || numLabel == null
            || iAttributeType <= (int)AFFECT_TYPE.NONE || iAttributeType >= (int)AFFECT_TYPE.MAX)
            return;

        string strAttributeType = GameCommon.ToAffectTypeString((AFFECT_TYPE)iAttributeType);
        bool bIsRate = strAttributeType.LastIndexOf("_RATE") > 0;

		fValue = fValue / 10000;
        string strValue = "";
        if (bIsRate)
        {
			fValue *= 100;
			int iInteger = (int)fValue;
			int iDecimal = (int)((fValue - iInteger) * 100) ;

			string strDecimal = iDecimal.ToString();
			if(iDecimal < 10)
				strDecimal = "0" + strDecimal;
			strValue = iInteger.ToString() + "." + strDecimal + "%";
        }
        else
        {
            fValue = (float)((int)fValue);
            strValue = fValue.ToString();
        }

        numLabel.text = strValue;
    }

	public void SetUseVisible(bool bIsVisible)
	{
		GameObject useBtn = mRoleEquipInfoGroup.transform.Find("RoleEquipIconInfo/RoleEquipBtnGroup/RoleEquipUseBtn").gameObject;
		GameObject unUseBtn = mRoleEquipInfoGroup.transform.Find("RoleEquipIconInfo/RoleEquipBtnGroup/RoleEquipUnUseBtn").gameObject;
		if(useBtn != null && unUseBtn != null)
		{
			useBtn.SetActive(bIsVisible);
			unUseBtn.SetActive(!bIsVisible);
		}
	}

	public void UseOK()
	{
		if(mCurSelEquipData == null)
			return;
		
		if(CommonParam.bIsNetworkGame)
		{
			CS_RequestRoleEquipUse quest = Net.StartEvent("CS_RequestRoleEquipUse") as CS_RequestRoleEquipUse;
            quest.set("DBID", mCurSelEquipData.itemId);
			quest.set("USER_ID", RoleLogicData.GetMainRole().mIndex);
			quest.mAction = () => {
				int iRoleEquipDBID = quest.get ("DBID");
				int iRoleEquipUserID = quest.get ("USER_ID");
				DataCenter.SetData("ROLE_EQUIP_CULTIVATE_WINDOW", "DBID", iRoleEquipDBID);
				DataCenter.SetData("ROLE_EQUIP_CULTIVATE_WINDOW", "USER_ID", iRoleEquipUserID);
				DataCenter.SetData("ROLE_EQUIP_CULTIVATE_WINDOW", "USE_RESULT", true);
			};
            quest.DoEvent();
		}
		else
		{
			// set role equip data
			mCurSelEquipData.mUserID = RoleLogicData.GetMainRole().mIndex;
			
			RoleEquipLogicData roleEquipLogicData = DataCenter.GetData("EQUIP_DATA") as RoleEquipLogicData;
			if(roleEquipLogicData != null)
            {
				roleEquipLogicData.SetUseEquip(TableCommon.GetNumberFromRoleEquipConfig(mCurSelEquipData.tid, "ROLEEQUIP_TYPE"), mCurSelEquipData);
			}
			
			// refresh bag role equip icons
			DataCenter.SetData("RoleEquipWindow", "UPDATE_ROLE_EQUIP_ICONS", true);

			Refresh (0);
		}
	}

	public void UseResult()
	{
		// set role equip data
		int iRoleEquipDBID = get ("DBID");
		int iRoleEquipUserID = get ("USER_ID");
		
		RoleEquipLogicData roleEquipLogicData = DataCenter.GetData("EQUIP_DATA") as RoleEquipLogicData;
		if(roleEquipLogicData != null)
		{
			EquipData equip = roleEquipLogicData.GetEquipDataByItemId(iRoleEquipDBID);
			if(equip != null)
			{
				int type = TableCommon.GetNumberFromRoleEquipConfig(equip.tid, "ROLEEQUIP_TYPE");
				equip.mUserID = iRoleEquipUserID;

				if(iRoleEquipUserID != 0)
					roleEquipLogicData.SetUseEquip(type, equip);
				else
					roleEquipLogicData.SetUseEquip(type, null);
			}
		}
		
		// refresh bag role equip icons
		DataCenter.SetData("RoleEquipWindow", "UPDATE_ROLE_EQUIP_ICONS", true);

		DataCenter.SetData("ExpansionInfoWindow", "CLOSE_WINDOW", true);
		
		Refresh (0);
	}
	
	public void UnuseOK()
	{
		if(mCurSelEquipData == null)
			return;
		
		if(CommonParam.bIsNetworkGame)
		{
			CS_RequestRoleEquipUse quest = Net.StartEvent("CS_RequestRoleEquipUse") as CS_RequestRoleEquipUse;
			quest.set("DBID", mCurSelEquipData.itemId);
			quest.set("USER_ID", 0);
			quest.mAction = () => {
				int iRoleEquipDBID = quest.get ("DBID");
				int iRoleEquipUserID = quest.get ("USER_ID");
				DataCenter.SetData("ROLE_EQUIP_CULTIVATE_WINDOW", "DBID", iRoleEquipDBID);
				DataCenter.SetData("ROLE_EQUIP_CULTIVATE_WINDOW", "USER_ID", iRoleEquipUserID);
				DataCenter.SetData("ROLE_EQUIP_CULTIVATE_WINDOW", "USE_RESULT", true);
			};
			quest.DoEvent();
		}
	}

	public void SaleOK()
	{
		if(mCurSelEquipData == null)
			return;
		
		if(CommonParam.bIsNetworkGame)
		{
			CS_RequestRoleEquipSale quest = Net.StartEvent("CS_RequestRoleEquipSale") as CS_RequestRoleEquipSale;
			quest.set("DBID", mCurSelEquipData.itemId);
			quest.mAction = () => DataCenter.SetData("ROLE_EQUIP_CULTIVATE_WINDOW", "SALE_RESULT", true);
			quest.DoEvent();
		}
		else
		{
			// set role equip data			
			RoleEquipLogicData roleEquipLogicData = DataCenter.GetData("EQUIP_DATA") as RoleEquipLogicData;
			if(roleEquipLogicData != null)
			{
				roleEquipLogicData.RemoveRoleEquip(mCurSelEquipData);
				if(roleEquipLogicData.mDicEquip.Count == 0)
				{
					//DataCenter.SetData("AllRoleAttributeInfoWindow", "SET_SEL_PAGE", ALL_ROLE_ATTRIBUTE_PAGE_TYPE.RoleEquipCultivate);
                    DataCenter.OpenWindow("ROLE_EQUIP_CULTIVATE_WINDOW");
				}
			}
			
			// refresh bag role equip icons
			DataCenter.SetData("RoleEquipWindow", "UPDATE_ROLE_EQUIP_ICONS", true);
			
			// gain gold
            int iGainGlod = TableCommon.GetNumberFromRoleEquipConfig(mCurSelEquipData.tid, "PRICE");
			GameCommon.RoleChangeGold(iGainGlod);
			
			// refresh role equip cultiavte ui
			Refresh (0);
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

			if(roleEquipLogicData.mDicEquip.Count == 0)
			{
				//DataCenter.SetData("AllRoleAttributeInfoWindow", "SET_SEL_PAGE", ALL_ROLE_ATTRIBUTE_PAGE_TYPE.RoleEquipCultivate);
                DataCenter.OpenWindow("ROLE_EQUIP_CULTIVATE_WINDOW");
			}
			else
			{
				// refresh bag role equip icons
				DataCenter.SetData("RoleEquipWindow", "REFRESH", null);	
				
				
				// refresh role equip cultiavte ui
				Refresh (0);
			}

			DataCenter.SetData("ExpansionInfoWindow", "CLOSE_WINDOW", true);
		}
	}
}
