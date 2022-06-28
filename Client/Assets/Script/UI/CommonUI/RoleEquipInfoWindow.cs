using UnityEngine;
using System.Collections;
using Logic;

public class RoleEquipInfoWindow : tWindow {
    
    public enum WINDOW_TYPE
    {
        COMMON,
        COMPOSITION,
    }
	int mCurEquipDBID = 0;
	public EquipData mCurSelEquipData;

	GameObject mRoleEquipIconInfo;
	GameObject mRoleEquipAttributeInfo;

	public override void Init ()
	{
		base.Init ();

		EventCenter.Self.RegisterEvent("Button_role_equip_info_use_btn", new DefineFactory<Button_RoleEquipInfoUseBtn>());
		EventCenter.Self.RegisterEvent("Button_role_equip_info_unuse_btn", new DefineFactory<Button_RoleEquipInfoUnUseBtn>());
		EventCenter.Self.RegisterEvent("Button_role_equip_info_sale_btn", new DefineFactory<Button_RoleEquipInfoSaleBtn>());

		EventCenter.Self.RegisterEvent("Button_role_equip_info_strengthen_button", new DefineFactory<Button_RoleEquipInfoStrengthenBtn>());
		EventCenter.Self.RegisterEvent("Button_role_equip_info_reset_button", new DefineFactory<Button_RoleEquipInfoResetBtn>());

		EventCenter.Self.RegisterEvent("Button_role_equip_info_window_close_button", new DefineFactory<Button_RoleEquipInfoWindowCloseButton>());
	}

	public override void Open (object param)
	{
		base.Open (param);

        mRoleEquipIconInfo = GameCommon.FindObject(mGameObjUI, "role_equip_icon_info").gameObject;
        mRoleEquipAttributeInfo = GameCommon.FindObject(mGameObjUI, "role_equip_attribute_info").gameObject;

		InitButtonIsUnLockOrNot();

        WINDOW_TYPE windowType = (WINDOW_TYPE)((int)get("WINDOW_TYPE"));
        GameCommon.FindObject(mGameObjUI, "role_equip_common_info_group").SetActive(windowType == WINDOW_TYPE.COMMON);
        GameCommon.FindObject(mGameObjUI, "role_equip_composition_info_group").SetActive(windowType == WINDOW_TYPE.COMPOSITION);
		Refresh((int)param);
	}

	void InitButtonIsUnLockOrNot()
	{
		GameCommon.ButtonEnbleButCanClick (mGameObjUI, "role_equip_info_reset_button", UNLOCK_FUNCTION_TYPE.RESET_FA_BAO);
		GameCommon.ButtonEnbleButCanClick (mGameObjUI, "role_equip_info_strengthen_button", UNLOCK_FUNCTION_TYPE.STRENGTHER_FA_BAO);
	}

