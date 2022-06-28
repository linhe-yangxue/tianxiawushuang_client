using UnityEngine;
using System.Collections;
using DataTable;
using Logic;
using System.Collections.Generic;
using System.Linq;

public enum ExpansionInfo_Page_Type
{
    Strengthen,
    //Evolution,
    Reset,
}

public class StrengthenEquipData
{
	public EquipData mEquipData;
	public int mIndex;
}

public class ExpansionInfoWindow : tWindow
{    
	RoleEquipLogicData mRoleEquipLogicData;
	UIGridContainer mAddRoleEquipGrid;
    GameObject mRoleEquipStrengthenWindow;
    //GameObject mRoleEquipEvolutionWindow;   
    GameObject mRoleEquipResetWindow;

	GameObject mSelectEquipGroup;
	
	int mCurSelGridIndex = 0;
	EquipData mCurSelEquipData;
    ExpansionInfo_Page_Type mCurPagType = ExpansionInfo_Page_Type.Strengthen;

	public Dictionary<int, StrengthenEquipData> mDicSelEquip = new Dictionary<int, StrengthenEquipData>();
	
	public int miAimExp = 0;
	public int miAimStrengthenLevel = 0; 
	public int miNeedSyntheticCoin = 0;
	
	public ExpansionInfoWindow(GameObject obj)
	{
		mGameObjUI = obj;
		Close();
	}
	
	public override void Init()
    {
        mRoleEquipLogicData = DataCenter.GetData("EQUIP_DATA") as RoleEquipLogicData;

        mRoleEquipStrengthenWindow = mGameObjUI.transform.Find("RoleEquipStrengthenWindow").gameObject;
        //mRoleEquipEvolutionWindow = mGameObjUI.transform.Find("RoleEquipEvolutionWindow").gameObject;
        mRoleEquipResetWindow = mGameObjUI.transform.Find("RoleEquipResetWindow").gameObject;

		mSelectEquipGroup = GameCommon.FindObject(mGameObjUI, "select_equip_group");

        mAddRoleEquipGrid = GameCommon.FindComponent<UIGridContainer>(mGameObjUI, "add_equip_grid");

		Refresh(null);
	}

    public void SetCurSelEquipData()
    {
        mCurSelEquipData = mRoleEquipLogicData.GetEquipDataByGridIndex(mCurSelGridIndex);
    }

	public override bool Refresh(object param)
	{
		if (mCurSelEquipData == null)
			return false;

        // hide effect
        GameCommon.FindObject(mGameObjUI, "ec_ui_qianghuafb").SetActive(false);

        if (mCurPagType == ExpansionInfo_Page_Type.Strengthen)
        {
//			// is add attribute
//			GameObject is_add_attr_description = mRoleEquipStrengthenWindow.transform.Find("is_add_attr_description").gameObject;
//			if(is_add_attr_description != null)
//			{
//				if((mCurSelEquipData.mStrengthenLevel + 1)%10 == 0)
//					is_add_attr_description.SetActive(true);
//				else
//					is_add_attr_description.SetActive(false);
//			}

            //// set need gold
            //GameObject needGoldNum = mRoleEquipStrengthenWindow.transform.Find("NeedGold/NumLabel").gameObject;
            //if(needGoldNum != null)
            //{
            //    int iBaseNeedGoldNum = TableCommon.GetNumberFromRoleEquipEvolutionConfig(mCurSelEquipData.mStarLevel, "STRENGTHEN_COST");
            //    int iDNeedGoldNum = TableCommon.GetNumberFromRoleEquipEvolutionConfig(mCurSelEquipData.mStarLevel, "STRENGTHEN_COST_ADD");

            //    int iNeedGlod = iBaseNeedGoldNum + mCurSelEquipData.mStrengthenLevel * iDNeedGoldNum;

            //    string strText = iNeedGlod.ToString();
            //    RoleLogicData logicData = RoleLogicData.Self;
            //    if(iNeedGlod > logicData.mGold)
            //    {
            //        strText = "[ff0000]" + strText;
            //    }
				
            //    UILabel needGlodNumLabel = needGoldNum.GetComponent<UILabel>();
            //    if(needGlodNumLabel != null)
            //        needGlodNumLabel.text = strText;
            //}

			UpdateUI();
        }
        //else if (mCurPagType == ExpansionInfo_Page_Type.Evolution)
        //{
        //    // src base attribute
        //    GameObject srcBaseObj = mRoleEquipEvolutionWindow.transform.Find("AttributeChange/src_attribute").gameObject;
        //    SetBaseAttribute(srcBaseObj, mCurSelEquipData.mStarLevel, mCurSelEquipData.mStrengthenLevel, mCurSelEquipData.mQualityType);

        //    // aim base attribute
        //    GameObject aimBaseObj = mRoleEquipEvolutionWindow.transform.Find("AttributeChange/aim_attribute").gameObject;
        //    SetBaseAttribute(aimBaseObj, mCurSelEquipData.mStarLevel + 1, 0, mCurSelEquipData.mQualityType);
						
        //    // set need gold
        //    GameObject needGoldNum = mRoleEquipEvolutionWindow.transform.Find("NeedGold/NumLabel").gameObject;
        //    if(needGoldNum != null)
        //    {
        //        int iNeedGlod = TableCommon.GetNumberFromRoleEquipEvolutionConfig(mCurSelEquipData.mStarLevel, "EVOLUTION_COST");

        //        string strText = iNeedGlod.ToString();
        //        RoleLogicData logicData = RoleLogicData.Self;
        //        if(iNeedGlod > logicData.mGold)
        //        {
        //            strText = "[ff0000]" + strText;
        //        }

        //        UILabel needGlodNumLabel = needGoldNum.GetComponent<UILabel>();
        //        if(needGlodNumLabel != null)
        //            needGlodNumLabel.text = strText;
        //    }
        //}
        else if (mCurPagType == ExpansionInfo_Page_Type.Reset)
        {
			// set need diamond
			GameObject needDiamondNum = mRoleEquipResetWindow.transform.Find("NeedDiamond/NumLabel").gameObject;
			if(needDiamondNum != null)
			{
				// cost diamond
                RoleLogicData roleLogicData = RoleLogicData.Self;
                int iIndex = roleLogicData.vipLevel + 1001;
                int iNeedDiamond = TableCommon.GetNumberFromVipList(iIndex, "RECAST_COST");

				string strText = iNeedDiamond.ToString();
				RoleLogicData logicData = RoleLogicData.Self;
				if(iNeedDiamond > logicData.diamond)
				{
					strText = "[ff0000]" + strText;
				}

				UILabel needDiamondNumLabel = needDiamondNum.GetComponent<UILabel>();
				if(needDiamondNumLabel != null)
					needDiamondNumLabel.text = strText;
			}

			SetRemainResetNum();

			SetResetTicketNum();
        }
       
		return true;
	}

