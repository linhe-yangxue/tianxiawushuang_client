using UnityEngine;
using System.Collections;
using Logic ;
using DataTable ;
using System.Collections.Generic;
using System.Linq ;
using System;

public class TeamsWindow : tWindow
{
	public EquipData curEquipData;
	//int curTeamPos = 0;
	int curEquipType = 0;

	public override void Init ()
	{
		EventCenter.Register ("Button_go_equip_strengthen_button", new DefineFactory<Button_go_equip_strengthen_button>());
        EventCenter.Register("Button_go_refine_equip_button", new DefineFactory<Button_go_refine_equip_button>());
        EventCenter.Register("Button_go_refine_magic_button", new DefineFactory<Button_go_refine_magic_button>());
		EventCenter.Register ("Button_equip_change_button", new DefineFactory<Button_equip_change_button>());
        EventCenter.Register("Button_equip_unload_button", new DefineFactory<Button_EquipUnloadButton>());
	}

	public override void Open (object param)
	{
		base.Open (param);

        DataCenter.CloseWindow("TEAM_POS_INFO_WINDOW");
        DataCenter.CloseWindow("TEAM_RIGHT_ROLE_INFO_WINDOW");
        //DataCenter.CloseWindow("EQUIP_INFO_WINDOW");

        curEquipData = param as EquipData;
        //by chenliang
        //begin

        //当mGameObjUI为空时，参数类型不为EquipData，此时应该直接返回
        if (curEquipData == null)
            return;

        //end
        curEquipType = PackageManager.GetSlotPosByTid(curEquipData.tid);
		//curTeamPos = (int)TeamManager.mCurTeamPos;

        GameCommon.GetButtonData(GameCommon.FindObject(mGameObjUI, "go_equip_strengthen_button")).set("ITEM_DATA", curEquipData);

        GameObject refineEquipBtn = GameCommon.FindObject(mGameObjUI, "go_refine_equip_button");
        GameObject refineMagicBtn = GameCommon.FindObject(mGameObjUI, "go_refine_magic_button");
        ITEM_TYPE itemType = PackageManager.GetItemTypeByTableID(curEquipData.tid);
        if (refineEquipBtn != null)
        {
            refineEquipBtn.SetActive(itemType == ITEM_TYPE.EQUIP);
            GameCommon.GetButtonData(refineEquipBtn).set("ITEM_DATA", curEquipData);
        }

        if (refineMagicBtn != null)
        {
            refineMagicBtn.SetActive(itemType == ITEM_TYPE.MAGIC);
            GameCommon.GetButtonData(refineMagicBtn).set("ITEM_DATA", curEquipData);
        }

        GameCommon.GetButtonData(GameCommon.FindObject(mGameObjUI, "equip_change_button")).set("ITEM_DATA", curEquipData);
        GameCommon.GetButtonData(GameCommon.FindObject(mGameObjUI, "equip_unload_button")).set("ITEM_DATA", curEquipData);

		GameCommon.FindObject (mGameObjUI, "go_equip_strengthen_button").SetActive (true );

		mGameObjUI.transform.Find("team_equip_infos/Scroll View").GetComponent<UIScrollView>().ResetPosition ();
		Refresh(param);
	}

	public override bool Refresh (object param)
	{
		UpdaeUI();
		return true;
	}

	public override void onChange(string keyIndex, object objVal)
	{
		base.onChange(keyIndex, objVal);
		
        if ("REFRESH_WINDOW" == keyIndex)
		{
			Open((int)objVal);
		}
        else if ("HIDE_BUTTON" == keyIndex)
        {
            ShowButton(false);
        }
	}