	public override void onChange (string keyIndex, object objVal)
	{
		base.onChange (keyIndex, objVal);
		switch (keyIndex)
		{
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

	public override bool Refresh (object param)
	{
		mCurEquipDBID = (int)param;

		RoleEquipLogicData roleEquipLogicData = DataCenter.GetData("EQUIP_DATA") as RoleEquipLogicData;
		mCurSelEquipData = roleEquipLogicData.GetEquipDataByItemId(mCurEquipDBID);

		UpdateRoleEquipInfo();

		return base.Refresh (param);
	}

	public void UpdateRoleEquipInfo()
	{
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
		GameObject equipNameObj = GameCommon.FindObject(mRoleEquipIconInfo, "role_equip_name_label");
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

    private void SetBaseAttribute()
    {
        if (mCurSelEquipData == null)
            return;

        for (int i = 0; i < 2; i++)
        {
            int iAttributeType = TableCommon.GetNumberFromRoleEquipConfig(mCurSelEquipData.tid, "ATTRIBUTE_TYPE_" + i.ToString());
            if (iAttributeType <= (int)AFFECT_TYPE.NONE || iAttributeType >= (int)AFFECT_TYPE.MAX)
                return;

            GameObject baseObj = GameCommon.FindObject(mRoleEquipAttributeInfo, "base_attribute_" + i.ToString()).gameObject;

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
		
		UIGridContainer grid = mRoleEquipAttributeInfo.transform.Find("added_attribute/grid").GetComponent<UIGridContainer>();
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
		GameObject useBtn = GameCommon.FindObject(mGameObjUI, "role_equip_info_use_btn").gameObject;
        GameObject unUseBtn = GameCommon.FindObject(mGameObjUI, "role_equip_info_unuse_btn").gameObject;
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

				DataCenter.SetData("ROLE_EQUIP_INFO_WINDOW", "DBID", iRoleEquipDBID);
				DataCenter.SetData("ROLE_EQUIP_INFO_WINDOW", "USER_ID", iRoleEquipUserID);
				DataCenter.SetData("ROLE_EQUIP_INFO_WINDOW", "USE_RESULT", true);
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
			int type = TableCommon.GetNumberFromRoleEquipConfig(mCurSelEquipData.tid, "ROLEEQUIP_TYPE");
			DataCenter.SetData("RoleEquipWindow", "UPDATE_ROLE_EQUIP_ICONS_FOR_TYPE", type);
			
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
				
				EquipData curUseEquip = roleEquipLogicData.GetUseEquip(type);
				if(curUseEquip != null)
					curUseEquip.mUserID = 0;
				
				equip.mUserID = iRoleEquipUserID;
				
				if(iRoleEquipUserID != 0)
					roleEquipLogicData.SetUseEquip(type, equip);
				else
					roleEquipLogicData.SetUseEquip(type, null);

				// refresh bag role equip icons
				DataCenter.SetData("RoleEquipWindow", "UPDATE_ROLE_EQUIP_ICONS_FOR_TYPE", type);

				// refresh role equip info
				DataCenter.SetData("ROLE_INFO_WINDOW", "REFRESH", true);

				// close
				DataCenter.CloseWindow("ROLE_EQUIP_INFO_WINDOW");
			}
		}
		
//		Refresh (0);
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
				DataCenter.SetData("ROLE_EQUIP_INFO_WINDOW", "DBID", iRoleEquipDBID);
				DataCenter.SetData("ROLE_EQUIP_INFO_WINDOW", "USER_ID", iRoleEquipUserID);
				DataCenter.SetData("ROLE_EQUIP_INFO_WINDOW", "USE_RESULT", true);
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
			quest.mAction = () => DataCenter.SetData("ROLE_EQUIP_INFO_WINDOW", "SALE_RESULT", true);
			quest.DoEvent();
		}
		else
		{
			int type = TableCommon.GetNumberFromRoleEquipConfig(mCurSelEquipData.tid, "ROLEEQUIP_TYPE");

			// set role equip data			
			RoleEquipLogicData roleEquipLogicData = DataCenter.GetData("EQUIP_DATA") as RoleEquipLogicData;
			if(roleEquipLogicData != null)
			{
				roleEquipLogicData.RemoveRoleEquip(mCurSelEquipData);
//				if(roleEquipLogicData.mDicEquip.Count == 0)
//				{
//					//DataCenter.SetData("AllRoleAttributeInfoWindow", "SET_SEL_PAGE", ALL_ROLE_ATTRIBUTE_PAGE_TYPE.RoleEquipCultivate);
//					DataCenter.OpenWindow("ROLE_EQUIP_CULTIVATE_WINDOW");
//				}
			}

			// refresh bag role equip icons
			DataCenter.SetData("RoleEquipWindow", "UPDATE_ROLE_EQUIP_ICONS_FOR_TYPE", type);
			
			// refresh role equip info
			DataCenter.SetData("ROLE_INFO_WINDOW", "REFRESH", true);

			// gain gold
            int iGainGlod = TableCommon.GetNumberFromRoleEquipConfig(mCurSelEquipData.tid, "PRICE");
			GameCommon.RoleChangeGold(iGainGlod);

			// refresh role equip cultiavte ui
//			Refresh (0);

			// close
			DataCenter.CloseWindow("ROLE_EQUIP_INFO_WINDOW");
		}
	}
	
	public void SaleResult()
	{
		// set role equip data			
		RoleEquipLogicData roleEquipLogicData = DataCenter.GetData("EQUIP_DATA") as RoleEquipLogicData;
		if(roleEquipLogicData != null)
		{
			int type = TableCommon.GetNumberFromRoleEquipConfig(mCurSelEquipData.tid, "ROLEEQUIP_TYPE");

			if(roleEquipLogicData.RemoveRoleEquip(mCurSelEquipData))
			{
				DEBUG.Log("sale role equip DBID = " + mCurSelEquipData.itemId.ToString() + " is successful, remove it OK");
				// gain gold
                int iGainGlod = TableCommon.GetNumberFromRoleEquipConfig(mCurSelEquipData.tid, "PRICE");
				GameCommon.RoleChangeGold(iGainGlod);
			}

			// refresh bag role equip icons
			DataCenter.SetData("RoleEquipWindow", "UPDATE_ROLE_EQUIP_ICONS_FOR_TYPE", type);
			
			// refresh role equip info
			DataCenter.SetData("ROLE_INFO_WINDOW", "REFRESH", true);

//			// refresh role equip cultiavte ui
//			Refresh (0);

			// close
			DataCenter.CloseWindow("ROLE_EQUIP_INFO_WINDOW");
		}
	}
}


public class Button_RoleEquipInfoUseBtn : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.SetData("ROLE_EQUIP_INFO_WINDOW", "USE", true);
		return true;
	}
}