	public void SetRemainResetNum()
	{
		UILabel remainResetNumLabel = GameCommon.FindObject(mRoleEquipResetWindow, "reset_times").GetComponent<UILabel>();
        remainResetNumLabel.text = (TableCommon.GetNumberFromVipList(RoleLogicData.Self.vipLevel + 1001, "MAX_RECAST") - mCurSelEquipData.mReset).ToString();
	}

	public void SetResetTicketNum()
	{
		UILabel resetTicrketNumLabel = GameCommon.FindObject(mRoleEquipResetWindow, "reset_ticket_num").GetComponent<UILabel>();
		resetTicrketNumLabel.text = RoleLogicData.Self.mResetNum.ToString();
	}


    public override void onChange(string keyIndex, object objVal)
    {
        base.onChange(keyIndex, objVal);
        switch (keyIndex)
        {
            case "SHOW_WINDOW":
                ShowWindow((ExpansionInfo_Page_Type)objVal);
                break;
			case "SET_GRID_INDEX":
				SetCurGridIndex((int)objVal);
				break;
            case "CLOSE_WINDOW":
                Close();
				DataCenter.SetData("RoleEquipWindow", "UPDATE_ROLE_EQUIP_ICONS", true);
                ClearAllSelEquips();
				DataCenter.SetData("ROLE_EQUIP_CULTIVATE_WINDOW", "SET_BTN_GROUP_VISIBLE", true);
				DataCenter.SetData("ROLE_EQUIP_CULTIVATE_WINDOW", "SET_ROLE_EQUIP_INFO_BTN_GROUP_VISIBLE", true);
				DataCenter.SetData("ROLE_EQUIP_CULTIVATE_WINDOW", "SET_ROLE_EQUIP_RESET_INFO_VISIBLE", false);

				if(mCurPagType == ExpansionInfo_Page_Type.Strengthen)
				{
					DataCenter.SetData("ROLE_EQUIP_CULTIVATE_WINDOW", "SET_ATTRIBUTE_INFO_UI_VISIBLE", true);
				}
				else if(mCurPagType == ExpansionInfo_Page_Type.Reset)
				{

				}
                break;
			case "SET_SELECT_EQUIP_BY_ID":
				SetSelEquipByID((int)objVal);
				break;
			case "CLEAR_SEL_EQUIPS":
				ClearAllSelEquips();
				break;
			case "STRENGTHEN_OK":
                bool bIsHadHighStarLevelRoleEquip = false;
                foreach (KeyValuePair<int, StrengthenEquipData> pair in mDicSelEquip)
                {
                    if (pair.Value.mEquipData.mStarLevel >= 3)
                    {
                        bIsHadHighStarLevelRoleEquip = true;
                        break;
                    }
                }

                if (bIsHadHighStarLevelRoleEquip)
                    DataCenter.OpenMessageOkWindow(STRING_INDEX.ERROR_ROLE_EQUIP_STRENGTHEN_HIGH_STAR_LEVEL, "", () => StrengthenOK());
                else
                    StrengthenOK();
				break;
            case "STRENGTHEN_RESULT":
                StrengthenResult();
                break;
			case "AUTO_ADD_EQUIP":
				AutoAddEquip();
				break;
            //case "EVOLUTION_OK":
            //    EvolutionOK();
            //    break;
            //case "EVOLUTION_RESULT":
            //    EvolutionResult((DataRecord)objVal);
            //    break;
			case "RESET_OK":
                DataCenter.OpenMessageOkWindow(STRING_INDEX.ERROR_ROLE_EQUIP_IS_RESET, "", () => ResetOK());
				break;
			case "RESET_RESULT":
				ResetResult((DataRecord)objVal);
				break;
			case "UPDATE_CUR_SEL_EQUIP_DATA":
				UpdateCurSelEquipData((int)objVal);
				break;
        }
    }