	public void EquipSetInfos()
	{
        if(null == curEquipData)
            return;

        GameObject setGroup = GetSub("equip_set_group");
        if (null == setGroup)
            return;

        if (ITEM_TYPE.MAGIC == PackageManager.GetItemTypeByTableID(curEquipData.tid))
        {
            setGroup.SetActive(false);
            return;
        }

        setGroup.SetActive(true);
		GameObject equipSetGroupObj = GameCommon.FindObject (mGameObjUI, "equip_set_group").gameObject;
		UILabel equipSetName = equipSetGroupObj.transform.Find ("equip_set_icon_group/equip_set_name").GetComponent<UILabel>();

		int equipSetNameIndex = 0;
		int iEquipSetNum = 0;
		foreach (KeyValuePair<int, DataRecord> v in DataCenter.mSetEquipConfig.GetAllRecord ())
		{
			if(v.Value.get ("SET_EQUIP_" + curEquipType.ToString()) == curEquipData.tid)
			{
				equipSetNameIndex = v.Key;
			}
		}

		equipSetName.text = TableCommon.GetStringFromSetEquipConfig(equipSetNameIndex, "SET_NAME");
		UIGridContainer equipSetIconGrid = equipSetGroupObj.transform.Find ("equip_set_icon_group/Grid").GetComponent<UIGridContainer>();
		equipSetIconGrid.MaxCount = (int)EQUIP_TYPE.MAX;
		UIGridContainer equipSetTipsGrid = equipSetGroupObj.transform.Find ("equip_set_tips_group/Grid").GetComponent<UIGridContainer>();
		equipSetTipsGrid.MaxCount = (int)EQUIP_TYPE.MAX - 1;

		for(int i = 0; i < equipSetIconGrid.MaxCount; i++)
		{
			GameObject obj = equipSetIconGrid.controlList[i];
			UISprite sprite = obj.transform.Find ("icon_sprite").GetComponent<UISprite>();
			GameObject haveTag = obj.transform.Find ("Checkmark").gameObject;
			haveTag.SetActive (false);
			int equipItemId =  TableCommon.GetNumberFromSetEquipConfig(equipSetNameIndex, "SET_EQUIP_" + i.ToString());

			string strAtlasName = TableCommon.GetStringFromRoleEquipConfig(equipItemId, "ICON_ATLAS_NAME");
			string strSpriteName = TableCommon.GetStringFromRoleEquipConfig(equipItemId, "ICON_SPRITE_NAME");
			GameCommon.SetIcon (sprite, strAtlasName, strSpriteName);

            if (GetCurEquipTid(i) == equipItemId)
            {
                haveTag.SetActive(true);
                haveTag.transform.localPosition = Vector3.zero;
                iEquipSetNum += 1;
            }
		}

		for(int i = 0; i < equipSetTipsGrid.MaxCount; i++)
		{
			GameObject obj = equipSetTipsGrid.controlList[i];
			UILabel equip_set_introduce_label = obj.transform.Find ("equip_set_introduce_label").GetComponent<UILabel>();
			string equipBuffs = "";

			for(int j = 0; j < 3; j++)
			{
				int equipSetBuffId = TableCommon.GetNumberFromSetEquipConfig(equipSetNameIndex, "SET" + (i + 2).ToString () + "_BUFF_ID_" + (j + 1).ToString ());
				if(null ==equipSetBuffId)
					return;
				string equipSetTipsLabel = TableCommon.GetStringFromAffectBuffer(equipSetBuffId, "TIP_TITLE");

				equipBuffs = equipBuffs + equipSetTipsLabel + "     ";
			}

			string strEquipTips = "[" + (i + 2).ToString () + "件套装]  " + equipBuffs;
			if(i < iEquipSetNum -1)
			{
				strEquipTips = "[ff0000]" + strEquipTips;
			}
//			else
//			{
//				strEquipTips = strEquipTips;
//			}

			GameCommon.SetUIText (obj, "equip_set_introduce_label", strEquipTips);
		}
	}