public class Button_RoleEquipInfoUnUseBtn : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.SetData("ROLE_EQUIP_INFO_WINDOW", "UNUSE", true);
		return true;
	}
}

public class Button_RoleEquipInfoSaleBtn : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.SetData("ROLE_EQUIP_INFO_WINDOW", "SALE", true);
		return true;
	}
}

public class Button_RoleEquipInfoStrengthenBtn : CEvent
{
	public override bool _DoEvent()
	{
		RoleEquipInfoWindow window = DataCenter.GetData("ROLE_EQUIP_INFO_WINDOW") as RoleEquipInfoWindow;
		if(window != null)
		{
			EquipData curSelEquipData = window.mCurSelEquipData;
			if(curSelEquipData != null)
			{
				DataCenter.CloseWindow("ROLE_EQUIP_INFO_WINDOW");

				EventCenter.Start("Button_RoleEquipCultivateBtn").DoEvent();
				DataCenter.SetData ("ALL_ROLE_ATTRIBUTE_INFO_WINDOW","SET_SEL_PAGE", "ROLE_EQUIP_CULTIVATE_WINDOW");
				DataCenter.SetData("ROLE_EQUIP_CULTIVATE_WINDOW", "REFRESH", curSelEquipData.mGridIndex);

				EventCenter.Start("Button_RoleEquipStrengthenBtn").DoEvent();
			}
		}
		return true;
	}
}

public class Button_RoleEquipInfoResetBtn : CEvent
{
	public override bool _DoEvent()
	{
		RoleEquipInfoWindow window = DataCenter.GetData("ROLE_EQUIP_INFO_WINDOW") as RoleEquipInfoWindow;
		if(window != null)
		{
			EquipData curSelEquipData = window.mCurSelEquipData;
			if(curSelEquipData != null)
			{
				DataCenter.CloseWindow("ROLE_EQUIP_INFO_WINDOW");
				
				EventCenter.Start("Button_RoleEquipCultivateBtn").DoEvent();
				DataCenter.SetData ("ALL_ROLE_ATTRIBUTE_INFO_WINDOW","SET_SEL_PAGE", "ROLE_EQUIP_CULTIVATE_WINDOW");
				DataCenter.SetData("ROLE_EQUIP_CULTIVATE_WINDOW", "REFRESH", curSelEquipData.mGridIndex);
				
				EventCenter.Start("Button_RoleEquipResetBtn").DoEvent();
			}
		}
		return true;
	}
}

public class Button_RoleEquipInfoWindowCloseButton : CEvent
{
	public override bool _DoEvent()
	{
		DataCenter.CloseWindow("ROLE_EQUIP_INFO_WINDOW");
		return true;
	}
}