	public void SetRoleEquipIcon()
	{
        GameObject obj = mRoleEquipStrengthenWindow.transform.Find("role_equip_strength_icon_group").gameObject;
		
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

    public void ShowWindow(ExpansionInfo_Page_Type param)
    {
		SetCurSelEquipData();

		if (mCurSelEquipData == null)
			return;

        mCurPagType = param;
        if (param == ExpansionInfo_Page_Type.Strengthen)
        {
			if(!StrengthenCondition(true))
				return;

			DataCenter.SetData("ROLE_EQUIP_CULTIVATE_WINDOW", "SET_ATTRIBUTE_INFO_UI_VISIBLE", false);

			DataCenter.SetData("ROLE_EQUIP_CULTIVATE_WINDOW", "SET_BTN_GROUP_VISIBLE", false);

			DataCenter.SetData("RoleEquipWindow", "UPDATE_ROLE_EQUIP_STRENGTHEN_ICONS", mCurSelEquipData.mGridIndex);
        }
        //else if (param == ExpansionInfo_Page_Type.Evolution)
        //{
        //    if(!OpenEvolutionCondition())
        //        return;
        //}
        else if (param == ExpansionInfo_Page_Type.Reset)
        {
			if(!ResetCondition())
				return;

			DataCenter.SetData("ROLE_EQUIP_CULTIVATE_WINDOW", "SET_ROLE_EQUIP_INFO_BTN_GROUP_VISIBLE", false);
			DataCenter.SetData("ROLE_EQUIP_CULTIVATE_WINDOW", "SET_ROLE_EQUIP_RESET_INFO_VISIBLE", true);

			UpdateResetUIInfo();
        }

		Open(true);

        Refresh(null);

		mRoleEquipStrengthenWindow.SetActive(mCurPagType == ExpansionInfo_Page_Type.Strengthen);
		//mRoleEquipEvolutionWindow.SetActive(mCurPagType == ExpansionInfo_Page_Type.Evolution);
		mRoleEquipResetWindow.SetActive(mCurPagType == ExpansionInfo_Page_Type.Reset);
    }

	public void UpdateResetUIInfo()
	{
		int iGroupID = TableCommon.GetNumberFromRoleEquipConfig(mCurSelEquipData.tid, "ATTACHATTRIBUTE_GROUP");
		Dictionary<int, DataRecord> dicEequipAttachAttribute = DataCenter.mEquipAttachAttributeConfig.GetAllRecord();
		if(dicEequipAttachAttribute != null && iGroupID != 0)
		{
			int iIndex = 0;
			foreach(KeyValuePair<int, DataRecord> pair in dicEequipAttachAttribute)
			{
				if(iIndex >= 4)
					return;

				DataRecord record = pair.Value;
				int groupID = record["ATTACHATTRIBUTE_GROUP"];
				int isbest = record["IS_BEST"];
				if(groupID == iGroupID && isbest == 1)
				{
					GameObject baseObj = mRoleEquipResetWindow.transform.Find("AttributeChange/base_attribute" + iIndex.ToString()).gameObject;
					SetResetBestAttribute(baseObj, record);
					iIndex++;
				}
			}
		}
	}

	public void SetCurGridIndex(int iGridIndex)
	{
		mCurSelGridIndex = iGridIndex;
	}

	public void SetResetBestAttribute(GameObject baseObj, DataRecord record)
	{
		if (mCurSelEquipData == null)
			return;

		if(baseObj == null || record == null)
			return;
		
		int iAttributeType = record["TYPE"];
		if (iAttributeType <= (int)AFFECT_TYPE.NONE || iAttributeType >= (int)AFFECT_TYPE.MAX)
			return;
		
		// set icon
		SetAttributeIcon(baseObj, iAttributeType);
		
		// set name
		SetAttributeName(baseObj, iAttributeType);
		
		// set number
		UILabel numLabel = GetAttributeNumLabel(baseObj, iAttributeType);
		float fValue = 0.0f;
		if (numLabel != null)
		{
			SetAttributeValueUI(numLabel, iAttributeType, record["ATTACHATTRIBUTE"]);
		}
	}

    public void SetBaseAttribute(string baseObjName, int iStrengthenLevel)
    {
        for (int i = 0; i < 2; i++)
        {
            SetBaseAttribute(i, baseObjName, iStrengthenLevel);
        }
    }

	public void SetBaseAttribute(int iIndex, string baseObjName, int iStrengthenLevel)
    {
        if (mCurSelEquipData == null)
            return;

        int iAttributeType = TableCommon.GetNumberFromRoleEquipConfig(mCurSelEquipData.tid, "ATTRIBUTE_TYPE_" + iIndex.ToString());
        if (iAttributeType <= (int)AFFECT_TYPE.NONE || iAttributeType >= (int)AFFECT_TYPE.MAX)
            return;

        GameObject baseObj = GameCommon.FindObject(mRoleEquipStrengthenWindow, baseObjName + iIndex.ToString());

        // set icon
        SetAttributeIcon(baseObj, iAttributeType);

        // set name
        SetAttributeName(baseObj, iAttributeType);

        // set base number
		SetBaseAttributeValue(iIndex, baseObj, iAttributeType, iStrengthenLevel);
    }

    // set equip attribute icon
    static public void SetAttributeIcon(GameObject obj, int iIndex)
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

	public void SetStrengthenLevel(GameObject obj, int iStrengthenLevel)
	{
		// set level
		GameObject level = obj.transform.Find("LevelLabel").gameObject;
		UILabel levelLabel = level.GetComponent<UILabel>();
		if (levelLabel != null)
		{
			levelLabel.text = iStrengthenLevel.ToString();
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

	public void StrengthenOK()
	{
		if(mCurSelEquipData == null)
			return;

		if(CommonParam.bIsNetworkGame)
		{
			if(StrengthenCondition(false))
			{
				tEvent quest = Net.StartEvent("CS_RequestRoleEquipStrengthen");
				DataBuffer dataBuffer = new DataBuffer(256);
				dataBuffer.write(mDicSelEquip.Count);
				foreach(KeyValuePair<int, StrengthenEquipData> kvp in mDicSelEquip)
				{
					EquipData tempEquipData = kvp.Value.mEquipData;
					dataBuffer.write(tempEquipData.itemId);
				}

				quest.set("DBID", mCurSelEquipData.itemId);
				quest.set("DATABUFFER", dataBuffer);
				quest.DoEvent();
			}
		}
		else
		{
			// cost gold
            //int iBaseNeedGoldNum = TableCommon.GetNumberFromRoleEquipEvolutionConfig(mCurSelEquipData.mStarLevel, "STRENGTHEN_COST");
            //int iDNeedGoldNum = TableCommon.GetNumberFromRoleEquipEvolutionConfig(mCurSelEquipData.mStarLevel, "STRENGTHEN_COST_ADD");
			
            //int iNeedGlod = iBaseNeedGoldNum + mCurSelEquipData.mStrengthenLevel * iDNeedGoldNum;

			GameCommon.RoleChangeGold((-1) * miNeedSyntheticCoin);


            //RoleEquipLogicData logicData = DataCenter.GetData("EQUIP_DATA") as RoleEquipLogicData;

            //UpdateCurSelEquipData(mCurSelEquipData.mDBID);

			mCurSelEquipData.strengthenLevel++;

			//mCurSelEquipData.AddEquipAttachAttriblute(i, re.getData("EXTRA_TYPE_" + (i + 1).ToString()) + re.getData("EXTRA_NUM_" + (i + 1).ToString()));
			if(mCurSelEquipData.strengthenLevel%10 == 0)
				mCurSelEquipData.AddEquipAttachAttriblute(mCurSelEquipData.mAttachAttributeList.Count, AFFECT_TYPE.ATTACK_RATE, 0.351f);

			// refresh role equip cultiavte ui
			DataCenter.SetData("ROLE_EQUIP_CULTIVATE_WINDOW", "REFRESH", mCurSelGridIndex);

			// refresh expansion info ui
			if(StrengthenCondition(false))
				Refresh(null);
		}
	}

    public void StrengthenResult()
    {
        // cost gold
        GameCommon.RoleChangeGold((-1) * miNeedSyntheticCoin);
		
		DeleteSelEquip();
		
        //RoleEquipLogicData logicData = DataCenter.GetData("EQUIP_DATA") as RoleEquipLogicData;
        //EquipData equipData = logicData.GetEquipDataByDBID(mCurSelEquipData.mDBID);
        mCurSelEquipData.strengthenLevel = miAimStrengthenLevel;
        mCurSelEquipData.mExp = miAimExp;
		
		// refresh role equip cultiavte ui
		DataCenter.SetData("RoleEquipWindow", "UPDATE_ROLE_EQUIP_ICONS", true);
        UpdateCurSelEquipData(mCurSelEquipData.itemId);
		//DataCenter.SetData("ROLE_EQUIP_CULTIVATE_WINDOW", "REFRESH", mCurSelGridIndex);
		
		//SetRoleEquipMaterialIcon();
		
		// refresh bag role strengthen equip icons
        DataCenter.SetData("RoleEquipWindow", "UPDATE_ROLE_EQUIP_STRENGTHEN_ICONS", mCurSelGridIndex);

        // show effect
        GameCommon.FindObject(mGameObjUI, "ec_ui_qianghuafb").SetActive(true);

        // refresh ui
		UpdateUI();
    }

    //public void EvolutionOK()
    //{
    //    if(mCurSelEquipData == null)
    //        return;


    //    // set need gold
    //    GameObject needGoldNum = mRoleEquipEvolutionWindow.transform.Find("NeedGold/NumLabel").gameObject;
    //    if(needGoldNum != null)
    //    {
    //        int iNeedGlod = TableCommon.GetNumberFromRoleEquipEvolutionConfig(mCurSelEquipData.mStarLevel, "EVOLUTION_COST");
			
    //        string strText = iNeedGlod.ToString();
    //        RoleLogicData logicData = RoleLogicData.Self;

    //        UILabel needGlodNumLabel = needGoldNum.GetComponent<UILabel>();
    //        if(needGlodNumLabel != null)
    //        {
    //            needGlodNumLabel.text = strText;
    //            if(iNeedGlod > logicData.mGold)
    //            {
    //                needGlodNumLabel.text = "[ff0000]" + strText;
    //                DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_ROLE_EQUIP_EVOLUTION_NEED_COIN);
    //                return;
    //            }
    //        }
    //    }

    //    if(CommonParam.bIsNetworkGame)
    //    {
    //        if(EvolutionCondition())
    //        {
    //            tEvent quest = Net.StartEvent("CS_RequestRoleEquipEvolution");
    //            quest.set("DBID", mCurSelEquipData.mDBID);
    //            quest.DoEvent();
    //        }
    //    }
    //    else
    //    {
    //        // cost gold
    //        int iNeedGlod = TableCommon.GetNumberFromRoleEquipEvolutionConfig(mCurSelEquipData.mStarLevel, "EVOLUTION_COST");			
    //        GameCommon.RoleChangeGold((-1) * iNeedGlod);

    //        // set role equip data
    //        RoleEquipLogicData logicData = DataCenter.GetData("EQUIP_DATA") as RoleEquipLogicData;
    //        RoleEquipData roleEquip = logicData.GetEquipDataByDBID(mCurSelEquipData.mDBID) as RoleEquipData;

    //        roleEquip.mStarLevel++;
    //        roleEquip.mStrengthenLevel = 0;

    //        roleEquip.mAttachAttributeList.Clear();

    //        // refresh bag role equip icons
    //        DataCenter.SetData("RoleEquipWindow", "UPDATE_ROLE_EQUIP_ICONS", true);

    //        UpdateCurSelEquipData(mCurSelEquipData.mDBID);

    //        // refresh role equip cultiavte ui
    //        DataCenter.SetData("ROLE_EQUIP_CULTIVATE_WINDOW", "REFRESH", mCurSelGridIndex);			

    //        // refresh expansion info ui
    //        if(EvolutionCondition())
    //            Refresh(null);
    //    }
    //}

    //public void EvolutionResult(DataRecord re)
    //{
    //    if (re != null)
    //    {
    //        // cost gold
    //        int iNeedGlod = TableCommon.GetNumberFromRoleEquipEvolutionConfig(mCurSelEquipData.mStarLevel, "EVOLUTION_COST");			
    //        GameCommon.RoleChangeGold((-1) * iNeedGlod);
			
    //        RoleEquipLogicData logicData = DataCenter.GetData("EQUIP_DATA") as RoleEquipLogicData;
    //        logicData.AttachRoleEquip(re);
			
    //        // refresh bag role equip icons
    //        DataCenter.SetData("RoleEquipWindow", "UPDATE_ROLE_EQUIP_ICONS", true);

    //        UpdateCurSelEquipData(re.getData("ID"));

    //        // refresh role equip cultiavte ui
    //        DataCenter.SetData("ROLE_EQUIP_CULTIVATE_WINDOW", "REFRESH", mCurSelGridIndex);			
			
    //        // refresh expansion info ui
    //        if(EvolutionCondition())
    //            Refresh(null);
    //    }
    //}
	
	public void ResetOK()
	{
		if(mCurSelEquipData == null)
			return;


		// set need diamond
		GameObject needDiamondNum = mRoleEquipResetWindow.transform.Find("NeedDiamond/NumLabel").gameObject;
		if(needDiamondNum != null)
		{
            RoleLogicData logicData = RoleLogicData.Self;
            // cost diamond
            int iIndex = logicData.vipLevel + 1001;
            int iNeedDiamond = TableCommon.GetNumberFromVipList(iIndex, "RECAST_COST");			
			string strText = iNeedDiamond.ToString();
			UILabel needDiamondNumLabel = needDiamondNum.GetComponent<UILabel>();
			if(needDiamondNumLabel != null)
			{
				needDiamondNumLabel.text = strText;
				if(logicData.mResetNum <= 0 && iNeedDiamond > logicData.diamond)
				{
                    needDiamondNumLabel.text = "[ff0000]" + strText;
                    DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_ROLE_EQUIP_RESET_NEED_COIN);
                    return;
				}
			}
		}

		if(CommonParam.bIsNetworkGame)
		{
			if(ResetCondition())
			{
				tEvent quest = Net.StartEvent("CS_RequestRoleEquipReset");
				quest.set("DBID", mCurSelEquipData.itemId);
				for(int i = 0; i < mCurSelEquipData.mAttachAttributeList.Count; i++)
				{
					if(mCurSelEquipData.mAttachAttributeList[i].mLockState == LOCK_STATE.LOCKED)
						quest.set("INDEX_" + (i + 1).ToString(), true);
				}
				quest.DoEvent();
			}
		}
		else
		{
			// cost diamond
            RoleLogicData roleLogicData = RoleLogicData.Self;
            int iIndex = roleLogicData.vipLevel + 1001;
            int iNeedDiamond = TableCommon.GetNumberFromVipList(iIndex, "RECAST_COST");
			GameCommon.RoleChangeDiamond((-1) * iNeedDiamond);

			// set role equip data
			RoleEquipLogicData logicData = DataCenter.GetData("EQUIP_DATA") as RoleEquipLogicData;
			RoleEquipData roleEquip = logicData.GetEquipDataByItemId(mCurSelEquipData.itemId) as RoleEquipData;

			roleEquip.strengthenLevel = 1;
			roleEquip.mReset++;
			roleEquip.mStarLevel = TableCommon.GetNumberFromRoleEquipConfig(roleEquip.tid, "STAR_LEVEL");
			roleEquip.mQualityType = (EQUIP_QUALITY_TYPE)TableCommon.GetNumberFromRoleEquipConfig(roleEquip.tid, "QUALITY");
			roleEquip.mElementType = (ELEMENT_TYPE)UnityEngine.Random.Range(0, 5);
			SetCurSelEquipData();

			// refresh bag role equip icons
			DataCenter.SetData("RoleEquipWindow", "UPDATE_ROLE_EQUIP_ICONS", true);

			UpdateCurSelEquipData(mCurSelEquipData.itemId);

			// refresh role equip cultiavte ui
			DataCenter.SetData("ROLE_EQUIP_CULTIVATE_WINDOW", "REFRESH", mCurSelGridIndex);			

			if(ResetCondition())
				Refresh(null);
		}
	}

	public void ResetResult(DataRecord re)
	{
		if (re != null)
		{
			// cost diamond
            RoleLogicData roleLogicData = RoleLogicData.Self;
            int iIndex = roleLogicData.vipLevel + 1001;
            if (roleLogicData.mResetNum > 0)
            {
                roleLogicData.mResetNum--;
            }
            else
            {
                int iNeedDiamond = TableCommon.GetNumberFromVipList(iIndex, "RECAST_COST");
                GameCommon.RoleChangeDiamond((-1) * iNeedDiamond);
            }
			
			// set role equip data
			RoleEquipLogicData logicData = DataCenter.GetData("EQUIP_DATA") as RoleEquipLogicData;
			logicData.AttachRoleEquip(re);
			
			// refresh bag role equip icons
			DataCenter.SetData("RoleEquipWindow", "UPDATE_ROLE_EQUIP_ICONS", true);

			UpdateCurSelEquipData(re.getData("ID"));

			// refresh role equip cultiavte ui
			DataCenter.SetData("ROLE_EQUIP_CULTIVATE_WINDOW", "REFRESH", mCurSelGridIndex);			
			
			if(ResetCondition())
				Refresh(null);
		}
	}

	public bool StrengthenCondition(bool bIsNeedMessageBox)
	{
		if(mCurSelEquipData == null)
			return false;

		if(mCurSelEquipData.strengthenLevel >= mCurSelEquipData.mMaxStrengthenLevel)
		{
			if(bIsNeedMessageBox)
				DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_ROLE_EQUIP_LEVEL_MAX);
			return false;
		}
		return true;
	}

//    public bool EvolutionCondition()
//    {
//        if(mCurSelEquipData == null)
//            return false;

//        int iMaxStrengthenLevel = TableCommon.GetNumberFromRoleEquipEvolutionConfig(mCurSelEquipData.mStarLevel, "MAX_EVOLUTION");
//        if(mCurSelEquipData.mStrengthenLevel < iMaxStrengthenLevel)
//        {
//            //Close();
//            //DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_ROLE_EQUIP_LEVEL_NOT_MAX, iMaxStrengthenLevel.ToString());

//            ShowWindow(ExpansionInfo_Page_Type.Strengthen);
//            return false;
//        }
		
////		if(mCurSelEquipData.mStarLevel >= 5)
////		{
//////			Close();
//////            DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_ROLE_EQUIP_STAR_LEVEL_MAX);
////
////			ShowWindow(ExpansionInfo_Page_Type.Strengthen);
////			return false;
////		}

//        int iEvolutionMax = TableCommon.GetNumberFromRoleEquipEvolutionConfig(mCurSelEquipData.mStarLevel, "ROLEEQUIP_EVO");
//        if(iEvolutionMax == 0)
//        {			
//            ShowWindow(ExpansionInfo_Page_Type.Strengthen);
//            return false;
//        }

//        return true;
//    }

//    public bool OpenEvolutionCondition()
//    {
//        if(mCurSelEquipData == null)
//            return false;
		
//        int iMaxStrengthenLevel = TableCommon.GetNumberFromRoleEquipEvolutionConfig(mCurSelEquipData.mStarLevel, "MAX_EVOLUTION");
//        if(mCurSelEquipData.mStrengthenLevel < iMaxStrengthenLevel)
//        {
//            //Close();
//            DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_ROLE_EQUIP_LEVEL_NOT_MAX, iMaxStrengthenLevel.ToString());
//            ShowWindow(ExpansionInfo_Page_Type.Strengthen);
//            return false;
//        }
		
////		if(mCurSelEquipData.mStarLevel >= 5)
////		{
////			Close();
////			DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_ROLE_EQUIP_STAR_LEVEL_MAX);
////			return false;
////		}

//        int iEvolutionMax = TableCommon.GetNumberFromRoleEquipEvolutionConfig(mCurSelEquipData.mStarLevel, "ROLEEQUIP_EVO");
//        if(iEvolutionMax == 0)
//        {
//            Close();
//            DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_ROLE_EQUIP_EVOLUTION_MAX);
//            return false;
//        }

//        return true;
//    }

	public bool ResetCondition()
	{
		if(mCurSelEquipData == null)
			return false;

		if (mCurSelEquipData.mReset >= TableCommon.GetNumberFromVipList(RoleLogicData.Self.vipLevel + 1001, "MAX_RECAST"))
        {
            DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_ROLE_EQUIP_RESET_COUNT, mCurSelEquipData.mReset.ToString());
            return false;
        }
        return true;
	}