    public int GetCurEquipTid(int i)
    {
        int curEquipTid = curEquipData.tid;
        if (GameCommon.GetDataByZoneUid("IS_FROM_TEAM_INFO") == "1")
        {
            EquipData curWearEquipData;
            curWearEquipData = TeamManager.GetRoleEquipDataByTeamPos((int)TeamManager.mCurTeamPos, i);
            if (curWearEquipData != null)
            {
                curEquipTid = curWearEquipData.tid;
            }
        }
        return curEquipTid;
    }

	public void UpdaeUI()
	{
        ShowButton(true);
        SetName();
        UpdateBaseAttribute();
        UpdateRefentAttribute();
		EquipSetInfos();
        UpdateTips();
		UpdateBtnStateUI ();
	}
    //刷新按钮状态UI
	private void UpdateBtnStateUI()
	{
		// 如果是经验法器则强化和精炼按钮变灰
		UIImageButton _strengthBtn = GameCommon.FindObject (mGameObjUI,"go_equip_strengthen_button").GetComponent<UIImageButton>();
		UIImageButton _refineBtn = GameCommon.FindObject (mGameObjUI,"go_refine_magic_button").GetComponent<UIImageButton>();
		_refineBtn.isEnabled = !GameCommon.CheckIsExpMagicEquip(curEquipData.tid);
		_strengthBtn.isEnabled = !GameCommon.CheckIsExpMagicEquip(curEquipData.tid);
	}


    /// <summary>
    /// 刷新基础属性信息
    /// </summary>
    public void UpdateBaseAttribute()
    {
		//added by xuke
		GameObject _baseAttrGroup = GameCommon.FindObject (mGameObjUI,"base_attribute_group");
		UIGridContainer _gridContainer = GameCommon.FindObject (_baseAttrGroup,"Grid").GetComponent<UIGridContainer>();
		// 设置强化等级
		GameObject _strengthRoot = GameCommon.FindObject(_baseAttrGroup, "strengthen_level_num");
        GameCommon.SetUIVisiable(_strengthRoot, "get_max", curEquipData.strengthenLevel >= curEquipData.mMaxStrengthenLevel);
		GameCommon.SetUIVisiable(_strengthRoot, "num_label", !(curEquipData.strengthenLevel >= curEquipData.mMaxStrengthenLevel));
		UILabel _curLvLbl = GameCommon.FindComponent<UILabel> (_strengthRoot,"num_label");
        _curLvLbl.text = curEquipData.strengthenLevel.ToString();
        UILabel _limitLvLbl = GameCommon.FindComponent<UILabel>(_strengthRoot, "num_label01");

		DataRecord _record = DataCenter.mRoleEquipConfig.GetRecord (curEquipData.tid);
        //int maxStreLevel = RoleLogicData.Self.chaLevel * 2;
        int maxStreLevel = curEquipData.mMaxStrengthenLevel;
		if (maxStreLevel > _record["MAX_GROW_LEVEL"])
        {
			maxStreLevel = _record["MAX_GROW_LEVEL"];
        }
		_limitLvLbl.text = maxStreLevel.ToString();

		// 如果是法器
		if (ITEM_TYPE.MAGIC == PackageManager.GetItemTypeByTableID (curEquipData.tid)) {
			_gridContainer.MaxCount = 2;
			GameCommon.SetBaseAttrInfo(_gridContainer.controlList[0],_record["ATTRIBUTE_TYPE_0"],_record["BASE_ATTRIBUTE_0"] + _record["STRENGTHEN_0"] * Mathf.Max(0,(curEquipData.strengthenLevel - 1)));
            GameCommon.SetBaseAttrInfo(_gridContainer.controlList[1], _record["ATTRIBUTE_TYPE_1"], _record["BASE_ATTRIBUTE_1"] + _record["STRENGTHEN_1"] * Mathf.Max(0, (curEquipData.strengthenLevel - 1)));
		} 
		// 如果是装备
		else {
			_gridContainer.MaxCount = 1;
            //DEBUG.Log(_record["STRENGTHEN_0"]);
            //DEBUG.Log(Mathf.Max(0, (curEquipData.strengthenLevel - 1)));
            GameCommon.SetBaseAttrInfo(_gridContainer.controlList[0], _record["ATTRIBUTE_TYPE_0"], _record["BASE_ATTRIBUTE_0"] + _record["STRENGTHEN_0"] * Mathf.Max(0, (curEquipData.strengthenLevel - 1)));
           
		}

		// 得到基础属性group的背景图片
		UISprite _baseAttrSprite = GameCommon.FindComponent<UISprite> (_baseAttrGroup,"base_attribute_bg");
		_baseAttrSprite.height = (int)(_gridContainer.CellHeight * (_gridContainer.MaxCount + 1) + 25);
		//end
    }
	
	// 设置基础属性信息
	private void SetBaseAttrInfo(GameObject kParent,int kAttrType,int kAttrValue)
	{
		GameCommon.SetAttributeIcon (kParent,kAttrType,"icon");
		GameCommon.SetAttributeName (kParent,kAttrType,"name");
		UILabel _attrValue = GameCommon.FindComponent<UILabel> (kParent,"num_label");
		float _realAffectValue = kAttrValue / 10000f;
		if (GameCommon.IsAffectTypeRate ((AFFECT_TYPE)kAttrType)) 
		{
			_realAffectValue *= 100f;
			_attrValue.text = _realAffectValue.ToString("f2") + "%";
		}
		else
			_attrValue.text = _realAffectValue.ToString ("f0");
	}

    /// <summary>
    /// 刷新精炼属性信息
    /// </summary>
    public void UpdateRefentAttribute()
    {
        GameObject refineGroup = GetSub("refine_attribute_group");
        GameObject levelInfo = GameCommon.FindObject(refineGroup, "refine_level_num");

        GameCommon.SetUIText(levelInfo, "num_label", curEquipData.refineLevel.ToString());
        GameCommon.SetUIText(levelInfo, "num_label01", curEquipData.mMaxRefineLevel.ToString());
        GameCommon.SetUIVisiable(levelInfo, "get_max", curEquipData.refineLevel >= curEquipData.mMaxRefineLevel);
        GameCommon.SetUIVisiable(levelInfo, "num_label", !(curEquipData.refineLevel >= curEquipData.mMaxRefineLevel));
		   
        DataRecord r = DataCenter.mRoleEquipConfig.GetRecord(curEquipData.tid);
        UIGridContainer container = GameCommon.FindComponent<UIGridContainer>(refineGroup, "Grid");
        GameCommon.SetRefineAttribute(container.controlList[0], r["REFINE_TYPE_0"], r["REFINE_VALUE_0"] * curEquipData.refineLevel);
        GameCommon.SetRefineAttribute(container.controlList[1], r["REFINE_TYPE_1"], r["REFINE_VALUE_1"] * curEquipData.refineLevel);


		GameObject _baseAttrGroup = GameCommon.FindObject (mGameObjUI,"base_attribute_group");
		UIGridContainer _gridContainer = GameCommon.FindObject (_baseAttrGroup,"Grid").GetComponent<UIGridContainer>();

        //MAX_REFINE_LEVEL
        GameCommon.SetUIText(levelInfo, "num_label01", r["MAX_REFINE_LEVEL"]);
		// 设置精炼group的位置
		if (ITEM_TYPE.MAGIC == PackageManager.GetItemTypeByTableID (curEquipData.tid)) {
			refineGroup.transform.localPosition = new Vector3 (refineGroup.transform.localPosition.x, -90f - _gridContainer.CellHeight, refineGroup.transform.localPosition.z);
		} else {
			refineGroup.transform.localPosition = new Vector3 (refineGroup.transform.localPosition.x, -90f, refineGroup.transform.localPosition.z);
		}
    }