	public void UpdateCurSelEquipData(int iItemId)
	{
		mRoleEquipLogicData = DataCenter.GetData("EQUIP_DATA") as RoleEquipLogicData;
		if(mRoleEquipLogicData != null)
		{
			EquipData equip = mRoleEquipLogicData.GetEquipDataByItemId(iItemId);
			if(equip != null)
				mCurSelGridIndex = equip.mGridIndex;
		}
		
		SetCurSelEquipData();
	}

	public bool IsEquipCanStrengthen(int iItemId, bool bIsNeedMessageBox)
	{
		RoleEquipLogicData logicData = DataCenter.GetData("EQUIP_DATA") as RoleEquipLogicData;
		EquipData equipData = logicData.GetEquipDataByItemId(iItemId);
		if(equipData != null)
		{
			StrengthenEquipData strengEquipData = null;
			if(mDicSelEquip.ContainsKey(equipData.itemId))
			{
				strengEquipData = mDicSelEquip[equipData.itemId];
			}

			if(strengEquipData == null)
			{
				if(IsEquipCurStrengthenLevelMax())
				{
					if(bIsNeedMessageBox)
						DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_ROLE_EQUIP_LEVEL_MAX);
					return false;
				}
				
				if(IsEquipAimStrengthenLevelMax())
				{
					if(bIsNeedMessageBox)
						DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_ROLE_EQUIP_STRENGTHEN_AIM_LEVEL_MAX);
					return false;
				}
				
				if(mDicSelEquip.Count >= 10)
				{
					if(bIsNeedMessageBox)
						DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_ROLE_EQUIP_STRENGTHEN_SELECT_COUNT_MAX);
					return false;
				}
			}
		}
		else
		{
			return false;
		}
		return true;
	}
	
	public bool IsEquipCurStrengthenLevelMax()
	{
		return IsEquipCurStrengthenLevelMax(mCurSelEquipData.itemId);
	}

	public bool IsEquipCurStrengthenLevelMax(int iItemId)
	{
		RoleEquipLogicData logicData = DataCenter.GetData("EQUIP_DATA") as RoleEquipLogicData;		
		EquipData equipData = logicData.GetEquipDataByItemId(iItemId);
		
		if(equipData != null)
		{
			if(equipData.strengthenLevel < equipData.mMaxStrengthenLevel)
			{
				return false;
			}
		}
		return true;
	}
	
	public bool IsEquipAimStrengthenLevelMax()
	{
		if(miAimStrengthenLevel < mCurSelEquipData.mMaxStrengthenLevel)
		{
			return false;
		}
		
		return true;
	}

	public void AddSelEquipByID(int iID)
	{
		RoleEquipLogicData logicData = DataCenter.GetData("EQUIP_DATA") as RoleEquipLogicData;
		EquipData equipData = logicData.GetEquipDataByItemId(iID);
		if(equipData != null)
		{
			StrengthenEquipData strengthenEquipData = new StrengthenEquipData();
			strengthenEquipData.mIndex = GetSelMaxIndex() + 1;
			strengthenEquipData.mEquipData = equipData;
			mDicSelEquip.Add(iID, strengthenEquipData);
		}
		
		UpdateUI();
	}
	
	public void RemoveSelEquip(int iID)
	{
		if(mDicSelEquip.ContainsKey(iID))
		{
			mDicSelEquip.Remove(iID);
			UpdateUI();
		}
	}
	
	public void ClearAllSelEquips()
	{
		mDicSelEquip.Clear();
		SetRoleEquipMaterialIcon();
		miAimStrengthenLevel = 1;
		miAimExp = 0;
	}
	
	public EquipData GetSelEquipDataByID(int iID)
	{
		StrengthenEquipData tempData = null;
		
		mDicSelEquip.TryGetValue(iID, out tempData);

		if(tempData != null)
			return tempData.mEquipData;
		
		return null;
	}
	
	public bool IsSelEquipByID(int iID)
	{
		EquipData tempEquipData = GetSelEquipDataByID(iID);
		return tempEquipData != null;
	}

	public int GetSelMaxIndex()
	{
		StrengthenEquipData strengthenEquipData = GetSelMaxIndexStrengthenEquipData();
		if(strengthenEquipData != null)
		{
			return strengthenEquipData.mIndex;
		}
		return 0;
	}

	public EquipData GetSelMaxIndexEquipData()
	{
		StrengthenEquipData strengthenEquipData = GetSelMaxIndexStrengthenEquipData();
		if(strengthenEquipData != null)
		{
			return strengthenEquipData.mEquipData;
		}
		return null;
	}

	public StrengthenEquipData GetSelMaxIndexStrengthenEquipData()
	{
		List<StrengthenEquipData> equipList = mDicSelEquip.Values.ToList();
		equipList = SortList(equipList);
		if(mDicSelEquip.Count > 0)
		{
			return equipList[0];
		}
		return null;
	}

	public List<StrengthenEquipData> SortList(List<StrengthenEquipData> list)
	{
		if(list != null && list.Count > 0)
		{
//			list = list.OrderByDescending(p => p.mIndex).ToList();
			list = GameCommon.SortList (list, SortListByIndex);
		}
		return list;
	}

	int SortListByIndex(StrengthenEquipData a, StrengthenEquipData b)
	{
		return GameCommon.Sort (a.mIndex, b.mIndex, true);
	}

	//------------------------------------------------------------------------
	public void UpdateUI()
	{
//		if(mDicSelEquip.Count > 0)
//		{
//			SetCurExpInfo();
//			SetAimExpInfo();
//			UpdateNeedCoinNum();
//		}
//		else
//		{
//			miAimStrengthenLevel = 1;
//			miAimExp = 0;
//		}

		SetCurExpInfo();
		SetAimExpInfo();
		UpdateNeedCoinNum();

        SetRoleEquipIcon();
		SetRoleEquipMaterialIcon();

		UILabel selEquipNumLabel = GameCommon.FindObject(mRoleEquipStrengthenWindow, "select_equip_numer").GetComponent<UILabel>();
		selEquipNumLabel.text = mDicSelEquip.Count.ToString() + "/10";

		Transform progressBarTans = mSelectEquipGroup.transform.Find("select_equip_numer/Progress Bar");
		if(progressBarTans != null)
		{
			UIProgressBar progressBar = progressBarTans.GetComponent<UIProgressBar>();
			if(progressBar != null)
				progressBar.value = mDicSelEquip.Count / 10.0f;
		}

//		mMaxLevelLabel.text = mCurSelEquipData.mMaxStrengthenLevel.ToString();
	}

	public void SetRoleEquipMaterialIcon()
	{
		EquipData equip  = GetSelMaxIndexEquipData();

		GameObject obj = GameCommon.FindObject(mRoleEquipStrengthenWindow, "role_equip_icon_group");
		obj.transform.Find("empty_bg").gameObject.SetActive(equip == null);
		obj.transform.Find("icon_sprite").gameObject.SetActive(equip != null);

		UIImageButton btn = mRoleEquipStrengthenWindow.transform.Find("RoleEquipStrengthenOKBtn").GetComponent<UIImageButton>();
		if(btn != null)
		{
			btn.isEnabled = equip != null;
		}

		if(equip != null)
		{
			// set role equip icon
			GameCommon.SetEquipIcon(obj, equip.tid);
			
			// set element icon and set equip element background icon
			int iElementIndex = (int)mCurSelEquipData.mElementType;
			GameCommon.SetEquipElementBgIcons(obj, mCurSelEquipData.tid, iElementIndex);
			
			// set star level
			GameCommon.SetStarLevelLabel(obj, mCurSelEquipData.mStarLevel);
			
			// set strengthen level text
			GameCommon.SetStrengthenLevelLabel(obj, mCurSelEquipData.strengthenLevel);

            mAddRoleEquipGrid.MaxCount = mDicSelEquip.Count - 1;
            mAddRoleEquipGrid.transform.localPosition = new Vector3(0 - mAddRoleEquipGrid.MaxCount * mAddRoleEquipGrid.CellWidth, 0, 0);
		}
		else
		{
			UILabel needCoinNumLabel = mRoleEquipStrengthenWindow.transform.Find("NeedGold/NumLabel").GetComponent<UILabel>();
			needCoinNumLabel.text = "0";

            mAddRoleEquipGrid.MaxCount = 0;
		}
	}

	public void SetSelEquipByID(int iID)
	{
		RoleEquipLogicData logicData = DataCenter.GetData("EQUIP_DATA") as RoleEquipLogicData;
		EquipData equipData = logicData.GetEquipDataByItemId(iID);
		if(equipData != null)
		{
			StrengthenEquipData tempEquipData = null;
			
			mDicSelEquip.TryGetValue(iID, out tempEquipData);
			if(tempEquipData != null)
			{
				// remove
				RemoveSelEquip(iID);
			}
			else
			{
				// add
				AddSelEquipByID(iID);
			}
		}
		else
		{
			Logic.EventCenter.Log(LOG_LEVEL.ERROR, "equip is null");
		}
	}
	
	public int GetNeedSyntheticCoinNum()
	{
		int iNeedSyntheticCoin = 0;
		foreach(KeyValuePair<int, StrengthenEquipData> kvp in mDicSelEquip)
		{
			EquipData equipData = kvp.Value.mEquipData;
			iNeedSyntheticCoin += TableCommon.GetNumberFromRoleEquipConfig(equipData.tid, "SUPPLY_EXP_GOLD");
		}
		return iNeedSyntheticCoin;
	}
	
	public void DeductCoin()
	{
		RoleLogicData logicData = RoleLogicData.Self;
		if(miNeedSyntheticCoin <= logicData.gold)
		{
			GameCommon.RoleChangeGold((-1)*miNeedSyntheticCoin);
		}
	}
	
	public void UpdateNeedCoinNum()
	{
		miNeedSyntheticCoin = GetNeedSyntheticCoinNum();

		RoleLogicData logicData = RoleLogicData.Self;
		UIImageButton strengthenBtn = GameCommon.FindObject(mRoleEquipStrengthenWindow,"RoleEquipStrengthenOKBtn").GetComponent<UIImageButton>();
		strengthenBtn.isEnabled = miNeedSyntheticCoin >= logicData.gold;

		// set need gold
		GameObject needGoldNum = mRoleEquipStrengthenWindow.transform.Find("NeedGold/NumLabel").gameObject;
		if(needGoldNum != null)
		{
			string strText = miNeedSyntheticCoin.ToString();
			if(miNeedSyntheticCoin > logicData.gold)
			{
				strText = "[ff0000]" + strText;
			}
			
			UILabel needGlodNumLabel = needGoldNum.GetComponent<UILabel>();
			if(needGlodNumLabel != null)
				needGlodNumLabel.text = strText;
		}
	}
	
	public void SetCurExpInfo()
	{
        // src base attribute
        GameObject srcBaseObj = GameCommon.FindObject(mRoleEquipStrengthenWindow, "src_attribute_group");
        SetBaseAttribute("src_attribute_", mCurSelEquipData.strengthenLevel);
		SetStrengthenLevel(srcBaseObj, mCurSelEquipData.strengthenLevel);
		
		int curExp = mCurSelEquipData.mExp;
		int maxExp = TableCommon.GetNumberFromRoleEquipConfig(mCurSelEquipData.tid, "STRENGTHEN_EXP_" + (mCurSelEquipData.strengthenLevel + 1).ToString());
		float percentage = curExp / (float)(maxExp > 0 ? maxExp : 1);

		int iPercentage = (int)(percentage * 100);

        UIProgressBar equipCurExpBar = mSelectEquipGroup.transform.Find("attribute_change/src_attribute_group/Progress Bar").GetComponent<UIProgressBar>();
		equipCurExpBar.value = percentage;

        UILabel equipCurExpPercentage = mSelectEquipGroup.transform.Find("attribute_change/src_attribute_group/Progress Bar/Label").GetComponent<UILabel>();
		equipCurExpPercentage.text = iPercentage.ToString() + "%";
	}

	public void SetAimExpInfo()
	{
		// aim base attribute
        GameObject aimBaseObj = GameCommon.FindObject(mRoleEquipStrengthenWindow, "aim_attribute_group");

		int iAddExp = 0;
		foreach(KeyValuePair<int, StrengthenEquipData> kvp in mDicSelEquip)
		{
			EquipData equipData = kvp.Value.mEquipData;
			iAddExp += TableCommon.GetNumberFromRoleEquipConfig(equipData.tid, "SUPPLY_EXP");
		}
		
		miAimExp = mCurSelEquipData.mExp;
		miAimStrengthenLevel = mCurSelEquipData.strengthenLevel;
		
		AddExp(iAddExp);

        SetBaseAttribute("aim_attribute_", miAimStrengthenLevel);
		SetStrengthenLevel(aimBaseObj, miAimStrengthenLevel);


		int iAimMaxExp = TableCommon.GetNumberFromRoleEquipConfig(mCurSelEquipData.tid, "STRENGTHEN_EXP_" + (miAimStrengthenLevel + 1).ToString());
		float percentage = miAimExp / (float)(iAimMaxExp > 0 ? iAimMaxExp : 1);
		
		int iPercentage = (int)(percentage * 100);

        UIProgressBar equipAimExpBar = mSelectEquipGroup.transform.Find("attribute_change/aim_attribute_group/Progress Bar").GetComponent<UIProgressBar>();
		equipAimExpBar.value = percentage;

        UILabel equipAimExpPercentage = mSelectEquipGroup.transform.Find("attribute_change/aim_attribute_group/Progress Bar/Label").GetComponent<UILabel>();
		equipAimExpPercentage.text = iPercentage.ToString() + "%";
	}
	
	public virtual void AddExp(int dExp)
	{
		if(miAimStrengthenLevel >= mCurSelEquipData.mMaxStrengthenLevel)
		{
			miAimStrengthenLevel = mCurSelEquipData.mMaxStrengthenLevel;
			miAimExp = 0;
			return;
		}
		
		miAimExp += dExp;

		int iMaxExp = TableCommon.GetNumberFromRoleEquipConfig(mCurSelEquipData.tid, "STRENGTHEN_EXP_" + (miAimStrengthenLevel + 1).ToString());
		if(miAimExp >= iMaxExp)
		{
			LevelUp();
			
			int iDExp = miAimExp - iMaxExp;
			
			miAimExp = 0;
			AddExp(iDExp);
		}
	}
	
	public virtual void LevelUp()
	{
		if(miAimStrengthenLevel >= mCurSelEquipData.mMaxStrengthenLevel)
			return;
		miAimStrengthenLevel += 1;
	}
	
	public void DeleteSelEquip()
	{
		RoleEquipLogicData logicData = DataCenter.GetData("EQUIP_DATA") as RoleEquipLogicData;
		
		foreach(KeyValuePair<int, StrengthenEquipData> iter in mDicSelEquip)
		{
			logicData.RemoveRoleEquip(iter.Value.mEquipData);
		}
		
		mDicSelEquip.Clear();
	}
	
	public void AutoAddEquip()
	{
		RoleEquipLogicData logicData = DataCenter.GetData("EQUIP_DATA") as RoleEquipLogicData;
		List<EquipData> equipList = logicData.mDicEquip.Values.ToList();
		equipList = SortAutoAddEquipList(equipList);
		
		for(int i = 0; i < equipList.Count; i++)
		{
			EquipData equip = equipList[i];
			if(equip != null)
			{
				if(equip.itemId == mCurSelEquipData.itemId
                   || TeamManager.IsEquipInTeam(equip.itemId) 
				   || equip.mStarLevel > 2 
				   || equip.strengthenLevel > 0)
					continue;
				
				bool isCan = IsEquipCanStrengthen(equip.itemId, false);
                if (isCan && mDicSelEquip.Count < 10 && !mDicSelEquip.ContainsKey(equip.itemId))
                {
                    DataCenter.SetData("ExpansionInfoWindow", "SET_SELECT_EQUIP_BY_ID", equip.itemId);

                    DataCenter.SetData("RoleEquipWindow", "SET_STRENTHEN_BTN_TOGGLE_STATE", equip.itemId);
                }
                //else
                //{
                //    DataCenter.OpenMessageWindow(STRING_INDEX.ERROR_ROLE_EQUIP_LEVEL_MAX);
                //}
			}
		}
	}
	
	public List<EquipData> SortAutoAddEquipList(List<EquipData> list)
	{
		if(list != null && list.Count > 0)
		{
//			list = list.OrderBy (p => p.mStarLevel).ToList();
			list = GameCommon.SortList (list, SortEquipDataListByStarLevel);
		}
		
		return list;
	}

	int SortEquipDataListByStarLevel(EquipData a, EquipData b)
	{
		return GameCommon.Sort (a.mStarLevel, b.mStarLevel, false);
	}

}