    /// <summary>
    /// 设置名称
    /// </summary>
    public void SetName()
    {
        string _colorPrefix = GameCommon.GetEquipTypeColor(TableCommon.GetNumberFromRoleEquipConfig(curEquipData.tid, "QUALITY"));
        GameObject equipNameObj = GameCommon.FindObject(mGameObjUI, "EquipNameLabel");
        UILabel equipNameLabel = equipNameObj.GetComponent<UILabel>();
        equipNameLabel.text = _colorPrefix + TableCommon.GetStringFromRoleEquipConfig(curEquipData.tid, "NAME") + "[-]";

        //added by xuke
        //设置类型
        UILabel _equipTypeDescLbl = GameCommon.FindComponent<UILabel>(mGameObjUI, "equip_type");
        _equipTypeDescLbl.text = _colorPrefix + GameCommon.GetEquipTypeDesc(TableCommon.GetNumberFromRoleEquipConfig(curEquipData.tid, "EQUIP_TYPE")) + "[-]";

        //by chenliang
        //begin

//        GameCommon.FindComponent<UILabel>(GameCommon.FindObject(mGameObjUI, "Scroll View"), "point_label").text = _colorPrefix + "•" + "[-]";
//--------------------
        //将"•"替换为"."，同时调整对应prefab
        GameCommon.FindComponent<UILabel>(GameCommon.FindObject(mGameObjUI, "Scroll View"), "point_label").text = _colorPrefix + "." + "[-]";

        //end
        //end
    }

    /// <summary>
    /// 刷新详情
    /// </summary>
    public void UpdateTips()
    {
        GameObject refineGroup = GetSub("refine_attribute_group");
        GameObject setGroup = GetSub("equip_set_group");
        GameObject tipsGroup = GetSub("equip_tips_group");
        if (null == refineGroup || null == setGroup || null == tipsGroup)
            return;

        if (setGroup.activeSelf)
        {
            UISprite setIconGroupBg = GameCommon.FindComponent<UISprite>(setGroup, "equip_set_icon_group");
            UISprite setTipsGroupBg = GameCommon.FindComponent<UISprite>(setGroup, "equip_set_tips_group");
            if (null == setIconGroupBg || null == setTipsGroupBg)
                return;

            tipsGroup.transform.localPosition = new Vector3(tipsGroup.transform.localPosition.x,
                setGroup.transform.localPosition.y + setIconGroupBg.drawingDimensions.y - 12.0f
                , tipsGroup.transform.localPosition.z);
        }
        else
        {
			GameObject _baseAttrGroup = GameCommon.FindObject (mGameObjUI,"base_attribute_group");
			UIGridContainer _gridContainer = GameCommon.FindObject (_baseAttrGroup,"Grid").GetComponent<UIGridContainer>();

            tipsGroup.transform.localPosition = new Vector3(tipsGroup.transform.localPosition.x,
                setGroup.transform.localPosition.y - _gridContainer.CellHeight, tipsGroup.transform.localPosition.z);
        }

        GameObject tipsObj = GameCommon.FindObject(mGameObjUI, "equip_tips_label");
        UILabel tipsObjLabel = tipsObj.GetComponent<UILabel>();
        tipsObjLabel.text = TableCommon.GetStringFromRoleEquipConfig(curEquipData.tid, "DESCRIPTION");
    }

    public void ShowButton(bool visible)
    {
        SetVisible("equip_unload_button", visible);
        SetVisible("equip_change_button", visible);
        Transform trans = mGameObjUI.transform.Find("team_equip_infos/Scroll View");
        UIPanel panel = trans.GetComponent<UIPanel>();
        Vector3 pt1 = GetSub("pt1").transform.localPosition;
        Vector3 pt2 = GetSub("pt2").transform.localPosition;
        Vector3 pt3 = GetSub("pt3").transform.localPosition;

        Vector4 baseClip = panel.baseClipRegion;

        if (visible)
        {
            baseClip.y = (pt1.y + pt2.y) / 2f;
            baseClip.w = pt1.y - pt2.y;
        }
        else 
        {
            baseClip.y = (pt1.y + pt3.y) / 2f;
            baseClip.w = pt1.y - pt3.y;
        }

        panel.baseClipRegion = baseClip;

        //added by xuke 红点相关
        GameObject _equipChangeBtnObj = GameCommon.FindObject(mGameObjUI, "equip_change_button");
        EquipNewMarkData _equipNewMarkData = TeamNewMarkManager.Self.GetEquipNewMarkData(curEquipData.teamPos, PackageManager.GetSlotPosByTid(curEquipData.tid), PackageManager.GetItemTypeByTableID(curEquipData.tid));
        GameCommon.SetNewMarkVisible(_equipChangeBtnObj, _equipNewMarkData == null ? false : _equipNewMarkData.ChangeVisible);
        //end
    }
}

//切换到强化界面
class Button_go_equip_strengthen_button : CEvent
{
	public override bool _DoEvent ()
	{
        EquipData itemData = getObject("ITEM_DATA") as EquipData;

        if (itemData != null)
        {
            ITEM_TYPE itemType = PackageManager.GetItemTypeByTableID(itemData.tid);
            if (itemType == ITEM_TYPE.EQUIP)
            {
                DataCenter.OpenWindow("EQUIP_INFO_WINDOW", itemData);
                DataCenter.SetData("EQUIP_INFO_WINDOW", "STRENGTHEN", itemData);
            }
            else if (itemType == ITEM_TYPE.MAGIC)
            {
                DataCenter.OpenWindow("MAGIC_INFO_WINDOW", itemData);
                DataCenter.SetData("MAGIC_INFO_WINDOW", "STRENGTHEN", itemData);
            }
            DataCenter.CloseWindow("TEAM_DATA_WINDOW");
        }
		return true;
	}
}

//切换到装备精炼界面
class Button_go_refine_equip_button : CEvent
{
    public override bool _DoEvent()
    {
        EquipData itemData = getObject("ITEM_DATA") as EquipData;
        if (itemData != null)
        {
            ITEM_TYPE itemType = PackageManager.GetItemTypeByTableID(itemData.tid);

            if (itemType == ITEM_TYPE.EQUIP)
            {
                DataCenter.OpenWindow("EQUIP_INFO_WINDOW", itemData);
                DataCenter.SetData("EQUIP_INFO_WINDOW", "EQUIP_REFINE", itemData);
                DataCenter.CloseWindow("TEAM_DATA_WINDOW");  
            }
        }

        return true;
    }
}

//切换到法器精炼界面
class Button_go_refine_magic_button : CEvent
{
    public override bool _DoEvent()
    {
        EquipData itemData = getObject("ITEM_DATA") as EquipData;
        if (itemData != null)
        {
            ITEM_TYPE itemType = PackageManager.GetItemTypeByTableID(itemData.tid);

            if (itemType == ITEM_TYPE.MAGIC)
            {
                DataCenter.OpenWindow("MAGIC_INFO_WINDOW", itemData);
                DataCenter.SetData("MAGIC_INFO_WINDOW", "MAGIC_REFINE", itemData);
                DataCenter.CloseWindow("TEAM_DATA_WINDOW");
            }
        }

        return true;
    }
}


class Button_equip_change_button : CEvent
{
    public override bool _DoEvent()
    {
        EquipData itemData = getObject("ITEM_DATA") as EquipData;

        if (itemData != null)
        {
            ITEM_TYPE itemType = PackageManager.GetItemTypeByTableID(itemData.tid);
            OpenBagObject<EquipData> openObj = new OpenBagObject<EquipData>();
            if (itemType == ITEM_TYPE.EQUIP)
            {
                openObj.mBagShowType = BAG_SHOW_TYPE.USE;
                openObj.mFilterCondition = (tempItemData) =>
                {
                    return !tempItemData.IsInTeam() && tempItemData.itemId != itemData.itemId
                        && PackageManager.GetSlotPosByTid(tempItemData.tid) == PackageManager.GetSlotPosByTid(itemData.tid);
                };

                openObj.mSelectAction = (upItemData) =>
                {
                    RoleEquipLogicData logic = DataCenter.GetData("EQUIP_DATA") as RoleEquipLogicData;
                    if (logic != null)
                    {
                        EquipData downEquipData = TeamManager.GetRoleEquipDataByCurTeamPos(PackageManager.GetSlotPosByTid(upItemData.tid));

                        //TeamManager.OnChangeTeamPos.Append(TeamInfoWindow.UpdateTeamPosInfo, true, false);

                        TeamManager.RequestChangeTeamPos((int)TeamManager.mCurTeamPos, upItemData.itemId, upItemData.tid,
                            downEquipData != null ? downEquipData.itemId : -1, downEquipData != null ? downEquipData.tid : -1);
                    }
                };

                DataCenter.SetData("BAG_EQUIP_WINDOW", "OPEN", openObj);
            }
            else if (itemType == ITEM_TYPE.MAGIC)
            {
                openObj.mBagShowType = BAG_SHOW_TYPE.USE;
                openObj.mFilterCondition = (tempItemData) =>
                {
                    return !tempItemData.IsInTeam() && tempItemData.itemId != itemData.itemId
                        && PackageManager.GetSlotPosByTid(tempItemData.tid) == PackageManager.GetSlotPosByTid(itemData.tid);
                };

                openObj.mSelectAction = (upItemData) =>
                {
                    RoleEquipLogicData logic = DataCenter.GetData("EQUIP_DATA") as RoleEquipLogicData;
                    if (logic != null)
                    {
                        EquipData downEquipData = TeamManager.GetMagicDataByCurTeamPos(PackageManager.GetSlotPosByTid(upItemData.tid));

                        //TeamManager.OnChangeTeamPos.Append(TeamInfoWindow.UpdateTeamPosInfo, true, false);

                        TeamManager.RequestChangeTeamPos((int)TeamManager.mCurTeamPos, upItemData.itemId, upItemData.tid,
                            downEquipData != null ? downEquipData.itemId : -1, downEquipData != null ? downEquipData.tid : -1);
                    }
                };

                DataCenter.SetData("BAG_MAGIC_WINDOW", "OPEN", openObj);
            }
        }
        return true;
    }
}

class Button_EquipUnloadButton : CEvent
{
    public override bool _DoEvent()
    {
        EquipData itemData = getObject("ITEM_DATA") as EquipData;

        if (itemData != null)
        {
            ITEM_TYPE itemType = PackageManager.GetItemTypeByTableID(itemData.tid);
            if (itemType == ITEM_TYPE.EQUIP)
            {
                // send message to server
                int iUpItemId = -1;
                int iUpTid = -1;

                EquipData downEquipData = TeamManager.GetRoleEquipDataByCurTeamPos(PackageManager.GetSlotPosByTid(itemData.tid));
                TeamManager.RequestChangeTeamPos((int)TeamManager.mCurTeamPos, iUpItemId, iUpTid,
                    downEquipData != null ? downEquipData.itemId : -1, downEquipData != null ? downEquipData.tid : -1);
            }
            else if (itemType == ITEM_TYPE.MAGIC)
            {
                // send message to server
                int iUpItemId = -1;
                int iUpTid = -1;

                EquipData downEquipData = TeamManager.GetMagicDataByCurTeamPos(PackageManager.GetSlotPosByTid(itemData.tid));
                TeamManager.RequestChangeTeamPos((int)TeamManager.mCurTeamPos, iUpItemId, iUpTid,
                    downEquipData != null ? downEquipData.itemId : -1, downEquipData != null ? downEquipData.tid : -1);
            }
            DataCenter.SetData("TEAM_POS_INFO_WINDOW", "UPFATE_BASE_ATTRIBUTE_INFO", true);
        }

        return true;
    }
}






